using Microsoft.Xna.Framework;
using StardewValley;

namespace OutfitReactions
{
    /// <summary>
    /// State owned exclusively by the close-partner/spouse outfit-reaction flow.
    /// Keeping it beside that flow prevents the general mod entry point from becoming the
    /// implicit owner of every transient reaction detail.
    /// </summary>
    public sealed partial class ModEntry
    {
        private bool isReactingToClothes;
        private int clothesInteractionCooldown;
        private bool clothesPathStarted;
        private bool clothesComplimentReady;
        private Point clothesPreferredOffset = Point.Zero;
        private Point clothesLastPlayerTile = Point.Zero;
        private Point clothesLastTargetTile = Point.Zero;
        private bool clothesFirstNoticeDone;
        private bool clothesEmoteFired;
        private int clothesNoticePauseTimer;
        private bool playerWasInClothesNoticeRange;
        private int clothesSecondNoticeCooldown;
        private int clothesChaseTimer;
        private NPC clothesReactingNpc;
        private bool outfitSequenceActive;

        private readonly SpouseDialogueSnapshot spouseDialogueSnapshot = new();

        // Captures the original route's destination, then recalculates a path after dialogue.
        // This preserves the NPC's schedule without replaying obsolete path steps.
        private readonly SpouseRouteSnapshot spouseRouteSnapshot = new();

        // The notice pause and post-dialogue linger use movementPause only, leaving the
        // spouse's controller and custom schedule intact whenever possible.
        private readonly SpouseProximityState spouseProximityState = new();
        private readonly SpouseSpecialActionController spouseSpecialActionController = new();
        private const float OutfitSpecialActionRestoreDistance = 300f;

        // Compatibility accessors for the pre-extracted reaction routines. They deliberately
        // project onto the single state objects above, so old and newly separated call sites
        // always observe the same transient reaction state.
        private bool spouseOutfitNoticePauseActive
        {
            get => spouseProximityState.NoticePauseActive;
            set => spouseProximityState.NoticePauseActive = value;
        }
        private int spousePendingOutfitBubbleTimer
        {
            get => spouseProximityState.PendingBubbleTimer;
            set => spouseProximityState.PendingBubbleTimer = value;
        }
        private const float SpouseOutfitNoticePauseDistance = SpouseProximityState.NoticePauseDistance;
        private const float SpouseOutfitNoticeReleaseDistance = SpouseProximityState.NoticeReleaseDistance;

        private System.Collections.Generic.List<Dialogue> spouseDialogueBackupBeforeOutfit
        {
            get => spouseDialogueSnapshot.DialogueQueue;
            set => spouseDialogueSnapshot.DialogueQueue = value;
        }
        private string spouseDialogueBackupNpcName
        {
            get => spouseDialogueSnapshot.NpcName;
            set => spouseDialogueSnapshot.NpcName = value;
        }
        private bool spouseFriendshipStateCaptured
        {
            get => spouseDialogueSnapshot.FriendshipStateCaptured;
            set => spouseDialogueSnapshot.FriendshipStateCaptured = value;
        }
        private bool spouseOriginalTalkedToToday
        {
            get => spouseDialogueSnapshot.OriginalTalkedToToday;
            set => spouseDialogueSnapshot.OriginalTalkedToToday = value;
        }
        private bool spouseForcedTalkedToToday
        {
            get => spouseDialogueSnapshot.ForcedTalkedToToday;
            set => spouseDialogueSnapshot.ForcedTalkedToToday = value;
        }

        private Point? spouseFinalDestinationBackup
        {
            get => spouseRouteSnapshot.FinalDestination;
            set => spouseRouteSnapshot.FinalDestination = value;
        }
        private StardewValley.Pathfinding.PathFindController.endBehavior spouseEndBehaviorBackup
        {
            get => spouseRouteSnapshot.EndBehavior;
            set => spouseRouteSnapshot.EndBehavior = value;
        }
        private int spouseFinalFacingBackup
        {
            get => spouseRouteSnapshot.FinalFacingDirection;
            set => spouseRouteSnapshot.FinalFacingDirection = value;
        }
        private StardewValley.Pathfinding.SchedulePathDescription spouseDirectionsBackup
        {
            get => spouseRouteSnapshot.Directions;
            set => spouseRouteSnapshot.Directions = value;
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
            public System.Collections.Generic.List<FarmerSprite.AnimationFrame> CurrentAnimation { get; set; }
        }

        private NpcOutfitSpecialActionSnapshot spouseOutfitSpecialActionSnapshot
        {
            get
            {
                SpouseOutfitSpecialActionSnapshot snapshot = spouseSpecialActionController.Current;
                if (snapshot == null)
                    return null;
                return new NpcOutfitSpecialActionSnapshot
                {
                    Npc = snapshot.Npc,
                    Location = snapshot.Location,
                    FacingDirection = snapshot.FacingDirection,
                    CurrentFrame = snapshot.CurrentFrame,
                    Flip = snapshot.Flip,
                    MovementPause = snapshot.MovementPause,
                    AddedSpeed = snapshot.AddedSpeed,
                    CurrentAnimation = snapshot.CurrentAnimation
                };
            }
            set
            {
                if (value == null)
                {
                    spouseSpecialActionController.Clear();
                    return;
                }
                spouseSpecialActionController.Capture(new SpouseOutfitSpecialActionSnapshot
                {
                    Npc = value.Npc,
                    Location = value.Location,
                    FacingDirection = value.FacingDirection,
                    CurrentFrame = value.CurrentFrame,
                    Flip = value.Flip,
                    MovementPause = value.MovementPause,
                    AddedSpeed = value.AddedSpeed,
                    CurrentAnimation = value.CurrentAnimation
                });
            }
        }
    }
}
