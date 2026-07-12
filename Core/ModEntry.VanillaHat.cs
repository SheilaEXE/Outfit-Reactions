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
{	private string GetCurrentVanillaHatId()
	{
		try
		{
			Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
			if (val == null)
			{
				return "";
			}
			return StringUtils.FirstNonEmpty(((Item)val).ItemId, ((Item)val).Name) ?? "";
		}
		catch
		{
			return "";
		}
	}

	private static bool IsEmptyFashionSenseValue(string value)
	{
		return string.IsNullOrWhiteSpace(value) || value.Trim().Equals("None", StringComparison.OrdinalIgnoreCase);
	}

	private bool IsFashionSenseHatCoveringVanilla()
	{
		string fsModData = GetFsModData("FashionSense.CustomHat.Id");
		if (!IsEmptyFashionSenseValue(fsModData))
		{
			return true;
		}
		string fsAppearanceId = GetFsAppearanceId(IFashionSenseApi.Type.Hat);
		return !IsEmptyFashionSenseValue(fsAppearanceId) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(fsAppearanceId);
	}

	private bool IsFashionSensePantsCoveringVanilla()
	{
		string fsModData = GetFsModData("FashionSense.CustomPants.Id");
		if (IsFashionSensePantsValueCoveringVanilla(fsModData))
		{
			return true;
		}
		string fsAppearanceId = GetFsAppearanceId(IFashionSenseApi.Type.Pants);
		return IsFashionSensePantsValueCoveringVanilla(fsAppearanceId);
	}

	private static bool IsFashionSensePantsValueCoveringVanilla(string value)
	{
		return !IsEmptyFashionSenseValue(value) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(value);
	}

	private string GetVisibleVanillaHatId()
	{
		if (IsFashionSenseHatCoveringVanilla())
		{
			return "";
		}
		return GetCurrentVanillaHatId();
	}

	private string GetCurrentVanillaHatName()
	{
		try
		{
			Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
			if (val == null)
			{
				return "";
			}
			return StringUtils.FirstNonEmpty(((Item)val).DisplayName, ((Item)val).Name) ?? "";
		}
		catch
		{
			return "";
		}
	}

	private string BuildVanillaHatMemoryContext(NPC npc)
	{
		if (hatMemoryService == null || npc == null)
		{
			return null;
		}
		string visibleVanillaHatId = GetVisibleVanillaHatId();
		if (string.IsNullOrWhiteSpace(visibleVanillaHatId))
		{
			return null;
		}
		string currentVanillaHatName = GetCurrentVanillaHatName();
		HatMemoryComparison memory = hatMemoryService.GetMemory(((Character)npc).Name, visibleVanillaHatId, currentVanillaHatName);
		if (memory == null)
		{
			return null;
		}
		return hatMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
	}

	private void RecordVanillaHatMemory(NPC npc)
	{
		if (hatMemoryService != null && npc != null && !IsFashionSenseHatCoveringVanilla())
		{
			hatMemoryService.RecordMemory(((Character)npc).Name, GetCurrentVanillaHatId(), GetCurrentVanillaHatName(), Game1.currentSeason, Game1.dayOfMonth, Game1.year);
		}
	}

	private void RecordVanillaPantsMemory(NPC npc, string pantsName)
	{
		if (npc == null || string.IsNullOrWhiteSpace(pantsName) || Game1.player == null)
		{
			return;
		}
		try
		{
			NetRef<Clothing> pantsItem = Game1.player.pantsItem;
			object obj;
			if (pantsItem == null)
			{
				obj = null;
			}
			else
			{
				Clothing value = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)pantsItem).Value;
				obj = ((value != null) ? ((Item)value).ItemId : null);
			}
			if (obj == null)
			{
				obj = "";
			}
			string text = (string)obj;
			if (!string.IsNullOrWhiteSpace(text))
			{
				string text2 = "NatrollEXE.OutfitReactions/PantsSeen/" + ((Character)npc).Name + "/" + text;
				int result = 0;
				string s = default(string);
				if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, out s))
				{
					int.TryParse(s, out result);
				}
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[text2] = (result + 1).ToString();
			}
		}
		catch
		{
		}
	}

	private int GetVanillaPantsSeenCount(NPC npc)
	{
		if (npc == null || Game1.player == null)
		{
			return 0;
		}
		try
		{
			NetRef<Clothing> pantsItem = Game1.player.pantsItem;
			object obj;
			if (pantsItem == null)
			{
				obj = null;
			}
			else
			{
				Clothing value = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)pantsItem).Value;
				obj = ((value != null) ? ((Item)value).ItemId : null);
			}
			if (obj == null)
			{
				obj = "";
			}
			string text = (string)obj;
			if (string.IsNullOrWhiteSpace(text))
			{
				return 0;
			}
			string text2 = "NatrollEXE.OutfitReactions/PantsSeen/" + ((Character)npc).Name + "/" + text;
			string s = default(string);
			if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, out s) && int.TryParse(s, out var result))
			{
				return result;
			}
			return 0;
		}
		catch
		{
			return 0;
		}
	}

	private string BuildVanillaPantsMemoryContext(NPC npc, string pantsName)
	{
		if (npc == null || string.IsNullOrWhiteSpace(pantsName))
		{
			return "";
		}
		int vanillaPantsSeenCount = GetVanillaPantsSeenCount(npc);
		if (vanillaPantsSeenCount <= 0)
		{
			return "";
		}
		string currentGameLanguageForPrompt = GetCurrentGameLanguageForPrompt();
		bool flag = currentGameLanguageForPrompt.Contains("pt", StringComparison.OrdinalIgnoreCase);
		return (vanillaPantsSeenCount != 1) ? (flag ? $"Este NPC já viu a(o) jogadora(o) usando {pantsName} antes ({vanillaPantsSeenCount} vezes). Devem reconhecer como algo já visto." : $"This NPC has seen the farmer wear {pantsName} before ({vanillaPantsSeenCount} times). They should recognize it as something they've seen.") : (flag ? ("Este NPC já viu a(o) jogadora(o) usando " + pantsName + " antes (1 vez). Pode reconhecer com familiaridade.") : ("This NPC has seen the farmer wear " + pantsName + " before (1 time). They may recognize it with familiarity."));
	}

	private string GetCurrentVanillaPantsName()
	{
		try
		{
			if (IsFashionSensePantsCoveringVanilla())
			{
				return "";
			}
			Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
			if (val == null)
			{
				return "";
			}
			return StringUtils.FirstNonEmpty(((Item)val).DisplayName, ((Item)val).Name) ?? "";
		}
		catch
		{
			return "";
		}
	}

	private string GetCurrentVanillaPantsDebugString()
	{
		try
		{
			Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
			if (val == null)
			{
				return "pantsItem=null";
			}
			return $"display='{((Item)val).DisplayName}' name='{((Item)val).Name}' itemId='{((Item)val).ItemId}' qid='{((Item)val).QualifiedItemId}' visibleName='{GetCurrentVanillaPantsName()}'";
		}
		catch (Exception ex)
		{
			return "error=" + ex.GetType().Name + ":" + ex.Message;
		}
	}

	private List<string> GetVanillaPantsSpecialItemCandidatesFromName(string displayName)
	{
		List<string> list = new List<string>();
		AddSpecialItemCandidate(list, displayName);
		return list;
	}

	private List<string> GetCurrentVanillaPantsSpecialItemCandidates(string displayName)
	{
		List<string> vanillaPantsSpecialItemCandidatesFromName = GetVanillaPantsSpecialItemCandidatesFromName(displayName);
		try
		{
			Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
			AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).DisplayName : null);
			AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).Name : null);
			AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).ItemId : null);
			AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).QualifiedItemId : null);
		}
		catch
		{
		}
		return vanillaPantsSpecialItemCandidatesFromName;
	}

	private List<string> GetCurrentVisibleVanillaHatSpecialItemCandidates(string displayName)
	{
		List<string> list = new List<string>();
		AddSpecialItemCandidate(list, displayName);
		try
		{
			Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
			AddSpecialItemCandidate(list, (val != null) ? ((Item)val).DisplayName : null);
			AddSpecialItemCandidate(list, (val != null) ? ((Item)val).Name : null);
			AddSpecialItemCandidate(list, (val != null) ? ((Item)val).ItemId : null);
			AddSpecialItemCandidate(list, (val != null) ? ((Item)val).QualifiedItemId : null);
		}
		catch
		{
		}
		return list;
	}

	private static void AddSpecialItemCandidate(List<string> candidates, string value)
	{
		if (candidates == null || string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		string text = value.Trim();
		foreach (string candidate in candidates)
		{
			if (candidate.Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		candidates.Add(text);
	}

	private static string FormatSpecialItemCandidates(IEnumerable<string> candidates)
	{
		if (candidates == null)
		{
			return "<null>";
		}
		List<string> list = new List<string>();
		foreach (string candidate in candidates)
		{
			if (!string.IsNullOrWhiteSpace(candidate))
			{
				list.Add(candidate.Trim());
			}
		}
		return (list.Count == 0) ? "<empty>" : string.Join(", ", list);
	}

	private static List<string> CloneSpecialItemCandidates(IEnumerable<string> candidates)
	{
		List<string> list = new List<string>();
		if (candidates == null)
		{
			return list;
		}
		foreach (string candidate in candidates)
		{
			AddSpecialItemCandidate(list, candidate);
		}
		return list;
	}

	private void LogSpecialItemDebugOnce(string key, string message)
	{
		if (DebugLog)
		{
			string item = key ?? "";
			if (loggedSpecialItemDebugKeys.Add(item))
			{
				((Mod)this).Monitor.Log("[SPECIAL ITEM DEBUG] " + message, (LogLevel)2);
			}
		}
	}

	private static string DescribeSpecialItemNotice(SpecialItemNoticeInfo notice)
	{
		if (notice == null)
		{
			return "<none>";
		}
		return $"entry='{notice.EntryId}' type='{notice.ItemType}' display='{notice.DisplayName}' matched='{notice.MatchedName}' removed={notice.WasRemoved} valid={notice.IsValid}";
	}

	private bool TryResolveSpecialItemCandidates(IEnumerable<string> candidateNames, string itemType, NPC npc, bool wasRemoved, out SpecialItemNoticeInfo notice)
	{
		notice = null;
		if (specialItemReactionService == null)
		{
			return false;
		}
		if (!specialItemReactionService.TryResolveItem(candidateNames, itemType, npc, GetCurrentGameLanguageForPrompt(), out var resolved, wasRemoved) || resolved == null || string.IsNullOrWhiteSpace(resolved.EntryId) || string.IsNullOrWhiteSpace(resolved.ReactionContext))
		{
			return false;
		}
		notice = new SpecialItemNoticeInfo
		{
			EntryId = resolved.EntryId,
			DisplayName = resolved.DisplayName,
			ItemType = (StringUtils.FirstNonEmpty(resolved.ItemType, itemType) ?? itemType ?? ""),
			MatchedName = resolved.MatchedName,
			ReactionContext = resolved.ReactionContext,
			WasRemoved = wasRemoved,
			HasSecret = resolved.HasSecret,
			SecretId = (resolved.SecretId ?? "")
		};
		notice.MemoryHint = BuildSpecialItemMemoryContext(npc, notice);
		return true;
	}

	private bool TryResolveSpecialItemNoticeForNpc(NPC npc, FashionSenseChangeInfo changeInfo, bool requireNpcMemoryForRemoval, out SpecialItemNoticeInfo notice)
	{
		notice = null;
		if (changeInfo == null || specialItemReactionService == null)
		{
			return false;
		}
		string text = ((npc != null) ? ((Character)npc).Name : null) ?? "<none>";
		string currentVanillaPantsName = GetCurrentVanillaPantsName();
		if (!string.IsNullOrWhiteSpace(currentVanillaPantsName))
		{
			List<string> currentVanillaPantsSpecialItemCandidates = GetCurrentVanillaPantsSpecialItemCandidates(currentVanillaPantsName);
			if (TryResolveSpecialItemCandidates(currentVanillaPantsSpecialItemCandidates, "Pants", npc, wasRemoved: false, out notice))
			{
				if (changeInfo.VanillaPantsChanged)
				{
					LogSpecialItemDebugOnce($"current-pants|{text}|{notice.EntryId}|{currentVanillaPantsName}", $"{text}: CURRENT visible pants matched {DescribeSpecialItemNotice(notice)} | current='{currentVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(currentVanillaPantsSpecialItemCandidates)}]");
				}
				return true;
			}
		}
		if (changeInfo.VanillaPantsChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaPantsName))
		{
			List<string> list = CloneSpecialItemCandidates(changeInfo.PreviousVanillaPantsSpecialItemCandidates);
			AddSpecialItemCandidate(list, changeInfo.PreviousVanillaPantsName);
			if (TryResolveSpecialItemCandidates(list, "Pants", npc, wasRemoved: true, out notice))
			{
				bool flag = HasSpecialItemMemory(npc, notice);
				LogSpecialItemDebugOnce($"removed-pants|{text}|{notice.EntryId}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}|{flag}|{requireNpcMemoryForRemoval}", $"{text}: PREVIOUS pants matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(list)}] npcHasMemory={flag} requireMemory={requireNpcMemoryForRemoval}");
				if (requireNpcMemoryForRemoval && !flag)
				{
					LogSpecialItemDebugOnce("removed-pants-ignored|" + text + "|" + notice.EntryId, text + ": removed special item ignored because this NPC has no memory for it.");
					notice = null;
					return false;
				}
				return true;
			}
			LogSpecialItemDebugOnce($"previous-pants-no-match|{text}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}", $"{text}: previous pants did NOT match a special item | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(list)}]");
		}
		string visibleVanillaHatId = GetVisibleVanillaHatId();
		if (!string.IsNullOrWhiteSpace(visibleVanillaHatId))
		{
			string currentVanillaHatName = GetCurrentVanillaHatName();
			if (TryResolveSpecialItemCandidates(GetCurrentVisibleVanillaHatSpecialItemCandidates(currentVanillaHatName), "Hat", npc, wasRemoved: false, out notice))
			{
				LogSpecialItemDebugOnce($"current-hat|{text}|{notice.EntryId}|{visibleVanillaHatId}|{currentVanillaHatName}", $"{text}: CURRENT visible vanilla hat matched {DescribeSpecialItemNotice(notice)} | hatId='{visibleVanillaHatId}' hatName='{currentVanillaHatName}'");
				return true;
			}
		}
		if (changeInfo.VanillaHatChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaHatId))
		{
			List<string> list2 = CloneSpecialItemCandidates(changeInfo.PreviousVanillaHatSpecialItemCandidates);
			AddSpecialItemCandidate(list2, changeInfo.PreviousVanillaHatId);
			if (TryResolveSpecialItemCandidates(list2, "Hat", npc, wasRemoved: true, out notice))
			{
				bool flag2 = HasSpecialItemMemory(npc, notice);
				LogSpecialItemDebugOnce($"removed-hat|{text}|{notice.EntryId}|{changeInfo.PreviousVanillaHatId}|{changeInfo.NewVanillaHatId}|{flag2}|{requireNpcMemoryForRemoval}", $"{text}: PREVIOUS hat matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaHatId}' new='{changeInfo.NewVanillaHatId}' candidates=[{FormatSpecialItemCandidates(list2)}] npcHasMemory={flag2} requireMemory={requireNpcMemoryForRemoval}");
				if (requireNpcMemoryForRemoval && !flag2)
				{
					LogSpecialItemDebugOnce("removed-hat-ignored|" + text + "|" + notice.EntryId, text + ": removed special hat ignored because this NPC has no memory for it.");
					notice = null;
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private string GetSpecialItemMemoryKey(NPC npc, SpecialItemNoticeInfo notice)
	{
		if (npc == null || notice == null || string.IsNullOrWhiteSpace(notice.EntryId))
		{
			return "";
		}
		return "NatrollEXE.OutfitReactions/SpecialItemSeen/" + MakeSafeModDataPart(((Character)npc).Name ?? "unknown") + "/" + MakeSafeModDataPart(StringUtils.FirstNonEmpty(notice.ItemType, "Item")) + "/" + MakeSafeModDataPart(notice.EntryId);
	}

	private bool HasSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
	{
		if (npc == null || notice == null || Game1.player == null)
		{
			return false;
		}
		string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
		if (string.IsNullOrWhiteSpace(specialItemMemoryKey))
		{
			return false;
		}
		string s = default(string);
		int result;
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, out s) && int.TryParse(s, out result) && result > 0;
	}

	private int GetSpecialItemSeenCount(NPC npc, SpecialItemNoticeInfo notice)
	{
		if (npc == null || notice == null || Game1.player == null)
		{
			return 0;
		}
		string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
		if (string.IsNullOrWhiteSpace(specialItemMemoryKey))
		{
			return 0;
		}
		string s = default(string);
		int result;
		return (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, out s) && int.TryParse(s, out result)) ? Math.Max(0, result) : 0;
	}

	private string BuildSpecialItemMemoryContext(NPC npc, SpecialItemNoticeInfo notice)
	{
		int specialItemSeenCount = GetSpecialItemSeenCount(npc, notice);
		if (specialItemSeenCount <= 0 || notice == null)
		{
			return "";
		}
		string text = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? "this special item";
		return (specialItemSeenCount == 1) ? ("This NPC has seen the farmer wear " + text + " before (1 time). They may recognize it with familiarity.") : $"This NPC has seen the farmer wear {text} before ({specialItemSeenCount} times). They should recognize it as something they've seen before.";
	}

	private void RecordSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
	{
		if (npc == null || notice == null || !notice.IsValid || Game1.player == null)
		{
			return;
		}
		string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
		if (!string.IsNullOrWhiteSpace(specialItemMemoryKey))
		{
			int result = 0;
			string s = default(string);
			if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, out s))
			{
				int.TryParse(s, out result);
			}
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[specialItemMemoryKey] = (result + 1).ToString();
			LogSpecialItemDebugOnce($"record-memory|{specialItemMemoryKey}|{result + 1}", $"Recorded memory key='{specialItemMemoryKey}' oldCount={result} newCount={result + 1} notice={DescribeSpecialItemNotice(notice)}");
			string text = specialItemMemoryKey + "/Name";
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[text] = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? notice.EntryId;
		}
	}
}
