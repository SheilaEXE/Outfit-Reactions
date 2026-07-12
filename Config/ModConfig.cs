using System;

namespace OutfitReactions
{
    public sealed class ModConfig
    {
        public bool Enabled { get; set; } = true;

        // --- Voice samples (MVP) ---------------------------------------------
        // When enabled, the mod reads a few of the NPC's REAL in-game dialogue lines
        // (base game and whatever a dialogue mod like Maggs' has replaced them with,
        // since those are what the game currently has loaded) and injects them into the
        // prompt purely as a VOICE/TONE reference, to make reactions sound more like that
        // specific character. The lines are labelled "reference only, do not copy", and the
        // NPC profile always stays the strongest authority above them.
        public bool UseVoiceSamples { get; set; } = true;

        // When enabled, the readable version of Fashion Sense internal IDs (outfit names,
        // accessory/hat change IDs) is sent to the AI as a theme/reference clue.
        // This helps when IDs are descriptive (e.g. "CuteFairyOutfit", "PikachuEars"),
        // but may confuse the AI when IDs are generic, numeric, or technical (e.g. "Set04").
        public bool UseFsInternalIdAsHint { get; set; } = true;

        // How many sample lines to inject (kept small so the prompt stays lean).
        public int VoiceSampleCount { get; set; } = 6;

        // Optional comma-separated list of NPC names to EXCLUDE from voice samples
        // (e.g. a heavily customized character you'd rather drive purely from its profile).
        // Example: "Sebastian, Abigail"
        public string VoiceSampleExcludedNpcs { get; set; } = "";
        // ---------------------------------------------------------------------


        // Built-in AI generation. Outfit Compliments currently uses AI-generated dialogue only.
        // Manual Content Patcher/JSON outfit dialogue fallback is disabled in this AI-only build.

        // Active provider. The provider-specific fields below stay saved separately,
        // so you can switch providers in GMCM without retyping keys/models/endpoints.
        public string AiProvider { get; set; } = "Gemini"; // OpenAI, DeepSeek, Gemini, OpenRouter, Local, Mistral, Groq, Together, Anthropic, xAI, Cerebras, Perplexity

        // Legacy fields kept for older config.json files. MigrateLegacyAiSettings() copies
        // them into the provider-specific fields when possible.
        public string AiModel { get; set; } = "";
        public string AiApiKey { get; set; } = "";
        public string AiCustomEndpoint { get; set; } = "";
        public int AiTemperaturePercent { get; set; } = 75;
        public int AiTimeoutSeconds { get; set; } = 60;
        public int AiMaxCharacters { get; set; } = 300;
        public int AiMinimumCharacters { get; set; } = 100;

        // Five reusable credential slots shown in GMCM. Each slot has its own provider, an
        // enabled checkbox, model, API key and (optional) endpoint. The mod uses the ENABLED
        // slot whose provider matches the active AiProvider. The old auto-guess (SlotMatches-
        // Provider) is kept only as a fallback when no enabled slot explicitly matches.
        public string AiModelSlot1 { get; set; } = "";
        public string AiModelSlot2 { get; set; } = "";
        public string AiModelSlot3 { get; set; } = "";
        public string AiModelSlot4 { get; set; } = "";
        public string AiModelSlot5 { get; set; } = "";
        public string AiApiKeySlot1 { get; set; } = "";
        public string AiApiKeySlot2 { get; set; } = "";
        public string AiApiKeySlot3 { get; set; } = "";
        public string AiApiKeySlot4 { get; set; } = "";
        public string AiApiKeySlot5 { get; set; } = "";
        public string AiCustomEndpointSlot1 { get; set; } = "";
        public string AiCustomEndpointSlot2 { get; set; } = "";
        public string AiCustomEndpointSlot3 { get; set; } = "";
        public string AiCustomEndpointSlot4 { get; set; } = "";
        public string AiCustomEndpointSlot5 { get; set; } = "";
        // Each slot's chosen provider (DeepSeek/Gemini/OpenAI/OpenRouter/Local/Mistral/Groq/Together/Anthropic/xAI/Cerebras/Perplexity).
        public string AiProviderSlot1 { get; set; } = "Gemini";
        public string AiProviderSlot2 { get; set; } = "OpenAI";
        public string AiProviderSlot3 { get; set; } = "OpenRouter";
        public string AiProviderSlot4 { get; set; } = "Groq";
        public string AiProviderSlot5 { get; set; } = "Mistral";
        // Whether each slot is enabled/available for use.
        public bool AiSlot1Enabled { get; set; } = true;
        public bool AiSlot2Enabled { get; set; } = false;
        public bool AiSlot3Enabled { get; set; } = false;
        public bool AiSlot4Enabled { get; set; } = false;
        public bool AiSlot5Enabled { get; set; } = false;
        // Vision mode per slot: "Auto" (detect from model name), "On" (force send image),
        // "Off" (never send image). Auto is the safe default.
        public string AiVisionModeSlot1 { get; set; } = "Auto";
        public string AiVisionModeSlot2 { get; set; } = "Auto";
        public string AiVisionModeSlot3 { get; set; } = "Auto";
        public string AiVisionModeSlot4 { get; set; } = "Auto";
        public string AiVisionModeSlot5 { get; set; } = "Auto";

