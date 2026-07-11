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

        private readonly SpouseDialogueController spouseDialogueController = new();

        // Captures the original route's destination, then recalculates a path after dialogue.
        // This preserves the NPC's schedule without replaying obsolete path steps.
        private readonly SpouseRouteController spouseRouteController = new();

        // The notice pause and post-dialogue linger use movementPause only, leaving the
        // spouse's controller and custom schedule intact whenever possible.
        private readonly SpouseProximityState spouseProximityState = new();
        private readonly SpouseSpecialActionController spouseSpecialActionController = new();
        private const float OutfitSpecialActionRestoreDistance = 300f;

        private const float SpouseOutfitNoticePauseDistance = SpouseProximityState.NoticePauseDistance;
        private const float SpouseOutfitNoticeReleaseDistance = SpouseProximityState.NoticeReleaseDistance;

    }
}
