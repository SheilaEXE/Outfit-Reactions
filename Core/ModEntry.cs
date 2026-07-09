using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OutfitReactions.Ai;

namespace OutfitReactions
{
    public sealed partial class ModEntry : Mod
    {
        internal static ModEntry Instance { get; private set; }

        private readonly Random random = new();
        private Harmony harmony;

        internal ModConfig Config { get; set; } = new();

        // True when the user turns on debug logging in the config menu. Operational/diagnostic
        // logs are only emitted when this is on, so by default they don't appear in the SMAPI log
        // at all (not even at Debug level), keeping the log clean.
        internal static bool DebugLog => Instance?.Config?.EnableDebugLogging ?? false;

        private IFashionSenseApi fsApi;
        private OtherNpcClothesReactionSystem otherNpcClothesReactionSystem;
        private OutfitAiService outfitAiService;
        private OutfitMemoryService outfitMemoryService;
        private HatMemoryService hatMemoryService;

        // Periodic vanilla-hat tracking: the vanilla hat (Game1.player.hat) can change WITHOUT the
        // Fashion Sense menu ever opening (e.g. equipping from inventory), so the menu-close path
        // never sees it. We poll for it on a light interval instead.
        private string lastKnownVanillaHatId;
        private List<string> lastKnownVanillaHatSpecialItemCandidates = new();
        private string lastKnownVanillaPantsName;
        private List<string> lastKnownVanillaPantsSpecialItemCandidates = new();
        // Dedupe set for [SPECIAL ITEM DEBUG] logs — cleared whenever a new change is detected.
        private readonly HashSet<string> loggedSpecialItemDebugKeys = new(StringComparer.Ordinal);
        private bool vanillaHatTrackingInitialized;
        private int vanillaHatPollTimer;
        private OutfitVisionService outfitVisionService;
        private FashionSenseVisualService fashionSenseVisualService;
        private SpecialHatReactionService specialHatReactionService;
        private SpecialItemReactionService specialItemReactionService;

        private bool changedClothes = false;
        // Tracks the saved outfit id we last made "eligible for notices" via the notice refresh, so
        // the refresh doesn't keep calling NotifyOutfitChanged() for the SAME unchanged outfit. That
        // call clears reactedNpcsThisOutfit, which would let NPCs re-notice the same look every tick
        // (e.g. peeking NPCs noticing again right after, without the player changing anything).
        private string lastEligibleSavedOutfitId = "";
        private bool isReactingToClothes = false;

        // modData key written on the Farmer so OTHER mods (e.g. the kiss mod) can tell when an
        // Outfit Reactions reaction is in progress and hold off. Value is "1" while active, removed
        // otherwise. Kept in modData (not a cross-mod API) so it works regardless of mod load order.
        internal const string ReactionActiveModDataKey = "NatrollEXE.OutfitReactions/ReactionActive";

        /// <summary>
        /// True whenever ANY outfit reaction is in progress and not yet fully read/closed — the spouse
        /// reaction sequence, the immediate "reacting to clothes" state, an unread saved-outfit notice,
        /// or any non-spouse NPC with a pending reaction. Used to block kiss interactions during the
        /// whole reaction (noticing + generating + dialogue open) until the dialogue ends.
        /// </summary>
        internal bool IsAnyOutfitReactionActive()
        {
            // NOTE: changedClothes is intentionally NOT checked here. It only means "the outfit
            // changed recently", not "a reaction is happening right now" — and the spouse flow ends
            // via ResetClothesReactionState(), which does not clear changedClothes, so including it
            // here left the cross-mod flag (and thus kisses) stuck after every spouse reaction.
            if (isReactingToClothes || outfitSequenceActive)
                return true;
            if (clothesComplimentReady || clothesPathStarted)
                return true;
            if (otherNpcClothesReactionSystem?.HasAnyActivePendingReaction() == true)
                return true;
            return false;
        }

        private void UpdateReactionActiveModDataFlag()
        {
            if (Game1.player == null)
                return;

            bool active = IsAnyOutfitReactionActive();
            bool hasFlag = Game1.player.modData.ContainsKey(ReactionActiveModDataKey);

            if (active && !hasFlag)
                Game1.player.modData[ReactionActiveModDataKey] = "1";
            else if (!active && hasFlag)
                Game1.player.modData.Remove(ReactionActiveModDataKey);
        }
        private int clothesInteractionCooldown = 0;
        private bool clothesPathStarted = false;
        private bool clothesComplimentReady = false;
        private Point clothesPreferredOffset = Point.Zero;
        private Point clothesLastPlayerTile = Point.Zero;
        private Point clothesLastTargetTile = Point.Zero;
        private bool clothesFirstNoticeDone = false;
        private bool clothesEmoteFired = false; // true after the one-shot ellipsis emote has played
        private int clothesNoticePauseTimer = 0;
        private bool playerWasInClothesNoticeRange = false;
        private int clothesSecondNoticeCooldown = 0;
        private int clothesChaseTimer = 0;
        private NPC clothesReactingNpc = null;
        private bool outfitSequenceActive = false;
        private FashionSenseSnapshot fsSnapshotBefore = null;
        private bool fashionSenseMenuOpen = false;
        private FashionSenseChangeInfo lastFashionSenseChangeInfo = null;
        // NPCs that already reacted to the CURRENT immediate change. Lets each nearby NPC react
        // once without wiping the change for everyone else (fixes the 2nd NPC losing the dialogue).
        private readonly HashSet<string> npcsReactedToCurrentNotice = new(StringComparer.OrdinalIgnoreCase);

        private List<Dialogue> spouseDialogueBackupBeforeOutfit = null;
        private string spouseDialogueBackupNpcName = "";
        private bool spouseFriendshipStateCaptured = false;
        private bool spouseOriginalTalkedToToday = false;
        private bool spouseForcedTalkedToToday = false;

        // Destination captured the moment the outfit sequence first interrupts the NPC.
        // We save only the final tile of the route (last point in the pathToEndPoint stack)
        // plus the end behavior, then recompute the path from the NPC's actual position
        // when the dialogue ends. This way the NPC walks straight to where it was going
        // from wherever it currently is — no detours, no replaying old steps.
        private Point? spouseFinalDestinationBackup = null;
        private PathFindController.endBehavior spouseEndBehaviorBackup = null;
        private int spouseFinalFacingBackup = -1;
        private SchedulePathDescription spouseDirectionsBackup = null;

        // After the spouse outfit compliment, keep them paused near the farmer for a
        // short affectionate beat, matching the kiss mod behavior: they resume when
        // the farmer walks far enough away or after a short timeout. This uses
        // movementPause only and does NOT delete the NPC controller.
        private bool spousePostOutfitLingerActive = false;
        private NPC spousePostOutfitLingerNpc = null;
        private int spousePostOutfitLingerTimer = 0;
        private const int SpousePostOutfitLingerDelayTicks = 360; // ~6 seconds
        private const float SpousePostOutfitLingerDistance = 600f;
        // During a pending outfit notice, do not steal the spouse/NPC controller.
        // They only pause if they naturally get very close to the farmer, then resume
        // once the farmer walks far enough away. This mirrors the kiss mod's safe
        // movementPause-only behavior.
        private const float SpouseOutfitNoticePauseDistance = 96f;   // roughly one tile / adjacent interaction distance
        private const float SpouseOutfitNoticeReleaseDistance = 300f;
        private bool spouseOutfitNoticePauseActive = false;
        private int spousePendingOutfitBubbleTimer = 0;

        private sealed class NpcOutfitSpecialActionSnapshot
        {
            public NPC Npc { get; set; }
            public GameLocation Location { get; set; }
            public int FacingDirection { get; set; }
            public int CurrentFrame { get; set; }
            public bool Flip { get; set; }
            public int MovementPause { get; set; }
            public int AddedSpeed { get; set; }
            public List<FarmerSprite.AnimationFrame> CurrentAnimation { get; set; }
        }

        private NpcOutfitSpecialActionSnapshot spouseOutfitSpecialActionSnapshot = null;
        private const float OutfitSpecialActionRestoreDistance = 300f;

        private sealed class OwnAiPendingGeneration
        {
            public string NpcName { get; set; } = "";
            public bool IsSpouseDialogue { get; set; }
            public bool ClearExistingDialogue { get; set; }
            public Task<string> Task { get; set; }
            public bool CompletionHandled { get; set; }
            public int WaitingDotCount { get; set; } = 1;
            public int WaitingDotTimer { get; set; } = 30;
            public int SafetyTimer { get; set; } = 7200;
        }

        private sealed class OwnAiPendingPlayerReplyGeneration
        {
            public string NpcName { get; set; } = "";
            public bool IsSpouseDialogue { get; set; }
            public string NpcCompliment { get; set; } = "";
            public string PlayerReply { get; set; } = "";
            public Task<string> Task { get; set; }
            public bool CompletionHandled { get; set; }
            public int WaitingDotCount { get; set; } = 1;
            public int WaitingDotTimer { get; set; } = 30;
            public int SafetyTimer { get; set; } = 7200;
            public Action OnFinished { get; set; }
        }

