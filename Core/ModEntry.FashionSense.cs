using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley;

namespace OutfitReactions
{
    /// <summary>
    /// Low-level Fashion Sense read helpers for <see cref="ModEntry"/>. These pull appearance ids,
    /// colors, and modData values from the Fashion Sense API (with safe fallbacks), and are the
    /// building blocks used by the snapshot/change-detection and reaction logic elsewhere.
    /// </summary>
    public sealed partial class ModEntry
    {
        private string GetFsModData(string key)
        {
            if (Game1.player == null)
                return null;

            return Game1.player.modData.TryGetValue(key, out string value) ? value : null;
        }

        private string GetFsAppearanceId(IFashionSenseApi.Type type)
        {
            if (fsApi == null || Game1.player == null)
                return null;

            try
            {
                KeyValuePair<bool, string> response = fsApi.GetCurrentAppearanceId(type, Game1.player);
                if (response.Key && !string.IsNullOrWhiteSpace(response.Value))
                {
                    string value = response.Value.Trim();
                    if (!value.Equals("None", StringComparison.OrdinalIgnoreCase))
                        return value;
                }
            }
            catch
            {
                // Optional Fashion Sense API data; fall back to modData where available.
            }

            return null;
        }

        private string GetFsAppearanceColorKey(IFashionSenseApi.Type type)
        {
            if (fsApi == null || Game1.player == null)
                return null;

            try
            {
                KeyValuePair<bool, Color> response = fsApi.GetAppearanceColor(type, Game1.player);
                if (!response.Key)
                    return null;

                Color color = response.Value;
                return color.R.ToString("X2", CultureInfo.InvariantCulture)
                    + color.G.ToString("X2", CultureInfo.InvariantCulture)
                    + color.B.ToString("X2", CultureInfo.InvariantCulture)
                    + color.A.ToString("X2", CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
    }
}
