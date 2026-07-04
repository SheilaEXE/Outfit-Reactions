using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using OutfitReactions.Ai;

namespace OutfitReactions
{
    /// <summary>
    /// Vanilla-hat helpers for <see cref="ModEntry"/>. These read the player's vanilla hat slot
    /// (Game1.player.hat) and reconcile it with Fashion Sense, which can cover the slot with its
    /// own hat or report the literal string "None" for an empty slot. Any reaction, change
    /// detection, or hat-memory decision involving the vanilla hat should go through these.
    /// </summary>
    public sealed partial class ModEntry
    {
        /// <summary>The raw vanilla hat id currently in the hat slot, or "" if none.</summary>
        private string GetCurrentVanillaHatId()
        {
            try
            {
                StardewValley.Objects.Hat hat = Game1.player?.hat?.Value;
                if (hat == null)
                    return "";
                return StringUtils.FirstNonEmpty(hat.ItemId, hat.Name) ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// True when a Fashion Sense appearance value means "nothing equipped". Fashion Sense reports
        /// the literal string "None" (case-insensitive) as well as null/blank for an empty slot, so
        /// both must be treated as empty.
        /// </summary>
        private static bool IsEmptyFashionSenseValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || value.Trim().Equals("None", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True when a real (meaningful) Fashion Sense hat is currently equipped, covering the vanilla hat slot.</summary>
        private bool IsFashionSenseHatCoveringVanilla()
        {
            // The modData key is the strongest signal: Fashion Sense writes it when a custom hat
            // is actually equipped. Even if the ID is generic (e.g. a pack/slot-like name), that
            // visible item still covers the vanilla hat sprite and the hidden vanilla hat must not
            // leak into reactions.
            string fsHatModDataId = GetFsModData("FashionSense.CustomHat.Id");
            if (!IsEmptyFashionSenseValue(fsHatModDataId))
                return true;

            // The API can sometimes report generic/default/internal values when no real custom
            // hat is equipped, so the fallback stays conservative to avoid suppressing normal
            // vanilla hats in saves with no visible Fashion Sense headwear.
            string fsHatApiId = GetFsAppearanceId(IFashionSenseApi.Type.Hat);
            return !IsEmptyFashionSenseValue(fsHatApiId)
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(fsHatApiId);
        }

        /// <summary>True when a real Fashion Sense pants/body-bottom appearance is currently covering the vanilla pants slot.</summary>
        private bool IsFashionSensePantsCoveringVanilla()
        {
            string fsPantsModDataId = GetFsModData("FashionSense.CustomPants.Id");
            if (IsFashionSensePantsValueCoveringVanilla(fsPantsModDataId))
                return true;

            string fsPantsApiId = GetFsAppearanceId(IFashionSenseApi.Type.Pants);
            return IsFashionSensePantsValueCoveringVanilla(fsPantsApiId);
        }

        /// <summary>True when a Fashion Sense pants value represents a real visible custom pants item, not blank/None/internal noise.</summary>
        private static bool IsFashionSensePantsValueCoveringVanilla(string value)
        {
            return !IsEmptyFashionSenseValue(value)
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(value);
        }

        /// <summary>
        /// The vanilla hat id, but ONLY when it is actually visible — i.e. no Fashion Sense hat is
        /// currently equipped on top of it. A Fashion Sense hat completely covers/replaces the
        /// vanilla hat slot visually, so a vanilla hat equipped underneath one is not something the
        /// farmer is "wearing" from any NPC's point of view and must never be reacted to, detected
        /// as a change, or recorded into hat memory. Use this (not the raw getter) for any reaction,
        /// detection, or memory decision involving the vanilla hat.
        /// </summary>
        private string GetVisibleVanillaHatId()
        {
            if (IsFashionSenseHatCoveringVanilla())
                return "";
            return GetCurrentVanillaHatId();
        }

        /// <summary>Human-readable name of the currently equipped vanilla hat, or "" if none.</summary>
        private string GetCurrentVanillaHatName()
        {
            try
            {
                StardewValley.Objects.Hat hat = Game1.player?.hat?.Value;
                if (hat == null)
                    return "";
                return StringUtils.FirstNonEmpty(hat.DisplayName, hat.Name) ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Builds the deep vanilla-hat memory hint for this NPC (independent of saved outfits).
        /// Returns null when there's nothing worth remembering.
        /// </summary>
        private string BuildVanillaHatMemoryContext(NPC npc)
        {
            if (hatMemoryService == null || npc == null)
                return null;

            string hatId = GetVisibleVanillaHatId();
            if (string.IsNullOrWhiteSpace(hatId))
                return null;

            string hatName = GetCurrentVanillaHatName();
            var memory = hatMemoryService.GetMemory(npc.Name, hatId, hatName);
            if (memory == null)
                return null;

            return hatMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
        }

        /// <summary>Records what hat (or bare head) this NPC just saw the farmer in, with date context.</summary>
        private void RecordVanillaHatMemory(NPC npc)
        {
            if (hatMemoryService == null || npc == null)
                return;

            // A Fashion Sense hat covers the vanilla hat slot entirely; whatever is underneath is
            // not visible, so there is nothing valid to record (not even "bare-headed") in that case.
            if (IsFashionSenseHatCoveringVanilla())
                return;

            hatMemoryService.RecordMemory(
                npc.Name,
                GetCurrentVanillaHatId(),
                GetCurrentVanillaHatName(),
                Game1.currentSeason,
                Game1.dayOfMonth,
                Game1.year);
        }
        // ── Vanilla pants helpers ─────────────────────────────────────────────────

        // modData key: "NatrollEXE.OutfitReactions/PantsSeen/{NpcName}/{ItemId}"
        private const string PantsSeenModDataPrefix = "NatrollEXE.OutfitReactions/PantsSeen/";

        private void RecordVanillaPantsMemory(NPC npc, string pantsName)
        {
            if (npc == null || string.IsNullOrWhiteSpace(pantsName) || Game1.player == null)
                return;
            try
            {
                string itemId = Game1.player.pantsItem?.Value?.ItemId ?? "";
                if (string.IsNullOrWhiteSpace(itemId)) return;
                string key = PantsSeenModDataPrefix + npc.Name + "/" + itemId;
                int count = 0;
                if (Game1.player.modData.TryGetValue(key, out string existing))
                    int.TryParse(existing, out count);
                Game1.player.modData[key] = (count + 1).ToString();
            }
            catch { }
        }

        private int GetVanillaPantsSeenCount(NPC npc)
        {
            if (npc == null || Game1.player == null) return 0;
            try
            {
                string itemId = Game1.player.pantsItem?.Value?.ItemId ?? "";
                if (string.IsNullOrWhiteSpace(itemId)) return 0;
                string key = PantsSeenModDataPrefix + npc.Name + "/" + itemId;
                if (Game1.player.modData.TryGetValue(key, out string val) && int.TryParse(val, out int count))
                    return count;
                return 0;
            }
            catch { return 0; }
        }

        private string BuildVanillaPantsMemoryContext(NPC npc, string pantsName)
        {
            if (npc == null || string.IsNullOrWhiteSpace(pantsName)) return "";
            int count = GetVanillaPantsSeenCount(npc);
            if (count <= 0) return "";
            string lang = GetCurrentGameLanguageForPrompt();
            bool pt = lang.Contains("pt", StringComparison.OrdinalIgnoreCase);
            return count == 1
                ? (pt ? $"Este NPC já viu a(o) jogadora(o) usando {pantsName} antes (1 vez). Pode reconhecer com familiaridade."
                      : $"This NPC has seen the farmer wear {pantsName} before (1 time). They may recognize it with familiarity.")
                : (pt ? $"Este NPC já viu a(o) jogadora(o) usando {pantsName} antes ({count} vezes). Devem reconhecer como algo já visto."
                      : $"This NPC has seen the farmer wear {pantsName} before ({count} times). They should recognize it as something they've seen.");
        }


        /// <summary>Display name of the currently worn vanilla pants, or "" if none or FS is overriding.</summary>
        private string GetCurrentVanillaPantsName()
        {
            try
            {
                if (IsFashionSensePantsCoveringVanilla())
                    return "";
                var pants = Game1.player?.pantsItem?.Value;
                if (pants == null)
                    return "";
                return StringUtils.FirstNonEmpty(pants.DisplayName, pants.Name) ?? "";
            }
            catch { return ""; }
        }

        private string GetCurrentVanillaPantsDebugString()
        {
            try
            {
                var pants = Game1.player?.pantsItem?.Value;
                if (pants == null)
                    return "pantsItem=null";

                return $"display='{pants.DisplayName}' name='{pants.Name}' itemId='{pants.ItemId}' qid='{pants.QualifiedItemId}' visibleName='{GetCurrentVanillaPantsName()}'";
            }
            catch (Exception ex)
            {
                return "error=" + ex.GetType().Name + ":" + ex.Message;
            }
        }

        // ── Generic special item memory/helpers ───────────────────────────────────────

        // modData key: "NatrollEXE.OutfitReactions/SpecialItemSeen/{NpcName}/{ItemType}/{EntryId}"
        private const string SpecialItemSeenModDataPrefix = "NatrollEXE.OutfitReactions/SpecialItemSeen/";

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

            public bool IsValid => !string.IsNullOrWhiteSpace(EntryId)
                && !string.IsNullOrWhiteSpace(ReactionContext);
        }

        private List<string> GetVanillaPantsSpecialItemCandidatesFromName(string displayName)
        {
            List<string> candidates = new();
            AddSpecialItemCandidate(candidates, displayName);
            return candidates;
        }

        private List<string> GetCurrentVanillaPantsSpecialItemCandidates(string displayName)
        {
            List<string> candidates = GetVanillaPantsSpecialItemCandidatesFromName(displayName);

            try
            {
                var pants = Game1.player?.pantsItem?.Value;
                AddSpecialItemCandidate(candidates, pants?.DisplayName);
                AddSpecialItemCandidate(candidates, pants?.Name);
                AddSpecialItemCandidate(candidates, pants?.ItemId);
                AddSpecialItemCandidate(candidates, pants?.QualifiedItemId);
            }
            catch { }

            return candidates;
        }

        private List<string> GetCurrentVisibleVanillaHatSpecialItemCandidates(string displayName)
        {
            List<string> candidates = new();
            AddSpecialItemCandidate(candidates, displayName);

            try
            {
                var hat = Game1.player?.hat?.Value;
                AddSpecialItemCandidate(candidates, hat?.DisplayName);
                AddSpecialItemCandidate(candidates, hat?.Name);
                AddSpecialItemCandidate(candidates, hat?.ItemId);
                AddSpecialItemCandidate(candidates, hat?.QualifiedItemId);
            }
            catch { }

            return candidates;
        }

        private static void AddSpecialItemCandidate(List<string> candidates, string value)
        {
            if (candidates == null || string.IsNullOrWhiteSpace(value))
                return;

            string trimmed = value.Trim();
            foreach (string existing in candidates)
            {
                if (existing.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            candidates.Add(trimmed);
        }

        private static string FormatSpecialItemCandidates(IEnumerable<string> candidates)
        {
            if (candidates == null)
                return "<null>";

            List<string> values = new();
            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                    values.Add(candidate.Trim());
            }

            return values.Count == 0 ? "<empty>" : string.Join(", ", values);
        }

        private static List<string> CloneSpecialItemCandidates(IEnumerable<string> candidates)
        {
            List<string> clone = new();
            if (candidates == null)
                return clone;

            foreach (string candidate in candidates)
                AddSpecialItemCandidate(clone, candidate);

            return clone;
        }

        private void LogSpecialItemDebugOnce(string key, string message)
        {
            if (!DebugLog)
                return;

            // Use a set keyed on the event TYPE + NPC + entry only (not the full message),
            // so alternating <none>/Lewis calls with the same scenario don't re-fire.
            string dedupeKey = key ?? "";
            if (!loggedSpecialItemDebugKeys.Add(dedupeKey))
                return;

            Monitor.Log("[SPECIAL ITEM DEBUG] " + message, LogLevel.Info);
        }

        private static string DescribeSpecialItemNotice(SpecialItemNoticeInfo notice)
        {
            if (notice == null)
                return "<none>";

            return $"entry='{notice.EntryId}' type='{notice.ItemType}' display='{notice.DisplayName}' matched='{notice.MatchedName}' removed={notice.WasRemoved} valid={notice.IsValid}";
        }

        private bool TryResolveSpecialItemCandidates(
            IEnumerable<string> candidateNames,
            string itemType,
            NPC npc,
            bool wasRemoved,
            out SpecialItemNoticeInfo notice)
        {
            notice = null;

            if (specialItemReactionService == null)
                return false;

            if (!specialItemReactionService.TryResolveItem(candidateNames, itemType, npc, GetCurrentGameLanguageForPrompt(), out SpecialItemReactionService.ResolvedSpecialItem resolved, wasRemoved)
                || resolved == null
                || string.IsNullOrWhiteSpace(resolved.EntryId)
                || string.IsNullOrWhiteSpace(resolved.ReactionContext))
            {
                return false;
            }

            notice = new SpecialItemNoticeInfo
            {
                EntryId = resolved.EntryId,
                DisplayName = resolved.DisplayName,
                ItemType = StringUtils.FirstNonEmpty(resolved.ItemType, itemType) ?? itemType ?? "",
                MatchedName = resolved.MatchedName,
                ReactionContext = resolved.ReactionContext,
                WasRemoved = wasRemoved,
                HasSecret = resolved.HasSecret,
                SecretId = resolved.SecretId ?? ""
            };
            notice.MemoryHint = BuildSpecialItemMemoryContext(npc, notice);
            return true;
        }

        private bool TryResolveSpecialItemNoticeForNpc(
            NPC npc,
            FashionSenseChangeInfo changeInfo,
            bool requireNpcMemoryForRemoval,
            out SpecialItemNoticeInfo notice)
        {
            notice = null;

            if (changeInfo == null || specialItemReactionService == null)
                return false;

            // Compact debug only: no per-candidate spam. The VANILLA POLL line still shows the raw
            // before/after pants state; these lines only say what the special-item resolver decided
            // for the NPC that actually tried to react.
            string npcName = npc?.Name ?? "<none>";

            // 1. Visible vanilla pants, e.g. the Mayor's purple shorts in the pants slot.
            // If Fashion Sense pants are active, GetCurrentVanillaPantsName() returns blank, so
            // hidden/overridden vanilla pants cannot leak into a special-item reaction.
            string currentPantsName = GetCurrentVanillaPantsName();
            if (!string.IsNullOrWhiteSpace(currentPantsName))
            {
                List<string> currentPantsCandidates = GetCurrentVanillaPantsSpecialItemCandidates(currentPantsName);
                if (TryResolveSpecialItemCandidates(currentPantsCandidates, "Pants", npc, wasRemoved: false, out notice))
                {
                    if (changeInfo.VanillaPantsChanged)
                        LogSpecialItemDebugOnce($"current-pants|{npcName}|{notice.EntryId}|{currentPantsName}",
                            $"{npcName}: CURRENT visible pants matched {DescribeSpecialItemNotice(notice)} | current='{currentPantsName}' candidates=[{FormatSpecialItemCandidates(currentPantsCandidates)}]");
                    return true;
                }
            }

            // 2. Removed/replaced vanilla pants. Do not look at the CURRENT pants to identify
            // what was removed — after the player takes off Mayor's shorts, the current slot is
            // blank or a normal pair of pants. Resolve the PREVIOUS visible pants instead.
            // This also catches special -> normal transitions, not only special -> empty.
            if (changeInfo.VanillaPantsChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaPantsName))
            {
                List<string> removedPantsCandidates = CloneSpecialItemCandidates(changeInfo.PreviousVanillaPantsSpecialItemCandidates);
                AddSpecialItemCandidate(removedPantsCandidates, changeInfo.PreviousVanillaPantsName);

                if (TryResolveSpecialItemCandidates(removedPantsCandidates, "Pants", npc, wasRemoved: true, out notice))
                {
                    bool hasMemory = HasSpecialItemMemory(npc, notice);
                    LogSpecialItemDebugOnce($"removed-pants|{npcName}|{notice.EntryId}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}|{hasMemory}|{requireNpcMemoryForRemoval}",
                        $"{npcName}: PREVIOUS pants matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(removedPantsCandidates)}] npcHasMemory={hasMemory} requireMemory={requireNpcMemoryForRemoval}");

                    if (requireNpcMemoryForRemoval && !hasMemory)
                    {
                        LogSpecialItemDebugOnce($"removed-pants-ignored|{npcName}|{notice.EntryId}",
                            $"{npcName}: removed special item ignored because this NPC has no memory for it.");
                        notice = null;
                        return false;
                    }

                    return true;
                }
                else
                {
                    LogSpecialItemDebugOnce($"previous-pants-no-match|{npcName}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}",
                        $"{npcName}: previous pants did NOT match a special item | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(removedPantsCandidates)}]");
                }
            }

            // 3. Visible vanilla hat entries from luckypurpleshorts.json.
            // For special items worn as hats (e.g. the mod short-hat), react while currently worn.
            string currentVisibleHatId = GetVisibleVanillaHatId();
            if (!string.IsNullOrWhiteSpace(currentVisibleHatId))
            {
                string currentHatName = GetCurrentVanillaHatName();
                if (TryResolveSpecialItemCandidates(GetCurrentVisibleVanillaHatSpecialItemCandidates(currentHatName), "Hat", npc, wasRemoved: false, out notice))
                {
                    LogSpecialItemDebugOnce($"current-hat|{npcName}|{notice.EntryId}|{currentVisibleHatId}|{currentHatName}",
                        $"{npcName}: CURRENT visible vanilla hat matched {DescribeSpecialItemNotice(notice)} | hatId='{currentVisibleHatId}' hatName='{currentHatName}'");
                    return true;
                }
            }

            // 4. Removed/replaced vanilla hat special item. Mirrors passo 2 for pants.
            // After the farmer takes off the short-hat mod, the current slot is empty or a
            // different hat. Resolve using the PREVIOUS hat candidates stored in changeInfo.
            if (changeInfo.VanillaHatChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaHatId))
            {
                List<string> removedHatCandidates = CloneSpecialItemCandidates(changeInfo.PreviousVanillaHatSpecialItemCandidates);
                AddSpecialItemCandidate(removedHatCandidates, changeInfo.PreviousVanillaHatId);

                if (TryResolveSpecialItemCandidates(removedHatCandidates, "Hat", npc, wasRemoved: true, out notice))
                {
                    bool hasMemory = HasSpecialItemMemory(npc, notice);
                    LogSpecialItemDebugOnce($"removed-hat|{npcName}|{notice.EntryId}|{changeInfo.PreviousVanillaHatId}|{changeInfo.NewVanillaHatId}|{hasMemory}|{requireNpcMemoryForRemoval}",
                        $"{npcName}: PREVIOUS hat matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaHatId}' new='{changeInfo.NewVanillaHatId}' candidates=[{FormatSpecialItemCandidates(removedHatCandidates)}] npcHasMemory={hasMemory} requireMemory={requireNpcMemoryForRemoval}");

                    if (requireNpcMemoryForRemoval && !hasMemory)
                    {
                        LogSpecialItemDebugOnce($"removed-hat-ignored|{npcName}|{notice.EntryId}",
                            $"{npcName}: removed special hat ignored because this NPC has no memory for it.");
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
                return "";

            return SpecialItemSeenModDataPrefix
                + MakeSafeModDataPart(npc.Name ?? "unknown") + "/"
                + MakeSafeModDataPart(StringUtils.FirstNonEmpty(notice.ItemType, "Item")) + "/"
                + MakeSafeModDataPart(notice.EntryId);
        }

        private bool HasSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
        {
            if (npc == null || notice == null || Game1.player == null)
                return false;

            string key = GetSpecialItemMemoryKey(npc, notice);
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return Game1.player.modData.TryGetValue(key, out string val)
                && int.TryParse(val, out int count)
                && count > 0;
        }

        private int GetSpecialItemSeenCount(NPC npc, SpecialItemNoticeInfo notice)
        {
            if (npc == null || notice == null || Game1.player == null)
                return 0;

            string key = GetSpecialItemMemoryKey(npc, notice);
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            return Game1.player.modData.TryGetValue(key, out string val) && int.TryParse(val, out int count)
                ? Math.Max(0, count)
                : 0;
        }

        private string BuildSpecialItemMemoryContext(NPC npc, SpecialItemNoticeInfo notice)
        {
            int count = GetSpecialItemSeenCount(npc, notice);
            if (count <= 0 || notice == null)
                return "";

            string displayName = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? "this special item";
            return count == 1
                ? $"This NPC has seen the farmer wear {displayName} before (1 time). They may recognize it with familiarity."
                : $"This NPC has seen the farmer wear {displayName} before ({count} times). They should recognize it as something they've seen before.";
        }

        private void RecordSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
        {
            if (npc == null || notice == null || !notice.IsValid || Game1.player == null)
                return;

            string key = GetSpecialItemMemoryKey(npc, notice);
            if (string.IsNullOrWhiteSpace(key))
                return;

            int count = 0;
            if (Game1.player.modData.TryGetValue(key, out string existing))
                int.TryParse(existing, out count);
            Game1.player.modData[key] = (count + 1).ToString();

            LogSpecialItemDebugOnce($"record-memory|{key}|{count + 1}",
                $"Recorded memory key='{key}' oldCount={count} newCount={count + 1} notice={DescribeSpecialItemNotice(notice)}");

            // Keep a small human-readable companion value for future debugging/save inspection.
            string nameKey = key + "/Name";
            Game1.player.modData[nameKey] = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? notice.EntryId;
        }

    }
}
