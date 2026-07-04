using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OutfitReactions.Ai;

namespace OutfitReactions
{
    public sealed partial class ModEntry
    {
        // ── AI context building — outfit change type, visual summary, prompt helpers ──

        private bool TryGetCurrentSavedFashionSenseOutfitId(out string outfitId)
        {
            outfitId = null;

            if (fsApi == null)
                return false;

            try
            {
                KeyValuePair<bool, string> currentOutfit = fsApi.GetCurrentOutfitId();

                if (!currentOutfit.Key || string.IsNullOrWhiteSpace(currentOutfit.Value))
                    return false;

                outfitId = currentOutfit.Value.Trim();
                return true;
            }
            catch (Exception ex)
            {
                if (DebugLog) Monitor.Log($"[FS] Could not read current saved outfit from Fashion Sense API: {ex.Message}", LogLevel.Info);
                return false;
            }
        }

        private string GetCurrentGameLanguageForPrompt()
        {
            return LocalizedContentManager.CurrentLanguageCode switch
            {
                LocalizedContentManager.LanguageCode.pt => "Brazilian Portuguese",
                LocalizedContentManager.LanguageCode.es => "Spanish",
                LocalizedContentManager.LanguageCode.de => "German",
                LocalizedContentManager.LanguageCode.fr => "French",
                LocalizedContentManager.LanguageCode.it => "Italian",
                LocalizedContentManager.LanguageCode.ja => "Japanese",
                LocalizedContentManager.LanguageCode.ko => "Korean",
                LocalizedContentManager.LanguageCode.ru => "Russian",
                LocalizedContentManager.LanguageCode.tr => "Turkish",
                LocalizedContentManager.LanguageCode.zh => "Chinese",
                LocalizedContentManager.LanguageCode.hu => "Hungarian",
                LocalizedContentManager.LanguageCode.th => "Thai",
                _ => "English"
            };
        }

        private OutfitAiContext BuildOutfitAiContext(NPC npc, bool isSpouseDialogue)
        {
            if (npc == null || lastFashionSenseChangeInfo == null)
                return null;

            FashionSenseChangeInfo effectiveChangeInfo = GetEffectiveFashionSenseChangeInfoForNpc(npc);
            if (effectiveChangeInfo == null)
                return null;

            string dialogueKey = GetFashionSenseDialogueKey(effectiveChangeInfo);
            if (string.IsNullOrWhiteSpace(dialogueKey))
                return null;

            // Accessory-only changes can happen while a saved outfit is still equipped.
            // Do not lose the saved outfit/theme just because the saved outfit ID itself
            // did not change in this Hand Mirror session.
            string outfitName = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveChangeInfo.NewOutfitId);

