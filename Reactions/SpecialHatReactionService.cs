using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OutfitReactions.Ai
{
    internal sealed class SpecialHatReactionService
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private SpecialHatReactionDefinitions definitions;
        private DateTime lastLoadedUtc = DateTime.MinValue;
        private bool missingFileLogged;

        public SpecialHatReactionService(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
        }

        public string BuildContextForCurrentVanillaHat(Farmer farmer, string targetLanguage)
        {
            try
            {
                StardewValley.Objects.Hat hat = farmer?.hat?.Value;
                if (hat == null)
                    return "";

                string displayName = (hat.DisplayName ?? "").Trim();
                string internalName = (hat.Name ?? "").Trim();
                string itemId = (hat.ItemId ?? "").Trim();
                string qualifiedItemId = (hat.QualifiedItemId ?? "").Trim();
                if (string.IsNullOrWhiteSpace(displayName) && string.IsNullOrWhiteSpace(internalName))
                    return "";

                if (!TryFindHatEntry(displayName, internalName, itemId, qualifiedItemId, out string entryId, out SpecialHatReactionEntry entry))
                    return "";

                return BuildPromptContext(entryId, entry, displayName, internalName, targetLanguage);
            }
            catch (Exception ex)
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor?.Log("[SPECIAL HAT] Could not build special hat reaction context: " + ex.Message, LogLevel.Info);
                return "";
            }
        }

        /// <summary>
        /// Builds reaction context for a hat that was just REMOVED, looked up by its name. Used so an
        /// NPC reacting to the removal still "remembers" what kind of hat it was (e.g. that the
        /// Blobfish Mask was hideous), without needing to state its name. Returns "" if the removed
        /// hat has no special entry (a plain hat needs no special framing on removal).
        /// </summary>
        public string BuildContextForRemovedHat(string removedHatName, string targetLanguage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(removedHatName))
                    return "";

                if (!TryFindHatEntry(removedHatName, removedHatName, "", "", out string entryId, out SpecialHatReactionEntry entry))
                    return "";

                string baseContext = BuildPromptContext(entryId, entry, removedHatName, removedHatName, targetLanguage);
                if (string.IsNullOrWhiteSpace(baseContext))
                    return "";

                bool isPt = string.Equals(targetLanguage, "pt", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(targetLanguage, "pt-BR", StringComparison.OrdinalIgnoreCase);

                // Reframe the special-hat info as memory of a hat that is no longer worn, and tell
                // the AI to use that opinion when reacting to the removal — without naming the hat.
                string frame = isPt
                    ? " CONTEXTO DA REMOÇÃO: as informações acima descrevem o chapéu que o jogador ACABOU DE TIRAR (não está mais usando). "
                      + "Use essa opinião/impressão ao reagir à remoção (por exemplo, alívio se era horrível, ou pena se você gostava), "
                      + "mas NÃO precisa dizer o nome do chapéu nem descrevê-lo em detalhe — apenas deixe a reação refletir o que você achava dele."
                    : " REMOVAL CONTEXT: the information above describes the hat the farmer JUST TOOK OFF (no longer worn). "
                      + "Use that opinion/impression when reacting to the removal (e.g. relief if it was hideous, or mild disappointment if you liked it), "
                      + "but you do NOT need to say the hat's name or describe it in detail — just let your reaction reflect how you felt about it.";

                return baseContext + frame;
            }
            catch (Exception ex)
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor?.Log("[SPECIAL HAT] Could not build removed-hat reaction context: " + ex.Message, LogLevel.Info);
                return "";
            }
        }

        private bool TryFindHatEntry(string displayName, string internalName, string itemId, string qualifiedItemId, out string entryId, out SpecialHatReactionEntry entry)
        {
            entryId = "";
            entry = null;

            SpecialHatReactionDefinitions data = LoadDefinitions();
            if (data?.Hats == null || data.Hats.Count <= 0)
                return false;

            string displayNorm = NormalizeForMatch(displayName);
            string internalNorm = NormalizeForMatch(internalName);

            // IDs are stable across languages, so they are the authoritative match.
            foreach (var pair in data.Hats)
            {
                SpecialHatReactionEntry candidate = pair.Value;
                if (candidate?.MatchIds == null)
                    continue;

                foreach (string id in candidate.MatchIds)
                {
                    if (EqualsExact(id, qualifiedItemId) || EqualsExact(id, itemId))
                    {
                        entryId = pair.Key;
                        entry = candidate;
                        return true;
                    }
                }
            }

            // Names remain as a compatibility fallback for older or cloned items.
            foreach (var pair in data.Hats)
            {
                SpecialHatReactionEntry candidate = pair.Value;
                if (candidate == null)
                    continue;

                foreach (string name in GetAllMatchNames(pair.Key, candidate))
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (EqualsLoose(name, displayName, displayNorm) || EqualsLoose(name, internalName, internalNorm))
                    {
                        entryId = pair.Key;
                        entry = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EqualsExact(string expected, string actual)
        {
            return !string.IsNullOrWhiteSpace(expected)
                && !string.IsNullOrWhiteSpace(actual)
                && expected.Trim().Equals(actual.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private SpecialHatReactionDefinitions LoadDefinitions()
        {
            string path = Path.Combine(helper.DirectoryPath, "assets", "special-reactions", "hat.json");
            try
            {
                if (!File.Exists(path))
                {
                    if (!missingFileLogged)
                    {
                        missingFileLogged = true;
                        if (OutfitReactions.ModEntry.DebugLog) monitor?.Log("[SPECIAL HAT] No special hat reaction file found at assets/special-reactions/hat.json. Special vanilla hat reactions are disabled.", LogLevel.Info);
                    }

                    definitions = null;
                    return null;
                }

                DateTime modifiedUtc = File.GetLastWriteTimeUtc(path);
                if (definitions != null && modifiedUtc == lastLoadedUtc)
                    return definitions;

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                string json = File.ReadAllText(path, Encoding.UTF8);
                definitions = JsonSerializer.Deserialize<SpecialHatReactionDefinitions>(json, options) ?? new SpecialHatReactionDefinitions();
                lastLoadedUtc = modifiedUtc;
                missingFileLogged = false;

                int count = definitions.Hats?.Count ?? 0;
                if (OutfitReactions.ModEntry.DebugLog) monitor?.Log("[SPECIAL HAT] Loaded " + count + " special hat reaction definitions.", LogLevel.Info);
                return definitions;
            }
            catch (Exception ex)
            {
                monitor?.Log("[SPECIAL HAT] Failed to load assets/special-reactions/hat.json: " + ex.Message, LogLevel.Warn);
                definitions = null;
                lastLoadedUtc = DateTime.MinValue;
                return null;
            }
        }

        private string BuildPromptContext(string entryId, SpecialHatReactionEntry entry, string actualDisplayName, string actualInternalName, string targetLanguage)
        {
            SpecialHatReactionDefinitions data = definitions ?? new SpecialHatReactionDefinitions();
            string localized = GetLocalizedName(entry, targetLanguage);
            List<string> parts = new();

            parts.Add("Current vanilla hat: " + StringUtils.FirstNonEmpty(actualDisplayName, actualInternalName, entry.DisplayName, entryId));
            if (!string.IsNullOrWhiteSpace(localized) && !localized.Equals(actualDisplayName, StringComparison.OrdinalIgnoreCase))
                parts.Add("localized hat name: " + localized);
            parts.Add("special hat entry id: " + entryId);

            if (!string.IsNullOrWhiteSpace(entry.Category))
                parts.Add("category: " + entry.Category);
            if (entry.Tags != null && entry.Tags.Count > 0)
                parts.Add("tags: " + string.Join(", ", entry.Tags.Where(tag => !string.IsNullOrWhiteSpace(tag))));
            if (entry.Intensity > 0)
            {
                string intensityText = "";
                if (data.IntensityScale != null && data.IntensityScale.TryGetValue(entry.Intensity.ToString(CultureInfo.InvariantCulture), out string scale))
                    intensityText = " (" + scale + ")";
                parts.Add("intensity: " + entry.Intensity.ToString(CultureInfo.InvariantCulture) + intensityText);
            }
            if (!string.IsNullOrWhiteSpace(entry.ReactionPriority))
                parts.Add("reaction priority: " + entry.ReactionPriority);
            if (!string.IsNullOrWhiteSpace(entry.CoreDescription))
                parts.Add("description: " + entry.CoreDescription);
            if (!string.IsNullOrWhiteSpace(entry.ReactionHint))
                parts.Add("reaction hint: " + entry.ReactionHint);

            string tagGuidance = BuildRelevantTagGuidance(entry.Tags, data.TagGuidance);
            if (!string.IsNullOrWhiteSpace(tagGuidance))
                parts.Add("relevant tag guidance: " + tagGuidance);

            string globalRules = BuildCompactGlobalRules(data.GlobalRules, entry.Intensity);
            if (!string.IsNullOrWhiteSpace(globalRules))
                parts.Add("global handling rules: " + globalRules);

            return string.Join("; ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildRelevantTagGuidance(List<string> tags, Dictionary<string, string> tagGuidance)
        {
            if (tags == null || tags.Count <= 0 || tagGuidance == null || tagGuidance.Count <= 0)
                return "";

            List<string> pieces = new();
            foreach (string tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                if (tagGuidance.TryGetValue(tag, out string guidance) && !string.IsNullOrWhiteSpace(guidance))
                    pieces.Add(tag + " = " + guidance);
            }

            return string.Join("; ", pieces);
        }

        private static string BuildCompactGlobalRules(List<string> rules, int intensity)
        {
            if (rules == null || rules.Count <= 0)
                return "";

            List<string> selected = new();

            if (rules.Count > 0)
                selected.Add(rules[0]);
            if (rules.Count > 1)
                selected.Add(rules[1]);
            if (rules.Count > 2)
                selected.Add(rules[2]);
            if (intensity >= 4 && rules.Count > 5)
                selected.Add(rules[5]);

            return string.Join(" ", selected.Where(rule => !string.IsNullOrWhiteSpace(rule)));
        }

        private static IEnumerable<string> GetAllMatchNames(string entryId, SpecialHatReactionEntry entry)
        {
            yield return entryId;
            yield return entry?.DisplayName;

            if (entry?.LocalizedNames != null)
            {
                foreach (string value in entry.LocalizedNames.Values)
                    yield return value;
            }

            if (entry?.MatchNames != null)
            {
                foreach (string value in entry.MatchNames)
                    yield return value;
            }
        }

        private static bool EqualsLoose(string expected, string actual, string actualNorm)
        {
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
                return false;

            if (expected.Trim().Equals(actual.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            string expectedNorm = NormalizeForMatch(expected);
            return !string.IsNullOrWhiteSpace(expectedNorm)
                && !string.IsNullOrWhiteSpace(actualNorm)
                && expectedNorm.Equals(actualNorm, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeForMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            string normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder builder = new();
            bool lastWasSpace = false;

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(c) || c == '?')
                {
                    builder.Append(c);
                    lastWasSpace = false;
                }
                else if (!lastWasSpace)
                {
                    builder.Append(' ');
                    lastWasSpace = true;
                }
            }

            return RegexCollapseSpaces(builder.ToString().Normalize(NormalizationForm.FormC));
        }

        private static string RegexCollapseSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            StringBuilder builder = new();
            bool lastWasSpace = false;
            foreach (char c in text.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace)
                    {
                        builder.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    builder.Append(c);
                    lastWasSpace = false;
                }
            }

            return builder.ToString();
        }

        private static string GetLocalizedName(SpecialHatReactionEntry entry, string targetLanguage)
        {
            if (entry?.LocalizedNames == null || entry.LocalizedNames.Count <= 0)
                return "";

            string key = LanguageToLocalizationKey(targetLanguage);
            if (!string.IsNullOrWhiteSpace(key) && entry.LocalizedNames.TryGetValue(key, out string localized) && !string.IsNullOrWhiteSpace(localized))
                return localized;

            return "";
        }

        private static string LanguageToLocalizationKey(string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(targetLanguage))
                return "";

            string lower = targetLanguage.ToLowerInvariant();
            if (lower.Contains("brazilian") || lower.Contains("portuguese") || lower.Contains("pt"))
                return "pt-BR";

            if (lower.Contains("spanish") || lower.Contains("es"))
                return "es";
            if (lower.Contains("french") || lower.Contains("fr"))
                return "fr";
            if (lower.Contains("german") || lower.Contains("de"))
                return "de";
            if (lower.Contains("italian") || lower.Contains("it"))
                return "it";
            if (lower.Contains("japanese") || lower.Contains("ja"))
                return "ja";
            if (lower.Contains("korean") || lower.Contains("ko"))
                return "ko";
            if (lower.Contains("russian") || lower.Contains("ru"))
                return "ru";
            if (lower.Contains("turkish") || lower.Contains("tr"))
                return "tr";
            if (lower.Contains("chinese") || lower.Contains("zh"))
                return "zh";

            return "";
        }

        private sealed class SpecialHatReactionDefinitions
        {
            public Dictionary<string, string> IntensityScale { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public List<string> GlobalRules { get; set; } = new();
            public Dictionary<string, string> TagGuidance { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> PersonalityGuidance { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, SpecialHatReactionEntry> Hats { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class SpecialHatReactionEntry
        {
            public string DisplayName { get; set; } = "";
            public Dictionary<string, string> LocalizedNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public List<string> MatchNames { get; set; } = new();
            public List<string> MatchIds { get; set; } = new();
            public string Category { get; set; } = "";
            public List<string> Tags { get; set; } = new();
            public int Intensity { get; set; }
            public string ReactionPriority { get; set; } = "";
            public string CoreDescription { get; set; } = "";
            public string ReactionHint { get; set; } = "";
        }
    }
}
