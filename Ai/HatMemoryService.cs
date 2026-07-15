using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace OutfitReactions.Ai
{
    /// <summary>
    /// Tracks, per NPC, the history of VANILLA hats the farmer has worn — completely independent
    /// of the Fashion Sense saved-outfit memory (OutfitMemoryService). This lets an NPC remember
    /// things like "you wore the Sombrero last time", "you've worn this hat several times", or
    /// "you finally took that hat off", with seasonal/temporal context, even when the player has
    /// no saved Fashion Sense outfit at all. Persisted in the save file under its own key.
    /// </summary>
    internal sealed class HatMemoryService
    {
        private const string SaveKey = "VanillaHatMemories";
        private const int CurrentSchemaVersion = 1;

        private readonly IModHelper helper;
        private readonly IMonitor monitor;

        // npcName -> hatId -> entry
        private Dictionary<string, Dictionary<string, HatMemoryEntry>> memories
            = new(StringComparer.OrdinalIgnoreCase);

        // npcName -> the hatId most recently RECORDED for that NPC (for "last time you wore X").
        private Dictionary<string, string> lastHatPerNpc
            = new(StringComparer.OrdinalIgnoreCase);

        public HatMemoryService(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
        }

        /// <summary>
        /// Returns the display name of the last vanilla hat this NPC saw the farmer wearing
        /// (before now), or "" if none/unknown. Used to say specifically WHICH hat was removed.
        /// </summary>
        public string GetLastHatNameForNpc(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return "";
            if (!lastHatPerNpc.TryGetValue(npcName, out string lastId) || string.IsNullOrWhiteSpace(lastId))
                return "";
            if (memories.TryGetValue(npcName, out var npcHats)
                && npcHats.TryGetValue(lastId, out var entry)
                && entry != null
                && !string.IsNullOrWhiteSpace(entry.HatName))
                return entry.HatName;
            return "";
        }

        /// <summary>Load hat memories from the save (call on save loaded).</summary>
        public void Load()
        {
            try
            {
                var saved = helper.Data.ReadSaveData<HatMemoryData>(SaveKey);
                if (saved?.Memories == null)
                {
                    memories = new(StringComparer.OrdinalIgnoreCase);
                    lastHatPerNpc = new(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    memories = saved.Memories;
                    lastHatPerNpc = saved.LastHatPerNpc ?? new(StringComparer.OrdinalIgnoreCase);
                }
                if (OutfitReactions.ModEntry.DebugLog) monitor.Log($"[HAT MEMORY] Loaded vanilla-hat memories for {memories.Count} NPC(s).", LogLevel.Info);
            }
            catch (Exception ex)
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor.Log("[HAT MEMORY] Failed to load hat memories: " + ex.Message, LogLevel.Info);
                memories = new(StringComparer.OrdinalIgnoreCase);
                lastHatPerNpc = new(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>Persist hat memories to the save (call on saving).</summary>
        public void Save()
        {
            try
            {
                helper.Data.WriteSaveData(SaveKey, new HatMemoryData
                {
                    Version = CurrentSchemaVersion,
                    Memories = memories,
                    LastHatPerNpc = lastHatPerNpc
                });
            }
            catch (Exception ex)
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor.Log("[HAT MEMORY] Failed to save hat memories: " + ex.Message, LogLevel.Info);
            }
        }

        /// <summary>
        /// Builds a memory comparison for the hat the NPC is about to react to. Returns null when
        /// there is nothing meaningful to remember (e.g. first time ever, no prior hat history).
        /// Does NOT record anything — call RecordMemory after the reaction is committed.
        /// </summary>
        public HatMemoryComparison GetMemory(string npcName, string currentHatId, string currentHatName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return null;

            string previousHatForNpc = lastHatPerNpc.TryGetValue(npcName, out string prev) ? prev : "";

            // No current hat: this is a "took the hat off" moment. Only meaningful if the NPC has
            // actually seen the farmer wear a hat before.
            bool currentlyHatless = string.IsNullOrWhiteSpace(currentHatId);
            if (currentlyHatless && string.IsNullOrWhiteSpace(previousHatForNpc))
                return null;

            HatMemoryEntry entry = null;
            if (!currentlyHatless
                && memories.TryGetValue(npcName, out var npcHats))
            {
                npcHats.TryGetValue(currentHatId, out entry);
            }


            return new HatMemoryComparison
            {
                CurrentHatId = currentHatId ?? "",
                CurrentHatName = currentHatName ?? "",
                CurrentlyHatless = currentlyHatless,
                PreviousHatId = previousHatForNpc,
                TimesSeenBefore = entry?.TimesSeen ?? 0,
                FirstSeenSeason = entry?.FirstSeenSeason ?? "",
                FirstSeenDay = entry?.FirstSeenDay ?? 0,
                FirstSeenYear = entry?.FirstSeenYear ?? 0,
                LastSeenSeason = entry?.LastSeenSeason ?? "",
                LastSeenDay = entry?.LastSeenDay ?? 0,
                LastSeenYear = entry?.LastSeenYear ?? 0
            };
        }

        /// <summary>
        /// Records that the NPC saw the farmer in this hat (or hatless) now. Updates first/last
        /// seen and counts. Pass the current in-game date for temporal context.
        /// </summary>
        public void RecordMemory(string npcName, string hatId, string hatName,
            string season, int day, int year)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return;

            // Always remember what the NPC last saw, even if hatless ("" = bare-headed),
            // so we can later say "you took the hat off".
            lastHatPerNpc[npcName] = hatId ?? "";

            if (string.IsNullOrWhiteSpace(hatId))
                return; // nothing to count for a bare head

            if (!memories.TryGetValue(npcName, out var npcHats))
            {
                npcHats = new Dictionary<string, HatMemoryEntry>(StringComparer.OrdinalIgnoreCase);
                memories[npcName] = npcHats;
            }

            if (!npcHats.TryGetValue(hatId, out var entry) || entry == null)
            {
                npcHats[hatId] = new HatMemoryEntry
                {
                    HatId = hatId,
                    HatName = hatName ?? "",
                    FirstSeenSeason = season ?? "",
                    FirstSeenDay = day,
                    FirstSeenYear = year,
                    LastSeenSeason = season ?? "",
                    LastSeenDay = day,
                    LastSeenYear = year,
                    TimesSeen = 1
                };
            }
            else
            {
                entry.TimesSeen++;
                entry.LastSeenSeason = season ?? "";
                entry.LastSeenDay = day;
                entry.LastSeenYear = year;
                if (!string.IsNullOrWhiteSpace(hatName))
                    entry.HatName = hatName;
            }
        }

        /// <summary>
        /// Produces an English internal memory hint for the prompt, or null if there
        /// is nothing worth saying. Kept separate from the outfit memory hint.
        /// </summary>
        public string BuildMemoryContextHint(HatMemoryComparison memory, string targetLanguage)
        {
            if (memory == null)
                return null;

            // Case 1: the farmer just took the hat off (no current hat, but had one before).
            if (memory.CurrentlyHatless)
            {
                if (string.IsNullOrWhiteSpace(memory.PreviousHatId))
                    return null;
                return "HAT MEMORY: the farmer was wearing a hat last time you saw them and is now bare-headed. "
                     + "React to them having taken the hat off, like someone who noticed it is gone.";
            }

            // Case 2: first time ever seeing this hat → no memory hint (let it be a fresh reaction).
            if (memory.TimesSeenBefore <= 0)
                return null;

            string firstSeen = FormatDate(memory.FirstSeenSeason, memory.FirstSeenDay, memory.FirstSeenYear);
            int times = memory.TimesSeenBefore;
            string freq = times == 1
                ? "you have seen the farmer in this hat once before"
                : $"you have seen the farmer in this hat {times} times before";
            string firstNote = string.IsNullOrWhiteSpace(firstSeen) ? "" : $" (first seen on {firstSeen})";
            return $"HAT MEMORY: {freq}{firstNote}. "
                 + "Show that you recognize this hat through familiarity, teasing, or another response that fits your personality. "
                 + "Do NOT react as if seeing this hat for the first time.";
        }

        private static string FormatDate(string season, int day, int year)
        {
            if (string.IsNullOrWhiteSpace(season) || day <= 0)
                return "";
            return $"day {day} of {season}, year {year}";
        }
    }

    internal sealed class HatMemoryData
    {
        public int Version { get; set; } = 1;
        public Dictionary<string, Dictionary<string, HatMemoryEntry>> Memories { get; set; }
        public Dictionary<string, string> LastHatPerNpc { get; set; }
    }

    internal sealed class HatMemoryEntry
    {
        public string HatId { get; set; } = "";
        public string HatName { get; set; } = "";
        public string FirstSeenSeason { get; set; } = "";
        public int FirstSeenDay { get; set; }
        public int FirstSeenYear { get; set; }
        public string LastSeenSeason { get; set; } = "";
        public int LastSeenDay { get; set; }
        public int LastSeenYear { get; set; }
        public int TimesSeen { get; set; } = 1;
    }

    internal sealed class HatMemoryComparison
    {
        public string CurrentHatId { get; set; } = "";
        public string CurrentHatName { get; set; } = "";
        public bool CurrentlyHatless { get; set; }
        public string PreviousHatId { get; set; } = "";
        public int TimesSeenBefore { get; set; }
        public string FirstSeenSeason { get; set; } = "";
        public int FirstSeenDay { get; set; }
        public int FirstSeenYear { get; set; }
        public string LastSeenSeason { get; set; } = "";
        public int LastSeenDay { get; set; }
        public int LastSeenYear { get; set; }
    }
}