            bool useFsIdHint = Config?.UseFsInternalIdAsHint != false;
            string safeOutfitHint = (useFsIdHint && !string.IsNullOrWhiteSpace(outfitName)) ? BuildSafeOutfitNameHint(outfitName) : "";
            string noticedChangeType = GetFashionSenseChangeType(effectiveChangeInfo);
            string noticedChangeName = GetFashionSenseChangedItemId(effectiveChangeInfo, noticedChangeType);
            string safeNoticedChangeHint = (useFsIdHint && !string.IsNullOrWhiteSpace(noticedChangeName)) ? BuildSafeOutfitNameHint(noticedChangeName) : "";
            GameLocation location = Game1.currentLocation;
            string locationName = location != null ? location.NameOrUniqueName : "";
            string season = Game1.currentSeason;
            int time = Game1.timeOfDay;
            string playerName = (Game1.player?.Name ?? "").Trim();
            string playerGender = Game1.player.IsMale ? "male" : "female";
            string targetLanguage = GetCurrentGameLanguageForPrompt();
            (string relationshipStatus, int relationshipHearts) = GetRelationshipDialogueContext(npc);
            // Build a location-qualified dialogue key for the AI context (e.g. Clothes.FarmHouse, Clothes.Outside).
            // This is the same suffix logic the old JSON system used, kept here so the AI knows the location sub-type.
            string contextualDialogueKey = dialogueKey;
            if (!string.IsNullOrWhiteSpace(dialogueKey))
            {
                bool _ctxFarmHouse = IsFarmHouseLocation(location);
                bool _ctxOutdoors  = location != null && location.IsOutdoors;
                bool _ctxNpcRoom   = !_ctxFarmHouse && !_ctxOutdoors && IsMarriageCandidateNpcRoom(npc, location);
                if (_ctxFarmHouse) contextualDialogueKey = dialogueKey + ".FarmHouse";
                else if (_ctxNpcRoom) contextualDialogueKey = dialogueKey + ".NpcRoom";
                else if (!_ctxOutdoors) contextualDialogueKey = dialogueKey + ".Inside";
                else contextualDialogueKey = dialogueKey + ".Outside";
            }
            bool isFarmHouse = IsFarmHouseLocation(location);
            bool isOutdoors = location != null && location.IsOutdoors;
            bool isNpcRoom = !isFarmHouse && !isOutdoors && IsMarriageCandidateNpcRoom(npc, location);
            bool isNpcPersonalLocation = !isFarmHouse && !isOutdoors && IsMarriageCandidatePersonalLocation(npc, location);
            bool isIndoors = location != null && !isOutdoors;
            // Vanilla-hat state for this reaction:
            //  - exclusive mode: a vanilla hat is currently equipped -> we react to the hat by NAME
            //    (from the game), so the rendered image and pixel-color sampling add nothing and can
            //    mislead (e.g. sampling the hair color). Skip both.
            //  - just removed: the farmer took a vanilla hat off -> there is no hat to color, and
            //    pixel sampling would grab the now-exposed hair/head color and report it as a "hat
            //    color". Skip the hat-color merge so the AI doesn't invent a colored hat.
            bool vanillaHatEquipped = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());
            bool vanillaHatJustRemoved = effectiveChangeInfo?.VanillaHatRemoved == true;
            // A vanilla hat is either known by name (visible now) or absent (just removed).
            // In both cases, the rendered image can mislead the model into treating exposed hair
            // pixels as a "hat". Keep vanilla-hat equip/removal reactions text-grounded.
            bool skipVisionForVanillaHat = vanillaHatEquipped || vanillaHatJustRemoved;

            OutfitVisionImage visionImage = skipVisionForVanillaHat ? null : TryCaptureVisionOutfitImageForAi();
            string fashionSenseVisualSummary = TryBuildFashionSenseVisualSummaryForAi(effectiveChangeInfo);
            fashionSenseVisualSummary = MergeRenderedHairColorIntoSummary(fashionSenseVisualSummary, visionImage, effectiveChangeInfo);
            // Only merge a rendered HAT color when we're NOT in a vanilla-hat scenario (equipped or
            // just removed), to avoid sampling hair/head pixels as a fake hat color.
            if (!vanillaHatEquipped && !vanillaHatJustRemoved)
                fashionSenseVisualSummary = MergeRenderedHatColorIntoSummary(fashionSenseVisualSummary, visionImage, effectiveChangeInfo);
            // If the player just TOOK OFF a vanilla hat, only NPCs who actually SAW them wearing it
            // should react to the removal. An NPC the farmer never interacted with while wearing the
            // hat has no memory of it and must not act like they remember it ("good, you took that
            // off"). We gate the whole removal framing on this NPC having a remembered hat.
            string rememberedRemovedHatName = vanillaHatJustRemoved
                ? (hatMemoryService?.GetLastHatNameForNpc(npc.Name) ?? "")
                : "";
            bool thisNpcRemembersRemovedHat = vanillaHatJustRemoved && !string.IsNullOrWhiteSpace(rememberedRemovedHatName);

            string vanillaHatFramingText = "";
            if (thisNpcRemembersRemovedHat)
            {
                string removalFraming = " The farmer just took off the hat they had been wearing and is now bare-headed; react to them having removed it. Do not describe or invent a color for the hat (it is no longer worn).";
                fashionSenseVisualSummary = (fashionSenseVisualSummary ?? "").TrimEnd() + removalFraming;
                vanillaHatFramingText = removalFraming.Trim();
            }

            // Deep vanilla-hat memory (independent of any saved Fashion Sense outfit): lets the NPC
            // remember hats this farmer wore in past sessions/days. Kept as its OWN prompt field
            // (not buried at the end of the technical visual-summary block) so the AI actually acts
            // on it instead of treating every wear as the first time.
            string hatMemoryHint = BuildVanillaHatMemoryContext(npc);
            string specialHatReactionContext;
            if (thisNpcRemembersRemovedHat)
            {
                // This NPC saw the farmer wearing the hat, so feed the special-hat opinion of the
                // removed hat (looked up by the remembered name) so their reaction reflects how they
                // felt about it — without naming it.
                specialHatReactionContext = specialHatReactionService?.BuildContextForRemovedHat(rememberedRemovedHatName, targetLanguage) ?? "";
            }
            else if (!vanillaHatJustRemoved && vanillaHatEquipped)
            {
                // Only feed special vanilla-hat context when the vanilla hat is truly visible.
                // If a Fashion Sense hat/headwear is covering it, the NPC should react to the
                // visible Fashion Sense look or saved outfit instead.
                specialHatReactionContext = specialHatReactionService?.BuildContextForCurrentVanillaHat(Game1.player, targetLanguage) ?? "";
            }
            else
            {
                // Removal, but this NPC never saw the hat: no special-hat context at all.
                specialHatReactionContext = "";
            }

