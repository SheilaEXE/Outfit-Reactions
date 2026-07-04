using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OutfitReactions.Ai
{
    /// <summary>
    /// Owns all direct communication with AI providers: building the HTTP request for each
    /// provider family (OpenAI-compatible, Anthropic, Gemini), sending it, handling the
    /// vision-fallback retry, and extracting the raw text from the response. The caller passes
    /// resolved ActiveAiSettings plus a minimum visible-length target; this class is otherwise
    /// self-contained.
    /// </summary>
    internal sealed class AiProviderClient
    {
        private readonly IMonitor monitor;
        private static readonly HttpClient Http = new();

        public AiProviderClient(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public async Task<string> GenerateRawAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, OutfitVisionImage visionImage = null)
        {
            string provider = (ai.Provider ?? "DeepSeek").Trim();
            using CancellationTokenSourceCompat timeout = new(TimeSpan.FromSeconds(Math.Clamp(ai.TimeoutSeconds, 3, 120)));

            try
            {
                if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                    return await GenerateGeminiAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);

                if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
                    return await GenerateAnthropicAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);

                return await GenerateOpenAiCompatibleAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);
            }
            catch (InvalidOperationException ex) when (ShouldRetryWithoutVision(ex, ai, visionImage))
            {
                monitor.Log(" Selected AI endpoint rejected image input. Retrying once without the attached image and using only text/confirmed visual clues.", LogLevel.Warn);

                string textOnlyPrompt = prompt
                    + "\n\nIMPORTANT VISION FALLBACK: The selected model/endpoint rejected image input, so no image is attached in this retry. "
                    + "Use only textual support data, saved outfit/theme clues, and confirmed color clues. "
                    + "Do not mention seeing an image, screenshot, PNG, pixels, or attachment. "
                    + "Do not invent exact visual details, scene objects, props, or current actions that are not explicitly stated.";

                if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                    return await GenerateGeminiAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token, null);

                if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
                    return await GenerateAnthropicAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token, null);

                return await GenerateOpenAiCompatibleAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token, null);
            }
        }

        private static string NormalizeEndpoint(ActiveAiSettings ai)
        {
            string provider = ai.Provider ?? "DeepSeek";
            string custom = (ai.Endpoint ?? "").Trim().TrimEnd('/');

            if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(custom))
                    return "https://api.deepseek.com/chat/completions";

                if (custom.EndsWith("/v1/chat/completions", StringComparison.OrdinalIgnoreCase)
                    && custom.StartsWith("https://api.deepseek.com", StringComparison.OrdinalIgnoreCase))
                    return "https://api.deepseek.com/chat/completions";

                if (custom.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                    return custom;

                if (custom.Equals("https://api.deepseek.com/v1", StringComparison.OrdinalIgnoreCase))
                    return "https://api.deepseek.com/chat/completions";

                if (custom.Equals("https://api.deepseek.com", StringComparison.OrdinalIgnoreCase))
                    return custom + "/chat/completions";

                return custom + "/chat/completions";
            }

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(custom))
                    return "https://api.openai.com/v1/responses";

                if (custom.EndsWith("/responses", StringComparison.OrdinalIgnoreCase)
                    || custom.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                    return custom;

                if (custom.Equals("https://api.openai.com", StringComparison.OrdinalIgnoreCase))
                    return custom + "/v1/responses";

                if (custom.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                    return custom + "/responses";

                return custom + "/responses";
            }

            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(custom))
                    return "https://openrouter.ai/api/v1/chat/completions";

                if (custom.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                    return custom;

                if (custom.Equals("https://openrouter.ai", StringComparison.OrdinalIgnoreCase))
                    return custom + "/api/v1/chat/completions";

                if (custom.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase) || custom.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                    return custom + "/chat/completions";

                return custom + "/chat/completions";
            }

            if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(custom))
                    return "http://localhost:11434/v1/chat/completions";

                if (custom.EndsWith("/v1/chat/completions", StringComparison.OrdinalIgnoreCase))
                    return custom;

                if (custom.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                {
                    string baseUrl = custom.Substring(0, custom.Length - "/chat/completions".Length).TrimEnd('/');
                    if (baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                        return custom;

                    // LM Studio and Ollama use the OpenAI-compatible /v1/chat/completions path.
                    return baseUrl + "/v1/chat/completions";
                }

                if (custom.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                    return custom + "/chat/completions";

                return custom + "/v1/chat/completions";
            }

            if (string.IsNullOrWhiteSpace(custom))
                return "https://api.openai.com/v1/responses";

            if (custom.EndsWith("/responses", StringComparison.OrdinalIgnoreCase)
                || custom.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                return custom;

            return custom + "/chat/completions";
        }

        private async Task<string> GenerateOpenAiCompatibleAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            string provider = ai.Provider ?? "DeepSeek";
            string endpoint = NormalizeEndpoint(ai);
            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint}", LogLevel.Info);

            bool useResponsesApi = endpoint.IndexOf("/responses", StringComparison.OrdinalIgnoreCase) >= 0;
            // Quality-friendly output budget for in-game dialogue.
            // This is NOT the final dialogue length; MaxCharacters still controls the visible text.
            // We give enough space for nuance, tone, JSON formatting, and moderate model reasoning,
            // without going back to the old 4000+ headroom that made Pro/Thinking models crawl.
            int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
            string modelLower = (ai.Model ?? "").ToLowerInvariant();
            bool looksLikeReasoningModel =
                modelLower.Contains("reasoner") || modelLower.Contains("reasoning") ||
                modelLower.Contains("-pro") || modelLower.Contains("pro-") || modelLower.EndsWith("pro") ||
                modelLower.Contains("r1") || modelLower.Contains("thinking") ||
                modelLower.Contains("o1") || modelLower.Contains("o3") || modelLower.Contains("o4");

            int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, looksLikeReasoningModel);
            monitor.Log($" AI output budget: {maxTokens} tokens for {ai.Provider}/{ai.Model} (visible target {visibleTarget}, reasoning-like={looksLikeReasoningModel}).", LogLevel.Trace);
            string requestJson;
            if (useResponsesApi)
            {
                object input = prompt;
                if (ShouldAttachVision(ai, visionImage))
                {
                    List<object> contentParts = new()
                    {
                        new { type = "input_text", text = prompt },
                        new { type = "input_image", image_url = visionImage.ToDataUri() }
                    };
                    if (visionImage.HasBackImage)
                        contentParts.Add(new { type = "input_image", image_url = visionImage.ToBackDataUri() });

                    input = new object[]
                    {
                        new
                        {
                            role = "user",
                            content = contentParts.ToArray()
                        }
                    };
                }

                requestJson = JsonSerializer.Serialize(new
                {
                    model = ai.Model,
                    input,
                    temperature = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
                    max_output_tokens = maxTokens
                });
            }
            else
            {
                string systemMessage = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. Return portraits as one key per dialogue box, starting at box 1; the array may have 1, 2, 3, or more keys depending on the number of #$b# boxes. No markdown. No explanation. No narration. No analysis.";

                double temperature = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0;
                if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
                    temperature = Math.Min(temperature, 0.25);

                object userContent = prompt;
                if (ShouldAttachVision(ai, visionImage))
                {
                    List<object> userParts = new()
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = visionImage.ToDataUri() } }
                    };
                    if (visionImage.HasBackImage)
                        userParts.Add(new { type = "image_url", image_url = new { url = visionImage.ToBackDataUri() } });
                    userContent = userParts.ToArray();
                }

                object[] messages = new object[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userContent }
                };

                Dictionary<string, object> chatBody = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["model"] = ai.Model,
                    ["messages"] = messages,
                    ["temperature"] = temperature,
                    ["max_tokens"] = maxTokens,
                    ["stream"] = false
                };

                // Disable model "thinking"/reasoning where supported, to keep reactions fast and
                // cheap (a quick outfit comment never needs chain-of-thought). There's no universal
                // switch, and some strict APIs reject unknown body fields with a 400 — so we apply
                // each opt-out ONLY to the provider that understands it, instead of spraying all of
                // them at everyone:
                //  - DeepSeek official (v4-flash/v4-pro default to thinking ON): thinking={type:"disabled"}.
                //  - Qwen (DashScope official): enable_thinking=false (top-level).
                //  - Self-hosted OpenAI-compatible (vLLM/SGLang) + many gateways: nested
                //    chat_template_kwargs.enable_thinking=false (safe; passed to the chat template).
                // Mistral standard models have no thinking mode; OpenAI/Anthropic are handled in their
                // own methods. Thinking-only models (deepseek-reasoner, *-thinking, o1/o3) can't be
                // turned off by any field — that's a model choice, not a parameter.
                bool isDeepSeek = provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase);
                bool isQwenOfficial = provider.IndexOf("Qwen", StringComparison.OrdinalIgnoreCase) >= 0
                    || provider.IndexOf("DashScope", StringComparison.OrdinalIgnoreCase) >= 0
                    || provider.IndexOf("Alibaba", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isGenericCompatible = provider.Equals("Local", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("Groq", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("Together", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase);
                bool isOpenRouter = provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase);

                if (isDeepSeek)
                    chatBody["thinking"] = new Dictionary<string, object> { ["type"] = "disabled" };
                if (isQwenOfficial)
                    chatBody["enable_thinking"] = false;
                if (isGenericCompatible)
                    chatBody["chat_template_kwargs"] = new Dictionary<string, object> { ["enable_thinking"] = false };
                // OpenRouter has its OWN reasoning control and ignores chat_template_kwargs. Its
                // documented disable signal is reasoning={enabled:false} (it maps this to whatever
                // the underlying model needs). On routes whose upstream forces reasoning on (e.g.
                // o3, grok-3-mini), OpenRouter silently ignores this rather than erroring for the
                // {enabled:false} form, so it's safe to always send for OpenRouter.
                if (isOpenRouter)
                    chatBody["reasoning"] = new Dictionary<string, object> { ["enabled"] = false };

                // DeepSeek supports JSON mode. Each model manages its own reasoning, so we no
                // longer send an explicit thinking switch.
                if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
                {
                    chatBody["response_format"] = new { type = "json_object" };
                }
                // Local/OpenAI-compatible servers are intentionally left in plain text mode.
                // Many local models follow a simple dash-line-style "- dialogue" line more reliably than JSON mode.

                requestJson = JsonSerializer.Serialize(chatBody);
            }

            using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            if (!string.IsNullOrWhiteSpace(ai.ApiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ai.ApiKey.Trim());

            if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://www.nexusmods.com/stardewvalley/mods/");
                request.Headers.TryAddWithoutValidation("X-Title", "Outfit Compliments");
            }

            using HttpResponseMessage response = await Http.SendAsync(request, token);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"{provider} HTTP {(int)response.StatusCode}: {TrimForLog(json)}");

            return ExtractOpenAiCompatibleText(json);
        }

        private async Task<string> GenerateAnthropicAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            string endpoint = !string.IsNullOrWhiteSpace(ai.Endpoint)
                ? ai.Endpoint.Trim()
                : "https://api.anthropic.com/v1/messages";

            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint}", LogLevel.Info);

            int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
            string modelLower = (ai.Model ?? "").ToLowerInvariant();
            bool looksLikeReasoningModel =
                modelLower.Contains("opus") || modelLower.Contains("thinking") || modelLower.Contains("reasoning");

            int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, looksLikeReasoningModel);
            monitor.Log($" Anthropic output budget: {maxTokens} tokens for {ai.Model} (visible target {visibleTarget}, reasoning-like={looksLikeReasoningModel}).", LogLevel.Trace);

            // Build user content — text only, or multimodal with vision
            object userContent;
            if (ShouldAttachVision(ai, visionImage))
            {
                List<object> contentParts = new()
                {
                    new { type = "text", text = prompt }
                };
                contentParts.Add(new Dictionary<string, object>
                {
                    ["type"] = "image",
                    ["source"] = new Dictionary<string, object>
                    {
                        ["type"] = "base64",
                        ["media_type"] = visionImage.MimeType,
                        ["data"] = visionImage.Base64Data
                    }
                });
                if (visionImage.HasBackImage)
                    contentParts.Add(new Dictionary<string, object>
                    {
                        ["type"] = "image",
                        ["source"] = new Dictionary<string, object>
                        {
                            ["type"] = "base64",
                            ["media_type"] = visionImage.MimeType,
                            ["data"] = visionImage.Base64DataBack
                        }
                    });
                userContent = contentParts.ToArray();
            }
            else
            {
                userContent = prompt;
            }

            object body = new
            {
                model = ai.Model,
                max_tokens = maxTokens,
                temperature = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
                system = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. Return portraits as one key per dialogue box, starting at box 1; the array may have 1, 2, 3, or more keys depending on the number of #$b# boxes. No markdown. No explanation. No narration. No analysis.",
                messages = new[]
                {
                    new { role = "user", content = userContent }
                }
            };

            using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            request.Headers.TryAddWithoutValidation("x-api-key", ai.ApiKey?.Trim() ?? "");
            request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");

            using HttpResponseMessage response = await Http.SendAsync(request, token);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Anthropic HTTP {(int)response.StatusCode}: {TrimForLog(json)}");

            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("content", out JsonElement contentArray))
            {
                StringBuilder combined = new();
                foreach (JsonElement block in contentArray.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out JsonElement typeEl)
                        && typeEl.GetString().Equals("text", StringComparison.OrdinalIgnoreCase)
                        && block.TryGetProperty("text", out JsonElement textEl))
                    {
                        combined.Append(textEl.GetString());
                    }
                }
                string result = combined.ToString();
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }

            monitor.Log(" Anthropic response did not contain content/text blocks. Raw response: " + TrimForLog(json), LogLevel.Warn);
            return "";
        }

        private async Task<string> GenerateGeminiAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            string endpoint = !string.IsNullOrWhiteSpace(ai.Endpoint)
                ? ai.Endpoint.Trim()
                : $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(ai.Model)}:generateContent";

            if (!endpoint.Contains("?key=", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(ai.ApiKey))
                endpoint += (endpoint.Contains("?") ? "&" : "?") + "key=" + Uri.EscapeDataString(ai.ApiKey.Trim());

            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint.Split('?')[0]}", LogLevel.Info);

            // Quality-friendly output budget for Gemini.
            // MaxCharacters still controls the final visible dialogue; this only gives the model
            // enough room to preserve tone and nuance.
            int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
            string geminiModelLower = (ai.Model ?? "").ToLowerInvariant();
            bool geminiReasoning =
                geminiModelLower.Contains("-pro") || geminiModelLower.Contains("pro-") || geminiModelLower.EndsWith("pro") ||
                geminiModelLower.Contains("thinking") || geminiModelLower.Contains("reasoning");

            int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, geminiReasoning);
            monitor.Log($" Gemini output budget: {maxTokens} tokens for {ai.Model} (visible target {visibleTarget}, reasoning-like={geminiReasoning}).", LogLevel.Trace);

            List<object> parts = new() { new { text = prompt } };
            if (ShouldAttachVision(ai, visionImage))
            {
                parts.Add(new Dictionary<string, object> { ["inline_data"] = new Dictionary<string, object> { ["mime_type"] = visionImage.MimeType, ["data"] = visionImage.Base64Data } });
                if (visionImage.HasBackImage)
                    parts.Add(new Dictionary<string, object> { ["inline_data"] = new Dictionary<string, object> { ["mime_type"] = visionImage.MimeType, ["data"] = visionImage.Base64DataBack } });
            }

            object body = new
            {
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                },
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = "You return one compact JSON object only. No markdown. No introduction. No explanation. Shape example: {\"text\":\"...\",\"portrait\":\"neutral fallback only\",\"portraits\":[\"actual portrait for box 1\"],\"needsClarification\":false}. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. The portraits array must have one key per dialogue box, matching the natural number of boxes in the text." }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = parts.ToArray()
                    }
                },
                generationConfig = new
                {
                    temperature = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
                    topP = 0.9,
                    maxOutputTokens = maxTokens,
                    // Disable Gemini "thinking" to keep reactions fast/cheap. thinkingBudget=0 turns
                    // it off on models that support the toggle (e.g. 2.5 Flash). Models that don't
                    // support it ignore the field.
                    thinkingConfig = new { thinkingBudget = 0 }
                }
            };

            using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await Http.SendAsync(request, token);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini HTTP {(int)response.StatusCode}: {TrimForLog(json)}");

            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates))
            {
                foreach (JsonElement candidate in candidates.EnumerateArray())
                {
                    if (candidate.TryGetProperty("finishReason", out JsonElement finishReason)
                        && !finishReason.GetString().Equals("STOP", StringComparison.OrdinalIgnoreCase))
                    {
                        monitor.Log(" Gemini finishReason was " + finishReason.GetString() + ". Trying to read usable text anyway.", LogLevel.Trace);
                    }

                    if (candidate.TryGetProperty("content", out JsonElement content) && content.TryGetProperty("parts", out JsonElement responseParts))
                    {
                        StringBuilder combined = new();
                        foreach (JsonElement part in responseParts.EnumerateArray())
                        {
                            if (part.TryGetProperty("text", out JsonElement text))
                                combined.Append(text.GetString());
                        }

                        string result = combined.ToString();
                        if (!string.IsNullOrWhiteSpace(result))
                            return result;
                    }
                }
            }

            monitor.Log(" Gemini response did not contain candidates/content/parts text. Raw response: " + TrimForLog(json), LogLevel.Warn);
            return "";
        }


        private static bool ShouldRetryWithoutVision(Exception ex, ActiveAiSettings ai, OutfitVisionImage visionImage)
        {
            if (ex == null || ai == null || visionImage == null || !visionImage.IsUsable)
                return false;

            string message = ex.Message ?? "";
            return message.IndexOf("support image input", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("image input", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("input_image", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("image_url", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("vision", StringComparison.OrdinalIgnoreCase) >= 0 && message.IndexOf("not", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ShouldAttachVision(ActiveAiSettings ai, OutfitVisionImage visionImage)
        {
            if (ai == null || visionImage == null || !visionImage.IsUsable)
                return false;

            string provider = ai.Provider ?? "";
            return provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("Local", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("xAI", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractOpenAiCompatibleText(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "";

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // OpenAI Responses API often exposes a convenience output_text field.
            if (root.TryGetProperty("output_text", out JsonElement outputText) && outputText.ValueKind == JsonValueKind.String)
                return outputText.GetString();

            // Chat Completions / DeepSeek / OpenAI-compatible APIs.
            if (root.TryGetProperty("choices", out JsonElement choices) && choices.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement choice in choices.EnumerateArray())
                {
                    if (choice.TryGetProperty("message", out JsonElement message) && message.TryGetProperty("content", out JsonElement content))
                    {
                        if (content.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(content.GetString()))
                            return content.GetString();

                        // Some providers can return content as an array of parts.
                        if (content.ValueKind == JsonValueKind.Array)
                        {
                            StringBuilder combined = new();
                            foreach (JsonElement part in content.EnumerateArray())
                            {
                                if (part.TryGetProperty("text", out JsonElement partText) && partText.ValueKind == JsonValueKind.String)
                                    combined.Append(partText.GetString());
                            }

                            if (combined.Length > 0)
                                return combined.ToString();
                        }
                    }

                    // NOTE: we deliberately do NOT fall back to reasoning_content. That field
                    // holds the model's THINKING, not the final answer; showing it would leak raw
                    // reasoning into the dialogue. An empty content means we should use the mod's
                    // normal fallback instead.

                    if (choice.TryGetProperty("text", out JsonElement text) && text.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(text.GetString()))
                        return text.GetString();
                }
            }

            // OpenAI Responses API structured output.
            if (root.TryGetProperty("output", out JsonElement output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out JsonElement content) || content.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (JsonElement contentItem in content.EnumerateArray())
                    {
                        if (contentItem.TryGetProperty("text", out JsonElement text) && text.ValueKind == JsonValueKind.String)
                            return text.GetString();
                    }
                }
            }

            return "";
        }

        private static int CalculateQualityOutputTokenBudget(int visibleTarget, bool reasoningLikeModel)
        {
            visibleTarget = Math.Clamp(visibleTarget, 80, 2000);

            // Token budget is derived purely from the visible character target, with no hard upper
            // cap. This avoids truncated JSON on models that use more tokens internally (e.g. Gemini
            // Flash/Pro) while still being naturally proportional to the requested dialogue length.
            int visibleBudget = visibleTarget * 3;
            int nuanceHeadroom = (int)(visibleTarget * 1.75);

            if (reasoningLikeModel)
            {
                int reasoningHeadroom = visibleTarget * 3;
                return Math.Max(1600, visibleBudget + reasoningHeadroom);
            }

            return Math.Max(1000, visibleBudget + nuanceHeadroom);
        }

        private static string TrimForLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length <= 500 ? text : text.Substring(0, 500) + "...";
        }

        private sealed class CancellationTokenSourceCompat : IDisposable
        {
            private readonly CancellationTokenSource source;
            public CancellationToken Token => source.Token;
            public CancellationTokenSourceCompat(TimeSpan timeout)
            {
                source = new CancellationTokenSource(timeout);
            }
            public void Dispose() => source.Dispose();
        }
    }
}