        public string DeepSeekAiModel { get; set; } = "";
        public string DeepSeekAiApiKey { get; set; } = "";
        public string DeepSeekAiCustomEndpoint { get; set; } = "";
        public int DeepSeekAiTemperaturePercent { get; set; } = 75;
        public int DeepSeekAiTimeoutSeconds { get; set; } = 60;
        public int DeepSeekAiMaxCharacters { get; set; } = 1000;

        public string GeminiAiModel { get; set; } = "";
        public string GeminiAiApiKey { get; set; } = "";
        public string GeminiAiCustomEndpoint { get; set; } = "";
        public int GeminiAiTemperaturePercent { get; set; } = 75;
        public int GeminiAiTimeoutSeconds { get; set; } = 60;
        public int GeminiAiMaxCharacters { get; set; } = 1000;

        public string OpenAiAiModel { get; set; } = "";
        public string OpenAiAiApiKey { get; set; } = "";
        public string OpenAiAiCustomEndpoint { get; set; } = "";
        public int OpenAiAiTemperaturePercent { get; set; } = 75;
        public int OpenAiAiTimeoutSeconds { get; set; } = 60;
        public int OpenAiAiMaxCharacters { get; set; } = 1000;

        public string OpenRouterAiModel { get; set; } = "";
        public string OpenRouterAiApiKey { get; set; } = "";
        public string OpenRouterAiCustomEndpoint { get; set; } = "";
        public int OpenRouterAiTemperaturePercent { get; set; } = 75;
        public int OpenRouterAiTimeoutSeconds { get; set; } = 60;
        public int OpenRouterAiMaxCharacters { get; set; } = 1000;

        public string LocalAiModel { get; set; } = "";
        public string LocalAiApiKey { get; set; } = "";
        public string LocalAiCustomEndpoint { get; set; } = "";
        public int LocalAiTemperaturePercent { get; set; } = 75;
        public int LocalAiTimeoutSeconds { get; set; } = 60;
        public int LocalAiMaxCharacters { get; set; } = 1000;
        public bool LocalAiSafeMode { get; set; } = true;
        public bool LocalAiConservativePortraits { get; set; } = true;

        public string MistralAiModel { get; set; } = "";
        public string MistralAiApiKey { get; set; } = "";
        public string MistralAiCustomEndpoint { get; set; } = "";
        public int MistralAiTemperaturePercent { get; set; } = 75;
        public int MistralAiTimeoutSeconds { get; set; } = 60;
        public int MistralAiMaxCharacters { get; set; } = 1000;

        public string GroqAiModel { get; set; } = "";
        public string GroqAiApiKey { get; set; } = "";
        public string GroqAiCustomEndpoint { get; set; } = "";
        public int GroqAiTemperaturePercent { get; set; } = 75;
        public int GroqAiTimeoutSeconds { get; set; } = 60;
        public int GroqAiMaxCharacters { get; set; } = 1000;

        public string TogetherAiModel { get; set; } = "";
        public string TogetherAiApiKey { get; set; } = "";
        public string TogetherAiCustomEndpoint { get; set; } = "";
        public int TogetherAiTemperaturePercent { get; set; } = 75;
        public int TogetherAiTimeoutSeconds { get; set; } = 60;
        public int TogetherAiMaxCharacters { get; set; } = 1000;

        public string AnthropicAiModel { get; set; } = "";
        public string AnthropicAiApiKey { get; set; } = "";
        public string AnthropicAiCustomEndpoint { get; set; } = "";
        public int AnthropicAiTemperaturePercent { get; set; } = 75;
        public int AnthropicAiTimeoutSeconds { get; set; } = 60;
        public int AnthropicAiMaxCharacters { get; set; } = 1000;

        public string XAiModel { get; set; } = "";
        public string XAiApiKey { get; set; } = "";
        public string XAiCustomEndpoint { get; set; } = "";
        public int XAiTemperaturePercent { get; set; } = 75;
        public int XAiTimeoutSeconds { get; set; } = 60;
        public int XAiMaxCharacters { get; set; } = 1000;

        public string CerebrasAiModel { get; set; } = "";
        public string CerebrasAiApiKey { get; set; } = "";
        public string CerebrasAiCustomEndpoint { get; set; } = "";
        public int CerebrasAiTemperaturePercent { get; set; } = 75;
        public int CerebrasAiTimeoutSeconds { get; set; } = 60;
        public int CerebrasAiMaxCharacters { get; set; } = 1000;

