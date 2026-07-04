using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace OutfitReactions
{
    /// <summary>
    /// SMAPI console command handlers for <see cref="ModEntry"/> (registered in Entry). These are
    /// diagnostic/maintenance commands — clearing outfit notice memory, reporting voice-sample
    /// coverage, previewing prompt voice lines, and explaining why an outfit notice can/can't start.
    /// </summary>
    public sealed partial class ModEntry
    {
        private void ClearOutfitNoticeMemoryCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady || Game1.player == null)
            {
                Monitor.Log("[OC DEBUG] Load a save before clearing outfit notice memory.", LogLevel.Warn);
                return;
            }

            List<string> keysToRemove = Game1.player.modData.Keys
                .Where(key => key.StartsWith(OutfitNoticeModDataPrefix, StringComparison.OrdinalIgnoreCase)
                    || key.StartsWith(SpecialItemSeenModDataPrefix, StringComparison.OrdinalIgnoreCase)
                    || key.StartsWith(PantsSeenModDataPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (string key in keysToRemove)
                Game1.player.modData.Remove(key);

            ResetClothesState(clearChangeFlag: true);
            npcsReactedToCurrentNotice.Clear();
            otherNpcClothesReactionSystem?.Reset();

            Monitor.Log($"[OC DEBUG] Cleared {keysToRemove.Count} outfit notice memory key(s) from this save and reset pending outfit notice state.", LogLevel.Info);
        }

        private void VoiceSampleReportCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Load a save first, then run oc_test_voicesamples so NPC dialogue is available.", LogLevel.Warn);
                return;
            }
            if (outfitAiService == null)
            {
                Monitor.Log("The outfit AI service is not ready yet.", LogLevel.Warn);
                return;
            }

            string report = outfitAiService.BuildVoiceSampleReport();
            Monitor.Log(report, LogLevel.Info);
        }

        private void VoiceSamplePreviewCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Load a save first, then run oc_preview_voicesamples <NpcName> so NPC dialogue is available.", LogLevel.Warn);
                return;
            }
            if (outfitAiService == null)
            {
                Monitor.Log("The outfit AI service is not ready yet.", LogLevel.Warn);
                return;
            }
            if (args == null || args.Length == 0)
            {
                Monitor.Log("Usage: oc_preview_voicesamples <NpcName>  (e.g. oc_preview_voicesamples Victoria)", LogLevel.Warn);
                return;
            }

            // Allow names with spaces (e.g. "Mr. Aguar") by joining all args.
            string npcName = string.Join(" ", args).Trim();
            string preview = outfitAiService.BuildVoiceSamplePreview(npcName, Game1.currentSeason);
            Monitor.Log(preview, LogLevel.Info);
        }

        private void DebugOutfitNoticeCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady || Game1.player == null)
            {
                Monitor.Log("[OC DEBUG] Load a save before running this command.", LogLevel.Warn);
                return;
            }


            string currentSavedOutfit = TryGetCurrentSavedFashionSenseOutfitId(out string outfitId) ? outfitId : "<none>";
            string dialogueKey = lastFashionSenseChangeInfo != null ? GetFashionSenseDialogueKey(lastFashionSenseChangeInfo) : "";
            string changeSummary = lastFashionSenseChangeInfo == null
                ? "<none>"
                : $"count={lastFashionSenseChangeInfo.CountChanges()} hair={lastFashionSenseChangeInfo.ChangedHair} accessory={lastFashionSenseChangeInfo.ChangedAccessory} hat={lastFashionSenseChangeInfo.ChangedHat} outfit={lastFashionSenseChangeInfo.ChangedOutfit} newHair={lastFashionSenseChangeInfo.NewHairId ?? ""} newAccessory={lastFashionSenseChangeInfo.NewAccessoryId ?? ""} newHat={lastFashionSenseChangeInfo.NewHatId ?? ""} newOutfit={lastFashionSenseChangeInfo.NewOutfitId ?? ""}";

            Monitor.Log($"[OC DEBUG] Config: Enabled={Config.Enabled}, NPC reactions={Config.EnableNpcOutfitReactions}, NPC chance={Config.NpcOutfitReactionChance}, NPC distance={Config.OutfitNoticeDistance}, spouse distance={Config.OutfitNoticeDistance}.", LogLevel.Info);
            Monitor.Log($"[OC DEBUG] Fashion Sense: fsApi={(fsApi != null)}, currentSavedOutfit={currentSavedOutfit}, changedClothes={changedClothes}, dialogueKey='{dialogueKey}', change={changeSummary}.", LogLevel.Info);
            Monitor.Log($"[OC DEBUG] NPC profile reactions: Sebastian={outfitAiService?.HasProfile("Sebastian")}, Penny={outfitAiService?.HasProfile("Penny")}, Robin={outfitAiService?.HasProfile("Robin")}. Any NPC with an enabled profile can react.", LogLevel.Info);

            NPC spouse = GetSpouse();
            if (spouse != null)
            {
                float spouseDistance = DistanceToPlayer(spouse);
                Monitor.Log($"[OC DEBUG] Spouse {spouse.Name}: sameLoc={spouse.currentLocation == Game1.player.currentLocation}, dist={spouseDistance:0}, facing={IsNpcFacingPlayer(spouse)}, canNotice={CanNpcNoticeCurrentOutfitNotice(spouse)}, shouldStart={ShouldStartClothesReaction(spouse)}, firstNoticeDone={clothesFirstNoticeDone}, reacting={isReactingToClothes}, ready={clothesComplimentReady}.", LogLevel.Info);
            }

            if (Game1.currentLocation?.characters == null || Game1.currentLocation.characters.Count <= 0)
            {
                Monitor.Log("[OC DEBUG] No NPCs are currently loaded in the player's location.", LogLevel.Info);
                return;
            }

            foreach (NPC npc in Game1.currentLocation.characters
                .Where(n => n != null)
                .OrderBy(DistanceToPlayer)
                .Take(10))
            {
                float distance = DistanceToPlayer(npc);
                bool sameLocation = npc.currentLocation == Game1.player.currentLocation;
                bool hasProfile = outfitAiService?.HasProfile(npc.Name) == true;
                bool canReact = CanNpcReactToOutfit(npc);
                bool canNotice = CanNpcNoticeCurrentOutfitNotice(npc);
                bool facing = IsNpcFacingPlayer(npc);
                bool tooFar = distance > Math.Max(64f, Config.OutfitNoticeDistance);

                Monitor.Log($"[OC DEBUG] NPC {npc.Name}: dist={distance:0}, sameLoc={sameLocation}, villager={npc.IsVillager}, invisible={npc.IsInvisible}, sleeping={npc.isSleeping.Value}, profile={hasProfile}, canReact={canReact}, canNotice={canNotice}, facing={facing}, tooFar={tooFar}.", LogLevel.Info);
            }
        }
    }
}
