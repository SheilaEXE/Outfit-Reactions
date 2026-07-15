using System.Collections.Generic;
using System.Net.Http;

namespace OutfitReactions.Ai.Providers
{
    internal sealed class OpenRouterProvider : AiProviderBase
    {
        public OpenRouterProvider() : base("OpenRouter", "OpenRouter", AiTransportKind.OpenAiChat, supportsVision: true) { }
        public override string ResolveEndpoint(string customEndpoint, string model) =>
            ResolveRoute(customEndpoint, "https://openrouter.ai/api/v1/chat/completions", "/chat/completions");
        public override void ConfigureRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://www.nexusmods.com/stardewvalley/mods/");
            request.Headers.TryAddWithoutValidation("X-OpenRouter-Title", "Outfit Reactions");
        }
        public override void ConfigureRequestBody(Dictionary<string, object> body, string model) =>
            body["reasoning"] = new Dictionary<string, object> { ["enabled"] = false };
    }
}
