using StardewModdingAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OutfitReactions;

namespace OutfitReactions.Ai
{
    internal sealed partial class OutfitAiService
    {
        internal const string AccessoryClarificationMarker = "{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}";

        private readonly IMonitor monitor;
        private readonly Func<ModConfig> getConfig;
        private readonly VoiceSampleService voiceSamples;
        private readonly CharacterProfileCatalog profileCatalog;
        private readonly AiProviderClient aiClient;
        private readonly ConcurrentDictionary<string, string> memoryCache = new(StringComparer.OrdinalIgnoreCase);
        private bool warnedMultipleAiProfilesThisSession;
        internal readonly PromptStyleService PromptStyle;

        // Set by ModEntry. Returns true if the named NPC can be romanced (vanilla or modded).
        // Used to decide whether to offer the $a (angry) and $l (love) vanilla portraits, which
        // non-romanceable NPCs usually don't have in their portrait sheet.
        public Func<string, bool> IsRomanceableNpc
        {
            get => profileCatalog.IsRomanceableNpc;
            set => profileCatalog.IsRomanceableNpc = value;
        }

        public OutfitAiService(IModHelper helper, IMonitor monitor, Func<ModConfig> getConfig)
        {
            this.monitor = monitor;
            this.getConfig = getConfig;
            this.voiceSamples = new VoiceSampleService(helper, monitor);
            this.profileCatalog = new CharacterProfileCatalog(helper, monitor, voiceSamples);
            this.aiClient = new AiProviderClient(monitor);
            this.PromptStyle = new PromptStyleService(helper, monitor);
            this.PromptStyle.Load(quiet: true);
        }

        public void LoadProfiles(bool quiet = false)
        {
            profileCatalog.Clear();
            PromptStyle.Load(quiet);
            profileCatalog.Load(quiet);
        }

        public Dictionary<string, CharacterAiProfile> LoadDefaultProfilesFromFiles()
        {
            return profileCatalog.LoadDefaultProfilesFromFiles();
        }

        public bool HasProfile(string npcName)
        {
            return profileCatalog.HasProfile(npcName);
        }

        public bool TryResolveProfile(string internalName, string displayName, out CharacterAiProfile profile)
        {
            return profileCatalog.TryResolveProfile(internalName, displayName, out profile);
        }

        public void QueueConnectionTestFromConfigMenu()
        {
            ModConfig config = getConfig?.Invoke() ?? new ModConfig();
            config.ApplyAiDefaultsAndLimits();

            if (HasInvalidAiProfileSelection(config))
                return;

            ActiveAiSettings ai = ActiveAiSettingsResolver.Resolve(config);
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

                    if (string.IsNullOrWhiteSpace(ai.ApiKey) && !ActiveAiSettingsResolver.IsLocal(ai))
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

                    string prompt = ActiveAiSettingsResolver.IsLocal(testAi)
                        ? "Connection test. Return exactly one line beginning with '- ' and no explanation: - Connection successful."
                        : "Connection test. Return exactly one compact JSON object only with this exact shape: {\"text\":\"Connection successful.\",\"portrait\":\"\"}";

                    string raw = await aiClient.GenerateRawAsync(testAi, prompt);
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

        public bool TryGenerateCompliment(OutfitAiContext context, out string dialogue, CancellationToken cancellationToken = default)
        {
            dialogue = null;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                return false;

            if (HasInvalidAiProfileSelection(config))
                return false;

            ActiveAiSettings ai = ActiveAiSettingsResolver.Resolve(config);

            if (context == null || string.IsNullOrWhiteSpace(context.NpcName))
                return false;

            if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out CharacterAiProfile profile) || profile == null || !profile.Enabled)
                return false;

            if (string.IsNullOrWhiteSpace(ai.ApiKey) && !ActiveAiSettingsResolver.IsLocal(ai))
            {
                monitor.Log(" API key is empty for " + ai.Provider + ". Skipping built-in AI and using fallback.", LogLevel.Trace);
                return false;
            }

            string prompt = BuildPrompt(profile, context, config, ai);
            string cacheKey = BuildCacheKey(context, config, prompt, ai);
            if (config.UseAiCache && memoryCache.TryGetValue(cacheKey, out string cached) && !string.IsNullOrWhiteSpace(cached))
            {
                dialogue = cached;
                RememberDialogueOpening(dialogue);
                return true;
            }

            try
            {
                if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" Sending outfit compliment request for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Info);

                string raw = aiClient.GenerateRawAsync(ai, prompt, context.VisionImage, cancellationToken).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    monitor.Log(" Provider returned an empty response. Using fallback.", LogLevel.Warn);
                    return false;
                }


