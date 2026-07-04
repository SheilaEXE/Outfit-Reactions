using System;
using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI;
using StardewValley;

namespace OutfitReactions
{
    /// <summary>
    /// Relationship lookups for <see cref="ModEntry"/>: who the player is married to or dating, and
    /// the status/heart context used to pick relationship-appropriate dialogue. These are pure reads
    /// (no state changes) and are shared by both the close-partner reaction flow and prompt building.
    /// </summary>
    public sealed partial class ModEntry
    {
        private NPC GetSpouse()
        {
            if (!Context.IsWorldReady || Game1.player == null || string.IsNullOrWhiteSpace(Game1.player.spouse))
                return null;

            NPC spouse = Game1.getCharacterFromName(Game1.player.spouse);

            return CanNpcReactToOutfit(spouse) ? spouse : null;
        }

        /// <summary>
        /// Returns the NPC the player is currently dating (status Dating/Engaged) who is NOT the
        /// official spouse, so they get the same close-partner reaction treatment as the spouse.
        /// Returns null when there is no dating partner or when the partner is already the spouse.
        /// </summary>
        private NPC GetDatingNpc()
        {
            if (!Context.IsWorldReady || Game1.player?.friendshipData == null)
                return null;

            string spouseName = Game1.player.spouse ?? "";

            foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
            {
                string npcName = pair.Key;
                if (string.IsNullOrWhiteSpace(npcName))
                    continue;

                // Skip the official spouse — they are handled by GetSpouse().
                if (!string.IsNullOrWhiteSpace(spouseName) &&
                    npcName.Equals(spouseName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDatingOrEngagedFriendship(pair.Value))
                    continue;

                NPC npc = Game1.getCharacterFromName(npcName);
                if (npc != null && CanNpcReactToOutfit(npc))
                    return npc;
            }

            return null;
        }

        private (string Status, int Hearts) GetRelationshipDialogueContext(NPC npc)
        {
            string status = "Friend";
            int hearts = 0;

            if (npc == null || Game1.player == null)
                return (status, hearts);

            Friendship friendship = null;
            if (Game1.player.friendshipData != null && Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship foundFriendship))
            {
                friendship = foundFriendship;

                if (friendship != null)
                    hearts = Math.Max(0, Math.Min(14, friendship.Points / 250));
            }

            if (!string.IsNullOrWhiteSpace(Game1.player.spouse) && npc.Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase))
            {
                status = "Spouse";
            }
            else if (IsDatingOrEngagedFriendship(friendship))
            {
                status = "Dating";
            }

            return (status, hearts);
        }

        private bool IsDatingOrEngagedFriendship(Friendship friendship)
        {
            if (friendship == null)
                return false;

            try
            {
                Type type = friendship.GetType();

                foreach (string methodName in new[] { "IsDating", "IsEngaged" })
                {
                    MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                    if (method != null && method.ReturnType == typeof(bool) && method.Invoke(friendship, null) is bool result && result)
                        return true;
                }

                object statusObject = type.GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship)
                    ?? type.GetField("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship);

                string statusText = statusObject?.ToString() ?? "";
                return statusText.Contains("Dating", StringComparison.OrdinalIgnoreCase)
                    || statusText.Contains("Engaged", StringComparison.OrdinalIgnoreCase)
                    || statusText.Contains("Fiance", StringComparison.OrdinalIgnoreCase)
                    || statusText.Contains("Fiancé", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<int> GetRelationshipHeartThresholds(string status, int hearts)
        {
            int[] thresholds = string.Equals(status, "Spouse", StringComparison.OrdinalIgnoreCase)
                ? new[] { 14, 12, 10, 8, 6, 4, 2 }
                : new[] { 10, 8, 6, 5, 4, 2 };

            foreach (int threshold in thresholds)
            {
                if (hearts >= threshold)
                    yield return threshold;
            }
        }
    }
}
