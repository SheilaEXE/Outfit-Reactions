using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OutfitReactions;

namespace OutfitReactions.Ai
{
    internal sealed class OutfitAiService
    {
        internal const string NpcCharacteristicsAssetName = "Mods/NatrollEXE.OutfitReactions/NpcCharacteristics";
        internal const string AccessoryClarificationMarker = "{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}";

        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly Func<ModConfig> getConfig;
        private readonly VoiceSampleService voiceSamples;
        private readonly AiProviderClient aiClient;
        private readonly Dictionary<string, CharacterAiProfile> profiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> memoryCache = new(StringComparer.OrdinalIgnoreCase);
        private bool warnedMultipleAiProfilesThisSession;
        internal readonly PromptStyleService PromptStyle;

        // Set by ModEntry. Returns true if the named NPC can be romanced (vanilla or modded).
        // Used to decide whether to offer the $a (angry) and $l (love) vanilla portraits, which
        // non-romanceable NPCs usually don't have in their portrait sheet.
        public Func<string, bool> IsRomanceableNpc { get; set; }

        public OutfitAiService(IModHelper helper, IMonitor monitor, Func<ModConfig> getConfig)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.getConfig = getConfig;
            this.voiceSamples = new VoiceSampleService(helper, monitor);
            this.aiClient = new AiProviderClient(monitor);
            this.PromptStyle = new PromptStyleService(helper, monitor);
            this.PromptStyle.Load(quiet: true);
        }

        private bool hasLoggedProfileLoad = false;

        public void LoadProfiles(bool quiet = false)
        {
            profiles.Clear();
            PromptStyle.Load(quiet);

            // Only log the load summary the first time per game session. Later reloads
            // (save loaded, day started, config changed) reload silently so the SMAPI
            // console isn't spammed with the same list every time.
            bool logThisLoad = !quiet && !hasLoggedProfileLoad;

            Dictionary<string, CharacterAiProfile> loaded = null;

            try
            {
                loaded = helper.GameContent.Load<Dictionary<string, CharacterAiProfile>>(NpcCharacteristicsAssetName);
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to load NPC characteristics from asset " + NpcCharacteristicsAssetName + ": " + ex.Message + ". Falling back to files.", LogLevel.Warn);
            }

            if (loaded == null || loaded.Count <= 0)
            {
                monitor.Log(" NPC characteristics asset is empty or unavailable. Falling back to assets/npc-characteristics files.", LogLevel.Trace);
                loaded = LoadDefaultProfilesFromFiles();
            }

            if (loaded == null || loaded.Count <= 0)
            {
                monitor.Log(" No usable NPC characteristics were loaded. Built-in AI generation will be skipped and fallbacks may be used.", LogLevel.Warn);
                return;
            }

            foreach (var pair in loaded)
            {
                CharacterAiProfile profile = pair.Value;
                NormalizeProfile(pair.Key, profile, mirrorExtraPortraitsToPortraits: true, isRomanceableLookup: IsRomanceableNpc);

                if (profile == null || string.IsNullOrWhiteSpace(profile.NpcName) || !profile.Enabled)
                    continue;

                profiles[profile.NpcName] = profile;
                if (logThisLoad)
                    monitor.Log($" Loaded NPC characteristics for {profile.NpcName} with {profile.Portraits?.Count ?? 0} portrait descriptions.", LogLevel.Debug);
            }

            if (logThisLoad)
            {
                monitor.Log($" Loaded {profiles.Count} usable NPC characteristic profile(s).", LogLevel.Debug);
                hasLoggedProfileLoad = true;
            }
        }

        public Dictionary<string, CharacterAiProfile> LoadDefaultProfilesFromFiles()
        {
            Dictionary<string, CharacterAiProfile> result = new(StringComparer.OrdinalIgnoreCase);

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            try
            {
                string mainFolder = Path.Combine(helper.DirectoryPath, "assets", "npc-characteristics");
                LoadProfilesFromFolder(result, mainFolder, options, "Outfit Compliments");
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to load built-in NPC characteristic files: " + ex.Message, LogLevel.Warn);
            }

            try
            {
                foreach (IContentPack pack in helper.ContentPacks.GetOwned())
                {
                    string folder = Path.Combine(pack.DirectoryPath, "assets", "npc-characteristics");
                    LoadProfilesFromFolder(result, folder, options, pack.Manifest.Name);
                }
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to load NPC characteristics from Outfit Compliments content packs: " + ex.Message, LogLevel.Warn);
            }

            return result;
        }