            // Special item reaction context (e.g. Mayor's shorts).
            // This is now a per-NPC primary focus, like HatOnly: visible special items are reacted
            // to as the item itself, and removed special items only apply to NPCs who remember them.
            bool specialItemOnlyMode = TryResolveSpecialItemNoticeForNpc(
                npc,
                effectiveChangeInfo,
                requireNpcMemoryForRemoval: true,
                out SpecialItemNoticeInfo specialItemNotice);

            string specialItemReactionContext = specialItemOnlyMode ? (specialItemNotice?.ReactionContext ?? "") : "";
            bool specialItemWasJustRemoved = specialItemOnlyMode && (specialItemNotice?.WasRemoved == true);
            string specialItemMemoryHint = specialItemOnlyMode ? (specialItemNotice?.MemoryHint ?? "") : "";

            if (specialItemOnlyMode && DebugLog)
                Monitor.Log(
                    $"[SPECIAL ITEM PROMPT] NPC={npc.Name} entry='{specialItemNotice?.EntryId}' removed={specialItemWasJustRemoved} " +
                    $"memoryHint='{(string.IsNullOrWhiteSpace(specialItemMemoryHint) ? "<none>" : specialItemMemoryHint)}' " +
                    $"combinedMode={ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined"}",
                    LogLevel.Info);

            // Legacy pants memory hint is kept only for non-special vanilla pants. Special items use
            // SpecialItemMemoryHint so the prompt/memory can stay generic for future item.json entries.
            string vanillaPantsMemoryHint = "";

            return new OutfitAiContext
            {
                NpcName = npc.Name,
                NpcDisplayName = npc.displayName,
                IsSpouse = isSpouseDialogue,
                DialogueKey = contextualDialogueKey,
                OutfitName = outfitName,
                SafeOutfitHint = safeOutfitHint,
                // Content-pack-driven outfit theme rules (OutfitThemes asset) were removed —
                // this system was never used by any published content pack.
                ThemeContext = "",
                ThemePriorityInstruction = "",
                LocationName = locationName,
                DetailedLocationName = GetDetailedLocationNameForAiPrompt(location),
                LocationType = GetLocationTypeForAiPrompt(location, isFarmHouse, isOutdoors, isNpcRoom),
                IsOutdoors = isOutdoors,
                IsIndoors = isIndoors,
                IsNpcRoom = isNpcRoom,
                IsNpcPersonalLocation = isNpcPersonalLocation,
                IsBeachOrIsland = IsBeachOrIslandLocation(location),
                IsFarmHouse = isFarmHouse,
                DayPart = GetDayPartForAiPrompt(time),
                FestivalContext = GetFestivalContextForAiPrompt(),
                FarmerBirthdayContext = GetFarmerBirthdayContextForAiPrompt(),
                Season = season,
                Weather = GetCurrentWeatherForAiPrompt(),
                Time = time,
                DayOfSeason = Game1.dayOfMonth,
                Year = Game1.year,
                PlayerName = playerName,
                PlayerGender = playerGender,
                TargetLanguage = targetLanguage,
                RelationshipStatus = relationshipStatus,
                RelationshipHearts = relationshipHearts,
                VisionImage = visionImage,
                FashionSenseVisualSummary = fashionSenseVisualSummary,
                // HatOnly is valid for a visible vanilla hat, or for a removed vanilla hat ONLY
                // when this specific NPC actually remembers seeing it before. Otherwise nearby NPCs
                // would be forced into a hat topic they never witnessed and could invent a fake hat.
                VanillaHatHatOnlyMode = !specialItemOnlyMode
                    && (vanillaHatEquipped || thisNpcRemembersRemovedHat)
                    && ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) == "HatOnly",
                VanillaHatFraming = vanillaHatFramingText,
                NpcWitnessedPreviousAccessory = DidNpcWitnessPreviousLook(npc),
                SpecialHatReactionContext = specialHatReactionContext,
                SpecialItemReactionContext = specialItemReactionContext,
                SpecialItemWasJustRemoved = specialItemWasJustRemoved,
                SpecialItemOnlyMode = specialItemOnlyMode,
                SpecialItemCombinedMode = specialItemOnlyMode
                    && ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined",
                SpecialItemMemoryHint = specialItemMemoryHint,
                VanillaPantsMemoryHint = vanillaPantsMemoryHint,
                VanillaHatMemoryHint = hatMemoryHint,
                AvailablePortraitCount = GetNpcPortraitCount(npc),
                NoticedChangeType = noticedChangeType,
                NoticedChangeName = noticedChangeName,
                SafeNoticedChangeHint = safeNoticedChangeHint,
                // Only non-spouse NPCs use the walking peeping mechanic; spouse has its own flow.
                WasCaughtPeeking = !isSpouseDialogue && (otherNpcClothesReactionSystem?.WasNpcCaughtPeeking(npc) ?? false),
                OutfitMemoryContext = BuildOutfitMemoryContext(npc, outfitName)
            };
        }

        private string BuildOutfitMemoryContext(NPC npc, string outfitId)
        {
            if (outfitMemoryService == null || npc == null)
                return null;

            string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
            if (string.IsNullOrWhiteSpace(currentOutfitId))
                return null;

            OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
            var memory = outfitMemoryService.GetMemory(npc.Name, currentOutfitId, current);
            if (memory == null)
                return null;

            return outfitMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
        }

        private void RecordOutfitMemory(NPC npc, string outfitId)
        {
            if (outfitMemoryService == null || npc == null)
                return;

            string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
            if (string.IsNullOrWhiteSpace(currentOutfitId))
                return;

            OutfitComponents components = BuildCurrentOutfitComponentsForMemory();

            outfitMemoryService.RecordMemory(
                npc.Name, currentOutfitId, components,
                Game1.currentSeason, Game1.dayOfMonth, Game1.year);
        }

        private string GetCurrentSavedFashionSenseOutfitIdForAi(string fallbackOutfitId = null)
        {
            if (TryGetCurrentSavedFashionSenseOutfitId(out string currentOutfitId) && !string.IsNullOrWhiteSpace(currentOutfitId))
                return currentOutfitId;

            return fallbackOutfitId ?? "";
        }

        private OutfitComponents BuildCurrentOutfitComponentsForMemory()
        {
            FashionSenseSnapshot snapshot = CaptureFashionSenseSnapshot();
            if (snapshot != null)
            {
                // Snapshot.VanillaHat is already visibility-filtered. If the vanilla hat is
                // hidden under Fashion Sense headwear, remember the visible Fashion Sense hat
                // instead of leaking the hidden vanilla slot into outfit memory.
                string memoryHat = !string.IsNullOrWhiteSpace(snapshot.VanillaHat)
                    ? ("vanilla:" + snapshot.VanillaHat)
                    : (snapshot.Hat ?? "");
                return new OutfitComponents
                {
                    Hat       = memoryHat,
                    Hair      = snapshot.Hair    ?? "",
                    Shirt     = snapshot.Shirt   ?? "",
                    Pants     = snapshot.Pants   ?? "",
                    Sleeves   = snapshot.Sleeves ?? "",
                    Accessory = BuildCurrentAccessoryMemoryValue(snapshot)
                };
            }

            return new OutfitComponents
            {
                Hat       = lastFashionSenseChangeInfo?.NewHatId       ?? "",
                Hair      = lastFashionSenseChangeInfo?.NewHairId      ?? "",
                Shirt     = lastFashionSenseChangeInfo?.NewShirtId     ?? "",
                Pants     = lastFashionSenseChangeInfo?.NewPantsId     ?? "",
                Sleeves   = lastFashionSenseChangeInfo?.NewSleevesId   ?? "",
                Accessory = lastFashionSenseChangeInfo?.NewAccessoryId ?? ""
            };
        }

        private static string BuildCurrentAccessoryMemoryValue(FashionSenseSnapshot snapshot)
        {
            if (snapshot == null)
                return "";

            return string.Join(" + ", new[]
            {
                NormalizeFashionSenseAccessoryId(snapshot.Accessory),
                NormalizeFashionSenseAccessoryId(snapshot.AccessorySecondary),
                NormalizeFashionSenseAccessoryId(snapshot.AccessoryTertiary)
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string NormalizeFashionSenseAccessoryId(string accessoryId)
        {
            if (string.IsNullOrWhiteSpace(accessoryId))
                return "";

            string trimmed = accessoryId.Trim();
            return IsIgnoredFashionSenseAccessoryId(trimmed) ? "" : trimmed;
        }

        private static bool IsIgnoredFashionSenseAccessoryId(string accessoryId)
        {
            if (string.IsNullOrWhiteSpace(accessoryId))
                return false;

            string humanized = FashionSenseVisualService.HumanizeAppearanceId(accessoryId);
            string lower = " " + string.Join(" ", new[] { accessoryId, humanized })
                .ToLowerInvariant()
                .Replace('_', ' ')
                .Replace('-', ' ')
                .Replace('.', ' ')
                .Replace('/', ' ') + " ";

            bool isEyeCosmetic =
                (lower.Contains(" eye ") || lower.Contains(" eyes ") || lower.Contains(" olho ") || lower.Contains(" olhos "))
                && (lower.Contains(" highlight ")
                    || lower.Contains(" highlights ")
                    || lower.Contains(" sparkle ")
                    || lower.Contains(" sparkles ")
                    || lower.Contains(" shine ")
                    || lower.Contains(" glitter ")
                    || lower.Contains(" gloss ")
                    || lower.Contains(" brilho ")
                    || lower.Contains(" brilhos "));

            bool isFaceCosmetic =
                (lower.Contains(" face ") || lower.Contains(" facial ") || lower.Contains(" rosto "))
                && (lower.Contains(" makeup ")
                    || lower.Contains(" maquiagem ")
                    || lower.Contains(" highlight ")
                    || lower.Contains(" blush ")
                    || lower.Contains(" sparkle ")
                    || lower.Contains(" shine ")
                    || lower.Contains(" glitter ")
                    || lower.Contains(" gloss ")
                    || lower.Contains(" brilho "));

            return isEyeCosmetic
                || isFaceCosmetic
                || lower.Contains(" makeup ")
                || lower.Contains(" maquiagem ")
                || lower.Contains(" blush ")
                || lower.Contains(" lipstick ")
                || lower.Contains(" batom ")
                || lower.Contains(" eyeshadow ")
                || lower.Contains(" eye shadow ")
                || lower.Contains(" sombra ")
                || lower.Contains(" eyeliner ")
                || lower.Contains(" delineador ")
                || lower.Contains(" rimel ")
                || lower.Contains(" rímel ");
        }

        private string GetFashionSenseChangeType(FashionSenseChangeInfo changeInfo)
        {
            if (changeInfo == null)
                return "";

            bool visionOn = AreVisionOnlyFashionSenseTriggersEnabled();

            // A full saved outfit should be treated as the full look, not as whichever
            // Fashion Sense slot happened to change inside it. This prevents generic
            // bow/tiara/head-slot IDs like "pack0005 hat 3" from making every NPC talk
            // about a "hat" instead of the actual pink/white outfit theme.
            if (changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId))
            {
                // Keep the special combo behavior only for meaningful visible accessories
                // like wings/capes/umbrellas. Do not let generic headwear/hair slots steal
                // the focus from the saved outfit.
                if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, visionOn))
                    return "Accessory";

                return "Outfit";
            }

            if (ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
                return "Outfit";

            if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, visionOn))
                return "Accessory";

            if (changeInfo.VanillaHatChanged)
                return "Hat";

            if (changeInfo.ChangedHat
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId)
                && (visionOn || ItemNameRevealsShape(changeInfo.NewHatId)))
                return "Hat";

            if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
                return "Hair";

            return "";
        }

        private bool ShouldTreatAccessoryAsCurrentComboFocus(string accessoryId, bool visionOn)
        {
            if (string.IsNullOrWhiteSpace(accessoryId))
                return false;

            if (IsIgnoredFashionSenseAccessoryId(accessoryId))
                return false;

            // Generic internal IDs are not strong enough to become the main topic.
            // Vision may still show the whole outfit, but the prompt should not say
            // "accessory" just because a tiny coded slot changed.
            if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(accessoryId))
                return false;

            return visionOn || ItemNameRevealsShape(accessoryId);
        }

        private bool ShouldTreatGenericHeadwearAsSavedOutfitPart(FashionSenseChangeInfo changeInfo)
        {
            if (changeInfo == null || !changeInfo.ChangedHat)
                return false;

            if (string.IsNullOrWhiteSpace(changeInfo.NewHatId))
                return false;

            if (!FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId))
                return false;

            string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(changeInfo.NewOutfitId);
            return !string.IsNullOrWhiteSpace(currentOutfitId);
        }

        private static string GetFashionSenseChangedItemId(FashionSenseChangeInfo changeInfo, string changeType)
        {
            if (changeInfo == null)
                return "";

            if (string.Equals(changeType, "Outfit", StringComparison.OrdinalIgnoreCase))
                return changeInfo.NewOutfitId ?? "";
            if (string.Equals(changeType, "Hair", StringComparison.OrdinalIgnoreCase))
                return changeInfo.NewHairId ?? "";
            if (string.Equals(changeType, "Hat", StringComparison.OrdinalIgnoreCase))
                return changeInfo.NewHatId ?? "";
            if (string.Equals(changeType, "Accessory", StringComparison.OrdinalIgnoreCase))
                return StringUtils.FirstNonEmpty(changeInfo.NewAccessoryId, "unknown-accessory-change") ?? "";

            return "";
        }

        private string TryBuildFashionSenseVisualSummaryForAi(FashionSenseChangeInfo effectiveChangeInfo)
        {
            // This summary is plain TEXT (item names + colors), so it helps text-only AI models
            // too, not just vision models. It no longer requires vision mode to be enabled.
            if (fashionSenseVisualService == null || Game1.player == null)
                return "";

            FashionSenseChangeInfo activeChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
            string currentOutfitId = GetCurrentSavedFashionSenseOutfitIdForAi(activeChangeInfo?.NewOutfitId);
            bool suppressHairAndGenericHeadwearForSavedOutfit = activeChangeInfo != null
                && (activeChangeInfo.ChangedOutfit
                    || ShouldTreatGenericHeadwearAsSavedOutfitPart(activeChangeInfo));

            bool visibleVanillaHatEquipped = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());

            if (fashionSenseVisualService.TryBuildVisualSummary(Game1.player, currentOutfitId, out string summary, out string reason, suppressHairAndGenericHeadwearForSavedOutfit, visibleVanillaHatEquipped))
            {
                string playerDescription = GetPlayerProvidedAccessoryDescriptionForCurrentChange(activeChangeInfo);
                if (!string.IsNullOrWhiteSpace(playerDescription))
                    summary += "; player-provided description for the current small accessory/change: " + playerDescription;

                return summary;
            }

            Monitor.Log(" Vision outfit analysis is enabled, but Fashion Sense API visual support data could not be read: " + reason, LogLevel.Trace);
            string fallbackPlayerDescription = GetPlayerProvidedAccessoryDescriptionForCurrentChange(activeChangeInfo);
            return string.IsNullOrWhiteSpace(fallbackPlayerDescription)
                ? ""
                : "Player-provided description for the current small accessory/change: " + fallbackPlayerDescription;
        }

        private string MergeRenderedHairColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
        {
            // Only treat the rendered top-of-sprite color as HAIR when the current change is a
            // hair change AND no hat is equipped. Otherwise the "hair" pixels are actually the
            // hat covering the head, which would report the hat's color as the hair color.
            FashionSenseChangeInfo activeChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
            bool currentChangeIsHair = activeChangeInfo != null
                && activeChangeInfo.ChangedHair;
            bool hatCoveringHead = activeChangeInfo != null
                && !string.IsNullOrWhiteSpace(activeChangeInfo.NewHatId);

            if (!currentChangeIsHair || hatCoveringHead)
                return summary;

            if (visionImage == null || !visionImage.HasHairColor || string.IsNullOrWhiteSpace(visionImage.HairColorName))
                return summary;

            // If the summary already states a CONFIRMED hair color (a real FS tint), trust that
            // and don't override it with the pixel estimate.
            if (!string.IsNullOrWhiteSpace(summary) &&
                summary.IndexOf("CONFIRMED hair color", StringComparison.OrdinalIgnoreCase) >= 0)
                return summary;

            string hairClue = "CONFIRMED hair color from the rendered sprite (authoritative, use exactly this; do NOT take hair color from the raw image): "
                + visionImage.HairColorName + " (" + visionImage.HairColorHex + ")";

            if (string.IsNullOrWhiteSpace(summary))
            {
                return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: "
                    + hairClue;
            }

            return summary + "; " + hairClue;
        }

        private string MergeRenderedHatColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
        {
            FashionSenseChangeInfo activeChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
            bool currentChangeIsHat = activeChangeInfo != null
                && activeChangeInfo.ChangedHat
                && !activeChangeInfo.ChangedOutfit
                && !ShouldTreatGenericHeadwearAsSavedOutfitPart(activeChangeInfo)
                && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(activeChangeInfo.NewHatId)
                && !string.IsNullOrWhiteSpace(activeChangeInfo.NewHatId);

            if (!currentChangeIsHat)
                return summary;

            if (visionImage == null || !visionImage.HasHatColor || string.IsNullOrWhiteSpace(visionImage.HatColorName))
                return summary;

            if (!string.IsNullOrWhiteSpace(summary) &&
                summary.IndexOf("CONFIRMED hat color", StringComparison.OrdinalIgnoreCase) >= 0)
                return summary;

            string hatClue = "CONFIRMED hat/headwear color from the rendered sprite (authoritative, use exactly this; do NOT take hat color from the raw image): "
                + visionImage.HatColorName + " (" + visionImage.HatColorHex + ")";

            if (string.IsNullOrWhiteSpace(summary))
            {
                return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: "
                    + hatClue;
            }

            return summary + "; " + hatClue;
        }

        private string GetPlayerProvidedAccessoryDescriptionForCurrentChange(FashionSenseChangeInfo effectiveChangeInfo = null)
        {
            FashionSenseChangeInfo activeChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
            if (Game1.player == null || activeChangeInfo == null)
                return "";

            string accessoryId = activeChangeInfo.NewAccessoryId ?? "";
            if (string.IsNullOrWhiteSpace(accessoryId))
                return "";

            string key = GetPlayerAccessoryDescriptionModDataKey(accessoryId);
            return Game1.player.modData.TryGetValue(key, out string description)
                ? CleanPlayerOutfitReplyText(description)
                : "";
        }

        private void SavePlayerProvidedAccessoryDescriptionForCurrentChange(string description)
        {
            if (Game1.player == null || lastFashionSenseChangeInfo == null)
                return;

            if (!lastFashionSenseChangeInfo.ChangedAccessory || string.IsNullOrWhiteSpace(lastFashionSenseChangeInfo.NewAccessoryId))
                return;

            string cleaned = CleanPlayerOutfitReplyText(description);
            if (string.IsNullOrWhiteSpace(cleaned))
                return;

            Game1.player.modData[GetPlayerAccessoryDescriptionModDataKey(lastFashionSenseChangeInfo.NewAccessoryId)] = cleaned;
            if (DebugLog) Monitor.Log("[FS VISUAL] Saved player-provided description for a small accessory/change.", LogLevel.Info);
        }

        private static string GetPlayerAccessoryDescriptionModDataKey(string accessoryId)
        {
            return PlayerAccessoryDescriptionModDataPrefix + GetStableHexHash(accessoryId ?? "");
        }

        private OutfitVisionImage TryCaptureVisionOutfitImageForAi()
        {
            // The sprite is rendered to read authoritative hair/hat/clothing colors from pixels.
            // This is useful even for NON-vision (text-only) AI models, because the colors are
            // turned into TEXT before reaching the AI. So we render whenever possible, and only
            // strip the actual image bytes when the model/provider can't use a picture.
            if (outfitVisionService == null || Game1.player == null)
                return null;

            if (!outfitVisionService.TryCaptureFarmerAppearance(Game1.player, out OutfitVisionImage image, out string reason))
            {
                Monitor.Log(" Could not render the farmer sprite for color reading: " + reason, LogLevel.Trace);
                return null;
            }

            // The image is sent ONLY when the active model supports vision, and only so the AI
            // can see the SHAPE of items (hat, wings, bows...). Color always comes from text
            // (pixel reading + Fashion Sense data), never from the image. Auto-detected: no toggle.
            bool sendImageToAi = ShouldTryVisionForCurrentAiProvider();
            if (!sendImageToAi && image != null)
            {
                // Keep the pixel-derived colors, but drop the image bytes so no picture is sent.
                image.Base64Data = "";
            }

            return image;
        }

        private bool ShouldTryVisionForCurrentAiProvider()
        {
            // Single source of truth: the config decides based on the active profile's vision
            // mode (Auto/On/Off) and, when Auto, the model name. This works for ANY provider —
            // a multimodal DeepSeek is detected/forced On, a text-only OpenRouter model stays Off.
            return Config?.ShouldSendImageToActiveModel() == true;
        }

        private string GetDetailedLocationNameForAiPrompt(GameLocation location)
        {
            if (location == null)
                return "unknown";

            string internalName = location.NameOrUniqueName ?? location.Name ?? "unknown";
            string displayName = internalName;

            try
            {
                PropertyInfo property = location.GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetValue(location) is string found && !string.IsNullOrWhiteSpace(found))
                    displayName = found;
            }
            catch
            {
                // Best-effort only.
            }

            return string.Equals(displayName, internalName, StringComparison.OrdinalIgnoreCase)
                ? internalName
                : displayName + " (internal map: " + internalName + ")";
        }

        private string GetLocationTypeForAiPrompt(GameLocation location, bool isFarmHouse, bool isOutdoors, bool isNpcRoom)
        {
            if (location == null)
                return "unknown";
            if (isFarmHouse)
                return "farmer farmhouse / home interior";
            if (isNpcRoom)
                return "marriage-candidate NPC room";
            if (location != null && !isFarmHouse && !isOutdoors)
            {
                // This may be the NPC's house/home interior even when the exact bedroom area
                // cannot be detected from the map alone.
                string name = (location.NameOrUniqueName ?? location.Name ?? "").ToLowerInvariant();
                if (name.Contains("house") || name.Contains("home") || name.Contains("shop") || name.Contains("trailer") || name.Contains("room") || name.Contains("basement"))
                    return "marriage-candidate home/private interior";
            }
            if (isOutdoors)
                return "outdoors";
            return "indoors";
        }

        private string GetDayPartForAiPrompt(int time)
        {
            if (time < 1200)
                return "morning";
            if (time < 1800)
                return "afternoon";
            return "night";
        }

        private string GetFestivalContextForAiPrompt()
        {
            bool isFestivalDay = false;
            try
            {
                MethodInfo method = typeof(Utility).GetMethod("isFestivalDay", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int), typeof(string) }, null);
                if (method?.Invoke(null, new object[] { Game1.dayOfMonth, Game1.currentSeason }) is bool found)
                    isFestivalDay = found;
            }
            catch
            {
                // Best-effort only.
            }

            return isFestivalDay
                ? "Today is a festival day in the current season. A subtle outfit reaction may reference the festive atmosphere if it fits naturally."
                : "Today is not a festival day.";
        }

        private string GetFarmerBirthdayContextForAiPrompt()
        {
            string birthdaySeason = (Config.FarmerBirthdaySeason ?? "").Trim();
            int birthdayDay = Config.FarmerBirthdayDay;

            if (string.IsNullOrWhiteSpace(birthdaySeason) || birthdayDay <= 0)
                return "Farmer birthday is not configured.";

            bool isBirthday = birthdayDay == Game1.dayOfMonth
                && birthdaySeason.Equals(Game1.currentSeason, StringComparison.OrdinalIgnoreCase);

            return isBirthday
                ? "Today is the farmer's birthday. The compliment may feel a little more special if it fits the NPC and relationship."
                : "Today is not the farmer's birthday. Farmer birthday is configured as " + birthdaySeason + " " + birthdayDay + ".";
        }

        private string GetCurrentWeatherForAiPrompt()
        {
            return Game1.isGreenRain ? "green rain" :
                Game1.isLightning ? "storm / thunderstorm" :
                Game1.isRaining ? "rain" :
                Game1.isSnowing ? "snow" :
                Game1.isDebrisWeather ? "windy / debris weather" :
                "sunny / clear";
        }

        private static string BuildSafeOutfitNameHint(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "No readable saved outfit name was provided.";

            string text = rawName.Trim();

            // Turn common separators into spaces: Coffee_04 -> Coffee 04.
            text = Regex.Replace(text, @"[_\-.]+", " ");

            // Split simple camelCase/PascalCase names: CuteFairy5 -> Cute Fairy5.
            text = Regex.Replace(text, @"([a-zà-ÿ])([A-Z])", "$1 $2");

            // Remove trailing numbers/order/version suffixes that players often use only to organize outfits.
            text = Regex.Replace(
                text,
                @"\s*(?:#|n[ºo]?\.?|v|ver|version|versao|versão|set)?\s*\d+\s*$",
                "",
                RegexOptions.IgnoreCase
            );

            // Clean duplicated spaces.
            text = Regex.Replace(text, @"\s{2,}", " ").Trim();

            if (string.IsNullOrWhiteSpace(text))
                return "The saved outfit name is only an internal label and does not provide a readable theme.";

            return text;
        }

    }
}
