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
{	private bool TryGetCurrentSavedFashionSenseOutfitId(out string outfitId)
	{
		outfitId = null;
		if (fsApi == null)
		{
			return false;
		}
		try
		{
			KeyValuePair<bool, string> currentOutfitId = fsApi.GetCurrentOutfitId();
			if (!currentOutfitId.Key || string.IsNullOrWhiteSpace(currentOutfitId.Value))
			{
				return false;
			}
			outfitId = currentOutfitId.Value.Trim();
			return true;
		}
		catch (Exception ex)
		{
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[FS] Could not read current saved outfit from Fashion Sense API: " + ex.Message, (LogLevel)2);
			}
			return false;
		}
	}

	private string GetCurrentGameLanguageForPrompt()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (1 == 0)
		{
		}
		string result = ((int)currentLanguageCode - 1) switch
		{
			3 => "Brazilian Portuguese", 
			4 => "Spanish", 
			5 => "German", 
			7 => "French", 
			9 => "Italian", 
			0 => "Japanese", 
			8 => "Korean", 
			1 => "Russian", 
			10 => "Turkish", 
			2 => "Chinese", 
			11 => "Hungarian", 
			6 => "Thai", 
			_ => "English", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private OutfitAiContext BuildOutfitAiContext(NPC npc, bool isSpouseDialogue)
	{
		if (npc == null || lastFashionSenseChangeInfo == null)
		{
			return null;
		}
		FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
		if (effectiveFashionSenseChangeInfoForNpc == null)
		{
			return null;
		}
		string fashionSenseDialogueKey = GetFashionSenseDialogueKey(effectiveFashionSenseChangeInfoForNpc);
		if (string.IsNullOrWhiteSpace(fashionSenseDialogueKey))
		{
			return null;
		}
		string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
		bool flag = Config?.UseFsInternalIdAsHint ?? true;
		string safeOutfitHint = ((flag && !string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi)) ? BuildSafeOutfitNameHint(currentSavedFashionSenseOutfitIdForAi) : "");
		string fashionSenseChangeType = GetFashionSenseChangeType(effectiveFashionSenseChangeInfoForNpc);
		string fashionSenseChangedItemId = GetFashionSenseChangedItemId(effectiveFashionSenseChangeInfoForNpc, fashionSenseChangeType);
		string safeNoticedChangeHint = ((flag && !string.IsNullOrWhiteSpace(fashionSenseChangedItemId)) ? BuildSafeOutfitNameHint(fashionSenseChangedItemId) : "");
		GameLocation currentLocation = Game1.currentLocation;
		string locationName = ((currentLocation != null) ? currentLocation.NameOrUniqueName : "");
		string currentSeason = Game1.currentSeason;
		int timeOfDay = Game1.timeOfDay;
		Farmer player = Game1.player;
		string playerName = (((player != null) ? ((Character)player).Name : null) ?? "").Trim();
		string playerGender = (Game1.player.IsMale ? "male" : "female");
		string currentGameLanguageForPrompt = GetCurrentGameLanguageForPrompt();
		(string Status, int Hearts) relationshipDialogueContext = GetRelationshipDialogueContext(npc);
		string item = relationshipDialogueContext.Status;
		int item2 = relationshipDialogueContext.Hearts;
		string dialogueKey = fashionSenseDialogueKey;
		if (!string.IsNullOrWhiteSpace(fashionSenseDialogueKey))
		{
			bool flag2 = IsFarmHouseLocation(currentLocation);
			bool flag3 = currentLocation != null && currentLocation.IsOutdoors;
			bool flag4 = !flag2 && !flag3 && IsMarriageCandidateNpcRoom(npc, currentLocation);
			dialogueKey = (flag2 ? (fashionSenseDialogueKey + ".FarmHouse") : (flag4 ? (fashionSenseDialogueKey + ".NpcRoom") : (flag3 ? (fashionSenseDialogueKey + ".Outside") : (fashionSenseDialogueKey + ".Inside"))));
		}
		bool flag5 = IsFarmHouseLocation(currentLocation);
		bool flag6 = currentLocation != null && currentLocation.IsOutdoors;
		bool isNpcRoom = !flag5 && !flag6 && IsMarriageCandidateNpcRoom(npc, currentLocation);
		bool isNpcPersonalLocation = !flag5 && !flag6 && IsMarriageCandidatePersonalLocation(npc, currentLocation);
		bool isIndoors = currentLocation != null && !flag6;
		bool flag7 = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());
		bool flag8 = effectiveFashionSenseChangeInfoForNpc?.VanillaHatRemoved ?? false;
		OutfitVisionImage visionImage = ((flag7 || flag8) ? null : TryCaptureVisionOutfitImageForAi());
		string summary = TryBuildFashionSenseVisualSummaryForAi(effectiveFashionSenseChangeInfoForNpc);
		summary = MergeRenderedHairColorIntoSummary(summary, visionImage, effectiveFashionSenseChangeInfoForNpc);
		if (!flag7 && !flag8)
		{
			summary = MergeRenderedHatColorIntoSummary(summary, visionImage, effectiveFashionSenseChangeInfoForNpc);
		}
		string text = ((!flag8) ? "" : (hatMemoryService?.GetLastHatNameForNpc(((Character)npc).Name) ?? ""));
		bool flag9 = flag8 && !string.IsNullOrWhiteSpace(text);
		string vanillaHatFraming = "";
		if (flag9)
		{
			string text2 = " The farmer just took off the hat they had been wearing and is now bare-headed; react to them having removed it. Do not describe or invent a color for the hat (it is no longer worn).";
			summary = (summary ?? "").TrimEnd() + text2;
			vanillaHatFraming = text2.Trim();
		}
		string vanillaHatMemoryHint = BuildVanillaHatMemoryContext(npc);
		string specialHatReactionContext = (flag9 ? (specialHatReactionService?.BuildContextForRemovedHat(text, currentGameLanguageForPrompt) ?? "") : ((!(!flag8 && flag7)) ? "" : (specialHatReactionService?.BuildContextForCurrentVanillaHat(Game1.player, currentGameLanguageForPrompt) ?? "")));
		SpecialItemNoticeInfo notice;
		bool flag10 = TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: true, out notice);
		string specialItemReactionContext = ((!flag10) ? "" : (notice?.ReactionContext ?? ""));
		if (flag10 && !string.IsNullOrWhiteSpace(specialItemReactionContext))
		{
			// A content-pack/special-item definition has higher priority than the built-in
			// vanilla-hat catalog, so never send two competing reaction descriptions.
			specialHatReactionContext = "";
		}
		bool flag11 = flag10 && (notice?.WasRemoved ?? false);
		string text3 = ((!flag10) ? "" : (notice?.MemoryHint ?? ""));
		if (flag10 && DebugLog)
		{
			((Mod)this).Monitor.Log($"[SPECIAL ITEM PROMPT] NPC={((Character)npc).Name} entry='{notice?.EntryId}' removed={flag11} memoryHint='{(string.IsNullOrWhiteSpace(text3) ? "<none>" : text3)}' combinedMode={ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined"}", (LogLevel)2);
		}
		string vanillaPantsMemoryHint = "";
		return new OutfitAiContext
		{
			NpcName = ((Character)npc).Name,
			NpcDisplayName = ((Character)npc).displayName,
			IsSpouse = isSpouseDialogue,
			DialogueKey = dialogueKey,
			OutfitName = currentSavedFashionSenseOutfitIdForAi,
			SafeOutfitHint = safeOutfitHint,
			ThemeContext = "",
			ThemePriorityInstruction = "",
			LocationName = locationName,
			DetailedLocationName = GetDetailedLocationNameForAiPrompt(currentLocation),
			LocationType = GetLocationTypeForAiPrompt(currentLocation, flag5, flag6, isNpcRoom),
			IsOutdoors = flag6,
			IsIndoors = isIndoors,
			IsNpcRoom = isNpcRoom,
			IsNpcPersonalLocation = isNpcPersonalLocation,
			IsBeachOrIsland = IsBeachOrIslandLocation(currentLocation),
			IsFarmHouse = flag5,
			DayPart = GetDayPartForAiPrompt(timeOfDay),
			FestivalContext = GetFestivalContextForAiPrompt(),
			FarmerBirthdayContext = GetFarmerBirthdayContextForAiPrompt(),
			Season = currentSeason,
			Weather = GetCurrentWeatherForAiPrompt(currentLocation),
			Time = timeOfDay,
			DayOfSeason = Game1.dayOfMonth,
			Year = Game1.year,
			PlayerName = playerName,
			PlayerGender = playerGender,
			TargetLanguage = currentGameLanguageForPrompt,
			RelationshipStatus = item,
			RelationshipHearts = item2,
			VisionImage = visionImage,
			FashionSenseVisualSummary = summary,
			VanillaHatHatOnlyMode = (!flag10 && (flag7 || flag9) && ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) == "HatOnly"),
			VanillaHatFraming = vanillaHatFraming,
			NpcWitnessedPreviousAccessory = DidNpcWitnessPreviousLook(npc),
			SpecialHatReactionContext = specialHatReactionContext,
			SpecialItemReactionContext = specialItemReactionContext,
			SpecialItemWasJustRemoved = flag11,
			SpecialItemOnlyMode = flag10,
			SpecialItemCombinedMode = (flag10 && ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined"),
			SpecialItemMemoryHint = text3,
			VanillaPantsMemoryHint = vanillaPantsMemoryHint,
			VanillaHatMemoryHint = vanillaHatMemoryHint,
			AvailablePortraitCount = GetNpcPortraitCount(npc),
			NoticedChangeType = fashionSenseChangeType,
			NoticedChangeName = fashionSenseChangedItemId,
			SafeNoticedChangeHint = safeNoticedChangeHint,
			WasCaughtPeeking = (!isSpouseDialogue && (otherNpcClothesReactionSystem?.WasNpcCaughtPeeking(npc) ?? false)),
			OutfitMemoryContext = BuildOutfitMemoryContext(npc, currentSavedFashionSenseOutfitIdForAi)
		};
	}

	private string BuildOutfitMemoryContext(NPC npc, string outfitId)
	{
		if (outfitMemoryService == null || npc == null)
		{
			return null;
		}
		string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
		if (string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
		{
			return null;
		}
		OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
		OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
		if (memory == null)
		{
			return null;
		}
		return outfitMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
	}

	private void RecordOutfitMemory(NPC npc, string outfitId)
	{
		if (outfitMemoryService != null && npc != null)
		{
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
			if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
			{
				OutfitComponents components = BuildCurrentOutfitComponentsForMemory();
				outfitMemoryService.RecordMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, components, Game1.currentSeason, Game1.dayOfMonth, Game1.year);
			}
		}
	}

	private string GetCurrentSavedFashionSenseOutfitIdForAi(string fallbackOutfitId = null)
	{
		if (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) && !string.IsNullOrWhiteSpace(outfitId))
		{
			return outfitId;
		}
		return fallbackOutfitId ?? "";
	}

	private OutfitComponents BuildCurrentOutfitComponentsForMemory()
	{
		FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
		if (fashionSenseSnapshot != null)
		{
			string hat = ((!string.IsNullOrWhiteSpace(fashionSenseSnapshot.VanillaHat)) ? ("vanilla:" + fashionSenseSnapshot.VanillaHat) : (fashionSenseSnapshot.Hat ?? ""));
			return new OutfitComponents
			{
				Hat = hat,
				Hair = (fashionSenseSnapshot.Hair ?? ""),
				Shirt = (fashionSenseSnapshot.Shirt ?? ""),
				Pants = (fashionSenseSnapshot.Pants ?? ""),
				Sleeves = (fashionSenseSnapshot.Sleeves ?? ""),
				Accessory = BuildCurrentAccessoryMemoryValue(fashionSenseSnapshot)
			};
		}
		return new OutfitComponents
		{
			Hat = (lastFashionSenseChangeInfo?.NewHatId ?? ""),
			Hair = (lastFashionSenseChangeInfo?.NewHairId ?? ""),
			Shirt = (lastFashionSenseChangeInfo?.NewShirtId ?? ""),
			Pants = (lastFashionSenseChangeInfo?.NewPantsId ?? ""),
			Sleeves = (lastFashionSenseChangeInfo?.NewSleevesId ?? ""),
			Accessory = (lastFashionSenseChangeInfo?.NewAccessoryId ?? "")
		};
	}

	private static string BuildCurrentAccessoryMemoryValue(FashionSenseSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return "";
		}
		return string.Join(" + ", new string[3]
		{
			NormalizeFashionSenseAccessoryId(snapshot.Accessory),
			NormalizeFashionSenseAccessoryId(snapshot.AccessorySecondary),
			NormalizeFashionSenseAccessoryId(snapshot.AccessoryTertiary)
		}.Where((string value) => !string.IsNullOrWhiteSpace(value)));
	}

	private static string NormalizeFashionSenseAccessoryId(string accessoryId)
	{
		if (string.IsNullOrWhiteSpace(accessoryId))
		{
			return "";
		}
		string text = accessoryId.Trim();
		return IsIgnoredFashionSenseAccessoryId(text) ? "" : text;
	}

	private static bool IsIgnoredFashionSenseAccessoryId(string accessoryId)
	{
		if (string.IsNullOrWhiteSpace(accessoryId))
		{
			return false;
		}
		string text = FashionSenseVisualService.HumanizeAppearanceId(accessoryId);
		string text2 = " " + string.Join(" ", accessoryId, text).ToLowerInvariant().Replace('_', ' ')
			.Replace('-', ' ')
			.Replace('.', ' ')
			.Replace('/', ' ') + " ";
		bool flag = (text2.Contains(" eye ") || text2.Contains(" eyes ") || text2.Contains(" olho ") || text2.Contains(" olhos ")) && (text2.Contains(" highlight ") || text2.Contains(" highlights ") || text2.Contains(" sparkle ") || text2.Contains(" sparkles ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho ") || text2.Contains(" brilhos "));
		bool flag2 = (text2.Contains(" face ") || text2.Contains(" facial ") || text2.Contains(" rosto ")) && (text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" highlight ") || text2.Contains(" blush ") || text2.Contains(" sparkle ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho "));
		return flag || flag2 || text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" blush ") || text2.Contains(" lipstick ") || text2.Contains(" batom ") || text2.Contains(" eyeshadow ") || text2.Contains(" eye shadow ") || text2.Contains(" sombra ") || text2.Contains(" eyeliner ") || text2.Contains(" delineador ") || text2.Contains(" rimel ") || text2.Contains(" rímel ");
	}

	private string GetFashionSenseChangeType(FashionSenseChangeInfo changeInfo)
	{
		if (changeInfo == null)
		{
			return "";
		}
		bool flag = AreVisionOnlyFashionSenseTriggersEnabled();
		if (changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId))
		{
			if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
			{
				return "Accessory";
			}
			return "Outfit";
		}
		if (ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
		{
			return "Outfit";
		}
		if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
		{
			return "Accessory";
		}
		if (changeInfo.VanillaHatChanged)
		{
			return "Hat";
		}
		if (changeInfo.VanillaShirtChanged || changeInfo.ChangedShirt)
		{
			return "Shirt";
		}
		if (changeInfo.VanillaPantsChanged || changeInfo.ChangedPants)
		{
			return "Pants";
		}
		if (changeInfo.VanillaShoesChanged || changeInfo.ChangedShoes)
		{
			return "Shoes";
		}
		if (changeInfo.ChangedHat && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId) && (flag || ItemNameRevealsShape(changeInfo.NewHatId)))
		{
			return "Hat";
		}
		if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
		{
			return "Hair";
		}
		return "";
	}

	private bool ShouldTreatAccessoryAsCurrentComboFocus(string accessoryId, bool visionOn)
	{
		if (string.IsNullOrWhiteSpace(accessoryId))
		{
			return false;
		}
		if (IsIgnoredFashionSenseAccessoryId(accessoryId))
		{
			return false;
		}
		if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(accessoryId))
		{
			return false;
		}
		return visionOn || ItemNameRevealsShape(accessoryId);
	}

	private bool ShouldTreatGenericHeadwearAsSavedOutfitPart(FashionSenseChangeInfo changeInfo)
	{
		if (changeInfo == null || !changeInfo.ChangedHat)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(changeInfo.NewHatId))
		{
			return false;
		}
		if (!FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId))
		{
			return false;
		}
		string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(changeInfo.NewOutfitId);
		return !string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi);
	}

	private static string GetFashionSenseChangedItemId(FashionSenseChangeInfo changeInfo, string changeType)
	{
		if (changeInfo == null)
		{
			return "";
		}
		if (string.Equals(changeType, "Outfit", StringComparison.OrdinalIgnoreCase))
		{
			return changeInfo.NewOutfitId ?? "";
		}
		if (string.Equals(changeType, "Hair", StringComparison.OrdinalIgnoreCase))
		{
			return changeInfo.NewHairId ?? "";
		}
		if (string.Equals(changeType, "Hat", StringComparison.OrdinalIgnoreCase))
		{
			return changeInfo.NewHatId ?? "";
		}
		if (string.Equals(changeType, "Accessory", StringComparison.OrdinalIgnoreCase))
		{
			return StringUtils.FirstNonEmpty(changeInfo.NewAccessoryId, "unknown-accessory-change") ?? "";
		}
		if (string.Equals(changeType, "Shirt", StringComparison.OrdinalIgnoreCase))
		{
			return StringUtils.FirstNonEmpty(changeInfo.NewVanillaShirtName, changeInfo.NewShirtId) ?? "";
		}
		if (string.Equals(changeType, "Pants", StringComparison.OrdinalIgnoreCase))
		{
			return StringUtils.FirstNonEmpty(changeInfo.NewVanillaPantsName, changeInfo.NewPantsId) ?? "";
		}
		if (string.Equals(changeType, "Shoes", StringComparison.OrdinalIgnoreCase))
		{
			return StringUtils.FirstNonEmpty(changeInfo.NewVanillaShoesName, changeInfo.NewShoesId) ?? "";
		}
		return "";
	}

	private string TryBuildFashionSenseVisualSummaryForAi(FashionSenseChangeInfo effectiveChangeInfo)
	{
		if (fashionSenseVisualService == null || Game1.player == null)
		{
			return "";
		}
		FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
		string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(fashionSenseChangeInfo?.NewOutfitId);
		bool suppressHairAndGenericHeadwearForSavedOutfit = fashionSenseChangeInfo != null && (fashionSenseChangeInfo.ChangedOutfit || ShouldTreatGenericHeadwearAsSavedOutfitPart(fashionSenseChangeInfo));
		bool visibleVanillaHatEquipped = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());
		if (fashionSenseVisualService.TryBuildVisualSummary(Game1.player, currentSavedFashionSenseOutfitIdForAi, out var summary, out var reason, suppressHairAndGenericHeadwearForSavedOutfit, visibleVanillaHatEquipped))
		{
			string playerProvidedAccessoryDescriptionForCurrentChange = GetPlayerProvidedAccessoryDescriptionForCurrentChange(fashionSenseChangeInfo);
			if (!string.IsNullOrWhiteSpace(playerProvidedAccessoryDescriptionForCurrentChange))
			{
				summary = summary + "; player-provided description for the current small accessory/change: " + playerProvidedAccessoryDescriptionForCurrentChange;
			}
			return summary;
		}
		((Mod)this).Monitor.Log(" Vision outfit analysis is enabled, but Fashion Sense API visual support data could not be read: " + reason, (LogLevel)0);
		string playerProvidedAccessoryDescriptionForCurrentChange2 = GetPlayerProvidedAccessoryDescriptionForCurrentChange(fashionSenseChangeInfo);
		return string.IsNullOrWhiteSpace(playerProvidedAccessoryDescriptionForCurrentChange2) ? "" : ("Player-provided description for the current small accessory/change: " + playerProvidedAccessoryDescriptionForCurrentChange2);
	}

	private string MergeRenderedHairColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
	{
		FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
		bool flag = fashionSenseChangeInfo?.ChangedHair ?? false;
		bool flag2 = fashionSenseChangeInfo != null && !string.IsNullOrWhiteSpace(fashionSenseChangeInfo.NewHatId);
		if (!flag || flag2)
		{
			return summary;
		}
		if (visionImage == null || !visionImage.HasHairColor || string.IsNullOrWhiteSpace(visionImage.HairColorName))
		{
			return summary;
		}
		if (!string.IsNullOrWhiteSpace(summary) && summary.IndexOf("CONFIRMED hair color", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return summary;
		}
		string text = "CONFIRMED hair color from the rendered sprite (authoritative, use exactly this; do NOT take hair color from the raw image): " + visionImage.HairColorName + " (" + visionImage.HairColorHex + ")";
		if (string.IsNullOrWhiteSpace(summary))
		{
			return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: " + text;
		}
		return summary + "; " + text;
	}

	private string MergeRenderedHatColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
	{
		FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
		if (fashionSenseChangeInfo == null || !fashionSenseChangeInfo.ChangedHat || fashionSenseChangeInfo.ChangedOutfit || ShouldTreatGenericHeadwearAsSavedOutfitPart(fashionSenseChangeInfo) || FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(fashionSenseChangeInfo.NewHatId) || string.IsNullOrWhiteSpace(fashionSenseChangeInfo.NewHatId))
		{
			return summary;
		}
		if (visionImage == null || !visionImage.HasHatColor || string.IsNullOrWhiteSpace(visionImage.HatColorName))
		{
			return summary;
		}
		if (!string.IsNullOrWhiteSpace(summary) && summary.IndexOf("CONFIRMED hat color", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return summary;
		}
		string text = "CONFIRMED hat/headwear color from the rendered sprite (authoritative, use exactly this; do NOT take hat color from the raw image): " + visionImage.HatColorName + " (" + visionImage.HatColorHex + ")";
		if (string.IsNullOrWhiteSpace(summary))
		{
			return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: " + text;
		}
		return summary + "; " + text;
	}

	private string GetPlayerProvidedAccessoryDescriptionForCurrentChange(FashionSenseChangeInfo effectiveChangeInfo = null)
	{
		FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
		if (Game1.player == null || fashionSenseChangeInfo == null)
		{
			return "";
		}
		string text = fashionSenseChangeInfo.NewAccessoryId ?? "";
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		string playerAccessoryDescriptionModDataKey = GetPlayerAccessoryDescriptionModDataKey(text);
		string text2 = default(string);
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(playerAccessoryDescriptionModDataKey, out text2) ? CleanPlayerOutfitReplyText(text2) : "";
	}

	private void SavePlayerProvidedAccessoryDescriptionForCurrentChange(string description)
	{
		if (Game1.player == null || lastFashionSenseChangeInfo == null || !lastFashionSenseChangeInfo.ChangedAccessory || string.IsNullOrWhiteSpace(lastFashionSenseChangeInfo.NewAccessoryId))
		{
			return;
		}
		string text = CleanPlayerOutfitReplyText(description);
		if (!string.IsNullOrWhiteSpace(text))
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[GetPlayerAccessoryDescriptionModDataKey(lastFashionSenseChangeInfo.NewAccessoryId)] = text;
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[FS VISUAL] Saved player-provided description for a small accessory/change.", (LogLevel)2);
			}
		}
	}

	private static string GetPlayerAccessoryDescriptionModDataKey(string accessoryId)
	{
		return "NatrollEXE.OutfitReactions.PlayerAccessoryDescription." + GetStableHexHash(accessoryId ?? "");
	}

	private OutfitVisionImage TryCaptureVisionOutfitImageForAi()
	{
		if (outfitVisionService == null || Game1.player == null)
		{
			return null;
		}
		if (!outfitVisionService.TryCaptureFarmerAppearance(Game1.player, out var image, out var reason))
		{
			((Mod)this).Monitor.Log(" Could not render the farmer sprite for color reading: " + reason, (LogLevel)0);
			return null;
		}
		if (!ShouldTryVisionForCurrentAiProvider() && image != null)
		{
			image.Base64Data = "";
		}
		return image;
	}

	private bool ShouldTryVisionForCurrentAiProvider()
	{
		return Config?.ShouldSendImageToActiveModel() ?? false;
	}

	private string GetDetailedLocationNameForAiPrompt(GameLocation location)
	{
		if (location == null)
		{
			return "unknown";
		}
		string text = location.NameOrUniqueName ?? location.Name ?? "unknown";
		string text2 = text;
		try
		{
			if (((object)location).GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(location) is string text3 && !string.IsNullOrWhiteSpace(text3))
			{
				text2 = text3;
			}
		}
		catch
		{
		}
		return string.Equals(text2, text, StringComparison.OrdinalIgnoreCase) ? text : (text2 + " (internal map: " + text + ")");
	}

	private string GetLocationTypeForAiPrompt(GameLocation location, bool isFarmHouse, bool isOutdoors, bool isNpcRoom)
	{
		if (location == null)
		{
			return "unknown";
		}
		if (isFarmHouse)
		{
			return "farmer farmhouse / home interior";
		}
		if (isNpcRoom)
		{
			return "marriage-candidate NPC room";
		}
		if (location != null && !isFarmHouse && !isOutdoors)
		{
			string text = (location.NameOrUniqueName ?? location.Name ?? "").ToLowerInvariant();
			if (text.Contains("house") || text.Contains("home") || text.Contains("shop") || text.Contains("trailer") || text.Contains("room") || text.Contains("basement"))
			{
				return "marriage-candidate home/private interior";
			}
		}
		if (isOutdoors)
		{
			return "outdoors";
		}
		return "indoors";
	}

	private string GetDayPartForAiPrompt(int time)
	{
		if (time >= 2400)
		{
			return "late night / after midnight";
		}
		if (time < 700)
		{
			return "dawn / very early morning";
		}
		if (time < 900)
		{
			return "early morning";
		}
		if (time < 1200)
		{
			return "morning";
		}
		if (time < 1400)
		{
			return "midday / early afternoon";
		}
		if (time < 1800)
		{
			return "afternoon";
		}
		if (time < 2100)
		{
			return "evening";
		}
		return "night";
	}

	private string GetFarmerBirthdayContextForAiPrompt()
	{
		string text = (Config.FarmerBirthdaySeason ?? "").Trim();
		int farmerBirthdayDay = Config.FarmerBirthdayDay;
		if (string.IsNullOrWhiteSpace(text) || farmerBirthdayDay <= 0)
		{
			return "Farmer birthday is not configured.";
		}
		return (farmerBirthdayDay == Game1.dayOfMonth && text.Equals(Game1.currentSeason, StringComparison.OrdinalIgnoreCase)) ? "Today is the farmer's birthday. The compliment may feel a little more special if it fits the NPC and relationship." : ("Today is not the farmer's birthday. Farmer birthday is configured as " + text + " " + farmerBirthdayDay + ".");
	}

	private string GetCurrentWeatherForAiPrompt(GameLocation location)
	{
		return Game1.IsGreenRainingHere(location) ? "green rain" : (Game1.IsLightningHere(location) ? "storm / thunderstorm" : (Game1.IsRainingHere(location) ? "rain" : (Game1.IsSnowingHere(location) ? "snow" : (Game1.IsDebrisWeatherHere(location) ? "windy / debris weather" : "sunny / clear"))));
	}

	private static string BuildSafeOutfitNameHint(string rawName)
	{
		if (string.IsNullOrWhiteSpace(rawName))
		{
			return "No readable saved outfit name was provided.";
		}
		string input = rawName.Trim();
		input = Regex.Replace(input, "[_\\-.]+", " ");
		input = Regex.Replace(input, "([a-zà-ÿ])([A-Z])", "$1 $2");
		input = Regex.Replace(input, "\\s*(?:#|n[ºo]?\\.?|v|ver|version|versao|versão|set)?\\s*\\d+\\s*$", "", RegexOptions.IgnoreCase);
		input = Regex.Replace(input, "\\s{2,}", " ").Trim();
		if (string.IsNullOrWhiteSpace(input))
		{
			return "The saved outfit name is only an internal label and does not provide a readable theme.";
		}
		return input;
	}
}