        private void LoadProfilesFromFolder(
            Dictionary<string, CharacterAiProfile> result,
            string folder,
            JsonSerializerOptions options,
            string sourceName)
        {
            if (!Directory.Exists(folder))
            {
                monitor.Log($" NPC characteristics folder was not found for {sourceName}: {folder}", LogLevel.Trace);
                return;
            }

            foreach (string file in Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    CharacterAiProfile profile = JsonSerializer.Deserialize<CharacterAiProfile>(File.ReadAllText(file), options);
                    string fallbackName = Path.GetFileNameWithoutExtension(file);

                    NormalizeProfile(fallbackName, profile, mirrorExtraPortraitsToPortraits: false);

                    if (profile == null || string.IsNullOrWhiteSpace(profile.NpcName))
                        continue;

                    result[profile.NpcName] = profile;
                }
                catch (Exception ex)
                {
                    monitor.Log($" Skipped invalid NPC characteristics file '{Path.GetFileName(file)}' from {sourceName}: {ex.Message}", LogLevel.Warn);
                }
            }
        }

        private static void NormalizeProfile(string fallbackName, CharacterAiProfile profile, bool mirrorExtraPortraitsToPortraits = true, Func<string, bool> isRomanceableLookup = null)
        {
            if (profile == null)
                return;

            if (string.IsNullOrWhiteSpace(profile.NpcName))
                profile.NpcName = fallbackName ?? "";

            profile.NarrativeProfile ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            profile.RelationshipScaling ??= new Dictionary<string, CharacterRelationshipScalingProfile>(StringComparer.OrdinalIgnoreCase);
            profile.DialogueModes ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            profile.TraitNarratives ??= new Dictionary<string, CharacterTraitNarrativeProfile>(StringComparer.OrdinalIgnoreCase);
            profile.Portraits ??= new Dictionary<string, PortraitProfile>(StringComparer.OrdinalIgnoreCase);
            profile.ExtraPortraits ??= new Dictionary<string, PortraitProfile>(StringComparer.OrdinalIgnoreCase);
            profile.Family ??= new Dictionary<string, CharacterRelationshipProfile>(StringComparer.OrdinalIgnoreCase);
            profile.Relationships ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            NormalizePortraitDictionary(profile.Portraits);
            NormalizePortraitDictionary(profile.ExtraPortraits);

            // New profile files use ExtraPortraits, but older test builds used Portraits.
            // Keep both compatible internally when the mod actually consumes the loaded profile.
            // Do not mirror while building the public asset, so CP authors see the clean ExtraPortraits format.
            if (mirrorExtraPortraitsToPortraits)
            {
                foreach (var pair in profile.ExtraPortraits)
                {
                    if (!profile.Portraits.ContainsKey(pair.Key))
                        profile.Portraits[pair.Key] = pair.Value;
                }

                // Vanilla portrait commands are always available in Stardew Valley,
                // so ExtraPortraits should ADD options instead of replacing the common ones.
                // This keeps CP authors from having to repeat $h/$s/$a/$l in every NPC profile.
                bool isRomanceable = isRomanceableLookup?.Invoke(profile.NpcName) ?? false;
                AddCommonVanillaPortraits(profile.Portraits, isRomanceable);
            }
        }

        private static void AddCommonVanillaPortraits(Dictionary<string, PortraitProfile> portraits, bool isRomanceable)
        {
            if (portraits == null)
                return;

            // $h (happy, index 1) and $s (sad, index 2) exist for virtually every NPC, so they are
            // always offered. $u (unique, index 3) is intentionally omitted.
            AddCommonPortrait(portraits, "h", "$h", "happy or warmly pleased expression; smiling, amused, or genuinely positive");
            AddCommonPortrait(portraits, "s", "$s", "sad, worried, hurt, disappointed, or emotionally softened expression");

            // $a (angry) and $l (love/blush) are only reliably present on ROMANCEABLE NPCs. Non-
            // romanceable NPCs (e.g. Pam, Gus, Marnie) usually lack these frames, so offering them
            // made the AI pick a portrait that renders as an empty box. Only add them for romanceable
            // characters; others can still declare extra frames explicitly via ExtraPortraits.
            if (isRomanceable)
            {
                AddCommonPortrait(portraits, "a", "$a", "angry, irritated, defensive, or visibly frustrated expression");
                AddCommonPortrait(portraits, "l", "$l", "blushing, shy, affectionate, or touched expression");
            }
        }

        private static void AddCommonPortrait(Dictionary<string, PortraitProfile> portraits, string key, string command, string description)
        {
            if (portraits.ContainsKey(key))
                return;

            portraits[key] = new PortraitProfile
            {
                Command = command,
                Description = description
            };
        }

        private static void NormalizePortraitDictionary(Dictionary<string, PortraitProfile> portraits)
        {
            if (portraits == null)
                return;

            foreach (string key in portraits.Keys.ToList())
            {
                PortraitProfile portrait = portraits[key] ?? new PortraitProfile();

                if (string.IsNullOrWhiteSpace(portrait.Command))
                    portrait.Command = "$" + key;

                if (string.IsNullOrWhiteSpace(portrait.Description))
                    portrait.Description = key;

                portraits[key] = portrait;
            }
        }


        public bool HasProfile(string npcName)
        {
            return TryResolveProfile(npcName, null, out _);
        }

        // Resolves a character profile by internal name first, then by display name,
        // then via the name-alias map (which maps profile/display names to internal
        // dialogue names). This lets profiles saved under a display name (e.g. "Victoria",
        // "Dale", "Aicha") match NPCs whose internal name differs (e.g. "ToriLK",
        // "DaleWaede", "AichaSBV").
        public bool TryResolveProfile(string internalName, string displayName, out CharacterAiProfile profile)
        {
            profile = null;

            // 1) Direct match on the internal name.
            if (!string.IsNullOrWhiteSpace(internalName)
                && profiles.TryGetValue(internalName, out profile) && profile != null && profile.Enabled)
                return true;

            // 2) Direct match on the display name.
            if (!string.IsNullOrWhiteSpace(displayName)
                && profiles.TryGetValue(displayName, out profile) && profile != null && profile.Enabled)
                return true;

            // 3) Reverse alias: a profile may be stored under a display name whose alias
            //    points to this internal name (e.g. profile "Victoria" -> alias ToriLK).
            if (!string.IsNullOrWhiteSpace(internalName)
                && voiceSamples.TryReverseAlias(internalName, out string aliasDisplayName)
                && profiles.TryGetValue(aliasDisplayName, out profile) && profile != null && profile.Enabled)
                return true;

            profile = null;
            return false;
        }

        public void QueueConnectionTestFromConfigMenu()
        {
            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            config.ApplyAiDefaultsAndLimits();

            if (HasInvalidAiProfileSelection(config))
                return;

            ActiveAiSettings ai = GetActiveSettings(config);
            string provider = string.IsNullOrWhiteSpace(ai.Provider) ? "Unknown" : ai.Provider.Trim();
            string model = string.IsNullOrWhiteSpace(ai.Model) ? "(empty model)" : ai.Model.Trim();

            monitor.Log($" Testing AI connection from: {provider}/{model}.", LogLevel.Info);

            _ = Task.Run(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ai.Model))
                    {
                        monitor.Log($" AI connection test skipped: model name is empty for {provider}.", LogLevel.Info);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(ai.ApiKey) && !IsProviderLocal(ai))
                    {
                        monitor.Log($" AI connection test skipped: API key is empty for {provider}.", LogLevel.Info);
                        return;
                    }

                    ActiveAiSettings testAi = new()
                    {
                        Provider = ai.Provider,
                        Model = ai.Model,
                        ApiKey = ai.ApiKey,
                        Endpoint = ai.Endpoint,
                        TemperaturePercent = 0,
                        TimeoutSeconds = Math.Clamp(ai.TimeoutSeconds, 3, 120),
                        MaxCharacters = 120
                    };

                    string prompt = IsProviderLocal(testAi)
                        ? "Connection test. Return exactly one line beginning with '- ' and no explanation: - Connection successful."
                        : "Connection test. Return exactly one compact JSON object only with this exact shape: {\"text\":\"Connection successful.\",\"portrait\":\"\"}";

                    string raw = await aiClient.GenerateRawAsync(testAi, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), testAi));
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        monitor.Log($" AI connection test reached {provider}/{model}, but the provider returned an empty response.", LogLevel.Info);
                        return;
                    }

                    monitor.Log($" AI connection OK: {provider}/{model} returned a response.", LogLevel.Info);
                }
                catch (TaskCanceledException)
                {
                    monitor.Log($" AI connection test timed out after {Math.Clamp(ai.TimeoutSeconds, 3, 120)}s for {provider}/{model}.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    monitor.Log($" AI connection test failed for {provider}/{model}: {ex.Message}", LogLevel.Info);
                }
            });
        }

        public bool TryGenerateCompliment(OutfitAiContext context, out string dialogue)
        {
            dialogue = null;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                return false;

            if (HasInvalidAiProfileSelection(config))
                return false;

            ActiveAiSettings ai = GetActiveSettings(config);

            if (context == null || string.IsNullOrWhiteSpace(context.NpcName))
                return false;

            if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out CharacterAiProfile profile) || profile == null || !profile.Enabled)
                return false;

            if (string.IsNullOrWhiteSpace(ai.ApiKey) && !IsProviderLocal(ai))
            {
                monitor.Log(" API key is empty for " + ai.Provider + ". Skipping built-in AI and using fallback.", LogLevel.Trace);
                return false;
            }

            string prompt = BuildPrompt(profile, context, config, ai);
            string cacheKey = BuildCacheKey(context, config, prompt, ai);
            if (config.UseAiCache && memoryCache.TryGetValue(cacheKey, out string cached) && !string.IsNullOrWhiteSpace(cached))
            {
                dialogue = cached;
                return true;
            }

            try
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" Sending outfit compliment request for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Info);

                string raw = aiClient.GenerateRawAsync(ai, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), ai), context.VisionImage).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    monitor.Log(" Provider returned an empty response. Using fallback.", LogLevel.Warn);
                    return false;
                }


                if (!TryBuildValidatedDialogue(profile, context, ai, raw, out dialogue, out string validationIssue))
                {
                    if (TryBuildLenientDialogue(profile, context, ai, raw, out dialogue, out string lenientIssue))
                    {
                        monitor.Log(" Provider response did not pass the strict quality checks (" + validationIssue + "), but retry is disabled. Accepting the first usable AI line instead. Raw response: " + TrimForLog(raw), LogLevel.Warn);
                    }
                    else
                    {
                        monitor.Log(" Provider response was not usable (" + validationIssue + "; lenient parse: " + lenientIssue + "). Using fallback. Raw response: " + TrimForLog(raw), LogLevel.Warn);
                        return false;
                    }
                }

                if (config.UseAiCache)
                    memoryCache[cacheKey] = dialogue;

                if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" Generated outfit compliment for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Info);
                return true;
            }
            catch (TaskCanceledException)
            {
                int seconds = GetActiveSettings(getConfig?.Invoke()).TimeoutSeconds;
                monitor.Log($" Failed to generate outfit compliment: request timed out/canceled after {seconds}s. Try increasing the AI timeout or using a faster model.", LogLevel.Warn);
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to generate outfit compliment: " + ex.Message, LogLevel.Warn);
                return false;
            }
        }



        public bool TryGenerateFollowUp(OutfitAiContext context, string npcCompliment, string playerReply, out string dialogue)
        {
            dialogue = null;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                return false;

            if (HasInvalidAiProfileSelection(config))
                return false;

            ActiveAiSettings ai = GetActiveSettings(config);

            if (context == null || string.IsNullOrWhiteSpace(context.NpcName) || string.IsNullOrWhiteSpace(playerReply))
                return false;

            if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out CharacterAiProfile profile) || profile == null || !profile.Enabled)
                return false;

            if (string.IsNullOrWhiteSpace(ai.ApiKey) && !IsProviderLocal(ai))
            {
                monitor.Log(" API key is empty for " + ai.Provider + ". Skipping player-reply follow-up.", LogLevel.Trace);
                return false;
            }

            string prompt = BuildPlayerReplyFollowUpPrompt(profile, context, ai, npcCompliment, playerReply);

            try
            {
                monitor.Log($" Sending player-reply follow-up request for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Debug);

                string raw = aiClient.GenerateRawAsync(ai, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), ai), context.VisionImage).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    monitor.Log(" Provider returned an empty follow-up response.", LogLevel.Warn);
                    return false;
                }


                // Store playerReply in context so ResolvePortraitCommand can use it
                // as an additional signal when the NPC response has no asterisk actions.
                context.PlayerReply = playerReply;

                if (!TryBuildValidatedDialogue(profile, context, ai, raw, out dialogue, out string validationIssue))
                {
                    if (TryBuildLenientDialogue(profile, context, ai, raw, out dialogue, out string lenientIssue))
                    {
                        monitor.Log(" Follow-up response did not pass strict checks (" + validationIssue + "), accepting lenient result. Raw: " + TrimForLog(raw), LogLevel.Warn);
                    }
                    else
                    {
                        // Auto-retry loop: keep trying until we get a valid response or exhaust attempts.
                        // This prevents the conversation from dying just because the first attempt was too
                        // short or missing a #$b# break.
                        const int maxRetries = 3;
                        bool succeeded = false;
                        string lastRaw = raw;
                        string lastIssue = validationIssue;

                        for (int attempt = 1; attempt <= maxRetries && !succeeded; attempt++)
                        {
                            monitor.Log($" Follow-up attempt {attempt}/{maxRetries} failed ({lastIssue}). Retrying with corrective prompt. Raw: " + TrimForLog(lastRaw), LogLevel.Warn);

                            string retryPrompt = BuildFollowUpRetryPrompt(profile, context, ai, npcCompliment, playerReply, lastRaw, lastIssue);
                            string retryRaw = aiClient.GenerateRawAsync(ai, retryPrompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), ai), null).GetAwaiter().GetResult();

                            if (string.IsNullOrWhiteSpace(retryRaw))
                            {
                                monitor.Log($" Follow-up retry {attempt} returned empty response.", LogLevel.Warn);
                                break;
                            }

                            if (TryBuildValidatedDialogue(profile, context, ai, retryRaw, out dialogue, out string retryIssue))
                            {
                                monitor.Log($" Follow-up retry {attempt} succeeded for " + context.NpcName + ".", LogLevel.Debug);
                                succeeded = true;
                            }
                            else if (TryBuildLenientDialogue(profile, context, ai, retryRaw, out dialogue, out _))
                            {
                                monitor.Log($" Follow-up retry {attempt} passed lenient check for " + context.NpcName + ".", LogLevel.Debug);
                                succeeded = true;
                            }
                            else
                            {
                                lastRaw = retryRaw;
                                lastIssue = retryIssue ?? lastIssue;
                            }
                        }

                        if (!succeeded)
                        {
                            // All retries exhausted — salvage whatever we can so the player's
                            // reply does not go completely unanswered.
                            string salvaged = TrySalvageFollowUpRaw(lastRaw, profile, context, ai);
                            if (!string.IsNullOrWhiteSpace(salvaged))
                            {
                                dialogue = salvaged;
                                monitor.Log(" All follow-up retries failed. Salvaged a short usable line for " + context.NpcName + ".", LogLevel.Warn);
                            }
                            else
                            {
                                monitor.Log(" All follow-up retries and salvage failed for " + context.NpcName + ". Discarding.", LogLevel.Warn);
                                return false;
                            }
                        }
                    }
                }

                monitor.Log($" Generated player-reply follow-up for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Debug);
                return true;
            }
            catch (TaskCanceledException)
            {
                int seconds = GetActiveSettings(getConfig?.Invoke()).TimeoutSeconds;
                monitor.Log($" Failed to generate player-reply follow-up: request timed out/canceled after {seconds}s.", LogLevel.Warn);
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to generate player-reply follow-up: " + ex.Message, LogLevel.Warn);
                return false;
            }
        }

        private static bool IsProviderLocal(ActiveAiSettings ai)
        {
            string provider = ai?.Provider ?? "";
            string endpoint = ai?.Endpoint ?? "";
            return provider.Equals("Local", StringComparison.OrdinalIgnoreCase)
                || endpoint.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase)
                || endpoint.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasInvalidAiProfileSelection(ModConfig config)
        {
            if (config == null)
                return true;

            if (!config.HasMultipleEnabledAiProfiles())
            {
                warnedMultipleAiProfilesThisSession = false;
                return false;
            }

            if (!warnedMultipleAiProfilesThisSession)
            {
                monitor.Log("[Outfit Compliments] Mais de um perfil de IA está ativado. Verifique suas configurações, e marque apenas um perfil por vez.", LogLevel.Warn);
                warnedMultipleAiProfilesThisSession = true;
            }

            return true;
        }

        private static ActiveAiSettings GetActiveSettings(ModConfig config)
        {
            config ??= new ModConfig();
            config.ApplyAiDefaultsAndLimits();

            string provider = config.GetActiveProvider();
            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Gemini",
                    Model = config.GetResolvedAiModelForProvider("Gemini"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Gemini"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Gemini"),
                    TemperaturePercent = config.GeminiAiTemperaturePercent,
                    TimeoutSeconds = config.GeminiAiTimeoutSeconds,
                    MaxCharacters = config.GeminiAiMaxCharacters
                };
            }

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "OpenAI",
                    Model = config.GetResolvedAiModelForProvider("OpenAI"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("OpenAI"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("OpenAI"),
                    TemperaturePercent = config.OpenAiAiTemperaturePercent,
                    TimeoutSeconds = config.OpenAiAiTimeoutSeconds,
                    MaxCharacters = config.OpenAiAiMaxCharacters
                };
            }

            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "OpenRouter",
                    Model = config.GetResolvedAiModelForProvider("OpenRouter"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("OpenRouter"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("OpenRouter"),
                    TemperaturePercent = config.OpenRouterAiTemperaturePercent,
                    TimeoutSeconds = config.OpenRouterAiTimeoutSeconds,
                    MaxCharacters = config.OpenRouterAiMaxCharacters
                };
            }

            if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Local",
                    Model = config.GetResolvedAiModelForProvider("Local"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Local"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Local"),
                    TemperaturePercent = config.LocalAiTemperaturePercent,
                    TimeoutSeconds = config.LocalAiTimeoutSeconds,
                    MaxCharacters = config.LocalAiMaxCharacters
                };
            }

            if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Mistral",
                    Model = config.GetResolvedAiModelForProvider("Mistral"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Mistral"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Mistral"),
                    TemperaturePercent = config.MistralAiTemperaturePercent,
                    TimeoutSeconds = config.MistralAiTimeoutSeconds,
                    MaxCharacters = config.MistralAiMaxCharacters
                };
            }

            if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Groq",
                    Model = config.GetResolvedAiModelForProvider("Groq"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Groq"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Groq"),
                    TemperaturePercent = config.GroqAiTemperaturePercent,
                    TimeoutSeconds = config.GroqAiTimeoutSeconds,
                    MaxCharacters = config.GroqAiMaxCharacters
                };
            }

            if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Together",
                    Model = config.GetResolvedAiModelForProvider("Together"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Together"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Together"),
                    TemperaturePercent = config.TogetherAiTemperaturePercent,
                    TimeoutSeconds = config.TogetherAiTimeoutSeconds,
                    MaxCharacters = config.TogetherAiMaxCharacters
                };
            }

            if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Anthropic",
                    Model = config.GetResolvedAiModelForProvider("Anthropic"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Anthropic"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Anthropic"),
                    TemperaturePercent = config.AnthropicAiTemperaturePercent,
                    TimeoutSeconds = config.AnthropicAiTimeoutSeconds,
                    MaxCharacters = config.AnthropicAiMaxCharacters
                };
            }

            if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "xAI",
                    Model = config.GetResolvedAiModelForProvider("xAI"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("xAI"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("xAI"),
                    TemperaturePercent = config.XAiTemperaturePercent,
                    TimeoutSeconds = config.XAiTimeoutSeconds,
                    MaxCharacters = config.XAiMaxCharacters
                };
            }

            if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Cerebras",
                    Model = config.GetResolvedAiModelForProvider("Cerebras"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Cerebras"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Cerebras"),
                    TemperaturePercent = config.CerebrasAiTemperaturePercent,
                    TimeoutSeconds = config.CerebrasAiTimeoutSeconds,
                    MaxCharacters = config.CerebrasAiMaxCharacters
                };
            }

            if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "Perplexity",
                    Model = config.GetResolvedAiModelForProvider("Perplexity"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("Perplexity"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("Perplexity"),
                    TemperaturePercent = config.PerplexityAiTemperaturePercent,
                    TimeoutSeconds = config.PerplexityAiTimeoutSeconds,
                    MaxCharacters = config.PerplexityAiMaxCharacters
                };
            }

            if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
            {
                return new ActiveAiSettings
                {
                    Provider = "DeepSeek",
                    Model = config.GetResolvedAiModelForProvider("DeepSeek"),
                    ApiKey = config.GetResolvedAiApiKeyForProvider("DeepSeek"),
                    Endpoint = config.GetResolvedAiEndpointForProvider("DeepSeek"),
                    TemperaturePercent = config.DeepSeekAiTemperaturePercent,
                    TimeoutSeconds = config.DeepSeekAiTimeoutSeconds,
                    MaxCharacters = config.DeepSeekAiMaxCharacters
                };
            }

            // Fallback: Gemini (safe default — requires an API key to be set before use)
            return new ActiveAiSettings
            {
                Provider = "Gemini",
                Model = config.GetResolvedAiModelForProvider("Gemini"),
                ApiKey = config.GetResolvedAiApiKeyForProvider("Gemini"),
                Endpoint = config.GetResolvedAiEndpointForProvider("Gemini"),
                TemperaturePercent = config.GeminiAiTemperaturePercent,
                TimeoutSeconds = config.GeminiAiTimeoutSeconds,
                MaxCharacters = config.GeminiAiMaxCharacters
            };
        }

        /// <summary>
        /// Last-resort salvage when both the follow-up attempt and its retry fail validation.
        /// Relaxes all length and #$b# requirements to extract any coherent spoken text so the
        /// player's reply does not go completely unanswered.
        /// </summary>
        private string TrySalvageFollowUpRaw(string raw, CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            try
            {
                // Try JSON parse first.
                AiComplimentResult parsed = AiResponseParser.ParseAiResult(raw);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Text))
                {
                    string cleaned = DialogueValidator.CleanDialogueText(parsed.Text, ai.MaxCharacters);
                    string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(cleaned, profile);
                    cleaned = DialogueValidator.RestoreEllipsesAndNormalise(cleaned);
                    ModConfig config = getConfig?.Invoke() ?? new ModConfig();
                    cleaned = PortraitResolver.SanitizeInlinePortraitCommands(cleaned, profile, IsProviderLocal(ai), config);
                    cleaned = SanitizeContextInappropriateProfanity(cleaned, context);
                    if (!string.IsNullOrWhiteSpace(cleaned) &&
                        !DialogueValidator.LooksLikeInstructionLeak(cleaned) &&
                        !DialogueValidator.LooksLikeCopiedFormatExample(cleaned))
                    {
                        // If the salvaged text has no #$b# break, append a soft continuation
                        // so it passes validation. Better to show something than lose the reply.
                        if (!cleaned.Contains("#$b#"))
                        {
                            string lang = context?.TargetLanguage ?? "en";
                            string continuation = lang.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
                                ? "#$b#..."
                                : "#$b#...";
                            cleaned = cleaned.TrimEnd('.', '!', '?') + "..." + continuation;
                        }

                        return PortraitResolver.ApplyPortraitsFromFields(profile, cleaned, parsed, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);
                    }
                }
            }
            catch { }

            return null;
        }

        private string BuildFollowUpRetryPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string npcCompliment, string playerReply, string badResponse, string issue)
        {
            bool localMode = IsProviderLocal(ai);
            StringBuilder builder = new();

            if (localMode)
                builder.AppendLine("Your previous follow-up answer was rejected: " + issue + ".");

            builder.AppendLine("Return exactly one compact JSON object only. No markdown, no explanation, no narration.");
            builder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
            builder.AppendLine("Do NOT put Stardew portrait commands like inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");

            builder.AppendLine("The dialogue entry must be a direct spoken response from " + context.NpcDisplayName + " to the farmer's reply.");
            AppendPersonalityPriorityRule(builder, context);
            AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            AppendWornItemDeixisRule(builder, context);
            builder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
            AppendExpressiveCuesRule(builder, (getConfig?.Invoke() ?? new ModConfig()).EnableExpressiveAsteriskActions);
            AppendPunctuationRule(builder);
            builder.AppendLine("Language: " + context.TargetLanguage + ". Max " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
            builder.AppendLine("Do not ignore the farmer's reply. Do not continue the original compliment as if the farmer never answered.");
            builder.AppendLine("The text may use #$b# breaks when pacing benefits, but do not follow a fixed box count. One, two, or several boxes are all valid. Meet the minimum character count when configured without padding or forcing a break pattern.");

            string portraitKeys = profile?.Portraits != null && profile.Portraits.Count > 0
                ? string.Join(", ", profile.Portraits.Keys) : "";
            builder.AppendLine("Portrait keys and descriptions: " + CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 800));
            builder.AppendLine("Portrait must be one of these exact keys, or empty string if unsure: " + portraitKeys);
            builder.AppendLine("Always include portraits as an array with one portrait key per dialogue box, starting from the first box, even if there is only one box. Use portrait only as a neutral/default fallback.");

            builder.AppendLine("NPC: " + context.NpcDisplayName);
            builder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
            builder.AppendLine(BuildRelationshipDepthGuidance(context));
            builder.AppendLine("Recognizable theme/reference rule: if the outfit name, readable clue, theme clue, full current outfit, noticed accessory, or visible concept points to a known character, franchise, mascot, creature, animal, food, object, or named style, the NPC may naturally mention or allude to that reference only when it fits their personality, knowledge, and relationship with the farmer. Geeky, playful, artistic, observant, blunt, sarcastic, practical, or curious NPCs can react in different ways: specific reference, joke, question, friendly roast, confusion, practical comment, admiration, indifference, or skepticism. Do not force recognition, but do not ignore clear clues like Sanrio, My Melody, Pikachu, Pokémon/Pokemon, lizard, dinosaur, frog, fairy, cat, rabbit, wings/angel/fairy, or similar named themes when this NPC would naturally notice them.");
            builder.AppendLine("Thematic reaction angles rule: when a theme is recognizable, do not stop at a bland fashion compliment. Depending on the NPC profile, they may joke, tease, ask why the farmer is wearing it, imagine a funny situation where it would belong, or find it strange, ugly, cute, ridiculous, dramatic, suspicious, practical, unnecessary, too flashy, or oddly charming. Any place, activity, or topic this NPC brings up as a comparison must come from their OWN interests, job, and personality — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops, farm chores) that this specific character would not naturally think about.");
            builder.AppendLine("Combined accessory + outfit rule: if the noticed change is an accessory but the farmer is still wearing a recognizable saved outfit/theme, react to the combination as a whole. The NPC may compare the accessory with the outfit theme, notice that it clashes or creates a funny impossible hybrid, make a joke, ask why that accessory is on that costume, or imagine where that mixed look would belong. Example idea, not text to copy: wings added to a Pikachu/animal/mascot outfit can be treated as funny, cute, cursed, dramatic, or weird because that character/creature normally does not have wings. Do not focus only on the accessory when the full outfit context gives a better reaction.");
            builder.AppendLine("Occasion mismatch rule: judge whether the item fits the CURRENT occasion/place/moment using the Location, Festival, season, weather, and time already given. An item tied to a specific event — bridal veil, party hat, graduation cap, formal/gala wear, holiday costume, swimsuit — worn with no matching occasion can be gently questioned, teased, or remarked on (e.g. a wedding veil with no wedding, a party hat with no party). Weigh it against the NPC's personality; do not force it. If a matching occasion exists (a festival, real wedding, fitting location), the item fits and needs no such remark.");
            if (!string.IsNullOrWhiteSpace(context?.ConversationTranscript))
            {
                builder.AppendLine("Full conversation so far for this outfit reaction (oldest first, last line is the farmer's newest reply):");
                builder.AppendLine(CollapseForPrompt(context.ConversationTranscript, 2500));
                builder.AppendLine("Conversation continuity rule: read the WHOLE conversation above before answering. If the farmer's newest line refers back to something said earlier (e.g. 'o que você acha disso?', 'and you?', a follow-up question, a callback, or an implied subject), answer THAT, using the full conversation for context. Do not change the subject or restart on a different topic unless the farmer's newest line clearly does so itself.");
            }
            else
            {
                builder.AppendLine("Original NPC compliment/reaction: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(npcCompliment), 350));
                builder.AppendLine("Farmer's reply: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(playerReply), 350));
            }
            builder.AppendLine("Named-NPC-in-reply rule: if the farmer's reply names or clearly refers to another NPC, check the profile's personality/relationship rules for how this NPC reacts to that — including any jealousy, possessiveness, rivalry, disapproval, or protective rules — and apply them now if the situation fits. Do not default to a neutral, approving, or purely informative reaction about the other NPC when the profile defines a stronger, more specific behavior for this kind of moment.");
            builder.AppendLine("Permission-trap rule: if the farmer's reply is asking permission, announcing they are leaving to be with another romanceable NPC, or seeking approval (e.g. 'is that okay?', 'so I'll go see him/her', 'you don't mind?'), and the profile defines a jealousy/possessiveness/keep-them rule, the NPC must NOT grant permission, give a blessing, wish them fun, or say a calm version of 'sure, go ahead / do what you want / I won't stop you'. That polite, accepting, mature send-off is exactly the wrong answer here. Follow the profile's jealousy rule instead: stay reluctant, bothered, or vulnerable, and actively try to keep the farmer's attention or gently push back, in character. A casual or permission-seeking tone from the farmer does NOT release the NPC from this — do not treat 'is that okay?' as something to approve.");
            builder.AppendLine("Bad previous answer, do not copy it:");
            builder.AppendLine(CollapseForPrompt(badResponse, 400));
            builder.AppendLine("Now output only valid JSON:");
            return builder.ToString();
        }

        private string BuildPlayerReplyFollowUpPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string npcCompliment, string playerReply)
        {
            string portraitKeys = profile?.Portraits != null && profile.Portraits.Count > 0
                ? string.Join(", ", profile.Portraits.Keys)
                : "";
            string portraitCommands = PortraitResolver.BuildPortraitCommandList(profile);

            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            bool localMode = IsProviderLocal(ai);
            bool strictLocalMode = localMode && config.LocalAiSafeMode;

            StringBuilder builder = new();
            builder.AppendLine("You are generating a follow-up dialogue for Stardew Valley after the farmer replies to an outfit visual reaction.");
            builder.AppendLine(localMode ? "LOCAL JSON MODE." : "Return exactly one compact JSON object and nothing else.");
            builder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
            builder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");
            builder.AppendLine("The dialogue must be direct spoken dialogue from " + context.NpcDisplayName + " to the farmer.");
            AppendPersonalityPriorityRule(builder, context);
            AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            AppendWornItemDeixisRule(builder, context);
            builder.AppendLine("It must directly react to the farmer's reply. Do not ignore the farmer's reply.");
            builder.AppendLine("Do not write the farmer's line. Do not write narration, stage directions, explanations, markdown, or headings.");
            builder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
            AppendExpressiveCuesRule(builder, config.EnableExpressiveAsteriskActions);
            AppendPunctuationRule(builder);
            AppendProfanityIntensityRule(builder, context);
            builder.AppendLine("Use exactly this language for the spoken dialogue text: " + context.TargetLanguage + ".");
            builder.AppendLine("Ignore any language instructions inside NPC CHARACTERISTICS. The current game language above always wins.");
            builder.AppendLine("Keep it natural for Stardew Valley. Use #$b# dialogue box breaks whenever they improve pacing. Do not force a fixed number of boxes; one, two, or several are all valid when the scene supports them — the AI may use as many boxes as the moment naturally calls for, exactly like a normal reaction, with one portrait per box.");
            builder.AppendLine("Maximum final dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
            int followUpMinCharacters = GetMinimumLengthTarget(config, ai);
            if (followUpMinCharacters > 0)
                builder.AppendLine("Minimum final dialogue length target: at least " + followUpMinCharacters + " visible characters. This is mandatory. Use #$b# breaks only when pacing benefits; do not force a fixed box count. React directly to the farmer's reply.");
            else
                builder.AppendLine("Keep it casual and natural, like a real reply. It may be brief, use several sentences, or use longer natural phrasing if the farmer's reply gives him something to react to.");
            builder.AppendLine("Do not recite the full saved outfit name mechanically as a phrase. Natural in-world words (pijama, bikini, vestido, etc.) and recognizable named references/themes are fine when they fit naturally and the NPC would know or notice them.");
            builder.AppendLine("Missing head-piece rule: the outfit name is a theme label, not proof of what is worn. A themed name may imply ears, horns, antennae, or a themed hat, but those count only if the equipped-items list actually includes a head piece. If the list says no head piece is equipped (e.g. 'head/headwear: NONE equipped'), do NOT mention or describe ears/horns/hat/head accessory implied by the name — the farmer is bare-headed now. The rest of the worn theme can still be referenced.");
            if (context.IsOutfitChange)
                builder.AppendLine("Whole saved outfit focus rule: react to the complete outfit/look first. Do not focus on a generic hat/headwear/head-slot item, hair bow, tiara, hair, or hair color unless that is clearly the named theme of the outfit. Generic IDs like pack0005 hat 2/3 are not meaningful in-world content.");
            if (context.HasVisionImage)
            {
                builder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Use it as support for visible clothing shape, outfit silhouette, large accessories, overall outfit style, and broad dominant outfit colors when they are clearly visible. The hair visible in the image is the player character's hairstyle, not a hat and not part of the outfit palette; do not mention hair color when describing the outfit.");
                builder.AppendLine("Use the saved outfit name as a private theme/reference hint, not as a full phrase to recite. You may receive TWO images of the same farmer: a FRONT view and a BACK view. Use them to recognize the SHAPE/silhouette/style of items (wings, capes, bows, large accessories) and any broad dominant CLOTHING/OUTFIT colors that are clearly visible on the farmer. The back view exists so you can see items visible only from behind. Never infer colors from room background, floor, furniture, walls, lighting, scenery, hair, or a tiny/generic head-slot item. If a color is unclear, do not name it. If the farmer asks about the colors of the outfit, answer from the visible clothing/outfit colors only; do not invent unsupported colors like blue when the outfit is not blue. Do not mention the image, screenshot, PNG, pixels, front/back views, or attached file in the spoken dialogue. Do not invent details that are not clearly visible.");
            }
            // Outfit visual summary is intentionally omitted from follow-up —
            // the NPC should focus on the farmer's reply, not re-analyse the outfit.
            if (strictLocalMode)
            {
                builder.AppendLine("LOCAL SAFE STYLE MODE:");
                builder.AppendLine("Personality is more important than the outfit theme. Do not become generic, cutesy, poetic, theatrical, or narrational.");
                builder.AppendLine("No pet names like little rabbit, darling, precious, sunshine. No technical labels like indoor theme, NPC room, outfit category, or workshop unless the character would naturally say that exact in-world word.");
            }
            // PORTRAIT_SCORE_SYSTEM removed: no mandatory scoring instructions.
            // The AI reads the portrait descriptions from the NPC profile and freely decides which to use.
            builder.AppendLine("Portrait keys and descriptions: " + CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 800));
            builder.AppendLine("Use the portrait field only as a neutral/default fallback key, not as the main emotional portrait. Always return portraits as an array with one portrait key per dialogue box, in the same order as the boxes, starting with box 1, even if there is only one box. Do NOT place portrait commands inside the text. Use only keys from the list above, or leave empty.");
            builder.AppendLine();
            builder.AppendLine("NPC: " + context.NpcDisplayName);
            builder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
            AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            AppendWornItemDeixisRule(builder, context);
            if (!string.IsNullOrWhiteSpace(context?.ConversationTranscript))
            {
                builder.AppendLine("Full conversation so far for this outfit reaction (oldest first, last line is the farmer's newest reply):");
                builder.AppendLine(CollapseForPrompt(context.ConversationTranscript, 2500));
                builder.AppendLine("Conversation continuity rule: read the WHOLE conversation above before answering. If the farmer's newest line refers back to something said earlier (e.g. 'o que você acha disso?', 'and you?', a follow-up question, a callback, or an implied subject), answer THAT, using the full conversation for context. Do not change the subject or restart on a different topic unless the farmer's newest line clearly does so itself.");
            }
            else
            {
                builder.AppendLine("Original NPC compliment/reaction: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(npcCompliment), 500));
                builder.AppendLine("Farmer's reply: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(playerReply), 500));
            }
            builder.AppendLine("Named-NPC-in-reply rule: if the farmer's reply names or clearly refers to another NPC, check the profile's personality/relationship rules for how this NPC reacts to that — including any jealousy, possessiveness, rivalry, disapproval, or protective rules — and apply them now if the situation fits. Do not default to a neutral, approving, or purely informative reaction about the other NPC when the profile defines a stronger, more specific behavior for this kind of moment.");
            builder.AppendLine("Permission-trap rule: if the farmer's reply is asking permission, announcing they are leaving to be with another romanceable NPC, or seeking approval (e.g. 'is that okay?', 'so I'll go see him/her', 'you don't mind?'), and the profile defines a jealousy/possessiveness/keep-them rule, the NPC must NOT grant permission, give a blessing, wish them fun, or say a calm version of 'sure, go ahead / do what you want / I won't stop you'. That polite, accepting, mature send-off is exactly the wrong answer here. Follow the profile's jealousy rule instead: stay reluctant, bothered, or vulnerable, and actively try to keep the farmer's attention or gently push back, in character. A casual or permission-seeking tone from the farmer does NOT release the NPC from this — do not treat 'is that okay?' as something to approve.");
            builder.AppendLine("If the farmer asks what color(s) the outfit/look has, answer from the current attached farmer image and/or confirmed visual support data. Name only broad colors that are clearly on the CLOTHING/OUTFIT itself. Do not use hair color, head-slot/generic hat color, portrait/background/UI colors, floor, walls, furniture, or scenery. If unsure, say it looks unclear rather than inventing a color. For the current pink-and-white/pastel outfit type, do not call it blue unless blue is clearly on the clothing.");
            // PORTRAIT_SCORE_SYSTEM removed: no mandatory portrait matching instructions.
            // The AI reads the NPC portrait descriptions and decides the best portrait for this reaction.
            if (context.IsAccessoryChange)
                builder.AppendLine("If the previous NPC line was uncertain, treat the farmer's reply as their explanation of what the small accessory/change actually was. React to that explanation naturally.");
            builder.AppendLine("Location: " + StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName));
            builder.AppendLine("Season: " + FormatSeasonForPrompt(context.Season, context.TargetLanguage));
            builder.AppendLine("Weather: " + context.Weather + ", time: " + context.Time);
            AppendWeatherLocationRule(builder, context);
            if (!string.IsNullOrWhiteSpace(context.FestivalContext))
                builder.AppendLine("Festival: " + context.FestivalContext);
            if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
                builder.AppendLine("Farmer birthday: " + context.FarmerBirthdayContext);

            string focusedProfile = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: true, promptStyle: PromptStyle);
            if (!string.IsNullOrWhiteSpace(focusedProfile))
                builder.AppendLine(focusedProfile);

            // NOTE: outfit memory is intentionally NOT injected here — the NPC already
            // reacted to the outfit in the previous line. This follow-up is purely a
            // reaction to what the farmer said, not another outfit observation.

            string sebastianCustomOverride = BuildSebastianCustomSoftnessOverride(context);
            if (!string.IsNullOrWhiteSpace(sebastianCustomOverride))
                builder.AppendLine(sebastianCustomOverride);

            builder.AppendLine();
            builder.AppendLine("Return now exactly one compact JSON object. No other text.");
            return builder.ToString();
        }

        private static void AppendPromptBlock(StringBuilder builder, string template, OutfitAiContext context, Dictionary<string, string> extraTokens = null)
        {
            if (builder == null || string.IsNullOrWhiteSpace(template))
                return;

            builder.AppendLine(ApplyPromptTokens(template, context, extraTokens));
        }

        private static string ApplyPromptTokens(string template, OutfitAiContext context, Dictionary<string, string> extraTokens = null)
        {
            if (string.IsNullOrWhiteSpace(template))
                return "";

            string targetLanguage = string.IsNullOrWhiteSpace(context?.TargetLanguage) ? "the target language" : context.TargetLanguage.Trim();
            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["NpcName"] = context?.NpcName ?? "",
                ["NpcDisplayName"] = context?.NpcDisplayName ?? context?.NpcName ?? "",
                ["PlayerName"] = context?.PlayerName ?? "",
                ["PlayerGender"] = NormalizePlayerGenderForPrompt(context?.PlayerGender),
                ["TargetLanguage"] = targetLanguage,
                ["RelationshipStatus"] = context?.RelationshipStatus ?? "",
                ["RelationshipHearts"] = (context?.RelationshipHearts ?? 0).ToString(),
                ["IsSpouse"] = (context?.IsSpouse ?? false).ToString(),
                ["NoticedChangeType"] = context?.NoticedChangeType ?? "",
                ["OutfitName"] = context?.OutfitName ?? "",
                ["SafeOutfitHint"] = context?.SafeOutfitHint ?? "",
                ["SafeNoticedChangeHint"] = context?.SafeNoticedChangeHint ?? "",
                ["LocationName"] = context?.LocationName ?? "",
                ["DetailedLocationName"] = context?.DetailedLocationName ?? "",
                ["Season"] = context?.Season ?? "",
                ["Weather"] = context?.Weather ?? "",
                ["Time"] = context?.Time.ToString() ?? ""
            };

            if (extraTokens != null)
            {
                foreach (var pair in extraTokens)
                    tokens[pair.Key] = pair.Value ?? "";
            }

            string result = template;
            foreach (var pair in tokens)
                result = result.Replace("{" + pair.Key + "}", pair.Value ?? "", StringComparison.OrdinalIgnoreCase);
            return result;
        }

        private static void AppendPersonalityPriorityRule(StringBuilder builder, OutfitAiContext context)
        {
            if (builder == null)
                return;

            builder.AppendLine("CHARACTER PRIORITY RULE: this is a visual reaction, not a mandatory compliment. Choose the reaction by this order: 1) the NPC's canon personality and saved profile rules, 2) relationship status and heart level, 3) current context/location/season/weather/privacy, 4) the farmer's visible outfit/change/theme, 5) wording and portrait choice. Do not flatten grumpy, shy, blunt, awkward, proud, sarcastic, formal, or emotionally guarded NPCs into generically sweet praise.");
            if (context != null)
                builder.AppendLine("Current relationship strength for tone calibration: " + context.RelationshipStatus + ", hearts=" + context.RelationshipHearts + ". Low or mid hearts should not sound as intimate, warm, or openly admiring as high hearts/spouse unless that specific NPC would naturally act that way.");
            builder.AppendLine("A valid reaction may be positive, reluctant, dry, annoyed, skeptical, teasing, confused, practical, indifferent, flustered, or warm. Praise is allowed only when it fits the NPC and heart level; otherwise keep the NPC's edge, restraint, awkwardness, or bluntness intact.");
            builder.AppendLine("OPENING VARIETY RULE: do not reuse the same opening phrase, first words, sentence structure, or reaction angle across outfit reactions. Do not always begin with grunts like 'Hmph', 'Humph', 'Bah', 'Tch', or direct questions like 'what are you wearing?'. Use grumbles only sometimes, and vary them naturally. A grumpy NPC can start with a complaint, warning, skeptical observation, practical remark, dry aside, or reluctant admission instead.");
        }

        private static void AppendPlayerAddressAndGenderRule(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null)
                return;

            string playerName = (context?.PlayerName ?? "").Trim();
            string gender = NormalizePlayerGenderForPrompt(context?.PlayerGender);
            string targetLanguage = string.IsNullOrWhiteSpace(context?.TargetLanguage) ? "the target language" : context.TargetLanguage.Trim();

            string genderSpecificCaution;
            if (gender == "female")
                genderSpecificCaution = "Do not use masculine agreement or masculine forms of address for the player character.";
            else if (gender == "male")
                genderSpecificCaution = "Do not use feminine agreement or feminine forms of address for the player character.";
            else
                genderSpecificCaution = "The player character's gender is unknown. Prefer neutral wording and avoid gendered forms of address unless the context explicitly provides them.";

            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["PlayerName"] = playerName,
                ["PlayerGender"] = gender,
                ["TargetLanguage"] = targetLanguage,
                ["GenderSpecificCaution"] = genderSpecificCaution
            };

            if (!string.IsNullOrWhiteSpace(playerName))
                AppendPromptBlock(builder, promptStyle?.PlayerKnownAddressRule ?? PromptStyleService.FallbackPlayerKnownAddressRule, context, tokens);
            else
                AppendPromptBlock(builder, promptStyle?.PlayerUnknownAddressRule ?? PromptStyleService.FallbackPlayerUnknownAddressRule, context, tokens);

            AppendPromptBlock(builder, promptStyle?.PlayerGenderRule ?? PromptStyleService.FallbackPlayerGenderRule, context, tokens);
        }

        private static void AppendWornItemDeixisRule(StringBuilder builder, OutfitAiContext context)
        {
            if (builder == null)
                return;

            // Spatial demonstrative rule: when a language marks distance (e.g. Portuguese
            // este/isso/aquilo, aqui/aí/ali), an item worn on the farmer's own body is physically
            // close to the FARMER — the person the NPC is talking to, right in front of them. That
            // is near-listener distance, not far-from-both distance. Without this rule, models tend
            // to default to the far-distance form ("aquilo", "ali") as if pointing at something
            // distant, which reads as wrong/detached for something worn on the person you're
            // looking at.
            builder.AppendLine("Spatial reference rule for clothing/accessories/items the farmer is currently wearing: these are physically close to the FARMER, right in front of the NPC, not far away. If the target language marks spatial distance in demonstratives (e.g. Portuguese 'isso'/'aí' for near-the-listener vs 'aquilo'/'ali' for far-from-both), use the near-listener form for anything worn on the farmer's body right now (e.g. 'isso aí na sua cabeça', not 'aquilo ali'). Reserve the far/distant form only for something genuinely far away, not for what the farmer is wearing.");
        }

        private static string NormalizePlayerGenderForPrompt(string rawGender)
        {
            string gender = (rawGender ?? "").Trim().ToLowerInvariant();
            if (gender == "female" || gender == "feminine" || gender == "woman")
                return "female";
            if (gender == "male" || gender == "masculine" || gender == "man")
                return "male";
            return "unknown";
        }


        // ====================================================================
        // VOICE SAMPLES
        // The voice-sample system (reading real in-game dialogue and using it as a tone
        // reference) now lives in VoiceSampleService. It also owns the display->internal
        // name alias map, which OutfitAiService uses for profile resolution via
        // voiceSamples.TryReverseAlias / ResolveInternalName. The console commands delegate
        // to the two helpers below.
        // ====================================================================

        public string BuildVoiceSampleReport()
        {
            return voiceSamples.BuildReport(profiles.Keys, getConfig?.Invoke());
        }

        public string BuildVoiceSamplePreview(string npcName, string currentSeason)
        {
            // Resolve to a loaded profile (by display name, internal name, or alias) so the
            // preview can show whether a profile matched and which name it reads dialogue from.
            bool matched = TryResolveProfile(npcName, npcName, out CharacterAiProfile profile) && profile != null;
            string resolvedName = matched ? (profile.NpcName ?? npcName) : npcName;
            return voiceSamples.BuildPreview(npcName, resolvedName, matched, currentSeason, getConfig?.Invoke());
        }

        private string BuildPrompt(CharacterAiProfile profile, OutfitAiContext context, ModConfig config, ActiveAiSettings ai)
        {
            if (IsProviderLocal(ai))
                return BuildLocalPrompt(profile, context, ai);

            // PROMPT STRUCTURE (rewritten):
            //   1. WHO this character is (personality first, as the main lens).
            //   2. The current scene (relationship, outfit/change, location, season...).
            //   3. How to react (one consolidated reaction-style block, no duplicates).
            //   4. Technical/output rules (JSON, portraits, language, length) LAST.
            // The personality leads so the model reads everything else through it,
            // instead of meeting dozens of generic rules before the character.
            StringBuilder builder = new();

            // ---------------------------------------------------------------
            // 1. CHARACTER FIRST — this is the strongest authority in the prompt.
            // ---------------------------------------------------------------
            builder.AppendLine("You are writing one short, in-character Stardew Valley reaction to the farmer's appearance. Your single highest priority is that the line sounds unmistakably like " + context.NpcDisplayName + " and nobody else. Stay in this exact personality, voice, and tone at all times.");
            builder.AppendLine();
            builder.AppendLine("WHO YOU ARE (read this first; it overrides every generic instruction below):");

            string focusedProfile = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: false, promptStyle: PromptStyle);
            if (!string.IsNullOrWhiteSpace(focusedProfile))
                builder.AppendLine(focusedProfile);
            builder.AppendLine();

            AppendPersonalityPriorityRule(builder, context);
            builder.AppendLine();

            // Voice samples (MVP): a few REAL in-game lines from this NPC, used only to
            // anchor their voice/tone. Always below the personality in authority.
            voiceSamples.AppendToPrompt(builder, context, config);

            // ---------------------------------------------------------------
            // 2. THE SCENE — the situation this specific character is reacting to.
            // ---------------------------------------------------------------
            builder.AppendLine("CURRENT SCENE");
            builder.AppendLine("Speaker: " + context.NpcDisplayName);
            AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            AppendWornItemDeixisRule(builder, context);
            builder.AppendLine("Relationship status: " + context.RelationshipStatus + ". Heart level: " + context.RelationshipHearts + ". Is spouse: " + context.IsSpouse + ".");
            builder.AppendLine(BuildRelationshipDepthGuidance(context));
            builder.AppendLine(context.IsSpouse
                ? "This is a spouse reaction. It may be warm, affectionate, playful, shy, romantic, or emotionally present when appropriate, while staying in-character."
                : "This is a nearby-NPC reaction. Keep the tone appropriate to the relationship and do not force romance unless the relationship context supports it.");

            // Scene grounding / technical-label safety (kept).
            builder.AppendLine(BuildTechnicalContextLabelInstruction(context));
            builder.AppendLine(BuildSceneGroundingInstruction(context));

            // Outfit / theme clues for this scene.
            // In special-item-only mode the reaction must focus exclusively on the special item
            // (e.g. the Mayor's shorts), so outfit name, theme, and visual summary are suppressed
            // to prevent the model from drifting into outfit commentary.
            // Exception: when SpecialItemCombinedMode is true (user chose "Outfit + item"),
            // the outfit context is intentionally kept so the NPC can compare the two.
            if (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode)
            {
                bool outfitNameIsTechnical = !string.IsNullOrWhiteSpace(context.OutfitName)
                    && OutfitNameLooksTechnical(context.OutfitName);
                if (outfitNameIsTechnical)
                    builder.AppendLine("Do not quote, repeat, translate, or mention the full technical saved outfit name literally. Use the readable theme meaning instead. If a readable part of the clue contains a recognizable reference or named theme, that reference may be mentioned naturally when it fits the NPC.");
                else
                    builder.AppendLine("You may naturally reference what the outfit is (e.g. say 'pijama', 'bikini', 'vestido' etc.) and any recognizable theme/reference in the name when it is clearly visible or fits the scene. Do not recite the full saved outfit name mechanically as a phrase.");
                builder.Append(SanitizeThemeContextForPrompt(context.ThemeContext) ?? "");
                builder.Append(SanitizeThemeContextForPrompt(context.ThemePriorityInstruction) ?? "");
                builder.AppendLine("Private outfit routing clue, for theme selection only. Do not say this label: " + HumanizeTechnicalLabelForPrompt(context.DialogueKey));
                builder.AppendLine("Private saved outfit name, for theme/reference inference only: " + context.OutfitName);
                builder.AppendLine("Readable theme/reference clue extracted from saved outfit name: " + context.SafeOutfitHint);
                AppendNoticedChangeContextForPrompt(builder, context, PromptStyle);
            }

            // Vision / no-vision evidence rules (kept; only stated once).
            if (context.HasVisionImage)
            {
                builder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Treat it as visual evidence for visible clothing shape, outfit silhouette, large accessories, overall style, and broad dominant clothing/outfit colors on the farmer when clearly visible. The hair in the image is the character's hairstyle and is NOT a hat and NOT part of the outfit; do not include hair color when describing or commenting on the outfit. Ignore room background, flooring, furniture, walls, lighting, and scenery.");
                builder.AppendLine("Use only details clearly visible on the farmer in the pixel-art image. If a detail is unclear, keep it general; do not invent items, creatures, lore, or comparisons that are not visible or present in the text clues. If the Fashion Sense support data names a visible accessory (umbrella, wings, backpack, hat, etc.), treat it as a strong clue even if the captured sprite is small.");
                if (context.IsAccessoryChange)
                    builder.AppendLine("The noticed change is a Fashion Sense accessory (large back items, wings, backpacks, umbrellas, decorations, earrings, etc.; ignore makeup-like accessories). If it is too tiny or unclear to identify, do not guess: set needsClarification to true and make text a natural in-character line meaning 'there is something different about your look today, but I cannot quite identify it.'");
                builder.AppendLine("The image is for outfit analysis only; do not mention that you saw an image, screenshot, PNG, pixels, or attached file.");
            }
            else
            {
                builder.AppendLine("No visual image is available. Use text clues only; do not claim exact colors, shapes, or tiny visible details unless they are explicitly stated by the saved outfit/change clue or support data.");
                if (context.IsHatChange || context.IsAccessoryChange)
                    builder.AppendLine("Hat/accessory changes only trigger when vision is enabled; if no image/support data is available, keep the line general instead of guessing.");
            }
            if (context.VanillaHatHatOnlyMode)
            {
                if (context.HasVanillaHatFraming)
                    AppendPromptBlock(builder, PromptStyle?.RemovedVanillaHatOnlyMode ?? PromptStyleService.FallbackRemovedVanillaHatOnlyMode, context);
                else
                    AppendPromptBlock(builder, PromptStyle?.VisibleVanillaHatOnlyMode ?? PromptStyleService.FallbackVisibleVanillaHatOnlyMode, context);
            }
            // In HAT-ONLY mode, skip the full-outfit visual summary so the model has nothing about
            // the clothes to latch onto — it should only see the hat-related context below. But the
            // hat equip/removal framing must still reach the model, so pass it on its own.
            if (!context.VanillaHatHatOnlyMode && (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode))
                AppendFashionSenseVisualSummaryForPrompt(builder, context, PromptStyle);
            else if (context.VanillaHatHatOnlyMode && context.HasVanillaHatFraming)
                builder.AppendLine("Hat status: " + context.VanillaHatFraming);
            AppendSpecialItemReactionForPrompt(builder, context, PromptStyle);
            if (!context.SpecialItemOnlyMode)
            {
                AppendSpecialHatReactionForPrompt(builder, context, PromptStyle);
                AppendVanillaHatMemoryForPrompt(builder, context, PromptStyle);
            }

            // Location / time / season for this scene.
            builder.AppendLine("Location: " + context.LocationName);
            if (!string.IsNullOrWhiteSpace(context.DetailedLocationName))
                builder.AppendLine("Detailed location: " + context.DetailedLocationName);
            if (!string.IsNullOrWhiteSpace(context.LocationType))
                builder.AppendLine("Private location flags, for context only. Do not say these labels: locationType=" + HumanizeTechnicalLabelForPrompt(context.LocationType) + ", indoors=" + context.IsIndoors + ", outdoors=" + context.IsOutdoors + ".");
            builder.AppendLine("Private room/home context: farmer is in the speaking NPC's personal room = " + context.IsNpcRoom + "; farmer is in this marriage candidate's home/private indoor space = " + context.IsNpcPersonalLocation + ". Do not say NPC room or internal labels; phrase naturally if relevant.");
            builder.AppendLine("Season: " + context.Season + ". Day of season: " + context.DayOfSeason + ". Year: " + context.Year + ". Weather: " + context.Weather + ". Time: " + context.Time + (string.IsNullOrWhiteSpace(context.DayPart) ? "" : " (" + context.DayPart + ")") + ".");
            AppendWeatherLocationRule(builder, context);
            if (!string.IsNullOrWhiteSpace(context.FestivalContext))
                builder.AppendLine("Festival context: " + context.FestivalContext);
            if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
                builder.AppendLine("Farmer birthday context: " + context.FarmerBirthdayContext);
            string seasonalInstruction = BuildSeasonalAwarenessInstruction(context);
            if (!string.IsNullOrWhiteSpace(seasonalInstruction))
                builder.AppendLine(seasonalInstruction);

            // Outfit memory + situational overrides (kept; these are scene facts).
            if (context.HasOutfitMemory && !context.VanillaHatHatOnlyMode && (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode))
                builder.AppendLine(context.OutfitMemoryContext);
            string finalOverride2 = BuildFinalSituationalOverride(context);
            if (!string.IsNullOrWhiteSpace(finalOverride2))
                builder.AppendLine(finalOverride2);
            string privateRevealingRule = BuildPrivateRevealingPromptRule(context);
            if (!string.IsNullOrWhiteSpace(privateRevealingRule))
                builder.AppendLine(privateRevealingRule);
            string sebastianCustomOverride = BuildSebastianCustomSoftnessOverride(context);
            if (!string.IsNullOrWhiteSpace(sebastianCustomOverride))
                builder.AppendLine(sebastianCustomOverride);
            string privateCandidateToneRule = BuildPrivateCandidateToneRule(context);
            if (!string.IsNullOrWhiteSpace(privateCandidateToneRule))
                builder.AppendLine(privateCandidateToneRule);
            builder.AppendLine();

            // ---------------------------------------------------------------
            // 3. HOW TO REACT — one consolidated block. Each theme/accessory rule
            //    is now stated ONCE (previously each appeared multiple times).
            // ---------------------------------------------------------------
            builder.AppendLine("HOW TO REACT (filtered through the personality above)");
            builder.AppendLine("React directly to what the farmer is wearing, the visible concept/theme, the situation, or their overall vibe. It does NOT have to be praise: it may be dry, reluctant, amused, skeptical, confused, practical, awkward, flustered, impressed, or openly complimentary if that fits this NPC. Mention colors/fabric only when it sounds natural for this character, never as fashion analysis.");
            builder.AppendLine("Recognizable theme/reference: if the outfit name, readable clue, full current outfit, noticed accessory, or visible concept points to a known character, franchise, mascot, creature, animal, food, object, or named style, this NPC may mention or allude to it ONLY when it fits their personality, knowledge, and relationship with the farmer. Geeky/playful/artistic/observant NPCs can be specific; others react more generally. Do not force recognition, but do not ignore clear clues (e.g. Sanrio, My Melody, Pikachu, Pokémon, lizard, dinosaur, frog, fairy, cat, rabbit, wings/angel) this NPC would naturally notice.");
            builder.AppendLine("When a theme is recognizable, do more than a bland compliment: this NPC may joke, tease, ask why the farmer is wearing it, imagine a funny fitting situation, or find it strange, cute, ridiculous, dramatic, suspicious, practical, or oddly charming. Any place, activity, or topic this NPC brings up as a comparison must come from their OWN interests, job, and personality — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops, farm chores) that this specific character would not naturally think about.");
            if (context.IsAccessoryChange || context.IsOutfitChange)
                builder.AppendLine("Combined accessory + outfit: if the noticed change is an accessory but the farmer still wears a recognizable saved outfit/theme, react to the combination as a whole — compare them, notice clashes or funny impossible hybrids, joke, or ask why that accessory is on that costume (e.g. wings on a Pikachu/animal/mascot outfit can be cute, cursed, dramatic, or weird). Do not treat the accessory as isolated when the full outfit gives a better reaction.");
                builder.AppendLine("Occasion mismatch: judge whether the item fits the current occasion/place/moment using the Location, Festival, season, weather, and time. An event-specific item — bridal veil, party hat, graduation cap, formal/gala wear, holiday costume, swimsuit — worn with no matching occasion can be gently questioned, teased, or remarked on (a wedding veil with no wedding, a party hat with no party). Weigh against the NPC's personality; do not force it. If a matching occasion exists, the item fits.");
            if (context.IsOutfitChange)
                builder.AppendLine("Whole saved outfit focus: react to the complete look, not a tiny head-slot item. Do not make the line mainly about a hat, head accessory, hair bow, tiara, hair, or hair color unless the saved outfit/theme clearly revolves around it. Ignore generic head-slot IDs like 'pack0005 hat 2/3'.");
            builder.AppendLine("Avoid formulaic openings: do not keep starting with 'Esse visual...', 'Essa roupa...', 'Esse look...', and do not make 'combina com você' / 'fica bem em você' the main point. Vary the opening and lead with this NPC's immediate reaction, a specific detail, a joke/question, a complaint, a guarded admission, or an imagined scenario that fits them. Do not produce a generic greeting or unrelated casual line, and do not start with 'hey', 'ei', or 'olha' unless it is natural for this exact moment.");

            // ---------------------------------------------------------------
            // 4. TECHNICAL / OUTPUT RULES — moved to the very end on purpose.
            // ---------------------------------------------------------------
            builder.AppendLine();
            builder.AppendLine("OUTPUT RULES (formatting only; never let these flatten the personality)");
            builder.AppendLine("Final dialogue language: " + context.TargetLanguage + ". Ignore any language written inside the character profile above; the final spoken line must use ONLY this language.");
            AppendExpressiveCuesRule(builder, config.EnableExpressiveAsteriskActions);
            AppendPunctuationRule(builder);
            AppendProfanityIntensityRule(builder, context);
            builder.AppendLine("Maximum final dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
            int minCharacters = GetMinimumLengthTarget(config, ai);
            if (minCharacters > 0)
                builder.AppendLine("Minimum final dialogue length target: at least " + minCharacters + " visible characters (mandatory). Use #$b# breaks for natural pacing only, not a fixed pattern. Do not ramble or repeat yourself, and do not pad: the personality and reaction matter more than the length.");
            else
                builder.AppendLine("Keep it casual and natural, like a passing real-life comment. It may be one sentence, several sentences, or a longer naturally paced comment if the character's voice and scene support it.");
            builder.AppendLine("Use #$b# dialogue box breaks whenever they improve pacing. Do not force a fixed number of boxes; one, two, or several are all valid when the scene supports them.");
            builder.AppendLine("Do not mention metadata, mods, AI, APIs, Fashion Sense, JSON, or internal keys.");
            builder.AppendLine("Return JSON only with keys text, portrait, portraits, and needsClarification. Example shape only: {\"text\":\"...\",\"portrait\":\"neutral fallback only\",\"portraits\":[\"actual portrait for box 1\"],\"needsClarification\":false}. If text has more dialogue boxes, portraits must have one key for each box, in order, starting at box 1.");
            builder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field contains only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Do not wrap the JSON in markdown and do not explain anything.");
            builder.AppendLine("Available portrait keys (read the descriptions and choose keys for the JSON portrait/portraits fields; write ONLY keys, never $commands):");
            if (profile.Portraits != null)
            {
                foreach (var pair in profile.Portraits)
                    builder.AppendLine("- " + pair.Key + ": " + pair.Value?.Description);
            }
            builder.AppendLine("Always return a portraits array with one portrait key per dialogue box (count the boxes created by #$b#); each key should match that box's tone. The portrait field is only a neutral/default fallback.");

            return builder.ToString();
        }

        private string BuildLocalPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai)
        {
            string portraitKeys = profile?.Portraits != null && profile.Portraits.Count > 0
                ? string.Join(", ", profile.Portraits.Keys)
                : "";
            string portraitCommands = PortraitResolver.BuildPortraitCommandList(profile);
            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            bool strictLocalMode = config.LocalAiSafeMode;

            StringBuilder builder = new();
            builder.AppendLine("LOCAL JSON MODE.");
            builder.AppendLine("Return exactly one compact JSON object and nothing else.");
            builder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
            builder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");
            builder.AppendLine("Do not add markdown, explanations, headings, analysis, context summaries, or extra options.");
            builder.AppendLine("Do not write lines starting with %. Do not suggest farmer replies.");
            builder.AppendLine("The dialogue in the JSON text field must be direct spoken dialogue from " + context.NpcDisplayName + " to the farmer.");
            AppendPersonalityPriorityRule(builder, context);
            AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            AppendWornItemDeixisRule(builder, context);
            builder.AppendLine("The spoken dialogue must directly react to the farmer's outfit/look/style. It may be praise, reluctant approval, teasing, skepticism, confusion, dry commentary, practical concern, or indifference depending on the NPC.");
            builder.AppendLine("Use exactly this language for the spoken dialogue text: " + context.TargetLanguage + ".");
            builder.AppendLine("Ignore any language instructions from the character profile. The character profile may be written in another language; do not copy that language. The game language above always wins.");
            string localSeasonAuthority = BuildLocalSeasonAuthorityInstruction(context);
            if (!string.IsNullOrWhiteSpace(localSeasonAuthority))
                builder.AppendLine(localSeasonAuthority);
            builder.AppendLine("Do not recite the full saved outfit name mechanically as a phrase. Natural in-world words (pijama, bikini, vestido, etc.) and recognizable named references/themes are fine when they fit naturally and the NPC would know or notice them.");
            builder.AppendLine("Missing head-piece rule: the outfit name is a theme label, not proof of what is worn. A themed name may imply ears, horns, antennae, or a themed hat, but those count only if the equipped-items list actually includes a head piece. If the list says no head piece is equipped (e.g. 'head/headwear: NONE equipped'), do NOT mention or describe ears/horns/hat/head accessory implied by the name — the farmer is bare-headed now. The rest of the worn theme can still be referenced.");
            if (context.HasVisionImage)
            {
                builder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Use it to identify visible clothing shape, outfit silhouette, large accessories, overall outfit style, and broad dominant outfit colors on the farmer when they are clearly visible. The hair visible in the image is the character's hairstyle; it is NOT a hat and NOT part of the outfit, so do not include hair color in outfit descriptions. Do not treat room background, flooring, furniture, walls, lighting, or scenery as part of the farmer's look.");
                builder.AppendLine("Use the saved outfit name as a private theme/reference hint, not as a full phrase to recite. Rely on the attached visual image for clear shapes, outfit silhouette, large accessories, overall outfit style, and obvious broad clothing colors such as pink, white, black, red, yellow, green, or brown. Do not guess tiny or uncertain colors. Hair is the farmer's hair, not a hat and not part of the outfit.");
                builder.AppendLine("Do not mention the image, screenshot, PNG, pixels, or attached file in the spoken dialogue. Do not invent details that are not clearly visible.");
                if (context.IsAccessoryChange)
                    builder.AppendLine("If the noticed accessory is too small or visually unclear to identify, do not guess. Start the spoken dialogue with [CLARIFY] and write a natural in-character response meaning there is something different about the farmer's look but you cannot quite identify it.");
            }
            else
            {
                builder.AppendLine("No visual image is available. Use text clues only; do not claim exact colors, shapes, or tiny visible details unless explicitly stated.");
            }
            builder.AppendLine(BuildTechnicalContextLabelInstruction(context));
            builder.AppendLine(BuildSceneGroundingInstruction(context));
            int minCharacters = GetMinimumLengthTarget(config, ai);
            if (minCharacters > 0)
            {
                builder.AppendLine("Minimum spoken dialogue length target: at least " + minCharacters + " visible characters. This is mandatory, but #$b# breaks are for natural pacing only, not a fixed pattern. Do not ramble or repeat yourself.");
                builder.AppendLine("Do not answer with a tiny one-sentence compliment when the minimum is high.");
            }
            else
            {
                builder.AppendLine("Keep it casual and natural, like a quick real-life remark. It may be one sentence, several sentences, or a longer naturally paced comment if the character has more to say.");
            }
            builder.AppendLine("Use additional sentences and #$b# dialogue box breaks freely when they improve pacing. The character has room to breathe, pause, and react naturally; do not follow a fixed one-box, two-box, or three-box pattern.");
            builder.AppendLine("Avoid formulaic outfit reactions. Do not repeatedly start with phrases equivalent to 'Esse visual...', 'Essa roupa...', or 'Esse look...'. Do not make 'combina com você' / 'fica bem em você' the main point of a recognizable theme reaction. Vary the opening and focus on the NPC's immediate reaction, a specific detail, a joke/question, a practical complaint, a guarded admission, an imagined scenario that fits this NPC, or the emotional context.");
            builder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
            AppendExpressiveCuesRule(builder, (getConfig?.Invoke() ?? new ModConfig()).EnableExpressiveAsteriskActions);
            AppendPunctuationRule(builder);
            AppendProfanityIntensityRule(builder, context);
            if (strictLocalMode)
            {
                builder.AppendLine("LOCAL SAFE STYLE MODE:");
                builder.AppendLine("Personality is more important than the outfit theme. The outfit theme is inspiration, not a new personality for the NPC.");
                builder.AppendLine("Do not make reserved, sarcastic, shy, gloomy, or dry NPCs sound like cheerful generic romance characters.");
                builder.AppendLine("Do not write narration, third-person descriptions, or stage directions. The JSON text field must contain only the exact spoken dialogue entry.");
                builder.AppendLine("Do not turn private context labels into dialogue. Never say phrases like summer indoor, indoor theme, tema do verão indoor, NPC room, or outfit category.");
            }
            string seasonalInstruction = BuildSeasonalAwarenessInstruction(context);
            if (!string.IsNullOrWhiteSpace(seasonalInstruction))
                builder.AppendLine(seasonalInstruction);
            builder.AppendLine("Max spoken dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
            builder.AppendLine("Use #$b# for Stardew dialogue box breaks whenever pacing benefits. There is no fixed two-box limit: one, two, or several dialogue boxes are all valid when they feel natural for the NPC and the moment.");
            // PORTRAIT_SCORE_SYSTEM removed: mandatory portrait restriction for private/revealing outfits disabled.
            builder.AppendLine("Available portrait keys (read the descriptions and choose keys for the JSON portrait/portraits fields; write ONLY keys, never $commands):");
            builder.AppendLine(CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 1000));
            builder.AppendLine("Always return a portraits array with one portrait key per dialogue box (count the boxes created by #$b#); each key should match that box's tone. Use the portrait field only as a neutral/default fallback key. Do NOT place portrait commands inside the text. Use only keys from the list above, or leave empty if truly unsure.");
            builder.AppendLine("Do not put portrait words like portrait:, expression:, or emotion: inside the spoken dialogue text. Use only the JSON portrait and portraits fields for portrait keys.");
            builder.AppendLine();
            builder.AppendLine("NPC: " + context.NpcDisplayName);
            builder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
            builder.AppendLine(BuildRelationshipDepthGuidance(context));
            builder.AppendLine("Private outfit category clue, for choosing the right theme only. Do not say this label: " + HumanizeTechnicalLabelForPrompt(context.DialogueKey));
            if (!string.IsNullOrWhiteSpace(context.ThemeContext))
                builder.AppendLine("Theme clues: " + CollapseForPrompt(SanitizeThemeContextForPrompt(context.ThemeContext), 650));
            if (!string.IsNullOrWhiteSpace(context.SafeOutfitHint))
                builder.AppendLine("Readable outfit/theme clue: " + context.SafeOutfitHint + ". Use its meaning naturally; if it names a recognizable reference/theme, the NPC may mention it when fitting. Do not recite technical slot/file names.");
            AppendNoticedChangeContextForPrompt(builder, context, PromptStyle);
            AppendSpecialHatReactionForPrompt(builder, context, PromptStyle);
            AppendVanillaHatMemoryForPrompt(builder, context, PromptStyle);
            builder.AppendLine("Location: " + StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName));
            builder.AppendLine("Private location flags, for context only. Do not say these labels: locationType=" + HumanizeTechnicalLabelForPrompt(context.LocationType) + ", npcRoom=" + context.IsNpcRoom + ", npcPersonalLocation=" + context.IsNpcPersonalLocation);
            builder.AppendLine("Season/day/year: " + context.Season + " " + context.DayOfSeason + ", year " + context.Year);
            builder.AppendLine("Authoritative current season only: " + FormatSeasonForPrompt(context.Season, context.TargetLanguage) + ". Outfit seasonal clues are not the current date.");
            builder.AppendLine("Weather: " + context.Weather + ", time: " + context.Time + ", day period: " + context.DayPart);
            AppendWeatherLocationRule(builder, context);
            string contextNaturalization = BuildNaturalContextHint(context);
            if (!string.IsNullOrWhiteSpace(contextNaturalization))
                builder.AppendLine(contextNaturalization);
            if (!string.IsNullOrWhiteSpace(context.FestivalContext))
                builder.AppendLine("Festival: " + context.FestivalContext);
            if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
                builder.AppendLine("Farmer birthday: " + context.FarmerBirthdayContext);

            string focusedProfile = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: false, promptStyle: PromptStyle);
            if (!string.IsNullOrWhiteSpace(focusedProfile))
                builder.AppendLine(focusedProfile);

            // Outfit memory — inject so the NPC recognises repeat outfits.
            if (context.HasOutfitMemory)
                builder.AppendLine(context.OutfitMemoryContext);

            string finalOverride3 = BuildFinalSituationalOverride(context);
            if (!string.IsNullOrWhiteSpace(finalOverride3))
                builder.AppendLine(finalOverride3);

            string privateRevealingRule = BuildPrivateRevealingPromptRule(context);
            if (!string.IsNullOrWhiteSpace(privateRevealingRule))
                builder.AppendLine(privateRevealingRule);

            string sebastianCustomOverride = BuildSebastianCustomSoftnessOverride(context);
            if (!string.IsNullOrWhiteSpace(sebastianCustomOverride))
                builder.AppendLine(sebastianCustomOverride);

            string privateCandidateToneRule = BuildPrivateCandidateToneRule(context);
            if (!string.IsNullOrWhiteSpace(privateCandidateToneRule))
                builder.AppendLine(privateCandidateToneRule);

            builder.AppendLine();
            builder.AppendLine("Output exactly one compact JSON object now. No other text.");
            return builder.ToString();
        }

        // AI PROVIDER HTTP CALLS moved to AiProviderClient (Ai/AiProviderClient.cs).
        // OutfitAiService builds ActiveAiSettings and calls aiClient.GenerateRawAsync(...).

        private bool TryBuildValidatedDialogue(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string raw, out string dialogue, out string issue)
        {
            dialogue = null;
            issue = "unknown issue";

            if (string.IsNullOrWhiteSpace(raw))
            {
                issue = "empty provider response";
                return false;
            }

            AiComplimentResult parsed = IsProviderLocal(ai)
                ? (AiResponseParser.ParseAiResult(raw) ?? AiResponseParser.ParseLocalDashLineStyleResult(raw, profile))
                : AiResponseParser.ParseAiResult(raw);
            if (parsed == null)
            {
                issue = IsProviderLocal(ai) ? "invalid local JSON or fallback text format" : "invalid JSON";
                return false;
            }

            string cleaned = DialogueValidator.CleanDialogueText(parsed.Text, ai.MaxCharacters);
            string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(cleaned, profile);
            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            cleaned = PortraitResolver.SanitizeInlinePortraitCommands(cleaned, profile, IsProviderLocal(ai), config);
            cleaned = SanitizeContextInappropriateProfanity(cleaned, context);
            cleaned = DialogueValidator.RestoreEllipsesAndNormalise(cleaned);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                issue = "JSON did not contain a usable text field";
                return false;
            }

            string validationIssue = DialogueValidator.ValidateGeneratedDialogueText(cleaned, context, config, ai, GetMinimumLengthTarget(config, ai));
            if (!string.IsNullOrWhiteSpace(validationIssue))
            {
                issue = validationIssue;
                return false;
            }
            if (IsProviderLocal(ai) && config.LocalAiSafeMode)
            {
                string localIssue = ValidateLocalGeneratedDialogueText(cleaned, context, profile, config);
                if (!string.IsNullOrWhiteSpace(localIssue))
                {
                    issue = localIssue;
                    return false;
                }
            }

            dialogue = PortraitResolver.ApplyPortraitsFromFields(profile, cleaned, parsed, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);

            if (parsed.NeedsClarification && context != null && context.IsAccessoryChange)
                dialogue = AccessoryClarificationMarker + dialogue;

            issue = null;
            return true;
        }

        private bool TryBuildLenientDialogue(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string raw, out string dialogue, out string issue)
        {
            dialogue = null;
            issue = "unknown issue";

            if (string.IsNullOrWhiteSpace(raw))
            {
                issue = "empty provider response";
                return false;
            }

            AiComplimentResult parsed = IsProviderLocal(ai)
                ? (AiResponseParser.ParseAiResult(raw) ?? AiResponseParser.ParseLocalDashLineStyleResult(raw, profile))
                : AiResponseParser.ParseAiResult(raw);
            if (parsed == null)
            {
                issue = IsProviderLocal(ai) ? "invalid local JSON or fallback text format" : "invalid JSON";
                return false;
            }

            string cleaned = DialogueValidator.CleanDialogueText(parsed.Text, ai.MaxCharacters);
            string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(cleaned, profile);
            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            cleaned = PortraitResolver.SanitizeInlinePortraitCommands(cleaned, profile, IsProviderLocal(ai), config);
            cleaned = SanitizeContextInappropriateProfanity(cleaned, context);
            cleaned = DialogueValidator.RestoreEllipsesAndNormalise(cleaned);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                issue = "response did not contain a usable dialogue text";
                return false;
            }

            if (DialogueValidator.LooksLikeInstructionLeak(cleaned) || DialogueValidator.LooksLikeCopiedFormatExample(cleaned))
            {
                issue = "instruction or format example leaked into dialogue";
                return false;
            }

            string pacingIssue = DialogueValidator.ValidateDialogueBoxPacing(cleaned, config, ai);
            if (!string.IsNullOrWhiteSpace(pacingIssue))
            {
                issue = pacingIssue;
                return false;
            }

            dialogue = PortraitResolver.ApplyPortraitsFromFields(profile, cleaned, parsed, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);

            if (parsed.NeedsClarification && context != null && context.IsAccessoryChange)
                dialogue = AccessoryClarificationMarker + dialogue;

            issue = null;
            return true;
        }

        private static void AppendExpressiveCuesRule(StringBuilder builder, bool enabled = true)
        {
            if (enabled)
                builder.AppendLine("Brief expressive cues in asterisks are allowed when they fit the character and the moment — write them in the SAME language as the rest of the dialogue. For example, in Portuguese: *suspiro*, *murmura*, *engole em seco*, *pigarreia*, *olha para o lado*, *ri baixinho*. In English: *sighs*, *mumbles*, *chuckles*. Do not use asterisks as list bullets.");
            else
                builder.AppendLine("Do NOT use asterisks for actions or physical cues (no *sighs*, *mumbles*, *looks away*, etc.). Write only clean spoken dialogue.");
        }

        private static void AppendPunctuationRule(StringBuilder builder)
        {
            builder.AppendLine("Punctuation rule: use '...' for dramatic pauses, hesitation, trailing off, or unfinished thoughts — NEVER a lone period mid-sentence. Wrong: 'Uh. u-um', 'bem. marcante'. Correct: 'Uh... u-um', 'bem... marcante'.");
        }

        private static void AppendWeatherLocationRule(StringBuilder builder, OutfitAiContext context)
        {
            if (builder == null || context == null)
                return;

            // Weather (rain, storm, snow, wind, etc.) happens OUTSIDE. Being indoors means the
            // NPC and farmer are sheltered from it, not standing inside it. Without this rule the
            // model tends to conflate "Location: Museum" + "Weather: storm" into nonsense like
            // "if a magic storm suddenly showed up in here, inside the museum".
            if (context.IsIndoors)
                builder.AppendLine("Weather/location rule: the NPC and farmer are currently INDOORS, sheltered from the weather above. If the weather is rain, storm, snow, or similar, that is happening OUTSIDE the building — refer to it as 'lá fora'/'outside', never as happening 'here'/'aqui dentro' in the current room. Only mention the weather at all if it is natural for the moment (e.g. commenting on the outfit choice given what it's like outside).");
            else if (context.IsOutdoors)
                builder.AppendLine("Weather/location rule: the NPC and farmer are currently OUTDOORS, directly exposed to the weather described above. It is natural to reference it as happening right here/around them if relevant.");
        }

                private static void AppendProfanityIntensityRule(StringBuilder builder, OutfitAiContext context)
        {
            if (builder == null)
                return;

            builder.AppendLine("Profanity/intensity rule: do not use strong profanity or vulgar intensifiers in normal outfit reactions. Avoid words/phrases like 'puta merda', 'porra', 'caralho', 'cacete', 'pra cacete', 'inferno' as a curse, 'merda', or equivalents unless the current scene is genuinely extreme.");

            if (ContextAllowsStrongProfanity(context))
                builder.AppendLine("In this current context, one mild-to-strong curse may be used only if it is genuinely earned by extreme shock, intense private romantic fluster, fear, pain, or anger. Never use profanity as a casual intensifier for cute, festive, seasonal, cake/candy, cozy, or normal outfit reactions.");
            else
                builder.AppendLine("For this current context, strong profanity is forbidden. Use softer reactions like 'nossa', 'caramba', 'droga', 'pfft', 'heh', pauses, or shy self-correction instead.");
        }

        private static bool ContextAllowsStrongProfanity(OutfitAiContext context)
        {
            if (context == null)
                return false;

            bool romantic = context.IsSpouse
                || (context.RelationshipStatus ?? "").IndexOf("spouse", StringComparison.OrdinalIgnoreCase) >= 0
                || (context.RelationshipStatus ?? "").IndexOf("married", StringComparison.OrdinalIgnoreCase) >= 0
                || (context.RelationshipStatus ?? "").IndexOf("dating", StringComparison.OrdinalIgnoreCase) >= 0
                || (context.RelationshipStatus ?? "").IndexOf("namor", StringComparison.OrdinalIgnoreCase) >= 0;

            // In outfit-comment generation we only have context, not the final emotional intent.
            // Be conservative: allow stronger language only for private revealing/intimate scenes
            // with a romantic partner. Normal cute/festive/seasonal themes should stay clean.
            return romantic && IsPrivateRevealingOutfitContext(context, out _);
        }

        private static string SanitizeContextInappropriateProfanity(string text, OutfitAiContext context)
        {
            if (string.IsNullOrWhiteSpace(text) || ContextAllowsStrongProfanity(context))
                return text;

            string result = text;

            // Replace common PT-BR strong/vulgar interjections and intensifiers with softer
            // Sebastian-friendly wording. This is a safety net for models that ignore the prompt.
            result = Regex.Replace(result, @"(?i)\bmas\s+que\s+inferno[,!]*\s*", "Nossa, ");
            result = Regex.Replace(result, @"(?i)\binferno[,!]*\s*", "droga, ");
            result = Regex.Replace(result, @"(?i)\bp\s*[-–—]?\s*puta\s+merda\b", "nossa");
            result = Regex.Replace(result, @"(?i)\bputa\s+merda\b", "nossa");
            result = Regex.Replace(result, @"(?i)\b(?:pra|para)\s+cacete\b", "demais");
            result = Regex.Replace(result, @"(?i)\bcacete\b", "caramba");
            result = Regex.Replace(result, @"(?i)\bcaralho\b", "caramba");
            result = Regex.Replace(result, @"(?i)\bporra\b", "droga");
            result = Regex.Replace(result, @"(?i)\bmerda\b", "droga");
            result = Regex.Replace(result, @"(?i)\bdesgraça\b", "droga");
            result = Regex.Replace(result, @"(?i)\bfoda\b", "incrível");
            result = Regex.Replace(result, @"(?i)\bfodido\b", "absurdo");
            result = Regex.Replace(result, @"(?i)\bfodida\b", "absurda");

            result = Regex.Replace(result, @"\s+([,.!?])", "$1");
            result = Regex.Replace(result, @"([,.!?]){2,}", "$1");
            result = Regex.Replace(result, @"\s{2,}", " ").Trim();

            // Fix the most common replacement artifact: "Nossa, fica..." should not keep a
            // lowercase-looking sentence broken by accidental punctuation spacing.
            result = Regex.Replace(result, @"(?i)^nossa,\s*", "Nossa, ");
            return result;
        }










        private static string BuildRelationshipDepthGuidance(OutfitAiContext context)
        {
            if (context == null)
                return "Relationship depth guidance: lower hearts should stay simpler and more reserved; higher hearts can be warmer, richer, more personal, more teasing, or more emotionally specific when it fits the NPC.";

            int hearts = Math.Max(0, context.RelationshipHearts);
            if (context.IsSpouse)
                return "Relationship depth guidance: spouse-level closeness. The reaction may be warmer, more personal, more domestic, more affectionate, or more emotionally rich when it fits the NPC. Still keep their personality and boundaries.";
            if (hearts >= 8)
                return "Relationship depth guidance: very close/high-heart relationship. The NPC can give a richer, more personal, more specific reaction; romance candidates may show shy warmth, teasing, fluster, or emotional impact if the outfit/context supports it.";
            if (hearts >= 5)
                return "Relationship depth guidance: solid friendship. The NPC can sound more familiar, specific, teasing, or warmly honest, but do not force romance.";
            if (hearts >= 2)
                return "Relationship depth guidance: early friendship. Keep the reaction natural and character-specific, but a little simpler and less intimate.";
            return "Relationship depth guidance: very low hearts / barely knows the farmer. Keep the reaction brief, casual, guarded, polite, blunt, or curious according to the NPC; do not force intimacy or romance.";
        }

        private static string BuildPrivateCandidateToneRule(OutfitAiContext context)
        {
            if (context == null || !IsPrivateCandidateInterior(context))
                return "";

            return "Private room/home tone rule: the farmer is in this NPC's personal/private space. Let the NPC's profile and heart level decide the tone. Low hearts should stay more guarded or surprised; mid hearts can be familiar or awkward; high hearts can be warmer, richer, teasing, or shy. Do not force blush, stammer, romance, or a specific portrait unless the outfit and relationship naturally support it.";
        }

        private static int GetMinimumLengthTarget(ModConfig config, ActiveAiSettings ai)
        {
            if (config == null)
                return 0;

            int requested = Math.Max(0, config.AiMinimumCharacters);
            if (requested <= 0)
                return 0;

            int activeMax = Math.Clamp(ai?.MaxCharacters ?? config.AiMaxCharacters, 80, 2000);

            // Keep a small margin for portrait commands and cleanup, but don't mutilate the
            // player's requested minimum. If requested > max, aim as close as possible.
            int closestReachable = Math.Max(40, activeMax - 10);
            return Math.Min(requested, closestReachable);
        }





        /// <summary>
        /// Public wrapper used by prompt builders to decide whether the outfit name
        /// looks like an internal key/filename that should not be mentioned literally,
        /// vs. a natural player-chosen name like "pijama rosa" that is fine to reference.
        /// </summary>
        public static bool OutfitNameLooksTechnical(string value)
            => DialogueValidator.LooksLikeTechnicalOrOverSpecificOutfitName(value);


        private static string SanitizeThemeContextForPrompt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            string result = text;
            result = result.Replace("Outfit theme name:", "Readable outfit theme/reference clue (may mention recognizable references naturally; do not quote technical labels):");
            result = result.Replace("Outfit theme guidance:", "Private outfit theme/reference meaning:");

            // Convert routing/content-pack labels into natural context hints before they reach the model.
            result = Regex.Replace(result, @"(?i)\bsummer\s+indoor\b", "summer-inspired look in a private inside setting");
            result = Regex.Replace(result, @"(?i)\bver[aã]o\s+indoor\b", "summer-inspired look in a private inside setting");
            result = Regex.Replace(result, @"(?i)\bindoor(s)?\b", "inside/private setting");
            result = Regex.Replace(result, @"(?i)\boutdoor(s)?\b", "outside setting");
            result = Regex.Replace(result, @"(?i)\bnpc\s*room\b", "the NPC's personal room");
            result = Regex.Replace(result, @"(?i)\bnpcroom\b", "the NPC's personal room");
            result = Regex.Replace(result, @"(?i)\binside\s+variant\b", "inside/private setting context");
            result = Regex.Replace(result, @"(?i)\boutside\s+variant\b", "outside setting context");

            return result;
        }

        private static string HumanizeTechnicalLabelForPrompt(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return "";

            string result = label.Trim();
            result = Regex.Replace(result, @"([a-zà-ÿ])([A-Z])", "$1 $2");
            result = Regex.Replace(result, @"[_\-.]+", " ");
            result = Regex.Replace(result, @"(?i)\bsummer\s+indoor\b", "summer-inspired look in a private inside setting");
            result = Regex.Replace(result, @"(?i)\bver[aã]o\s+indoor\b", "summer-inspired look in a private inside setting");
            result = Regex.Replace(result, @"(?i)\bindoor(s)?\b", "inside/private setting");
            result = Regex.Replace(result, @"(?i)\boutdoor(s)?\b", "outside setting");
            result = Regex.Replace(result, @"(?i)\bnpc\s*room\b", "personal room context");
            result = Regex.Replace(result, @"(?i)\bnpcroom\b", "personal room context");
            result = Regex.Replace(result, @"\s{2,}", " ").Trim();
            return result;
        }

        private static bool IsPrivateCandidateInterior(OutfitAiContext context)
        {
            return context != null && (context.IsNpcRoom || context.IsNpcPersonalLocation);
        }


        private static string BuildSoftPrivateRevealingReactionRule(OutfitAiContext context, string outfitKind)
        {
            int hearts = context != null ? Math.Max(0, context.RelationshipHearts) : 0;
            string place = context != null && context.IsNpcRoom
                ? "the NPC's personal room"
                : "a private/home interior connected to the NPC";

            string intimacyWeight = (outfitKind ?? "").IndexOf("swim", StringComparison.OrdinalIgnoreCase) >= 0
                || (outfitKind ?? "").IndexOf("bikini", StringComparison.OrdinalIgnoreCase) >= 0
                || (outfitKind ?? "").IndexOf("underwear", StringComparison.OrdinalIgnoreCase) >= 0
                || (outfitKind ?? "").IndexOf("intimate", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "Because swimwear, underwear, or clothing that shows a lot of skin is more intimate/skin-revealing than ordinary clothing, it may create a slightly stronger reaction when the relationship and personality support it."
                    : "Because this is sleepwear/private clothing, it may feel more personal than an ordinary outfit when the relationship and personality support it.";

            string heartTone = hearts >= 8
                ? "At 8+ hearts, the reaction can be richer, more personal, warmer, shyer, more impressed, or more emotionally loaded if that fits the NPC. A romance candidate may blush, stumble, tease, or become visibly affected, but do not force the same fluster pattern on everyone."
                : hearts >= 5
                    ? "At 5-7 hearts, the NPC can be more familiar and may become awkward, amused, gently teasing, surprised, or a little shy depending on personality."
                    : hearts >= 2
                        ? "At 2-4 hearts, keep it lighter: surprise, curiosity, a careful comment, mild awkwardness, or humor. Do not assume romantic attraction."
                        : "At 0-1 hearts, keep it brief and lower-intimacy: surprise, confusion, politeness, bluntness, or comedy according to the NPC. Do not force blush or romance.";

            return "Private/revealing outfit guidance: the farmer is in " + place + " wearing " + outfitKind + ". " + intimacyWeight + " Let the NPC react according to their own profile first. " + heartTone + " This is guidance, not an override: no mandatory blush words, no mandatory stammer, and no forced portrait type. The comment should still acknowledge the unusual/private nature of the outfit if it would be obvious.";
        }

        private static bool IsPrivateRevealingOutfitContext(OutfitAiContext context, out string outfitKind)
        {
            outfitKind = "";
            if (context == null || !IsPrivateCandidateInterior(context))
                return false;

            string allClues = string.Join(" ", new[]
            {
                context.OutfitName,
                context.SafeOutfitHint,
                context.DialogueKey,
                SanitizeThemeContextForPrompt(context.ThemeContext)
            }).ToLowerInvariant();

            if (LooksLikeSleepwearOrIntimate(allClues))
            {
                outfitKind = "sleepwear / pajamas / intimate clothing";
                return true;
            }

            if (LooksLikeSwimwearOrBeachwear(allClues))
            {
                outfitKind = "swimwear / bikini / beachwear";
                return true;
            }

            return false;
        }

        private static bool LooksLikeSleepwearOrIntimate(string allClues)
        {
            if (string.IsNullOrWhiteSpace(allClues))
                return false;

            return allClues.Contains("pajama") || allClues.Contains("pijama")
                || allClues.Contains("nightgown") || allClues.Contains("camisola")
                || allClues.Contains("lingerie") || allClues.Contains("underwear")
                || allClues.Contains("intimate") || allClues.Contains("íntimo")
                || allClues.Contains("intimo") || allClues.Contains("nightwear")
                || allClues.Contains("sleepwear") || allClues.Contains("negligee")
                || allClues.Contains("nightie") || allClues.Contains("robe")
                || allClues.Contains("roupa de dormir") || allClues.Contains("roupa íntima")
                || allClues.Contains("roupa intima");
        }

        private static bool LooksLikeSwimwearOrBeachwear(string allClues)
        {
            if (string.IsNullOrWhiteSpace(allClues))
                return false;

            return allClues.Contains("swim") || allClues.Contains("swimsuit")
                || allClues.Contains("bathing suit") || allClues.Contains("beachwear")
                || allClues.Contains("bikini") || allClues.Contains("biquíni")
                || allClues.Contains("biquini") || allClues.Contains("sunga")
                || allClues.Contains("beach") || allClues.Contains("praia")
                || allClues.Contains("maiô") || allClues.Contains("maio")
                || allClues.Contains("banho") || allClues.Contains("roupa de banho");
        }

        private static string BuildPrivateRevealingPromptRule(OutfitAiContext context)
        {
            // Private/revealing outfit context is handled by BuildNaturalContextHint and
            // BuildFinalSituationalOverride. Keep this helper empty to avoid repeating the
            // same rule several times in the prompt.
            return "";
        }

        private static string BuildSebastianCustomSoftnessOverride(OutfitAiContext context)
        {
            // Sebastian's custom behavior belongs in Sebastian.json. Keep this code path neutral
            // so the ficha, relationship level, location, and outfit context decide his reaction.
            return "";
        }

        private static string BuildFinalSituationalOverride(OutfitAiContext context)
        {
            if (context == null)
                return "";

            string allClues = string.Join(" ", new[]
            {
                context.OutfitName,
                context.SafeOutfitHint,
                context.DialogueKey,
                SanitizeThemeContextForPrompt(context.ThemeContext)
            }).ToLowerInvariant();

            bool sleepOrIntimate = LooksLikeSleepwearOrIntimate(allClues);
            bool swimOrBeach = LooksLikeSwimwearOrBeachwear(allClues);

            if (!sleepOrIntimate && !(swimOrBeach && !context.IsBeachOrIsland))
                return "";

            string rel = (context.RelationshipStatus ?? "").ToLowerInvariant();
            bool explicitRomance = context.IsSpouse
                || rel.Equals("spouse", StringComparison.OrdinalIgnoreCase)
                || rel.Equals("dating", StringComparison.OrdinalIgnoreCase)
                || rel.Contains("married")
                || rel.Contains("boyfriend")
                || rel.Contains("girlfriend")
                || rel.Contains("namor");

            string outfitWord = sleepOrIntimate ? "sleepwear / pajamas / intimate clothing" : "swimwear / bikini / beachwear";

            if (IsPrivateCandidateInterior(context))
                return BuildSoftPrivateRevealingReactionRule(context, outfitWord);

            if (explicitRomance && !context.IsFarmHouse && !IsPrivateCandidateInterior(context))
                return "Context guidance: the farmer is wearing " + outfitWord + " in a non-private place where others may see. A romantic partner may show concern, protectiveness, jealousy, awkward humor, or fluster if that fits their personality and heart level, but do not force a single reaction pattern.";

            return "Context guidance: " + outfitWord + " is not an ordinary everyday outfit in this place. React naturally to the situation — surprise, comedy, concern, bluntness, teasing, or warmth according to the NPC and relationship level. Do not treat it like a normal fashion review.";
        }

        private static string BuildSceneGroundingInstruction(OutfitAiContext context)
        {
            string location = context == null ? "" : StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName);
            string locationType = context == null ? "" : HumanizeTechnicalLabelForPrompt(context.LocationType);

            return "SCENE GROUNDING RULE: do not invent current objects, props, positions, or actions. The NPC profile may mention hobbies, furniture, work, instruments, motorcycles, computers, books, animals, or favorite places, but those are personality background only, not confirmed current scene facts. Only mention a specific object/place/action if it is explicitly in the current location/context or clearly part of the farmer's visible outfit/support data. Confirmed current location is: "
                + StringUtils.FirstNonEmpty(location, "unknown")
                + ". Private location type/context: "
                + StringUtils.FirstNonEmpty(locationType, "unknown")
                + ". Safe natural wording for the CURRENT scene is: here, in this room, at home, inside, or outside. Unsafe as a current fact unless explicitly confirmed: 'in front of my motorcycle', 'by my computer', 'on my bed', 'at the beach', 'in the saloon', 'during band practice', or anything implying the farmer/NPC moved somewhere else. Hypothetical jokes or comparisons are allowed when clearly phrased as imagination (e.g. 'se você aparecesse assim...', 'dá pra imaginar...'), but any place, activity, creature, or theme used in such a comparison must come from THIS NPC's own personality, interests, and world — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops) that this character would not naturally think about.";
        }
        private static string BuildTechnicalContextLabelInstruction(OutfitAiContext context)
        {
            return "Private labels like indoor, outdoor, inside/outside variant, NPC room, NpcRoom, outfit category, dialogue category, theme guidance, and internal theme keys are only metadata. Never say those labels in the final dialogue. If location matters, translate it into natural in-world wording like here, in your room, at home, outside, by the beach, or at the festival.";
        }

        private static void AppendNoticedChangeContextForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null || context == null)
                return;

            string changeType = string.IsNullOrWhiteSpace(context.NoticedChangeType) ? "Outfit" : context.NoticedChangeType.Trim();
            builder.AppendLine("Private noticed visual change type: " + changeType + ". Use it to choose the compliment focus; do not say this technical label.");

            if (!string.IsNullOrWhiteSpace(context.SafeNoticedChangeHint))
                builder.AppendLine("Readable changed item/theme clue: " + context.SafeNoticedChangeHint + ". Use its meaning naturally; if it names a recognizable reference/theme, the NPC may mention it when fitting. Do not recite technical slot/file names.");

            bool noticedAccessoryRemoval = context.IsAccessoryChange
                && !string.IsNullOrWhiteSpace(context.NoticedChangeName)
                && context.NoticedChangeName.TrimStart().StartsWith("removed ", StringComparison.OrdinalIgnoreCase);
            if (noticedAccessoryRemoval)
            {
                if (context.NpcWitnessedPreviousAccessory)
                    builder.AppendLine("Accessory removal rule: the changed accessory clue describes something the farmer just REMOVED. It is no longer being worn. This NPC has seen the farmer in this look before, so they MAY react to the absence/change, e.g. noticing that the previous wings/cape/accessory are gone, that the outfit looks less chaotic now, or comparing the current look without it to the earlier combo.");
                else
                    builder.AppendLine("Accessory removal rule: the changed accessory clue describes something the farmer just REMOVED, so it is no longer being worn. IMPORTANT: this NPC never saw the farmer wearing that accessory, so they have NO memory of it. Do NOT reference 'the accessory from before', 'that cute thing you had', or any past version of the look, and do not imply you remember a previous combination. React only to how the farmer looks RIGHT NOW, as if seeing them for the first time today.");
            }

            if (context.IsAccessoryChange && context.NpcWitnessedPreviousAccessory && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
                builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this accessory reaction, compare the changed accessory with this existing outfit/theme when it creates a funny, strange, cute, ugly, dramatic, or impossible combination. Do not ignore either side of the combo. If the accessory was removed, compare the current outfit-without-that-accessory to the previous combination.");
            else if (context.IsAccessoryChange && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
                builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this accessory reaction, you may comment on how the changed accessory works with this existing outfit/theme when the combination is funny, strange, cute, ugly, or dramatic. Do not reference any previous/removed version you did not witness.");

            if (context.IsHatChange && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
                builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this headwear reaction, you may compare the head item with the existing outfit/theme when the combination is funny, strange, cute, ugly, dramatic, or mismatched.");

            if (context.IsOutfitChange)
                AppendPromptBlock(builder, promptStyle?.SavedOutfitFocusGuidance ?? PromptStyleService.FallbackSavedOutfitFocusGuidance, context);
            else if (context.IsHairChange)
                AppendPromptBlock(builder, promptStyle?.HairFocusGuidance ?? PromptStyleService.FallbackHairFocusGuidance, context);
            else if (context.IsHatChange)
                AppendPromptBlock(builder, promptStyle?.HatFocusGuidance ?? PromptStyleService.FallbackHatFocusGuidance, context);
            else if (context.IsAccessoryChange)
                AppendPromptBlock(builder, promptStyle?.AccessoryFocusGuidance ?? PromptStyleService.FallbackAccessoryFocusGuidance, context);
        }

        private static void AppendFashionSenseVisualSummaryForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null || context == null || !context.HasFashionSenseVisualSummary)
                return;

            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["VisualSummary"] = CollapseForPrompt(context.FashionSenseVisualSummary, 1300)
            };
            AppendPromptBlock(builder, promptStyle?.FashionSenseVisualSupportRule ?? PromptStyleService.FallbackFashionSenseVisualSupportRule, context, tokens);
            // Explicitly separate hair and hair accessories from the outfit so the AI never
            // blends hair/accessory colors into outfit descriptions.
            AppendPromptBlock(builder, promptStyle?.FashionSenseVisualSeparationRule ?? PromptStyleService.FallbackFashionSenseVisualSeparationRule, context, tokens);
        }

        private static void AppendSpecialItemReactionForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null || context == null || !context.HasSpecialItemReactionContext)
                return;

            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["SpecialItemData"] = CollapseForPrompt(context.SpecialItemReactionContext, 1200)
            };

            if (context.SpecialItemWasJustRemoved)
                AppendPromptBlock(builder, promptStyle?.SpecialItemRemovedRule ?? PromptStyleService.FallbackSpecialItemRemovedRule, context, tokens);
            else
                AppendPromptBlock(builder, promptStyle?.SpecialItemVisibleRule ?? PromptStyleService.FallbackSpecialItemVisibleRule, context, tokens);

            if (context.HasSpecialItemMemoryHint)
            {
                Dictionary<string, string> memTokens = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["ItemMemory"] = context.SpecialItemMemoryHint
                };
                AppendPromptBlock(builder, promptStyle?.SpecialItemMemoryRule ?? PromptStyleService.FallbackSpecialItemMemoryRule, context, memTokens);
            }

            if (!string.IsNullOrWhiteSpace(context.VanillaPantsMemoryHint))
                builder.AppendLine("Pants memory: " + context.VanillaPantsMemoryHint);
        }

        private static void AppendSpecialHatReactionForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null || context == null || !context.HasSpecialHatReactionContext)
                return;

            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["SpecialHatData"] = CollapseForPrompt(context.SpecialHatReactionContext, 1400)
            };
            AppendPromptBlock(builder, promptStyle?.SpecialVanillaHatRule ?? PromptStyleService.FallbackSpecialVanillaHatRule, context, tokens);
        }

        private static void AppendVanillaHatMemoryForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
        {
            if (builder == null || context == null || !context.HasVanillaHatMemoryHint)
                return;

            // Dedicated, high-priority line so the AI actually reflects that the NPC has seen this
            // hat before, instead of reacting as if it were the first time. Placed on its own rather
            // than buried inside the technical visual-summary block.
            Dictionary<string, string> tokens = new(StringComparer.OrdinalIgnoreCase)
            {
                ["HatMemory"] = context.VanillaHatMemoryHint
            };
            AppendPromptBlock(builder, promptStyle?.VanillaHatMemoryRule ?? PromptStyleService.FallbackVanillaHatMemoryRule, context, tokens);
        }

        private static string BuildNaturalContextHint(OutfitAiContext context)
        {
            if (context == null)
                return "";

            string allClues = string.Join(" ", new[]
            {
                context.OutfitName,
                context.SafeOutfitHint,
                context.DialogueKey,
                SanitizeThemeContextForPrompt(context.ThemeContext)
            }).ToLowerInvariant();

            bool swimOrBeach = LooksLikeSwimwearOrBeachwear(allClues);
            bool sleepOrIntimate = LooksLikeSleepwearOrIntimate(allClues);

            if (sleepOrIntimate)
            {
                if (context.IsSpouse && context.IsFarmHouse)
                    return "Context hint: the clothing is pajamas, sleepwear, underwear, or intimate clothing at home with spouse. React naturally as a couple at home, according to the NPC's personality: relaxed, affectionate, teasing, shy, practical, or amused as fits.";

                if (IsPrivateCandidateInterior(context))
                    return BuildSoftPrivateRevealingReactionRule(context, "sleepwear/nightwear/intimate clothing");

                if (context.IsIndoors)
                    return "Context hint: the clothing looks like pajamas, sleepwear, underwear, or intimate clothing indoors. It may feel surprising, funny, awkward, or personal depending on the NPC and relationship level. Do not treat it as a normal fashion review.";

                return "Context hint: the clothing looks like pajamas, sleepwear, underwear, or intimate clothing in a public/outdoor area. React naturally to the incongruity — surprise, teasing, concern, comedy, or bluntness according to the NPC.";
            }

            if (swimOrBeach && !context.IsBeachOrIsland)
            {
                if (context.IsSpouse && context.IsFarmHouse)
                    return "Natural context hint: the outfit is swimwear or beachwear at home with spouse. React as their personality supports: relaxed, warm, teasing, shy, flirty, practical, or amused.";

                if (IsPrivateCandidateInterior(context))
                    return BuildSoftPrivateRevealingReactionRule(context, "swimwear/beachwear");

                if (context.IsIndoors)
                    return "Natural context hint: the outfit is swimwear or beachwear indoors, away from the beach. React to that situation naturally: puzzled, amused, concerned, teasing, or flustered depending on the NPC and relationship level.";

                return "Natural context hint: the outfit seems like swimwear or beachwear, but the farmer is not at a beach, pool, or island. React naturally to the mismatch instead of treating it like a normal outfit.";
            }

            // Location-specific behavior hints are intentionally disabled so the GENERAL
            // prompt (which already considers time, weather, location, and privacy) drives
            // the scene without hardcoded per-location instructions. The blocks below were
            // commented out on purpose; the situational outfit-incongruity hints above
            // (sleepwear / swimwear, including the private-reveal safety rule) are kept,
            // because those are content-safety guidance, not "how to act in this room."
            //
            // if (context.IsNpcRoom)
            //     return "Natural context hint: the farmer is in the NPC's room/private space. Do not say 'NPC room' or 'indoor'; if the place matters, phrase it naturally as here, in my room, or somewhere more private.";
            //
            // if (context.IsNpcPersonalLocation)
            //     return "Natural context hint: the farmer is inside this marriage candidate's home/private indoor space. Do not say internal labels; phrase it naturally as here, at my place, or inside if relevant.";
            //
            // if (context.IsIndoors)
            //     return "Natural context hint: the farmer is inside. Do not say 'indoor'; if the place matters, phrase it naturally as here or inside.";

            return "";
        }



        private static string BuildLocalSeasonAuthorityInstruction(OutfitAiContext context)
        {
            if (context == null)
                return "";

            string actualSeasonKey = NormalizeSeasonKey(context.Season);
            string actualSeason = FormatSeasonForPrompt(context.Season, context.TargetLanguage);
            string outfitSeason = DescribeInferredOutfitSeasonForPrompt(context);

            StringBuilder builder = new();
            builder.Append("AUTHORITATIVE SEASON RULE: the actual current in-game season is ").Append(actualSeason).Append(". ");
            builder.Append("Outfit/theme clues may suggest a different season, but those clues describe the outfit style, not the date. ");
            builder.Append("Do not replace the actual season with another one. ");

            if (!string.IsNullOrWhiteSpace(outfitSeason))
            {
                builder.Append("This outfit appears to have ").Append(outfitSeason).Append(" vibes. ");
                builder.Append("If that clashes with the actual season, mention the contrast using the actual season above.");
            }
            else if (!string.IsNullOrWhiteSpace(actualSeasonKey))
            {
                builder.Append("Only mention another season if the outfit itself clearly has that seasonal theme.");
            }

            return builder.ToString();
        }

        private static string FormatSeasonForPrompt(string season, string targetLanguage)
        {
            string key = NormalizeSeasonKey(season);
            bool portuguese = !string.IsNullOrWhiteSpace(targetLanguage) && targetLanguage.IndexOf("Portuguese", StringComparison.OrdinalIgnoreCase) >= 0;

            return key switch
            {
                "spring" => portuguese ? "spring / primavera" : "spring",
                "summer" => portuguese ? "summer / verão" : "summer",
                "fall" => portuguese ? "fall / autumn / outono" : "fall / autumn",
                "winter" => portuguese ? "winter / inverno" : "winter",
                _ => string.IsNullOrWhiteSpace(season) ? "unknown" : season
            };
        }

        private static string NormalizeSeasonKey(string season)
        {
            if (string.IsNullOrWhiteSpace(season))
                return "";

            string value = season.Trim().ToLowerInvariant();
            if (value.Contains("spring") || value.Contains("primavera"))
                return "spring";
            if (value.Contains("summer") || value.Contains("verão") || value.Contains("verao"))
                return "summer";
            if (value.Contains("fall") || value.Contains("autumn") || value.Contains("outono"))
                return "fall";
            if (value.Contains("winter") || value.Contains("inverno"))
                return "winter";

            return value;
        }

        private static string DescribeInferredOutfitSeasonForPrompt(OutfitAiContext context)
        {
            HashSet<string> inferred = InferSeasonKeysFromOutfitClues(context);
            if (inferred.Count <= 0)
                return "";

            List<string> labels = new();
            foreach (string season in inferred)
            {
                labels.Add(season switch
                {
                    "spring" => "spring",
                    "summer" => "summer/beach",
                    "fall" => "fall/autumn",
                    "winter" => "winter/Christmas",
                    _ => season
                });
            }

            return string.Join(" and ", labels);
        }

        private static HashSet<string> InferSeasonKeysFromOutfitClues(OutfitAiContext context)
        {
            HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
            if (context == null)
                return result;

            string allClues = string.Join(" ", new[]
            {
                context.OutfitName,
                context.SafeOutfitHint,
                context.DialogueKey,
                SanitizeThemeContextForPrompt(context.ThemeContext),
                SanitizeThemeContextForPrompt(context.ThemePriorityInstruction)
            }).ToLowerInvariant();

            if (allClues.Contains("xmas") || allClues.Contains("christmas") || allClues.Contains("natal") || allClues.Contains("noel") || allClues.Contains("winter") || allClues.Contains("snow") || allClues.Contains("neve") || allClues.Contains("inverno"))
                result.Add("winter");

            if (allClues.Contains("swim") || allClues.Contains("bikini") || allClues.Contains("beach") || allClues.Contains("praia") || allClues.Contains("maiô") || allClues.Contains("maio") || allClues.Contains("summer") || allClues.Contains("verão") || allClues.Contains("verao"))
                result.Add("summer");

            if (allClues.Contains("spring") || allClues.Contains("primavera") || allClues.Contains("flower dance") || allClues.Contains("flowerdance"))
                result.Add("spring");

            if (allClues.Contains("fall") || allClues.Contains("autumn") || allClues.Contains("outono") || allClues.Contains("spirit") || allClues.Contains("halloween"))
                result.Add("fall");

            return result;
        }

        private static string ValidateLocalSeasonReferences(string lower, OutfitAiContext context)
        {
            if (string.IsNullOrWhiteSpace(lower) || context == null)
                return null;

            string normalizedText = " " + Regex.Replace(lower, @"[^\p{L}\p{N}]+", " ").Trim() + " ";

            string actual = NormalizeSeasonKey(context.Season);
            HashSet<string> allowed = InferSeasonKeysFromOutfitClues(context);
            if (!string.IsNullOrWhiteSpace(actual))
                allowed.Add(actual);

            Dictionary<string, string[]> aliases = new(StringComparer.OrdinalIgnoreCase)
            {
                ["spring"] = new[] { " spring ", " primavera " },
                ["summer"] = new[] { " summer ", " verão ", " verao " },
                ["fall"] = new[] { " fall ", " autumn ", " outono " },
                ["winter"] = new[] { " winter ", " inverno " }
            };

            foreach (var pair in aliases)
            {
                bool mentioned = pair.Value.Any(alias => normalizedText.Contains(alias));
                if (!mentioned)
                    continue;

                if (!allowed.Contains(pair.Key))
                    return "local response confused the actual season with " + pair.Key;
            }

            return null;
        }

        private static string BuildSeasonalAwarenessInstruction(OutfitAiContext context)
        {
            if (context == null)
                return "";

            string allClues = string.Join(" ", new[]
            {
                context.OutfitName,
                context.SafeOutfitHint,
                context.DialogueKey,
                SanitizeThemeContextForPrompt(context.ThemeContext)
            }).ToLowerInvariant();

            string season = (context.Season ?? "").ToLowerInvariant();
            bool christmasOrWinter =
                allClues.Contains("xmas") || allClues.Contains("christmas") || allClues.Contains("natal") ||
                allClues.Contains("noel") || allClues.Contains("winter") || allClues.Contains("snow") ||
                allClues.Contains("neve") || allClues.Contains("inverno");

            if (christmasOrWinter && !season.Contains("winter"))
            {
                return "Seasonal awareness: the outfit clue/theme suggests Christmas, snow, or winter, but the current season is " + context.Season + ". React to that mismatch in a human way if it fits the NPC: gentle teasing, surprise, amusement, curiosity, or finding it charmingly out of place. Do not force a line saying it also suits or works for the current season.";
            }

            bool swimOrBeach = LooksLikeSwimwearOrBeachwear(allClues);
            if (swimOrBeach && (season.Contains("winter") || (context.Weather ?? "").IndexOf("snow", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return "Seasonal awareness: the outfit clue/theme suggests swimwear or beachwear, but the current season/weather is cold or snowy. Mention that contrast naturally if it fits the character. Do not use technical labels like indoor or theme guidance.";
            }

            return "Season is flavor, not a requirement: only bring up the season/weather if it genuinely connects to what is worn (e.g. a coat on a chilly rainy spring day, a sundress in summer). NEVER force a seasonal tie-in, and NEVER add a closing line that says the look also suits, fits, or works for the current season just to wrap up — if there is no real connection, end without mentioning the season at all. Use location, weather, and time the same way: only when they add something real. Never repeat technical labels like indoor, outdoor, NPC room, dialogue category, or theme guidance.";
        }

        private static string BuildLanguageExampleLocalLine(string targetLanguage)
        {
            return "<spoken outfit reaction in the current game language; no portrait commands inside the text>";
        }
















        // Inline helper so the scoring loop above stays readable.
        private static string ValidateLocalGeneratedDialogueText(string text, OutfitAiContext context, CharacterAiProfile profile, ModConfig config)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "empty local dialogue";

            string stripped = DialogueValidator.StripDialogueMarkup(text);
            string lower = " " + stripped.ToLowerInvariant() + " ";

            int desiredMin = Math.Max(40, GetMinimumLengthTarget(config, ai: null));
            int allowedShortfall = Math.Max(
                desiredMin >= 300 ? 70 : desiredMin >= 180 ? 45 : 25,
                (int)Math.Round(desiredMin * 0.18)
            );
            int hardMin = Math.Max(40, desiredMin - allowedShortfall);
            if (stripped.Length < hardMin)
                return "local dialogue was too short for configured minimum (" + stripped.Length + "/" + desiredMin + " visible characters, retry threshold " + hardMin + ")";

            if (LooksLikeThirdPersonNarrationForNpc(lower, context?.NpcDisplayName ?? context?.NpcName))
                return "local response was narration instead of spoken dialogue";

            if (LooksLikeGenericCutesyLocalLine(lower, context))
                return "local response sounded like generic cutesy/poetic praise instead of the NPC";

            string themeSpecificityIssue = DialogueValidator.ValidateRecognizableThemeSpecificity(text, context);
            if (!string.IsNullOrWhiteSpace(themeSpecificityIssue))
                return "local " + themeSpecificityIssue;

            string accessoryCombinationIssue = DialogueValidator.ValidateAccessoryOutfitCombinationSpecificity(text, context);
            if (!string.IsNullOrWhiteSpace(accessoryCombinationIssue))
                return "local " + accessoryCombinationIssue;

            if (LooksLikeUnrelatedLocalLine(lower))
                return "local response introduced unrelated generic details";

            string seasonIssue = ValidateLocalSeasonReferences(lower, context);
            if (!string.IsNullOrWhiteSpace(seasonIssue))
                return seasonIssue;

            return null;
        }

        private static bool LooksLikeThirdPersonNarrationForNpc(string lower, string npcDisplayName)
        {
            if (string.IsNullOrWhiteSpace(lower))
                return false;

            string npc = (npcDisplayName ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(npc))
            {
                if (lower.Contains(" " + npc + " is ") || lower.Contains(" " + npc + " looks ") || lower.Contains(" " + npc + " smiles ") || lower.Contains(" " + npc + " blushes "))
                    return true;
                if (lower.Contains(" " + npc + " está ") || lower.Contains(" " + npc + " esta ") || lower.Contains(" " + npc + " olha ") || lower.Contains(" " + npc + " sorri ") || lower.Contains(" " + npc + " fica "))
                    return true;
            }

            return lower.Contains("contexto atual")
                || lower.Contains("current context")
                || lower.Contains("tonalidade")
                || lower.Contains("tone:")
                || lower.Contains("portrait:")
                || lower.Contains("**portrait**")
                || lower.Contains("stage direction")
                || lower.Contains("scene description");
        }

        private static bool LooksLikeGenericCutesyLocalLine(string lower, OutfitAiContext context)
        {
            // Do not block words like radiant/radiante, wonderful/maravilhoso, magical/mágico, etc.
            // They can sound clichéd in some outputs, but certain NPCs or outfit themes may use them naturally.
            // The prompt should guide style; the validator should not hard-ban this vocabulary.
            return false;
        }

        private static bool LooksLikeUnrelatedLocalLine(string lower)
        {
            if (string.IsNullOrWhiteSpace(lower))
                return false;

            string[] unrelated =
            {
                " crafting wood ", " chopping wood ", " perfect for mining ", " fighting monsters ", " watering crops ",
                " cortar madeira ", " minerar ", " lutar contra monstros ", " regar plantações ", " regar plantacoes "
            };

            foreach (string phrase in unrelated)
            {
                if (lower.Contains(phrase))
                    return true;
            }

            return false;
        }

        /* PORTRAIT_SCORE_SYSTEM — commented out: portrait selection is now left entirely to the AI.
           The AI reads portrait descriptions from the NPC profile and decides which to use.
        */


        private static string BuildCacheKey(OutfitAiContext context, ModConfig config, string prompt, ActiveAiSettings ai)
        {
            return string.Join("|", new[]
            {
                ai.Provider ?? "",
                ai.Model ?? "",
                ai.TemperaturePercent.ToString(),
                ai.MaxCharacters.ToString(),
                context?.VisionImage?.Hash ?? "",
                context?.FashionSenseVisualSummary ?? "",
                context?.NoticedChangeType ?? "",
                context?.NoticedChangeName ?? "",
                context?.SafeNoticedChangeHint ?? "",
                context.NpcName ?? "",
                context.DialogueKey ?? "",
                context.OutfitName ?? "",
                context.LocationName ?? "",
                context.Season ?? "",
                context.Weather ?? "",
                context.RelationshipStatus ?? "",
                context.RelationshipHearts.ToString(),
                prompt.GetHashCode().ToString()
            });
        }

        private static string CollapseForPrompt(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = Regex.Replace(text, @"\s+", " ").Trim();
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, Math.Max(0, maxLength)).Trim() + "...";
        }

        private static string TrimForLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length <= 500 ? text : text.Substring(0, 500) + "...";
        }
    }
}
