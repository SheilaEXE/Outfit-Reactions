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
    /// <summary>
    /// Snapshot of an NPC's special-action state while a spouse outfit reaction temporarily
    /// takes control of the interaction. Kept outside ModEntry so the eventual spouse-reaction
    /// controller owns its state instead of the global mod entry point.
    /// </summary>
    internal sealed class SpouseOutfitSpecialActionSnapshot
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

    /// <summary>Owns the temporary special-action snapshot for the active spouse reaction.</summary>
    internal sealed class SpouseSpecialActionController
    {
        public SpouseOutfitSpecialActionSnapshot Current { get; private set; }

        public bool HasSnapshotFor(NPC npc) => Current != null && Current.Npc == npc;
        public void Capture(SpouseOutfitSpecialActionSnapshot snapshot) => Current = snapshot;
        public void Clear() => Current = null;
    }

    /// <summary>Temporary route data captured while an outfit reaction pauses a spouse.</summary>
    internal sealed class SpouseRouteSnapshot
    {
        public Point? FinalDestination { get; set; }
        public PathFindController.endBehavior EndBehavior { get; set; }
        public int FinalFacingDirection { get; set; } = -1;
        public SchedulePathDescription Directions { get; set; }

        public void Clear()
        {
            FinalDestination = null;
            EndBehavior = null;
            FinalFacingDirection = -1;
            Directions = null;
        }
    }

    /// <summary>Owns the interruption and restoration of the spouse's active route.</summary>
    internal sealed class SpouseRouteController
    {
        private static readonly FieldInfo DirectionsToNewLocationField =
            typeof(NPC).GetField("directionsToNewLocation", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public SpouseRouteSnapshot Snapshot { get; } = new();
        public bool HasRoute => Snapshot.FinalDestination != null;

        public void Stop(NPC npc, IMonitor monitor, bool debugLog)
        {
            if (npc == null)
                return;

            if (!HasRoute)
            {
                try
                {
                    if (npc.controller != null && npc.controller.pathToEndPoint is Stack<Point> path && path.Count > 0)
                    {
                        Point finalTile = path.Last();
                        Snapshot.FinalDestination = finalTile;
                        Snapshot.EndBehavior = npc.controller.endBehaviorFunction;
                        Snapshot.FinalFacingDirection = npc.controller.finalFacingDirection;
                        Snapshot.Directions = GetDirections(npc);
                        if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Captured destination {finalTile} for {npc.Name}.", LogLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Could not capture destination for {npc.Name}: {ex.Message}", LogLevel.Info);
                }
            }

            npc.controller = null;
            npc.Halt();
            npc.Sprite.StopAnimation();
        }

        public void Restore(NPC npc, IMonitor monitor, bool debugLog)
        {
            if (npc == null || !HasRoute)
            {
                npc?.checkSchedule(Game1.timeOfDay);
                Clear();
                return;
            }

            try
            {
                Point destination = Snapshot.FinalDestination.Value;
                var restoredController = new PathFindController(npc, Utility.getGameLocationOfCharacter(npc), destination, Snapshot.FinalFacingDirection, Snapshot.EndBehavior);
                if (restoredController.pathToEndPoint != null && restoredController.pathToEndPoint.Count > 0)
                {
                    restoredController.endBehaviorFunction = Snapshot.EndBehavior;
                    npc.controller = restoredController;
                    if (Snapshot.Directions != null)
                        SetDirections(npc, Snapshot.Directions);
                    if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Restored {npc.Name}'s path to {destination} ({restoredController.pathToEndPoint.Count} steps).", LogLevel.Info);
                }
                else
                {
                    if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Could not pathfind to {destination} for {npc.Name} — falling back to checkSchedule.", LogLevel.Info);
                    npc.checkSchedule(Game1.timeOfDay);
                }
            }
            catch (Exception ex)
            {
                if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Error restoring path for {npc.Name}: {ex.Message} — falling back to checkSchedule.", LogLevel.Info);
                npc.checkSchedule(Game1.timeOfDay);
            }
            finally
            {
                Clear();
            }
        }

        public void Clear() => Snapshot.Clear();

        private static SchedulePathDescription GetDirections(NPC npc)
        {
            try { return DirectionsToNewLocationField?.GetValue(npc) as SchedulePathDescription; }
            catch { return null; }
        }

        private static void SetDirections(NPC npc, SchedulePathDescription value)
        {
            try { DirectionsToNewLocationField?.SetValue(npc, value); }
            catch { }
        }
    }

    /// <summary>Dialogue and friendship state temporarily replaced by a spouse outfit reaction.</summary>
    internal sealed class SpouseDialogueSnapshot
    {
        public List<Dialogue> DialogueQueue { get; set; }
        public string NpcName { get; set; } = "";
        public bool FriendshipStateCaptured { get; set; }
        public bool OriginalTalkedToToday { get; set; }
        public bool ForcedTalkedToToday { get; set; }

        public void Clear()
        {
            DialogueQueue = null;
            NpcName = "";
            FriendshipStateCaptured = false;
            OriginalTalkedToToday = false;
            ForcedTalkedToToday = false;
        }
    }

    /// <summary>Owns the dialogue and friendship state temporarily replaced by an outfit reaction.</summary>
    internal sealed class SpouseDialogueController
    {
        public SpouseDialogueSnapshot Snapshot { get; } = new();
        public bool HasBackup => !string.IsNullOrWhiteSpace(Snapshot.NpcName);

        public void Capture(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
        {
            Clear();
            if (npc == null)
                return;

            // Stack<T> enumerates top-to-bottom; Restore pushes bottom-to-top to preserve it.
            Snapshot.DialogueQueue = npc.CurrentDialogue?.ToList() ?? new List<Dialogue>();
            Snapshot.NpcName = npc.Name;
            TemporarilySkipFirstDailyDialogue(npc, player, monitor, debugLog);
        }

        public void TemporarilySkipFirstDailyDialogue(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
        {
            if (npc == null || player == null)
                return;

            try
            {
                if (!player.friendshipData.TryGetValue(npc.Name, out Friendship friendship) || friendship == null)
                    return;

                Snapshot.FriendshipStateCaptured = true;
                Snapshot.OriginalTalkedToToday = friendship.TalkedToToday;
                if (!friendship.TalkedToToday)
                {
                    friendship.TalkedToToday = true;
                    Snapshot.ForcedTalkedToToday = true;
                    if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Temporarily skipped first daily dialogue for {npc.Name} so the outfit compliment can play first.", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Could not temporarily skip first daily dialogue for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }

        public void Restore(NPC npc, Farmer player, bool restoreTalkState, bool clearCurrentDialogue, IMonitor monitor, bool debugLog)
        {
            if (npc == null || !HasBackup || !npc.Name.Equals(Snapshot.NpcName, StringComparison.OrdinalIgnoreCase))
                return;

            if (clearCurrentDialogue)
                npc.CurrentDialogue.Clear();

            if (Snapshot.DialogueQueue != null && Snapshot.DialogueQueue.Count > 0)
            {
                for (int i = Snapshot.DialogueQueue.Count - 1; i >= 0; i--)
                    npc.CurrentDialogue.Push(Snapshot.DialogueQueue[i]);
            }

            if (restoreTalkState)
                RestoreTalkedToToday(npc, player, monitor, debugLog);

            Clear();
        }

        public void Clear() => Snapshot.Clear();

        private void RestoreTalkedToToday(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
        {
            if (npc == null || player == null || !Snapshot.FriendshipStateCaptured || !Snapshot.ForcedTalkedToToday)
                return;

            try
            {
                if (player.friendshipData.TryGetValue(npc.Name, out Friendship friendship) && friendship != null)
                    friendship.TalkedToToday = Snapshot.OriginalTalkedToToday;
            }
            catch (Exception ex)
            {
                if (debugLog) monitor.Log($"[CLOTHES SPOUSE] Could not restore first daily dialogue state for {npc.Name}: {ex.Message}", LogLevel.Info);
            }
        }
    }

    /// <summary>Temporary proximity, pause, and linger state for the active spouse reaction.</summary>
    internal sealed class SpouseProximityState
    {
        public const int PostOutfitLingerDelayTicks = 360;
        public const float PostOutfitLingerDistance = 600f;
        public const float NoticePauseDistance = 96f;
        public const float NoticeReleaseDistance = 300f;

        public bool LingerActive { get; set; }
        public NPC LingerNpc { get; set; }
        public int LingerTimer { get; set; }
        public bool LingerPoseApplied { get; set; }
        public bool NoticePauseActive { get; set; }
        public bool NoticeHoldPoseApplied { get; set; }
        public int PendingBubbleTimer { get; set; }

        public void ClearLinger()
        {
            LingerActive = false;
            LingerNpc = null;
            LingerTimer = 0;
            LingerPoseApplied = false;
        }

        public void ClearNotice()
        {
            NoticePauseActive = false;
            NoticeHoldPoseApplied = false;
            PendingBubbleTimer = 0;
        }
    }

    /// <summary>Owns the timing and release decision for the post-dialogue spouse linger.</summary>
    internal static class SpousePostOutfitLingerController
    {
        public static void Begin(SpouseProximityState state, NPC npc)
        {
            state.LingerActive = true;
            state.LingerNpc = npc;
            state.LingerTimer = SpouseProximityState.PostOutfitLingerDelayTicks;
            state.LingerPoseApplied = false;
        }

        public static bool TickAndShouldResume(
            SpouseProximityState state,
            bool sameLocation,
            float distance,
            bool hasCapturedSpecialAction,
            float specialActionRestoreDistance)
        {
            if (state.LingerTimer > 0)
                state.LingerTimer--;

            return hasCapturedSpecialAction
                ? (!sameLocation || distance >= specialActionRestoreDistance)
                : (!sameLocation || distance >= SpouseProximityState.PostOutfitLingerDistance || state.LingerTimer <= 0);
        }

        public static void ApplyHoldPose(SpouseProximityState state, NPC npc, Farmer player)
        {
            if (npc.movementPause < 6)
                npc.movementPause = 6;

            // The pose is applied once only, so another mod can animate this NPC while the
            // linger remains active (for example, an automatic kiss animation).
            if (state.LingerPoseApplied)
                return;

            npc.Sprite?.StopAnimation();
            npc.faceGeneralDirection(player.getStandingPosition(), 0, false, false);
            state.LingerPoseApplied = true;
        }

        public static void Clear(SpouseProximityState state) => state.ClearLinger();
    }

    /// <summary>Pure proximity decisions used by the spouse outfit-reaction flow.</summary>
    internal static class SpouseOutfitReactionController
    {
        public static bool ResolveNoticePause(bool wasPaused, bool isSameLocation, float distance)
        {
            if (!isSameLocation)
                return false;
            if (distance <= SpouseProximityState.NoticePauseDistance)
                return true;
            if (distance >= SpouseProximityState.NoticeReleaseDistance)
                return false;
            return wasPaused;
        }

        public static bool CanShowPendingBubble(bool force, bool alreadyEmoted, int bubbleTimer)
        {
            return force || (!alreadyEmoted && bubbleTimer <= 0);
        }

        public static int GetPendingBubbleCooldown(bool force) => force ? 180 : 240;
    }
}
