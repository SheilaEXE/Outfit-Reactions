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

public sealed class ModEntry : Mod
{
	[HarmonyPatch]
	private static class NPCCheckActionPatch
	{
		private static bool firstRunLogged;

		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(NPC), "checkAction", new Type[2]
			{
				typeof(Farmer),
				typeof(GameLocation)
			});
		}

		[HarmonyPriority(800)]
		private static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
		{
			try
			{
				if (!firstRunLogged)
				{
					firstRunLogged = true;
					if (DebugLog)
					{
						ModEntry instance = Instance;
						if (instance != null)
						{
							IMonitor monitor = ((Mod)instance).Monitor;
							if (monitor != null)
							{
								monitor.Log("[CLOTHES PRIORITY] NPC.checkAction prefix ran for the first time.", (LogLevel)2);
							}
						}
					}
				}
				ModEntry instance2 = Instance;
				if (instance2 != null && instance2.TryHandleOutfitDialogueOrBlockNpcInteraction(__instance))
				{
					__result = true;
					return false;
				}
			}
			catch (Exception ex)
			{
				ModEntry instance3 = Instance;
				if (instance3 != null)
				{
					IMonitor monitor2 = ((Mod)instance3).Monitor;
					if (monitor2 != null)
					{
						monitor2.Log("[CLOTHES PRIORITY] Error while prioritizing/blocking outfit dialogue before NPC.checkAction: " + ex, (LogLevel)3);
					}
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(NPC), "doMiddleAnimation")]
	private static class NPCDoMiddleAnimationPatch
	{
		private static bool Prefix(NPC __instance)
		{
			try
			{
				if (__instance == null || Instance?.otherNpcClothesReactionSystem == null)
				{
					return true;
				}
				if (!Instance.otherNpcClothesReactionSystem.IsHeldForFishingSpecialAction(__instance))
				{
					return true;
				}
				return false;
			}
			catch (Exception value)
			{
				ModEntry instance = Instance;
				if (instance != null)
				{
					IMonitor monitor = ((Mod)instance).Monitor;
					if (monitor != null)
					{
						monitor.Log($"[NPC OUTFIT] Error suppressing doMiddleAnimation: {value}", (LogLevel)3);
					}
				}
				return true;
			}
		}
	}

	private sealed class FashionSenseChangeInfo
	{
		public bool ChangedHair;

		public bool ChangedAccessory;

		public bool ChangedHat;

		public bool ChangedShirt;

		public bool ChangedPants;

		public bool ChangedSleeves;

		public bool ChangedShoes;

		public bool ChangedOutfit;

		public string NewHairId;

		public string NewAccessoryId;

		public string NewHatId;

		public string NewVanillaHatId;

		public bool VanillaHatChanged;

		public bool VanillaHatRemoved;

		public string PreviousVanillaHatId;

		public List<string> PreviousVanillaHatSpecialItemCandidates = new List<string>();

		public bool VanillaPantsChanged;

		public bool VanillaPantsRemoved;

		public string PreviousVanillaPantsName;

		public string NewVanillaPantsName;

		public List<string> PreviousVanillaPantsSpecialItemCandidates = new List<string>();

		public List<string> NewVanillaPantsSpecialItemCandidates = new List<string>();

		public string NewShirtId;

		public string NewPantsId;

		public string NewSleevesId;

		public string NewShoesId;

		public string NewOutfitId;

		public int CountChanges()
		{
			int num = 0;
			if (ChangedHair)
			{
				num++;
			}
			if (ChangedAccessory)
			{
				num++;
			}
			if (ChangedHat)
			{
				num++;
			}
			if (ChangedShirt)
			{
				num++;
			}
			if (ChangedPants)
			{
				num++;
			}
			if (VanillaPantsChanged)
			{
				num++;
			}
			if (ChangedSleeves)
			{
				num++;
			}
			if (ChangedOutfit)
			{
				num++;
			}
			return num;
		}
	}

	private sealed class FashionSenseSnapshot
	{
		public string Hair;

		public string Accessory;

		public string AccessorySecondary;

		public string AccessoryTertiary;

		public string Hat;

		public bool FashionSenseHatCoversVanilla;

		public string VanillaHat;

		public List<string> VanillaHatSpecialItemCandidates = new List<string>();

		public string VanillaPants;

		public List<string> VanillaPantsSpecialItemCandidates = new List<string>();

		public string Shirt;

		public string Pants;

		public string Sleeves;

		public string Shoes;

		public string OutfitId;

		public string HairColor;

		public string AccessoryColor;

		public string AccessorySecondaryColor;

		public string AccessoryTertiaryColor;

		public string HatColor;

		public string ShirtColor;

		public string PantsColor;

		public string SleevesColor;

		public string ShoesColor;
	}

	private sealed class SpecialItemNoticeInfo
	{
		public string EntryId { get; set; } = "";

		public string DisplayName { get; set; } = "";

		public string ItemType { get; set; } = "";

		public string MatchedName { get; set; } = "";

		public string ReactionContext { get; set; } = "";

		public bool WasRemoved { get; set; }

		public string MemoryHint { get; set; } = "";

		public bool HasSecret { get; set; }

		public string SecretId { get; set; } = "";

		public bool IsValid => !string.IsNullOrWhiteSpace(EntryId) && !string.IsNullOrWhiteSpace(ReactionContext);
	}

	private readonly Random random = new Random();

	private Harmony harmony;

	private IFashionSenseApi fsApi;

	private OtherNpcClothesReactionSystem otherNpcClothesReactionSystem;

	private OutfitAiService outfitAiService;

	private OutfitMemoryService outfitMemoryService;

	private HatMemoryService hatMemoryService;

	private string lastKnownVanillaHatId;

	private List<string> lastKnownVanillaHatSpecialItemCandidates = new List<string>();

	private string lastKnownVanillaPantsName;

	private List<string> lastKnownVanillaPantsSpecialItemCandidates = new List<string>();

	private readonly HashSet<string> loggedSpecialItemDebugKeys = new HashSet<string>(StringComparer.Ordinal);

	private bool vanillaClothingTrackingInitialized;

	private int vanillaClothingPollTimer;

	private OutfitVisionService outfitVisionService;

	private FashionSenseVisualService fashionSenseVisualService;

	private SpecialHatReactionService specialHatReactionService;

	private SpecialItemReactionService specialItemReactionService;

	private bool changedClothes = false;

	private string lastEligibleSavedOutfitId = "";

	internal const string ReactionActiveModDataKey = "NatrollEXE.OutfitReactions/ReactionActive";

	private const string AutoKissClickActiveModDataKey = "NatrollEXE.LotsOfKisses/AutoKissClickActive";

	private FashionSenseSnapshot fsSnapshotBefore = null;

	private bool fashionSenseMenuOpen = false;

	private FashionSenseChangeInfo lastFashionSenseChangeInfo = null;

	private readonly HashSet<string> npcsReactedToCurrentNotice = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly AiGenerationCoordinator aiGenerationCoordinator = new AiGenerationCoordinator();

	private readonly OutfitReplyConversationHistory outfitReplyConversationHistory = new OutfitReplyConversationHistory();

	private const string AssetPrefix = "Mods/NatrollEXE.OutfitReactions/Clothes";

	private const string OutfitNoticeModDataPrefix = "NatrollEXE.OutfitReactions.OutfitNotice.";

	private const string PlayerAccessoryDescriptionModDataPrefix = "NatrollEXE.OutfitReactions.PlayerAccessoryDescription.";

	private const string LotsOfKissesBystanderWatchingModDataKey = "NatrollEXE.LotsOfKisses/BystanderWatching";

	private readonly SpouseOutfitReactionProgressState spouseOutfitReactionProgressState = new SpouseOutfitReactionProgressState();

	private readonly SpouseDialogueController spouseDialogueController = new SpouseDialogueController();

	private SpouseOutfitReactionCoordinator spouseOutfitReactionCoordinator;

	private readonly SpouseRouteController spouseRouteController = new SpouseRouteController();

	private readonly SpouseOutfitApproachController spouseOutfitApproachController = new SpouseOutfitApproachController();

	private readonly SpouseOutfitNoticeController spouseOutfitNoticeController = new SpouseOutfitNoticeController();

	private readonly SpouseProximityState spouseProximityState = new SpouseProximityState();

	private readonly SpouseSpecialActionController spouseSpecialActionController = new SpouseSpecialActionController();

	private const float OutfitSpecialActionRestoreDistance = 300f;

	private const float SpouseOutfitNoticePauseDistance = 96f;

	private const float SpouseOutfitNoticeReleaseDistance = 300f;

	private const string PantsSeenModDataPrefix = "NatrollEXE.OutfitReactions/PantsSeen/";

	private const string SpecialItemSeenModDataPrefix = "NatrollEXE.OutfitReactions/SpecialItemSeen/";

	internal static ModEntry Instance { get; private set; }

	internal ModConfig Config { get; set; } = new ModConfig();

	internal static bool DebugLog => Instance?.Config?.EnableDebugLogging == true;

	private bool isReactingToClothes
	{
		get
		{
			return spouseOutfitReactionProgressState.IsReacting;
		}
		set
		{
			spouseOutfitReactionProgressState.IsReacting = value;
		}
	}

	private int clothesInteractionCooldown
	{
		get
		{
			return spouseOutfitReactionProgressState.InteractionCooldown;
		}
		set
		{
			spouseOutfitReactionProgressState.InteractionCooldown = value;
		}
	}

	private bool clothesPathStarted
	{
		get
		{
			return spouseOutfitReactionProgressState.PathStarted;
		}
		set
		{
			spouseOutfitReactionProgressState.PathStarted = value;
		}
	}

	private bool clothesComplimentReady
	{
		get
		{
			return spouseOutfitReactionProgressState.ComplimentReady;
		}
		set
		{
			spouseOutfitReactionProgressState.ComplimentReady = value;
		}
	}

	private Point clothesPreferredOffset
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return spouseOutfitReactionProgressState.PreferredOffset;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			spouseOutfitReactionProgressState.PreferredOffset = value;
		}
	}

	private Point clothesLastPlayerTile
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return spouseOutfitReactionProgressState.LastPlayerTile;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			spouseOutfitReactionProgressState.LastPlayerTile = value;
		}
	}

	private Point clothesLastTargetTile
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return spouseOutfitReactionProgressState.LastTargetTile;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			spouseOutfitReactionProgressState.LastTargetTile = value;
		}
	}

	private bool clothesFirstNoticeDone
	{
		get
		{
			return spouseOutfitReactionProgressState.FirstNoticeDone;
		}
		set
		{
			spouseOutfitReactionProgressState.FirstNoticeDone = value;
		}
	}

	private bool clothesEmoteFired
	{
		get
		{
			return spouseOutfitReactionProgressState.EmoteFired;
		}
		set
		{
			spouseOutfitReactionProgressState.EmoteFired = value;
		}
	}

	private int clothesNoticePauseTimer
	{
		get
		{
			return spouseOutfitReactionProgressState.NoticePauseTimer;
		}
		set
		{
			spouseOutfitReactionProgressState.NoticePauseTimer = value;
		}
	}

	private bool playerWasInClothesNoticeRange
	{
		get
		{
			return spouseOutfitReactionProgressState.PlayerWasInNoticeRange;
		}
		set
		{
			spouseOutfitReactionProgressState.PlayerWasInNoticeRange = value;
		}
	}

	private int clothesSecondNoticeCooldown
	{
		get
		{
			return spouseOutfitReactionProgressState.SecondNoticeCooldown;
		}
		set
		{
			spouseOutfitReactionProgressState.SecondNoticeCooldown = value;
		}
	}

	private int clothesChaseTimer
	{
		get
		{
			return spouseOutfitReactionProgressState.ChaseTimer;
		}
		set
		{
			spouseOutfitReactionProgressState.ChaseTimer = value;
		}
	}

	private NPC clothesReactingNpc
	{
		get
		{
			return spouseOutfitReactionProgressState.ReactingNpc;
		}
		set
		{
			spouseOutfitReactionProgressState.ReactingNpc = value;
		}
	}

	private bool outfitSequenceActive
	{
		get
		{
			return spouseOutfitReactionProgressState.SequenceActive;
		}
		set
		{
			spouseOutfitReactionProgressState.SequenceActive = value;
		}
	}

	private SpouseOutfitReactionCoordinator SpouseOutfitReactionCoordinator => spouseOutfitReactionCoordinator ?? (spouseOutfitReactionCoordinator = new SpouseOutfitReactionCoordinator(spouseOutfitReactionProgressState, UpdateClothesReactionSystem, ShouldStartClothesReaction, ResetClothesState, UpdateSpousePostOutfitLinger, TryHandleOutfitDialogueOrBlockNpcInteractionCore));

	private bool TryGetCurrentSavedFashionSenseOutfitId(out string outfitId)
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
		LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (1 == 0)
		{
		}
		string result = (currentLanguageCode - 1) switch
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
			Weather = GetCurrentWeatherForAiPrompt(),
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
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(playerAccessoryDescriptionModDataKey, ref text2) ? CleanPlayerOutfitReplyText(text2) : "";
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
		if (time < 1200)
		{
			return "morning";
		}
		if (time < 1800)
		{
			return "afternoon";
		}
		return "night";
	}

	private string GetFestivalContextForAiPrompt()
	{
		bool flag = false;
		try
		{
			if (typeof(Utility).GetMethod("isFestivalDay", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
			{
				typeof(int),
				typeof(string)
			}, null)?.Invoke(null, new object[2]
			{
				Game1.dayOfMonth,
				Game1.currentSeason
			}) is bool flag2)
			{
				flag = flag2;
			}
		}
		catch
		{
		}
		return flag ? "Today is a festival day in the current season. A subtle outfit reaction may reference the festive atmosphere if it fits naturally." : "Today is not a festival day.";
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

	private string GetCurrentWeatherForAiPrompt()
	{
		return Game1.isGreenRain ? "green rain" : (Game1.isLightning ? "storm / thunderstorm" : (Game1.isRaining ? "rain" : (Game1.isSnowing ? "snow" : (Game1.isDebrisWeather ? "windy / debris weather" : "sunny / clear"))));
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

	private void ClearOutfitNoticeMemoryCommand(string command, string[] args)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!Context.IsWorldReady || Game1.player == null)
		{
			((Mod)this).Monitor.Log("[OC DEBUG] Load a save before clearing outfit notice memory.", (LogLevel)3);
			return;
		}
		List<string> list = ((IEnumerable<string>)(object)((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Keys).Where((string key) => key.StartsWith("NatrollEXE.OutfitReactions.OutfitNotice.", StringComparison.OrdinalIgnoreCase) || key.StartsWith("NatrollEXE.OutfitReactions/SpecialItemSeen/", StringComparison.OrdinalIgnoreCase) || key.StartsWith("NatrollEXE.OutfitReactions/PantsSeen/", StringComparison.OrdinalIgnoreCase)).ToList();
		foreach (string item in list)
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Remove(item);
		}
		ResetClothesState(clearChangeFlag: true);
		npcsReactedToCurrentNotice.Clear();
		otherNpcClothesReactionSystem?.Reset();
		((Mod)this).Monitor.Log($"[OC DEBUG] Cleared {list.Count} outfit notice memory key(s) from this save and reset pending outfit notice state.", (LogLevel)2);
	}

	private void VoiceSampleReportCommand(string command, string[] args)
	{
		if (!Context.IsWorldReady)
		{
			((Mod)this).Monitor.Log("Load a save first, then run oc_test_voicesamples so NPC dialogue is available.", (LogLevel)3);
			return;
		}
		if (outfitAiService == null)
		{
			((Mod)this).Monitor.Log("The outfit AI service is not ready yet.", (LogLevel)3);
			return;
		}
		string text = outfitAiService.BuildVoiceSampleReport();
		((Mod)this).Monitor.Log(text, (LogLevel)2);
	}

	private void VoiceSamplePreviewCommand(string command, string[] args)
	{
		if (!Context.IsWorldReady)
		{
			((Mod)this).Monitor.Log("Load a save first, then run oc_preview_voicesamples <NpcName> so NPC dialogue is available.", (LogLevel)3);
			return;
		}
		if (outfitAiService == null)
		{
			((Mod)this).Monitor.Log("The outfit AI service is not ready yet.", (LogLevel)3);
			return;
		}
		if (args == null || args.Length == 0)
		{
			((Mod)this).Monitor.Log("Usage: oc_preview_voicesamples <NpcName>  (e.g. oc_preview_voicesamples Victoria)", (LogLevel)3);
			return;
		}
		string npcName = string.Join(" ", args).Trim();
		string text = outfitAiService.BuildVoiceSamplePreview(npcName, Game1.currentSeason);
		((Mod)this).Monitor.Log(text, (LogLevel)2);
	}

	private void DebugOutfitNoticeCommand(string command, string[] args)
	{
		if (!Context.IsWorldReady || Game1.player == null)
		{
			((Mod)this).Monitor.Log("[OC DEBUG] Load a save before running this command.", (LogLevel)3);
			return;
		}
		string outfitId;
		string value = (TryGetCurrentSavedFashionSenseOutfitId(out outfitId) ? outfitId : "<none>");
		string value2 = ((lastFashionSenseChangeInfo != null) ? GetFashionSenseDialogueKey(lastFashionSenseChangeInfo) : "");
		string value3 = ((lastFashionSenseChangeInfo == null) ? "<none>" : $"count={lastFashionSenseChangeInfo.CountChanges()} hair={lastFashionSenseChangeInfo.ChangedHair} accessory={lastFashionSenseChangeInfo.ChangedAccessory} hat={lastFashionSenseChangeInfo.ChangedHat} outfit={lastFashionSenseChangeInfo.ChangedOutfit} newHair={lastFashionSenseChangeInfo.NewHairId ?? ""} newAccessory={lastFashionSenseChangeInfo.NewAccessoryId ?? ""} newHat={lastFashionSenseChangeInfo.NewHatId ?? ""} newOutfit={lastFashionSenseChangeInfo.NewOutfitId ?? ""}");
		((Mod)this).Monitor.Log($"[OC DEBUG] Config: Enabled={Config.Enabled}, NPC reactions={Config.EnableNpcOutfitReactions}, NPC chance={Config.NpcOutfitReactionChance}, NPC distance={Config.OutfitNoticeDistance}, spouse distance={Config.OutfitNoticeDistance}.", (LogLevel)2);
		((Mod)this).Monitor.Log($"[OC DEBUG] Fashion Sense: fsApi={fsApi != null}, currentSavedOutfit={value}, changedClothes={changedClothes}, dialogueKey='{value2}', change={value3}.", (LogLevel)2);
		((Mod)this).Monitor.Log($"[OC DEBUG] NPC profile reactions: Sebastian={outfitAiService?.HasProfile("Sebastian")}, Penny={outfitAiService?.HasProfile("Penny")}, Robin={outfitAiService?.HasProfile("Robin")}. Any NPC with an enabled profile can react.", (LogLevel)2);
		NPC spouse = GetSpouse();
		if (spouse != null)
		{
			float value4 = DistanceToPlayer(spouse);
			((Mod)this).Monitor.Log($"[OC DEBUG] Spouse {((Character)spouse).Name}: sameLoc={((Character)spouse).currentLocation == ((Character)Game1.player).currentLocation}, dist={value4:0}, facing={IsNpcFacingPlayer(spouse)}, canNotice={CanNpcNoticeCurrentOutfitNotice(spouse)}, shouldStart={ShouldStartClothesReaction(spouse)}, firstNoticeDone={clothesFirstNoticeDone}, reacting={isReactingToClothes}, ready={clothesComplimentReady}.", (LogLevel)2);
		}
		if (Game1.currentLocation?.characters == null || Game1.currentLocation.characters.Count <= 0)
		{
			((Mod)this).Monitor.Log("[OC DEBUG] No NPCs are currently loaded in the player's location.", (LogLevel)2);
			return;
		}
		foreach (NPC item in ((IEnumerable<NPC>)Game1.currentLocation.characters).Where((NPC n) => n != null).OrderBy(DistanceToPlayer).Take(10))
		{
			float num = DistanceToPlayer(item);
			bool value5 = ((Character)item).currentLocation == ((Character)Game1.player).currentLocation;
			bool value6 = outfitAiService?.HasProfile(((Character)item).Name) ?? false;
			bool value7 = CanNpcReactToOutfit(item);
			bool value8 = CanNpcNoticeCurrentOutfitNotice(item);
			bool value9 = IsNpcFacingPlayer(item);
			bool value10 = num > Math.Max(64f, Config.OutfitNoticeDistance);
			((Mod)this).Monitor.Log($"[OC DEBUG] NPC {((Character)item).Name}: dist={num:0}, sameLoc={value5}, villager={((Character)item).IsVillager}, invisible={item.IsInvisible}, sleeping={((NetFieldBase<bool, NetBool>)(object)item.isSleeping).Value}, profile={value6}, canReact={value7}, canNotice={value8}, facing={value9}, tooFar={value10}.", (LogLevel)2);
		}
	}

	internal bool IsAnyOutfitReactionActive()
	{
		if (isReactingToClothes || outfitSequenceActive)
		{
			return true;
		}
		if (clothesComplimentReady || clothesPathStarted)
		{
			return true;
		}
		OtherNpcClothesReactionSystem obj = otherNpcClothesReactionSystem;
		if (obj != null && obj.HasAnyActivePendingReaction())
		{
			return true;
		}
		return false;
	}

	private void UpdateReactionActiveModDataFlag()
	{
		if (Game1.player != null)
		{
			bool flag = IsAnyOutfitReactionActive();
			bool flag2 = ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).ContainsKey("NatrollEXE.OutfitReactions/ReactionActive");
			if (flag && !flag2)
			{
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)["NatrollEXE.OutfitReactions/ReactionActive"] = "1";
			}
			else if (!flag && flag2)
			{
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Remove("NatrollEXE.OutfitReactions/ReactionActive");
			}
		}
	}

	internal void QueueAiConnectionTestFromConfigMenu()
	{
		outfitAiService?.QueueConnectionTestFromConfigMenu();
	}

	public override void Entry(IModHelper helper)
	{
		Instance = this;
		Config = helper.ReadConfig<ModConfig>();
		Config.MigrateLegacyAiSettings();
		outfitAiService = new OutfitAiService(helper, ((Mod)this).Monitor, () => Config);
		outfitAiService.IsRomanceableNpc = IsNpcRomanceable;
		outfitMemoryService = new OutfitMemoryService(helper, ((Mod)this).Monitor);
		hatMemoryService = new HatMemoryService(helper, ((Mod)this).Monitor);
		outfitVisionService = new OutfitVisionService(((Mod)this).Monitor);
		fashionSenseVisualService = new FashionSenseVisualService(((Mod)this).Monitor, () => fsApi);
		specialHatReactionService = new SpecialHatReactionService(helper, ((Mod)this).Monitor);
		specialItemReactionService = new SpecialItemReactionService(helper, ((Mod)this).Monitor);
		otherNpcClothesReactionSystem = new OtherNpcClothesReactionSystem(((Mod)this).Monitor, () => Config, TryQueueOtherNpcOutfitDialogue, RefreshOtherNpcOutfitPrompt, ClearOutfitPrompt, HasNoticeableCurrentFashionSenseAppearance, CanNpcNoticeCurrentOutfitNotice, MarkCurrentOutfitAsNoticed, CanNpcReactToCurrentOutfitNotice, HasNpcSeenCurrentVisualBefore);
		helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
		helper.Events.GameLoop.DayStarted += OnDayStarted;
		helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
		helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
		helper.Events.Display.MenuChanged += OnMenuChanged;
		helper.Events.Display.RenderedHud += OnRenderedHud;
		helper.Events.Input.ButtonPressed += OnButtonPressed;
		helper.Events.Player.Warped += OnWarped;
		helper.Events.Content.AssetRequested += OnAssetRequested;
		helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
		helper.Events.GameLoop.Saving += OnSaving;
		helper.ConsoleCommands.Add("oc_debug_notice", "Outfit Compliments: print why the current outfit notice can/can't start for nearby NPCs.", (Action<string, string[]>)DebugOutfitNoticeCommand);
		helper.ConsoleCommands.Add("oc_clear_notice_memory", "Outfit Compliments: clear this save's outfit notice memory and reset pending notice state.", (Action<string, string[]>)ClearOutfitNoticeMemoryCommand);
		helper.ConsoleCommands.Add("oc_test_voicesamples", "Outfit Reactions: report how many real in-game voice-sample lines each NPC profile has (run after loading a save).", (Action<string, string[]>)VoiceSampleReportCommand);
		helper.ConsoleCommands.Add("oc_preview_voicesamples", "Outfit Reactions: show the exact voice-sample lines that would be injected into the prompt for ONE NPC. Usage: oc_preview_voicesamples <NpcName>", (Action<string, string[]>)VoiceSamplePreviewCommand);
		ApplyHarmonyPatches();
	}

	private void ApplyHarmonyPatches()
	{
		try
		{
			MethodBase methodBase = AccessTools.Method(typeof(NPC), "checkAction", new Type[2]
			{
				typeof(Farmer),
				typeof(GameLocation)
			});
			if (methodBase == null)
			{
				((Mod)this).Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction target method was NOT found. Patch was not applied.", (LogLevel)3);
				return;
			}
			harmony = new Harmony(((Mod)this).ModManifest.UniqueID);
			harmony.PatchAll(typeof(ModEntry).Assembly);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction Harmony patch applied.", (LogLevel)2);
			}
		}
		catch (Exception ex)
		{
			((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Failed to apply NPC.checkAction patch: " + ex, (LogLevel)3);
		}
	}

	internal bool PrioritizeOutfitDialogueBeforeNpcCheckAction(NPC npc)
	{
		if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
		{
			return false;
		}
		if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return false;
		}
		if (Game1.eventUp)
		{
			return false;
		}
		if (TryPrioritizeSpouseOutfitDialogueForClick(npc))
		{
			return true;
		}
		return otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npc) ?? false;
	}

	internal bool TryHandleOutfitDialogueOrBlockNpcInteraction(NPC npc)
	{
		return SpouseOutfitReactionCoordinator.TryHandleInteraction(npc);
	}

	private bool TryHandleOutfitDialogueOrBlockNpcInteractionCore(NPC npc)
	{
		if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
		{
			return false;
		}
		if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return false;
		}
		if (Game1.eventUp)
		{
			return false;
		}
		if (((Character)Game1.player).modData != null && ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).ContainsKey("NatrollEXE.LotsOfKisses/AutoKissClickActive"))
		{
			return false;
		}
		if (TryOpenPrioritizedOutfitDialogueFromCheckAction(npc))
		{
			return true;
		}
		if (ShouldBlockNpcInteractionUntilOutfitDialogueRead(npc))
		{
			ShowPendingOutfitBlockedInteractionFeedback(npc);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Blocked normal interaction/kiss with " + ((Character)npc).Name + " because an unread outfit dialogue is pending.", (LogLevel)2);
			}
			return true;
		}
		return false;
	}

	private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
	{
		fsApi = ((Mod)this).Helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");
		((Mod)this).Monitor.Log((fsApi != null) ? "Fashion Sense API loaded successfully." : "Fashion Sense API not found. Outfit compliments will not detect clothing changes.", (LogLevel)((fsApi != null) ? 1 : 3));
		outfitAiService?.LoadProfiles();
		try
		{
			IGenericModConfigMenuApi api = ((Mod)this).Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			ModConfigMenu.Register(this, api);
		}
		catch (Exception ex)
		{
			((Mod)this).Monitor.Log("Failed to register GMCM options: " + ex.Message, (LogLevel)0);
		}
	}

	private void OnSaving(object sender, SavingEventArgs e)
	{
		outfitMemoryService?.Save();
		hatMemoryService?.Save();
	}

	private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		outfitMemoryService?.Load();
		hatMemoryService?.Load();
		specialItemReactionService?.ResetModRegistryCache();
		vanillaClothingTrackingInitialized = false;
		lastKnownVanillaHatId = null;
		lastKnownVanillaPantsName = null;
		ResetClothesState(clearChangeFlag: true);
		otherNpcClothesReactionSystem?.Reset();
		outfitAiService?.LoadProfiles(quiet: true);
	}

	private void OnDayStarted(object sender, DayStartedEventArgs e)
	{
		ResetClothesState(clearChangeFlag: true);
		otherNpcClothesReactionSystem?.Reset();
		Farmer player = Game1.player;
		if (player != null)
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)player).modData)?.Remove("NatrollEXE.OutfitReactions/ReactionActive");
		}
		outfitAiService?.LoadProfiles(quiet: true);
	}

	private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
	{
		CancelAllPendingOwnAiGenerations();
		ResetClothesState(clearChangeFlag: true);
		otherNpcClothesReactionSystem?.Reset();
	}

	private void OnRenderedHud(object sender, RenderedHudEventArgs e)
	{
		if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
		{
			return;
		}
		foreach (PendingAiGeneration item in aiGenerationCoordinator.GetOutfitSnapshot())
		{
			if (item != null && item.Task != null && !item.Task.IsCompleted)
			{
				NPC characterFromName = Game1.getCharacterFromName(item.NpcName, true, false);
				if (characterFromName != null && ((Character)characterFromName).currentLocation == ((Character)Game1.player).currentLocation)
				{
					DrawOwnAiWaitingHudMessage(e.SpriteBatch, characterFromName, GetOwnAiWaitingDialogueText(characterFromName, item.WaitingDotCount));
					return;
				}
			}
		}
		foreach (PendingAiPlayerReplyGeneration item2 in aiGenerationCoordinator.GetReplySnapshot())
		{
			if (item2 != null && item2.Task != null && !item2.Task.IsCompleted)
			{
				NPC characterFromName2 = Game1.getCharacterFromName(item2.NpcName, true, false);
				if (characterFromName2 != null && ((Character)characterFromName2).currentLocation == ((Character)Game1.player).currentLocation)
				{
					DrawOwnAiWaitingHudMessage(e.SpriteBatch, characterFromName2, GetOwnAiReplyWaitingDialogueText(characterFromName2, item2.WaitingDotCount));
					break;
				}
			}
		}
	}

	private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (Context.IsWorldReady && Game1.player != null && Game1.currentLocation != null && Config.Enabled && Game1.activeClickableMenu == null && !Game1.eventUp && (SButtonExtensions.IsActionButton(e.Button) || SButtonExtensions.IsUseToolButton(e.Button)))
		{
			NPC npcBeingInteractedWith = GetNpcBeingInteractedWith();
			if (npcBeingInteractedWith != null && !TryPrioritizeSpouseOutfitDialogueForClick(npcBeingInteractedWith))
			{
				otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npcBeingInteractedWith);
			}
		}
	}

	private NPC GetNpcBeingInteractedWith()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Game1.player == null || Game1.currentLocation == null)
		{
			return null;
		}
		Vector2 grabTile = ((Character)Game1.player).GetGrabTile();
		NPC val = ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>().FirstOrDefault((NPC c) => c != null && !c.IsInvisible && ((Character)c).TilePoint.X == (int)grabTile.X && ((Character)c).TilePoint.Y == (int)grabTile.Y);
		if (val != null)
		{
			return val;
		}
		int mouseTileX = (Game1.getOldMouseX() + ((Rectangle)(ref Game1.viewport)).X) / 64;
		int mouseTileY = (Game1.getOldMouseY() + ((Rectangle)(ref Game1.viewport)).Y) / 64;
		val = (from c in ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>()
			where c != null && !c.IsInvisible && ((Character)c).TilePoint.X == mouseTileX && ((Character)c).TilePoint.Y == mouseTileY
			orderby Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position)
			select c).FirstOrDefault((NPC c) => Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position) <= 192f);
		if (val != null)
		{
			return val;
		}
		return (from c in ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>()
			where c != null && !c.IsInvisible && ((Character)c).currentLocation == ((Character)Game1.player).currentLocation
			where Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position) <= 112f
			orderby Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position)
			select c).FirstOrDefault();
	}

	private bool TryPrioritizeSpouseOutfitDialogueForClick(NPC npc)
	{
		if (npc == null || clothesReactingNpc == null)
		{
			return false;
		}
		if (!((Character)npc).Name.Equals(((Character)clothesReactingNpc).Name, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!clothesComplimentReady || !outfitSequenceActive)
		{
			return false;
		}
		if (lastFashionSenseChangeInfo == null)
		{
			return false;
		}
		if (!CanNpcNoticeCurrentOutfitNotice(npc))
		{
			return false;
		}
		if (!spouseDialogueController.HasBackup)
		{
			spouseDialogueController.Capture(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
		}
		else
		{
			spouseDialogueController.TemporarilySkipFirstDailyDialogue(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
		}
		bool flag = QueueSpouseOutfitDialogueOnly(npc);
		if (flag)
		{
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Re-prioritized outfit dialogue for " + ((Character)npc).Name + " at click time.", (LogLevel)2);
			}
			else
			{
				KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "AI queue was not available on click.");
			}
		}
		return flag;
	}

	private void OnWarped(object sender, WarpedEventArgs e)
	{
		if (Context.IsWorldReady && e != null && e.IsLocalPlayer)
		{
			CancelAllPendingOwnAiGenerations();
			if (isReactingToClothes || outfitSequenceActive)
			{
				ResetClothesReactionState();
			}
			otherNpcClothesReactionSystem?.Reset();
		}
	}

	private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo("Mods/NatrollEXE.OutfitReactions/NpcCharacteristics", false))
		{
			e.LoadFrom((Func<object>)(() => outfitAiService?.LoadDefaultProfilesFromFiles() ?? new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase)), (AssetLoadPriority)(-1000), (string)null);
		}
	}

	private void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
	{
		if (!Context.IsWorldReady)
		{
			return;
		}
		foreach (IAssetName item in e.NamesWithoutLocale)
		{
			string text = ((object)item).ToString();
			if (!text.StartsWith("Mods/NatrollEXE.OutfitReactions/Clothes", StringComparison.OrdinalIgnoreCase) && text.Equals("Mods/NatrollEXE.OutfitReactions/NpcCharacteristics", StringComparison.OrdinalIgnoreCase))
			{
				outfitAiService?.LoadProfiles(quiet: true);
			}
		}
	}

	private void OnMenuChanged(object sender, MenuChangedEventArgs e)
	{
		if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
		{
			return;
		}
		bool flag = e.NewMenu != null && IsFashionSenseMenu(e.NewMenu);
		bool flag2 = e.OldMenu != null && IsFashionSenseMenu(e.OldMenu);
		if (flag && !fashionSenseMenuOpen)
		{
			fashionSenseMenuOpen = true;
			fsSnapshotBefore = CaptureFashionSenseSnapshot();
		}
		else
		{
			if ((fashionSenseMenuOpen && flag) || !(fashionSenseMenuOpen && flag2) || e.NewMenu != null)
			{
				return;
			}
			fashionSenseMenuOpen = false;
			DelayedAction.functionAfterDelay((Action)delegate
			{
				if (Context.IsWorldReady && Game1.player != null)
				{
					FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
					FashionSenseChangeInfo fashionSenseChangeInfo = CompareFashionSenseSnapshots(fsSnapshotBefore, fashionSenseSnapshot);
					fsSnapshotBefore = null;
					lastKnownVanillaHatId = fashionSenseSnapshot?.VanillaHat ?? "";
					lastKnownVanillaPantsName = fashionSenseSnapshot?.VanillaPants ?? "";
					lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(fashionSenseSnapshot?.VanillaPantsSpecialItemCandidates);
					vanillaClothingTrackingInitialized = true;
					if (fashionSenseChangeInfo != null && fashionSenseChangeInfo.CountChanges() > 0)
					{
						ApplyDetectedClothesChange(fashionSenseChangeInfo);
					}
				}
			}, 200);
		}
	}

	private void PollVanillaHatAndPantsChange()
	{
		if (vanillaClothingPollTimer > 0)
		{
			vanillaClothingPollTimer--;
			return;
		}
		vanillaClothingPollTimer = 15;
		if (fashionSenseMenuOpen)
		{
			return;
		}
		string visibleVanillaHatId = GetVisibleVanillaHatId();
		string currentVanillaHatName = GetCurrentVanillaHatName();
		List<string> candidates = ((!string.IsNullOrWhiteSpace(visibleVanillaHatId)) ? GetCurrentVisibleVanillaHatSpecialItemCandidates(currentVanillaHatName) : new List<string>());
		string currentVanillaPantsName = GetCurrentVanillaPantsName();
		List<string> candidates2 = ((!string.IsNullOrWhiteSpace(currentVanillaPantsName)) ? GetCurrentVanillaPantsSpecialItemCandidates(currentVanillaPantsName) : new List<string>());
		if (!vanillaClothingTrackingInitialized)
		{
			lastKnownVanillaHatId = visibleVanillaHatId;
			lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
			lastKnownVanillaPantsName = currentVanillaPantsName ?? "";
			lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
			vanillaClothingTrackingInitialized = true;
			return;
		}
		bool flag = !string.Equals(visibleVanillaHatId, lastKnownVanillaHatId ?? "", StringComparison.OrdinalIgnoreCase);
		bool flag2 = !string.Equals(currentVanillaPantsName ?? "", lastKnownVanillaPantsName ?? "", StringComparison.OrdinalIgnoreCase);
		if (!flag && !flag2)
		{
			return;
		}
		FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
		fashionSenseSnapshot.VanillaHat = lastKnownVanillaHatId ?? "";
		fashionSenseSnapshot.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaHatSpecialItemCandidates);
		fashionSenseSnapshot.VanillaPants = lastKnownVanillaPantsName ?? "";
		fashionSenseSnapshot.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaPantsSpecialItemCandidates);
		FashionSenseSnapshot fashionSenseSnapshot2 = CaptureFashionSenseSnapshot();
		fashionSenseSnapshot2.VanillaHat = visibleVanillaHatId;
		fashionSenseSnapshot2.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
		fashionSenseSnapshot2.VanillaPants = currentVanillaPantsName ?? "";
		fashionSenseSnapshot2.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
		lastKnownVanillaHatId = visibleVanillaHatId;
		lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
		lastKnownVanillaPantsName = currentVanillaPantsName ?? "";
		lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
		FashionSenseChangeInfo changeInfo = CompareFashionSenseSnapshots(fashionSenseSnapshot, fashionSenseSnapshot2);
		int num = changeInfo?.CountChanges() ?? 0;
		if (DebugLog)
		{
			((Mod)this).Monitor.Log($"[VANILLA POLL] hatChanged={flag} (now='{visibleVanillaHatId}' was='{fashionSenseSnapshot.VanillaHat}') | pantsChanged={flag2} (now='{currentVanillaPantsName}' was='{fashionSenseSnapshot.VanillaPants}') | changeCount={num} vanillaPantsChanged={changeInfo?.VanillaPantsChanged} vanillaPantsRemoved={changeInfo?.VanillaPantsRemoved} fsPantsAfter='{fashionSenseSnapshot2.Pants}' pantsDebug={GetCurrentVanillaPantsDebugString()}", (LogLevel)2);
		}
		if (changeInfo == null || num <= 0)
		{
			return;
		}
		DelayedAction.functionAfterDelay((Action)delegate
		{
			if (Context.IsWorldReady && Game1.player != null)
			{
				ApplyDetectedClothesChange(changeInfo);
			}
		}, 200);
	}

	private void ApplyDetectedClothesChange(FashionSenseChangeInfo changeInfo)
	{
		if (string.IsNullOrEmpty(GetFashionSenseDialogueKey(changeInfo)))
		{
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[FS] Detected change had nothing describable (total={changeInfo.CountChanges()}, likely a vanilla-pants-only side effect) — ignoring, not resetting notice state.", (LogLevel)2);
			}
			return;
		}
		ResetClothesState(clearChangeFlag: true);
		npcsReactedToCurrentNotice.Clear();
		loggedSpecialItemDebugKeys.Clear();
		otherNpcClothesReactionSystem?.Reset();
		lastEligibleSavedOutfitId = "";
		lastFashionSenseChangeInfo = changeInfo;
		changedClothes = true;
		otherNpcClothesReactionSystem?.NotifyOutfitChanged();
		if (DebugLog)
		{
			((Mod)this).Monitor.Log($"[FS] outfit change detected | total={changeInfo.CountChanges()} hair={changeInfo.ChangedHair} accessory={changeInfo.ChangedAccessory} hat={changeInfo.ChangedHat} vanillaHat={changeInfo.VanillaHatChanged} shirt={changeInfo.ChangedShirt} pants={changeInfo.ChangedPants} sleeves={changeInfo.ChangedSleeves} shoes={changeInfo.ChangedShoes} outfit={changeInfo.ChangedOutfit} newHair={changeInfo.NewHairId} newHat={changeInfo.NewHatId} newAccessory={changeInfo.NewAccessoryId}", (LogLevel)2);
		}
		if (changeInfo.ChangedAccessory && !AreVisionOnlyFashionSenseTriggersEnabled())
		{
			bool flag = ItemNameRevealsShape(changeInfo.NewAccessoryId);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log(flag ? "[FS] Accessory changed (no vision): item name reveals its shape, so it will be noticed." : "[FS] Accessory changed (no vision): item name is too generic to describe, so it is skipped.", (LogLevel)2);
			}
		}
	}

	private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
	{
		if (Context.IsWorldReady && Config.Enabled)
		{
			UpdateReactionActiveModDataFlag();
			SpouseOutfitReactionCoordinator.AdvanceTimers();
			if (spouseProximityState.PendingBubbleTimer > 0)
			{
				spouseProximityState.PendingBubbleTimer--;
			}
			RefreshCurrentSavedOutfitNoticeCandidate();
			PollVanillaHatAndPantsChange();
			NPC spouse = GetSpouse();
			NPC datingNpc = GetDatingNpc();
			NPC val = spouse ?? datingNpc;
			SpouseOutfitReactionCoordinator.Update(val, spouse, changedClothes && lastFashionSenseChangeInfo != null);
			UpdatePendingOwnAiGenerations();
			UpdatePendingOwnAiPlayerReplyGenerations();
			object obj = ((val != null) ? ((Character)val).Name : null);
			if (obj == null)
			{
				Farmer player = Game1.player;
				obj = ((player != null) ? player.spouse : null);
			}
			string spouseName = (string)obj;
			otherNpcClothesReactionSystem?.Update(spouseName);
		}
	}

	private void RefreshCurrentSavedOutfitNoticeCandidate()
	{
		if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled || (changedClothes && lastFashionSenseChangeInfo != null && !lastFashionSenseChangeInfo.ChangedOutfit))
		{
			return;
		}
		if (!TryGetCurrentSavedFashionSenseOutfitId(out var outfitId))
		{
			if (IsSavedOutfitNoticeChange(lastFashionSenseChangeInfo))
			{
				ResetClothesState(clearChangeFlag: true);
				otherNpcClothesReactionSystem?.Reset();
			}
			return;
		}
		if (lastFashionSenseChangeInfo != null && lastFashionSenseChangeInfo.ChangedOutfit && string.Equals(lastFashionSenseChangeInfo.NewOutfitId, outfitId, StringComparison.OrdinalIgnoreCase))
		{
			changedClothes = true;
			return;
		}
		FashionSenseChangeInfo changeInfo = new FashionSenseChangeInfo
		{
			ChangedOutfit = true,
			NewOutfitId = outfitId
		};
		if (string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(changeInfo)))
		{
			return;
		}
		if (string.Equals(lastEligibleSavedOutfitId, outfitId, StringComparison.OrdinalIgnoreCase))
		{
			changedClothes = true;
			return;
		}
		lastEligibleSavedOutfitId = outfitId;
		npcsReactedToCurrentNotice.Clear();
		lastFashionSenseChangeInfo = changeInfo;
		changedClothes = true;
		otherNpcClothesReactionSystem?.NotifyOutfitChanged();
		if (DebugLog)
		{
			((Mod)this).Monitor.Log("[CLOTHES NOTICE] Current saved outfit is eligible for outfit notices: " + outfitId, (LogLevel)2);
		}
	}

	private bool IsSavedOutfitNoticeChange(FashionSenseChangeInfo changeInfo)
	{
		return changeInfo != null && changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
	}

	private bool IsVanillaHatRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
	{
		return changeInfo != null && changeInfo.VanillaHatRemoved && changeInfo.VanillaHatChanged && changeInfo.CountChanges() == 1;
	}

	private bool IsSpecialItemRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
	{
		if (changeInfo == null || changeInfo.CountChanges() != 1)
		{
			return false;
		}
		if (!changeInfo.VanillaPantsChanged && !changeInfo.VanillaHatChanged)
		{
			return false;
		}
		SpecialItemNoticeInfo notice;
		return TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out notice) && notice != null && notice.WasRemoved;
	}

	private bool NpcRemembersRemovedSpecialItem(NPC npc, FashionSenseChangeInfo changeInfo)
	{
		SpecialItemNoticeInfo notice;
		return npc != null && changeInfo != null && TryResolveSpecialItemNoticeForNpc(npc, changeInfo, requireNpcMemoryForRemoval: false, out notice) && notice != null && notice.WasRemoved && HasSpecialItemMemory(npc, notice);
	}

	private bool NpcRemembersRemovedVanillaHat(NPC npc)
	{
		return npc != null && !string.IsNullOrWhiteSpace(hatMemoryService?.GetLastHatNameForNpc(((Character)npc).Name) ?? "");
	}

	private FashionSenseChangeInfo TryBuildCurrentSavedOutfitNoticeChange()
	{
		if (!TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) || string.IsNullOrWhiteSpace(outfitId))
		{
			return null;
		}
		FashionSenseChangeInfo fashionSenseChangeInfo = new FashionSenseChangeInfo
		{
			ChangedOutfit = true,
			NewOutfitId = outfitId
		};
		return string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(fashionSenseChangeInfo)) ? null : fashionSenseChangeInfo;
	}

	private FashionSenseChangeInfo GetEffectiveFashionSenseChangeInfoForNpc(NPC npc)
	{
		if (lastFashionSenseChangeInfo == null)
		{
			return null;
		}
		if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo) && npc != null)
		{
			if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? ""))
			{
				return null;
			}
			if (NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
			{
				return lastFashionSenseChangeInfo;
			}
			FashionSenseChangeInfo fashionSenseChangeInfo = TryBuildCurrentSavedOutfitNoticeChange();
			if (fashionSenseChangeInfo != null)
			{
				return fashionSenseChangeInfo;
			}
		}
		if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && npc != null && !NpcRemembersRemovedVanillaHat(npc))
		{
			FashionSenseChangeInfo fashionSenseChangeInfo2 = TryBuildCurrentSavedOutfitNoticeChange();
			if (fashionSenseChangeInfo2 != null)
			{
				return fashionSenseChangeInfo2;
			}
		}
		return lastFashionSenseChangeInfo;
	}

	private bool CanNpcNoticeCurrentOutfitNotice(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		return !HasNpcReactedToCurrentOutfitNotice(npc, lastFashionSenseChangeInfo?.NewOutfitId);
	}

	private bool HasNpcSeenCurrentVisualBefore(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
		if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: false, out var notice) && notice != null && notice.IsValid)
		{
			return HasSpecialItemMemory(npc, notice);
		}
		if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && outfitMemoryService != null)
		{
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
			if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
			{
				OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
				OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
				return memory != null && memory.TimesSeenBefore > 0;
			}
		}
		string visibleVanillaHatId = GetVisibleVanillaHatId();
		if (!string.IsNullOrWhiteSpace(visibleVanillaHatId) && hatMemoryService != null)
		{
			HatMemoryComparison memory2 = hatMemoryService.GetMemory(((Character)npc).Name, visibleVanillaHatId, GetCurrentVanillaHatName());
			return memory2 != null && memory2.TimesSeenBefore > 0;
		}
		return false;
	}

	private bool DidNpcWitnessPreviousLook(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? ""))
		{
			return true;
		}
		if (outfitMemoryService != null && lastFashionSenseChangeInfo != null)
		{
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(lastFashionSenseChangeInfo.NewOutfitId);
			if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
			{
				OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
				OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
				if (memory != null && memory.TimesSeenBefore > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void MarkCurrentOutfitAsNoticed(NPC npc)
	{
		if (npc == null || lastFashionSenseChangeInfo == null)
		{
			return;
		}
		string item = ((Character)npc).Name ?? "";
		FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
		if (effectiveFashionSenseChangeInfoForNpc == null || npcsReactedToCurrentNotice.Contains(item))
		{
			return;
		}
		SpecialItemNoticeInfo notice;
		bool flag = ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(npc, effectiveFashionSenseChangeInfoForNpc, out notice);
		bool flag2 = ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(npc);
		npcsReactedToCurrentNotice.Add(item);
		if (flag)
		{
			RecordSpecialItemMemory(npc, notice);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[SPECIAL ITEM MEMORY] {((Character)npc).Name} reacted to special item '{notice?.EntryId}'; saved outfit memory was not updated for this item-focused reaction.", (LogLevel)2);
			}
			if (notice != null && notice.WasRemoved)
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[SPECIAL ITEM MEMORY] Special item '" + notice.EntryId + "' was a removal reaction; clearing the notice so it does not repeat.", (LogLevel)2);
				}
				changedClothes = false;
				lastFashionSenseChangeInfo = null;
				if (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) && !string.IsNullOrWhiteSpace(outfitId))
				{
					lastEligibleSavedOutfitId = outfitId;
				}
			}
		}
		else if (flag2)
		{
			RecordVanillaHatMemory(npc);
			string currentVanillaPantsName = GetCurrentVanillaPantsName();
			if (!string.IsNullOrWhiteSpace(currentVanillaPantsName))
			{
				RecordVanillaPantsMemory(npc, currentVanillaPantsName);
			}
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[HAT MEMORY] " + ((Character)npc).Name + " reacted to a vanilla-hat focused notice; saved outfit memory was not updated for this hat-focused reaction.", (LogLevel)2);
			}
		}
		else if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc))
		{
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
			if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
			{
				RecordOutfitMemory(npc, currentSavedFashionSenseOutfitIdForAi);
			}
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[CLOTHES NOTICE] Recorded that {((Character)npc).Name} reacted to outfit '{currentSavedFashionSenseOutfitIdForAi}'.", (LogLevel)2);
			}
		}
		else if (IsImmediateFashionSenseNoticeChange(effectiveFashionSenseChangeInfoForNpc))
		{
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[FS] " + ((Character)npc).Name + " reacted to the immediate change; it stays available for other NPCs.", (LogLevel)2);
			}
			if (effectiveFashionSenseChangeInfoForNpc.VanillaHatChanged || !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
			{
				RecordVanillaHatMemory(npc);
			}
			string currentVanillaPantsName2 = GetCurrentVanillaPantsName();
			if (!string.IsNullOrWhiteSpace(currentVanillaPantsName2))
			{
				RecordVanillaPantsMemory(npc, currentVanillaPantsName2);
			}
			string currentSavedFashionSenseOutfitIdForAi2 = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
			if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi2))
			{
				RecordOutfitMemory(npc, currentSavedFashionSenseOutfitIdForAi2);
			}
		}
	}

	private bool ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(NPC npc, FashionSenseChangeInfo effectiveChangeInfo, out SpecialItemNoticeInfo notice)
	{
		notice = null;
		if (npc == null || effectiveChangeInfo == null)
		{
			return false;
		}
		if (!TryResolveSpecialItemNoticeForNpc(npc, effectiveChangeInfo, requireNpcMemoryForRemoval: true, out notice))
		{
			return false;
		}
		return notice != null && notice.IsValid;
	}

	private bool ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(NPC npc)
	{
		if (lastFashionSenseChangeInfo == null)
		{
			return false;
		}
		if (ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) != "HatOnly")
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
		{
			return true;
		}
		return IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && NpcRemembersRemovedVanillaHat(npc);
	}

	private static bool IsImmediateFashionSenseNoticeChange(FashionSenseChangeInfo changeInfo)
	{
		return changeInfo != null && !IsSavedOutfitNoticeChangeStatic(changeInfo) && (changeInfo.ChangedHair || changeInfo.ChangedHat || changeInfo.ChangedAccessory);
	}

	private static bool IsSavedOutfitNoticeChangeStatic(FashionSenseChangeInfo changeInfo)
	{
		return changeInfo != null && changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
	}

	private bool HasNpcReactedToCurrentOutfitNotice(NPC npc, string outfitId)
	{
		if (npc == null)
		{
			return false;
		}
		return npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? "");
	}

	private static string MakeSafeModDataPart(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "unknown";
		}
		string text = NormalizeOutfitText(value);
		StringBuilder stringBuilder = new StringBuilder();
		string text2 = text;
		foreach (char c in text2)
		{
			if (char.IsLetterOrDigit(c))
			{
				stringBuilder.Append(c);
			}
			else if (c == '_' || c == '-')
			{
				stringBuilder.Append(c);
			}
		}
		return (stringBuilder.Length > 0) ? stringBuilder.ToString() : "unknown";
	}

	private static string GetStableHexHash(string value)
	{
		uint num = 2166136261u;
		string text = value ?? "";
		string text2 = text;
		foreach (char c in text2)
		{
			num ^= c;
			num *= 16777619;
		}
		return num.ToString("x8", CultureInfo.InvariantCulture);
	}

	private static bool IsNpcFacingPlayer(NPC npc)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (npc == null || Game1.player == null)
		{
			return false;
		}
		Vector2 standingPosition = ((Character)npc).getStandingPosition();
		Vector2 standingPosition2 = ((Character)Game1.player).getStandingPosition();
		Vector2 val = standingPosition2 - standingPosition;
		if (((Vector2)(ref val)).LengthSquared() < 256f)
		{
			return true;
		}
		int facingDirection = ((Character)npc).FacingDirection;
		if (1 == 0)
		{
		}
		bool result = facingDirection switch
		{
			0 => val.Y < 0f, 
			1 => val.X > 0f, 
			2 => val.Y > 0f, 
			3 => val.X < 0f, 
			_ => true, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private float DistanceToPlayer(NPC npc)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (npc == null || Game1.player == null)
		{
			return float.MaxValue;
		}
		return Vector2.Distance(((Character)npc).Position, ((Character)Game1.player).Position);
	}

	private bool CanNpcReactToCurrentOutfitNotice(NPC npc)
	{
		return CanNpcReactToOutfit(npc) && ShouldStartClothesReaction(npc);
	}

	private bool IsNpcWatchingAsKissBystander(NPC npc)
	{
		return ((npc != null) ? ((Character)npc).modData : null) != null && ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)npc).modData).ContainsKey("NatrollEXE.LotsOfKisses/BystanderWatching");
	}

	private bool CanNpcReactToOutfit(NPC npc)
	{
		if (npc == null || string.IsNullOrWhiteSpace(((Character)npc).Name))
		{
			return false;
		}
		if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name))
		{
			return false;
		}
		if (IsNpcWatchingAsKissBystander(npc))
		{
			return false;
		}
		return outfitAiService?.HasProfile(((Character)npc).Name) ?? false;
	}

	private bool HasMinimumFriendshipForOutfitReaction(NPC npc)
	{
		return npc != null;
	}

	private int GetNpcPortraitCount(NPC npc)
	{
		try
		{
			if (((npc != null) ? npc.Portrait : null) == null)
			{
				return 0;
			}
			int num = Math.Max(1, npc.Portrait.Width / 64);
			int num2 = Math.Max(1, npc.Portrait.Height / 64);
			return num * num2;
		}
		catch
		{
			return 0;
		}
	}

	private bool HasNoticeableCurrentFashionSenseAppearance()
	{
		return ShouldStartClothesReaction();
	}

	private FashionSenseSnapshot CaptureFashionSenseSnapshot()
	{
		if (Game1.player == null)
		{
			return null;
		}
		string hat = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHat.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hat));
		bool flag = IsFashionSenseHatCoveringVanilla();
		string text = (IsFashionSensePantsCoveringVanilla() ? "" : (GetCurrentVanillaPantsName() ?? ""));
		List<string> vanillaPantsSpecialItemCandidates = ((!string.IsNullOrWhiteSpace(text)) ? GetCurrentVanillaPantsSpecialItemCandidates(text) : new List<string>());
		FashionSenseSnapshot fashionSenseSnapshot = new FashionSenseSnapshot();
		fashionSenseSnapshot.Hair = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHair.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hair));
		fashionSenseSnapshot.Accessory = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Id"), GetFsModData("FashionSense.CustomAccessory.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Accessory)));
		fashionSenseSnapshot.AccessorySecondary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Id"), GetFsModData("FashionSense.CustomAccessorySecondary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessorySecondary)));
		fashionSenseSnapshot.AccessoryTertiary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Id"), GetFsModData("FashionSense.CustomAccessoryTertiary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessoryTertiary)));
		fashionSenseSnapshot.Hat = hat;
		fashionSenseSnapshot.FashionSenseHatCoversVanilla = flag;
		fashionSenseSnapshot.VanillaHat = (flag ? "" : GetCurrentVanillaHatId());
		fashionSenseSnapshot.VanillaHatSpecialItemCandidates = ((!flag && !string.IsNullOrWhiteSpace(GetCurrentVanillaHatName())) ? GetCurrentVisibleVanillaHatSpecialItemCandidates(GetCurrentVanillaHatName()) : new List<string>());
		fashionSenseSnapshot.VanillaPants = text;
		fashionSenseSnapshot.VanillaPantsSpecialItemCandidates = vanillaPantsSpecialItemCandidates;
		fashionSenseSnapshot.Shirt = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShirt.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shirt));
		fashionSenseSnapshot.Pants = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomPants.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Pants));
		fashionSenseSnapshot.Sleeves = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomSleeves.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Sleeves));
		fashionSenseSnapshot.Shoes = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShoes.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shoes));
		fashionSenseSnapshot.OutfitId = (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) ? outfitId : null);
		fashionSenseSnapshot.HairColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hair"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hair));
		fashionSenseSnapshot.AccessoryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.Accessory"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Accessory));
		fashionSenseSnapshot.AccessorySecondaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessorySecondary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessorySecondary));
		fashionSenseSnapshot.AccessoryTertiaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessoryTertiary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessoryTertiary));
		fashionSenseSnapshot.HatColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hat"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hat));
		fashionSenseSnapshot.ShirtColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shirt"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shirt));
		fashionSenseSnapshot.PantsColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Pants"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Pants));
		fashionSenseSnapshot.SleevesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Sleeves"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Sleeves));
		fashionSenseSnapshot.ShoesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shoes"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shoes));
		return fashionSenseSnapshot;
	}

	private FashionSenseChangeInfo CompareFashionSenseSnapshots(FashionSenseSnapshot before, FashionSenseSnapshot after)
	{
		if (before == null || after == null)
		{
			return null;
		}
		bool flag = !string.IsNullOrWhiteSpace(after.OutfitId);
		bool flag2 = !string.IsNullOrWhiteSpace(after.Hair);
		bool flag3 = !string.IsNullOrWhiteSpace(StringUtils.FirstNonEmpty(before.Accessory, before.AccessorySecondary, before.AccessoryTertiary, after.Accessory, after.AccessorySecondary, after.AccessoryTertiary));
		bool flag4 = !IsEmptyFashionSenseValue(after.Hat) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(after.Hat);
		bool flag5 = after.FashionSenseHatCoversVanilla || flag4;
		bool flag6 = !flag5 && !string.Equals(before.VanillaHat ?? "", after.VanillaHat ?? "", StringComparison.OrdinalIgnoreCase);
		bool vanillaHatRemoved = !flag5 && !string.IsNullOrWhiteSpace(before.VanillaHat) && string.IsNullOrWhiteSpace(after.VanillaHat);
		bool flag7 = IsFashionSensePantsValueCoveringVanilla(after.Pants);
		bool vanillaPantsChanged = !flag7 && !string.Equals(before.VanillaPants ?? "", after.VanillaPants ?? "", StringComparison.OrdinalIgnoreCase);
		bool vanillaPantsRemoved = !flag7 && !string.IsNullOrWhiteSpace(before.VanillaPants) && string.IsNullOrWhiteSpace(after.VanillaPants);
		bool flag8 = before.Accessory != after.Accessory || before.AccessorySecondary != after.AccessorySecondary || before.AccessoryTertiary != after.AccessoryTertiary;
		bool flag9 = before.AccessoryColor != after.AccessoryColor || before.AccessorySecondaryColor != after.AccessorySecondaryColor || before.AccessoryTertiaryColor != after.AccessoryTertiaryColor;
		bool flag10 = flag && !string.Equals(before.OutfitId, after.OutfitId, StringComparison.OrdinalIgnoreCase);
		string changedAccessoryId = GetChangedAccessoryId(before, after, flag10);
		bool flag11 = !string.IsNullOrWhiteSpace(BuildCurrentAccessoryMemoryValue(after));
		return new FashionSenseChangeInfo
		{
			ChangedHair = (flag2 && (before.Hair != after.Hair || before.HairColor != after.HairColor)),
			ChangedAccessory = (flag10 ? flag11 : (flag8 || (flag3 && flag9))),
			ChangedHat = ((flag4 && (before.Hat != after.Hat || before.HatColor != after.HatColor)) || flag6),
			ChangedShirt = (before.Shirt != after.Shirt || before.ShirtColor != after.ShirtColor),
			ChangedPants = (before.Pants != after.Pants || before.PantsColor != after.PantsColor),
			ChangedSleeves = (before.Sleeves != after.Sleeves || before.SleevesColor != after.SleevesColor),
			ChangedShoes = (before.Shoes != after.Shoes || before.ShoesColor != after.ShoesColor),
			ChangedOutfit = flag10,
			NewHairId = after.Hair,
			NewAccessoryId = changedAccessoryId,
			NewHatId = after.Hat,
			NewVanillaHatId = after.VanillaHat,
			VanillaHatChanged = flag6,
			VanillaHatRemoved = vanillaHatRemoved,
			PreviousVanillaHatId = before.VanillaHat,
			PreviousVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaHatSpecialItemCandidates),
			VanillaPantsChanged = vanillaPantsChanged,
			VanillaPantsRemoved = vanillaPantsRemoved,
			PreviousVanillaPantsName = before.VanillaPants,
			NewVanillaPantsName = after.VanillaPants,
			PreviousVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaPantsSpecialItemCandidates),
			NewVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(after.VanillaPantsSpecialItemCandidates),
			NewShirtId = after.Shirt,
			NewPantsId = after.Pants,
			NewSleevesId = after.Sleeves,
			NewShoesId = after.Shoes,
			NewOutfitId = after.OutfitId
		};
	}

	private static string GetChangedAccessoryId(FashionSenseSnapshot before, FashionSenseSnapshot after, bool outfitChanged)
	{
		if (before == null || after == null)
		{
			return "";
		}
		string result = BuildCurrentAccessoryMemoryValue(after);
		if (outfitChanged)
		{
			return result;
		}
		string changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.Accessory, after.Accessory, before.AccessoryColor, after.AccessoryColor, "accessory");
		if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
		{
			return changedAccessorySlotDescription;
		}
		changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.AccessorySecondary, after.AccessorySecondary, before.AccessorySecondaryColor, after.AccessorySecondaryColor, "secondary accessory");
		if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
		{
			return changedAccessorySlotDescription;
		}
		changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.AccessoryTertiary, after.AccessoryTertiary, before.AccessoryTertiaryColor, after.AccessoryTertiaryColor, "tertiary accessory");
		if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
		{
			return changedAccessorySlotDescription;
		}
		return result;
	}

	private static string GetChangedAccessorySlotDescription(string beforeId, string afterId, string beforeColor, string afterColor, string slotLabel)
	{
		bool flag = !string.Equals(beforeId, afterId, StringComparison.OrdinalIgnoreCase);
		bool flag2 = !string.Equals(beforeColor, afterColor, StringComparison.OrdinalIgnoreCase);
		if (!flag && !flag2)
		{
			return "";
		}
		if (!string.IsNullOrWhiteSpace(afterId))
		{
			return afterId;
		}
		if (!string.IsNullOrWhiteSpace(beforeId))
		{
			return "removed " + beforeId;
		}
		return "changed " + slotLabel;
	}

	private string GetFsModData(string key)
	{
		if (Game1.player == null)
		{
			return null;
		}
		string text = default(string);
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(key, ref text) ? text : null;
	}

	private string GetFsAppearanceId(IFashionSenseApi.Type type)
	{
		if (fsApi == null || Game1.player == null)
		{
			return null;
		}
		try
		{
			KeyValuePair<bool, string> currentAppearanceId = fsApi.GetCurrentAppearanceId(type, Game1.player);
			if (currentAppearanceId.Key && !string.IsNullOrWhiteSpace(currentAppearanceId.Value))
			{
				string text = currentAppearanceId.Value.Trim();
				if (!text.Equals("None", StringComparison.OrdinalIgnoreCase))
				{
					return text;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private string GetFsAppearanceColorKey(IFashionSenseApi.Type type)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (fsApi == null || Game1.player == null)
		{
			return null;
		}
		try
		{
			KeyValuePair<bool, Color> appearanceColor = fsApi.GetAppearanceColor(type, Game1.player);
			if (!appearanceColor.Key)
			{
				return null;
			}
			Color value = appearanceColor.Value;
			return ((Color)(ref value)).R.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).G.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).B.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).A.ToString("X2", CultureInfo.InvariantCulture);
		}
		catch
		{
			return null;
		}
	}

	internal bool TryOpenPrioritizedOutfitDialogueFromCheckAction(NPC npc)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
		{
			return false;
		}
		if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return false;
		}
		if (Game1.eventUp)
		{
			return false;
		}
		if (!IsOwnAiWaitingStateActiveFor(npc) && !PrioritizeOutfitDialogueBeforeNpcCheckAction(npc))
		{
			return false;
		}
		bool flag = IsOwnAiWaitingStateActiveFor(npc);
		if ((npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= 0) && !flag)
		{
			return false;
		}
		try
		{
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
			((Character)Game1.player).Halt();
			if (flag)
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Holding " + ((Character)npc).Name + "'s normal dialogue behind the prioritized outfit AI wait.", (LogLevel)2);
				}
				return true;
			}
			Game1.drawDialogue(npc);
			otherNpcClothesReactionSystem?.NotifyPrioritizedDialogueOpenedByHarmony(npc);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Opened prioritized outfit dialogue for " + ((Character)npc).Name + " and skipped original NPC.checkAction.", (LogLevel)2);
			}
			return true;
		}
		catch (Exception value)
		{
			((Mod)this).Monitor.Log($"[CLOTHES PRIORITY] Failed to open prioritized outfit dialogue for {((Character)npc).Name}: {value}", (LogLevel)3);
			return false;
		}
	}

	private bool ShouldBlockNpcInteractionUntilOutfitDialogueRead(NPC npc)
	{
		if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return false;
		}
		if (IsOwnAiWaitingStateActiveFor(npc))
		{
			return true;
		}
		if (IsUnreadSpouseOutfitDialoguePendingFor(npc))
		{
			return true;
		}
		return otherNpcClothesReactionSystem?.HasUnreadPendingDialogueFor(npc) ?? false;
	}

	private bool IsUnreadSpouseOutfitDialoguePendingFor(NPC npc)
	{
		if (npc == null || Game1.player == null)
		{
			return false;
		}
		if (!IsPlayerSpouse(npc))
		{
			return false;
		}
		if (lastFashionSenseChangeInfo == null)
		{
			return false;
		}
		if (!CanNpcNoticeCurrentOutfitNotice(npc))
		{
			return false;
		}
		if (clothesReactingNpc != null && ((Character)npc).Name.Equals(((Character)clothesReactingNpc).Name, StringComparison.OrdinalIgnoreCase) && (outfitSequenceActive || isReactingToClothes))
		{
			return true;
		}
		if (outfitSequenceActive && clothesFirstNoticeDone)
		{
			return true;
		}
		return false;
	}

	private bool IsPlayerSpouse(NPC npc)
	{
		return npc != null && Game1.player != null && !string.IsNullOrWhiteSpace(Game1.player.spouse) && ((Character)npc).Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase);
	}

	private void ShowPendingOutfitBlockedInteractionFeedback(NPC npc)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return;
		}
		((Character)Game1.player).Halt();
		try
		{
			if (!((Character)npc).isMoving() && ((Character)npc).controller == null)
			{
				((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false, false);
			}
		}
		catch
		{
		}
		if (IsPlayerSpouse(npc))
		{
			ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
			UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
		}
		else
		{
			((Character)npc).doEmote(40, true);
		}
	}

	private static bool IsNpcRomanceable(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		try
		{
			if (Game1.characterData != null && Game1.characterData.TryGetValue(name, out var value) && value != null)
			{
				return value.CanBeRomanced;
			}
		}
		catch
		{
		}
		return false;
	}

	private bool IsFashionSenseMenu(IClickableMenu menu)
	{
		string text = ((object)menu)?.GetType().FullName ?? "";
		return text.Contains("FashionSense", StringComparison.OrdinalIgnoreCase);
	}

	private static string NormalizeOutfitText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		string text2 = text.ToLowerInvariant().Trim().Normalize(NormalizationForm.FormD);
		StringBuilder stringBuilder = new StringBuilder();
		string text3 = text2;
		foreach (char c in text3)
		{
			UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (unicodeCategory != UnicodeCategory.NonSpacingMark)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
	}

	private string GetFashionSenseDialogueKey(FashionSenseChangeInfo changeInfo)
	{
		if (changeInfo == null)
		{
			return null;
		}
		int num = changeInfo.CountChanges();
		if (num <= 0)
		{
			return null;
		}
		if (TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out var _))
		{
			return "Clothes";
		}
		if ((changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId)) || ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
		{
			return "Clothes";
		}
		bool flag = AreVisionOnlyFashionSenseTriggersEnabled();
		if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
		{
			return "Accessory";
		}
		if (changeInfo.VanillaHatChanged)
		{
			return "Hat";
		}
		if (changeInfo.ChangedHat && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId) && (flag || ItemNameRevealsShape(changeInfo.NewHatId)))
		{
			return "Hat";
		}
		if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
		{
			return "Hair";
		}
		return null;
	}

	private bool AreVisionOnlyFashionSenseTriggersEnabled()
	{
		return ShouldTryVisionForCurrentAiProvider();
	}

	private bool ItemNameRevealsShape(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return false;
		}
		if (IsIgnoredFashionSenseAccessoryId(itemId))
		{
			return false;
		}
		if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(itemId))
		{
			return false;
		}
		string text = FashionSenseVisualService.HumanizeAppearanceId(itemId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		string[] array = text.Split(' ');
		foreach (string text2 in array)
		{
			string text3 = text2.Trim('\'', '"', '.', ',', '(', ')');
			if (text3.Length < 3)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			string text4 = text3;
			foreach (char c in text4)
			{
				if (char.IsDigit(c))
				{
					flag = true;
				}
				else if (char.IsLetter(c))
				{
					flag2 = true;
				}
			}
			if (flag2 && !flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsFarmHouseLocation(GameLocation location)
	{
		if (location == null)
		{
			return false;
		}
		string text = location.Name ?? "";
		string text2 = location.NameOrUniqueName ?? "";
		string text3 = ((object)location).GetType().Name ?? "";
		string text4 = ((object)location).GetType().FullName ?? "";
		return text.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text2.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text3.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text3.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text4.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private bool IsBeachOrIslandLocation(GameLocation location)
	{
		if (location == null)
		{
			return false;
		}
		string text = location.Name ?? "";
		string text2 = location.NameOrUniqueName ?? "";
		return text.Equals("Beach", StringComparison.OrdinalIgnoreCase) || text2.Equals("Beach", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Island", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("Island", StringComparison.OrdinalIgnoreCase);
	}

	private bool IsMarriageCandidateNpcRoom(NPC npc, GameLocation location)
	{
		if (npc == null || location == null)
		{
			return false;
		}
		if (!IsMarriageCandidate(npc))
		{
			return false;
		}
		string npcName = NormalizeOutfitText(((Character)npc).Name);
		string displayName = NormalizeOutfitText(((Character)npc).displayName);
		string text = NormalizeOutfitText(location.Name + " " + location.NameOrUniqueName);
		if (LooksLikeNpcRoomText(text) && TextMentionsNpc(text, npcName, displayName))
		{
			return true;
		}
		return MapPropertiesSuggestNpcRoom(location, npcName, displayName);
	}

	private bool IsMarriageCandidatePersonalLocation(NPC npc, GameLocation location)
	{
		if (npc == null || location == null || !IsMarriageCandidate(npc))
		{
			return false;
		}
		if (location.IsOutdoors || IsFarmHouseLocation(location))
		{
			return false;
		}
		string npcName = NormalizeOutfitText(((Character)npc).Name);
		string displayName = NormalizeOutfitText(((Character)npc).displayName);
		string text = NormalizeOutfitText(location.Name + " " + location.NameOrUniqueName);
		if (TextMentionsNpc(text, npcName, displayName))
		{
			return true;
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Abigail"] = new string[3] { "seedshop", "pierres", "pierre" };
		dictionary["Alex"] = new string[2] { "joshhouse", "alexhouse" };
		dictionary["Elliott"] = new string[2] { "elliotthouse", "elliottcabin" };
		dictionary["Emily"] = new string[2] { "haleyhouse", "emilyhouse" };
		dictionary["Haley"] = new string[1] { "haleyhouse" };
		dictionary["Harvey"] = new string[3] { "harveyroom", "harveyclinic", "hospital" };
		dictionary["Leah"] = new string[2] { "leahhouse", "leahcottage" };
		dictionary["Maru"] = new string[2] { "sciencehouse", "robinhouse" };
		dictionary["Penny"] = new string[1] { "trailer" };
		dictionary["Sam"] = new string[1] { "samhouse" };
		dictionary["Sebastian"] = new string[4] { "sciencehouse", "sebastianbasement", "sebastianroom", "robinhouse" };
		dictionary["Shane"] = new string[3] { "animalshop", "marnieranch", "ranch" };
		Dictionary<string, string[]> dictionary2 = dictionary;
		if (dictionary2.TryGetValue(((Character)npc).Name ?? "", out var value))
		{
			string[] array = value;
			foreach (string text2 in array)
			{
				if (!string.IsNullOrWhiteSpace(text2) && text.Contains(NormalizeOutfitText(text2)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsMarriageCandidate(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		try
		{
			object obj = ((object)npc).GetType().GetField("datable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(npc) ?? ((object)npc).GetType().GetProperty("datable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(npc);
			if (obj == null)
			{
				return false;
			}
			object obj2 = obj.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
			bool flag = default(bool);
			int num;
			if (obj2 is bool)
			{
				flag = (bool)obj2;
				num = 1;
			}
			else
			{
				num = 0;
			}
			return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
		}
		catch
		{
			return false;
		}
	}

	private bool MapPropertiesSuggestNpcRoom(GameLocation location, string npcName, string displayName)
	{
		try
		{
			object obj;
			if (location == null)
			{
				obj = null;
			}
			else
			{
				Map map = location.map;
				obj = ((map != null) ? ((Component)map).Properties : null);
			}
			if (obj == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, PropertyValue> item in (IEnumerable<KeyValuePair<string, PropertyValue>>)((Component)location.map).Properties)
			{
				object obj2 = item;
				string text = "";
				string text2 = "";
				if (obj2 is DictionaryEntry dictionaryEntry)
				{
					text = dictionaryEntry.Key?.ToString() ?? "";
					text2 = dictionaryEntry.Value?.ToString() ?? "";
				}
				else if (obj2 != null)
				{
					Type type = obj2.GetType();
					text = type.GetProperty("Key")?.GetValue(obj2)?.ToString() ?? "";
					text2 = type.GetProperty("Value")?.GetValue(obj2)?.ToString() ?? "";
				}
				string text3 = NormalizeOutfitText(text);
				string text4 = NormalizeOutfitText(text2);
				string text5 = text3 + " " + text4;
				if (LooksLikeNpcRoomText(text5) && TextMentionsNpc(text5, npcName, displayName))
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private bool LooksLikeNpcRoomText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		return text.Contains("room") || text.Contains("bedroom") || text.Contains("bed room") || text.Contains("npcroom") || text.Contains("npc room") || text.Contains("quarto") || text.Contains("suite") || text.Contains("basement") || text.Contains("cellar") || text.Contains("porão") || text.Contains("porao");
	}

	private bool TextMentionsNpc(string text, string npcName, string displayName)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		return (!string.IsNullOrWhiteSpace(npcName) && text.Contains(npcName)) || (!string.IsNullOrWhiteSpace(displayName) && text.Contains(displayName));
	}

	private bool TryShowOwnAiOutfitDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
	{
		return TryQueueOwnAiWaitingDialogue(npc, isSpouseDialogue, clearExistingDialogue);
	}

	private bool CanUseOwnAiForOutfitDialogue(NPC npc)
	{
		if (outfitAiService == null || npc == null || lastFashionSenseChangeInfo == null)
		{
			return false;
		}
		return outfitAiService.HasProfile(((Character)npc).Name);
	}

	private bool ShouldUseDeferredOwnAiForNpc(NPC npc)
	{
		return CanUseOwnAiForOutfitDialogue(npc);
	}

	private bool TryQueueOwnAiWaitingDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
	{
		if (!CanUseOwnAiForOutfitDialogue(npc))
		{
			return false;
		}
		OutfitAiContext context = BuildOutfitAiContext(npc, isSpouseDialogue);
		if (context == null)
		{
			return false;
		}
		outfitAiService.PrepareVoiceSamplesForNpc(((Character)npc).Name);
		if (clearExistingDialogue)
		{
			npc.CurrentDialogue.Clear();
		}
		Game1.activeClickableMenu = null;
		Game1.afterDialogues = null;
		if (!aiGenerationCoordinator.TryGetOutfit(((Character)npc).Name, out var pending) || pending == null || pending.Task == null || pending.Task.IsCompleted)
		{
			pending = new PendingAiGeneration
			{
				NpcName = ((Character)npc).Name,
				IsSpouseDialogue = isSpouseDialogue,
				ClearExistingDialogue = clearExistingDialogue,
				WaitingDotCount = 1,
				WaitingDotTimer = 30,
				SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
				Cancellation = new CancellationTokenSource()
			};
			aiGenerationCoordinator.StartOutfit(pending, delegate(CancellationToken cancellationToken)
			{
				try
				{
					string dialogue;
					return outfitAiService.TryGenerateCompliment(context, out dialogue, cancellationToken) ? dialogue : null;
				}
				catch (OperationCanceledException)
				{
					return (string)null;
				}
				catch (Exception ex2)
				{
					((Mod)this).Monitor.Log(" Background outfit generation crashed: " + ex2.Message, (LogLevel)3);
					return (string)null;
				}
			});
			if (DebugLog)
			{
				((Mod)this).Monitor.Log(" Started background outfit compliment generation for " + ((Character)npc).Name + ". HUD waiting message is active.", (LogLevel)2);
			}
		}
		else
		{
			((Mod)this).Monitor.Log(" " + ((Character)npc).Name + " already has a background outfit compliment generation in progress.", (LogLevel)0);
		}
		return true;
	}

	private int GetActiveAiTimeoutSecondsForSafety()
	{
		string text = Config?.GetActiveProvider() ?? "DeepSeek";
		if (1 == 0)
		{
		}
		int num = text switch
		{
			"Gemini" => Config.GeminiAiTimeoutSeconds, 
			"OpenAI" => Config.OpenAiAiTimeoutSeconds, 
			"OpenRouter" => Config.OpenRouterAiTimeoutSeconds, 
			"Mistral" => Config.MistralAiTimeoutSeconds, 
			"Groq" => Config.GroqAiTimeoutSeconds, 
			"Together" => Config.TogetherAiTimeoutSeconds, 
			"Local" => Config.LocalAiTimeoutSeconds, 
			_ => Config.DeepSeekAiTimeoutSeconds, 
		};
		if (1 == 0)
		{
		}
		int value = num;
		return Math.Clamp(value, 3, 120);
	}

	private bool IsOwnAiWaitingStateActiveFor(NPC npc)
	{
		PendingAiGeneration pending;
		return npc != null && aiGenerationCoordinator.TryGetOutfit(((Character)npc).Name, out pending) && pending != null && pending.Task != null && !pending.Task.IsCompleted;
	}

	private string GetOwnAiWaitingDialogueText(NPC npc, int dotCount)
	{
		int count = Math.Clamp(dotCount, 1, 3);
		string text = new string('.', count);
		string name = ((!string.IsNullOrWhiteSpace((npc != null) ? ((Character)npc).displayName : null)) ? ((Character)npc).displayName : (((npc != null) ? ((Character)npc).Name : null) ?? "NPC"));
		return ((object)((Mod)this).Helper.Translation.Get("hud.npc-noticing", (object)new { name })).ToString() + text;
	}

	private string GetOwnAiReplyWaitingDialogueText(NPC npc, int dotCount)
	{
		int count = Math.Clamp(dotCount, 1, 3);
		string text = new string('.', count);
		string name = ((!string.IsNullOrWhiteSpace((npc != null) ? ((Character)npc).displayName : null)) ? ((Character)npc).displayName : (((npc != null) ? ((Character)npc).Name : null) ?? "NPC"));
		return ((object)((Mod)this).Helper.Translation.Get("hud.npc-thinking", (object)new { name })).ToString() + text;
	}

	private void DrawOwnAiWaitingHudMessage(SpriteBatch spriteBatch, NPC npc, string text)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (spriteBatch != null && npc != null && !string.IsNullOrWhiteSpace(text) && Game1.smallFont != null)
		{
			Vector2 val = Game1.smallFont.MeasureString(text);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor(32f, Math.Max(32f, (float)((Rectangle)(ref Game1.uiViewport)).Height - val.Y - 72f));
			Rectangle val3 = default(Rectangle);
			((Rectangle)(ref val3))..ctor((int)val2.X - 16, (int)val2.Y - 10, (int)val.X + 32, (int)val.Y + 20);
			spriteBatch.Draw(Game1.staminaRect, val3, Color.Black * 0.55f);
			spriteBatch.DrawString(Game1.smallFont, text, val2 + new Vector2(2f, 2f), Color.Black * 0.75f);
			spriteBatch.DrawString(Game1.smallFont, text, val2, Color.White);
		}
	}

	private void UpdatePendingOwnAiGenerations()
	{
		if (!aiGenerationCoordinator.HasOutfitGenerations)
		{
			return;
		}
		foreach (string outfitNpcName in aiGenerationCoordinator.GetOutfitNpcNames())
		{
			if (!aiGenerationCoordinator.TryGetOutfit(outfitNpcName, out var pending))
			{
				continue;
			}
			NPC characterFromName = Game1.getCharacterFromName(outfitNpcName, true, false);
			if (pending == null || characterFromName == null || pending.Task == null)
			{
				aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
				continue;
			}
			switch (AiDialogueLifecycle.Advance(pending))
			{
			case AiGenerationLifecycleState.Completed:
				if (!pending.CompletionHandled)
				{
					pending.CompletionHandled = true;
					string generated = null;
					try
					{
						if (pending.Task.Status == TaskStatus.RanToCompletion)
						{
							generated = pending.Task.Result;
						}
					}
					catch (Exception ex)
					{
						((Mod)this).Monitor.Log(" Could not read background AI result: " + ex.Message, (LogLevel)3);
					}
					OpenGeneratedOrFallbackOutfitDialogue(characterFromName, pending, generated);
				}
				aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
				break;
			case AiGenerationLifecycleState.TimedOut:
				((Mod)this).Monitor.Log(" Background generation for " + outfitNpcName + " exceeded the safety timer. Removing pending waiting state.", (LogLevel)3);
				AiRequestLifecycle.Cancel(pending.Cancellation);
				if (pending.IsSpouseDialogue && characterFromName != null)
				{
					ResetClothesState();
					aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
				}
				else if (characterFromName != null)
				{
					otherNpcClothesReactionSystem?.CancelPendingOwnAiGeneration(characterFromName);
					aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
				}
				else
				{
					aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
				}
				break;
			default:
				UpdateOwnAiWaitingVisual(characterFromName, pending);
				break;
			}
		}
	}

	private void UpdateOwnAiWaitingVisual(NPC npc, PendingAiGeneration pending)
	{
		if (npc == null || pending == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			return;
		}
		if (pending.WaitingDotTimer > 0)
		{
			pending.WaitingDotTimer--;
			return;
		}
		pending.WaitingDotTimer = 30;
		pending.WaitingDotCount++;
		if (pending.WaitingDotCount > 3)
		{
			pending.WaitingDotCount = 1;
		}
	}

	private void OpenGeneratedOrFallbackOutfitDialogue(NPC npc, PendingAiGeneration pending, string generated)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		if (npc == null || pending == null)
		{
			return;
		}
		if (Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			((Mod)this).Monitor.Log(" AI outfit compliment for " + pending.NpcName + " finished, but the player is no longer nearby. Discarding it.", (LogLevel)0);
			return;
		}
		bool flag = false;
		bool flag2 = false;
		string text = null;
		if (!string.IsNullOrWhiteSpace(generated))
		{
			if (generated.StartsWith("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}", StringComparison.Ordinal))
			{
				flag2 = true;
				generated = generated.Substring("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}".Length).Trim();
			}
			npc.CurrentDialogue.Clear();
			string text2 = (pending.IsSpouseDialogue ? "OutfitReactions_SpouseOwnAiOutfitReaction" : "OutfitReactions_GlobalOwnAiOutfitReaction");
			npc.CurrentDialogue.Push(new Dialogue(npc, text2, generated));
			text = generated;
			flag = true;
			if (DebugLog)
			{
				((Mod)this).Monitor.Log(" Background outfit compliment for " + ((Character)npc).Name + " is ready and queued.", (LogLevel)2);
			}
		}
		else
		{
			((Mod)this).Monitor.Log(" Background outfit generation did not produce a usable line for " + ((Character)npc).Name + ". Trying configured fallbacks.", (LogLevel)3);
			flag = TryQueueNonAiOutfitFallback(npc, pending.IsSpouseDialogue, clearExistingDialogue: true);
		}
		if (!flag || npc.CurrentDialogue.Count <= 0)
		{
			Game1.activeClickableMenu = null;
			if (pending.IsSpouseDialogue)
			{
				KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "background AI generation did not produce a usable line.");
			}
			else
			{
				otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueFailed(npc);
			}
			return;
		}
		Action onFinished = null;
		if (pending.IsSpouseDialogue)
		{
			onFinished = delegate
			{
				CompleteSpouseAfterOutfitDialogue(npc);
			};
		}
		bool flag3 = false;
		FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
		if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: true, out var notice) && notice != null && notice.WasRemoved)
		{
			flag3 = true;
		}
		bool flag4 = false;
		string secretId = null;
		if (!flag3 && !string.IsNullOrWhiteSpace(text) && !flag2 && Config.EnablePlayerReplyMenuAfterOutfitCompliment && specialItemReactionService != null)
		{
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc2 = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc2, requireNpcMemoryForRemoval: false, out var notice2) && notice2 != null && notice2.HasSecret && !notice2.WasRemoved && !specialItemReactionService.NpcAlreadyKnowsSecret(notice2.SecretId, ((Character)npc).Name))
			{
				flag4 = true;
				secretId = notice2.SecretId;
			}
		}
		if (!string.IsNullOrWhiteSpace(text) && flag2 && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
		{
			InstallAccessoryClarificationInputAfterOutfitDialogue(npc, pending.IsSpouseDialogue, text, onFinished);
		}
		else if (flag4)
		{
			InstallSecretRevealChoiceMenu(npc, pending.IsSpouseDialogue, text, secretId, onFinished);
		}
		else if (!flag3 && !string.IsNullOrWhiteSpace(text) && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
		{
			InstallPlayerReplyMenuAfterOutfitDialogue(npc, pending.IsSpouseDialogue, text, onFinished);
		}
		else if (pending.IsSpouseDialogue)
		{
			InstallSpouseAfterOutfitDialogue(npc);
		}
		if (!pending.IsSpouseDialogue)
		{
			otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueOpened(npc);
		}
		if (!pending.IsSpouseDialogue)
		{
			MarkCurrentOutfitAsNoticed(npc);
		}
		Game1.activeClickableMenu = null;
		((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
		Game1.drawDialogue(npc);
	}

	private bool TryQueueNonAiOutfitFallback(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
	{
		if (npc != null)
		{
			((Mod)this).Monitor.Log(" No AI outfit dialogue was queued for " + ((Character)npc).Name + ". Manual JSON outfit dialogue is disabled in this AI-only build.", (LogLevel)3);
		}
		return false;
	}

	private void InstallSpouseAfterOutfitDialogue(NPC npc)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		Game1.afterDialogues = (afterFadeFunction)delegate
		{
			CompleteSpouseAfterOutfitDialogue(npc);
		};
	}

	private void CompleteSpouseAfterOutfitDialogue(NPC npc)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		MarkCurrentOutfitAsNoticed(npc);
		ClearOutfitPrompt(npc);
		bool flag = npc != null && Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
		if (flag)
		{
			CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
			AnimatedSprite sprite = ((Character)npc).Sprite;
			if (sprite != null)
			{
				sprite.StopAnimation();
			}
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
			DoClothesFinalEmotes(npc);
			if (spouseRouteController.HasRoute)
			{
				spouseRouteController.Restore(npc, ((Mod)this).Monitor, DebugLog);
			}
			else
			{
				spouseRouteController.Clear();
			}
		}
		else
		{
			spouseRouteController.Clear();
		}
		spouseDialogueController.Restore(npc, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, ((Mod)this).Monitor, DebugLog);
		ResetClothesState();
		if (flag)
		{
			BeginSpousePostOutfitLinger(npc);
		}
	}

	private void BeginSpousePostOutfitLinger(NPC npc)
	{
		if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			ClearSpousePostOutfitLinger();
			return;
		}
		SpousePostOutfitLingerController.Begin(spouseProximityState, npc);
		CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
		SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, npc, Game1.player);
		if (DebugLog)
		{
			((Mod)this).Monitor.Log($"[CLOTHES SPOUSE] {((Character)npc).Name} will linger after the outfit compliment until distance >= {600f:F0} or {360} ticks.", (LogLevel)2);
		}
	}

	private void UpdateSpousePostOutfitLinger()
	{
		if (!spouseProximityState.LingerActive)
		{
			return;
		}
		NPC lingerNpc = spouseProximityState.LingerNpc;
		if (lingerNpc == null || Game1.player == null || !Context.IsWorldReady)
		{
			ClearSpousePostOutfitLinger();
			return;
		}
		bool flag = ((Character)lingerNpc).currentLocation == ((Character)Game1.player).currentLocation;
		float distance = (flag ? DistanceToPlayer(lingerNpc) : 600f);
		bool hasCapturedSpecialAction = spouseSpecialActionController.HasSnapshotFor(lingerNpc);
		if (!SpousePostOutfitLingerController.TickAndShouldResume(spouseProximityState, flag, distance, hasCapturedSpecialAction, 300f))
		{
			SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, lingerNpc, Game1.player);
			return;
		}
		if (!spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog))
		{
			((Character)lingerNpc).movementPause = 0;
		}
		ClearSpousePostOutfitLinger();
	}

	private void ClearSpousePostOutfitLinger()
	{
		SpousePostOutfitLingerController.Clear(spouseProximityState);
	}

	private void InstallPlayerReplyMenuAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		OutfitReplyConversationHistory obj = outfitReplyConversationHistory;
		NPC obj2 = npc;
		obj.Start((obj2 != null) ? ((Character)obj2).Name : null, npcCompliment);
		Game1.afterDialogues = (afterFadeFunction)delegate
		{
			ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
		};
	}

	private void InstallSecretRevealChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, string secretId, Action onFinished)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		Game1.afterDialogues = (afterFadeFunction)delegate
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Invalid comparison between Unknown and I4
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				ModEntry modEntry = this;
				Action onFinished2 = onFinished;
				NPC obj = npc;
				modEntry.FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
			}
			else
			{
				bool isPt = (int)LocalizedContentManager.CurrentLanguageCode == 4;
				string text = ((Character)npc).displayName ?? ((Character)npc).Name;
				string title = (isPt ? ("Contar o segredo a " + text + "?") : ("Tell " + text + " the secret?"));
				string replyLabel = (isPt ? "Contar" : "Tell them");
				string leaveLabel = (isPt ? "Não" : "Not now");
				Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyChoiceMenu(title, replyLabel, leaveLabel, delegate
				{
					specialItemReactionService?.RevealSecret(secretId, ((Character)npc).Name);
					OutfitAiContext outfitAiContext = BuildOutfitAiContext(npc, isSpouseDialogue);
					if (outfitAiContext == null)
					{
						ModEntry modEntry2 = this;
						Action onFinished3 = onFinished;
						NPC obj2 = npc;
						modEntry2.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
					}
					else
					{
						string text2 = specialItemReactionService?.GetSecretRevealMessage(secretId) ?? "";
						string playerReply = ((!string.IsNullOrWhiteSpace(text2)) ? text2 : (isPt ? "[O jogador contou ao NPC sobre a origem secreta do item.]" : "[The player just told the NPC about the item's secret origin.]"));
						outfitAiContext.ConversationTranscript = null;
						StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, playerReply, onFinished, outfitAiContext);
					}
				}, delegate
				{
					ModEntry modEntry2 = this;
					Action onFinished3 = onFinished;
					NPC obj2 = npc;
					modEntry2.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
				});
			}
		};
	}

	private void InstallAccessoryClarificationInputAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcLine, Action onFinished)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		Game1.afterDialogues = (afterFadeFunction)delegate
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			string titleOverride = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder:" : "Reply:");
			OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcLine, onFinished, titleOverride, delegate
			{
				ModEntry modEntry = this;
				Action onFinished2 = onFinished;
				NPC obj = npc;
				modEntry.FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
			}, saveAccessoryClarification: true);
		};
	}

	private void ShowPlayerReplyChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Invalid comparison between Unknown and I4
		if (!Config.EnablePlayerReplyMenuAfterOutfitCompliment || npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			Action onFinished2 = onFinished;
			NPC obj = npc;
			FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
			return;
		}
		string title = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder ao comentário?" : "Reply to the comment?");
		string replyLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder" : "Reply");
		string leaveLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Ir embora" : "Leave");
		Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyChoiceMenu(title, replyLabel, leaveLabel, delegate
		{
			OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
		}, delegate
		{
			ModEntry modEntry = this;
			Action onFinished3 = onFinished;
			NPC obj2 = npc;
			modEntry.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
		});
	}

	private void OpenPlayerOutfitReplyInputMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished, string titleOverride = null, Action cancelOverride = null, bool saveAccessoryClarification = false)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Invalid comparison between Unknown and I4
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Invalid comparison between Unknown and I4
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Invalid comparison between Unknown and I4
		if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			Action onFinished2 = onFinished;
			NPC obj = npc;
			FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
			return;
		}
		string title = ((!string.IsNullOrWhiteSpace(titleOverride)) ? titleOverride : (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Escreva sua resposta:" : "Write your reply:"));
		string sendLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Enviar" : "Send");
		string cancelLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Cancelar" : "Cancel");
		Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyTextInputMenu(title, sendLabel, cancelLabel, delegate(string replyText)
		{
			string text = CleanPlayerOutfitReplyText(replyText);
			if (string.IsNullOrWhiteSpace(text))
			{
				OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished, titleOverride, cancelOverride, saveAccessoryClarification);
			}
			else
			{
				if (saveAccessoryClarification)
				{
					SavePlayerProvidedAccessoryDescriptionForCurrentChange(text);
				}
				if (!CanUseOwnAiForOutfitDialogue(npc))
				{
					ModEntry modEntry = this;
					Action onFinished3 = onFinished;
					NPC obj2 = npc;
					modEntry.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
				}
				else
				{
					OutfitReplyConversationHistory obj3 = outfitReplyConversationHistory;
					NPC obj4 = npc;
					obj3.Append((obj4 != null) ? ((Character)obj4).Name : null, "Player", text);
					StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, text, onFinished);
				}
			}
		}, delegate
		{
			if (cancelOverride != null)
			{
				cancelOverride();
			}
			else
			{
				ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
			}
		});
	}

	private static string CleanPlayerOutfitReplyText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		text = Regex.Replace(text, "\\s+", " ").Trim();
		if (text.Length > 800)
		{
			text = text.Substring(0, 800).Trim();
		}
		return text;
	}

	private void StartPlayerReplyFollowUpGeneration(NPC npc, bool isSpouseDialogue, string npcCompliment, string playerReply, Action onFinished, OutfitAiContext prebuiltContext = null)
	{
		if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
		{
			FinishPlayerReplyInteraction(onFinished, (npc != null) ? ((Character)npc).Name : null);
			return;
		}
		OutfitAiContext context = prebuiltContext ?? BuildOutfitAiContext(npc, isSpouseDialogue);
		if (context == null)
		{
			FinishPlayerReplyInteraction(onFinished, (npc != null) ? ((Character)npc).Name : null);
			return;
		}
		if (prebuiltContext == null)
		{
			context.ConversationTranscript = outfitReplyConversationHistory.BuildTranscript(((Character)npc).Name);
		}
		Game1.activeClickableMenu = null;
		Game1.afterDialogues = null;
		PendingAiPlayerReplyGeneration pending = new PendingAiPlayerReplyGeneration
		{
			NpcName = ((Character)npc).Name,
			IsSpouseDialogue = isSpouseDialogue,
			NpcCompliment = (npcCompliment ?? ""),
			PlayerReply = (playerReply ?? ""),
			WaitingDotCount = 1,
			WaitingDotTimer = 30,
			SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
			Cancellation = new CancellationTokenSource(),
			OnFinished = onFinished
		};
		aiGenerationCoordinator.StartReply(pending, delegate(CancellationToken cancellationToken)
		{
			try
			{
				string dialogue;
				return outfitAiService.TryGenerateFollowUp(context, pending.NpcCompliment, pending.PlayerReply, out dialogue, cancellationToken) ? dialogue : null;
			}
			catch (OperationCanceledException)
			{
				return (string)null;
			}
			catch (Exception ex2)
			{
				((Mod)this).Monitor.Log(" Background player-reply follow-up crashed: " + ex2.Message, (LogLevel)3);
				return (string)null;
			}
		});
		if (DebugLog)
		{
			((Mod)this).Monitor.Log(" Started background player-reply follow-up generation for " + ((Character)npc).Name + ".", (LogLevel)2);
		}
	}

	private void UpdatePendingOwnAiPlayerReplyGenerations()
	{
		if (!aiGenerationCoordinator.HasReplyGenerations)
		{
			return;
		}
		foreach (string replyNpcName in aiGenerationCoordinator.GetReplyNpcNames())
		{
			if (!aiGenerationCoordinator.TryGetReply(replyNpcName, out var pending))
			{
				continue;
			}
			NPC characterFromName = Game1.getCharacterFromName(replyNpcName, true, false);
			if (pending == null || characterFromName == null || pending.Task == null)
			{
				FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
				aiGenerationCoordinator.RemoveReply(replyNpcName);
				continue;
			}
			switch (AiDialogueLifecycle.Advance(pending))
			{
			case AiGenerationLifecycleState.Completed:
				if (!pending.CompletionHandled)
				{
					pending.CompletionHandled = true;
					string generated = null;
					try
					{
						if (pending.Task.Status == TaskStatus.RanToCompletion)
						{
							generated = pending.Task.Result;
						}
					}
					catch (Exception ex)
					{
						((Mod)this).Monitor.Log(" Could not read player-reply follow-up result: " + ex.Message, (LogLevel)3);
					}
					OpenGeneratedPlayerReplyFollowUp(characterFromName, pending, generated);
				}
				aiGenerationCoordinator.RemoveReply(replyNpcName);
				break;
			case AiGenerationLifecycleState.TimedOut:
				((Mod)this).Monitor.Log(" Player-reply follow-up generation for " + replyNpcName + " exceeded the safety timer.", (LogLevel)3);
				AiRequestLifecycle.Cancel(pending.Cancellation);
				FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
				aiGenerationCoordinator.RemoveReply(replyNpcName);
				break;
			default:
				UpdateOwnAiPlayerReplyWaitingVisual(pending);
				break;
			}
		}
	}

	private void UpdateOwnAiPlayerReplyWaitingVisual(PendingAiPlayerReplyGeneration pending)
	{
		if (pending == null)
		{
			return;
		}
		if (pending.WaitingDotTimer > 0)
		{
			pending.WaitingDotTimer--;
			return;
		}
		pending.WaitingDotTimer = 30;
		pending.WaitingDotCount++;
		if (pending.WaitingDotCount > 3)
		{
			pending.WaitingDotCount = 1;
		}
	}

	private void OpenGeneratedPlayerReplyFollowUp(NPC npc, PendingAiPlayerReplyGeneration pending, string generated)
	{
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		if (npc == null || pending == null)
		{
			FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
			return;
		}
		if (Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || string.IsNullOrWhiteSpace(generated))
		{
			FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
			return;
		}
		if (generated.StartsWith("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}", StringComparison.Ordinal))
		{
			generated = generated.Substring("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}".Length).Trim();
		}
		if (string.IsNullOrWhiteSpace(generated))
		{
			FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
			return;
		}
		npc.CurrentDialogue.Clear();
		string text = (pending.IsSpouseDialogue ? "OutfitReactions_SpousePlayerReplyFollowUp" : "OutfitReactions_GlobalPlayerReplyFollowUp");
		npc.CurrentDialogue.Push(new Dialogue(npc, text, generated));
		outfitReplyConversationHistory.Append(pending.NpcName, "NPC", generated);
		Game1.activeClickableMenu = null;
		Game1.afterDialogues = (afterFadeFunction)delegate
		{
			ShowPlayerReplyChoiceMenu(npc, pending.IsSpouseDialogue, generated, pending.OnFinished);
		};
		((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
		Game1.drawDialogue(npc);
	}

	private void CancelAllPendingOwnAiGenerations()
	{
		IReadOnlyList<PendingAiPlayerReplyGeneration> readOnlyList = aiGenerationCoordinator.CancelAll();
		foreach (PendingAiPlayerReplyGeneration item in readOnlyList)
		{
			FinishPlayerReplyInteraction(item?.OnFinished, item?.NpcName);
		}
	}

	private void FinishPlayerReplyInteraction(Action onFinished, string npcName = null)
	{
		outfitReplyConversationHistory.Reset(npcName);
		Game1.activeClickableMenu = null;
		Game1.afterDialogues = null;
		onFinished?.Invoke();
	}

	private bool TryQueueOtherNpcOutfitDialogue(NPC npc)
	{
		if (!Config.EnableNpcOutfitReactions || npc == null)
		{
			return false;
		}
		if (TryShowOwnAiOutfitDialogue(npc, isSpouseDialogue: false, clearExistingDialogue: false))
		{
			return true;
		}
		((Mod)this).Monitor.Log(" No AI outfit dialogue was queued for " + ((Character)npc).Name + ". Manual JSON outfit dialogue is disabled in this AI-only build.", (LogLevel)3);
		return false;
	}

	private bool RefreshOtherNpcOutfitPrompt(NPC npc)
	{
		return npc != null;
	}

	private void ClearOutfitPrompt(NPC npc)
	{
	}

	private NPC GetSpouse()
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
		Enumerator<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>> enumerator = ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).Pairs.GetEnumerator();
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
		if (Game1.player.friendshipData != null && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).TryGetValue(((Character)npc).Name, ref val2))
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

	private bool ShouldStartClothesReaction(NPC npc = null)
	{
		if (!changedClothes || lastFashionSenseChangeInfo == null)
		{
			return false;
		}
		FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
		if (effectiveFashionSenseChangeInfoForNpc == null)
		{
			return false;
		}
		if (npc != null && IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && !CanNpcNoticeCurrentOutfitNotice(npc))
		{
			return false;
		}
		if (npc != null && IsSpecialItemRemovalOnlyNotice(effectiveFashionSenseChangeInfoForNpc))
		{
			if (!NpcRemembersRemovedSpecialItem(npc, effectiveFashionSenseChangeInfoForNpc))
			{
				return false;
			}
		}
		else if (npc != null && IsVanillaHatRemovalOnlyNotice(effectiveFashionSenseChangeInfoForNpc) && !NpcRemembersRemovedVanillaHat(npc))
		{
			return false;
		}
		string fashionSenseDialogueKey = GetFashionSenseDialogueKey(effectiveFashionSenseChangeInfoForNpc);
		return !string.IsNullOrEmpty(fashionSenseDialogueKey);
	}

	private void UpdateClothesReactionSystem(NPC npc)
	{
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		if (changedClothes && !isReactingToClothes)
		{
			playerWasInClothesNoticeRange = false;
		}
		if (npc == null || !Context.IsWorldReady || Game1.player == null)
		{
			return;
		}
		float num = DistanceToPlayer(npc);
		bool flag = num < (float)Config.OutfitNoticeDistance && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
		bool flag2 = ShouldStartClothesReaction(npc);
		bool flag3 = spouseOutfitApproachController.ShouldApproach(npc);
		if (changedClothes && !isReactingToClothes && !flag2)
		{
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && !CanNpcNoticeCurrentOutfitNotice(npc))
			{
				playerWasInClothesNoticeRange = flag;
			}
			else if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && !NpcRemembersRemovedVanillaHat(npc))
			{
				playerWasInClothesNoticeRange = flag;
			}
			else if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo) && !NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
			{
				playerWasInClothesNoticeRange = flag;
			}
			else
			{
				ResetClothesState();
			}
			return;
		}
		if (outfitSequenceActive && !isReactingToClothes && clothesFirstNoticeDone && (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || num > (float)Config.OutfitCancelDistance))
		{
			ResetClothesReactionState();
		}
		if (Config.Enabled && flag2 && !((NetFieldBase<bool, NetBool>)(object)npc.isSleeping).Value && !isReactingToClothes)
		{
			if (!clothesFirstNoticeDone && flag && IsNpcFacingPlayer(npc))
			{
				spouseOutfitReactionProgressState.BeginFirstNotice();
				if (flag3)
				{
					spouseRouteController.Stop(npc, ((Mod)this).Monitor, DebugLog);
				}
				ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
				UpdateSpouseOutfitNoticeHold(npc, num);
			}
			if (clothesFirstNoticeDone && !isReactingToClothes && clothesNoticePauseTimer <= 0 && flag && clothesSecondNoticeCooldown <= 0)
			{
				outfitSequenceActive = true;
				if (flag3)
				{
					spouseRouteController.Stop(npc, ((Mod)this).Monitor, DebugLog);
					((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
					spouseOutfitReactionProgressState.BeginApproach(npc);
				}
				else
				{
					spouseOutfitReactionProgressState.BeginClickReady(npc);
					if (DebugLog)
					{
						((Mod)this).Monitor.Log("[CLOTHES SPOUSE] " + ((Character)npc).Name + "'s outfit compliment is ready on click without pathing because they are outside the farmhouse.", (LogLevel)2);
					}
					ShowSpousePendingOutfitBubbleIfNeeded(npc);
					UpdateSpouseOutfitNoticeHold(npc, num);
				}
				clothesSecondNoticeCooldown = 300;
			}
		}
		if (isReactingToClothes && clothesReactingNpc == npc)
		{
			outfitSequenceActive = true;
			UpdateSpouseOutfitNoticeHold(npc, num);
			if (clothesComplimentReady)
			{
				ShowSpousePendingOutfitBubbleIfNeeded(npc);
			}
			if (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || num > (float)Config.OutfitCancelDistance)
			{
				ResetClothesState();
				return;
			}
			if (!clothesComplimentReady)
			{
				if (num <= 140f || clothesChaseTimer <= 0)
				{
					ShowOutfitCompliment(npc, flag);
					return;
				}
				if (((Character)npc).controller == null)
				{
					if (spouseOutfitApproachController.TryStartPath(npc, ((Mod)this).Monitor, DebugLog))
					{
						clothesPathStarted = true;
					}
					else
					{
						if (DebugLog)
						{
							((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Could not find an approach path for " + ((Character)npc).Name + " inside the farmhouse; making the outfit compliment ready on click.", (LogLevel)2);
						}
						clothesComplimentReady = true;
					}
					if (clothesInteractionCooldown <= 0)
					{
						clothesInteractionCooldown = 180;
					}
				}
				playerWasInClothesNoticeRange = flag;
				return;
			}
		}
		playerWasInClothesNoticeRange = flag;
	}

	private void UpdateSpouseOutfitNoticeHold(NPC npc, float distance)
	{
		spouseOutfitNoticeController.UpdateHold(spouseProximityState, npc, Game1.player, distance, CaptureSpouseOutfitSpecialActionBeforeOutfit);
	}

	private void ShowSpousePendingOutfitBubbleIfNeeded(NPC npc, bool force = false)
	{
		if (spouseOutfitNoticeController.TryShowPendingBubble(spouseProximityState, npc, Game1.player, force, clothesEmoteFired, Config.OutfitNoticeDistance, Game1.activeClickableMenu != null || Game1.eventUp, DistanceToPlayer))
		{
			clothesEmoteFired = true;
		}
	}

	private void ShowOutfitCompliment(NPC npc, bool inClothesNoticeRange)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
		CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
		((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
		spouseDialogueController.Capture(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
		if (!ShouldUseDeferredOwnAiForNpc(npc))
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
			((Mod)this).Monitor.Log(" " + ((Character)npc).Name + "'s spouse outfit compliment is waiting for player click before AI generation starts.", (LogLevel)1);
		}
		spouseOutfitReactionProgressState.MarkComplimentStarted(npc, inClothesNoticeRange);
	}

	private void KeepSpouseOutfitNoticePendingAfterAiFailure(NPC npc, string reason = null)
	{
		if (npc != null)
		{
			spouseDialogueController.RestoreNormalDialogueAfterAiFailure(npc, ClearOutfitPrompt, delegate(NPC npcToRestore)
			{
				spouseDialogueController.Restore(npcToRestore, Game1.player, restoreTalkState: true, clearCurrentDialogue: false, ((Mod)this).Monitor, DebugLog);
			}, ((Mod)this).Monitor, DebugLog);
			spouseOutfitReactionProgressState.KeepPendingAfterAiFailure(npc);
			if (Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation)
			{
				float num = DistanceToPlayer(npc);
				ShowSpousePendingOutfitBubbleIfNeeded(npc);
				UpdateSpouseOutfitNoticeHold(npc, num);
				playerWasInClothesNoticeRange = num < (float)Config.OutfitNoticeDistance;
			}
			string text = (string.IsNullOrWhiteSpace(reason) ? "" : (" Reason: " + reason));
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Outfit AI failed for " + ((Character)npc).Name + ", but the outfit was NOT marked as read. The current notice will stay pending until click retry or distance cancel." + text, (LogLevel)2);
			}
		}
	}

	private bool QueueSpouseOutfitDialogueOnly(NPC npc)
	{
		return spouseDialogueController.TryQueueOwnAiDialogue(npc, (NPC npcToQueue) => TryShowOwnAiOutfitDialogue(npcToQueue, isSpouseDialogue: true, clearExistingDialogue: false), ((Mod)this).Monitor);
	}

	private void RestoreSpouseDialogueBackupIfPending()
	{
		if (spouseDialogueController.HasBackup)
		{
			NPC characterFromName = Game1.getCharacterFromName(spouseDialogueController.Snapshot.NpcName, true, false);
			if (characterFromName == null)
			{
				spouseDialogueController.Clear();
				return;
			}
			ClearOutfitPrompt(characterFromName);
			spouseDialogueController.Restore(characterFromName, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, ((Mod)this).Monitor, DebugLog);
		}
	}

	private int TryGetAnimationFrameIndex(AnimationFrame frame)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object obj = frame;
			FieldInfo field = obj.GetType().GetField("frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && field.GetValue(obj) is int result)
			{
				return result;
			}
			PropertyInfo property = obj.GetType().GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.GetValue(obj) is int result2)
			{
				return result2;
			}
		}
		catch
		{
		}
		return -1;
	}

	private bool AnimationLooksLikeSpecialAction(List<AnimationFrame> animation)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (animation == null || animation.Count <= 0)
		{
			return false;
		}
		foreach (AnimationFrame item in animation)
		{
			int num = TryGetAnimationFrameIndex(item);
			if (num >= 16)
			{
				return true;
			}
		}
		return false;
	}

	private void CaptureSpouseOutfitSpecialActionBeforeOutfit(NPC npc)
	{
		if (npc == null || ((Character)npc).Sprite == null || ((Character)npc).currentLocation == null || spouseSpecialActionController.HasSnapshotFor(npc) || ((Character)npc).isMoving())
		{
			return;
		}
		List<AnimationFrame> list = null;
		if (((Character)npc).Sprite.CurrentAnimation != null && ((Character)npc).Sprite.CurrentAnimation.Count > 0)
		{
			list = new List<AnimationFrame>(((Character)npc).Sprite.CurrentAnimation);
		}
		bool flag = list != null && list.Count > 0;
		bool flag2 = ((Character)npc).Sprite.CurrentFrame >= 16;
		if (flag || flag2)
		{
			spouseSpecialActionController.Capture(new SpouseOutfitSpecialActionSnapshot
			{
				Npc = npc,
				Location = ((Character)npc).currentLocation,
				FacingDirection = ((Character)npc).FacingDirection,
				CurrentFrame = ((Character)npc).Sprite.CurrentFrame,
				Flip = ((Character)npc).flip,
				MovementPause = ((Character)npc).movementPause,
				AddedSpeed = (int)((Character)npc).addedSpeed,
				CurrentAnimation = list
			});
			((Character)npc).Sprite.StopAnimation();
			((Character)npc).Sprite.ClearAnimation();
			((Character)npc).Sprite.CurrentAnimation = null;
			((Character)npc).flip = false;
			((Character)npc).Sprite.CurrentFrame = GetNpcIdleFrameForDirection(((Character)npc).FacingDirection);
			((Character)npc).Sprite.UpdateSourceRect();
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[CLOTHES SPOUSE] Saved special animation for {((Character)npc).Name} before outfit reaction. frame={spouseSpecialActionController.Current.CurrentFrame} anim={list?.Count ?? 0}", (LogLevel)2);
			}
		}
	}

	private int GetNpcIdleFrameForDirection(int facingDirection)
	{
		return facingDirection switch
		{
			0 => 8, 
			1 => 4, 
			2 => 0, 
			3 => 12, 
			_ => 0, 
		};
	}

	private void DoClothesFinalEmotes(NPC npc)
	{
		if (npc != null && Game1.player != null)
		{
			int[] array = new int[2] { 20, 60 };
			((Character)npc).doEmote(array[random.Next(array.Length)], true);
			Game1.player.doEmote(array[random.Next(array.Length)]);
		}
	}

	private void ResetClothesReactionState()
	{
		spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog);
		spouseOutfitReactionProgressState.ClearCurrentReaction();
		spouseProximityState.ClearNotice();
	}

	private void ResetClothesState(bool clearChangeFlag = false)
	{
		RestoreSpouseDialogueBackupIfPending();
		spouseRouteController.Clear();
		spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog);
		ClearSpousePostOutfitLinger();
		spouseOutfitReactionProgressState.ClearAllProgress();
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

	private string GetCurrentVanillaHatId()
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
				if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, ref s))
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
			if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, ref s) && int.TryParse(s, out var result))
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
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s) && int.TryParse(s, out result) && result > 0;
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
		return (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s) && int.TryParse(s, out result)) ? Math.Max(0, result) : 0;
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
			if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s))
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
