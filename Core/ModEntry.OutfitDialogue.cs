using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OutfitReactions.Ai;

namespace OutfitReactions
{
    public sealed partial class ModEntry
    {
        // ── Outfit dialogue — AI queue, player reply, NPC interaction, theme/location helpers ──

        internal bool TryOpenPrioritizedOutfitDialogueFromCheckAction(NPC npc)
        {
            if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
                return false;

            if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                return false;

            if (Game1.eventUp)
                return false;

            bool ownAiWaitingState = IsOwnAiWaitingStateActiveFor(npc);

            // If Outfit Compliments already started generating the prioritized line for this NPC,
            // keep swallowing talk clicks until the generated line is ready. This prevents any
            // vanilla/other-mod dialogue that is already sitting in CurrentDialogue from opening
            // before the outfit compliment.
            if (!ownAiWaitingState && !PrioritizeOutfitDialogueBeforeNpcCheckAction(npc))
                return false;

            ownAiWaitingState = IsOwnAiWaitingStateActiveFor(npc);

            if ((npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= 0) && !ownAiWaitingState)
                return false;

            try
            {
                npc.faceGeneralDirection(Game1.player.getStandingPosition());
                Game1.player.Halt();

                // Built-in AI is the priority dialogue. While it is generating, do NOT open
                // any existing CurrentDialogue entry. The old dialogue stack stays backed up/
                // untouched and is restored only after the outfit compliment is read or fails.
                if (ownAiWaitingState)
                {
                    if (DebugLog) Monitor.Log($"[CLOTHES PRIORITY] Holding {npc.Name}'s normal dialogue behind the prioritized outfit AI wait.", LogLevel.Info);
                    return true;
                }

                // We open the outfit dialogue ourselves and skip vanilla NPC.checkAction.
                // If vanilla/another mod keeps running after we queue the outfit line, it can
                // create the NPC's first daily dialogue at the same click and put it in front.
                // Skipping the original method only for this pending outfit reaction guarantees
                // the compliment is read first, while our backup/restore code keeps the old line
                // available for the next click.
                Game1.drawDialogue(npc);

                otherNpcClothesReactionSystem?.NotifyPrioritizedDialogueOpenedByHarmony(npc);

                if (DebugLog) Monitor.Log($"[CLOTHES PRIORITY] Opened prioritized outfit dialogue for {npc.Name} and skipped original NPC.checkAction.", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"[CLOTHES PRIORITY] Failed to open prioritized outfit dialogue for {npc.Name}: {ex}", LogLevel.Warn);
                return false;
            }
        }

        private bool ShouldBlockNpcInteractionUntilOutfitDialogueRead(NPC npc)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
                return false;

            if (IsOwnAiWaitingStateActiveFor(npc))
                return true;

            if (IsUnreadSpouseOutfitDialoguePendingFor(npc))
                return true;

            return otherNpcClothesReactionSystem?.HasUnreadPendingDialogueFor(npc) == true;
        }

        private bool IsUnreadSpouseOutfitDialoguePendingFor(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return false;

            if (!IsPlayerSpouse(npc))
                return false;

            if (lastFashionSenseChangeInfo == null)
                return false;

            // Once the current notice has been marked read, normal interactions
            // should be allowed again. Before that, any active spouse outfit notice counts
            // as unread, including the first emote beat before the dialogue becomes ready.
            if (!CanNpcNoticeCurrentOutfitNotice(npc))
                return false;

            bool sameReactingNpc = clothesReactingNpc != null
                && npc.Name.Equals(clothesReactingNpc.Name, StringComparison.OrdinalIgnoreCase);

            if (sameReactingNpc && (outfitSequenceActive || isReactingToClothes))
                return true;

            if (outfitSequenceActive && clothesFirstNoticeDone)
                return true;

            return false;
        }

        private bool IsPlayerSpouse(NPC npc)
        {
            return npc != null
                && Game1.player != null
                && !string.IsNullOrWhiteSpace(Game1.player.spouse)
                && npc.Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowPendingOutfitBlockedInteractionFeedback(NPC npc)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
                return;

            Game1.player.Halt();

            try
            {
                if (!npc.isMoving() && npc.controller == null)
                    npc.faceGeneralDirection(Game1.player.getStandingPosition(), 0, false, false);
            }
            catch
            {
                // visual feedback only; never let this break the interaction blocker
            }

            if (IsPlayerSpouse(npc))
            {
                ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
                UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
            }
            else
            {
                npc.doEmote(40);
            }
        }

        private static bool IsNpcRomanceable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Stardew's character data knows which NPCs can be romanced — vanilla AND modded.
            // This is the reliable source so we don't need to hardcode a list per mod.
            try
            {
                if (Game1.characterData != null
                    && Game1.characterData.TryGetValue(name, out var data)
                    && data != null)
                    return data.CanBeRomanced;
            }
            catch { }

