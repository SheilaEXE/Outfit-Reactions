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
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using OutfitReactions.Ai;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace OutfitReactions;

public sealed partial class ModEntry : Mod
{	private NPC GetSpouse()
	{
		if (!Context.IsWorldReady || Game1.player == null || string.IsNullOrWhiteSpace(Game1.player.spouse))
		{
			return null;
		}
		NPC characterFromName = Game1.getCharacterFromName(Game1.player.spouse, true, false);
		return CanNpcReactToOutfit(characterFromName) ? characterFromName : null;
	}

	private NPC GetDatingNpc()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!Context.IsWorldReady || Game1.player?.friendshipData == null)
		{
			return null;
		}
		string value = Game1.player.spouse ?? "";
		var enumerator = Game1.player.friendshipData.Pairs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, Friendship> current = enumerator.Current;
				string key = current.Key;
				if (!string.IsNullOrWhiteSpace(key) && (string.IsNullOrWhiteSpace(value) || !key.Equals(value, StringComparison.OrdinalIgnoreCase)) && IsDatingOrEngagedFriendship(current.Value))
				{
					NPC characterFromName = Game1.getCharacterFromName(key, true, false);
					if (characterFromName != null && CanNpcReactToOutfit(characterFromName))
					{
						return characterFromName;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	private (string Status, int Hearts) GetRelationshipDialogueContext(NPC npc)
	{
		string item = "Friend";
		int item2 = 0;
		if (npc == null || Game1.player == null)
		{
			return (Status: item, Hearts: item2);
		}
		Friendship val = null;
		Friendship val2 = default(Friendship);
		if (Game1.player.friendshipData != null && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).TryGetValue(((Character)npc).Name, out val2))
		{
			val = val2;
			if (val != null)
			{
				item2 = Math.Max(0, Math.Min(14, val.Points / 250));
			}
		}
		if (!string.IsNullOrWhiteSpace(Game1.player.spouse) && ((Character)npc).Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase))
		{
			item = "Spouse";
		}
		else if (IsDatingOrEngagedFriendship(val))
		{
			item = "Dating";
		}
		return (Status: item, Hearts: item2);
	}

	private bool IsDatingOrEngagedFriendship(Friendship friendship)
	{
		if (friendship == null)
		{
			return false;
		}
		try
		{
			Type type = ((object)friendship).GetType();
			string[] array = new string[2] { "IsDating", "IsEngaged" };
			bool flag = default(bool);
			foreach (string name in array)
			{
				MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				int num;
				if (method != null && method.ReturnType == typeof(bool))
				{
					object obj = method.Invoke(friendship, null);
					if (obj is bool)
					{
						flag = (bool)obj;
						num = 1;
					}
					else
					{
						num = 0;
					}
				}
				else
				{
					num = 0;
				}
				if (((uint)num & (flag ? 1u : 0u)) != 0)
				{
					return true;
				}
			}
			string text = (type.GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship) ?? type.GetField("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship))?.ToString() ?? "";
			return text.Contains("Dating", StringComparison.OrdinalIgnoreCase) || text.Contains("Engaged", StringComparison.OrdinalIgnoreCase) || text.Contains("Fiance", StringComparison.OrdinalIgnoreCase) || text.Contains("Fiancé", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private IEnumerable<int> GetRelationshipHeartThresholds(string status, int hearts)
	{
		int[] thresholds = ((!string.Equals(status, "Spouse", StringComparison.OrdinalIgnoreCase)) ? new int[6] { 10, 8, 6, 5, 4, 2 } : new int[7] { 14, 12, 10, 8, 6, 4, 2 });
		int[] array = thresholds;
		foreach (int threshold in array)
		{
			if (hearts >= threshold)
			{
				yield return threshold;
			}
		}
	}
}