        public string PerplexityAiModel { get; set; } = "";
        public string PerplexityAiApiKey { get; set; } = "";
        public string PerplexityAiCustomEndpoint { get; set; } = "";
        public int PerplexityAiTemperaturePercent { get; set; } = 75;
        public int PerplexityAiTimeoutSeconds { get; set; } = 60;
        public int PerplexityAiMaxCharacters { get; set; } = 1000;

        public bool UseAiCache { get; set; } = true;
        public bool EnableVisionOutfitAnalysis { get; set; } = false;
        public bool ShowOwnAiWaitingDialogue { get; set; } = true;
        public bool EnablePlayerReplyMenuAfterOutfitCompliment { get; set; } = true;
        public bool GenerateNpcFollowUpToPlayerOutfitReply { get; set; } = true;
        public bool EnableExpressiveAsteriskActions { get; set; } = true;

        public bool IncludeFestivalContextForAi { get; set; } = true;
        public bool IncludeFarmerBirthdayContextForAi { get; set; } = true;
        public string FarmerBirthdaySeason { get; set; } = "";
        public int FarmerBirthdayDay { get; set; } = 0;
        public bool IncludeDetailedLocationContextForAi { get; set; } = true;
        public bool IncludeDayPartContextForAi { get; set; } = true;
        public bool IncludeIndoorOutdoorContextForAi { get; set; } = true;
        public bool IncludeNpcRoomContextForAi { get; set; } = true;

        public bool UseJsonFallbackForOutfitReactions { get; set; } = false; // AI-only build: manual JSON fallback disabled


        // NPCs que não são cônjuges: reações leves via IA própria.
        public bool EnableNpcOutfitReactions { get; set; } = true;
        public int NpcOutfitReactionChance { get; set; } = 30;
        public int NpcRepeatedVisualNoticeChance { get; set; } = 15;
        public bool RomanticPartnersAlwaysNoticeOutfitChanges { get; set; } = true;
        public bool EnableDebugLogging { get; set; } = false;

        // How NPCs react when the farmer is wearing a vanilla hat (Game1.player.hat):
        //  - "Combined": react to the current outfit/look AND the vanilla hat together (default, original behavior).
        //  - "HatOnly": react EXCLUSIVELY to the vanilla hat, ignoring the rest of the outfit/clothes context.
        //    This can produce funnier, more focused reactions to just the hat.
        public string VanillaHatReactionMode { get; set; } = "Combined";

        // How NPCs react when the farmer is wearing a special vanilla item (e.g. Mayor's Purple Shorts):
        //  - "Combined": react to the current outfit/look AND the special item together.
        //    The NPC can comment on both the item and how it clashes or fits with the outfit.
        //  - "ItemOnly": react EXCLUSIVELY to the special item, ignoring the rest of the outfit.
        //    This produces more focused, item-centric reactions.
        public string VanillaSpecialItemReactionMode { get; set; } = "ItemOnly";

        // Unified notice/cancel distances — apply to both the spouse and all other NPCs.
        public int OutfitNoticeDistance { get; set; } = 650;
        public int OutfitCancelDistance { get; set; } = 900;

