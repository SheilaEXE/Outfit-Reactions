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
{	private void PollVanillaHatAndPantsChange()
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
		if (val.LengthSquared() < 256f)
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
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(key, out text) ? text : null;
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
			return value.R.ToString("X2", CultureInfo.InvariantCulture) + value.G.ToString("X2", CultureInfo.InvariantCulture) + value.B.ToString("X2", CultureInfo.InvariantCulture) + value.A.ToString("X2", CultureInfo.InvariantCulture);
		}
		catch
		{
			return null;
		}
	}
}
