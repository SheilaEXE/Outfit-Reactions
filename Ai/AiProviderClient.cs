using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OutfitReactions.Ai.Providers;

namespace OutfitReactions.Ai
{
    /// <summary>
    /// Owns all direct communication with AI providers: building the HTTP request for each
    /// provider family (OpenAI-compatible, Anthropic, Gemini), sending it, handling the
    /// vision-fallback retry, and extracting the raw text from the response. The caller passes
    /// resolved ActiveAiSettings; this class is otherwise self-contained.
    /// </summary>
    internal sealed class AiProviderClient
    {
        private readonly IMonitor monitor;
        private static readonly HttpClient Http = new();

        public AiProviderClient(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public async Task<string> GenerateRawAsync(ActiveAiSettings ai, string prompt, OutfitVisionImage visionImage = null, CancellationToken cancellationToken = default)
        {
            IAiProvider provider = AiProviderRegistry.Get(ai.Provider);
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(ai.TimeoutSeconds, 3, 120)));

            try
            {
                if (provider.Transport == AiTransportKind.GeminiGenerateContent)
                    return await GenerateGeminiAsync(ai, prompt, timeout.Token, visionImage);

                if (provider.Transport == AiTransportKind.AnthropicMessages)
                    return await GenerateAnthropicAsync(ai, prompt, timeout.Token, visionImage);

                return await GenerateOpenAiCompatibleAsync(ai, prompt, timeout.Token, visionImage);
            }
            catch (InvalidOperationException ex) when (ShouldRetryWithoutVision(ex, ai, visionImage))
            {
                monitor.Log(" Selected AI endpoint rejected image input. Retrying once without the attached image and using only text/confirmed visual clues.", LogLevel.Warn);

                string textOnlyPrompt = prompt
                    + "\n\nIMPORTANT VISION FALLBACK: The selected model/endpoint rejected image input, so no image is attached in this retry. "
                    + "Use only textual support data, saved outfit/theme clues, and confirmed color clues. "
                    + "Do not mention seeing an image, screenshot, PNG, pixels, or attachment. "
                    + "Do not invent exact visual details, scene objects, props, or current actions that are not explicitly stated.";

                if (provider.Transport == AiTransportKind.GeminiGenerateContent)
                    return await GenerateGeminiAsync(ai, textOnlyPrompt, timeout.Token, null);

                if (provider.Transport == AiTransportKind.AnthropicMessages)
                    return await GenerateAnthropicAsync(ai, textOnlyPrompt, timeout.Token, null);

                return await GenerateOpenAiCompatibleAsync(ai, textOnlyPrompt, timeout.Token, null);
            }
        }

        private static string NormalizeEndpoint(ActiveAiSettings ai)
        {
            IAiProvider provider = AiProviderRegistry.Get(ai?.Provider);
            return provider.ResolveEndpoint(ai?.Endpoint, ai?.Model);
        }

        private async Task<string> GenerateOpenAiCompatibleAsync(ActiveAiSettings ai, string prompt, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            IAiProvider provider = AiProviderRegistry.Get(ai.Provider);
            string endpoint = NormalizeEndpoint(ai);
            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint}", LogLevel.Info);

            bool useResponsesApi = endpoint.IndexOf("/responses", StringComparison.OrdinalIgnoreCase) >= 0;
            // Quality-friendly output budget for in-game dialogue.
            // This is NOT a hard final-dialogue limit; MaxCharacters is a soft visible-length target.
            // We give enough space for nuance, tone, JSON formatting, and moderate model reasoning,
            // without going back to the old 4000+ headroom that made Pro/Thinking models crawl.
            int visibleTarget = Math.Clamp(ai.MaxCharacters, 80, 400);
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

