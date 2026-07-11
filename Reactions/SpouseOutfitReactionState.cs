using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Pathfinding;
using System.Collections.Generic;

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
        public bool NoticePauseActive { get; set; }
        public int PendingBubbleTimer { get; set; }

        public void ClearLinger()
        {
            LingerActive = false;
            LingerNpc = null;
            LingerTimer = 0;
        }

        public void ClearNotice()
        {
            NoticePauseActive = false;
            PendingBubbleTimer = 0;
        }
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