                if (!TryBuildValidatedDialogue(profile, context, ai, raw, out dialogue, out string validationIssue))
                {
                    if (TryBuildLenientDialogue(profile, context, ai, raw, out dialogue, out string lenientIssue))
                    {
                        monitor.Log(" Provider response did not pass the strict quality checks (" + validationIssue + "), but retry is disabled. Accepting the first usable AI line instead.", LogLevel.Warn);
                    }
                    else
                    {
                        monitor.Log(" Provider response was not usable (" + validationIssue + "; lenient parse: " + lenientIssue + "). Using fallback.", LogLevel.Warn);
                        return false;
                    }
                }

                if (config.UseAiCache)
                    memoryCache[cacheKey] = dialogue;

                RememberDialogueOpening(dialogue);

                if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" Generated outfit compliment for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Info);
                return true;
            }
            catch (TaskCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;
                int seconds = ActiveAiSettingsResolver.Resolve(getConfig?.Invoke()).TimeoutSeconds;
                monitor.Log($" Failed to generate outfit compliment: request timed out/canceled after {seconds}s. Try increasing the AI timeout or using a faster model.", LogLevel.Warn);
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to generate outfit compliment: " + ex.Message, LogLevel.Warn);
                return false;
            }
        }



        public bool TryGenerateFollowUp(OutfitAiContext context, string npcCompliment, string playerReply, out string dialogue, CancellationToken cancellationToken = default)
        {
            dialogue = null;

            ModConfig config = getConfig?.Invoke();
            if (config == null)
                return false;

            if (HasInvalidAiProfileSelection(config))
                return false;

            ActiveAiSettings ai = ActiveAiSettingsResolver.Resolve(config);

            if (context == null || string.IsNullOrWhiteSpace(context.NpcName) || string.IsNullOrWhiteSpace(playerReply))
                return false;

            if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out CharacterAiProfile profile) || profile == null || !profile.Enabled)
                return false;

            if (string.IsNullOrWhiteSpace(ai.ApiKey) && !ActiveAiSettingsResolver.IsLocal(ai))
            {
                monitor.Log(" API key is empty for " + ai.Provider + ". Skipping player-reply follow-up.", LogLevel.Trace);
                return false;
            }

            string prompt = BuildPlayerReplyFollowUpPrompt(profile, context, ai, npcCompliment, playerReply);
            PromptSizeDiagnostics.Log(
                monitor,
                "player-reply-follow-up",
                context.NpcName,
                ai.Provider,
                ai.Model,
                prompt.Length,
                context.HasVisionImage,
                new KeyValuePair<string, int>("complete-follow-up-prompt", prompt.Length));

            try
            {
                monitor.Log($" Sending player-reply follow-up request for {context.NpcName} using {ai.Provider}/{ai.Model}.", LogLevel.Debug);

                string raw = aiClient.GenerateRawAsync(ai, prompt, context.VisionImage, cancellationToken).GetAwaiter().GetResult();
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
                        monitor.Log(" Follow-up response did not pass strict checks (" + validationIssue + "), accepting lenient result.", LogLevel.Warn);
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
                            monitor.Log($" Follow-up attempt {attempt}/{maxRetries} failed ({lastIssue}). Retrying with corrective prompt.", LogLevel.Warn);

                            string retryPrompt = BuildFollowUpRetryPrompt(profile, context, ai, npcCompliment, playerReply, lastRaw, lastIssue);
                            PromptSizeDiagnostics.Log(
                                monitor,
                                "player-reply-retry",
                                context.NpcName,
                                ai.Provider,
                                ai.Model,
                                retryPrompt.Length,
                                false,
                                new KeyValuePair<string, int>("complete-retry-prompt", retryPrompt.Length));
                            string retryRaw = aiClient.GenerateRawAsync(ai, retryPrompt, null, cancellationToken).GetAwaiter().GetResult();

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
                if (cancellationToken.IsCancellationRequested)
                    return false;
                int seconds = ActiveAiSettingsResolver.Resolve(getConfig?.Invoke()).TimeoutSeconds;
                monitor.Log($" Failed to generate player-reply follow-up: request timed out/canceled after {seconds}s.", LogLevel.Warn);
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(" Failed to generate player-reply follow-up: " + ex.Message, LogLevel.Warn);
                return false;
            }
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


        public string BuildVoiceSampleReport()
        {
            return voiceSamples.BuildReport(profileCatalog.GetNpcNames(), getConfig?.Invoke());
        }

        public string BuildVoiceSamplePreview(string npcName, string currentSeason)
        {
            // Resolve to a loaded profile (by display name, internal name, or alias) so the
            // preview can show whether a profile matched and which name it reads dialogue from.
            bool matched = TryResolveProfile(npcName, npcName, out CharacterAiProfile profile) && profile != null;
            string resolvedName = matched ? (profile.NpcName ?? npcName) : npcName;
            return voiceSamples.BuildPreview(npcName, resolvedName, matched, currentSeason, getConfig?.Invoke());
        }

        /// <summary>Prepares optional game dialogue samples before a background AI request starts.</summary>
        public void PrepareVoiceSamplesForNpc(string npcName)
        {
            voiceSamples.PrepareSamplesForNpc(npcName, getConfig?.Invoke());
        }
    }
}