        private readonly Dictionary<string, OwnAiPendingGeneration> pendingOwnAiGenerations = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, OwnAiPendingPlayerReplyGeneration> pendingOwnAiPlayerReplyGenerations = new(StringComparer.OrdinalIgnoreCase);

        // Full back-and-forth history for the CURRENT outfit-reaction conversation with each NPC.
        // Each entry is (speaker, text), oldest first. Cleared whenever the conversation truly ends
        // (farmer chooses "Leave"/cancels, or the dialogue closes for any other reason) so the next
        // conversation starts fresh. Kept separate from the per-round "pending" objects, which are
        // recreated every time a new AI request starts.
        private readonly Dictionary<string, List<(string Speaker, string Text)>> activeOutfitReplyConversations = new(StringComparer.OrdinalIgnoreCase);

        private void ResetOutfitReplyConversation(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return;
            activeOutfitReplyConversations.Remove(npcName);
        }

        private void StartOutfitReplyConversation(string npcName, string npcOpeningLine)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return;

            List<(string Speaker, string Text)> history = new();
            if (!string.IsNullOrWhiteSpace(npcOpeningLine))
                history.Add(("NPC", npcOpeningLine));
            activeOutfitReplyConversations[npcName] = history;
        }

        private void AppendToOutfitReplyConversation(string npcName, string speaker, string text)
        {
            if (string.IsNullOrWhiteSpace(npcName) || string.IsNullOrWhiteSpace(text))
                return;

            if (!activeOutfitReplyConversations.TryGetValue(npcName, out List<(string Speaker, string Text)> history))
            {
                history = new();
                activeOutfitReplyConversations[npcName] = history;
            }
            history.Add((speaker, text));
        }

        private string BuildOutfitReplyConversationTranscript(string npcName, int maxChars = 2500)
        {
            if (string.IsNullOrWhiteSpace(npcName) || !activeOutfitReplyConversations.TryGetValue(npcName, out List<(string Speaker, string Text)> history) || history.Count == 0)
                return "";

            StringBuilder sb = new();
            foreach ((string speaker, string text) in history)
            {
                string label = speaker == "NPC" ? "NPC" : "Farmer";
                sb.Append(label).Append(": ").Append(text.Trim()).Append('\n');
            }

            string transcript = sb.ToString().Trim();
            if (transcript.Length > maxChars)
            {
                // Keep the most recent part of the conversation; trimming from the start preserves
                // what just happened, which matters most for staying on-topic.
                transcript = "...(earlier conversation trimmed)...\n" + transcript.Substring(transcript.Length - maxChars);
            }
            return transcript;
        }

        private const string AssetPrefix = "Mods/NatrollEXE.OutfitReactions/Clothes";

        // AI-only build:
        // Manual outfit dialogue JSON from Mods/NatrollEXE.OutfitReactions/Clothes is intentionally disabled.
        // The old loader/fallback helpers are kept in the source for future use, but every runtime path below
        // returns before reading/queueing those manual lines.
        // Manual JSON dialogue path is disabled in this AI-only build. This is a property (not a
        // const) on purpose: as a const, the compiler folds it to false and flags every method
        // below it as "unreachable code" (CS0162). As a property the value is evaluated at runtime,
        private const string OutfitNoticeModDataPrefix = "NatrollEXE.OutfitReactions.OutfitNotice.";
        private const string PlayerAccessoryDescriptionModDataPrefix = "NatrollEXE.OutfitReactions.PlayerAccessoryDescription.";
        internal void QueueAiConnectionTestFromConfigMenu()
        {
            outfitAiService?.QueueConnectionTestFromConfigMenu();
        }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = helper.ReadConfig<ModConfig>();
            Config.MigrateLegacyAiSettings();
            outfitAiService = new OutfitAiService(helper, Monitor, () => Config);
            outfitAiService.IsRomanceableNpc = IsNpcRomanceable;
            outfitMemoryService = new OutfitMemoryService(helper, Monitor);
            hatMemoryService = new HatMemoryService(helper, Monitor);
            outfitVisionService = new OutfitVisionService(Monitor);
            fashionSenseVisualService = new FashionSenseVisualService(Monitor, () => fsApi);
            specialHatReactionService = new SpecialHatReactionService(helper, Monitor);
            specialItemReactionService = new SpecialItemReactionService(helper, Monitor);

            otherNpcClothesReactionSystem = new OtherNpcClothesReactionSystem(
                Monitor,
                () => Config,
                TryQueueOtherNpcOutfitDialogue,
                RefreshOtherNpcOutfitPrompt,
                ClearOutfitPrompt,
                HasNoticeableCurrentFashionSenseAppearance,
                CanNpcNoticeCurrentOutfitNotice,
                MarkCurrentOutfitAsNoticed,
                CanNpcReactToCurrentOutfitNotice,
                HasNpcSeenCurrentVisualBefore
            );

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            helper.Events.GameLoop.Saving += OnSaving;

            helper.ConsoleCommands.Add("oc_debug_notice", "Outfit Compliments: print why the current outfit notice can/can't start for nearby NPCs.", DebugOutfitNoticeCommand);
            helper.ConsoleCommands.Add("oc_clear_notice_memory", "Outfit Compliments: clear this save's outfit notice memory and reset pending notice state.", ClearOutfitNoticeMemoryCommand);
            helper.ConsoleCommands.Add("oc_test_voicesamples", "Outfit Reactions: report how many real in-game voice-sample lines each NPC profile has (run after loading a save).", VoiceSampleReportCommand);
            helper.ConsoleCommands.Add("oc_preview_voicesamples", "Outfit Reactions: show the exact voice-sample lines that would be injected into the prompt for ONE NPC. Usage: oc_preview_voicesamples <NpcName>", VoiceSamplePreviewCommand);

