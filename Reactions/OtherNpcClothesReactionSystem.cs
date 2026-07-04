using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OutfitReactions
{
    /// <summary>
    /// Light outfit reactions for NPCs who are not the player's spouse.
    /// This system never starts pathfinding, never halts schedules, and keeps saved outfits eligible
    /// until each NPC actually reads the current pending outfit compliment.
    /// </summary>
    internal sealed class OtherNpcClothesReactionSystem
    {
        private sealed class PendingPrompt
        {
            public int DialogueCountBeforePush { get; set; }
            public int DialogueCountAfterPush { get; set; }
            public int OriginalFacingDirection { get; set; }
            public bool WasLookingAtPlayer { get; set; }
            // True when this reaction was armed by the walking "peeping" mechanic (the NPC noticed the
            // outfit while walking and stopped to glance). WasCaughtPeeking is true if the player also
            // caught them in the act (looked at them mid-stare). Used to flavor the AI reaction.
            public bool CameFromPeeking { get; set; }
            public bool WasCaughtPeeking { get; set; }
            public int NoticeDelayTimer { get; set; }
            public bool DialogueQueued { get; set; }
            public bool NoticePauseActive { get; set; }
            public int PendingBubbleCooldown { get; set; }
            public bool PostDialogueRestoreApplied { get; set; }
            public bool PostDialogueLingerActive { get; set; }
            public int PostDialogueLingerTimer { get; set; }

            // CurrentDialogue is a stack. If an NPC already had a daily/mod dialogue waiting,
            // temporarily move it behind the outfit compliment and restore it after the
            // outfit line is read or the reaction is cancelled. This makes the outfit
            // compliment play first without deleting the original dialogue.
            public List<Dialogue> DialogueBackupBeforeOutfit { get; set; } = new();
            public bool HasDialogueBackup { get; set; }

            // Once the player clicks the queued outfit AI generation tag, Stardew removes
            // it from CurrentDialogue before Outfit Compliments AI necessarily finishes building the
            // final line. Keep the outfit prompt override alive for a little while instead
            // of clearing it immediately, matching the spouse flow more closely.
            public bool DialogueWasConsumed { get; set; }
            public bool SawDialogueMenuAfterConsumption { get; set; }
            public bool PromptClearedAfterFirstDialogueMenu { get; set; }
            public object FirstDialogueMenu { get; set; }
            public int FirstDialogueMenuTicks { get; set; }
            public int PromptKeepAliveTimer { get; set; }

            // After the outfit dialogue closes, wait a few update ticks before restoring
            // the old NPC dialogue. Some dialogue systems clear/replace CurrentDialogue on
            // the same tick the menu closes; restoring immediately can be wiped out.
            public bool WaitingForPostDialogueRestore { get; set; }
            public int PostDialogueRestoreDelay { get; set; }
            public bool PostDialogueOutfitWasRead { get; set; }
            public bool WaitingForOwnAiFinalDialogue { get; set; }
            public bool EmoteFired { get; set; } // true after the one-shot ellipsis emote has played

            // Some non-spouse NPCs have an unread daily/first-talk dialogue that can
            // take priority over our queued outfit AI generation tag. While the
            // outfit reaction is pending, temporarily mark them as already talked to
            // so the outfit line can be the first dialogue the farmer sees. If the
            // prompt is cancelled before being read, restore the original value.
            public bool HasFriendshipEntry { get; set; }
            public bool OriginalTalkedToToday { get; set; }
            public bool ForcedTalkedToToday { get; set; }
            public NpcOutfitSpecialActionSnapshot SpecialActionSnapshot { get; set; }
        }

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

        private readonly IMonitor monitor;
        private readonly Func<ModConfig> getConfig;
        private readonly Func<NPC, bool> tryQueueOutfitDialogue;
        private readonly Func<NPC, bool> refreshOutfitPrompt;
        private readonly Action<NPC> clearOutfitPrompt;
        private readonly Func<bool> hasNoticeableCurrentFashionSenseAppearance;
        private readonly Func<NPC, bool> canNoticeCurrentOutfitNotice;
        private readonly Action<NPC> markCurrentOutfitAsNoticed;
        private readonly Func<NPC, bool> canNpcReactToOutfit;
        private readonly Func<NPC, bool> hasNpcSeenCurrentVisualBefore;
        private readonly Random random = new();

        private const int FailedRollCooldownTicks = 900;
        private const int CancelledReactionCooldownTicks = 600;
        private const float OutfitNoticePauseDistance = 96f;       // roughly one tile / adjacent interaction distance
        private const float OutfitNoticeReleaseDistance = 300f;    // stop refreshing movementPause after this
        private const float PostDialogueLingerDistance = 600f;
        private const int PostDialogueLingerTicks = 360;           // ~6 seconds
        private const float NpcSpecialActionRestoreDistance = 300f;
        private const int PendingBubbleCooldownTicks = 240;

        private readonly HashSet<string> reactedNpcsThisOutfit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PendingPrompt> pendingPrompts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> rollCooldowns = new(StringComparer.OrdinalIgnoreCase);

        // ── Peeping / glancing mechanic ──────────────────────────────────────────────
        // Tracks NPCs that are currently "peeping" at the player while walking. Only
        // movementPause is used — no controller changes, no schedule interference.
        private sealed class SpyingState
        {
            /// <summary>Direction the NPC was facing before they stopped to peek.</summary>
            public int OriginalFacingDirection { get; set; }

            /// <summary>
            /// True while the player is looking at this NPC (they caught them peeking).
            /// The NPC plays a reaction emote and "pretends" to look away.
            /// </summary>
            public bool IsBeingWatched { get; set; }

            /// <summary>
            /// How many ticks the NPC stays "pretending" to look away after being caught,
            /// before they can peek again the moment the player looks elsewhere.
            /// </summary>
            public int PretendTimer { get; set; }

            /// <summary>
            /// True once the player has caught this NPC peeking at least once (looked at them while
            /// they were staring). Used to flavor the reaction dialogue ("oh, you caught me looking…").
            /// </summary>
            public bool WasEverCaught { get; set; }

            /// <summary>
            /// Ticks remaining where the NPC walks normally before peeking again. Set after the player
            /// stops looking, so the NPC strolls for ~2s instead of immediately re-staring (which
            /// spammed back-to-back whistles/glances). While &gt; 0 the NPC is not peeking.
            /// </summary>
            public int WalkCooldownTimer { get; set; }

            /// <summary>
            /// Grace period (in ticks) after the NPC first notices the outfit before they can be
            /// "caught". Prevents the disguise emote from firing immediately when the player is
            /// already facing the NPC at the moment they notice.
            /// </summary>
            public int PeekGraceTimer { get; set; }
        }

        private readonly Dictionary<string, SpyingState> spyingNpcs = new(StringComparer.OrdinalIgnoreCase);

        // Tracks how many ticks ago each NPC was last seen actually moving. npc.isMoving() flickers
        // false for a tick at tile boundaries/turns, so we smooth it: an NPC counts as "walking" if
        // they moved within the last few ticks. A genuinely standing NPC stops moving entirely and
        // this counter climbs past the threshold, releasing them to the standing-reaction path.
        private readonly Dictionary<string, int> ticksSinceLastMoving = new(StringComparer.OrdinalIgnoreCase);
        private const int WalkingGraceTicks = 30; // ~0.5s at 60fps before a paused NPC counts as standing
        // ─────────────────────────────────────────────────────────────────────────────


        public OtherNpcClothesReactionSystem(
            IMonitor monitor,
            Func<ModConfig> getConfig,
            Func<NPC, bool> tryQueueOutfitDialogue,
            Func<NPC, bool> refreshOutfitPrompt,
            Action<NPC> clearOutfitPrompt,
            Func<bool> hasNoticeableCurrentFashionSenseAppearance,
            Func<NPC, bool> canNoticeCurrentOutfitNotice,
            Action<NPC> markCurrentOutfitAsNoticed,
            Func<NPC, bool> canNpcReactToOutfit,
            Func<NPC, bool> hasNpcSeenCurrentVisualBefore)
        {
            this.monitor = monitor;
            this.getConfig = getConfig;
            this.tryQueueOutfitDialogue = tryQueueOutfitDialogue;
            this.refreshOutfitPrompt = refreshOutfitPrompt;
            this.clearOutfitPrompt = clearOutfitPrompt;
            this.hasNoticeableCurrentFashionSenseAppearance = hasNoticeableCurrentFashionSenseAppearance;
            this.canNoticeCurrentOutfitNotice = canNoticeCurrentOutfitNotice;
            this.markCurrentOutfitAsNoticed = markCurrentOutfitAsNoticed;
            this.canNpcReactToOutfit = canNpcReactToOutfit;
            this.hasNpcSeenCurrentVisualBefore = hasNpcSeenCurrentVisualBefore;
        }

        public void Reset(bool clearPrompts = true)
        {
            if (clearPrompts)
                ClearAllPendingPrompts(removeQueuedDialogues: true);

            reactedNpcsThisOutfit.Clear();
            pendingPrompts.Clear();
            rollCooldowns.Clear();
            spyingNpcs.Clear();
            ticksSinceLastMoving.Clear();
        }

        public void NotifyOutfitChanged()
        {
            ClearAllPendingPrompts(removeQueuedDialogues: true);

            reactedNpcsThisOutfit.Clear();
            pendingPrompts.Clear();
            rollCooldowns.Clear();
            spyingNpcs.Clear();
            ticksSinceLastMoving.Clear();

            // The current saved outfit remains eligible until each NPC actually reads its
            // outfit compliment for the current notice. No short notice window is needed anymore.
        }

        public void Update(string spouseName)
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null)
                return;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                return;

            UpdateRollCooldowns();
            UpdatePendingPrompts(config);

            if (!config.EnableNpcOutfitReactions)
                return;

            // Do not let NPCs notice/react unless the player is actually wearing
            // a saved Fashion Sense outfit or an active Fashion Sense hairstyle.
            // This prevents vanilla/default clothing on a new save from triggering
            // emotes, looking, and outfit dialogue just because the NPC has hearts.
            if (hasNoticeableCurrentFashionSenseAppearance?.Invoke() != true)
                return;

            if (Game1.eventUp || Game1.activeClickableMenu != null)
                return;

            float noticeDistance = Math.Max(64f, config.OutfitNoticeDistance);
            float cancelDistance = Math.Max(noticeDistance, config.OutfitCancelDistance);

            // Tick the peeping mechanic for all NPCs currently mid-spy.
            UpdateSpyingNpcs(noticeDistance, cancelDistance);

            int newVisualChance = Math.Clamp(config.NpcOutfitReactionChance, 0, 100);
            int repeatedVisualChance = Math.Clamp(config.NpcRepeatedVisualNoticeChance, 0, 100);
            if (newVisualChance <= 0 && repeatedVisualChance <= 0)
                return;

            foreach (NPC npc in Game1.currentLocation.characters.ToList())
            {
                if (!IsValidNpc(npc, spouseName))
                    continue;

                if (pendingPrompts.ContainsKey(npc.Name))
                    continue;

                // Robust "is this NPC walking its schedule?" check. npc.isMoving() flickers false for
                // a tick at tile boundaries/turns, which previously let the standing % sneak in on a
                // walking NPC. We smooth it with a small grace window: track how long since they last
                // actually moved, and treat them as walking until they've been still for a while.
                // (NPCs mid-peek are paused on purpose and handled separately, so skip their tracking.)
                if (!spyingNpcs.ContainsKey(npc.Name))
                {
                    if (npc.isMoving())
                        ticksSinceLastMoving[npc.Name] = 0;
                    else if (ticksSinceLastMoving.TryGetValue(npc.Name, out int t))
                        ticksSinceLastMoving[npc.Name] = t + 1;
                    else
                        ticksSinceLastMoving[npc.Name] = WalkingGraceTicks + 1; // unknown = treat as standing
                }

                bool npcIsWalking = ticksSinceLastMoving.TryGetValue(npc.Name, out int sinceMoved)
                    && sinceMoved < WalkingGraceTicks;

                // ── Peeping mechanic (NPC is walking) ────────────────────────────
                // While an NPC is actively moving through their schedule and the player
                // is in range, give them a 1% per-tick chance to "notice" the outfit and
                // stop to peek. Peeking IS the "notice" for a walking NPC: it arms the same
                // pending reaction, so if the player goes over and clicks, the NPC reacts.
                // Only movementPause is used — no controller/schedule interference.
                if (npcIsWalking
                    && !spyingNpcs.ContainsKey(npc.Name)
                    && !reactedNpcsThisOutfit.Contains(npc.Name)
                    && DistanceToPlayer(npc) <= noticeDistance
                    && HasLineOfSightToPlayer(npc)
                    && random.Next(1000) < 3)   // ~0.3% per tick ≈ one notice every ~5-7s nearby
                {
                    spyingNpcs[npc.Name] = new SpyingState
                    {
                        OriginalFacingDirection = npc.FacingDirection,
                        PeekGraceTimer = 30  // ~0.5s before the player can "catch" them
                    };
                    // Snap the sprite to idle so the NPC doesn't freeze mid-stride.
                    if (npc.Sprite != null)
                    {
                        npc.Sprite.StopAnimation();
                        npc.Sprite.CurrentFrame = GetNpcIdleFrameForDirection(npc.FacingDirection);
                        npc.Sprite.UpdateSourceRect();
                    }
                    // Peeking counts as noticing: arm the pending reaction (no bubble/pause here —
                    // the peeking visuals are driven by UpdateSpyingNpcs) so a click still reacts.
                    ArmPendingReactionForSpy(npc);
                }

                // NPCs already in the spying state are handled by UpdateSpyingNpcs;
                // skip the normal standing-reaction logic while they're peeking.
                if (spyingNpcs.ContainsKey(npc.Name))
                    continue;
                // ─────────────────────────────────────────────────────────────────

                // Normal reaction only fires when the NPC is STANDING still (no movement and no
                // active walk route). Walking NPCs are handled exclusively by the peeping mechanic.
                if (npcIsWalking)
                    continue;

                // Notice the current look at most once until it changes (NotifyOutfitChanged clears
                // this) or the day resets. This prevents re-noticing the same continuous outfit over
                // and over. Whether they notice at all is decided by the new/repeated chance below.
                if (reactedNpcsThisOutfit.Contains(npc.Name))
                    continue;

                // Repeated visuals are no longer hard-blocked. Instead, an NPC that has already
                // seen the player's current look uses the (usually lower) "repeated visual" chance,
                // while a brand-new look uses the normal "new visual" chance.
                bool seenBefore = hasNpcSeenCurrentVisualBefore?.Invoke(npc) ?? false;
                int chance = seenBefore ? repeatedVisualChance : newVisualChance;
                if (chance <= 0)
                    continue;

                if (DistanceToPlayer(npc) > noticeDistance)
                    continue;

                // Only notice if the NPC is roughly facing the player AND has an unobstructed line of
                // sight — walls, closed doors, and solid tiles block the notice, so an NPC in a
                // closed room of the same location (e.g. Penny in her room in the trailer) won't
                // react even if they're within the notice distance.
                if (!HasLineOfSightToPlayer(npc))
                    continue;

                if (!IsNpcFacingPlayer(npc))
                    continue;

                if (rollCooldowns.TryGetValue(npc.Name, out int cooldown) && cooldown > 0)
                    continue;

                if (random.Next(100) >= chance)
                {
                    rollCooldowns[npc.Name] = FailedRollCooldownTicks;
                    continue;
                }

                TryStartReaction(npc, config);
            }
        }

        private bool IsValidNpc(NPC npc, string spouseName)
        {
            if (npc == null || Game1.player == null)
                return false;

            if (npc.currentLocation != Game1.player.currentLocation)
                return false;

            if (!string.IsNullOrWhiteSpace(spouseName) &&
                npc.Name.Equals(spouseName, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!npc.IsVillager)
                return false;

            if (npc.IsInvisible || npc.isSleeping.Value)
                return false;

            if (canNpcReactToOutfit?.Invoke(npc) != true)
                return false;

            return true;
        }

        private void TryStartReaction(NPC npc, ModConfig config)
        {
            if (npc == null)
                return;

            PendingPrompt pending = new PendingPrompt
            {
                OriginalFacingDirection = npc.FacingDirection,
                WasLookingAtPlayer = false,
                NoticeDelayTimer = 75,
                DialogueQueued = false,
                NoticePauseActive = false,
                PendingBubbleCooldown = 0
            };

            reactedNpcsThisOutfit.Add(npc.Name);
            pendingPrompts[npc.Name] = pending;
            // If this NPC was peeping, end the spy state cleanly — the full reaction takes over.
            spyingNpcs.Remove(npc.Name);

            ShowPendingDialogueBubbleIfNeeded(npc, pending, config, force: true);
            UpdateNpcLookState(npc, pending, config);

            // Do NOT generate/queue the AI line here. The NPC has only noticed the outfit.
            // The expensive AI call starts when the player actually clicks this NPC.
            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] {npc.Name} noticed the outfit. Outfit dialogue is pending until player click.", LogLevel.Info);
        }

        /// <summary>
        /// Arms a pending outfit reaction for an NPC that just started peeking while walking, WITHOUT
        /// the normal notice bubble or movement pause. The peeking visuals (stop, turn, glance away)
        /// are driven entirely by <see cref="UpdateSpyingNpcs"/>; this only records the pending state
        /// so that if the player walks over and clicks, the full reaction plays just like a standing
        /// notice. The reaction is marked as already-noticed for this outfit to avoid double-noticing.
        /// </summary>
        private void ArmPendingReactionForSpy(NPC npc)
        {
            if (npc == null)
                return;

            PendingPrompt pending = new PendingPrompt
            {
                OriginalFacingDirection = npc.FacingDirection,
                WasLookingAtPlayer = false,
                CameFromPeeking = true,
                NoticeDelayTimer = 0,
                DialogueQueued = false,
                NoticePauseActive = false,
                PendingBubbleCooldown = 0
            };

            reactedNpcsThisOutfit.Add(npc.Name);
            pendingPrompts[npc.Name] = pending;
            // No ShowPendingDialogueBubbleIfNeeded / UpdateNpcLookState here on purpose: while peeking,
            // the NPC's stop/turn/glance is controlled by UpdateSpyingNpcs, not the standing-notice path.
        }

        private bool TryQueuePromptAfterNotice(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return false;

            if (pending.DialogueQueued)
                return true;

            CaptureQueuedDialoguesBeforeOutfit(npc, pending);
            TemporarilySkipFirstDailyDialogue(npc, pending);

            if (tryQueueOutfitDialogue?.Invoke(npc) != true)
            {
                RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                RestoreTalkedToTodayIfUnread(npc, pending);
                rollCooldowns[npc.Name] = FailedRollCooldownTicks;
                return false;
            }

            int dialogueCountAfterPush = npc.CurrentDialogue.Count;
            if (dialogueCountAfterPush <= 0)
            {
                RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                RestoreTalkedToTodayIfUnread(npc, pending);
                rollCooldowns[npc.Name] = FailedRollCooldownTicks;
                return false;
            }

            pending.DialogueQueued = true;
            pending.DialogueCountBeforePush = Math.Max(0, dialogueCountAfterPush - 1);
            pending.DialogueCountAfterPush = dialogueCountAfterPush;
            refreshOutfitPrompt?.Invoke(npc);

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] {npc.Name}'s outfit dialogue is now ready after the notice beat.", LogLevel.Info);
            return true;
        }

        /// <summary>
        /// True if the given NPC's currently-pending outfit reaction was armed by the walking peeping
        /// mechanic AND the player caught them in the act. Lets the AI reaction add a "you caught me
        /// looking…" flavor. Returns false for normal (standing) notices or uncaught peeks.
        /// </summary>
        public bool WasNpcCaughtPeeking(NPC npc)
        {
            if (npc == null)
                return false;
            bool result = pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending)
                && pending != null
                && pending.CameFromPeeking
                && pending.WasCaughtPeeking;
            return result;
        }

        public bool TryPrioritizePendingDialogueForClick(NPC npc)
        {
            if (npc == null)
                return false;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return false;

            if (pending.DialogueWasConsumed)
                return false;

            // The player engaged this NPC — the peeking "game" is over; the full reaction takes over.
            // First carry over whether they were ever caught peeking, so the AI can flavor the line.
            if (spyingNpcs.TryGetValue(npc.Name, out SpyingState spyState) && spyState != null)
            {
                pending.WasCaughtPeeking = spyState.WasEverCaught;
            }
            spyingNpcs.Remove(npc.Name);

            if (pending.NoticeDelayTimer > 0)
                pending.NoticeDelayTimer = 0;

            ModConfig config = getConfig?.Invoke();
            bool likelyOwnAiClickWait = config != null && tryQueueOutfitDialogue != null;

            // This is called from the Harmony prefix just before Stardew handles the talk click.
            // Queue the outfit reaction only now, so AI generation never starts on the notice/emote tick.
            CaptureQueuedDialoguesBeforeOutfit(npc, pending);

            int originalDialogueCount = npc.CurrentDialogue?.Count ?? 0;

            // For built-in AI-on-click, do NOT clear CurrentDialogue or mark TalkedToToday here.
            // The final AI line will clear/push itself when ready, and this pending prompt will
            // restore the original dialogue afterwards. Clearing here made normal NPC dialogue
            // disappear while slow local models were still generating.
            if (!likelyOwnAiClickWait)
            {
                TemporarilySkipFirstDailyDialogue(npc, pending);
                npc.CurrentDialogue.Clear();
            }

            if (tryQueueOutfitDialogue?.Invoke(npc) != true)
            {
                if (!likelyOwnAiClickWait)
                    RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                RestoreTalkedToTodayIfUnread(npc, pending);
                return false;
            }

            pending.DialogueQueued = true;

            // Built-in Outfit Compliments AI starts generation on click without pushing a
            // temporary DialogueBox. In that case CurrentDialogue either stays empty or keeps
            // the original NPC dialogue stack untouched while the lower-HUD waiting message is drawn.
            if (likelyOwnAiClickWait && (npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= originalDialogueCount))
            {
                pending.DialogueWasConsumed = true;
                pending.WaitingForOwnAiFinalDialogue = true;
                pending.PromptKeepAliveTimer = Math.Max(pending.PromptKeepAliveTimer, 900);
                pending.PostDialogueOutfitWasRead = false;
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Started built-in AI outfit generation for {npc.Name} at click time; kept original dialogue stack untouched until final line is ready.", LogLevel.Info);
                return true;
            }

            if (npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= 0)
            {
                pending.DialogueWasConsumed = true;
                pending.WaitingForOwnAiFinalDialogue = true;
                pending.PromptKeepAliveTimer = Math.Max(pending.PromptKeepAliveTimer, 900);
                pending.PostDialogueOutfitWasRead = false;
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Started built-in AI outfit generation for {npc.Name} at click time; no temporary dialogue box was queued.", LogLevel.Info);
                return true;
            }

            pending.DialogueCountBeforePush = Math.Max(0, npc.CurrentDialogue.Count - 1);
            pending.DialogueCountAfterPush = npc.CurrentDialogue.Count;
            refreshOutfitPrompt?.Invoke(npc);

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Queued outfit dialogue for {npc.Name} at click time.", LogLevel.Info);
            return true;
        }

        private void UpdateRollCooldowns()
        {
            if (rollCooldowns.Count <= 0)
                return;

            foreach (string npcName in rollCooldowns.Keys.ToList())
            {
                rollCooldowns[npcName]--;

                if (rollCooldowns[npcName] <= 0)
                    rollCooldowns.Remove(npcName);
            }
        }

        private void UpdatePendingPrompts(ModConfig config)
        {
            if (pendingPrompts.Count <= 0)
                return;

            float cancelDistance = Math.Max(
                Math.Max(64f, config.OutfitNoticeDistance) + 64f,
                config.OutfitCancelDistance
            );

            foreach (string npcName in pendingPrompts.Keys.ToList())
            {
                PendingPrompt pending = pendingPrompts[npcName];
                NPC npc = Game1.getCharacterFromName(npcName);

                if (npc == null)
                {
                    pendingPrompts.Remove(npcName);
                    continue;
                }

                if (pending.PendingBubbleCooldown > 0)
                    pending.PendingBubbleCooldown--;

                if (pending.WaitingForPostDialogueRestore)
                {
                    UpdatePostDialogueRestore(npc, pending);

                    if (!pending.WaitingForPostDialogueRestore)
                        pendingPrompts.Remove(npcName);

                    continue;
                }

                // The NPC should visibly notice the outfit first (emote/look), then the
                // outfit dialogue becomes available. This prevents the click-priority patch
                // from feeling like an instant/direct dialogue with no reaction beat.
                if (!pending.DialogueQueued)
                {
                    if (Game1.player == null || npc.currentLocation != Game1.player.currentLocation || DistanceToPlayer(npc) > cancelDistance)
                    {
                        CancelPendingPrompt(npc, pending);
                        pendingPrompts.Remove(npcName);
                        continue;
                    }

                    UpdateNpcLookState(npc, pending, config);
                    ShowPendingDialogueBubbleIfNeeded(npc, pending, config);

                    if (pending.NoticeDelayTimer > 0)
                    {
                        pending.NoticeDelayTimer--;
                        continue;
                    }

                    // The outfit reaction is now intentionally queued on click, not here.
                    // Keep the NPC looking/interested while the pending click window is active.
                    continue;
                }

                if (pending.WaitingForOwnAiFinalDialogue)
                {
                    // The visible waiting line was opened, but the background AI result is not ready yet.
                    // Do not restore the old dialogue stack until ModEntry opens the final generated line.
                    UpdateNpcLookState(npc, pending, config);
                    ShowPendingDialogueBubbleIfNeeded(npc, pending, config);
                    continue;
                }

                // If the queued outfit AI generation tag was consumed, do NOT clear the
                // prompt override immediately. Outfit Compliments AI may still be turning that tag into
                // the final dialogue line, and clearing here can make it fall back to a casual
                // conversation. This mirrors the spouse flow, where the prompt is only cleared
                // after the dialogue is done.
                if (pending.DialogueWasConsumed || npc.CurrentDialogue.Count < pending.DialogueCountAfterPush)
                {
                    KeepConsumedDialoguePromptAlive(npc, pending);

                    if (ShouldClearConsumedPrompt(pending))
                    {
                        clearOutfitPrompt?.Invoke(npc);
                        SchedulePostDialogueRestore(pending, outfitWasRead: pending.SawDialogueMenuAfterConsumption);
                    }

                    continue;
                }

                // If the farmer moved too far away or changed maps, cancel this pending reaction.
                if (Game1.player == null || npc.currentLocation != Game1.player.currentLocation || DistanceToPlayer(npc) > cancelDistance)
                {
                    CancelPendingPrompt(npc, pending);
                    pendingPrompts.Remove(npcName);
                    continue;
                }

                // Keep the outfit-specific Outfit Compliments AI prompt alive until the player reads it.
                // Some Outfit Compliments AI flows rebuild their prompt right when the dialogue starts, so
                // registering only when the emote appears can fall back to a casual conversation.
                refreshOutfitPrompt?.Invoke(npc);

                UpdateNpcLookState(npc, pending, config);
                ShowPendingDialogueBubbleIfNeeded(npc, pending, config);
            }
        }


        private void KeepConsumedDialoguePromptAlive(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return;

            if (!pending.DialogueWasConsumed)
            {
                pending.DialogueWasConsumed = true;
                pending.PromptKeepAliveTimer = 900; // fallback while Outfit Compliments AI turns the generation tag into the first line

                if (pending.WasLookingAtPlayer)
                    FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);

                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] {npc.Name}'s outfit prompt was consumed; keeping Outfit Compliments AI override alive until the first real reply/input step.", LogLevel.Info);
            }

            if (pending.PromptKeepAliveTimer > 0)
                pending.PromptKeepAliveTimer--;

            object currentMenu = Game1.activeClickableMenu;

            if (currentMenu != null)
            {
                pending.SawDialogueMenuAfterConsumption = true;

                if (pending.FirstDialogueMenu == null)
                {
                    pending.FirstDialogueMenu = currentMenu;
                    pending.FirstDialogueMenuTicks = 0;
                }
                else if (ReferenceEquals(pending.FirstDialogueMenu, currentMenu))
                {
                    pending.FirstDialogueMenuTicks++;
                }
                else if (!pending.PromptClearedAfterFirstDialogueMenu && LooksLikeReplyOrInputMenu(currentMenu))
                {
                    // Do NOT clear just because the menu object changed. Outfit Compliments AI can swap
                    // from its generation tag dialogue box to the final generated dialogue box,
                    // and clearing during that handoff makes the outfit compliment disappear.
                    // Only clear once Outfit Compliments AI has moved to a reply/input style menu, so the
                    // player's typed response is no longer forced to use the outfit-notice prompt.
                    clearOutfitPrompt?.Invoke(npc);
                    pending.PromptClearedAfterFirstDialogueMenu = true;
                    if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Cleared outfit prompt for {npc.Name} when Outfit Compliments AI opened a reply/input menu.", LogLevel.Info);
                    return;
                }
                else
                {
                    // Outfit Compliments AI changed from one non-reply dialogue menu to another. Treat
                    // the new one as the current first dialogue menu so the close-timing guard
                    // below can distinguish a real read from a short generation handoff.
                    pending.FirstDialogueMenu = currentMenu;
                    pending.FirstDialogueMenuTicks = 0;
                }

                if (!pending.PromptClearedAfterFirstDialogueMenu)
                    refreshOutfitPrompt?.Invoke(npc);

                return;
            }

            if (pending.SawDialogueMenuAfterConsumption && !pending.PromptClearedAfterFirstDialogueMenu)
            {
                // Outfit Compliments AI may briefly close the generation-tag menu before opening the
                // actual generated outfit line. If the first menu was only visible for a few
                // ticks, keep the prompt alive instead of clearing too early.
                if (pending.FirstDialogueMenuTicks < 15 && pending.PromptKeepAliveTimer > 0)
                {
                    refreshOutfitPrompt?.Invoke(npc);
                    return;
                }

                clearOutfitPrompt?.Invoke(npc);
                pending.PromptClearedAfterFirstDialogueMenu = true;
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Cleared outfit prompt for {npc.Name} after the first outfit dialogue menu closed.", LogLevel.Info);
                return;
            }

            // Before the dialogue menu appears, Outfit Compliments AI may still be generating/replacing
            // the tag. Keep refreshing during that handoff window, never after the first prompt
            // has been cleared for replies.
            if (!pending.PromptClearedAfterFirstDialogueMenu)
                refreshOutfitPrompt?.Invoke(npc);
        }

        private static bool LooksLikeReplyOrInputMenu(object menu)
        {
            if (menu == null)
                return false;

            string typeName = menu.GetType().Name ?? "";

            // Vanilla's normal spoken lines use DialogueBox. Keep the outfit prompt alive
            // for those, because Outfit Compliments AI may still be converting the generation tag.
            if (typeName.Equals("DialogueBox", StringComparison.OrdinalIgnoreCase))
                return false;

            string[] replyMarkers =
            {
                "Response", "Reply", "Input", "Text", "TextBox", "Keyboard",
                "Question", "Answer", "Choice"
            };

            return replyMarkers.Any(marker => typeName.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool ShouldClearConsumedPrompt(PendingPrompt pending)
        {
            if (pending == null || !pending.DialogueWasConsumed)
                return false;

            // Once the first outfit menu has closed and the prompt was cleared, the prompt did its job.
            if (pending.SawDialogueMenuAfterConsumption && pending.PromptClearedAfterFirstDialogueMenu && Game1.activeClickableMenu == null)
                return true;

            // Safety fallback, so a prompt cannot stay forever if something consumes the tag
            // without opening/changing a visible dialogue menu.
            if (pending.PromptKeepAliveTimer <= 0)
            {
                pending.PromptClearedAfterFirstDialogueMenu = true;
                return true;
            }

            return false;
        }

        private void SchedulePostDialogueRestore(PendingPrompt pending, bool outfitWasRead)
        {
            if (pending == null || pending.WaitingForPostDialogueRestore)
                return;

            pending.WaitingForPostDialogueRestore = true;
            pending.PostDialogueRestoreDelay = 8;
            pending.PostDialogueOutfitWasRead = outfitWasRead;
        }

        public void NotifyPrioritizedDialogueOpenedByHarmony(NPC npc)
        {
            if (npc == null)
                return;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return;

            // The Harmony prefix opened the outfit dialogue directly and skipped the
            // original NPC.checkAction. In that path, CurrentDialogue.Count isn't a
            // reliable signal for when the outfit line was consumed, so mark it here
            // and let the normal delayed restore run after the dialogue box closes.
            pending.DialogueWasConsumed = true;
            pending.PromptKeepAliveTimer = Math.Max(pending.PromptKeepAliveTimer, 300);

            // Do not mark SawDialogueMenuAfterConsumption here. Game1.drawDialogue can open
            // a short-lived generation-tag menu before Outfit Compliments AI shows the final generated
            // line. If we mark the menu as seen too early, the next empty-menu tick can clear
            // the prompt before Outfit Compliments AI uses it. Let KeepConsumedDialoguePromptAlive mark
            // the first real menu when it actually sees Game1.activeClickableMenu.

            pending.PostDialogueOutfitWasRead = true;

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Harmony opened {npc.Name}'s outfit dialogue; waiting until the first Outfit Compliments AI menu is done before restoring their previous dialogue.", LogLevel.Info);
        }

        public void NotifyOwnAiWaitingDialogueOpened(NPC npc)
        {
            if (npc == null)
                return;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return;

            pending.DialogueWasConsumed = true;
            pending.WaitingForOwnAiFinalDialogue = true;
            pending.PromptKeepAliveTimer = Math.Max(pending.PromptKeepAliveTimer, 900);
            pending.PostDialogueOutfitWasRead = false;

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Opened AI waiting dialogue for {npc.Name}; keeping outfit reaction pending until final AI line is ready.", LogLevel.Info);
        }

        public void NotifyOwnAiFinalDialogueOpened(NPC npc)
        {
            if (npc == null)
                return;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return;

            pending.WaitingForOwnAiFinalDialogue = false;
            pending.DialogueWasConsumed = true;
            pending.PromptKeepAliveTimer = Math.Max(pending.PromptKeepAliveTimer, 300);
            pending.PostDialogueOutfitWasRead = true;

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Opened final AI outfit dialogue for {npc.Name}; it will be marked read after the menu closes.", LogLevel.Info);
        }

        public void NotifyOwnAiFinalDialogueFailed(NPC npc)
        {
            if (npc == null)
                return;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return;

            // The AI generation failed/timed out, so no dialogue was actually shown. Restore the
            // NPC's normal dialogue WITHOUT marking the outfit as read, and re-arm this pending
            // reaction so the farmer can click the NPC again to retry generating it. We deliberately
            // do NOT schedule the post-dialogue restore here, because that path removes the pending
            // prompt entirely (ending the reaction). Keeping the prompt alive but un-consumed is what
            // lets the click retry work, matching the spouse "keep pending after AI failure" behavior.
            try
            {
                RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                RestoreTalkedToTodayIfUnread(npc, pending);
                clearOutfitPrompt?.Invoke(npc);
            }
            catch (System.Exception ex)
            {
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Could not restore dialogue after failed AI for {npc.Name}: {ex.Message}", LogLevel.Info);
            }

            pending.WaitingForOwnAiFinalDialogue = false;
            pending.DialogueWasConsumed = false;
            pending.DialogueQueued = false;
            pending.SawDialogueMenuAfterConsumption = false;
            pending.PromptClearedAfterFirstDialogueMenu = false;
            pending.PostDialogueOutfitWasRead = false;
            pending.DialogueCountAfterPush = 0;
            // Brief cooldown so the ellipsis/notice bubble does not immediately re-fire every tick,
            // but the reaction stays available for a manual click retry until the farmer walks away.
            pending.PendingBubbleCooldown = Math.Max(pending.PendingBubbleCooldown, 180);

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] AI outfit dialogue failed for {npc.Name}; restored previous dialogue (outfit NOT marked read) and reopened it for a click retry.", LogLevel.Info);
        }


        private void UpdatePostDialogueRestore(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return;

            if (pending.PostDialogueLingerActive)
            {
                UpdatePostDialogueLinger(npc, pending);
                return;
            }

            if (pending.PostDialogueRestoreApplied)
            {
                pending.WaitingForPostDialogueRestore = false;
                return;
            }

            // Do not restore while a dialogue/menu is still active. If we restore on the
            // same frame the dialogue closes, Stardew/Outfit Compliments AI can still clear the stack.
            if (Game1.activeClickableMenu != null)
                return;

            if (pending.PostDialogueRestoreDelay > 0)
            {
                pending.PostDialogueRestoreDelay--;
                return;
            }

            bool beginLinger = false;

            try
            {
                clearOutfitPrompt?.Invoke(npc);

                if (pending.PostDialogueOutfitWasRead)
                    markCurrentOutfitAsNoticed?.Invoke(npc);

                RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                RestoreTalkedToTodayIfUnread(npc, pending);

                pending.PostDialogueRestoreApplied = true;

                beginLinger = pending.PostDialogueOutfitWasRead
                    && Game1.player != null
                    && npc.currentLocation == Game1.player.currentLocation;

                if (beginLinger)
                {
                    pending.PostDialogueLingerActive = true;
                    pending.PostDialogueLingerTimer = PostDialogueLingerTicks;
                    UpdatePostDialogueLinger(npc, pending);
                    if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] {npc.Name} will linger after the outfit compliment until distance >= {PostDialogueLingerDistance:F0} or {PostDialogueLingerTicks} ticks.", LogLevel.Info);
                    return;
                }

                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Finished delayed restore after outfit dialogue for {npc.Name}.", LogLevel.Info);
            }
            finally
            {
                if (!beginLinger)
                    pending.WaitingForPostDialogueRestore = false;
            }
        }

        private void UpdateNpcLookState(NPC npc, PendingPrompt pending, ModConfig config)
        {
            if (npc == null || pending == null || Game1.player == null)
                return;

            if (npc.currentLocation != Game1.player.currentLocation)
            {
                pending.NoticePauseActive = false;
                return;
            }

            float distance = DistanceToPlayer(npc);

            // Kiss-style notice pause: don't delete or replace npc.controller. Let the NPC keep
            // walking normally, but if they naturally get adjacent to the farmer while the outfit
            // compliment is pending, hold them briefly with movementPause and make them look at her.
            if (distance <= OutfitNoticePauseDistance)
                pending.NoticePauseActive = true;
            else if (distance >= OutfitNoticeReleaseDistance)
                pending.NoticePauseActive = false;

            if (pending.NoticePauseActive)
            {
                CaptureNpcSpecialActionBeforeOutfit(npc, pending);

                if (npc.movementPause < 6)
                    npc.movementPause = 6;

                npc.Sprite?.StopAnimation();
                if (FacePlayerIfSafe(npc))
                    pending.WasLookingAtPlayer = true;

                return;
            }

            // Once released, don't keep forcing their facing direction. Their schedule/controller
            // should be free to choose the walking direction again.
            pending.WasLookingAtPlayer = false;
        }

        private void ShowPendingDialogueBubbleIfNeeded(NPC npc, PendingPrompt pending, ModConfig config, bool force = false)
        {
            if (npc == null || pending == null || config == null || Game1.player == null)
                return;

            if (npc.currentLocation != Game1.player.currentLocation)
                return;

            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            float noticeDistance = Math.Max(64f, config.OutfitNoticeDistance);
            if (DistanceToPlayer(npc) > noticeDistance)
                return;

            if (!force && pending.PendingBubbleCooldown > 0)
                return;

            // Fire the ellipsis emote only ONCE per outfit notice — re-fires if the player
            // leaves and comes back (EmoteFired is reset when the pending prompt is cancelled).
            if (!pending.EmoteFired)
            {
                npc.doEmote(40);
                pending.EmoteFired = true;
            }
            pending.PendingBubbleCooldown = force ? 180 : PendingBubbleCooldownTicks;
        }

        /// <summary>
        /// True if ANY non-spouse NPC currently has an outfit reaction in progress that has not yet
        /// been fully read/closed (noticing, generating, or dialogue still open). Used to let other
        /// mods (e.g. the kiss mod) know an outfit reaction is active so they can hold off.
        /// </summary>
        public bool HasAnyActivePendingReaction()
        {
            foreach (PendingPrompt pending in pendingPrompts.Values)
            {
                if (pending == null)
                    continue;
                // Once the line has actually been read/opened and we're just in the delayed
                // restore/linger cleanup, the reaction no longer needs to block other interactions.
                if (pending.PostDialogueOutfitWasRead || pending.PostDialogueRestoreApplied || pending.PostDialogueLingerActive)
                    continue;
                // A pending that has not yet been triggered by a player click (DialogueQueued == false)
                // means an NPC merely noticed the outfit but the player hasn't approached them yet.
                // That passive state should NOT block cross-mod interactions like kisses — only
                // reactions actively in progress (AI generating, dialogue open, etc.) should.
                if (!pending.DialogueQueued)
                    continue;
                return true;
            }
            return false;
        }


        public bool HasUnreadPendingDialogueFor(NPC npc)
        {
            if (npc == null)
                return false;

            if (!pendingPrompts.TryGetValue(npc.Name, out PendingPrompt pending) || pending == null)
                return false;

            // After the outfit line has been opened/read, the player can use normal
            // interactions again while the delayed restore/linger cleanup finishes.
            if (pending.PostDialogueOutfitWasRead || pending.PostDialogueRestoreApplied || pending.PostDialogueLingerActive)
                return false;

            return true;
        }

        public IEnumerable<NPC> GetPendingDialogueIndicatorNpcs()
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null)
                yield break;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                yield break;

            float noticeDistance = Math.Max(64f, config.OutfitNoticeDistance);

            foreach (string npcName in pendingPrompts.Keys.ToList())
            {
                if (!pendingPrompts.TryGetValue(npcName, out PendingPrompt pending) || pending == null)
                    continue;

                if (pending.WaitingForPostDialogueRestore || pending.PostDialogueLingerActive || pending.WaitingForOwnAiFinalDialogue)
                    continue;

                if (pending.DialogueWasConsumed)
                    continue;

                NPC npc = Game1.getCharacterFromName(npcName);
                if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                    continue;

                if (DistanceToPlayer(npc) > noticeDistance)
                    continue;

                yield return npc;
            }
        }

        private void UpdatePostDialogueLinger(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return;

            bool sameLocation = Game1.player != null && npc.currentLocation == Game1.player.currentLocation;
            float distance = sameLocation ? DistanceToPlayer(npc) : PostDialogueLingerDistance;
            bool hasCapturedSpecialAction = pending.SpecialActionSnapshot != null;

            if (pending.PostDialogueLingerTimer > 0)
                pending.PostDialogueLingerTimer--;

            bool shouldFinish = hasCapturedSpecialAction
                ? (!sameLocation || distance >= NpcSpecialActionRestoreDistance)
                : (!sameLocation || distance >= PostDialogueLingerDistance || pending.PostDialogueLingerTimer <= 0);

            if (!shouldFinish)
            {
                CaptureNpcSpecialActionBeforeOutfit(npc, pending);

                if (npc.movementPause < 6)
                    npc.movementPause = 6;

                npc.Sprite?.StopAnimation();
                FacePlayerIfSafe(npc);
                return;
            }

            bool restoredSpecialAction = TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true);
            if (!restoredSpecialAction)
                npc.movementPause = 0;

            pending.PostDialogueLingerActive = false;
            pending.WaitingForPostDialogueRestore = false;
        }

        private void CaptureQueuedDialoguesBeforeOutfit(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return;

            try
            {
                // Stack<T>.ToList() enumerates from top to bottom. Store that order,
                // then restore by pushing bottom-to-top later so the original top
                // dialogue remains the next one the player sees.
                pending.DialogueBackupBeforeOutfit = npc.CurrentDialogue?.ToList() ?? new List<Dialogue>();
                pending.HasDialogueBackup = pending.DialogueBackupBeforeOutfit.Count > 0;
            }
            catch (Exception ex)
            {
                pending.DialogueBackupBeforeOutfit = new List<Dialogue>();
                pending.HasDialogueBackup = false;
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Could not capture existing dialogue for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }

        private void RestoreQueuedDialoguesAfterOutfit(NPC npc, PendingPrompt pending, bool clearCurrentDialogue)
        {
            if (npc == null || pending == null)
                return;

            if (!pending.HasDialogueBackup || pending.DialogueBackupBeforeOutfit == null || pending.DialogueBackupBeforeOutfit.Count <= 0)
            {
                if (clearCurrentDialogue)
                    npc.CurrentDialogue.Clear();
                return;
            }

            try
            {
                if (clearCurrentDialogue)
                    npc.CurrentDialogue.Clear();

                for (int i = pending.DialogueBackupBeforeOutfit.Count - 1; i >= 0; i--)
                    npc.CurrentDialogue.Push(pending.DialogueBackupBeforeOutfit[i]);

                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Restored {pending.DialogueBackupBeforeOutfit.Count} previous dialogue(s) for {npc.Name} after outfit reaction.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Could not restore previous dialogue for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }

        private void TemporarilySkipFirstDailyDialogue(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null || Game1.player == null)
                return;

            try
            {
                if (!Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship) || friendship == null)
                    return;

                pending.HasFriendshipEntry = true;
                pending.OriginalTalkedToToday = friendship.TalkedToToday;

                if (!friendship.TalkedToToday)
                {
                    friendship.TalkedToToday = true;
                    pending.ForcedTalkedToToday = true;
                    if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Temporarily skipped first daily dialogue for {npc.Name} so the outfit reaction can play first.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Could not temporarily skip first daily dialogue for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }

        private void RestoreTalkedToTodayIfUnread(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null || Game1.player == null)
                return;

            if (!pending.HasFriendshipEntry || !pending.ForcedTalkedToToday)
                return;

            try
            {
                if (Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship) && friendship != null)
                    friendship.TalkedToToday = pending.OriginalTalkedToToday;
            }
            catch (Exception ex)
            {
                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Could not restore first daily dialogue state for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }

        private void CancelPendingPrompt(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null)
                return;

            if (pending.DialogueQueued)
            {
                RemoveQueuedDialogueIfStillPending(npc, pending);
                RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                clearOutfitPrompt?.Invoke(npc);
                RestoreTalkedToTodayIfUnread(npc, pending);
            }

            rollCooldowns[npc.Name] = CancelledReactionCooldownTicks;

            bool restoredSpecialAction = TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true);

            if (!restoredSpecialAction && pending.WasLookingAtPlayer)
                FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);
        }

        private void ClearAllPendingPrompts(bool removeQueuedDialogues)
        {
            foreach (var pair in pendingPrompts.ToList())
            {
                string npcName = pair.Key;
                PendingPrompt pending = pair.Value;
                NPC npc = Game1.getCharacterFromName(npcName);
                if (npc == null)
                    continue;

                if (removeQueuedDialogues && pending.DialogueQueued)
                {
                    RemoveQueuedDialogueIfStillPending(npc, pending);
                    RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
                }

                if (pending.DialogueQueued)
                {
                    clearOutfitPrompt?.Invoke(npc);
                    RestoreTalkedToTodayIfUnread(npc, pending);
                }

                bool restoredSpecialAction = TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true);

                if (!restoredSpecialAction && pending.WasLookingAtPlayer)
                    FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);
            }

            pendingPrompts.Clear();
        }

        private void RemoveQueuedDialogueIfStillPending(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null || !pending.DialogueQueued)
                return;

            // Our Outfit Compliments AI dialogue is pushed last. Only pop it if the stack still has exactly the size
            // it had right after we pushed, so we don't accidentally remove another mod's dialogue.
            if (npc.CurrentDialogue.Count == pending.DialogueCountAfterPush &&
                pending.DialogueCountAfterPush == pending.DialogueCountBeforePush + 1)
            {
                npc.CurrentDialogue.Pop();
            }
        }

        private int TryGetAnimationFrameIndex(FarmerSprite.AnimationFrame frame)
        {
            try
            {
                object boxed = frame;
                FieldInfo field = boxed.GetType().GetField("frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.GetValue(boxed) is int fieldValue)
                    return fieldValue;

                PropertyInfo property = boxed.GetType().GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null && property.GetValue(boxed) is int propertyValue)
                    return propertyValue;
            }
            catch
            {
                // Internal animation frame details are optional; CurrentFrame still covers the common cases.
            }

            return -1;
        }

        private bool AnimationLooksLikeSpecialAction(List<FarmerSprite.AnimationFrame> animation)
        {
            if (animation == null || animation.Count <= 0)
                return false;

            foreach (FarmerSprite.AnimationFrame frame in animation)
            {
                int frameIndex = TryGetAnimationFrameIndex(frame);
                if (frameIndex >= 16)
                    return true;
            }

            return false;
        }

        private void CaptureNpcSpecialActionBeforeOutfit(NPC npc, PendingPrompt pending)
        {
            if (npc == null || pending == null || npc.Sprite == null || npc.currentLocation == null)
                return;

            if (pending.SpecialActionSnapshot != null)
                return;

            // Walking schedules should keep their controller/route. Only save scene-style animations.
            if (npc.isMoving())
                return;

            List<FarmerSprite.AnimationFrame> animation = null;
            if (npc.Sprite.CurrentAnimation != null && npc.Sprite.CurrentAnimation.Count > 0)
                animation = new List<FarmerSprite.AnimationFrame>(npc.Sprite.CurrentAnimation);

            bool hasSpecialAnimation = animation != null && animation.Count > 0;
            bool hasSpecialStaticFrame = npc.Sprite.CurrentFrame >= 16;

            if (!hasSpecialAnimation && !hasSpecialStaticFrame)
                return;

            pending.SpecialActionSnapshot = new NpcOutfitSpecialActionSnapshot
            {
                Npc = npc,
                Location = npc.currentLocation,
                FacingDirection = npc.FacingDirection,
                CurrentFrame = npc.Sprite.CurrentFrame,
                Flip = npc.flip,
                MovementPause = (int)npc.movementPause,
                AddedSpeed = (int)npc.addedSpeed,
                CurrentAnimation = animation
            };

            npc.Sprite.StopAnimation();
            npc.Sprite.ClearAnimation();
            npc.Sprite.CurrentAnimation = null;
            npc.flip = false;
            npc.Sprite.CurrentFrame = GetNpcIdleFrameForDirection(npc.FacingDirection);
            npc.Sprite.UpdateSourceRect();

            if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Saved special animation for {npc.Name} before outfit reaction. frame={pending.SpecialActionSnapshot.CurrentFrame} anim={(animation != null ? animation.Count : 0)}", LogLevel.Info);
        }

        private bool TryRestoreNpcSpecialActionAfterOutfit(NPC npc, PendingPrompt pending, bool force = false)
        {
            if (pending == null || pending.SpecialActionSnapshot == null)
                return false;

            NpcOutfitSpecialActionSnapshot snapshot = pending.SpecialActionSnapshot;
            npc ??= snapshot.Npc;

            if (npc == null || npc.Sprite == null || npc.currentLocation == null)
            {
                pending.SpecialActionSnapshot = null;
                return false;
            }

            if (npc != snapshot.Npc || npc.currentLocation != snapshot.Location)
            {
                pending.SpecialActionSnapshot = null;
                return false;
            }

            if (!force)
            {
                if (Game1.activeClickableMenu != null || Game1.dialogueUp)
                    return false;

                if (Game1.player != null && npc.currentLocation == Game1.player.currentLocation && DistanceToPlayer(npc) < NpcSpecialActionRestoreDistance)
                    return false;
            }

            try
            {
                npc.FacingDirection = snapshot.FacingDirection;
                npc.flip = snapshot.Flip;
                npc.movementPause = snapshot.MovementPause;
                npc.addedSpeed = snapshot.AddedSpeed;

                if (snapshot.CurrentAnimation != null && snapshot.CurrentAnimation.Count > 0)
                {
                    npc.Sprite.CurrentAnimation = new List<FarmerSprite.AnimationFrame>(snapshot.CurrentAnimation);
                    TrySetSpritePrivateField(npc.Sprite, "currentAnimationIndex", 0);
                    TrySetSpritePrivateField(npc.Sprite, "timer", 0);
                }
                else
                {
                    npc.Sprite.StopAnimation();
                    npc.Sprite.ClearAnimation();
                    npc.Sprite.CurrentAnimation = null;
                }

                npc.Sprite.CurrentFrame = snapshot.CurrentFrame;
                npc.Sprite.UpdateSourceRect();

                if (ModEntry.DebugLog) monitor?.Log($"[NPC OUTFIT] Restored special animation for {npc.Name} after outfit reaction. frame={snapshot.CurrentFrame} anim={(snapshot.CurrentAnimation != null ? snapshot.CurrentAnimation.Count : 0)}", LogLevel.Info);

                pending.SpecialActionSnapshot = null;
                return true;
            }
            catch (Exception ex)
            {
                monitor?.Log($"[NPC OUTFIT] Could not restore special animation for {npc?.Name ?? "null"}: {ex.Message}", LogLevel.Warn);
                pending.SpecialActionSnapshot = null;
                return false;
            }
        }

        private void TrySetSpritePrivateField(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(fieldName))
                return;

            try
            {
                FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                    field.SetValue(target, value);
            }
            catch
            {
                // Optional internal field.
            }
        }

        private int GetNpcIdleFrameForDirection(int facingDirection)
        {
            switch (facingDirection)
            {
                case 0: return 8;
                case 1: return 4;
                case 2: return 0;
                case 3: return 12;
                default: return 0;
            }
        }

        // ── Peeping / glancing mechanic ──────────────────────────────────────────────

        private void UpdateSpyingNpcs(float noticeDistance, float cancelDistance)
        {
            if (Game1.player == null)
                return;

            // Tick down each spying NPC.
            foreach (string name in spyingNpcs.Keys.ToList())
            {
                NPC npc = Game1.currentLocation?.characters
                    .FirstOrDefault(c => c?.Name != null && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                // Remove if NPC left the location or walked out of cancel range.
                if (npc == null || npc.currentLocation != Game1.player.currentLocation
                    || DistanceToPlayer(npc) > cancelDistance)
                {
                    spyingNpcs.Remove(name);
                    continue;
                }

                SpyingState state = spyingNpcs[name];

                // Walk cooldown: after the player looks away, the NPC strolls normally for ~2s before
                // peeking again, instead of snapping back to a stare immediately. Let them walk freely
                // (no movementPause) and just count down; only re-peek once the timer expires.
                if (state.WalkCooldownTimer > 0)
                {
                    state.WalkCooldownTimer--;
                    continue;
                }

                // Cooldown finished. Only resume peeking if the NPC is still within notice range and
                // still walking; otherwise keep strolling and re-check after another short interval.
                if (DistanceToPlayer(npc) > noticeDistance)
                {
                    state.WalkCooldownTimer = 60; // re-check in ~1s; they may walk back into range
                    continue;
                }

                if (state.IsBeingWatched)
                {
                    // Caught! The NPC is "pretending" nothing happened — they resume walking their
                    // schedule normally. Do NOT renew movementPause here: we want them moving again.
                    // We just wait until the player looks away, then start the walk cooldown.

                    if (state.PretendTimer > 0)
                    {
                        state.PretendTimer--;
                        continue;
                    }

                    // Brief pretend timer expired — now check if player is STILL watching.
                    if (IsPlayerFacingNpc(npc))
                    {
                        // Still being watched; keep pretending (walking normally) a bit longer.
                        state.PretendTimer = 12;
                        continue;
                    }

                    // Player looked away — walk normally for ~2s before considering peeking again.
                    state.IsBeingWatched = false;
                    state.WalkCooldownTimer = 120; // ~2 seconds at 60fps
                    continue;
                }

                // ── Active peeking ────────────────────────────────────────────────
                // Renew movementPause every tick (safe: never kills schedule/controller).
                if (npc.movementPause < 6)
                    npc.movementPause = 6;

                // Face the player. Do this every tick regardless of isMoving(): with movementPause
                // active the NPC can still report isMoving()==true (residual velocity while paused),
                // which previously skipped the turn entirely and left them frozen mid-stride.
                npc.faceGeneralDirection(Game1.player.getStandingPosition(), 0, false, false);

                // Re-apply idle frame every tick: faceGeneralDirection (and residual movement
                // velocity) can reset the sprite to a walk frame, so we force the standing pose.
                if (npc.Sprite != null)
                {
                    npc.Sprite.StopAnimation();
                    npc.Sprite.CurrentFrame = GetNpcIdleFrameForDirection(npc.FacingDirection);
                    npc.Sprite.UpdateSourceRect();
                }

                // Brief grace period after first noticing — prevents the disguise emote from
                // firing immediately if the player was already facing the NPC when they noticed.
                if (state.PeekGraceTimer > 0)
                {
                    state.PeekGraceTimer--;
                    continue;
                }

                // If player looks at the NPC, they get "caught".
                if (IsPlayerFacingNpc(npc))
                {
                    state.IsBeingWatched = true;
                    state.WasEverCaught = true;
                    state.PretendTimer = 15;

                    // Persist "caught" onto the pending reaction RIGHT NOW, so even if the spy state is
                    // later cleared (NPC leaves range, etc.) before the player clicks, the reaction
                    // still knows they were caught and can open with the "you saw me looking…" line.
                    // If the pending was cleared in the meantime (e.g. a notice refresh), re-arm it so
                    // the "caught" flavor is never lost — this affected NPCs whose pending got wiped.
                    if (!pendingPrompts.TryGetValue(name, out PendingPrompt caughtPending) || caughtPending == null)
                    {
                        ArmPendingReactionForSpy(npc);
                        pendingPrompts.TryGetValue(name, out caughtPending);
                    }
                    if (caughtPending != null)
                        caughtPending.WasCaughtPeeking = true;

                    // Randomly react with one of two emotes (50/50).
                    if (random.Next(2) == 0)
                        npc.doEmote(28);
                    else
                        npc.doEmote(16);

                    // Release movementPause so NPC immediately resumes walking.
                    npc.movementPause = 0;
                }
            }
        }

        /// <summary>
        /// True when the player is roughly facing toward this NPC — i.e. the player
        /// would "see" the NPC when they look in their current facing direction.
        /// </summary>
        private static bool IsPlayerFacingNpc(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return false;

            Vector2 playerPos = Game1.player.getStandingPosition();
            Vector2 npcPos    = npc.getStandingPosition();
            Vector2 delta     = npcPos - playerPos;

            if (delta.LengthSquared() < 16f * 16f)
                return true;

            return Game1.player.FacingDirection switch
            {
                0 => delta.Y < 0,   // player facing up,   NPC is above
                1 => delta.X > 0,   // player facing right, NPC is to the right
                2 => delta.Y > 0,   // player facing down,  NPC is below
                3 => delta.X < 0,   // player facing left,  NPC is to the left
                _ => true
            };
        }

        // ─────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// True when there is an unobstructed tile path between the NPC and the player — i.e. no
        /// solid wall, door, or impassable tile blocks the line between them. Uses a simple
        /// tile-by-tile raycast along the line connecting the two positions. This prevents NPCs in
        /// closed-off rooms (same GameLocation but behind a wall/door) from noticing the player.
        /// </summary>
        private static bool HasLineOfSightToPlayer(NPC npc)
        {
            if (npc == null || Game1.player == null || npc.currentLocation == null)
                return false;

            GameLocation location = npc.currentLocation;
            Vector2 npcTile    = new Vector2((int)(npc.Position.X / Game1.tileSize), (int)(npc.Position.Y / Game1.tileSize));
            Vector2 playerTile = new Vector2((int)(Game1.player.Position.X / Game1.tileSize), (int)(Game1.player.Position.Y / Game1.tileSize));

            // If they're on the same tile or adjacent, always visible.
            float dx = playerTile.X - npcTile.X;
            float dy = playerTile.Y - npcTile.Y;
            int steps = (int)Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps <= 1)
                return true;

            // Walk tile-by-tile along the line and check for solid obstacles.
            for (int i = 1; i < steps; i++)
            {
                float t = (float)i / steps;
                int tileX = (int)Math.Round(npcTile.X + dx * t);
                int tileY = (int)Math.Round(npcTile.Y + dy * t);

                try
                {
                    // isTilePassable returns false for walls, solid objects, and closed doors.
                    xTile.Dimensions.Location tileLoc = new(tileX, tileY);
                    if (!location.isTilePassable(tileLoc, Game1.viewport))
                        return false;
                }
                catch
                {
                    // Out-of-bounds or unexpected tile — treat as blocked.
                    return false;
                }
            }

            return true;
        }

        private static bool IsNpcFacingPlayer(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return false;

            Vector2 npcPos    = npc.getStandingPosition();
            Vector2 playerPos = Game1.player.getStandingPosition();
            Vector2 delta     = playerPos - npcPos;

            if (delta.LengthSquared() < 16f * 16f)
                return true;

            return npc.FacingDirection switch
            {
                0 => delta.Y < 0,
                1 => delta.X > 0,
                2 => delta.Y > 0,
                3 => delta.X < 0,
                _ => true
            };
        }

        private float DistanceToPlayer(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return float.MaxValue;

            return Vector2.Distance(npc.Position, Game1.player.Position);
        }

        private bool FacePlayerIfSafe(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return false;

            if (npc.currentLocation != Game1.player.currentLocation)
                return false;

            // Don't stop schedules or clear controllers; while movementPause is holding a
            // pending outfit notice, the controller may still exist even though the NPC is idle.
            if (npc.isMoving())
                return false;

            npc.faceGeneralDirection(Game1.player.getStandingPosition(), 0, false, false);
            return true;
        }

        private bool FaceDirectionIfSafe(NPC npc, int direction)
        {
            if (npc == null)
                return false;

            if (npc.isMoving())
                return false;

            npc.faceDirection(direction);
            return true;
        }
    }
}
