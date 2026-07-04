using System;
using StardewModdingAPI;

namespace OutfitReactions
{
    internal static class ModConfigMenu
    {
        public static void Register(ModEntry mod, IGenericModConfigMenuApi configMenu)
        {
            if (configMenu == null)
                return;

            mod.Config.ApplyAiDefaultsAndLimits();

            configMenu.Register(
                mod: mod.ModManifest,
                reset: () => mod.Config = new ModConfig(),
                save: () =>
                {
                    mod.Config.ApplyAiDefaultsAndLimits();
                    mod.Helper.WriteConfig(mod.Config);
                    mod.QueueAiConnectionTestFromConfigMenu();
                }
            );

            string[] providers = new[] { "Gemini", "OpenAI", "OpenRouter", "Mistral", "Groq", "Together", "Anthropic", "xAI", "Cerebras", "Perplexity", "DeepSeek", "Local" };

            string NormalizeProvider(string provider)
            {
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                    return "Gemini";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                    return "OpenAI";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                    return "OpenRouter";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
                    return "Mistral";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
                    return "Groq";
                if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Together AI", StringComparison.OrdinalIgnoreCase)))
                    return "Together";
                if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)))
                    return "Local";
                if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) || provider.Equals("Claude", StringComparison.OrdinalIgnoreCase)))
                    return "Anthropic";
                if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Grok", StringComparison.OrdinalIgnoreCase)))
                    return "xAI";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
                    return "Cerebras";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
                    return "Perplexity";
                if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
                    return "DeepSeek";
                return "Gemini";
            }

            string ActiveProvider()
            {
                return NormalizeProvider(mod.Config.GetActiveProvider());
            }

            string FormatProvider(string provider)
            {
                string normalized = NormalizeProvider(provider);
                if (normalized.Equals("Local", StringComparison.OrdinalIgnoreCase))
                    return "Local / OpenAI Compatible";
                if (normalized.Equals("Together", StringComparison.OrdinalIgnoreCase))
                    return "Together AI";
                if (normalized.Equals("xAI", StringComparison.OrdinalIgnoreCase))
                    return "xAI (Grok)";
                return normalized;
            }

            string GetSlotModel(int slot)
            {
                return slot switch
                {
                    1 => mod.Config.AiModelSlot1,
                    2 => mod.Config.AiModelSlot2,
                    3 => mod.Config.AiModelSlot3,
                    4 => mod.Config.AiModelSlot4,
                    5 => mod.Config.AiModelSlot5,
                    _ => ""
                };
            }

            void SetSlotModel(int slot, string value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiModelSlot1 = value; break;
                    case 2: mod.Config.AiModelSlot2 = value; break;
                    case 3: mod.Config.AiModelSlot3 = value; break;
                    case 4: mod.Config.AiModelSlot4 = value; break;
                    case 5: mod.Config.AiModelSlot5 = value; break;
                }
            }

            string GetSlotApiKey(int slot)
            {
                return slot switch
                {
                    1 => mod.Config.AiApiKeySlot1,
                    2 => mod.Config.AiApiKeySlot2,
                    3 => mod.Config.AiApiKeySlot3,
                    4 => mod.Config.AiApiKeySlot4,
                    5 => mod.Config.AiApiKeySlot5,
                    _ => ""
                };
            }

            void SetSlotApiKey(int slot, string value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiApiKeySlot1 = value; break;
                    case 2: mod.Config.AiApiKeySlot2 = value; break;
                    case 3: mod.Config.AiApiKeySlot3 = value; break;
                    case 4: mod.Config.AiApiKeySlot4 = value; break;
                    case 5: mod.Config.AiApiKeySlot5 = value; break;
                }
            }

            string GetSlotEndpoint(int slot)
            {
                return slot switch
                {
                    1 => mod.Config.AiCustomEndpointSlot1,
                    2 => mod.Config.AiCustomEndpointSlot2,
                    3 => mod.Config.AiCustomEndpointSlot3,
                    4 => mod.Config.AiCustomEndpointSlot4,
                    5 => mod.Config.AiCustomEndpointSlot5,
                    _ => ""
                };
            }

            void SetSlotEndpoint(int slot, string value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiCustomEndpointSlot1 = value; break;
                    case 2: mod.Config.AiCustomEndpointSlot2 = value; break;
                    case 3: mod.Config.AiCustomEndpointSlot3 = value; break;
                    case 4: mod.Config.AiCustomEndpointSlot4 = value; break;
                    case 5: mod.Config.AiCustomEndpointSlot5 = value; break;
                }
            }

            string GetSlotProvider(int slot)
            {
                return slot switch
                {
                    1 => mod.Config.AiProviderSlot1,
                    2 => mod.Config.AiProviderSlot2,
                    3 => mod.Config.AiProviderSlot3,
                    4 => mod.Config.AiProviderSlot4,
                    5 => mod.Config.AiProviderSlot5,
                    _ => "Gemini"
                };
            }

            void SetSlotProvider(int slot, string value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiProviderSlot1 = value; break;
                    case 2: mod.Config.AiProviderSlot2 = value; break;
                    case 3: mod.Config.AiProviderSlot3 = value; break;
                    case 4: mod.Config.AiProviderSlot4 = value; break;
                    case 5: mod.Config.AiProviderSlot5 = value; break;
                }
            }

            bool GetSlotEnabled(int slot)
            {
                return slot switch
                {
                    1 => mod.Config.AiSlot1Enabled,
                    2 => mod.Config.AiSlot2Enabled,
                    3 => mod.Config.AiSlot3Enabled,
                    4 => mod.Config.AiSlot4Enabled,
                    5 => mod.Config.AiSlot5Enabled,
                    _ => false
                };
            }

            void SetSlotEnabled(int slot, bool value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiSlot1Enabled = value; break;
                    case 2: mod.Config.AiSlot2Enabled = value; break;
                    case 3: mod.Config.AiSlot3Enabled = value; break;
                    case 4: mod.Config.AiSlot4Enabled = value; break;
                    case 5: mod.Config.AiSlot5Enabled = value; break;
                }
            }

            string GetSlotVisionMode(int slot)
            {
                string v = slot switch
                {
                    1 => mod.Config.AiVisionModeSlot1,
                    2 => mod.Config.AiVisionModeSlot2,
                    3 => mod.Config.AiVisionModeSlot3,
                    4 => mod.Config.AiVisionModeSlot4,
                    5 => mod.Config.AiVisionModeSlot5,
                    _ => "Auto"
                };
                return string.IsNullOrWhiteSpace(v) ? "Auto" : v;
            }

            void SetSlotVisionMode(int slot, string value)
            {
                switch (slot)
                {
                    case 1: mod.Config.AiVisionModeSlot1 = value; break;
                    case 2: mod.Config.AiVisionModeSlot2 = value; break;
                    case 3: mod.Config.AiVisionModeSlot3 = value; break;
                    case 4: mod.Config.AiVisionModeSlot4 = value; break;
                    case 5: mod.Config.AiVisionModeSlot5 = value; break;
                }
            }

            int GetActiveTemperature()
            {
                switch (ActiveProvider())
                {
                    case "Gemini": return mod.Config.GeminiAiTemperaturePercent;
                    case "OpenAI": return mod.Config.OpenAiAiTemperaturePercent;
                    case "OpenRouter": return mod.Config.OpenRouterAiTemperaturePercent;
                    case "Mistral": return mod.Config.MistralAiTemperaturePercent;
                    case "Groq": return mod.Config.GroqAiTemperaturePercent;
                    case "Together": return mod.Config.TogetherAiTemperaturePercent;
                    case "Local": return mod.Config.LocalAiTemperaturePercent;
                    case "Anthropic": return mod.Config.AnthropicAiTemperaturePercent;
                    case "xAI": return mod.Config.XAiTemperaturePercent;
                    case "Cerebras": return mod.Config.CerebrasAiTemperaturePercent;
                    case "Perplexity": return mod.Config.PerplexityAiTemperaturePercent;
                    default: return mod.Config.DeepSeekAiTemperaturePercent;
                }
            }

            void SetActiveTemperature(int value)
            {
                switch (ActiveProvider())
                {
                    case "Gemini": mod.Config.GeminiAiTemperaturePercent = value; break;
                    case "OpenAI": mod.Config.OpenAiAiTemperaturePercent = value; break;
                    case "OpenRouter": mod.Config.OpenRouterAiTemperaturePercent = value; break;
                    case "Mistral": mod.Config.MistralAiTemperaturePercent = value; break;
                    case "Groq": mod.Config.GroqAiTemperaturePercent = value; break;
                    case "Together": mod.Config.TogetherAiTemperaturePercent = value; break;
                    case "Local": mod.Config.LocalAiTemperaturePercent = value; break;
                    case "Anthropic": mod.Config.AnthropicAiTemperaturePercent = value; break;
                    case "xAI": mod.Config.XAiTemperaturePercent = value; break;
                    case "Cerebras": mod.Config.CerebrasAiTemperaturePercent = value; break;
                    case "Perplexity": mod.Config.PerplexityAiTemperaturePercent = value; break;
                    default: mod.Config.DeepSeekAiTemperaturePercent = value; break;
                }
            }

            int GetActiveTimeout()
            {
                switch (ActiveProvider())
                {
                    case "Gemini": return mod.Config.GeminiAiTimeoutSeconds;
                    case "OpenAI": return mod.Config.OpenAiAiTimeoutSeconds;
                    case "OpenRouter": return mod.Config.OpenRouterAiTimeoutSeconds;
                    case "Mistral": return mod.Config.MistralAiTimeoutSeconds;
                    case "Groq": return mod.Config.GroqAiTimeoutSeconds;
                    case "Together": return mod.Config.TogetherAiTimeoutSeconds;
                    case "Local": return mod.Config.LocalAiTimeoutSeconds;
                    case "Anthropic": return mod.Config.AnthropicAiTimeoutSeconds;
                    case "xAI": return mod.Config.XAiTimeoutSeconds;
                    case "Cerebras": return mod.Config.CerebrasAiTimeoutSeconds;
                    case "Perplexity": return mod.Config.PerplexityAiTimeoutSeconds;
                    default: return mod.Config.DeepSeekAiTimeoutSeconds;
                }
            }

            void SetActiveTimeout(int value)
            {
                switch (ActiveProvider())
                {
                    case "Gemini": mod.Config.GeminiAiTimeoutSeconds = value; break;
                    case "OpenAI": mod.Config.OpenAiAiTimeoutSeconds = value; break;
                    case "OpenRouter": mod.Config.OpenRouterAiTimeoutSeconds = value; break;
                    case "Mistral": mod.Config.MistralAiTimeoutSeconds = value; break;
                    case "Groq": mod.Config.GroqAiTimeoutSeconds = value; break;
                    case "Together": mod.Config.TogetherAiTimeoutSeconds = value; break;
                    case "Local": mod.Config.LocalAiTimeoutSeconds = value; break;
                    case "Anthropic": mod.Config.AnthropicAiTimeoutSeconds = value; break;
                    case "xAI": mod.Config.XAiTimeoutSeconds = value; break;
                    case "Cerebras": mod.Config.CerebrasAiTimeoutSeconds = value; break;
                    case "Perplexity": mod.Config.PerplexityAiTimeoutSeconds = value; break;
                    default: mod.Config.DeepSeekAiTimeoutSeconds = value; break;
                }
            }

            int GetActiveMaxCharacters()
            {
                switch (ActiveProvider())
                {
                    case "Gemini": return mod.Config.GeminiAiMaxCharacters;
                    case "OpenAI": return mod.Config.OpenAiAiMaxCharacters;
                    case "OpenRouter": return mod.Config.OpenRouterAiMaxCharacters;
                    case "Mistral": return mod.Config.MistralAiMaxCharacters;
                    case "Groq": return mod.Config.GroqAiMaxCharacters;
                    case "Together": return mod.Config.TogetherAiMaxCharacters;
                    case "Local": return mod.Config.LocalAiMaxCharacters;
                    case "Anthropic": return mod.Config.AnthropicAiMaxCharacters;
                    case "xAI": return mod.Config.XAiMaxCharacters;
                    case "Cerebras": return mod.Config.CerebrasAiMaxCharacters;
                    case "Perplexity": return mod.Config.PerplexityAiMaxCharacters;
                    default: return mod.Config.DeepSeekAiMaxCharacters;
                }
            }

            void SetActiveMaxCharacters(int value)
            {
                switch (ActiveProvider())
                {
                    case "Gemini": mod.Config.GeminiAiMaxCharacters = value; break;
                    case "OpenAI": mod.Config.OpenAiAiMaxCharacters = value; break;
                    case "OpenRouter": mod.Config.OpenRouterAiMaxCharacters = value; break;
                    case "Mistral": mod.Config.MistralAiMaxCharacters = value; break;
                    case "Groq": mod.Config.GroqAiMaxCharacters = value; break;
                    case "Together": mod.Config.TogetherAiMaxCharacters = value; break;
                    case "Local": mod.Config.LocalAiMaxCharacters = value; break;
                    case "Anthropic": mod.Config.AnthropicAiMaxCharacters = value; break;
                    case "xAI": mod.Config.XAiMaxCharacters = value; break;
                    case "Cerebras": mod.Config.CerebrasAiMaxCharacters = value; break;
                    case "Perplexity": mod.Config.PerplexityAiMaxCharacters = value; break;
                    default: mod.Config.DeepSeekAiMaxCharacters = value; break;
                }
            }

            string FormatTemperature(int value)
            {
                return (value / 100f).ToString("0.00");
            }

            string T(string key)
            {
                return mod.Helper.Translation.Get(key).ToString();
            }

            string TWithNumber(string key, int number)
            {
                return mod.Helper.Translation.Get(key, new { number }).ToString();
            }

            string TWithValue(string key, int value)
            {
                return mod.Helper.Translation.Get(key, new { value }).ToString();
            }

            string FormatVisionMode(string value)
            {
                return value == "On"
                    ? T("gmcm.value.vision-mode.on")
                    : value == "Off"
                        ? T("gmcm.value.vision-mode.off")
                        : T("gmcm.value.vision-mode.auto");
            }

            string FormatProviderTranslated(string provider)
            {
                string normalized = NormalizeProvider(provider);
                if (normalized.Equals("Local", StringComparison.OrdinalIgnoreCase))
                    return T("gmcm.value.provider.local");
                if (normalized.Equals("Together", StringComparison.OrdinalIgnoreCase))
                    return T("gmcm.value.provider.together");
                return FormatProvider(normalized);
            }

            configMenu.AddSectionTitle(
                mod.ModManifest,
                () => T("gmcm.section.general")
            );

            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.enabled.name"),
                tooltip: () => T("gmcm.option.enabled.tooltip"),
                getValue: () => mod.Config.Enabled,
                setValue: value => mod.Config.Enabled = value
            );

            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.use-ai-cache.name"),
                tooltip: () => T("gmcm.option.use-ai-cache.tooltip"),
                getValue: () => mod.Config.UseAiCache,
                setValue: value => mod.Config.UseAiCache = value
            );

            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.player-reply.name"),
                tooltip: () => T("gmcm.option.player-reply.tooltip"),
                getValue: () => mod.Config.EnablePlayerReplyMenuAfterOutfitCompliment,
                setValue: value => mod.Config.EnablePlayerReplyMenuAfterOutfitCompliment = value
            );
            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.expressive-actions.name"),
                tooltip: () => T("gmcm.option.expressive-actions.tooltip"),
                getValue: () => mod.Config.EnableExpressiveAsteriskActions,
                setValue: value => mod.Config.EnableExpressiveAsteriskActions = value
            );

            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => "Use Fashion Sense ID as theme hint",
                tooltip: () => "When enabled, the readable version of Fashion Sense internal item IDs (outfit name, changed item) is sent to the AI as a theme/reference clue. "
                    + "This works well when IDs are descriptive (e.g. \"CuteFairyOutfit\", \"PikachuEars\"), but may not help — or may confuse — when IDs are generic, numeric, or technical (e.g. \"Set04\", \"Hair03\"). "
                    + "Disable if you notice the AI referencing outfit names incorrectly or describing themes that don't match the visual.",
                getValue: () => mod.Config.UseFsInternalIdAsHint,
                setValue: value => mod.Config.UseFsInternalIdAsHint = value
            );

            // --- Voice samples (MVP) ---
            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => "Use voice samples",
                tooltip: () => "Inject a few of each NPC's real in-game lines into the prompt as a voice/tone reference, to make reactions sound more like that character. They are used as style reference only, never copied, and the NPC profile stays in charge.",
                getValue: () => mod.Config.UseVoiceSamples,
                setValue: value => mod.Config.UseVoiceSamples = value
            );
            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => "Voice sample count",
                tooltip: () => "How many real in-game lines to use as a voice reference (kept small so the prompt stays lean). Default 6.",
                getValue: () => mod.Config.VoiceSampleCount,
                setValue: value => mod.Config.VoiceSampleCount = value,
                min: 1,
                max: 20
            );
            configMenu.AddTextOption(
                mod: mod.ModManifest,
                name: () => "Voice sample: excluded NPCs",
                tooltip: () => "Comma-separated NPC names to exclude from voice samples (e.g. a heavily customized character you want driven purely by its profile). Example: Sebastian, Abigail",
                getValue: () => mod.Config.VoiceSampleExcludedNpcs,
                setValue: value => mod.Config.VoiceSampleExcludedNpcs = value
            );

            for (int slot = 1; slot <= 5; slot++)
            {
                int capturedSlot = slot;

                configMenu.AddSectionTitle(
                    mod: mod.ModManifest,
                    text: () => TWithNumber("gmcm.section.profile", capturedSlot)
                );

                configMenu.AddBoolOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.profile-enabled.name"),
                    tooltip: () => T("gmcm.option.profile-enabled.tooltip"),
                    getValue: () => GetSlotEnabled(capturedSlot),
                    setValue: value => SetSlotEnabled(capturedSlot, value)
                );

                configMenu.AddTextOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.provider.name"),
                    tooltip: () => T("gmcm.option.provider.tooltip"),
                    getValue: () => NormalizeProvider(GetSlotProvider(capturedSlot)),
                    setValue: value => SetSlotProvider(capturedSlot, NormalizeProvider(value)),
                    allowedValues: providers,
                    formatAllowedValue: FormatProviderTranslated
                );

                configMenu.AddTextOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.model.name"),
                    tooltip: () => T("gmcm.option.model.tooltip"),
                    getValue: () => GetSlotModel(capturedSlot),
                    setValue: value => SetSlotModel(capturedSlot, value)
                );

                configMenu.AddTextOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.api-key.name"),
                    tooltip: () => T("gmcm.option.api-key.tooltip"),
                    getValue: () => GetSlotApiKey(capturedSlot),
                    setValue: value => SetSlotApiKey(capturedSlot, value)
                );

                configMenu.AddTextOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.endpoint.name"),
                    tooltip: () => T("gmcm.option.endpoint.tooltip"),
                    getValue: () => GetSlotEndpoint(capturedSlot),
                    setValue: value => SetSlotEndpoint(capturedSlot, value)
                );

                configMenu.AddTextOption(
                    mod: mod.ModManifest,
                    name: () => T("gmcm.option.vision-mode.name"),
                    tooltip: () => T("gmcm.option.vision-mode.tooltip"),
                    getValue: () => GetSlotVisionMode(capturedSlot),
                    setValue: value => SetSlotVisionMode(capturedSlot, value),
                    allowedValues: new[] { "Auto", "On", "Off" },
                    formatAllowedValue: FormatVisionMode
                );
            }

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.temperature.name"),
                tooltip: () => T("gmcm.option.temperature.tooltip"),
                getValue: GetActiveTemperature,
                setValue: SetActiveTemperature,
                min: 0,
                max: 200,
                interval: 5,
                formatValue: FormatTemperature
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.timeout.name"),
                tooltip: () => T("gmcm.option.timeout.tooltip"),
                getValue: GetActiveTimeout,
                setValue: SetActiveTimeout,
                min: 3,
                max: 120,
                interval: 1,
                formatValue: value => TWithValue("gmcm.value.seconds", value)
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.min-characters.name"),
                tooltip: () => T("gmcm.option.min-characters.tooltip"),
                getValue: () => mod.Config.AiMinimumCharacters,
                setValue: value => mod.Config.AiMinimumCharacters = value,
                min: 0,
                max: 2000,
                interval: 10,
                formatValue: value => value <= 0 ? T("gmcm.value.no-minimum") : TWithValue("gmcm.value.characters", value)
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.max-characters.name"),
                tooltip: () => T("gmcm.option.max-characters.tooltip"),
                getValue: GetActiveMaxCharacters,
                setValue: SetActiveMaxCharacters,
                min: 80,
                max: 2000,
                interval: 10,
                formatValue: value => TWithValue("gmcm.value.characters", value)
            );

            configMenu.AddSectionTitle(
                mod.ModManifest,
                () => T("gmcm.section.distance")
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.npc-reaction-chance.name"),
                tooltip: () => T("gmcm.option.npc-reaction-chance.tooltip"),
                getValue: () => mod.Config.NpcOutfitReactionChance,
                setValue: value => mod.Config.NpcOutfitReactionChance = value,
                min: 0,
                max: 100,
                interval: 5,
                formatValue: value => $"{value}%"
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.npc-repeated-visual-chance.name"),
                tooltip: () => T("gmcm.option.npc-repeated-visual-chance.tooltip"),
                getValue: () => mod.Config.NpcRepeatedVisualNoticeChance,
                setValue: value => mod.Config.NpcRepeatedVisualNoticeChance = value,
                min: 0,
                max: 100,
                interval: 5,
                formatValue: value => $"{value}%"
            );

            configMenu.AddBoolOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.debug-logging.name"),
                tooltip: () => T("gmcm.option.debug-logging.tooltip"),
                getValue: () => mod.Config.EnableDebugLogging,
                setValue: value => mod.Config.EnableDebugLogging = value
            );

            configMenu.AddTextOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.vanilla-hat-mode.name"),
                tooltip: () => T("gmcm.option.vanilla-hat-mode.tooltip"),
                getValue: () => NormalizeVanillaHatReactionMode(mod.Config.VanillaHatReactionMode),
                setValue: value => mod.Config.VanillaHatReactionMode = NormalizeVanillaHatReactionMode(value),
                allowedValues: new[] { "Combined", "HatOnly" },
                formatAllowedValue: value => T("gmcm.option.vanilla-hat-mode.value." + (value ?? "Combined"))
            );

            configMenu.AddTextOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.vanilla-special-item-mode.name"),
                tooltip: () => T("gmcm.option.vanilla-special-item-mode.tooltip"),
                getValue: () => NormalizeVanillaSpecialItemReactionMode(mod.Config.VanillaSpecialItemReactionMode),
                setValue: value => mod.Config.VanillaSpecialItemReactionMode = NormalizeVanillaSpecialItemReactionMode(value),
                allowedValues: new[] { "Combined", "ItemOnly" },
                formatAllowedValue: value => T("gmcm.option.vanilla-special-item-mode.value." + (value ?? "ItemOnly"))
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.outfit-notice-distance.name"),
                tooltip: () => T("gmcm.option.outfit-notice-distance.tooltip"),
                getValue: () => mod.Config.OutfitNoticeDistance,
                setValue: value => mod.Config.OutfitNoticeDistance = value,
                min: 100,
                max: 2000,
                interval: 50
            );

            configMenu.AddNumberOption(
                mod: mod.ModManifest,
                name: () => T("gmcm.option.outfit-cancel-distance.name"),
                tooltip: () => T("gmcm.option.outfit-cancel-distance.tooltip"),
                getValue: () => mod.Config.OutfitCancelDistance,
                setValue: value => mod.Config.OutfitCancelDistance = value,
                min: 200,
                max: 3000,
                interval: 50
            );
        }

        /// <summary>
        /// Normalizes the vanilla-hat reaction mode to one of the known values ("Combined" or
        /// "HatOnly"), defaulting to "Combined" for anything unrecognized so a bad/edited config
        /// value never breaks the dropdown or the reaction logic.
        /// </summary>
        internal static string NormalizeVanillaHatReactionMode(string mode)
        {
            if (string.Equals(mode, "HatOnly", System.StringComparison.OrdinalIgnoreCase))
                return "HatOnly";
            return "Combined";
        }

        /// <summary>
        /// Normalizes the vanilla special item reaction mode to one of the known values
        /// ("Combined" or "ItemOnly"), defaulting to "ItemOnly" for anything unrecognized.
        /// </summary>
        internal static string NormalizeVanillaSpecialItemReactionMode(string mode)
        {
            if (string.Equals(mode, "Combined", System.StringComparison.OrdinalIgnoreCase))
                return "Combined";
            return "ItemOnly";
        }
    }
}