            ApplyHarmonyPatches();
        }

        private void ApplyHarmonyPatches()
        {
            try
            {
                MethodBase target = AccessTools.Method(
                    typeof(NPC),
                    nameof(NPC.checkAction),
                    new[] { typeof(Farmer), typeof(GameLocation) }
                );

                if (target == null)
                {
                    Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction target method was NOT found. Patch was not applied.", LogLevel.Warn);
                    return;
                }

                harmony = new Harmony(ModManifest.UniqueID);
                harmony.PatchAll(typeof(ModEntry).Assembly);

                if (DebugLog) Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction Harmony patch applied.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log("[CLOTHES PRIORITY] Failed to apply NPC.checkAction patch: " + ex, LogLevel.Warn);
            }
        }
        [HarmonyPatch]
        private static class NPCCheckActionPatch
        {
            private static bool firstRunLogged = false;

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(NPC),
                    nameof(NPC.checkAction),
                    new[] { typeof(Farmer), typeof(GameLocation) }
                );
            }

            [HarmonyPriority(Priority.First)]
            private static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
            {
                try
                {
                    if (!firstRunLogged)
                    {
                        firstRunLogged = true;
                        if (DebugLog) Instance?.Monitor?.Log("[CLOTHES PRIORITY] NPC.checkAction prefix ran for the first time.", LogLevel.Info);
                    }

                    if (Instance?.TryHandleOutfitDialogueOrBlockNpcInteraction(__instance) == true)
                    {
                        __result = true;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Instance?.Monitor?.Log("[CLOTHES PRIORITY] Error while prioritizing/blocking outfit dialogue before NPC.checkAction: " + ex, LogLevel.Warn);
                }

                return true;
            }
        }
        internal bool PrioritizeOutfitDialogueBeforeNpcCheckAction(NPC npc)
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
                return false;

            if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                return false;

            if (Game1.eventUp)
                return false;

            if (TryPrioritizeSpouseOutfitDialogueForClick(npc))
                return true;

            return otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npc) == true;
        }

        internal bool TryHandleOutfitDialogueOrBlockNpcInteraction(NPC npc)
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
                return false;

            if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                return false;

            if (Game1.eventUp)
                return false;

            // First try the normal priority path: if the outfit line is ready, or the
            // built-in AI needs to start/continue generating, consume the click and
            // let Outfit Compliments own this interaction.
            if (TryOpenPrioritizedOutfitDialogueFromCheckAction(npc))
                return true;

            // If the NPC has already noticed the outfit but the outfit dialogue is not
            // readable yet, still swallow the click. This blocks vanilla kisses, spouse
            // kisses from other checkAction-based mods, and normal dialogue until the
            // outfit compliment is actually read or the notice is cancelled by distance.
            if (ShouldBlockNpcInteractionUntilOutfitDialogueRead(npc))
            {
                ShowPendingOutfitBlockedInteractionFeedback(npc);
                if (DebugLog) Monitor.Log($"[CLOTHES PRIORITY] Blocked normal interaction/kiss with {npc.Name} because an unread outfit dialogue is pending.", LogLevel.Info);
                return true;
            }

            return false;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            fsApi = Helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");
            Monitor.Log(fsApi != null
                ? "Fashion Sense API loaded successfully."
                : "Fashion Sense API not found. Outfit compliments will not detect clothing changes.",
                fsApi != null ? LogLevel.Debug : LogLevel.Warn);

            outfitAiService?.LoadProfiles();

            try
            {
                var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                ModConfigMenu.Register(this, gmcm);
            }
            catch (Exception ex)
            {
                Monitor.Log("Failed to register GMCM options: " + ex.Message, LogLevel.Trace);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            outfitMemoryService?.Save();
            hatMemoryService?.Save();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            outfitMemoryService?.Load();
            hatMemoryService?.Load();
            // Repopulate the installed-mod-IDs cache now that all mods are fully loaded,
            // so ConditionalMatchNames in item.json are evaluated against the real mod list.
            specialItemReactionService?.ResetModRegistryCache();
            // Reset vanilla-hat tracking so the first poll on this save just sets the baseline
            // instead of firing a spurious "hat changed" on load.
            vanillaHatTrackingInitialized = false;
            lastKnownVanillaHatId = null;
            lastKnownVanillaPantsName = null;
            ResetClothesState(true);
            otherNpcClothesReactionSystem?.Reset();
            outfitAiService?.LoadProfiles(quiet: true);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            ResetClothesState(true);
            otherNpcClothesReactionSystem?.Reset();
            // Clear the cross-mod reaction flag in case the game closed mid-reaction; the Update loop
            // will re-set it if a reaction is genuinely active.
            Game1.player?.modData?.Remove(ReactionActiveModDataKey);
            outfitAiService?.LoadProfiles(quiet: true);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            ResetClothesState(true);
            otherNpcClothesReactionSystem?.Reset();
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
                return;

            foreach (OwnAiPendingGeneration pending in pendingOwnAiGenerations.Values.ToList())
            {
                if (pending == null || pending.Task == null || pending.Task.IsCompleted)
                    continue;

                NPC npc = Game1.getCharacterFromName(pending.NpcName);
                if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                    continue;

                DrawOwnAiWaitingHudMessage(e.SpriteBatch, npc, GetOwnAiWaitingDialogueText(npc, pending.WaitingDotCount));
                return;
            }

            foreach (OwnAiPendingPlayerReplyGeneration pending in pendingOwnAiPlayerReplyGenerations.Values.ToList())
            {
                if (pending == null || pending.Task == null || pending.Task.IsCompleted)
                    continue;

                NPC npc = Game1.getCharacterFromName(pending.NpcName);
                if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                    continue;

                DrawOwnAiWaitingHudMessage(e.SpriteBatch, npc, GetOwnAiReplyWaitingDialogueText(npc, pending.WaitingDotCount));
                return;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
                return;

            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
                return;

            NPC npc = GetNpcBeingInteractedWith();
            if (npc == null)
                return;

            // This runs right before Stardew handles the click. Some vanilla/other-mod first-talk
            // dialogues are generated at click time and can jump in front of the outfit line if we
            // only queued it earlier. Re-queue the outfit reaction here so it is definitely the
            // first dialogue seen, while our backup/restore logic keeps the previous dialogue for
            // the next click.
            if (TryPrioritizeSpouseOutfitDialogueForClick(npc))
                return;

            otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npc);
        }

        private NPC GetNpcBeingInteractedWith()
        {
            if (Game1.player == null || Game1.currentLocation == null)
                return null;

            // Main Stardew interaction target: the tile the farmer is facing/grabbing.
            Vector2 grabTile = Game1.player.GetGrabTile();
            NPC npc = Game1.currentLocation.characters
                .OfType<NPC>()
                .FirstOrDefault(c => c != null && !c.IsInvisible && c.TilePoint.X == (int)grabTile.X && c.TilePoint.Y == (int)grabTile.Y);

            if (npc != null)
                return npc;

            // Mouse fallback, useful when the player right-clicks an NPC directly.
            int mouseTileX = (Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize;
            int mouseTileY = (Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;

            npc = Game1.currentLocation.characters
                .OfType<NPC>()
                .Where(c => c != null && !c.IsInvisible && c.TilePoint.X == mouseTileX && c.TilePoint.Y == mouseTileY)
                .OrderBy(c => Vector2.Distance(c.Position, Game1.player.Position))
                .FirstOrDefault(c => Vector2.Distance(c.Position, Game1.player.Position) <= 192f);

            if (npc != null)
                return npc;

            // Last fallback for slightly offset sprites or modded NPC sizes.
            return Game1.currentLocation.characters
                .OfType<NPC>()
                .Where(c => c != null && !c.IsInvisible && c.currentLocation == Game1.player.currentLocation)
                .Where(c => Vector2.Distance(c.Position, Game1.player.Position) <= 112f)
                .OrderBy(c => Vector2.Distance(c.Position, Game1.player.Position))
                .FirstOrDefault();
        }

        private bool TryPrioritizeSpouseOutfitDialogueForClick(NPC npc)
        {
            if (npc == null || clothesReactingNpc == null)
                return false;

            if (!npc.Name.Equals(clothesReactingNpc.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!clothesComplimentReady || !outfitSequenceActive)
                return false;

            if (lastFashionSenseChangeInfo == null)
                return false;

            if (!CanNpcNoticeCurrentOutfitNotice(npc))
                return false;

            if (string.IsNullOrWhiteSpace(spouseDialogueBackupNpcName))
                CaptureSpouseDialogueBeforeOutfit(npc);
            else
                TemporarilySkipSpouseFirstDailyDialogue(npc);

            bool queued = QueueSpouseOutfitDialogueOnly(npc);
            if (queued)
                if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Re-prioritized outfit dialogue for {npc.Name} at click time.", LogLevel.Info);
            else
                KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "AI queue was not available on click.");

            return queued;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady || e == null || !e.IsLocalPlayer)
                return;

            if (isReactingToClothes || outfitSequenceActive)
                ResetClothesReactionState();

            otherNpcClothesReactionSystem?.Reset();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(OutfitAiService.NpcCharacteristicsAssetName))
            {
                e.LoadFrom(
                    () => outfitAiService?.LoadDefaultProfilesFromFiles() ?? new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase),
                    AssetLoadPriority.Low
                );
                return;
            }
        }

        private void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            foreach (var name in e.NamesWithoutLocale)
            {
                string assetName = name.ToString();

                if (assetName.StartsWith(AssetPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (assetName.Equals(OutfitAiService.NpcCharacteristicsAssetName, StringComparison.OrdinalIgnoreCase))
                {
                    outfitAiService?.LoadProfiles(quiet: true);
                    continue;
                }
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
                return;

            bool newIsFs = e.NewMenu != null && IsFashionSenseMenu(e.NewMenu);
            bool oldIsFs = e.OldMenu != null && IsFashionSenseMenu(e.OldMenu);

            if (newIsFs && !fashionSenseMenuOpen)
            {
                fashionSenseMenuOpen = true;
                fsSnapshotBefore = CaptureFashionSenseSnapshot();
                return;
            }

            if (fashionSenseMenuOpen && newIsFs)
                return;

            if (fashionSenseMenuOpen && oldIsFs && e.NewMenu == null)
            {
                fashionSenseMenuOpen = false;

                DelayedAction.functionAfterDelay(() =>
                {
                    if (!Context.IsWorldReady || Game1.player == null)
                        return;

                    FashionSenseSnapshot after = CaptureFashionSenseSnapshot();
                    FashionSenseChangeInfo changeInfo = CompareFashionSenseSnapshots(fsSnapshotBefore, after);
                    fsSnapshotBefore = null;

                    // Keep the lightweight vanilla poller in sync with changes handled by the
                    // Fashion Sense menu-close path. This prevents duplicate reactions after a
                    // Fashion Sense hat hides/reveals a vanilla hat underneath it.
                    lastKnownVanillaHatId = after?.VanillaHat ?? "";
                    lastKnownVanillaPantsName = after?.VanillaPants ?? "";
                    lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(after?.VanillaPantsSpecialItemCandidates);
                    vanillaHatTrackingInitialized = true;

                    if (changeInfo != null && changeInfo.CountChanges() > 0)
                    {
                        ApplyDetectedClothesChange(changeInfo);
                    }
                }, 200);
            }
        }

        /// <summary>
        /// Polls the vanilla hat slot (Game1.player.hat) on a light interval and, if it changed
        /// since the last check, builds a change and routes it through the normal reaction flow.
        /// This is needed because vanilla hats can be equipped/removed from the inventory without
        /// ever opening the Fashion Sense menu, which is the only other place changes are detected.
        /// </summary>
        private void PollVanillaHatChange()
        {
            // Light throttle: check a few times per second, not every tick.
            if (vanillaHatPollTimer > 0)
            {
                vanillaHatPollTimer--;
                return;
            }
            vanillaHatPollTimer = 15;

            // Don't poll while the Fashion Sense menu is open; that path handles changes on close.
            if (fashionSenseMenuOpen)
                return;

            string currentHatId = GetVisibleVanillaHatId();
            string currentHatName = GetCurrentVanillaHatName();
            List<string> currentHatSpecialItemCandidates = !string.IsNullOrWhiteSpace(currentHatId)
                ? GetCurrentVisibleVanillaHatSpecialItemCandidates(currentHatName)
                : new List<string>();
            string currentPantsName = GetCurrentVanillaPantsName();
            List<string> currentPantsSpecialItemCandidates = !string.IsNullOrWhiteSpace(currentPantsName)
                ? GetCurrentVanillaPantsSpecialItemCandidates(currentPantsName)
                : new List<string>();

            // First observation after load: just record the baseline, don't fire a reaction.
            if (!vanillaHatTrackingInitialized)
            {
                lastKnownVanillaHatId = currentHatId;
                lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(currentHatSpecialItemCandidates);
                lastKnownVanillaPantsName = currentPantsName ?? "";
                lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(currentPantsSpecialItemCandidates);
                vanillaHatTrackingInitialized = true;
                return;
            }

            bool hatChanged = !string.Equals(currentHatId, lastKnownVanillaHatId ?? "", StringComparison.OrdinalIgnoreCase);
            bool pantsChanged = !string.Equals(currentPantsName ?? "", lastKnownVanillaPantsName ?? "", StringComparison.OrdinalIgnoreCase);

            if (!hatChanged && !pantsChanged)
                return; // no change

            // Build before/after snapshots so the vanilla-hat/pants fields are populated correctly.
            FashionSenseSnapshot before = CaptureFashionSenseSnapshot();
            before.VanillaHat = lastKnownVanillaHatId ?? "";
            before.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaHatSpecialItemCandidates);
            before.VanillaPants = lastKnownVanillaPantsName ?? "";
            before.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaPantsSpecialItemCandidates);
            FashionSenseSnapshot after = CaptureFashionSenseSnapshot();
            after.VanillaHat = currentHatId;
            after.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(currentHatSpecialItemCandidates);
            after.VanillaPants = currentPantsName ?? "";
            after.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(currentPantsSpecialItemCandidates);

            lastKnownVanillaHatId = currentHatId;
            lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(currentHatSpecialItemCandidates);
            lastKnownVanillaPantsName = currentPantsName ?? "";
            lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(currentPantsSpecialItemCandidates);

            FashionSenseChangeInfo changeInfo = CompareFashionSenseSnapshots(before, after);
            int changeCount = changeInfo?.CountChanges() ?? 0;

            if (DebugLog)
            {
                Monitor.Log(
                    $"[VANILLA POLL] hatChanged={hatChanged} (now='{currentHatId}' was='{before.VanillaHat}') | " +
                    $"pantsChanged={pantsChanged} (now='{currentPantsName}' was='{before.VanillaPants}') | " +
                    $"changeCount={changeCount} vanillaPantsChanged={changeInfo?.VanillaPantsChanged} vanillaPantsRemoved={changeInfo?.VanillaPantsRemoved} " +
                    $"fsPantsAfter='{after.Pants}' pantsDebug={GetCurrentVanillaPantsDebugString()}",
                    LogLevel.Info);
            }

            // Delay applying the change by 200ms, mirroring the Fashion Sense menu-close path.
            // ApplyDetectedClothesChange resets the per-NPC notice tracking, which makes every
            // nearby NPC eligible again and triggers a burst of line-of-sight checks on the next
            // Update() tick. Applying it immediately means that burst lands on the exact tick the
            // player equips the hat, which is felt as a hitch right at the click. Delaying it
            // decouples the cost from the click the same way the FS path already does, so it's
            // no longer synchronized with (and therefore no longer noticeable at) the moment of
            // interaction. All tracking fields above are already updated synchronously, so a
            // fresh poll during the delay window won't re-detect the same change.
            if (changeInfo != null && changeCount > 0)
            {
                DelayedAction.functionAfterDelay(() =>
                {
                    if (!Context.IsWorldReady || Game1.player == null)
                        return;

                    ApplyDetectedClothesChange(changeInfo);
                }, 200);
            }
        }

        /// <summary>
        /// Applies a freshly detected clothing change: cancels any stale pending notice, sets the
        /// new change as current, and notifies the other-NPC reaction system. Shared by the Fashion
        /// Sense menu-close path and the periodic vanilla-hat poll.
        /// </summary>
        private void ApplyDetectedClothesChange(FashionSenseChangeInfo changeInfo)
        {
            // A fresh change should cancel any previous one-shot hair/hat/accessory notice that may
            // still be waiting for cleanup. Without this, changing A -> B could leave the old
            // pending reaction blocking the new one.
            ResetClothesState(clearChangeFlag: true);
            npcsReactedToCurrentNotice.Clear();
            loggedSpecialItemDebugKeys.Clear();
            otherNpcClothesReactionSystem?.Reset();

            // A genuine change resets the "already made eligible" tracker so the new look is treated
            // as fresh (and the notice refresh can later re-arm it correctly).
            lastEligibleSavedOutfitId = "";

            lastFashionSenseChangeInfo = changeInfo;
            changedClothes = true;
            otherNpcClothesReactionSystem?.NotifyOutfitChanged();

            if (DebugLog) Monitor.Log(
                $"[FS] outfit change detected | total={changeInfo.CountChanges()} hair={changeInfo.ChangedHair} accessory={changeInfo.ChangedAccessory} hat={changeInfo.ChangedHat} vanillaHat={changeInfo.VanillaHatChanged} shirt={changeInfo.ChangedShirt} pants={changeInfo.ChangedPants} sleeves={changeInfo.ChangedSleeves} shoes={changeInfo.ChangedShoes} outfit={changeInfo.ChangedOutfit} newHair={changeInfo.NewHairId} newHat={changeInfo.NewHatId} newAccessory={changeInfo.NewAccessoryId}",
                LogLevel.Info
            );

            if (changeInfo.ChangedAccessory && !AreVisionOnlyFashionSenseTriggersEnabled())
            {
                bool willNotice = ItemNameRevealsShape(changeInfo.NewAccessoryId);
                if (DebugLog) Monitor.Log(willNotice
                    ? "[FS] Accessory changed (no vision): item name reveals its shape, so it will be noticed."
                    : "[FS] Accessory changed (no vision): item name is too generic to describe, so it is skipped.", LogLevel.Info);
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !e.IsMultipleOf(1) || !Config.Enabled)
                return;

            UpdateReactionActiveModDataFlag();

            if (clothesNoticePauseTimer > 0)
                clothesNoticePauseTimer--;

            if (clothesChaseTimer > 0)
                clothesChaseTimer--;

            if (clothesSecondNoticeCooldown > 0)
                clothesSecondNoticeCooldown--;

            if (clothesInteractionCooldown > 0)
                clothesInteractionCooldown--;

            if (spousePendingOutfitBubbleTimer > 0)
                spousePendingOutfitBubbleTimer--;

            RefreshCurrentSavedOutfitNoticeCandidate();
            PollVanillaHatChange();

            NPC spouse = GetSpouse();
            NPC datingNpc = GetDatingNpc();
            // The "active partner" for the close-partner reaction flow is the spouse when present,
            // otherwise the dating NPC. If both exist (polyamory mods), spouse takes priority for
            // the reaction system (only one can be the active focus at a time) while the dating NPC
            // is still excluded from the other-NPC system below.
            NPC activePartner = spouse ?? datingNpc;

            if (activePartner != null)
            {
                UpdateClothesReactionSystem(activePartner);
            }
            else if (changedClothes && lastFashionSenseChangeInfo != null && !ShouldStartClothesReaction(spouse))
            {
                ResetClothesState(true);
            }

            UpdateSpousePostOutfitLinger();

            UpdatePendingOwnAiGenerations();
            UpdatePendingOwnAiPlayerReplyGenerations();
            // Exclude both the official spouse AND the dating NPC from the other-NPC system —
            // both get the close-partner treatment above instead.
            string excludedPartnerName = activePartner?.Name ?? Game1.player?.spouse;
            otherNpcClothesReactionSystem?.Update(excludedPartnerName);
        }

        private void RefreshCurrentSavedOutfitNoticeCandidate()
        {
            if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
                return;

            // If the player just changed only hair/accessories/clothing pieces without switching
            // a saved outfit, keep that immediate reaction instead of replacing it with the
            // current saved outfit candidate. Once that reaction is consumed/cancelled, the
            // saved outfit can become eligible again.
            if (changedClothes && lastFashionSenseChangeInfo != null && !lastFashionSenseChangeInfo.ChangedOutfit)
                return;

            if (!TryGetCurrentSavedFashionSenseOutfitId(out string currentOutfitId))
            {
                // If the player removed/cleared the saved FS outfit, stop keeping the
                // previous saved outfit eligible for notices. Hair-only reactions are
                // left alone because they are handled by the immediate FS change flow.
                if (IsSavedOutfitNoticeChange(lastFashionSenseChangeInfo))
                {
                    ResetClothesState(true);
                    otherNpcClothesReactionSystem?.Reset();
                }

                return;
            }

            if (lastFashionSenseChangeInfo != null &&
                lastFashionSenseChangeInfo.ChangedOutfit &&
                string.Equals(lastFashionSenseChangeInfo.NewOutfitId, currentOutfitId, StringComparison.OrdinalIgnoreCase))
            {
                changedClothes = true;
                return;
            }

            FashionSenseChangeInfo currentOutfitChange = new()
            {
                ChangedOutfit = true,
                NewOutfitId = currentOutfitId
            };

            if (string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(currentOutfitChange)))
                return;

            // Only fire a fresh "outfit changed" notification (which clears the per-outfit reacted
            // set) when this saved outfit is actually NEW since the last time we made it eligible.
            // Re-firing for the same unchanged outfit would let NPCs notice it over and over.
            if (string.Equals(lastEligibleSavedOutfitId, currentOutfitId, StringComparison.OrdinalIgnoreCase))
            {
                changedClothes = true;
                return;
            }
            lastEligibleSavedOutfitId = currentOutfitId;
            npcsReactedToCurrentNotice.Clear();

            lastFashionSenseChangeInfo = currentOutfitChange;
            changedClothes = true;
            otherNpcClothesReactionSystem?.NotifyOutfitChanged();

            if (DebugLog) Monitor.Log($"[CLOTHES NOTICE] Current saved outfit is eligible for outfit notices: {currentOutfitId}", LogLevel.Info);
        }

        private bool IsSavedOutfitNoticeChange(FashionSenseChangeInfo changeInfo)
        {
            return changeInfo != null
                && changeInfo.ChangedOutfit
                && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
        }

        private bool IsVanillaHatRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
        {
            return changeInfo != null
                && changeInfo.VanillaHatRemoved
                && changeInfo.VanillaHatChanged
                && changeInfo.CountChanges() == 1;
        }

        private bool IsSpecialItemRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
        {
            // A special item can be "removed" even when the vanilla pants slot did not become
            // literally empty (e.g. Mayor's shorts -> Farmer Pants). What matters is that the
            // previous visible vanilla pants matched luckypurpleshorts.json, and the current
            // visible pants no longer match that same special item. Same logic applies to
            // special items worn as hats (e.g. the mod short-hat).
            if (changeInfo == null || changeInfo.CountChanges() != 1)
                return false;

            if (!changeInfo.VanillaPantsChanged && !changeInfo.VanillaHatChanged)
                return false;

            return TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out SpecialItemNoticeInfo notice)
                && notice != null
                && notice.WasRemoved;
        }

        private bool NpcRemembersRemovedSpecialItem(NPC npc, FashionSenseChangeInfo changeInfo)
        {
            return npc != null
                && changeInfo != null
                && TryResolveSpecialItemNoticeForNpc(npc, changeInfo, requireNpcMemoryForRemoval: false, out SpecialItemNoticeInfo notice)
                && notice != null
                && notice.WasRemoved
                && HasSpecialItemMemory(npc, notice);
        }

        private bool NpcRemembersRemovedVanillaHat(NPC npc)
        {
            return npc != null
                && !string.IsNullOrWhiteSpace(hatMemoryService?.GetLastHatNameForNpc(npc.Name) ?? "");
        }

        private FashionSenseChangeInfo TryBuildCurrentSavedOutfitNoticeChange()
        {
            if (!TryGetCurrentSavedFashionSenseOutfitId(out string currentOutfitId)
                || string.IsNullOrWhiteSpace(currentOutfitId))
                return null;

            FashionSenseChangeInfo outfitChange = new()
            {
                ChangedOutfit = true,
                NewOutfitId = currentOutfitId
            };

            return string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(outfitChange))
                ? null
                : outfitChange;
        }

        /// <summary>
        /// Returns the change this specific NPC should react to. A vanilla-hat removal is only
        /// meaningful to NPCs who remember seeing that hat. NPCs who did not witness it can fall
        /// back to the currently equipped saved outfit, if one is still active, instead of being
        /// forced into a hat topic they have no context for.
        /// </summary>
        private FashionSenseChangeInfo GetEffectiveFashionSenseChangeInfoForNpc(NPC npc)
        {
            if (lastFashionSenseChangeInfo == null)
                return null;

            // Special item removal (e.g. the short-hat mod) must be handled BEFORE vanilla hat
            // removal, because the short-hat triggers both VanillaHatChanged and VanillaHatRemoved,
            // and the vanilla hat removal path would incorrectly fall back to the saved outfit
            // (it uses HatMemoryService, which doesn't know about special items).
            if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo) && npc != null)
            {
                // If this NPC has already reacted to the removal notice, there is nothing
                // more to show — return null so they do not react a second time.
                if (npcsReactedToCurrentNotice.Contains(npc.Name ?? ""))
                    return null;

                // If the NPC remembers the removed special item, the removal notice applies to
                // them directly. Return it here so the vanilla-hat block below cannot hijack it
                // with a saved-outfit fallback (its memory check uses HatMemoryService, which
                // knows nothing about special items).
                if (NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
                    return lastFashionSenseChangeInfo;

                // NPC has no memory of the item — they can't react to its removal.
                // Fall back to a saved-outfit notice if one is available.
                FashionSenseChangeInfo specialItemFallback = TryBuildCurrentSavedOutfitNoticeChange();
                if (specialItemFallback != null)
                    return specialItemFallback;
            }

            if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo)
                && npc != null
                && !NpcRemembersRemovedVanillaHat(npc))
            {
                FashionSenseChangeInfo fallbackOutfitChange = TryBuildCurrentSavedOutfitNoticeChange();
                if (fallbackOutfitChange != null)
                    return fallbackOutfitChange;
            }

            return lastFashionSenseChangeInfo;
        }

        private bool CanNpcNoticeCurrentOutfitNotice(NPC npc)
        {
            if (npc == null)
                return false;

            // Gating is now per active outfit/change notice, not tied to an in-game calendar period.
            // The set is cleared only when a real new change is detected, so the same NPC
            // will not repeat the same pending notice, but can react again after the look changes.
            return !HasNpcReactedToCurrentOutfitNotice(npc, lastFashionSenseChangeInfo?.NewOutfitId);
        }

        // True if this NPC has already seen the player's CURRENT look before, so the reaction system
        // can use the lower "repeated visual" chance instead of the "new visual" chance.
        // Saved outfits use the deep outfit memory; vanilla hats use the dedicated hat memory.
        private bool HasNpcSeenCurrentVisualBefore(NPC npc)
        {
            if (npc == null)
                return false;

            FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);

            if (TryResolveSpecialItemNoticeForNpc(npc, effectiveChangeInfo, requireNpcMemoryForRemoval: false, out SpecialItemNoticeInfo specialItemNotice)
                && specialItemNotice != null
                && specialItemNotice.IsValid)
                return HasSpecialItemMemory(npc, specialItemNotice);

            if (IsSavedOutfitNoticeChange(effectiveChangeInfo) && outfitMemoryService != null)
            {
                string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveChangeInfo.NewOutfitId);
                if (!string.IsNullOrWhiteSpace(currentOutfitId))
                {
                    OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
                    var memory = outfitMemoryService.GetMemory(npc.Name, currentOutfitId, current);
                    return memory != null && memory.TimesSeenBefore > 0;
                }
            }

            string hatId = GetVisibleVanillaHatId();
            if (!string.IsNullOrWhiteSpace(hatId) && hatMemoryService != null)
            {
                var memory = hatMemoryService.GetMemory(npc.Name, hatId, GetCurrentVanillaHatName());
                return memory != null && memory.TimesSeenBefore > 0;
            }

            return false;
        }

        /// <summary>
        /// True only when this NPC plausibly already saw the farmer's PREVIOUS look (the one that
        /// included the accessory that was just removed). Used so an NPC the farmer never interacted
        /// with while wearing the accessory does not talk about it as something they remember "from
        /// before". We treat the NPC as a witness if they have prior memory of the current saved
        /// outfit, since that means they have seen this farmer in this outfit/combination before.
        /// </summary>
        private bool DidNpcWitnessPreviousLook(NPC npc)
        {
            if (npc == null)
                return false;

            // If this NPC already reacted to the current notice earlier, they are part of
            // this ongoing change sequence and can reference what just happened.
            if (npcsReactedToCurrentNotice.Contains(npc.Name ?? ""))
                return true;

            // Otherwise, only count them as a witness if they have prior memory of this saved outfit.
            if (outfitMemoryService != null && lastFashionSenseChangeInfo != null)
            {
                string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(lastFashionSenseChangeInfo.NewOutfitId);
                if (!string.IsNullOrWhiteSpace(currentOutfitId))
                {
                    OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
                    var memory = outfitMemoryService.GetMemory(npc.Name, currentOutfitId, current);
                    if (memory != null && memory.TimesSeenBefore > 0)
                        return true;
                }
            }

            return false;
        }

        private void MarkCurrentOutfitAsNoticed(NPC npc)
        {
            if (npc == null || lastFashionSenseChangeInfo == null)
                return;

            string npcName = npc.Name ?? "";
            FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);
            if (effectiveChangeInfo == null)
                return;

            // This method can be reached twice for the same final dialogue: once when the AI line
            // opens and once when the non-spouse restore path runs after the menu closes. Guard first
            // so outfit/hat memory is recorded only once per actual reaction.
            if (npcsReactedToCurrentNotice.Contains(npcName))
                return;

            bool specialItemOnlyReaction = ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(npc, effectiveChangeInfo, out SpecialItemNoticeInfo specialItemNotice);
            bool vanillaHatOnlyReaction = ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(npc);

            npcsReactedToCurrentNotice.Add(npcName);

            if (specialItemOnlyReaction)
            {
                RecordSpecialItemMemory(npc, specialItemNotice);

                if (DebugLog)
                    Monitor.Log($"[SPECIAL ITEM MEMORY] {npc.Name} reacted to special item '{specialItemNotice?.EntryId}'; saved outfit memory was not updated for this item-focused reaction.", LogLevel.Info);

                // If the NPC reacted to a REMOVAL (the item is no longer equipped), the notice
                // has been consumed. Clear the change info so subsequent NPC interactions do not
                // re-trigger the same "just removed" reaction indefinitely.
                // Equip reactions (WasRemoved=false) are intentionally left alive so other
                // nearby NPCs can still notice the same currently-worn special item.
                if (specialItemNotice?.WasRemoved == true)
                {
                    if (DebugLog)
                        Monitor.Log($"[SPECIAL ITEM MEMORY] Special item '{specialItemNotice.EntryId}' was a removal reaction; clearing the notice so it does not repeat.", LogLevel.Info);
                    changedClothes = false;
                    lastFashionSenseChangeInfo = null;
                    // Stamp the current saved outfit as already eligible so RefreshCurrentSavedOutfitNoticeCandidate
                    // does not immediately re-arm it and trigger a second reaction on the next tick.
                    if (TryGetCurrentSavedFashionSenseOutfitId(out string currentOutfitId) && !string.IsNullOrWhiteSpace(currentOutfitId))
                        lastEligibleSavedOutfitId = currentOutfitId;
                }

                return;
            }

            if (vanillaHatOnlyReaction)
            {
                RecordVanillaHatMemory(npc);

                string currentPantsName = GetCurrentVanillaPantsName();
                if (!string.IsNullOrWhiteSpace(currentPantsName))
                    RecordVanillaPantsMemory(npc, currentPantsName);

                if (DebugLog) Monitor.Log($"[HAT MEMORY] {npc.Name} reacted to a vanilla-hat focused notice; saved outfit memory was not updated for this hat-focused reaction.", LogLevel.Info);
                return;
            }

            if (IsSavedOutfitNoticeChange(effectiveChangeInfo))
            {
                // Save outfit memory so next time the NPC recognises this look.
                // Capture the current equipped pieces from Fashion Sense, not only the one
                // piece reported by the latest change. This keeps outfit+accessory combos
                // accurate when a player adds/removes wings, capes, backpacks, etc.
                string currentSavedOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveChangeInfo.NewOutfitId);
                if (!string.IsNullOrWhiteSpace(currentSavedOutfitId))
                    RecordOutfitMemory(npc, currentSavedOutfitId);

                if (DebugLog) Monitor.Log($"[CLOTHES NOTICE] Recorded that {npc.Name} reacted to outfit '{currentSavedOutfitId}'.", LogLevel.Info);
                return;
            }

            if (IsImmediateFashionSenseNoticeChange(effectiveChangeInfo))
            {
                // Hair/hat/accessory changes are immediate one-shot notices. Mark only THIS npc
                // as having reacted, so the same NPC won't repeat the compliment on every click,
                // but other nearby NPCs can still react to the same change.
                if (DebugLog) Monitor.Log($"[FS] {npc.Name} reacted to the immediate change; it stays available for other NPCs.", LogLevel.Info);

                // Record what vanilla hat (or bare head) this NPC just saw, so future sessions can
                // remember it — independent of any saved Fashion Sense outfit.
                if (effectiveChangeInfo.VanillaHatChanged || !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
                    RecordVanillaHatMemory(npc);

                // Record vanilla pants this NPC just saw (e.g. Mayor's Purple Shorts).
                string currentPantsName = GetCurrentVanillaPantsName();
                if (!string.IsNullOrWhiteSpace(currentPantsName))
                    RecordVanillaPantsMemory(npc, currentPantsName);

                // If the immediate change happened while a saved outfit is still equipped,
                // update that NPC's memory for the whole current combination too. This lets
                // them notice: "same Pikachu outfit, but the moth wings are gone" or
                // "now the black wings replaced the moth wings" instead of acting like
                // each accessory is unrelated to the saved outfit.
                string currentSavedOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveChangeInfo.NewOutfitId);
                if (!string.IsNullOrWhiteSpace(currentSavedOutfitId))
                    RecordOutfitMemory(npc, currentSavedOutfitId);
            }
        }

        private bool ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(NPC npc, FashionSenseChangeInfo effectiveChangeInfo, out SpecialItemNoticeInfo notice)
        {
            notice = null;

            if (npc == null || effectiveChangeInfo == null)
                return false;

            if (!TryResolveSpecialItemNoticeForNpc(npc, effectiveChangeInfo, requireNpcMemoryForRemoval: true, out notice))
                return false;

            return notice != null && notice.IsValid;
        }

        private bool ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(NPC npc)
        {
            if (lastFashionSenseChangeInfo == null)
                return false;

            if (ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) != "HatOnly")
                return false;

            // Keep memory aligned with the exact prompt mode: when HatOnly is active, the model is
            // told to ignore the outfit only if there is a visible vanilla hat now, or if THIS NPC
            // remembers the vanilla hat that was just removed. NPCs who never saw the removed hat
            // must not be forced into a hat-focused reaction.
            if (!string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
                return true;

            return IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo)
                && NpcRemembersRemovedVanillaHat(npc);
        }

        private static bool IsImmediateFashionSenseNoticeChange(FashionSenseChangeInfo changeInfo)
        {
            return changeInfo != null
                && !IsSavedOutfitNoticeChangeStatic(changeInfo)
                && (changeInfo.ChangedHair || changeInfo.ChangedHat || changeInfo.ChangedAccessory);
        }

        private static bool IsSavedOutfitNoticeChangeStatic(FashionSenseChangeInfo changeInfo)
        {
            return changeInfo != null
                && changeInfo.ChangedOutfit
                && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
        }

        private bool HasNpcReactedToCurrentOutfitNotice(NPC npc, string outfitId)
        {
            if (npc == null)
                return false;

            return npcsReactedToCurrentNotice.Contains(npc.Name ?? "");
        }

        private static string MakeSafeModDataPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            string normalized = NormalizeOutfitText(value);
            StringBuilder builder = new();

            foreach (char c in normalized)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(c);
                else if (c == '_' || c == '-')
                    builder.Append(c);
            }

            return builder.Length > 0 ? builder.ToString() : "unknown";
        }

        private static string GetStableHexHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                string text = value ?? "";

                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                return hash.ToString("x8", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Returns true if the NPC is roughly facing the player (within a ~120 degree cone).
        /// Direction: 0=up, 1=right, 2=down, 3=left (Stardew convention).
        /// </summary>
        private static bool IsNpcFacingPlayer(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return false;

            Vector2 npcPos    = npc.getStandingPosition();
            Vector2 playerPos = Game1.player.getStandingPosition();
            Vector2 delta     = playerPos - npcPos;

            // If very close, direction doesn't matter.
            if (delta.LengthSquared() < 16f * 16f)
                return true;

            // Allow a wide cone: the player must be in the forward half-plane.
            return npc.FacingDirection switch
            {
                0 => delta.Y < 0,   // facing up    → player must be above
                1 => delta.X > 0,   // facing right → player must be to the right
                2 => delta.Y > 0,   // facing down  → player must be below
                3 => delta.X < 0,   // facing left  → player must be to the left
                _ => true
            };
        }

        private float DistanceToPlayer(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return float.MaxValue;

            return Vector2.Distance(npc.Position, Game1.player.Position);
        }

        private bool CanNpcReactToCurrentOutfitNotice(NPC npc)
        {
            // Other-NPC reactions need both checks: the NPC must be eligible/profiled, and the
            // current pending notice must be valid for this specific NPC. This matters most for
            // vanilla-hat removals: NPCs who never saw the hat should not react to its absence.
            return CanNpcReactToOutfit(npc) && ShouldStartClothesReaction(npc);
        }

        // Cross-mod flag read from a bystander NPC's own modData while Lots of Kisses has them
        // turned to watch a public multi-kiss. While present, this NPC must not start an outfit
        // reaction — it would visually collide with the kiss audience moment. Using modData means
        // no hard dependency or load-order requirement between the two mods.
        private const string LotsOfKissesBystanderWatchingModDataKey = "NatrollEXE.LotsOfKisses/BystanderWatching";

        private bool IsNpcWatchingAsKissBystander(NPC npc)
        {
            return npc?.modData != null
                && npc.modData.ContainsKey(LotsOfKissesBystanderWatchingModDataKey);
        }

        private bool CanNpcReactToOutfit(NPC npc)
        {
            if (npc == null || string.IsNullOrWhiteSpace(npc.Name))
                return false;

            // Already reacted to the current active notice? Don't repeat for this NPC,
            // but other NPCs can still react until the look changes.
            if (npcsReactedToCurrentNotice.Contains(npc.Name))
                return false;

            // Lots of Kisses has this NPC turned to watch a public multi-kiss right now — do not
            // start an outfit reaction on them until they're released back to normal.
            if (IsNpcWatchingAsKissBystander(npc))
                return false;

            // Any NPC with an enabled AI profile is allowed to notice outfits.
            // Content packs only need assets/npc-characteristics/*.json; no ReactingNpcs.json is needed.
            return outfitAiService?.HasProfile(npc.Name) == true;
        }

        private bool HasMinimumFriendshipForOutfitReaction(NPC npc)
        {
            // Kept as a compatibility helper for older call sites, but the current design
            // allows outfit reactions at any heart level. The prompt controls intimacy/richness.
            return npc != null;
        }

        /// <summary>
        /// Returns a stable identifier for the VANILLA hat the farmer currently has equipped
        /// (Game1.player.hat), or "" if none. Used by the appearance snapshot so that equipping,
        /// swapping, or removing a vanilla hat is detected as a change (the same way Fashion Sense
        /// accessories are). Uses the item id so different hats produce different values.
        /// </summary>
        /// <summary>
        /// Returns how many portraits the NPC actually has in their portrait spritesheet. Each
        /// portrait is 64x64, laid out in a grid, so the count is (width/64) * (height/64). Returns
        /// 0 if it can't be determined (treated as "unknown" — no validation applied).
        /// </summary>
        private int GetNpcPortraitCount(NPC npc)
        {
            try
            {
                if (npc?.Portrait == null)
                    return 0;
                int cols = Math.Max(1, npc.Portrait.Width / 64);
                int rows = Math.Max(1, npc.Portrait.Height / 64);
                return cols * rows;
            }
            catch
            {
                return 0;
            }
        }

        private bool HasNoticeableCurrentFashionSenseAppearance()
        {
            return ShouldStartClothesReaction(null);
        }

        private FashionSenseSnapshot CaptureFashionSenseSnapshot()
        {
            if (Game1.player == null)
                return null;

            string fsHatId = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHat.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hat));
            bool fsHatCoversVanilla = IsFashionSenseHatCoveringVanilla();
            bool fsPantsCoverVanilla = IsFashionSensePantsCoveringVanilla();
            string visibleVanillaPantsName = fsPantsCoverVanilla ? "" : (GetCurrentVanillaPantsName() ?? "");
            List<string> visibleVanillaPantsCandidates = !string.IsNullOrWhiteSpace(visibleVanillaPantsName)
                ? GetCurrentVanillaPantsSpecialItemCandidates(visibleVanillaPantsName)
                : new List<string>();

            return new FashionSenseSnapshot
            {
                Hair = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHair.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hair)),
                Accessory = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Id"), GetFsModData("FashionSense.CustomAccessory.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Accessory))),
                AccessorySecondary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Id"), GetFsModData("FashionSense.CustomAccessorySecondary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessorySecondary))),
                AccessoryTertiary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Id"), GetFsModData("FashionSense.CustomAccessoryTertiary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessoryTertiary))),
                Hat = fsHatId,
                FashionSenseHatCoversVanilla = fsHatCoversVanilla,
                // Store the visible vanilla hat, not the raw equipped slot. A Fashion Sense
                // hat/headwear can cover the vanilla slot, and NPCs should only react to what
                // they can actually see.
                VanillaHat = fsHatCoversVanilla ? "" : GetCurrentVanillaHatId(),
                VanillaHatSpecialItemCandidates = !fsHatCoversVanilla && !string.IsNullOrWhiteSpace(GetCurrentVanillaHatName())
                    ? GetCurrentVisibleVanillaHatSpecialItemCandidates(GetCurrentVanillaHatName())
                    : new List<string>(),
                VanillaPants = visibleVanillaPantsName,
                VanillaPantsSpecialItemCandidates = visibleVanillaPantsCandidates,
                Shirt = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShirt.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shirt)),
                Pants = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomPants.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Pants)),
                Sleeves = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomSleeves.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Sleeves)),
                Shoes = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShoes.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shoes)),
                OutfitId = TryGetCurrentSavedFashionSenseOutfitId(out string currentOutfitId) ? currentOutfitId : null,
                HairColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hair"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hair)),
                AccessoryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.Accessory"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Accessory)),
                AccessorySecondaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessorySecondary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessorySecondary)),
                AccessoryTertiaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessoryTertiary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessoryTertiary)),
                HatColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hat"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hat)),
                ShirtColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shirt"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shirt)),
                PantsColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Pants"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Pants)),
                SleevesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Sleeves"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Sleeves)),
                ShoesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shoes"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shoes))
            };
        }

        private FashionSenseChangeInfo CompareFashionSenseSnapshots(FashionSenseSnapshot before, FashionSenseSnapshot after)
        {
            if (before == null || after == null)
                return null;

            bool afterHasSavedOutfit = !string.IsNullOrWhiteSpace(after.OutfitId);
            bool afterHasFashionSenseHair = !string.IsNullOrWhiteSpace(after.Hair);
            bool beforeOrAfterHasFashionSenseAccessory = !string.IsNullOrWhiteSpace(StringUtils.FirstNonEmpty(before.Accessory, before.AccessorySecondary, before.AccessoryTertiary, after.Accessory, after.AccessorySecondary, after.AccessoryTertiary));
            // A "real" Fashion Sense hat is one with a meaningful custom appearance id. Fashion Sense
            // can report a non-empty but generic/internal/default value in the hat slot even when the
            // farmer has no actual FS hat equipped, so we must NOT treat every non-empty after.Hat as
            // a covering FS hat — otherwise a plain vanilla hat (equipped OR removed) would always be
            // ignored. Only a meaningful custom id counts as actually covering the vanilla slot.
            // NOTE: Fashion Sense uses the literal string "None" (and blank) to mean "no hat", so those
            // must be treated as empty here.
            bool afterHasFashionSenseHat = !IsEmptyFashionSenseValue(after.Hat)
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(after.Hat);
            bool afterFashionSenseHatCoversVanilla = after.FashionSenseHatCoversVanilla || afterHasFashionSenseHat;
            // A vanilla hat (Game1.player.hat) is tracked separately from the Fashion Sense hat.
            // Any change to it — equipping a different hat, removing one, or putting one back on —
            // should be noticed just like accessory changes are. The equipped name can be blank
            // (removed), so we compare the raw ids and treat any difference as a change.
            // IMPORTANT: when a Fashion Sense hat is currently equipped, it visually covers/replaces
            // the vanilla hat slot entirely (the vanilla hat is not rendered and is not what the
            // farmer "is wearing" from an in-world perspective). So vanilla-hat changes must be
            // ignored completely whenever a Fashion Sense hat is active — equipping, swapping, or
            // removing a vanilla hat underneath a Fashion Sense hat is invisible and must not be
            // treated as a noticeable change.
            bool vanillaHatChanged = !afterFashionSenseHatCoversVanilla
                && !string.Equals(before.VanillaHat ?? "", after.VanillaHat ?? "", StringComparison.OrdinalIgnoreCase);
            // True specifically when a vanilla hat that WAS equipped is now gone (taken off).
            // Also gated on no Fashion Sense hat being active now, for the same reason as above.
            bool vanillaHatRemoved = !afterFashionSenseHatCoversVanilla
                && !string.IsNullOrWhiteSpace(before.VanillaHat) && string.IsNullOrWhiteSpace(after.VanillaHat);

            // Vanilla pants tracking (same pattern as vanilla hat).
            bool afterHasFsPants = IsFashionSensePantsValueCoveringVanilla(after.Pants);
            bool vanillaPantsChanged = !afterHasFsPants
                && !string.Equals(before.VanillaPants ?? "", after.VanillaPants ?? "", StringComparison.OrdinalIgnoreCase);
            bool vanillaPantsRemoved = !afterHasFsPants
                && !string.IsNullOrWhiteSpace(before.VanillaPants) && string.IsNullOrWhiteSpace(after.VanillaPants);
            bool accessoryIdChanged = before.Accessory != after.Accessory || before.AccessorySecondary != after.AccessorySecondary || before.AccessoryTertiary != after.AccessoryTertiary;
            bool accessoryColorChanged = before.AccessoryColor != after.AccessoryColor || before.AccessorySecondaryColor != after.AccessorySecondaryColor || before.AccessoryTertiaryColor != after.AccessoryTertiaryColor;
            bool outfitIdChanged = afterHasSavedOutfit && !string.Equals(before.OutfitId, after.OutfitId, StringComparison.OrdinalIgnoreCase);
            string changedAccessoryId = GetChangedAccessoryId(before, after, outfitIdChanged);
            bool afterHasVisibleAccessory = !string.IsNullOrWhiteSpace(BuildCurrentAccessoryMemoryValue(after));

            return new FashionSenseChangeInfo
            {
                // A saved Fashion Sense outfit, active Fashion Sense hair, hat/headwear,
                // or accessory can start a noticing reaction. Vanilla/default clothing pieces
                // and unsaved single-piece body clothing changes still should not trigger
                // emotes, looking, or AI/manual outfit dialogue by themselves.
                ChangedHair = afterHasFashionSenseHair && (before.Hair != after.Hair || before.HairColor != after.HairColor),
                // Accessory changes must be recognized even if the new accessory ID is blank/unknown
                // (some Fashion Sense slots/API versions can fail to expose secondary/tertiary accessory IDs,
                // or the player may remove an accessory). When the saved outfit changed too, never describe
                // a removed accessory from the PREVIOUS outfit as if it belonged to the NEW outfit; focus on
                // the accessory currently equipped with the new outfit instead.
                ChangedAccessory = outfitIdChanged
                    ? afterHasVisibleAccessory
                    : (accessoryIdChanged || (beforeOrAfterHasFashionSenseAccessory && accessoryColorChanged)),
                ChangedHat = (afterHasFashionSenseHat && (before.Hat != after.Hat || before.HatColor != after.HatColor)) || vanillaHatChanged,
                ChangedShirt = before.Shirt != after.Shirt || before.ShirtColor != after.ShirtColor,
                ChangedPants = before.Pants != after.Pants || before.PantsColor != after.PantsColor,
                ChangedSleeves = before.Sleeves != after.Sleeves || before.SleevesColor != after.SleevesColor,
                ChangedShoes = before.Shoes != after.Shoes || before.ShoesColor != after.ShoesColor,
                ChangedOutfit = outfitIdChanged,
                NewHairId = after.Hair,
                NewAccessoryId = changedAccessoryId,
                NewHatId = after.Hat,
                NewVanillaHatId = after.VanillaHat,
                VanillaHatChanged = vanillaHatChanged,
                VanillaHatRemoved = vanillaHatRemoved,
                PreviousVanillaHatId = before.VanillaHat,
                PreviousVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaHatSpecialItemCandidates),
                VanillaPantsChanged = vanillaPantsChanged,
                VanillaPantsRemoved = vanillaPantsRemoved,
                PreviousVanillaPantsName = before.VanillaPants,
                NewVanillaPantsName = after.VanillaPants,
                PreviousVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaPantsSpecialItemCandidates),
                NewVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(after.VanillaPantsSpecialItemCandidates),
                NewShirtId = after.Shirt,
                NewPantsId = after.Pants,
                NewSleevesId = after.Sleeves,
                NewShoesId = after.Shoes,
                NewOutfitId = after.OutfitId
            };
        }

        private static string GetChangedAccessoryId(FashionSenseSnapshot before, FashionSenseSnapshot after, bool outfitChanged)
        {
            if (before == null || after == null)
                return "";

            string currentAccessoryCombo = BuildCurrentAccessoryMemoryValue(after);

            // If the saved outfit changed, old accessories belong to the old look.
            // Example: Pikachu + moth wings -> dinosaur + black wings should be read as
            // CURRENT dinosaur + black wings, not as "where did the moth wings go?".
            // If the new outfit has no visible accessory, return blank so the NPC reacts to the outfit itself.
            if (outfitChanged)
                return currentAccessoryCombo;

            string changed = GetChangedAccessorySlotDescription(before.Accessory, after.Accessory, before.AccessoryColor, after.AccessoryColor, "accessory");
            if (!string.IsNullOrWhiteSpace(changed))
                return changed;

            changed = GetChangedAccessorySlotDescription(before.AccessorySecondary, after.AccessorySecondary, before.AccessorySecondaryColor, after.AccessorySecondaryColor, "secondary accessory");
            if (!string.IsNullOrWhiteSpace(changed))
                return changed;

            changed = GetChangedAccessorySlotDescription(before.AccessoryTertiary, after.AccessoryTertiary, before.AccessoryTertiaryColor, after.AccessoryTertiaryColor, "tertiary accessory");
            if (!string.IsNullOrWhiteSpace(changed))
                return changed;

            return currentAccessoryCombo;
        }

        private static string GetChangedAccessorySlotDescription(string beforeId, string afterId, string beforeColor, string afterColor, string slotLabel)
        {
            bool idChanged = !string.Equals(beforeId, afterId, StringComparison.OrdinalIgnoreCase);
            bool colorChanged = !string.Equals(beforeColor, afterColor, StringComparison.OrdinalIgnoreCase);
            if (!idChanged && !colorChanged)
                return "";

            if (!string.IsNullOrWhiteSpace(afterId))
                return afterId;

            if (!string.IsNullOrWhiteSpace(beforeId))
                return "removed " + beforeId;

            return "changed " + slotLabel;
        }

        private sealed class FashionSenseChangeInfo
        {
            public bool ChangedHair;
            public bool ChangedAccessory;
            public bool ChangedHat;
            public bool ChangedShirt;
            public bool ChangedPants;
            public bool ChangedSleeves;
            public bool ChangedShoes;
            public bool ChangedOutfit;
            public string NewHairId;
            public string NewAccessoryId;
            public string NewHatId;
            public string NewVanillaHatId;
            public bool VanillaHatChanged;
            public bool VanillaHatRemoved;
            public string PreviousVanillaHatId;
            public List<string> PreviousVanillaHatSpecialItemCandidates = new();
            public bool VanillaPantsChanged;
            public bool VanillaPantsRemoved;
            public string PreviousVanillaPantsName;
            public string NewVanillaPantsName;
            public List<string> PreviousVanillaPantsSpecialItemCandidates = new();
            public List<string> NewVanillaPantsSpecialItemCandidates = new();
            public string NewShirtId;
            public string NewPantsId;
            public string NewSleevesId;
            public string NewShoesId;
            public string NewOutfitId;

            public int CountChanges()
            {
                int count = 0;
                if (ChangedHair) count++;
                if (ChangedAccessory) count++;
                if (ChangedHat) count++;
                if (ChangedShirt) count++;
                if (ChangedPants) count++;
                if (VanillaPantsChanged) count++;
                if (ChangedSleeves) count++;
                // Shoes are intentionally excluded: the mod never reacts to or comments on footwear,
                // so a shoes-only change must not trigger an outfit reaction.
                if (ChangedOutfit) count++;
                return count;
            }
        }

        // A non-vision (text) AI can only describe an accessory/hat well if the item's NAME
        // reveals its shape (e.g. "Hawkmoth Wings", "Witch Hat"). Generic code names like
        // "pack0010" or "item3" tell the AI nothing, so in non-vision mode we skip those to
        // avoid vague compliments. Vision mode can always see the shape, so it isn't gated.

        // Cached reflection for NPC internals not exposed publicly in 1.6.

        // When Fashion Sense reports no tint for the hair (texture-painted hair), fall back to
        // the dominant hair color read from the rendered sprite pixels, and present it to the
        // model as a confirmed, authoritative color so it never guesses from the raw image.

        // Same idea as hair: when the player changed a Fashion Sense hat/headwear item,
        // use the rendered transparent sprite to provide an authoritative hat color. This
        // avoids relying on FS tint data for texture-painted hats, and prevents the model
        // from guessing color from floors, walls, or lighting.

    }
}