            return false;
        }

        private sealed class FashionSenseSnapshot
        {
            public string Hair;
            public string Accessory;
            public string AccessorySecondary;
            public string AccessoryTertiary;
            public string Hat;
            public bool FashionSenseHatCoversVanilla;
            public string VanillaHat;
            public List<string> VanillaHatSpecialItemCandidates = new();
            public string VanillaPants;
            public List<string> VanillaPantsSpecialItemCandidates = new();
            public string Shirt;
            public string Pants;
            public string Sleeves;
            public string Shoes;
            public string OutfitId;
            public string HairColor;
            public string AccessoryColor;
            public string AccessorySecondaryColor;
            public string AccessoryTertiaryColor;
            public string HatColor;
            public string ShirtColor;
            public string PantsColor;
            public string SleevesColor;
            public string ShoesColor;
        }

        private bool IsFashionSenseMenu(IClickableMenu menu)
        {
            string name = menu?.GetType().FullName ?? "";
            return name.Contains("FashionSense", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeOutfitText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            string normalized = text
                .ToLowerInvariant()
                .Trim()
                .Normalize(NormalizationForm.FormD);

            StringBuilder builder = new();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string GetFashionSenseDialogueKey(FashionSenseChangeInfo changeInfo)
        {
            if (changeInfo == null)
                return null;

            int total = changeInfo.CountChanges();
            if (total <= 0)
                return null;

            // Special wearable items from assets/special-reactions/luckypurpleshorts.json (e.g. the
            // Mayor's purple shorts) can be the primary notice even while a saved outfit is active.
            if (TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out _))
                return "Clothes";

            if ((changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId))
                || ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
            {
                return "Clothes";
            }

            // Accessory/hat are noticed when EITHER vision is on (it can see the shape) OR the
            // item's name reveals its shape (so a text AI can describe it meaningfully).
            bool visionOn = AreVisionOnlyFashionSenseTriggersEnabled();

            if (changeInfo.ChangedAccessory
                && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, visionOn))
                return "Accessory";

            if (changeInfo.VanillaHatChanged)
                return "Hat";

            if (changeInfo.ChangedHat
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId)
                && (visionOn || ItemNameRevealsShape(changeInfo.NewHatId)))
                return "Hat";

            if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
                return "Hair";

            // Do not react to vanilla/default clothes or unsaved Fashion Sense body pieces.
            return null;
        }

        private bool AreVisionOnlyFashionSenseTriggersEnabled()
        {
            // Vision is now automatic: enabled whenever the active model supports image input.
            return ShouldTryVisionForCurrentAiProvider();
        }

        private bool ItemNameRevealsShape(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            if (IsIgnoredFashionSenseAccessoryId(itemId))
                return false;

            if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(itemId))
                return false;

            string name = FashionSenseVisualService.HumanizeAppearanceId(itemId);
            if (string.IsNullOrWhiteSpace(name))
                return false;

            foreach (string raw in name.Split(' '))
            {
                string token = raw.Trim('\'', '"', '.', ',', '(', ')');
                if (token.Length < 3)
                    continue;

                bool hasDigit = false;
                bool hasLetter = false;
                foreach (char ch in token)
                {
                    if (char.IsDigit(ch)) hasDigit = true;
                    else if (char.IsLetter(ch)) hasLetter = true;
                }

                // A real descriptive word: has letters and is not a code (no digits mixed in).
                if (hasLetter && !hasDigit)
                    return true;
            }

            return false;
        }

        private bool IsFarmHouseLocation(GameLocation location)
        {
            if (location == null)
                return false;

            string name = location.Name ?? "";
            string uniqueName = location.NameOrUniqueName ?? "";
            string typeName = location.GetType().Name ?? "";

            string fullTypeName = location.GetType().FullName ?? "";

            return name.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || uniqueName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || typeName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)
                || name.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0
                || uniqueName.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0
                || fullTypeName.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsBeachOrIslandLocation(GameLocation location)
        {
            if (location == null)
                return false;

            string name = location.Name ?? "";
            string uniqueName = location.NameOrUniqueName ?? "";

            return name.Equals("Beach", StringComparison.OrdinalIgnoreCase)
                || uniqueName.Equals("Beach", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Island", StringComparison.OrdinalIgnoreCase)
                || uniqueName.StartsWith("Island", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsMarriageCandidateNpcRoom(NPC npc, GameLocation location)
        {
            if (npc == null || location == null)
                return false;

            if (!IsMarriageCandidate(npc))
                return false;

            string npcName = NormalizeOutfitText(npc.Name);
            string displayName = NormalizeOutfitText(npc.displayName);
            string locationText = NormalizeOutfitText((location.Name ?? "") + " " + (location.NameOrUniqueName ?? ""));

            if (LooksLikeNpcRoomText(locationText) && TextMentionsNpc(locationText, npcName, displayName))
                return true;

            return MapPropertiesSuggestNpcRoom(location, npcName, displayName);
        }

        private bool IsMarriageCandidatePersonalLocation(NPC npc, GameLocation location)
        {
            if (npc == null || location == null || !IsMarriageCandidate(npc))
                return false;

            if (location.IsOutdoors || IsFarmHouseLocation(location))
                return false;

            string npcName = NormalizeOutfitText(npc.Name);
            string displayName = NormalizeOutfitText(npc.displayName);
            string locationText = NormalizeOutfitText((location.Name ?? "") + " " + (location.NameOrUniqueName ?? ""));

            // Direct cases like SamHouse, HaleyHouse, LeahHouse, ElliottHouse,
            // SebastianBasement, etc. This catches many vanilla and modded homes/rooms.
            if (TextMentionsNpc(locationText, npcName, displayName))
                return true;

            // Vanilla marriage candidates whose personal/home maps do not always include
            // the NPC's name in the map name.
            Dictionary<string, string[]> vanillaHomes = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Abigail"] = new[] { "seedshop", "pierres", "pierre" },
                ["Alex"] = new[] { "joshhouse", "alexhouse" },
                ["Elliott"] = new[] { "elliotthouse", "elliottcabin" },
                ["Emily"] = new[] { "haleyhouse", "emilyhouse" },
                ["Haley"] = new[] { "haleyhouse" },
                ["Harvey"] = new[] { "harveyroom", "harveyclinic", "hospital" },
                ["Leah"] = new[] { "leahhouse", "leahcottage" },
                ["Maru"] = new[] { "sciencehouse", "robinhouse" },
                ["Penny"] = new[] { "trailer" },
                ["Sam"] = new[] { "samhouse" },
                ["Sebastian"] = new[] { "sciencehouse", "sebastianbasement", "sebastianroom", "robinhouse" },
                ["Shane"] = new[] { "animalshop", "marnieranch", "ranch" }
            };

            if (vanillaHomes.TryGetValue(npc.Name ?? "", out string[] homes))
            {
                foreach (string home in homes)
                {
                    if (!string.IsNullOrWhiteSpace(home) && locationText.Contains(NormalizeOutfitText(home)))
                        return true;
                }
            }

            return false;
        }

        private bool IsMarriageCandidate(NPC npc)
        {
            if (npc == null)
                return false;

            try
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                object datableObj = npc.GetType().GetField("datable", flags)?.GetValue(npc)
                    ?? npc.GetType().GetProperty("datable", flags)?.GetValue(npc);

                if (datableObj == null)
                    return false;

                object value = datableObj.GetType().GetProperty("Value", flags)?.GetValue(datableObj);
                return value is bool canDate && canDate;
            }
            catch
            {
                return false;
            }
        }

        private bool MapPropertiesSuggestNpcRoom(GameLocation location, string npcName, string displayName)
        {
            try
            {
                if (location?.map?.Properties == null)
                    return false;

                foreach (object rawProperty in location.map.Properties)
                {
                    string rawKey = "";
                    string rawValue = "";

                    if (rawProperty is DictionaryEntry entry)
                    {
                        rawKey = entry.Key?.ToString() ?? "";
                        rawValue = entry.Value?.ToString() ?? "";
                    }
                    else if (rawProperty != null)
                    {
                        Type propertyType = rawProperty.GetType();
                        rawKey = propertyType.GetProperty("Key")?.GetValue(rawProperty)?.ToString() ?? "";
                        rawValue = propertyType.GetProperty("Value")?.GetValue(rawProperty)?.ToString() ?? "";
                    }

                    string key = NormalizeOutfitText(rawKey);
                    string value = NormalizeOutfitText(rawValue);
                    string combined = key + " " + value;

                    if (LooksLikeNpcRoomText(combined) && TextMentionsNpc(combined, npcName, displayName))
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private bool LooksLikeNpcRoomText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.Contains("room")
                || text.Contains("bedroom")
                || text.Contains("bed room")
                || text.Contains("npcroom")
                || text.Contains("npc room")
                || text.Contains("quarto")
                || text.Contains("suite")
                || text.Contains("basement")
                || text.Contains("cellar")
                || text.Contains("porão")
                || text.Contains("porao");
        }

        private bool TextMentionsNpc(string text, string npcName, string displayName)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return (!string.IsNullOrWhiteSpace(npcName) && text.Contains(npcName))
                || (!string.IsNullOrWhiteSpace(displayName) && text.Contains(displayName));
        }

        private bool TryShowOwnAiOutfitDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
        {
            // Outfit AI starts on the outfit-reaction click and has priority over any normal
            // NPC dialogue already in the stack. Normal dialogue is restored only after the
            // outfit compliment is read or generation fails.
            return TryQueueOwnAiWaitingDialogue(npc, isSpouseDialogue, clearExistingDialogue);
        }

        private bool CanUseOwnAiForOutfitDialogue(NPC npc)
        {
            if (outfitAiService == null || npc == null || lastFashionSenseChangeInfo == null)
                return false;

            return outfitAiService.HasProfile(npc.Name);
        }

        private bool ShouldUseDeferredOwnAiForNpc(NPC npc)
        {
            return CanUseOwnAiForOutfitDialogue(npc);
        }

        private bool TryQueueOwnAiWaitingDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
        {
            if (!CanUseOwnAiForOutfitDialogue(npc))
                return false;

            OutfitAiContext context = BuildOutfitAiContext(npc, isSpouseDialogue);
            if (context == null)
                return false;

            // This handler runs on the game thread. Prime the optional voice-reference cache here
            // so the Task.Run request below never calls GameContent.Load from a worker thread.
            outfitAiService.PrepareVoiceSamplesForNpc(npc.Name);

            if (clearExistingDialogue)
                npc.CurrentDialogue.Clear();

            // Do not push a temporary DialogueBox here. The AI request starts on click,
            // but the waiting state is shown as a persistent HUD message so Escape/click
            // cannot close it before the generated compliment is ready.
            Game1.activeClickableMenu = null;
            Game1.afterDialogues = null;

            if (!aiGenerationCoordinator.TryGetOutfit(npc.Name, out PendingAiGeneration pending) || pending == null || pending.Task == null || pending.Task.IsCompleted)
            {
                pending = new PendingAiGeneration
                {
                    NpcName = npc.Name,
                    IsSpouseDialogue = isSpouseDialogue,
                    ClearExistingDialogue = clearExistingDialogue,
                    WaitingDotCount = 1,
                    WaitingDotTimer = 30,
                    SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
                    Cancellation = new CancellationTokenSource()
                };

                aiGenerationCoordinator.StartOutfit(pending, cancellationToken =>
                {
                    try
                    {
                        return outfitAiService.TryGenerateCompliment(context, out string aiLine, cancellationToken) ? aiLine : null;
                    }
                    catch (OperationCanceledException)
                    {
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log(" Background outfit generation crashed: " + ex.Message, LogLevel.Warn);
                        return null;
                    }
                });
                if (DebugLog) Monitor.Log($" Started background outfit compliment generation for {npc.Name}. HUD waiting message is active.", LogLevel.Info);
            }
            else
            {
                Monitor.Log($" {npc.Name} already has a background outfit compliment generation in progress.", LogLevel.Trace);
            }

            return true;
        }

        private int GetActiveAiTimeoutSecondsForSafety()
        {
            string provider = Config?.GetActiveProvider() ?? "DeepSeek";
            int seconds = provider switch
            {
                "Gemini" => Config.GeminiAiTimeoutSeconds,
                "OpenAI" => Config.OpenAiAiTimeoutSeconds,
                "OpenRouter" => Config.OpenRouterAiTimeoutSeconds,
                "Mistral" => Config.MistralAiTimeoutSeconds,
                "Groq" => Config.GroqAiTimeoutSeconds,
                "Together" => Config.TogetherAiTimeoutSeconds,
                "Local" => Config.LocalAiTimeoutSeconds,
                _ => Config.DeepSeekAiTimeoutSeconds
            };
            return Math.Clamp(seconds, 3, 120);
        }

        private bool IsOwnAiWaitingStateActiveFor(NPC npc)
        {
            return npc != null
                && aiGenerationCoordinator.TryGetOutfit(npc.Name, out PendingAiGeneration pending)
                && pending != null
                && pending.Task != null
                && !pending.Task.IsCompleted;
        }

        private string GetOwnAiWaitingDialogueText(NPC npc, int dotCount)
        {
            int dots = Math.Clamp(dotCount, 1, 3);
            string suffix = new string('.', dots);
            string npcName = !string.IsNullOrWhiteSpace(npc?.displayName) ? npc.displayName : (npc?.Name ?? "NPC");

            return Helper.Translation.Get("hud.npc-noticing", new { name = npcName }).ToString() + suffix;
        }

        private string GetOwnAiReplyWaitingDialogueText(NPC npc, int dotCount)
        {
            int dots = Math.Clamp(dotCount, 1, 3);
            string suffix = new string('.', dots);
            string npcName = !string.IsNullOrWhiteSpace(npc?.displayName) ? npc.displayName : (npc?.Name ?? "NPC");

            return Helper.Translation.Get("hud.npc-thinking", new { name = npcName }).ToString() + suffix;
        }

        private void DrawOwnAiWaitingHudMessage(SpriteBatch spriteBatch, NPC npc, string text)
        {
            if (spriteBatch == null || npc == null || string.IsNullOrWhiteSpace(text) || Game1.smallFont == null)
                return;

            Vector2 textSize = Game1.smallFont.MeasureString(text);
            Vector2 position = new Vector2(32f, Math.Max(32f, Game1.uiViewport.Height - textSize.Y - 72f));

            Rectangle background = new Rectangle(
                (int)position.X - 16,
                (int)position.Y - 10,
                (int)textSize.X + 32,
                (int)textSize.Y + 20
            );

            spriteBatch.Draw(Game1.staminaRect, background, Color.Black * 0.55f);
            spriteBatch.DrawString(Game1.smallFont, text, position + new Vector2(2f, 2f), Color.Black * 0.75f);
            spriteBatch.DrawString(Game1.smallFont, text, position, Color.White);
        }

        private void UpdatePendingOwnAiGenerations()
        {
            if (!aiGenerationCoordinator.HasOutfitGenerations)
                return;

            foreach (string npcName in aiGenerationCoordinator.GetOutfitNpcNames())
            {
                if (!aiGenerationCoordinator.TryGetOutfit(npcName, out PendingAiGeneration pending))
                    continue;
                NPC npc = Game1.getCharacterFromName(npcName);

                if (pending == null || npc == null || pending.Task == null)
                {
                    aiGenerationCoordinator.RemoveOutfit(npcName);
                    continue;
                }

                AiGenerationLifecycleState lifecycleState = AiDialogueLifecycle.Advance(pending);
                if (lifecycleState == AiGenerationLifecycleState.Completed)
                {
                    if (!pending.CompletionHandled)
                    {
                        pending.CompletionHandled = true;
                        string generated = null;

                        try
                        {
                            if (pending.Task.Status == TaskStatus.RanToCompletion)
                                generated = pending.Task.Result;
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log(" Could not read background AI result: " + ex.Message, LogLevel.Warn);
                        }

                        OpenGeneratedOrFallbackOutfitDialogue(npc, pending, generated);
                    }

                    aiGenerationCoordinator.RemoveOutfit(npcName);
                    continue;
                }

                if (lifecycleState == AiGenerationLifecycleState.TimedOut)
                {
                    Monitor.Log($" Background generation for {npcName} exceeded the safety timer. Removing pending waiting state.", LogLevel.Warn);
                    AiRequestLifecycle.Cancel(pending.Cancellation);

                    if (pending.IsSpouseDialogue && npc != null)
                    {
                        ResetClothesState(clearChangeFlag: false);
                        aiGenerationCoordinator.RemoveOutfit(npcName);
                    }
                    else if (npc != null)
                    {
                        otherNpcClothesReactionSystem?.CancelPendingOwnAiGeneration(npc);
                        aiGenerationCoordinator.RemoveOutfit(npcName);
                    }
                    else
                    {
                        aiGenerationCoordinator.RemoveOutfit(npcName);
                    }

                    continue;
                }

                UpdateOwnAiWaitingVisual(npc, pending);
            }
        }

        private void UpdateOwnAiWaitingVisual(NPC npc, PendingAiGeneration pending)
        {
            if (npc == null || pending == null || Game1.player == null)
                return;

            if (npc.currentLocation != Game1.player.currentLocation)
                return;

            if (pending.WaitingDotTimer > 0)
            {
                pending.WaitingDotTimer--;
                return;
            }

            pending.WaitingDotTimer = 30;
            pending.WaitingDotCount++;
            if (pending.WaitingDotCount > 3)
                pending.WaitingDotCount = 1;

            // The waiting text is drawn in the lower HUD by OnRenderedHud.
            // Do not use showTextAboveHead here; it can interfere with the final dialogue handoff.
        }

        private void OpenGeneratedOrFallbackOutfitDialogue(NPC npc, PendingAiGeneration pending, string generated)
        {
            if (npc == null || pending == null)
                return;

            if (Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                Monitor.Log($" AI outfit compliment for {pending.NpcName} finished, but the player is no longer nearby. Discarding it.", LogLevel.Trace);
                return;
            }

            bool queued = false;
            bool needsAccessoryClarification = false;
            string finalNpcLineForReply = null;
            if (!string.IsNullOrWhiteSpace(generated))
            {
                if (generated.StartsWith(OutfitAiService.AccessoryClarificationMarker, StringComparison.Ordinal))
                {
                    needsAccessoryClarification = true;
                    generated = generated.Substring(OutfitAiService.AccessoryClarificationMarker.Length).Trim();
                }

                npc.CurrentDialogue.Clear();
                string dialogueId = pending.IsSpouseDialogue
                    ? "OutfitReactions_SpouseOwnAiOutfitReaction"
                    : "OutfitReactions_GlobalOwnAiOutfitReaction";
                npc.CurrentDialogue.Push(new Dialogue(npc, dialogueId, generated));
                finalNpcLineForReply = generated;
                queued = true;
                if (DebugLog) Monitor.Log($" Background outfit compliment for {npc.Name} is ready and queued.", LogLevel.Info);
            }
            else
            {
                Monitor.Log($" Background outfit generation did not produce a usable line for {npc.Name}. Trying configured fallbacks.", LogLevel.Warn);
                queued = TryQueueNonAiOutfitFallback(npc, pending.IsSpouseDialogue, clearExistingDialogue: true);
            }

            if (!queued || npc.CurrentDialogue.Count <= 0)
            {
                Game1.activeClickableMenu = null;

                if (pending.IsSpouseDialogue)
                {
                    // If the provider fails or returns unusable text, do not mark the outfit as read.
                    // Keep the current spouse notice pending so it does not spam ellipses while the
                    // player remains nearby, but can be retried on click or after cancel/re-approach.
                    KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "background AI generation did not produce a usable line.");
                }
                else
                {
                    otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueFailed(npc);
                }

                return;
            }

            Action postOutfitCleanup = null;
            if (pending.IsSpouseDialogue)
                postOutfitCleanup = () => CompleteSpouseAfterOutfitDialogue(npc);

            // Do not show any reply menu when the NPC is reacting to a special item removal —
            // the reaction is self-contained and there is nothing meaningful to reply to.
            bool isSpecialItemRemoval = false;
            {
                FashionSenseChangeInfo removalCheck = GetEffectiveFashionSenseChangeInfoForNpc(npc);
                if (TryResolveSpecialItemNoticeForNpc(npc, removalCheck, requireNpcMemoryForRemoval: true, out SpecialItemNoticeInfo removalNotice)
                    && removalNotice?.WasRemoved == true)
                    isSpecialItemRemoval = true;
            }

            // Check if the current special item has an unrevealed secret for this NPC.
            // Lewis and Marnie know by default, so they skip this menu and go to the normal reply flow.
            bool shouldShowSecretMenu = false;
            string pendingSecretId = null;
            if (!isSpecialItemRemoval
                && !string.IsNullOrWhiteSpace(finalNpcLineForReply)
                && !needsAccessoryClarification
                && Config.EnablePlayerReplyMenuAfterOutfitCompliment
                && specialItemReactionService != null)
            {
                FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);
                if (TryResolveSpecialItemNoticeForNpc(npc, effectiveChangeInfo, requireNpcMemoryForRemoval: false, out SpecialItemNoticeInfo secretNotice)
                    && secretNotice != null
                    && secretNotice.HasSecret
                    && !secretNotice.WasRemoved
                    && !specialItemReactionService.NpcAlreadyKnowsSecret(secretNotice.SecretId, npc.Name))
                {
                    shouldShowSecretMenu = true;
                    pendingSecretId = secretNotice.SecretId;
                }
            }

            if (!string.IsNullOrWhiteSpace(finalNpcLineForReply) && needsAccessoryClarification && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
                InstallAccessoryClarificationInputAfterOutfitDialogue(npc, pending.IsSpouseDialogue, finalNpcLineForReply, postOutfitCleanup);
            else if (shouldShowSecretMenu)
                InstallSecretRevealChoiceMenu(npc, pending.IsSpouseDialogue, finalNpcLineForReply, pendingSecretId, postOutfitCleanup);
            else if (!isSpecialItemRemoval && !string.IsNullOrWhiteSpace(finalNpcLineForReply) && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
                InstallPlayerReplyMenuAfterOutfitDialogue(npc, pending.IsSpouseDialogue, finalNpcLineForReply, postOutfitCleanup);
            else if (pending.IsSpouseDialogue)
                InstallSpouseAfterOutfitDialogue(npc);

            if (!pending.IsSpouseDialogue)
                otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueOpened(npc);

            // Register that this NPC just reacted, so the notice/memory systems update. For spouse
            // dialogue this happens in CompleteSpouseAfterOutfitDialogue; for everyone else it must
            // happen here, otherwise non-spouse NPCs (e.g. Pam) never record what they saw and the
            // vanilla-hat memory (and outfit notice) never persists.
            if (!pending.IsSpouseDialogue)
                MarkCurrentOutfitAsNoticed(npc);

            Game1.activeClickableMenu = null;
            npc.faceGeneralDirection(Game1.player.getStandingPosition());
            Game1.drawDialogue(npc);
        }

        private bool TryQueueNonAiOutfitFallback(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
        {
            // AI-only build: manual JSON fallback is disabled.
            // If AI generation fails, no manual Clothes/<language>/<NPC>.json line is queued.
            // Manual fallback is disabled, so do not clear the NPC's current/backup dialogue here.
            // The caller decides how to restore or keep the outfit notice pending.
            if (npc != null)
                Monitor.Log($" No AI outfit dialogue was queued for {npc.Name}. Manual JSON outfit dialogue is disabled in this AI-only build.", LogLevel.Warn);

            return false;
        }

        private void InstallSpouseAfterOutfitDialogue(NPC npc)
        {
            Game1.afterDialogues = () => CompleteSpouseAfterOutfitDialogue(npc);
        }

        private void CompleteSpouseAfterOutfitDialogue(NPC npc)
        {
            MarkCurrentOutfitAsNoticed(npc);
            ClearOutfitPrompt(npc);

            bool sameLocation = npc != null && Game1.player != null && npc.currentLocation == Game1.player.currentLocation;

            if (sameLocation)
            {
                // Do NOT call StopNpcForClothesReaction here. After the dialogue, we
                // only want the kiss-mod-style linger: pause with movementPause and look
                // at the farmer. Deleting the controller here can break outdoor schedules.
                CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
                npc.Sprite?.StopAnimation();
                npc.faceGeneralDirection(Game1.player.getStandingPosition());
                DoClothesFinalEmotes(npc);

                // If the spouse was interrupted inside the farmhouse and we captured a
                // simple local route, restore it before pausing. Outside the farmhouse
                // there should be no backup because we never stopped the controller.
                if (spouseRouteController.HasRoute)
                    spouseRouteController.Restore(npc, Monitor, DebugLog);
                else
                    ClearSpouseControllerBackup();
            }
            else
            {
                ClearSpouseControllerBackup();
            }

            spouseDialogueController.Restore(npc, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, monitor: Monitor, debugLog: DebugLog);
            // clearChangeFlag is intentionally false here. changedClothes/lastFashionSenseChangeInfo
            // are shared global state that HasNoticeableCurrentFashionSenseAppearance() (and therefore
            // every OTHER npc's ability to notice this same outfit) also reads. Sebastian's own
            // dedup against re-reacting to this outfit is already handled separately via
            // HasNpcReactedToCurrentOutfitNotice/npcsReactedToCurrentNotice (see MarkCurrentOutfitAsNoticed
            // above), so clearing the shared flag here isn't needed for his own correctness — it was
            // only wiping out the notice for every NPC who hadn't seen the player yet.
            ResetClothesState(false);

            if (sameLocation)
                BeginSpousePostOutfitLinger(npc);
        }

        private void BeginSpousePostOutfitLinger(NPC npc)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                ClearSpousePostOutfitLinger();
                return;
            }

            SpousePostOutfitLingerController.Begin(spouseProximityState, npc);

            CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);

            SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, npc, Game1.player);

            if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] {npc.Name} will linger after the outfit compliment until distance >= {SpouseProximityState.PostOutfitLingerDistance:F0} or {SpouseProximityState.PostOutfitLingerDelayTicks} ticks.", LogLevel.Info);
        }

        private void UpdateSpousePostOutfitLinger()
        {
            if (!spouseProximityState.LingerActive)
                return;

            NPC npc = spouseProximityState.LingerNpc;
            if (npc == null || Game1.player == null || !Context.IsWorldReady)
            {
                ClearSpousePostOutfitLinger();
                return;
            }

            bool sameLocation = npc.currentLocation == Game1.player.currentLocation;
            float distance = sameLocation ? DistanceToPlayer(npc) : SpouseProximityState.PostOutfitLingerDistance;
            bool hasCapturedSpecialAction = spouseSpecialActionController.HasSnapshotFor(npc);

            bool shouldResume = SpousePostOutfitLingerController.TickAndShouldResume(
                spouseProximityState,
                sameLocation,
                distance,
                hasCapturedSpecialAction,
                OutfitSpecialActionRestoreDistance);

            if (!shouldResume)
            {
                SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, npc, Game1.player);
                return;
            }

            bool restoredSpecialAction = TryRestoreSpouseOutfitSpecialAction(force: true);
            if (!restoredSpecialAction)
                npc.movementPause = 0;

            ClearSpousePostOutfitLinger();
        }

        private void ClearSpousePostOutfitLinger()
        {
            SpousePostOutfitLingerController.Clear(spouseProximityState);
        }

        private void InstallPlayerReplyMenuAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
        {
            outfitReplyConversationHistory.Start(npc?.Name, npcCompliment);
            Game1.afterDialogues = () => ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
        }

        private void InstallSecretRevealChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, string secretId, Action onFinished)
        {
            Game1.afterDialogues = () =>
            {
                if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
                {
                    FinishPlayerReplyInteraction(onFinished, npc?.Name);
                    return;
                }

                bool isPt = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt;

                string npcDisplayName = npc.displayName ?? npc.Name;
                string title = isPt
                    ? $"Contar o segredo a {npcDisplayName}?"
                    : $"Tell {npcDisplayName} the secret?";
                string yesLabel = isPt ? "Contar" : "Tell them";
                string noLabel  = isPt ? "Não" : "Not now";

                Game1.activeClickableMenu = new OutfitPlayerReplyChoiceMenu(
                    title,
                    yesLabel,
                    noLabel,
                    respond: () =>
                    {
                        // Reveal the secret and rebuild context so the follow-up already knows.
                        specialItemReactionService?.RevealSecret(secretId, npc.Name);
                        OutfitAiContext revealContext = BuildOutfitAiContext(npc, isSpouseDialogue);
                        if (revealContext == null)
                        {
                            FinishPlayerReplyInteraction(onFinished, npc?.Name);
                            return;
                        }
                        // Use the item's configured reveal message as the "player reply" so the
                        // follow-up prompt knows exactly what was just revealed to the NPC.
                        // Fall back to a generic message if none is configured.
                        string configuredReveal = specialItemReactionService?.GetSecretRevealMessage(secretId) ?? "";
                        string revealMsg = !string.IsNullOrWhiteSpace(configuredReveal)
                            ? configuredReveal
                            : (isPt
                                ? "[O jogador contou ao NPC sobre a origem secreta do item.]"
                                : "[The player just told the NPC about the item's secret origin.]");
                        revealContext.ConversationTranscript = null;
                        StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, revealMsg, onFinished, revealContext);
                    },
                    leave: () => FinishPlayerReplyInteraction(onFinished, npc?.Name)
                );
            };
        }

        private void InstallAccessoryClarificationInputAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcLine, Action onFinished)
        {
            Game1.afterDialogues = () =>
            {
                string title = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                    ? "Responder:"
                    : "Reply:";

                OpenPlayerOutfitReplyInputMenu(
                    npc,
                    isSpouseDialogue,
                    npcLine,
                    onFinished,
                    titleOverride: title,
                    cancelOverride: () => FinishPlayerReplyInteraction(onFinished, npc?.Name),
                    saveAccessoryClarification: true
                );
            };
        }

        private void ShowPlayerReplyChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
        {
            if (!Config.EnablePlayerReplyMenuAfterOutfitCompliment || npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                FinishPlayerReplyInteraction(onFinished, npc?.Name);
                return;
            }

            string title = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                ? "Responder ao comentário?"
                : "Reply to the comment?";
            string replyLabel = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                ? "Responder"
                : "Reply";
            string leaveLabel = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                ? "Ir embora"
                : "Leave";

            Game1.activeClickableMenu = new OutfitPlayerReplyChoiceMenu(
                title,
                replyLabel,
                leaveLabel,
                respond: () => OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished),
                leave: () => FinishPlayerReplyInteraction(onFinished, npc?.Name)
            );
        }

        private void OpenPlayerOutfitReplyInputMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished, string titleOverride = null, Action cancelOverride = null, bool saveAccessoryClarification = false)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                FinishPlayerReplyInteraction(onFinished, npc?.Name);
                return;
            }

            string title = !string.IsNullOrWhiteSpace(titleOverride)
                ? titleOverride
                : (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                    ? "Escreva sua resposta:"
                    : "Write your reply:");
            string sendLabel = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                ? "Enviar"
                : "Send";
            string cancelLabel = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt
                ? "Cancelar"
                : "Cancel";

            Game1.activeClickableMenu = new OutfitPlayerReplyTextInputMenu(
                title,
                sendLabel,
                cancelLabel,
                submit: replyText =>
                {
                    string cleanedReply = CleanPlayerOutfitReplyText(replyText);
                    if (string.IsNullOrWhiteSpace(cleanedReply))
                    {
                        OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished, titleOverride, cancelOverride, saveAccessoryClarification);
                        return;
                    }

                    if (saveAccessoryClarification)
                        SavePlayerProvidedAccessoryDescriptionForCurrentChange(cleanedReply);

                    if (!CanUseOwnAiForOutfitDialogue(npc))
                    {
                        FinishPlayerReplyInteraction(onFinished, npc?.Name);
                        return;
                    }

                    outfitReplyConversationHistory.Append(npc?.Name, "Player", cleanedReply);
                    StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, cleanedReply, onFinished);
                },
                cancel: () =>
                {
                    if (cancelOverride != null)
                        cancelOverride();
                    else
                        ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
                }
            );
        }

        private static string CleanPlayerOutfitReplyText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = Regex.Replace(text, @"\s+", " ").Trim();
            if (text.Length > 800)
                text = text.Substring(0, 800).Trim();
            return text;
        }

        private void StartPlayerReplyFollowUpGeneration(NPC npc, bool isSpouseDialogue, string npcCompliment, string playerReply, Action onFinished, OutfitAiContext prebuiltContext = null)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                FinishPlayerReplyInteraction(onFinished, npc?.Name);
                return;
            }

            OutfitAiContext context = prebuiltContext ?? BuildOutfitAiContext(npc, isSpouseDialogue);
            if (context == null)
            {
                FinishPlayerReplyInteraction(onFinished, npc?.Name);
                return;
            }

            // If a prebuilt context was passed (e.g. from the secret reveal flow), the transcript
            // is already set by the caller. Otherwise build it from the conversation history.
            if (prebuiltContext == null)
                context.ConversationTranscript = outfitReplyConversationHistory.BuildTranscript(npc.Name);

            Game1.activeClickableMenu = null;
            Game1.afterDialogues = null;

            PendingAiPlayerReplyGeneration pending = new()
            {
                NpcName = npc.Name,
                IsSpouseDialogue = isSpouseDialogue,
                NpcCompliment = npcCompliment ?? "",
                PlayerReply = playerReply ?? "",
                WaitingDotCount = 1,
                WaitingDotTimer = 30,
                SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
                Cancellation = new CancellationTokenSource(),
                OnFinished = onFinished
            };

            aiGenerationCoordinator.StartReply(pending, cancellationToken =>
            {
                try
                {
                    return outfitAiService.TryGenerateFollowUp(context, pending.NpcCompliment, pending.PlayerReply, out string followUp, cancellationToken) ? followUp : null;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Monitor.Log(" Background player-reply follow-up crashed: " + ex.Message, LogLevel.Warn);
                    return null;
                }
            });
            if (DebugLog) Monitor.Log($" Started background player-reply follow-up generation for {npc.Name}.", LogLevel.Info);
        }

        private void UpdatePendingOwnAiPlayerReplyGenerations()
        {
            if (!aiGenerationCoordinator.HasReplyGenerations)
                return;

            foreach (string npcName in aiGenerationCoordinator.GetReplyNpcNames())
            {
                if (!aiGenerationCoordinator.TryGetReply(npcName, out PendingAiPlayerReplyGeneration pending))
                    continue;
                NPC npc = Game1.getCharacterFromName(npcName);

                if (pending == null || npc == null || pending.Task == null)
                {
                    FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
                    aiGenerationCoordinator.RemoveReply(npcName);
                    continue;
                }

                AiGenerationLifecycleState lifecycleState = AiDialogueLifecycle.Advance(pending);
                if (lifecycleState == AiGenerationLifecycleState.Completed)
                {
                    if (!pending.CompletionHandled)
                    {
                        pending.CompletionHandled = true;
                        string generated = null;

                        try
                        {
                            if (pending.Task.Status == TaskStatus.RanToCompletion)
                                generated = pending.Task.Result;
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log(" Could not read player-reply follow-up result: " + ex.Message, LogLevel.Warn);
                        }

                        OpenGeneratedPlayerReplyFollowUp(npc, pending, generated);
                    }

                    aiGenerationCoordinator.RemoveReply(npcName);
                    continue;
                }

                if (lifecycleState == AiGenerationLifecycleState.TimedOut)
                {
                    Monitor.Log($" Player-reply follow-up generation for {npcName} exceeded the safety timer.", LogLevel.Warn);
                    AiRequestLifecycle.Cancel(pending.Cancellation);
                    FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
                    aiGenerationCoordinator.RemoveReply(npcName);
                    continue;
                }

                UpdateOwnAiPlayerReplyWaitingVisual(pending);
            }
        }

        private void UpdateOwnAiPlayerReplyWaitingVisual(PendingAiPlayerReplyGeneration pending)
        {
            if (pending == null)
                return;

            if (pending.WaitingDotTimer > 0)
            {
                pending.WaitingDotTimer--;
                return;
            }

            pending.WaitingDotTimer = 30;
            pending.WaitingDotCount++;
            if (pending.WaitingDotCount > 3)
                pending.WaitingDotCount = 1;
        }

        private void OpenGeneratedPlayerReplyFollowUp(NPC npc, PendingAiPlayerReplyGeneration pending, string generated)
        {
            if (npc == null || pending == null)
            {
                FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
                return;
            }

            if (Game1.player == null || npc.currentLocation != Game1.player.currentLocation || string.IsNullOrWhiteSpace(generated))
            {
                FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
                return;
            }

            if (generated.StartsWith(OutfitAiService.AccessoryClarificationMarker, StringComparison.Ordinal))
                generated = generated.Substring(OutfitAiService.AccessoryClarificationMarker.Length).Trim();

            if (string.IsNullOrWhiteSpace(generated))
            {
                FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
                return;
            }

            npc.CurrentDialogue.Clear();
            string dialogueId = pending.IsSpouseDialogue
                ? "OutfitReactions_SpousePlayerReplyFollowUp"
                : "OutfitReactions_GlobalPlayerReplyFollowUp";
            npc.CurrentDialogue.Push(new Dialogue(npc, dialogueId, generated));

            outfitReplyConversationHistory.Append(pending.NpcName, "NPC", generated);

            Game1.activeClickableMenu = null;
            Game1.afterDialogues = () => ShowPlayerReplyChoiceMenu(npc, pending.IsSpouseDialogue, generated, pending.OnFinished);
            npc.faceGeneralDirection(Game1.player.getStandingPosition());
            Game1.drawDialogue(npc);
        }

        /// <summary>
        /// Stop every background request which belongs to the current game state. This is called
        /// when that state is abandoned (warp, title screen, reset), so no old request can later
        /// deliver dialogue or consume provider time after it stopped being relevant.
        /// </summary>
        private void CancelAllPendingOwnAiGenerations()
        {
            IReadOnlyList<PendingAiPlayerReplyGeneration> replyGenerations = aiGenerationCoordinator.CancelAll();

            // A canceled reply must still finish its menu callback; otherwise the NPC can stay in
            // the temporary reply state after the farmer leaves the area.
            foreach (PendingAiPlayerReplyGeneration pending in replyGenerations)
                FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
        }

        private void FinishPlayerReplyInteraction(Action onFinished, string npcName = null)
        {
            outfitReplyConversationHistory.Reset(npcName);
            Game1.activeClickableMenu = null;
            Game1.afterDialogues = null;
            onFinished?.Invoke();
        }

        private bool TryQueueOtherNpcOutfitDialogue(NPC npc)
        {
            if (!Config.EnableNpcOutfitReactions || npc == null)
                return false;

            // AI-only build:
            // 1) Outfit Compliments built-in AI.
            // Manual NPC-specific/generic JSON fallbacks are intentionally disabled/commented out.
            if (TryShowOwnAiOutfitDialogue(npc, isSpouseDialogue: false, clearExistingDialogue: false))
                return true;

            Monitor.Log($" No AI outfit dialogue was queued for {npc.Name}. Manual JSON outfit dialogue is disabled in this AI-only build.", LogLevel.Warn);
            return false;
        }

        private bool RefreshOtherNpcOutfitPrompt(NPC npc)
        {
            return npc != null;
        }

        private void ClearOutfitPrompt(NPC npc)
        {
            // External dialogue-system support was removed; there is no external prompt override to clear.
        }

    }
}