                Dictionary<string, object> responsesBody = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["model"] = ai.Model,
                    ["input"] = input,
                    ["temperature"] = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
                    ["max_output_tokens"] = maxTokens
                };
                provider.ConfigureRequestBody(responsesBody, ai.Model);
                requestJson = JsonSerializer.Serialize(responsesBody);
            }
            else
            {
                // Portrait-per-box is intentionally optional for now so the model can decide
                // whether an expression should stay the same or change during the dialogue.
                string systemMessage = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text. Use portrait for the primary expression. If text has multiple #$b# dialogue boxes, portraits must contain one valid key per box in order; reuse a key only while the mood stays the same and change it when the emotional beat changes. No markdown. No explanation. No narration. No analysis.";

                double temperature = Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0;
                if (provider.IsLocal)
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

                // Only the selected provider adds its supported reasoning or JSON options.
                provider.ConfigureRequestBody(chatBody, ai.Model);
                // Local/OpenAI-compatible servers are intentionally left in plain text mode.
                // Many local models follow a simple dash-line-style "- dialogue" line more reliably than JSON mode.

                requestJson = JsonSerializer.Serialize(chatBody);
            }

            using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            if (!string.IsNullOrWhiteSpace(ai.ApiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ai.ApiKey.Trim());
            provider.ConfigureRequestHeaders(request);

            using HttpResponseMessage response = await Http.SendAsync(request, token);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"{provider.Id} HTTP {(int)response.StatusCode}.");

            return ExtractOpenAiCompatibleText(json);
        }

        private async Task<string> GenerateAnthropicAsync(ActiveAiSettings ai, string prompt, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            string endpoint = AiProviderRegistry.Get("Anthropic").ResolveEndpoint(ai.Endpoint, ai.Model);

            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint}", LogLevel.Info);

            int visibleTarget = Math.Clamp(ai.MaxCharacters, 80, 400);
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
                system = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text. Use portrait for the primary expression. If text has multiple #$b# dialogue boxes, portraits must contain one valid key per box in order; reuse a key only while the mood stays the same and change it when the emotional beat changes. No markdown. No explanation. No narration. No analysis.",
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
                throw new InvalidOperationException($"Anthropic HTTP {(int)response.StatusCode}.");

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

            monitor.Log(" Anthropic response did not contain content/text blocks.", LogLevel.Warn);
            return "";
        }

        private async Task<string> GenerateGeminiAsync(ActiveAiSettings ai, string prompt, System.Threading.CancellationToken token, OutfitVisionImage visionImage = null)
        {
            string endpoint = AiProviderRegistry.Get("Gemini").ResolveEndpoint(ai.Endpoint, ai.Model);

            if (!endpoint.Contains("?key=", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(ai.ApiKey))
                endpoint += (endpoint.Contains("?") ? "&" : "?") + "key=" + Uri.EscapeDataString(ai.ApiKey.Trim());

            if (OutfitReactions.ModEntry.DebugLog) monitor.Log($" HTTP endpoint: {endpoint.Split('?')[0]}", LogLevel.Info);

            // Quality-friendly output budget for Gemini.
            // MaxCharacters is a soft visible-dialogue target; this only gives the model
            // enough room to preserve tone and nuance.
            int visibleTarget = Math.Clamp(ai.MaxCharacters, 80, 400);
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
                        new { text = "Return one compact JSON object only with text, portrait, portraits, and needsClarification. No markdown, introduction, or explanation. Do not put Stardew portrait $commands inside text. Use portrait for the primary expression. For multiple #$b# boxes, portraits must contain one valid key per box; reuse while the mood stays the same and change it when the emotional beat changes." }
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
                throw new InvalidOperationException($"Gemini HTTP {(int)response.StatusCode}.");

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

            monitor.Log(" Gemini response did not contain candidates/content/parts text.", LogLevel.Warn);
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

            return AiProviderRegistry.Get(ai.Provider).SupportsVision;
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
            visibleTarget = Math.Clamp(visibleTarget, 80, 400);

            // Keep enough output headroom for complete JSON and model reasoning while scaling
            // proportionally with the player's soft visible-character target.
            int visibleBudget = visibleTarget * 3;
            int nuanceHeadroom = (int)(visibleTarget * 1.75);

            if (reasoningLikeModel)
            {
                int reasoningHeadroom = visibleTarget * 3;
                return Math.Max(1600, visibleBudget + reasoningHeadroom);
            }

            return Math.Max(1000, visibleBudget + nuanceHeadroom);
        }

    }
}