        public void MigrateLegacyAiSettings()
        {
            string provider = NormalizeProvider(AiProvider);
            AiProvider = provider;

            // Copy the old single-provider fields into the matching new provider section.
            // This avoids making you retype the current key/model after updating the mod.
            if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(DeepSeekAiModel) && !string.IsNullOrWhiteSpace(AiModel))
                    DeepSeekAiModel = AiModel;
                if (string.IsNullOrWhiteSpace(DeepSeekAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
                    DeepSeekAiApiKey = AiApiKey;
                if (string.IsNullOrWhiteSpace(DeepSeekAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
                    DeepSeekAiCustomEndpoint = AiCustomEndpoint;
                if (DeepSeekAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
                    DeepSeekAiTemperaturePercent = AiTemperaturePercent;
                if (DeepSeekAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
                    DeepSeekAiTimeoutSeconds = AiTimeoutSeconds;
                if (DeepSeekAiMaxCharacters == 200 && AiMaxCharacters != 200)
                    DeepSeekAiMaxCharacters = AiMaxCharacters;
            }
            else if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(GeminiAiModel) && !string.IsNullOrWhiteSpace(AiModel))
                    GeminiAiModel = AiModel;
                if (string.IsNullOrWhiteSpace(GeminiAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
                    GeminiAiApiKey = AiApiKey;
                if (string.IsNullOrWhiteSpace(GeminiAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
                    GeminiAiCustomEndpoint = AiCustomEndpoint;
                if (GeminiAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
                    GeminiAiTemperaturePercent = AiTemperaturePercent;
                if (GeminiAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
                    GeminiAiTimeoutSeconds = AiTimeoutSeconds;
                if (GeminiAiMaxCharacters == 200 && AiMaxCharacters != 200)
                    GeminiAiMaxCharacters = AiMaxCharacters;
            }
            else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(OpenAiAiModel) && !string.IsNullOrWhiteSpace(AiModel))
                    OpenAiAiModel = AiModel;
                if (string.IsNullOrWhiteSpace(OpenAiAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
                    OpenAiAiApiKey = AiApiKey;
                if (string.IsNullOrWhiteSpace(OpenAiAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
                    OpenAiAiCustomEndpoint = AiCustomEndpoint;
                if (OpenAiAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
                    OpenAiAiTemperaturePercent = AiTemperaturePercent;
                if (OpenAiAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
                    OpenAiAiTimeoutSeconds = AiTimeoutSeconds;
                if (OpenAiAiMaxCharacters == 200 && AiMaxCharacters != 200)
                    OpenAiAiMaxCharacters = AiMaxCharacters;
            }
            else if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(OpenRouterAiModel) && !string.IsNullOrWhiteSpace(AiModel))
                    OpenRouterAiModel = AiModel;
                if (string.IsNullOrWhiteSpace(OpenRouterAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
                    OpenRouterAiApiKey = AiApiKey;
                if (string.IsNullOrWhiteSpace(OpenRouterAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
                    OpenRouterAiCustomEndpoint = AiCustomEndpoint;
                if (OpenRouterAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
                    OpenRouterAiTemperaturePercent = AiTemperaturePercent;
                if (OpenRouterAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
                    OpenRouterAiTimeoutSeconds = AiTimeoutSeconds;
                if (OpenRouterAiMaxCharacters == 200 && AiMaxCharacters != 200)
                    OpenRouterAiMaxCharacters = AiMaxCharacters;
            }
            else if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(LocalAiModel) && !string.IsNullOrWhiteSpace(AiModel))
                    LocalAiModel = AiModel;
                if (string.IsNullOrWhiteSpace(LocalAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
                    LocalAiApiKey = AiApiKey;
                if (string.IsNullOrWhiteSpace(LocalAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
                    LocalAiCustomEndpoint = AiCustomEndpoint;
                if (LocalAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
                    LocalAiTemperaturePercent = AiTemperaturePercent;
                if (LocalAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
                    LocalAiTimeoutSeconds = AiTimeoutSeconds;
                if (LocalAiMaxCharacters == 200 && AiMaxCharacters != 200)
                    LocalAiMaxCharacters = AiMaxCharacters;
            }


            ApplyAiDefaultsAndLimits();
        }

        public void ApplyAiDefaultsAndLimits()
        {
            AiProvider = NormalizeProvider(AiProvider);

            // These options are no longer exposed in GMCM. Keep their runtime behavior fixed.
            // Manual outfit dialogue JSON fallback stays disabled so the mod focuses on AI-generated lines.
            UseJsonFallbackForOutfitReactions = false;
            EnableNpcOutfitReactions = true;

            // Default AI models and endpoints are intentionally NOT set here.
            // The user must configure their own model (and endpoint, where applicable)
            // for whichever provider they choose. This keeps the mod from shipping any
            // built-in model/endpoint defaults. Left commented for reference only:
            //
            // if (string.IsNullOrWhiteSpace(DeepSeekAiModel))
            //     DeepSeekAiModel = "deepseek-v4-flash";
            // if (string.IsNullOrWhiteSpace(GeminiAiModel))
            //     GeminiAiModel = "gemini-2.5-flash";
            // if (string.IsNullOrWhiteSpace(OpenAiAiModel))
            //     OpenAiAiModel = "gpt-4.1-mini";
            // if (string.IsNullOrWhiteSpace(OpenRouterAiModel))
            //     OpenRouterAiModel = "openrouter/free";
            // if (string.IsNullOrWhiteSpace(OpenRouterAiCustomEndpoint))
            //     OpenRouterAiCustomEndpoint = "https://openrouter.ai/api/v1";
            // if (string.IsNullOrWhiteSpace(LocalAiModel))
            //     LocalAiModel = "llama3.2";
            // if (string.IsNullOrWhiteSpace(LocalAiCustomEndpoint))
            //     LocalAiCustomEndpoint = "http://localhost:11434/v1";
            // if (string.IsNullOrWhiteSpace(MistralAiModel))
            //     MistralAiModel = "mistral-small-latest";
            // if (string.IsNullOrWhiteSpace(GroqAiModel))
            //     GroqAiModel = "llama-3.3-70b-versatile";
            // if (string.IsNullOrWhiteSpace(TogetherAiModel))
            //     TogetherAiModel = "meta-llama/Llama-3.3-70B-Instruct-Turbo";
            // if (string.IsNullOrWhiteSpace(AnthropicAiModel))
            //     AnthropicAiModel = "claude-haiku-4-5-20251001";
            // if (string.IsNullOrWhiteSpace(XAiModel))
            //     XAiModel = "grok-3-mini";
            // if (string.IsNullOrWhiteSpace(CerebrasAiModel))
            //     CerebrasAiModel = "llama-3.3-70b";
            // if (string.IsNullOrWhiteSpace(PerplexityAiModel))
            //     PerplexityAiModel = "sonar";

            BackfillAiCredentialSlots();

            DeepSeekAiTemperaturePercent = Clamp(DeepSeekAiTemperaturePercent, 0, 200);
            GeminiAiTemperaturePercent = Clamp(GeminiAiTemperaturePercent, 0, 200);
            OpenAiAiTemperaturePercent = Clamp(OpenAiAiTemperaturePercent, 0, 200);
            OpenRouterAiTemperaturePercent = Clamp(OpenRouterAiTemperaturePercent, 0, 200);
            LocalAiTemperaturePercent = Clamp(LocalAiTemperaturePercent, 0, 200);
            MistralAiTemperaturePercent = Clamp(MistralAiTemperaturePercent, 0, 200);
            GroqAiTemperaturePercent = Clamp(GroqAiTemperaturePercent, 0, 200);
            TogetherAiTemperaturePercent = Clamp(TogetherAiTemperaturePercent, 0, 200);
            AnthropicAiTemperaturePercent = Clamp(AnthropicAiTemperaturePercent, 0, 200);
            XAiTemperaturePercent = Clamp(XAiTemperaturePercent, 0, 200);
            CerebrasAiTemperaturePercent = Clamp(CerebrasAiTemperaturePercent, 0, 200);
            PerplexityAiTemperaturePercent = Clamp(PerplexityAiTemperaturePercent, 0, 200);

            DeepSeekAiTimeoutSeconds = Clamp(DeepSeekAiTimeoutSeconds, 3, 120);
            GeminiAiTimeoutSeconds = Clamp(GeminiAiTimeoutSeconds, 3, 120);
            OpenAiAiTimeoutSeconds = Clamp(OpenAiAiTimeoutSeconds, 3, 120);
            OpenRouterAiTimeoutSeconds = Clamp(OpenRouterAiTimeoutSeconds, 3, 120);
            LocalAiTimeoutSeconds = Clamp(LocalAiTimeoutSeconds, 3, 120);
            MistralAiTimeoutSeconds = Clamp(MistralAiTimeoutSeconds, 3, 120);
            GroqAiTimeoutSeconds = Clamp(GroqAiTimeoutSeconds, 3, 120);
            TogetherAiTimeoutSeconds = Clamp(TogetherAiTimeoutSeconds, 3, 120);
            AnthropicAiTimeoutSeconds = Clamp(AnthropicAiTimeoutSeconds, 3, 120);
            XAiTimeoutSeconds = Clamp(XAiTimeoutSeconds, 3, 120);
            CerebrasAiTimeoutSeconds = Clamp(CerebrasAiTimeoutSeconds, 3, 120);
            PerplexityAiTimeoutSeconds = Clamp(PerplexityAiTimeoutSeconds, 3, 120);

            DeepSeekAiMaxCharacters = Clamp(DeepSeekAiMaxCharacters, 80, 2000);
            GeminiAiMaxCharacters = Clamp(GeminiAiMaxCharacters, 80, 2000);
            OpenAiAiMaxCharacters = Clamp(OpenAiAiMaxCharacters, 80, 2000);
            OpenRouterAiMaxCharacters = Clamp(OpenRouterAiMaxCharacters, 80, 2000);
            LocalAiMaxCharacters = Clamp(LocalAiMaxCharacters, 80, 2000);
            MistralAiMaxCharacters = Clamp(MistralAiMaxCharacters, 80, 2000);
            GroqAiMaxCharacters = Clamp(GroqAiMaxCharacters, 80, 2000);
            TogetherAiMaxCharacters = Clamp(TogetherAiMaxCharacters, 80, 2000);
            AnthropicAiMaxCharacters = Clamp(AnthropicAiMaxCharacters, 80, 2000);
            XAiMaxCharacters = Clamp(XAiMaxCharacters, 80, 2000);
            CerebrasAiMaxCharacters = Clamp(CerebrasAiMaxCharacters, 80, 2000);
            PerplexityAiMaxCharacters = Clamp(PerplexityAiMaxCharacters, 80, 2000);

            AiMaxCharacters = Clamp(AiMaxCharacters, 80, 2000);
            // Let the player choose the minimum freely. Runtime prompt/validation will use
            // the closest possible target if the chosen minimum is higher than the active max.
            AiMinimumCharacters = Clamp(AiMinimumCharacters, 0, 2000);
            AiTimeoutSeconds = Clamp(AiTimeoutSeconds, 3, 120);
            AiTemperaturePercent = Clamp(AiTemperaturePercent, 0, 200);
            NpcOutfitReactionChance = Clamp(NpcOutfitReactionChance, 0, 100);
            OutfitNoticeDistance = Clamp(OutfitNoticeDistance, 64, 3000);
            OutfitCancelDistance = Clamp(OutfitCancelDistance, 64, 5000);

            FarmerBirthdayDay = Clamp(FarmerBirthdayDay, 0, 28);
            FarmerBirthdaySeason = NormalizeSeason(FarmerBirthdaySeason);
        }


        private void BackfillAiCredentialSlots()
        {
            if (!string.IsNullOrWhiteSpace(AiModelSlot1) || !string.IsNullOrWhiteSpace(AiApiKeySlot1) || !string.IsNullOrWhiteSpace(AiCustomEndpointSlot1)
                || !string.IsNullOrWhiteSpace(AiModelSlot2) || !string.IsNullOrWhiteSpace(AiApiKeySlot2) || !string.IsNullOrWhiteSpace(AiCustomEndpointSlot2)
                || !string.IsNullOrWhiteSpace(AiModelSlot3) || !string.IsNullOrWhiteSpace(AiApiKeySlot3) || !string.IsNullOrWhiteSpace(AiCustomEndpointSlot3))
            {
                return;
            }

            string provider = NormalizeProvider(AiProvider);
            AiModelSlot1 = GetProviderFallbackModel(provider);
            AiApiKeySlot1 = GetProviderFallbackApiKey(provider);
            AiCustomEndpointSlot1 = GetProviderFallbackEndpoint(provider);
        }

        public string GetResolvedAiModelForProvider(string provider)
        {
            provider = NormalizeProvider(provider);
            int slot = GetAiCredentialSlotForProvider(provider);
            string value = slot > 0 ? GetSlotModel(slot) : "";
            return !string.IsNullOrWhiteSpace(value) ? value : GetProviderFallbackModel(provider);
        }

        public string GetResolvedAiApiKeyForProvider(string provider)
        {
            provider = NormalizeProvider(provider);
            int slot = GetAiCredentialSlotForProvider(provider);
            string value = slot > 0 ? GetSlotApiKey(slot) : "";
            return !string.IsNullOrWhiteSpace(value) ? value : GetProviderFallbackApiKey(provider);
        }

        public string GetResolvedAiEndpointForProvider(string provider)
        {
            provider = NormalizeProvider(provider);
            int slot = GetAiCredentialSlotForProvider(provider);
            string value = slot > 0 ? GetSlotEndpoint(slot) : "";
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            // Gemini uses its native format with a per-model URL built elsewhere; keep it blank
            // unless the user typed a custom endpoint, so we never force an incompatible URL.
            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                return "";

            // Slot endpoint left blank (automatic): use the provider's default endpoint.
            string fallback = GetProviderFallbackEndpoint(provider);
            return !string.IsNullOrWhiteSpace(fallback) ? fallback : GetDefaultEndpointForProvider(provider);
        }

        public int GetAiCredentialSlotForProvider(string provider)
        {
            provider = NormalizeProvider(provider);

            // 1. PRIMARY: an enabled slot whose chosen provider matches (checkbox wins).
            for (int slot = 1; slot <= 5; slot++)
            {
                if (IsSlotEnabled(slot)
                    && NormalizeProvider(GetSlotProvider(slot)).Equals(provider, StringComparison.OrdinalIgnoreCase)
                    && (!string.IsNullOrWhiteSpace(GetSlotModel(slot)) || !string.IsNullOrWhiteSpace(GetSlotApiKey(slot)) || !string.IsNullOrWhiteSpace(GetSlotEndpoint(slot))))
                    return slot;
            }

            // 2. FALLBACK: the old auto-guess based on model/endpoint hints.
            for (int slot = 1; slot <= 5; slot++)
            {
                if (SlotMatchesProvider(slot, provider))
                    return slot;
            }

            return 0;
        }

        // The active provider is the provider of the enabled profile.
        // The AI service refuses to generate if more than one profile is enabled,
        // so this method only falls back to the first enabled profile for compatibility.
        public string GetActiveProvider()
        {
            for (int slot = 1; slot <= 5; slot++)
            {
                if (IsSlotEnabled(slot))
                    return NormalizeProvider(GetSlotProvider(slot));
            }

            return NormalizeProvider(GetSlotProvider(1));
        }

        private string GetSlotVisionMode(int slot)
        {
            string mode = slot switch
            {
                1 => AiVisionModeSlot1,
                2 => AiVisionModeSlot2,
                3 => AiVisionModeSlot3,
                4 => AiVisionModeSlot4,
                5 => AiVisionModeSlot5,
                _ => "Auto"
            };
            if (string.IsNullOrWhiteSpace(mode)) return "Auto";
            if (mode.Equals("On", StringComparison.OrdinalIgnoreCase)) return "On";
            if (mode.Equals("Off", StringComparison.OrdinalIgnoreCase)) return "Off";
            return "Auto";
        }

        // Detects whether a model is multimodal (accepts images) from its NAME. This catches the
        // common cases where the model advertises vision in its name; it can't know every model,
        // which is why the per-profile vision mode can force it On/Off.
        public static bool ModelNameLooksMultimodal(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return false;
            string m = model.ToLowerInvariant();
            string[] hints =
            {
                "vl", "vision", "multimodal", "-mm", "llava", "pixtral",
                "gpt-4o", "4o-", "gpt-5", "gemini",
                "claude-3", "claude-4", "claude-haiku", "claude-opus", "claude-sonnet",
                "qwen2-vl", "qwen2.5-vl", "qwen3-vl", "qwen-vl", "qwen-omni", "qwen3-omni",
                "qwen3.7", "qwen3-7", "qwen/qwen3.7", "qwen/qwen3-7",
                "llama-3.2", "llama3.2", "internvl",
                "molmo", "phi-3.5-vision", "phi-4-multimodal", "maverick", "scout"
            };
            foreach (string h in hints)
                if (m.Contains(h)) return true;
            return false;
        }

        // Final decision: should the rendered farmer image be sent to the active profile's model?
        public bool ShouldSendImageToActiveModel()
        {
            int slot = 0;
            string provider = GetActiveProvider();
            for (int s = 1; s <= 5; s++)
            {
                if (IsSlotEnabled(s)) { slot = s; break; }
            }
            if (slot == 0) slot = 1;

            string mode = GetSlotVisionMode(slot);
            if (mode.Equals("On", StringComparison.OrdinalIgnoreCase)) return true;
            if (mode.Equals("Off", StringComparison.OrdinalIgnoreCase)) return false;

            // Auto: decide from the model name.
            string model = GetResolvedAiModelForProvider(provider);
            return ModelNameLooksMultimodal(model);
        }

        private bool SlotMatchesProvider(int slot, string provider)
        {
            string model = (GetSlotModel(slot) ?? "").Trim().ToLowerInvariant();
            string endpoint = (GetSlotEndpoint(slot) ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(model) && string.IsNullOrWhiteSpace(endpoint))
                return false;

            provider = NormalizeProvider(provider);

            if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
                return endpoint.StartsWith("http://localhost") || endpoint.StartsWith("http://127.0.0.1") || endpoint.Contains("localhost:") || endpoint.Contains("127.0.0.1:");

            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                return endpoint.Contains("openrouter.ai") || model.Equals("openrouter/free") || model.Contains(":free") || (model.Contains("/") && !endpoint.Contains("deepseek.com") && !endpoint.Contains("googleapis.com") && !endpoint.Contains("openai.com"));

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                return endpoint.Contains("generativelanguage") || endpoint.Contains("googleapis.com") || model.Contains("gemini");

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                return endpoint.Contains("api.openai.com") || model.StartsWith("gpt-") || model.StartsWith("o1") || model.StartsWith("o3") || model.StartsWith("o4");

            return endpoint.Contains("deepseek.com") || (model.StartsWith("deepseek") && !endpoint.Contains("openrouter.ai"));
        }

        private string GetSlotModel(int slot)
        {
            return slot switch
            {
                1 => AiModelSlot1,
                2 => AiModelSlot2,
                3 => AiModelSlot3,
                4 => AiModelSlot4,
                5 => AiModelSlot5,
                _ => ""
            };
        }

        private string GetSlotApiKey(int slot)
        {
            return slot switch
            {
                1 => AiApiKeySlot1,
                2 => AiApiKeySlot2,
                3 => AiApiKeySlot3,
                4 => AiApiKeySlot4,
                5 => AiApiKeySlot5,
                _ => ""
            };
        }

        private string GetSlotEndpoint(int slot)
        {
            return slot switch
            {
                1 => AiCustomEndpointSlot1,
                2 => AiCustomEndpointSlot2,
                3 => AiCustomEndpointSlot3,
                4 => AiCustomEndpointSlot4,
                5 => AiCustomEndpointSlot5,
                _ => ""
            };
        }

        public int CountEnabledAiProfiles()
        {
            int count = 0;
            for (int slot = 1; slot <= 5; slot++)
            {
                if (IsSlotEnabled(slot))
                    count++;
            }

            return count;
        }

        public bool HasMultipleEnabledAiProfiles()
        {
            return CountEnabledAiProfiles() > 1;
        }

        private string GetSlotProvider(int slot)
        {
            return slot switch
            {
                1 => AiProviderSlot1,
                2 => AiProviderSlot2,
                3 => AiProviderSlot3,
                4 => AiProviderSlot4,
                5 => AiProviderSlot5,
                _ => ""
            };
        }

        private bool IsSlotEnabled(int slot)
        {
            return slot switch
            {
                1 => AiSlot1Enabled,
                2 => AiSlot2Enabled,
                3 => AiSlot3Enabled,
                4 => AiSlot4Enabled,
                5 => AiSlot5Enabled,
                _ => false
            };
        }

        private string GetProviderFallbackModel(string provider)
        {
            provider = NormalizeProvider(provider);
            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                return GeminiAiModel;
            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                return OpenAiAiModel;
            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                return OpenRouterAiModel;
            if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
                return MistralAiModel;
            if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
                return GroqAiModel;
            if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase))
                return TogetherAiModel;
            if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
                return LocalAiModel;
            if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
                return AnthropicAiModel;
            if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
                return XAiModel;
            if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
                return CerebrasAiModel;
            if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
                return PerplexityAiModel;
            return DeepSeekAiModel;
        }

        private string GetProviderFallbackApiKey(string provider)
        {
            provider = NormalizeProvider(provider);
            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                return GeminiAiApiKey;
            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                return OpenAiAiApiKey;
            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                return OpenRouterAiApiKey;
            if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
                return MistralAiApiKey;
            if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
                return GroqAiApiKey;
            if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase))
                return TogetherAiApiKey;
            if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
                return LocalAiApiKey;
            if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
                return AnthropicAiApiKey;
            if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
                return XAiApiKey;
            if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
                return CerebrasAiApiKey;
            if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
                return PerplexityAiApiKey;
            return DeepSeekAiApiKey;
        }

        private string GetProviderFallbackEndpoint(string provider)
        {
            provider = NormalizeProvider(provider);
            string custom;
            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)) custom = GeminiAiCustomEndpoint;
            else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)) custom = OpenAiAiCustomEndpoint;
            else if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase)) custom = OpenRouterAiCustomEndpoint;
            else if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase)) custom = MistralAiCustomEndpoint;
            else if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase)) custom = GroqAiCustomEndpoint;
            else if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase)) custom = TogetherAiCustomEndpoint;
            else if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase)) custom = LocalAiCustomEndpoint;
            else if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase)) custom = AnthropicAiCustomEndpoint;
            else if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase)) custom = XAiCustomEndpoint;
            else if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase)) custom = CerebrasAiCustomEndpoint;
            else if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase)) custom = PerplexityAiCustomEndpoint;
            else custom = DeepSeekAiCustomEndpoint;

            // Fall back to the provider's default endpoint when no custom one is set.
            return !string.IsNullOrWhiteSpace(custom) ? custom : GetDefaultEndpointForProvider(provider);
        }

        private static string NormalizeProvider(string provider)
        {
            if (provider != null && provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                return "OpenAI";
            if (provider != null && provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                return "Gemini";
            if (provider != null && provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                return "OpenRouter";
            if (provider != null && provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
                return "Mistral";
            if (provider != null && provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
                return "Groq";
            if (provider != null && (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Together AI", StringComparison.OrdinalIgnoreCase)))
                return "Together";
            if (provider != null && (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)))
                return "Local";
            if (provider != null && (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) || provider.Equals("Claude", StringComparison.OrdinalIgnoreCase)))
                return "Anthropic";
            if (provider != null && (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Grok", StringComparison.OrdinalIgnoreCase) || provider.Equals("x.ai", StringComparison.OrdinalIgnoreCase)))
                return "xAI";
            if (provider != null && provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
                return "Cerebras";
            if (provider != null && provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
                return "Perplexity";
            if (provider != null && provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
                return "DeepSeek";
            return "Gemini";
        }

        // Default API endpoint for each provider (used when the slot/provider endpoint is blank).
        // All three new providers use the OpenAI-compatible /chat/completions shape.
        public static string GetDefaultEndpointForProvider(string provider)
        {
            switch (NormalizeProvider(provider))
            {
                // Gemini uses its NATIVE format (contents/system_instruction), so the endpoint is
                // built per-model as .../models/{model}:generateContent by GenerateGeminiAsync when
                // left blank. Returning blank here avoids forcing the OpenAI-compatible URL.
                case "Gemini": return "";
                case "OpenAI": return "https://api.openai.com/v1/chat/completions";
                case "OpenRouter": return "https://openrouter.ai/api/v1/chat/completions";
                case "Mistral": return "https://api.mistral.ai/v1/chat/completions";
                case "Groq": return "https://api.groq.com/openai/v1/chat/completions";
                case "Together": return "https://api.together.xyz/v1/chat/completions";
                case "Local": return "http://localhost:1234/v1/chat/completions";
                case "Anthropic": return "https://api.anthropic.com/v1/messages";
                case "xAI": return "https://api.x.ai/v1/chat/completions";
                case "Cerebras": return "https://api.cerebras.ai/v1/chat/completions";
                case "Perplexity": return "https://api.perplexity.ai/chat/completions";
                default: return "https://api.deepseek.com/v1/chat/completions";
            }
        }

        private static string NormalizeSeason(string season)
        {
            if (string.IsNullOrWhiteSpace(season))
                return "";

            string value = season.Trim().ToLowerInvariant();
            return value switch
            {
                "spring" or "primavera" => "spring",
                "summer" or "verao" or "verão" => "summer",
                "fall" or "autumn" or "outono" => "fall",
                "winter" or "inverno" => "winter",
                _ => value
            };
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}
