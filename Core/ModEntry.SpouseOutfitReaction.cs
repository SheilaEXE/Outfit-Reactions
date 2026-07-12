using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OutfitReactions
{
    public sealed partial class ModEntry
    {
        // â”€â”€ Spouse/NPC clothes reaction flow â€” approach, animation, dialogue, reset â”€â”€

        private bool ShouldStartClothesReaction(NPC npc = null)
        {
            if (!changedClothes || lastFashionSenseChangeInfo == null)
                return false;

            FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);
            if (effectiveChangeInfo == null)
                return false;

            if (npc != null && IsSavedOutfitNoticeChange(effectiveChangeInfo) && !CanNpcNoticeCurrentOutfitNotice(npc))
                return false;

            // A special item removal (e.g. the Mayor's purple shorts, or the short-hat mod)
            // follows a witness rule: only NPCs who previously saw that item may react to its
            // absence. This must be checked BEFORE the generic vanilla-hat removal check below,
            // because a special item worn as a hat also triggers VanillaHatRemoved/VanillaHatChanged.
            // The two checks are chained with else-if: once the special-item check has decided this
            // is (or isn't) a witnessed special-item removal, the generic vanilla-hat check must NOT
            // run afterwards â€” it uses HatMemoryService, which knows nothing about special items,
            // and would otherwise re-block a reaction the special-item check just approved.
            if (npc != null && IsSpecialItemRemovalOnlyNotice(effectiveChangeInfo))
            {
                if (!NpcRemembersRemovedSpecialItem(npc, effectiveChangeInfo))
                    return false;
            }
            // A vanilla-hat REMOVAL is only noticeable to an NPC who actually saw the farmer wearing
            // that hat. Without this, NPCs the farmer never interacted with would react to the
            // removal ("good, you took that off") despite never having seen the hat. If a saved
            // outfit is still active, GetEffectiveFashionSenseChangeInfoForNpc() turns this into
            // an outfit notice for that NPC instead.
            else if (npc != null
                && IsVanillaHatRemovalOnlyNotice(effectiveChangeInfo)
                && !NpcRemembersRemovedVanillaHat(npc))
            {
                return false;
            }

            string dialogueKey = GetFashionSenseDialogueKey(effectiveChangeInfo);
            return !string.IsNullOrEmpty(dialogueKey);
        }

        private bool ShouldSpouseApproachPlayerForOutfit(NPC npc)
        {
            // Outfit notices now use the same safe idea as Lots and Lots of Kisses:
            // pause with movementPause and keep the NPC looking at the farmer, without
            // stealing, nulling, or replacing the schedule/controller. The old approach
            // path is intentionally disabled because it can break custom schedules and
            // cross-map pathing.
            return false;
        }

        private bool TryStartSpouseApproachPath(NPC npc)
        {
            if (npc == null || Game1.player == null || Game1.currentLocation == null)
                return false;

            Point playerTile = Game1.player.TilePoint;
            Point npcTile = npc.TilePoint;

            int offsetX = Math.Sign(npcTile.X - playerTile.X);
            int offsetY = Math.Sign(npcTile.Y - playerTile.Y);

            List<Point> candidates = new();

            // Prefer the side the spouse is already standing on, so he does not circle around
            // the farmer unless that tile is blocked.
            if (Math.Abs(npcTile.X - playerTile.X) > Math.Abs(npcTile.Y - playerTile.Y))
            {
                if (offsetX != 0)
                    candidates.Add(new Point(playerTile.X + offsetX, playerTile.Y));
                if (offsetY != 0)
                    candidates.Add(new Point(playerTile.X, playerTile.Y + offsetY));
            }
            else
            {
                if (offsetY != 0)
                    candidates.Add(new Point(playerTile.X, playerTile.Y + offsetY));
                if (offsetX != 0)
                    candidates.Add(new Point(playerTile.X + offsetX, playerTile.Y));
            }

            // Then try every adjacent tile around the player. This makes farmhouse approach
            // much less fragile when custom furniture, rugs, or decorations block the first
            // guessed target tile.
            candidates.Add(new Point(playerTile.X + 1, playerTile.Y));
            candidates.Add(new Point(playerTile.X - 1, playerTile.Y));
            candidates.Add(new Point(playerTile.X, playerTile.Y + 1));
            candidates.Add(new Point(playerTile.X, playerTile.Y - 1));
            candidates.Add(new Point(playerTile.X + 1, playerTile.Y + 1));
            candidates.Add(new Point(playerTile.X - 1, playerTile.Y + 1));
            candidates.Add(new Point(playerTile.X + 1, playerTile.Y - 1));
            candidates.Add(new Point(playerTile.X - 1, playerTile.Y - 1));

            foreach (Point target in candidates
                .Distinct()
                .OrderBy(tile => Math.Abs(tile.X - npcTile.X) + Math.Abs(tile.Y - npcTile.Y)))
            {
                if (target == npcTile)
                    continue;

                var tileLocation = new xTile.Dimensions.Location(target.X, target.Y);
                if (!Game1.currentLocation.isTilePassable(tileLocation, Game1.viewport))
                    continue;

                try
                {
                    var path = new PathFindController(npc, Game1.currentLocation, target, -1, false);
                    if (path?.pathToEndPoint != null && path.pathToEndPoint.Count > 0)
                    {
                        npc.controller = path;
                        if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Started farmhouse approach path for {npc.Name} to {target} ({path.pathToEndPoint.Count} steps).", LogLevel.Info);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Failed approach path candidate {target} for {npc.Name}: {ex.Message}", LogLevel.Info);
                }
            }

            return false;
        }

        private void UpdateClothesReactionSystem(NPC npc)
        {
            if (changedClothes && !isReactingToClothes)
                playerWasInClothesNoticeRange = false;

            if (npc == null || !Context.IsWorldReady || Game1.player == null)
                return;

            float distance = DistanceToPlayer(npc);
            bool inClothesNoticeRange = distance < Config.OutfitNoticeDistance && npc.currentLocation == Game1.player.currentLocation;
            bool shouldStartClothesReaction = ShouldStartClothesReaction(npc);
            bool spouseCanApproachPlayer = ShouldSpouseApproachPlayerForOutfit(npc);

            if (changedClothes && !isReactingToClothes && !shouldStartClothesReaction)
            {
                // This saved outfit was already read by this spouse/NPC for the current notice.
                // Keep the current outfit candidate alive for other NPCs instead of
                // clearing it every tick.
                FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);
                if (IsSavedOutfitNoticeChange(effectiveChangeInfo) && !CanNpcNoticeCurrentOutfitNotice(npc))
                {
                    playerWasInClothesNoticeRange = inClothesNoticeRange;
                    return;
                }

                // NPC-specific vanilla-hat removals should not clear the global notice just because
                // this partner did not witness the removed hat. Other NPCs nearby may still have
                // hat memory and be allowed to react to the removal.
                if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo)
                    && !NpcRemembersRemovedVanillaHat(npc))
                {
                    playerWasInClothesNoticeRange = inClothesNoticeRange;
                    return;
                }

                // Same idea for special wearable items (e.g. Mayor's shorts): if this NPC did not
                // see the removed item and there is no saved-outfit fallback for them, keep the
                // global removal notice alive for NPCs who did see it.
                if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo)
                    && !NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
                {
                    playerWasInClothesNoticeRange = inClothesNoticeRange;
                    return;
                }

                // Any other change type (e.g. a general piece-level FS change with ChangedOutfit=false)
                // falls through to here. This must NOT clear the shared changedClothes/
                // lastFashionSenseChangeInfo state: those are read by HasNoticeableCurrentFashionSenseAppearance(),
                // which gates every OTHER (non-spouse) NPC's ability to notice this same change. Sebastian
                // being done with his own reaction doesn't mean nobody else has seen it yet â€” only his
                // own per-NPC state (isReactingToClothes, clothesFirstNoticeDone, etc.) needs resetting here.
                ResetClothesState(false);
                return;
            }

            if (outfitSequenceActive && !isReactingToClothes && clothesFirstNoticeDone)
            {
                if (npc.currentLocation != Game1.player.currentLocation || distance > Config.OutfitCancelDistance)
                    ResetClothesReactionState();
            }

            if (Config.Enabled && shouldStartClothesReaction && !npc.isSleeping.Value && !isReactingToClothes)
            {
                if (!clothesFirstNoticeDone && inClothesNoticeRange && IsNpcFacingPlayer(npc))
                {
                    outfitSequenceActive = true;
                    clothesFirstNoticeDone = true;
                    clothesNoticePauseTimer = 90;

                    if (spouseCanApproachPlayer)
                        spouseRouteController.Stop(npc, Monitor, DebugLog);
                    // Show that an outfit compliment is pending, but do not force-stop the route
                    // unless the spouse is actually close enough for the kiss-style pause.
                    ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
                    UpdateSpouseOutfitNoticeHold(npc, distance);
                }

                if (clothesFirstNoticeDone && !isReactingToClothes)
                {
                    if (clothesNoticePauseTimer <= 0 && inClothesNoticeRange && clothesSecondNoticeCooldown <= 0)
                    {
                        outfitSequenceActive = true;

                        if (spouseCanApproachPlayer)
                        {
                            spouseRouteController.Stop(npc, Monitor, DebugLog);
                            npc.faceGeneralDirection(Game1.player.getStandingPosition());

                            isReactingToClothes = true;
                            clothesPathStarted = false;
                            clothesComplimentReady = false;
                            clothesInteractionCooldown = 180;
                            clothesPreferredOffset = Point.Zero;
                            clothesLastPlayerTile = Point.Zero;
                            clothesLastTargetTile = Point.Zero;
                            clothesChaseTimer = 420;
                            clothesReactingNpc = npc;
                        }
                        else
                        {
                            // Outside the farmhouse, do not interrupt the spouse's schedule or
                            // controller. Just make the outfit compliment available on click,
                            // like a prioritized dialogue, while they keep their normal route.

                            isReactingToClothes = true;
                            clothesPathStarted = false;
                            clothesComplimentReady = true;
                            clothesInteractionCooldown = 180;
                            clothesPreferredOffset = Point.Zero;
                            clothesLastPlayerTile = Point.Zero;
                            clothesLastTargetTile = Point.Zero;
                            clothesChaseTimer = 0;
                            clothesReactingNpc = npc;
                            if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] {npc.Name}'s outfit compliment is ready on click without pathing because they are outside the farmhouse.", LogLevel.Info);
                            ShowSpousePendingOutfitBubbleIfNeeded(npc);
                            UpdateSpouseOutfitNoticeHold(npc, distance);
                        }

                        clothesSecondNoticeCooldown = 300;
                    }
                }
            }

            // Don't expose the spouse compliment immediately after the first emote.
            // Always let the original reaction beat happen first: notice -> pause/call -> approach if needed -> dialogue ready.
            if (isReactingToClothes && clothesReactingNpc == npc)
            {
                outfitSequenceActive = true;
                UpdateSpouseOutfitNoticeHold(npc, distance);
                if (clothesComplimentReady)
                    ShowSpousePendingOutfitBubbleIfNeeded(npc);

                if (npc.currentLocation != Game1.player.currentLocation || distance > Config.OutfitCancelDistance)
                {
                    // Cancelling Sebastian's own in-progress reaction (player walked away) must not
                    // wipe the shared changedClothes/lastFashionSenseChangeInfo state either â€” same
                    // reasoning as the other ResetClothesState(true) call sites above: it would block
                    // every other NPC from ever noticing this same outfit change.
                    ResetClothesState(false);
                    return;
                }

                if (!clothesComplimentReady)
                {
                    if (distance <= 140f || clothesChaseTimer <= 0)
                    {
                        ShowOutfitCompliment(npc, inClothesNoticeRange);
                        return;
                    }

                    if (npc.controller == null)
                    {
                        if (TryStartSpouseApproachPath(npc))
                        {
                            clothesPathStarted = true;
                        }
                        else
                        {
                            // If every nearby tile is blocked by furniture/decor, don't freeze the
                            // whole reaction forever. Keep the call/dialogue available instead.
                            if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Could not find an approach path for {npc.Name} inside the farmhouse; making the outfit compliment ready on click.", LogLevel.Info);
                            clothesComplimentReady = true;
                        }

                        if (clothesInteractionCooldown <= 0)
                        {
                            clothesInteractionCooldown = 180;
                        }
                    }

                    playerWasInClothesNoticeRange = inClothesNoticeRange;
                    return;
                }
            }

            playerWasInClothesNoticeRange = inClothesNoticeRange;
        }

        private void UpdateSpouseOutfitNoticeHold(NPC npc, float distance)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
            {
                spouseProximityState.ClearNotice();
                return;
            }

            // Start the hold only when the spouse naturally gets very close (about one tile).
            // Once held, keep the soft pause until the farmer backs away enough. We never touch
            // npc.controller here, so schedules/pathfinding keep their original route alive.
            spouseProximityState.NoticePauseActive = SpouseOutfitReactionController.ResolveNoticePause(
                spouseProximityState.NoticePauseActive,
                isSameLocation: true,
                distance);

            if (!spouseProximityState.NoticePauseActive)
            {
                spouseProximityState.NoticeHoldPoseApplied = false;
                return;
            }

            CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);

            if (npc.movementPause < 6)
                npc.movementPause = 6;

            // Only strike the "looking at the farmer" pose once, on the tick the hold starts â€”
            // not every tick. Other mods (e.g. a kiss mod) may animate this same NPC while the
            // hold is active; re-applying StopAnimation()/faceGeneralDirection() every tick would
            // immediately cancel that animation back to idle right after it's set, making the NPC
            // appear frozen during, say, a kiss. movementPause alone is enough to keep them in place.
            if (!spouseProximityState.NoticeHoldPoseApplied)
            {
                npc.Sprite?.StopAnimation();
                npc.faceGeneralDirection(Game1.player.getStandingPosition(), 0, false, false);
                spouseProximityState.NoticeHoldPoseApplied = true;
            }
        }

        private void ShowSpousePendingOutfitBubbleIfNeeded(NPC npc, bool force = false)
        {
            if (npc == null || Game1.player == null || npc.currentLocation != Game1.player.currentLocation)
                return;

            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            // Only keep the reminder visible while the farmer is near enough to reasonably click.
            if (DistanceToPlayer(npc) > Config.OutfitNoticeDistance)
                return;

            // Fire the ellipsis emote only ONCE per outfit notice.
            // It re-fires only if the player left the notice range and came back
            // (clothesEmoteFired is reset in ResetClothesState/ResetClothesReactionState).
            if (!SpouseOutfitReactionController.CanShowPendingBubble(
                force,
                clothesEmoteFired,
                spouseProximityState.PendingBubbleTimer))
                return;

            // 40 = ellipsis bubble. This reads as "I noticed something / talk to me" without
            // interrupting the NPC's current route.
            npc.doEmote(40);
            clothesEmoteFired = true;
            spouseProximityState.PendingBubbleTimer = SpouseOutfitReactionController.GetPendingBubbleCooldown(force);
        }

        private void ShowOutfitCompliment(NPC npc, bool inClothesNoticeRange)
        {
            outfitSequenceActive = true;
            // Do not clear or replace the controller here. The outfit reaction should behave
            // like the kiss mod pause: the NPC can be held briefly with movementPause, but
            // their active schedule/path remains intact.
            UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
            CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
            npc.faceGeneralDirection(Game1.player.getStandingPosition());

            spouseDialogueController.Capture(npc, Game1.player, Monitor, DebugLog);

            bool deferOwnAiUntilClick = ShouldUseDeferredOwnAiForNpc(npc);
            if (!deferOwnAiUntilClick)
            {
                if (!QueueSpouseOutfitDialogueOnly(npc))
                {
                    KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "AI queue was not available during the spouse outfit reaction.");
                    return;
                }

                InstallSpouseAfterOutfitDialogue(npc);
            }
            else
            {
                Monitor.Log($" {npc.Name}'s spouse outfit compliment is waiting for player click before AI generation starts.", LogLevel.Debug);
            }

            isReactingToClothes = true;
            clothesPathStarted = true;
            clothesComplimentReady = true;
            clothesChaseTimer = 0;
            clothesReactingNpc = npc;
            playerWasInClothesNoticeRange = inClothesNoticeRange;
        }

        private void KeepSpouseOutfitNoticePendingAfterAiFailure(NPC npc, string reason = null)
        {
            if (npc == null)
                return;

            try
            {
                ClearOutfitPrompt(npc);
                spouseDialogueController.Restore(npc, Game1.player, restoreTalkState: true, clearCurrentDialogue: false, monitor: Monitor, debugLog: DebugLog);
            }
            catch (Exception ex)
            {
                if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Could not restore normal dialogue after failed outfit AI for {npc.Name}: {ex.Message}", LogLevel.Info);
            }

            // Keep the notice alive without marking the outfit as read.
            // This prevents a new ellipsis bubble from firing every tick while the player
            // remains nearby, but the existing pending outfit reaction can still be clicked
            // again. Moving far enough away cancels this state; approaching later lets the
            // spouse notice the outfit again through the normal distance/cancel logic.
            outfitSequenceActive = true;
            isReactingToClothes = true;
            clothesComplimentReady = true;
            clothesPathStarted = false;
            clothesFirstNoticeDone = true;
            clothesNoticePauseTimer = 0;
            clothesSecondNoticeCooldown = Math.Max(clothesSecondNoticeCooldown, 300);
            clothesChaseTimer = 0;
            clothesReactingNpc = npc;

            if (Game1.player != null && npc.currentLocation == Game1.player.currentLocation)
            {
                float distance = DistanceToPlayer(npc);
                ShowSpousePendingOutfitBubbleIfNeeded(npc);
                UpdateSpouseOutfitNoticeHold(npc, distance);
                playerWasInClothesNoticeRange = distance < Config.OutfitNoticeDistance;
            }

            string suffix = string.IsNullOrWhiteSpace(reason) ? "" : " Reason: " + reason;
            if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Outfit AI failed for {npc.Name}, but the outfit was NOT marked as read. The current notice will stay pending until click retry or distance cancel.{suffix}", LogLevel.Info);
        }

        private bool QueueSpouseOutfitDialogueOnly(NPC npc)
        {
            if (npc == null)
                return false;

            // AI-only build:
            // 1) Outfit Compliments built-in AI.
            // Manual NPC-specific/generic JSON fallbacks are intentionally disabled/commented out.
            //
            // Built-in AI no longer pushes a temporary Dialogue object immediately. It starts a
            // background generation task and draws the waiting text on the HUD instead. That means
            // CurrentDialogue can legitimately stay empty here; returning false would let vanilla
            // checkAction continue and could restart the outfit notice loop forever.
            if (TryShowOwnAiOutfitDialogue(npc, isSpouseDialogue: true, clearExistingDialogue: false))
                return true;

            Monitor.Log($" No AI outfit dialogue was queued for {npc.Name}. Manual JSON outfit dialogue is disabled in this AI-only build. Keeping this outfit notice pending until the player cancels by moving away.", LogLevel.Warn);
            return false;
        }

        private void RestoreSpouseDialogueBackupIfPending()
        {
            if (!spouseDialogueController.HasBackup)
                return;

            NPC npc = Game1.getCharacterFromName(spouseDialogueController.Snapshot.NpcName);
            if (npc == null)
            {
                spouseDialogueController.Clear();
                return;
            }

            ClearOutfitPrompt(npc);
            spouseDialogueController.Restore(npc, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, monitor: Monitor, debugLog: DebugLog);
        }

        // Kept temporarily while the inactive legacy route block is removed in a later cleanup.
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

        private void CaptureSpouseOutfitSpecialActionBeforeOutfit(NPC npc)
        {
            if (npc == null || npc.Sprite == null || npc.currentLocation == null)
                return;

            if (spouseSpecialActionController.HasSnapshotFor(npc))
                return;

            if (npc.isMoving())
                return;

            List<FarmerSprite.AnimationFrame> animation = null;
            if (npc.Sprite.CurrentAnimation != null && npc.Sprite.CurrentAnimation.Count > 0)
                animation = new List<FarmerSprite.AnimationFrame>(npc.Sprite.CurrentAnimation);

            bool hasSpecialAnimation = animation != null && animation.Count > 0;
            bool hasSpecialStaticFrame = npc.Sprite.CurrentFrame >= 16;

            if (!hasSpecialAnimation && !hasSpecialStaticFrame)
                return;

            spouseSpecialActionController.Capture(new SpouseOutfitSpecialActionSnapshot
            {
                Npc = npc,
                Location = npc.currentLocation,
                FacingDirection = npc.FacingDirection,
                CurrentFrame = npc.Sprite.CurrentFrame,
                Flip = npc.flip,
                MovementPause = (int)npc.movementPause,
                AddedSpeed = (int)npc.addedSpeed,
                CurrentAnimation = animation
            });

            npc.Sprite.StopAnimation();
            npc.Sprite.ClearAnimation();
            npc.Sprite.CurrentAnimation = null;
            npc.flip = false;
            npc.Sprite.CurrentFrame = GetNpcIdleFrameForDirection(npc.FacingDirection);
            npc.Sprite.UpdateSourceRect();

            if (DebugLog) Monitor.Log($"[CLOTHES SPOUSE] Saved special animation for {npc.Name} before outfit reaction. frame={spouseSpecialActionController.Current.CurrentFrame} anim={(animation != null ? animation.Count : 0)}", LogLevel.Info);
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

        private void DoClothesFinalEmotes(NPC npc)
        {
            if (npc == null || Game1.player == null)
                return;

            int[] possibleEmotes = { 20, 60 };
            npc.doEmote(possibleEmotes[random.Next(possibleEmotes.Length)]);
            Game1.player.doEmote(possibleEmotes[random.Next(possibleEmotes.Length)]);
        }

        private void ResetClothesReactionState()
        {
            spouseSpecialActionController.TryRestore(true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, OutfitSpecialActionRestoreDistance, Monitor, DebugLog);

            isReactingToClothes = false;
            clothesPathStarted = false;
            clothesComplimentReady = false;
            clothesPreferredOffset = Point.Zero;
            clothesLastPlayerTile = Point.Zero;
            clothesLastTargetTile = Point.Zero;
            clothesFirstNoticeDone = false;
            clothesEmoteFired = false;
            clothesNoticePauseTimer = 0;
            playerWasInClothesNoticeRange = false;
            clothesChaseTimer = 0;
            clothesReactingNpc = null;
            outfitSequenceActive = false;
            spouseProximityState.ClearNotice();
        }

        private void ResetClothesState(bool clearChangeFlag = false)
        {
            RestoreSpouseDialogueBackupIfPending();
            spouseRouteController.Clear();
            spouseSpecialActionController.TryRestore(true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, OutfitSpecialActionRestoreDistance, Monitor, DebugLog);
            ClearSpousePostOutfitLinger();

            isReactingToClothes = false;
            clothesPathStarted = false;
            clothesComplimentReady = false;
            clothesFirstNoticeDone = false;
            clothesEmoteFired = false;
            clothesNoticePauseTimer = 0;
            clothesSecondNoticeCooldown = 0;
            playerWasInClothesNoticeRange = false;
            clothesInteractionCooldown = 0;
            clothesPreferredOffset = Point.Zero;
            clothesLastPlayerTile = Point.Zero;
            clothesLastTargetTile = Point.Zero;
            clothesReactingNpc = null;
            outfitSequenceActive = false;
            spouseProximityState.ClearNotice();
            fashionSenseMenuOpen = false;
            fsSnapshotBefore = null;
            CancelAllPendingOwnAiGenerations();

            if (clearChangeFlag)
            {
                changedClothes = false;
                lastFashionSenseChangeInfo = null;
            }
        }

    }
}


