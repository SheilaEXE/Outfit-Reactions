using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using OutfitReactions.Ai;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyCompany("OutfitReactions")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.1.0")]
[assembly: AssemblyInformationalVersion("1.0.1+761fdc30222d62ce478f0c7c8b44428984e317ff")]
[assembly: AssemblyProduct("OutfitReactions")]
[assembly: AssemblyTitle("OutfitReactions")]
[assembly: AssemblyVersion("1.0.1.0")]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
}
namespace OutfitReactions
{
	public sealed class ModConfig
	{
		public bool Enabled { get; set; } = true;

		public bool UseVoiceSamples { get; set; } = true;

		public bool UseFsInternalIdAsHint { get; set; } = true;

		public int VoiceSampleCount { get; set; } = 6;

		public string VoiceSampleExcludedNpcs { get; set; } = "";

		public string AiProvider { get; set; } = "Gemini";

		public string AiModel { get; set; } = "";

		public string AiApiKey { get; set; } = "";

		public string AiCustomEndpoint { get; set; } = "";

		public int AiTemperaturePercent { get; set; } = 75;

		public int AiTimeoutSeconds { get; set; } = 60;

		public int AiMaxCharacters { get; set; } = 300;

		public int AiMinimumCharacters { get; set; } = 100;

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

		public string AiProviderSlot1 { get; set; } = "Gemini";

		public string AiProviderSlot2 { get; set; } = "OpenAI";

		public string AiProviderSlot3 { get; set; } = "OpenRouter";

		public string AiProviderSlot4 { get; set; } = "Groq";

		public string AiProviderSlot5 { get; set; } = "Mistral";

		public bool AiSlot1Enabled { get; set; } = true;

		public bool AiSlot2Enabled { get; set; } = false;

		public bool AiSlot3Enabled { get; set; } = false;

		public bool AiSlot4Enabled { get; set; } = false;

		public bool AiSlot5Enabled { get; set; } = false;

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

		public bool UseJsonFallbackForOutfitReactions { get; set; } = false;

		public bool EnableNpcOutfitReactions { get; set; } = true;

		public int NpcOutfitReactionChance { get; set; } = 30;

		public int NpcRepeatedVisualNoticeChance { get; set; } = 15;

		public bool EnableDebugLogging { get; set; } = false;

		public string VanillaHatReactionMode { get; set; } = "Combined";

		public string VanillaSpecialItemReactionMode { get; set; } = "ItemOnly";

		public int OutfitNoticeDistance { get; set; } = 650;

		public int OutfitCancelDistance { get; set; } = 900;

		public void MigrateLegacyAiSettings()
		{
			string text = (AiProvider = NormalizeProvider(AiProvider));
			if (text.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(DeepSeekAiModel) && !string.IsNullOrWhiteSpace(AiModel))
				{
					DeepSeekAiModel = AiModel;
				}
				if (string.IsNullOrWhiteSpace(DeepSeekAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
				{
					DeepSeekAiApiKey = AiApiKey;
				}
				if (string.IsNullOrWhiteSpace(DeepSeekAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
				{
					DeepSeekAiCustomEndpoint = AiCustomEndpoint;
				}
				if (DeepSeekAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
				{
					DeepSeekAiTemperaturePercent = AiTemperaturePercent;
				}
				if (DeepSeekAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
				{
					DeepSeekAiTimeoutSeconds = AiTimeoutSeconds;
				}
				if (DeepSeekAiMaxCharacters == 200 && AiMaxCharacters != 200)
				{
					DeepSeekAiMaxCharacters = AiMaxCharacters;
				}
			}
			else if (text.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(GeminiAiModel) && !string.IsNullOrWhiteSpace(AiModel))
				{
					GeminiAiModel = AiModel;
				}
				if (string.IsNullOrWhiteSpace(GeminiAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
				{
					GeminiAiApiKey = AiApiKey;
				}
				if (string.IsNullOrWhiteSpace(GeminiAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
				{
					GeminiAiCustomEndpoint = AiCustomEndpoint;
				}
				if (GeminiAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
				{
					GeminiAiTemperaturePercent = AiTemperaturePercent;
				}
				if (GeminiAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
				{
					GeminiAiTimeoutSeconds = AiTimeoutSeconds;
				}
				if (GeminiAiMaxCharacters == 200 && AiMaxCharacters != 200)
				{
					GeminiAiMaxCharacters = AiMaxCharacters;
				}
			}
			else if (text.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(OpenAiAiModel) && !string.IsNullOrWhiteSpace(AiModel))
				{
					OpenAiAiModel = AiModel;
				}
				if (string.IsNullOrWhiteSpace(OpenAiAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
				{
					OpenAiAiApiKey = AiApiKey;
				}
				if (string.IsNullOrWhiteSpace(OpenAiAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
				{
					OpenAiAiCustomEndpoint = AiCustomEndpoint;
				}
				if (OpenAiAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
				{
					OpenAiAiTemperaturePercent = AiTemperaturePercent;
				}
				if (OpenAiAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
				{
					OpenAiAiTimeoutSeconds = AiTimeoutSeconds;
				}
				if (OpenAiAiMaxCharacters == 200 && AiMaxCharacters != 200)
				{
					OpenAiAiMaxCharacters = AiMaxCharacters;
				}
			}
			else if (text.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(OpenRouterAiModel) && !string.IsNullOrWhiteSpace(AiModel))
				{
					OpenRouterAiModel = AiModel;
				}
				if (string.IsNullOrWhiteSpace(OpenRouterAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
				{
					OpenRouterAiApiKey = AiApiKey;
				}
				if (string.IsNullOrWhiteSpace(OpenRouterAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
				{
					OpenRouterAiCustomEndpoint = AiCustomEndpoint;
				}
				if (OpenRouterAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
				{
					OpenRouterAiTemperaturePercent = AiTemperaturePercent;
				}
				if (OpenRouterAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
				{
					OpenRouterAiTimeoutSeconds = AiTimeoutSeconds;
				}
				if (OpenRouterAiMaxCharacters == 200 && AiMaxCharacters != 200)
				{
					OpenRouterAiMaxCharacters = AiMaxCharacters;
				}
			}
			else if (text.Equals("Local", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(LocalAiModel) && !string.IsNullOrWhiteSpace(AiModel))
				{
					LocalAiModel = AiModel;
				}
				if (string.IsNullOrWhiteSpace(LocalAiApiKey) && !string.IsNullOrWhiteSpace(AiApiKey))
				{
					LocalAiApiKey = AiApiKey;
				}
				if (string.IsNullOrWhiteSpace(LocalAiCustomEndpoint) && !string.IsNullOrWhiteSpace(AiCustomEndpoint))
				{
					LocalAiCustomEndpoint = AiCustomEndpoint;
				}
				if (LocalAiTemperaturePercent == 75 && AiTemperaturePercent != 75)
				{
					LocalAiTemperaturePercent = AiTemperaturePercent;
				}
				if (LocalAiTimeoutSeconds == 60 && AiTimeoutSeconds != 45)
				{
					LocalAiTimeoutSeconds = AiTimeoutSeconds;
				}
				if (LocalAiMaxCharacters == 200 && AiMaxCharacters != 200)
				{
					LocalAiMaxCharacters = AiMaxCharacters;
				}
			}
			ApplyAiDefaultsAndLimits();
		}

		public void ApplyAiDefaultsAndLimits()
		{
			AiProvider = NormalizeProvider(AiProvider);
			UseJsonFallbackForOutfitReactions = false;
			EnableNpcOutfitReactions = true;
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
			if (string.IsNullOrWhiteSpace(AiModelSlot1) && string.IsNullOrWhiteSpace(AiApiKeySlot1) && string.IsNullOrWhiteSpace(AiCustomEndpointSlot1) && string.IsNullOrWhiteSpace(AiModelSlot2) && string.IsNullOrWhiteSpace(AiApiKeySlot2) && string.IsNullOrWhiteSpace(AiCustomEndpointSlot2) && string.IsNullOrWhiteSpace(AiModelSlot3) && string.IsNullOrWhiteSpace(AiApiKeySlot3) && string.IsNullOrWhiteSpace(AiCustomEndpointSlot3))
			{
				string provider = NormalizeProvider(AiProvider);
				AiModelSlot1 = GetProviderFallbackModel(provider);
				AiApiKeySlot1 = GetProviderFallbackApiKey(provider);
				AiCustomEndpointSlot1 = GetProviderFallbackEndpoint(provider);
			}
		}

		public string GetResolvedAiModelForProvider(string provider)
		{
			provider = NormalizeProvider(provider);
			int aiCredentialSlotForProvider = GetAiCredentialSlotForProvider(provider);
			string text = ((aiCredentialSlotForProvider > 0) ? GetSlotModel(aiCredentialSlotForProvider) : "");
			return (!string.IsNullOrWhiteSpace(text)) ? text : GetProviderFallbackModel(provider);
		}

		public string GetResolvedAiApiKeyForProvider(string provider)
		{
			provider = NormalizeProvider(provider);
			int aiCredentialSlotForProvider = GetAiCredentialSlotForProvider(provider);
			string text = ((aiCredentialSlotForProvider > 0) ? GetSlotApiKey(aiCredentialSlotForProvider) : "");
			return (!string.IsNullOrWhiteSpace(text)) ? text : GetProviderFallbackApiKey(provider);
		}

		public string GetResolvedAiEndpointForProvider(string provider)
		{
			provider = NormalizeProvider(provider);
			int aiCredentialSlotForProvider = GetAiCredentialSlotForProvider(provider);
			string text = ((aiCredentialSlotForProvider > 0) ? GetSlotEndpoint(aiCredentialSlotForProvider) : "");
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			string providerFallbackEndpoint = GetProviderFallbackEndpoint(provider);
			return (!string.IsNullOrWhiteSpace(providerFallbackEndpoint)) ? providerFallbackEndpoint : GetDefaultEndpointForProvider(provider);
		}

		public int GetAiCredentialSlotForProvider(string provider)
		{
			provider = NormalizeProvider(provider);
			for (int i = 1; i <= 5; i++)
			{
				if (IsSlotEnabled(i) && NormalizeProvider(GetSlotProvider(i)).Equals(provider, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(GetSlotModel(i)) || !string.IsNullOrWhiteSpace(GetSlotApiKey(i)) || !string.IsNullOrWhiteSpace(GetSlotEndpoint(i))))
				{
					return i;
				}
			}
			for (int j = 1; j <= 5; j++)
			{
				if (SlotMatchesProvider(j, provider))
				{
					return j;
				}
			}
			return 0;
		}

		public string GetActiveProvider()
		{
			for (int i = 1; i <= 5; i++)
			{
				if (IsSlotEnabled(i))
				{
					return NormalizeProvider(GetSlotProvider(i));
				}
			}
			return NormalizeProvider(GetSlotProvider(1));
		}

		private string GetSlotVisionMode(int slot)
		{
			if (1 == 0)
			{
			}
			string text = slot switch
			{
				1 => AiVisionModeSlot1, 
				2 => AiVisionModeSlot2, 
				3 => AiVisionModeSlot3, 
				4 => AiVisionModeSlot4, 
				5 => AiVisionModeSlot5, 
				_ => "Auto", 
			};
			if (1 == 0)
			{
			}
			string text2 = text;
			if (string.IsNullOrWhiteSpace(text2))
			{
				return "Auto";
			}
			if (text2.Equals("On", StringComparison.OrdinalIgnoreCase))
			{
				return "On";
			}
			if (text2.Equals("Off", StringComparison.OrdinalIgnoreCase))
			{
				return "Off";
			}
			return "Auto";
		}

		public static bool ModelNameLooksMultimodal(string model)
		{
			if (string.IsNullOrWhiteSpace(model))
			{
				return false;
			}
			string text = model.ToLowerInvariant();
			string[] array = new string[33]
			{
				"vl", "vision", "multimodal", "-mm", "llava", "pixtral", "gpt-4o", "4o-", "gpt-5", "gemini",
				"claude-3", "claude-4", "claude-haiku", "claude-opus", "claude-sonnet", "qwen2-vl", "qwen2.5-vl", "qwen3-vl", "qwen-vl", "qwen-omni",
				"qwen3-omni", "qwen3.7", "qwen3-7", "qwen/qwen3.7", "qwen/qwen3-7", "llama-3.2", "llama3.2", "internvl", "molmo", "phi-3.5-vision",
				"phi-4-multimodal", "maverick", "scout"
			};
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (text.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool ShouldSendImageToActiveModel()
		{
			int num = 0;
			string activeProvider = GetActiveProvider();
			for (int i = 1; i <= 5; i++)
			{
				if (IsSlotEnabled(i))
				{
					num = i;
					break;
				}
			}
			if (num == 0)
			{
				num = 1;
			}
			string slotVisionMode = GetSlotVisionMode(num);
			if (slotVisionMode.Equals("On", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (slotVisionMode.Equals("Off", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			string resolvedAiModelForProvider = GetResolvedAiModelForProvider(activeProvider);
			return ModelNameLooksMultimodal(resolvedAiModelForProvider);
		}

		private bool SlotMatchesProvider(int slot, string provider)
		{
			string text = (GetSlotModel(slot) ?? "").Trim().ToLowerInvariant();
			string text2 = (GetSlotEndpoint(slot) ?? "").Trim().ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(text2))
			{
				return false;
			}
			provider = NormalizeProvider(provider);
			if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
			{
				return text2.StartsWith("http://localhost") || text2.StartsWith("http://127.0.0.1") || text2.Contains("localhost:") || text2.Contains("127.0.0.1:");
			}
			if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				return text2.Contains("openrouter.ai") || text.Equals("openrouter/free") || text.Contains(":free") || (text.Contains("/") && !text2.Contains("deepseek.com") && !text2.Contains("googleapis.com") && !text2.Contains("openai.com"));
			}
			if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				return text2.Contains("generativelanguage") || text2.Contains("googleapis.com") || text.Contains("gemini");
			}
			if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				return text2.Contains("api.openai.com") || text.StartsWith("gpt-") || text.StartsWith("o1") || text.StartsWith("o3") || text.StartsWith("o4");
			}
			return text2.Contains("deepseek.com") || (text.StartsWith("deepseek") && !text2.Contains("openrouter.ai"));
		}

		private string GetSlotModel(int slot)
		{
			if (1 == 0)
			{
			}
			string result = slot switch
			{
				1 => AiModelSlot1, 
				2 => AiModelSlot2, 
				3 => AiModelSlot3, 
				4 => AiModelSlot4, 
				5 => AiModelSlot5, 
				_ => "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private string GetSlotApiKey(int slot)
		{
			if (1 == 0)
			{
			}
			string result = slot switch
			{
				1 => AiApiKeySlot1, 
				2 => AiApiKeySlot2, 
				3 => AiApiKeySlot3, 
				4 => AiApiKeySlot4, 
				5 => AiApiKeySlot5, 
				_ => "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private string GetSlotEndpoint(int slot)
		{
			if (1 == 0)
			{
			}
			string result = slot switch
			{
				1 => AiCustomEndpointSlot1, 
				2 => AiCustomEndpointSlot2, 
				3 => AiCustomEndpointSlot3, 
				4 => AiCustomEndpointSlot4, 
				5 => AiCustomEndpointSlot5, 
				_ => "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		public int CountEnabledAiProfiles()
		{
			int num = 0;
			for (int i = 1; i <= 5; i++)
			{
				if (IsSlotEnabled(i))
				{
					num++;
				}
			}
			return num;
		}

		public bool HasMultipleEnabledAiProfiles()
		{
			return CountEnabledAiProfiles() > 1;
		}

		private string GetSlotProvider(int slot)
		{
			if (1 == 0)
			{
			}
			string result = slot switch
			{
				1 => AiProviderSlot1, 
				2 => AiProviderSlot2, 
				3 => AiProviderSlot3, 
				4 => AiProviderSlot4, 
				5 => AiProviderSlot5, 
				_ => "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private bool IsSlotEnabled(int slot)
		{
			if (1 == 0)
			{
			}
			bool result = slot switch
			{
				1 => AiSlot1Enabled, 
				2 => AiSlot2Enabled, 
				3 => AiSlot3Enabled, 
				4 => AiSlot4Enabled, 
				5 => AiSlot5Enabled, 
				_ => false, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private string GetProviderFallbackModel(string provider)
		{
			provider = NormalizeProvider(provider);
			if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				return GeminiAiModel;
			}
			if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				return OpenAiAiModel;
			}
			if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				return OpenRouterAiModel;
			}
			if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
			{
				return MistralAiModel;
			}
			if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
			{
				return GroqAiModel;
			}
			if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase))
			{
				return TogetherAiModel;
			}
			if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
			{
				return LocalAiModel;
			}
			if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
			{
				return AnthropicAiModel;
			}
			if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
			{
				return XAiModel;
			}
			if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
			{
				return CerebrasAiModel;
			}
			if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
			{
				return PerplexityAiModel;
			}
			return DeepSeekAiModel;
		}

		private string GetProviderFallbackApiKey(string provider)
		{
			provider = NormalizeProvider(provider);
			if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				return GeminiAiApiKey;
			}
			if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				return OpenAiAiApiKey;
			}
			if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				return OpenRouterAiApiKey;
			}
			if (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
			{
				return MistralAiApiKey;
			}
			if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
			{
				return GroqAiApiKey;
			}
			if (provider.Equals("Together", StringComparison.OrdinalIgnoreCase))
			{
				return TogetherAiApiKey;
			}
			if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
			{
				return LocalAiApiKey;
			}
			if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
			{
				return AnthropicAiApiKey;
			}
			if (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
			{
				return XAiApiKey;
			}
			if (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
			{
				return CerebrasAiApiKey;
			}
			if (provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
			{
				return PerplexityAiApiKey;
			}
			return DeepSeekAiApiKey;
		}

		private string GetProviderFallbackEndpoint(string provider)
		{
			provider = NormalizeProvider(provider);
			string text = (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) ? GeminiAiCustomEndpoint : (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) ? OpenAiAiCustomEndpoint : (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) ? OpenRouterAiCustomEndpoint : (provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase) ? MistralAiCustomEndpoint : (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase) ? GroqAiCustomEndpoint : (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) ? TogetherAiCustomEndpoint : (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) ? LocalAiCustomEndpoint : (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) ? AnthropicAiCustomEndpoint : (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase) ? XAiCustomEndpoint : (provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase) ? CerebrasAiCustomEndpoint : ((!provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase)) ? DeepSeekAiCustomEndpoint : PerplexityAiCustomEndpoint)))))))))));
			return (!string.IsNullOrWhiteSpace(text)) ? text : GetDefaultEndpointForProvider(provider);
		}

		private static string NormalizeProvider(string provider)
		{
			if (provider != null && provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				return "OpenAI";
			}
			if (provider != null && provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
			{
				return "Gemini";
			}
			if (provider != null && provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				return "OpenRouter";
			}
			if (provider != null && provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
			{
				return "Mistral";
			}
			if (provider != null && provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
			{
				return "Groq";
			}
			if (provider != null && (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Together AI", StringComparison.OrdinalIgnoreCase)))
			{
				return "Together";
			}
			if (provider != null && (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)))
			{
				return "Local";
			}
			if (provider != null && (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) || provider.Equals("Claude", StringComparison.OrdinalIgnoreCase)))
			{
				return "Anthropic";
			}
			if (provider != null && (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Grok", StringComparison.OrdinalIgnoreCase) || provider.Equals("x.ai", StringComparison.OrdinalIgnoreCase)))
			{
				return "xAI";
			}
			if (provider != null && provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
			{
				return "Cerebras";
			}
			if (provider != null && provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
			{
				return "Perplexity";
			}
			if (provider != null && provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
			{
				return "DeepSeek";
			}
			return "Gemini";
		}

		public static string GetDefaultEndpointForProvider(string provider)
		{
			return NormalizeProvider(provider) switch
			{
				"Gemini" => "", 
				"OpenAI" => "https://api.openai.com/v1/chat/completions", 
				"OpenRouter" => "https://openrouter.ai/api/v1/chat/completions", 
				"Mistral" => "https://api.mistral.ai/v1/chat/completions", 
				"Groq" => "https://api.groq.com/openai/v1/chat/completions", 
				"Together" => "https://api.together.xyz/v1/chat/completions", 
				"Local" => "http://localhost:1234/v1/chat/completions", 
				"Anthropic" => "https://api.anthropic.com/v1/messages", 
				"xAI" => "https://api.x.ai/v1/chat/completions", 
				"Cerebras" => "https://api.cerebras.ai/v1/chat/completions", 
				"Perplexity" => "https://api.perplexity.ai/chat/completions", 
				_ => "https://api.deepseek.com/v1/chat/completions", 
			};
		}

		private static string NormalizeSeason(string season)
		{
			if (string.IsNullOrWhiteSpace(season))
			{
				return "";
			}
			string text = season.Trim().ToLowerInvariant();
			if (1 == 0)
			{
			}
			string result;
			switch (text)
			{
			case "spring":
			case "primavera":
				result = "spring";
				break;
			case "summer":
			case "verao":
			case "verão":
				result = "summer";
				break;
			case "fall":
			case "autumn":
			case "outono":
				result = "fall";
				break;
			case "winter":
			case "inverno":
				result = "winter";
				break;
			default:
				result = text;
				break;
			}
			if (1 == 0)
			{
			}
			return result;
		}

		private static int Clamp(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}
	}
	internal static class ModConfigMenu
	{
		public static void Register(ModEntry mod, IGenericModConfigMenuApi configMenu)
		{
			if (configMenu == null)
			{
				return;
			}
			mod.Config.ApplyAiDefaultsAndLimits();
			configMenu.Register(((Mod)mod).ModManifest, delegate
			{
				mod.Config = new ModConfig();
			}, delegate
			{
				mod.Config.ApplyAiDefaultsAndLimits();
				((Mod)mod).Helper.WriteConfig<ModConfig>(mod.Config);
				mod.QueueAiConnectionTestFromConfigMenu();
			});
			string[] allowedValues = new string[12]
			{
				"Gemini", "OpenAI", "OpenRouter", "Mistral", "Groq", "Together", "Anthropic", "xAI", "Cerebras", "Perplexity",
				"DeepSeek", "Local"
			};
			configMenu.AddSectionTitle(((Mod)mod).ModManifest, () => T("gmcm.section.general"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.Enabled, delegate(bool value)
			{
				mod.Config.Enabled = value;
			}, () => T("gmcm.option.enabled.name"), () => T("gmcm.option.enabled.tooltip"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.UseAiCache, delegate(bool value)
			{
				mod.Config.UseAiCache = value;
			}, () => T("gmcm.option.use-ai-cache.name"), () => T("gmcm.option.use-ai-cache.tooltip"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.EnablePlayerReplyMenuAfterOutfitCompliment, delegate(bool value)
			{
				mod.Config.EnablePlayerReplyMenuAfterOutfitCompliment = value;
			}, () => T("gmcm.option.player-reply.name"), () => T("gmcm.option.player-reply.tooltip"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.EnableExpressiveAsteriskActions, delegate(bool value)
			{
				mod.Config.EnableExpressiveAsteriskActions = value;
			}, () => T("gmcm.option.expressive-actions.name"), () => T("gmcm.option.expressive-actions.tooltip"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.UseFsInternalIdAsHint, delegate(bool value)
			{
				mod.Config.UseFsInternalIdAsHint = value;
			}, () => T("gmcm.option.fs-id-hint.name"), () => T("gmcm.option.fs-id-hint.tooltip"));
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.UseVoiceSamples, delegate(bool value)
			{
				mod.Config.UseVoiceSamples = value;
			}, () => T("gmcm.option.voice-samples.name"), () => T("gmcm.option.voice-samples.tooltip"));
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.VoiceSampleCount, delegate(int value)
			{
				mod.Config.VoiceSampleCount = value;
			}, () => T("gmcm.option.voice-sample-count.name"), () => T("gmcm.option.voice-sample-count.tooltip"), 1, 20);
			configMenu.AddTextOption(((Mod)mod).ModManifest, () => mod.Config.VoiceSampleExcludedNpcs, delegate(string value)
			{
				mod.Config.VoiceSampleExcludedNpcs = value;
			}, () => T("gmcm.option.voice-sample-excluded.name"), () => T("gmcm.option.voice-sample-excluded.tooltip"));
			for (int num = 1; num <= 5; num++)
			{
				int capturedSlot = num;
				configMenu.AddSectionTitle(((Mod)mod).ModManifest, () => TWithNumber("gmcm.section.profile", capturedSlot));
				configMenu.AddBoolOption(((Mod)mod).ModManifest, () => GetSlotEnabled(capturedSlot), delegate(bool value)
				{
					SetSlotEnabled(capturedSlot, value);
				}, () => T("gmcm.option.profile-enabled.name"), () => T("gmcm.option.profile-enabled.tooltip"));
				configMenu.AddTextOption(((Mod)mod).ModManifest, () => NormalizeProvider(GetSlotProvider(capturedSlot)), delegate(string value)
				{
					SetSlotProvider(capturedSlot, NormalizeProvider(value));
				}, () => T("gmcm.option.provider.name"), () => T("gmcm.option.provider.tooltip"), allowedValues, FormatProviderTranslated);
				configMenu.AddTextOption(((Mod)mod).ModManifest, () => GetSlotModel(capturedSlot), delegate(string value)
				{
					SetSlotModel(capturedSlot, value);
				}, () => T("gmcm.option.model.name"), () => T("gmcm.option.model.tooltip"));
				configMenu.AddTextOption(((Mod)mod).ModManifest, () => GetSlotApiKey(capturedSlot), delegate(string value)
				{
					SetSlotApiKey(capturedSlot, value);
				}, () => T("gmcm.option.api-key.name"), () => T("gmcm.option.api-key.tooltip"));
				configMenu.AddTextOption(((Mod)mod).ModManifest, () => GetSlotEndpoint(capturedSlot), delegate(string value)
				{
					SetSlotEndpoint(capturedSlot, value);
				}, () => T("gmcm.option.endpoint.name"), () => T("gmcm.option.endpoint.tooltip"));
				configMenu.AddTextOption(((Mod)mod).ModManifest, () => GetSlotVisionMode(capturedSlot), delegate(string value)
				{
					SetSlotVisionMode(capturedSlot, value);
				}, () => T("gmcm.option.vision-mode.name"), () => T("gmcm.option.vision-mode.tooltip"), new string[3] { "Auto", "On", "Off" }, FormatVisionMode);
			}
			configMenu.AddNumberOption(((Mod)mod).ModManifest, GetActiveTemperature, SetActiveTemperature, () => T("gmcm.option.temperature.name"), () => T("gmcm.option.temperature.tooltip"), 0, 200, 5, FormatTemperature);
			configMenu.AddNumberOption(((Mod)mod).ModManifest, GetActiveTimeout, SetActiveTimeout, () => T("gmcm.option.timeout.name"), () => T("gmcm.option.timeout.tooltip"), 3, 120, 1, (int value) => TWithValue("gmcm.value.seconds", value));
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.AiMinimumCharacters, delegate(int value)
			{
				mod.Config.AiMinimumCharacters = value;
			}, () => T("gmcm.option.min-characters.name"), () => T("gmcm.option.min-characters.tooltip"), 0, 2000, 10, (int value) => (value <= 0) ? T("gmcm.value.no-minimum") : TWithValue("gmcm.value.characters", value));
			configMenu.AddNumberOption(((Mod)mod).ModManifest, GetActiveMaxCharacters, SetActiveMaxCharacters, () => T("gmcm.option.max-characters.name"), () => T("gmcm.option.max-characters.tooltip"), 80, 2000, 10, (int value) => TWithValue("gmcm.value.characters", value));
			configMenu.AddSectionTitle(((Mod)mod).ModManifest, () => T("gmcm.section.distance"));
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.NpcOutfitReactionChance, delegate(int value)
			{
				mod.Config.NpcOutfitReactionChance = value;
			}, () => T("gmcm.option.npc-reaction-chance.name"), () => T("gmcm.option.npc-reaction-chance.tooltip"), 0, 100, 5, (int value) => $"{value}%");
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.NpcRepeatedVisualNoticeChance, delegate(int value)
			{
				mod.Config.NpcRepeatedVisualNoticeChance = value;
			}, () => T("gmcm.option.npc-repeated-visual-chance.name"), () => T("gmcm.option.npc-repeated-visual-chance.tooltip"), 0, 100, 5, (int value) => $"{value}%");
			configMenu.AddBoolOption(((Mod)mod).ModManifest, () => mod.Config.EnableDebugLogging, delegate(bool value)
			{
				mod.Config.EnableDebugLogging = value;
			}, () => T("gmcm.option.debug-logging.name"), () => T("gmcm.option.debug-logging.tooltip"));
			configMenu.AddTextOption(((Mod)mod).ModManifest, () => NormalizeVanillaHatReactionMode(mod.Config.VanillaHatReactionMode), delegate(string value)
			{
				mod.Config.VanillaHatReactionMode = NormalizeVanillaHatReactionMode(value);
			}, () => T("gmcm.option.vanilla-hat-mode.name"), () => T("gmcm.option.vanilla-hat-mode.tooltip"), new string[2] { "Combined", "HatOnly" }, (string value) => T("gmcm.option.vanilla-hat-mode.value." + (value ?? "Combined")));
			configMenu.AddTextOption(((Mod)mod).ModManifest, () => NormalizeVanillaSpecialItemReactionMode(mod.Config.VanillaSpecialItemReactionMode), delegate(string value)
			{
				mod.Config.VanillaSpecialItemReactionMode = NormalizeVanillaSpecialItemReactionMode(value);
			}, () => T("gmcm.option.vanilla-special-item-mode.name"), () => T("gmcm.option.vanilla-special-item-mode.tooltip"), new string[2] { "Combined", "ItemOnly" }, (string value) => T("gmcm.option.vanilla-special-item-mode.value." + (value ?? "ItemOnly")));
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.OutfitNoticeDistance, delegate(int value)
			{
				mod.Config.OutfitNoticeDistance = value;
			}, () => T("gmcm.option.outfit-notice-distance.name"), () => T("gmcm.option.outfit-notice-distance.tooltip"), 100, 2000, 50);
			configMenu.AddNumberOption(((Mod)mod).ModManifest, () => mod.Config.OutfitCancelDistance, delegate(int value)
			{
				mod.Config.OutfitCancelDistance = value;
			}, () => T("gmcm.option.outfit-cancel-distance.name"), () => T("gmcm.option.outfit-cancel-distance.tooltip"), 200, 3000, 50);
			string ActiveProvider()
			{
				return NormalizeProvider(mod.Config.GetActiveProvider());
			}
			static string FormatProvider(string provider)
			{
				string text = NormalizeProvider(provider);
				if (text.Equals("Local", StringComparison.OrdinalIgnoreCase))
				{
					return "Local / OpenAI Compatible";
				}
				if (text.Equals("Together", StringComparison.OrdinalIgnoreCase))
				{
					return "Together AI";
				}
				if (text.Equals("xAI", StringComparison.OrdinalIgnoreCase))
				{
					return "xAI (Grok)";
				}
				return text;
			}
			string FormatProviderTranslated(string provider)
			{
				string text = NormalizeProvider(provider);
				if (text.Equals("Local", StringComparison.OrdinalIgnoreCase))
				{
					return T("gmcm.value.provider.local");
				}
				if (text.Equals("Together", StringComparison.OrdinalIgnoreCase))
				{
					return T("gmcm.value.provider.together");
				}
				return FormatProvider(text);
			}
			static string FormatTemperature(int value)
			{
				return ((float)value / 100f).ToString("0.00");
			}
			string FormatVisionMode(string value)
			{
				return (value == "On") ? T("gmcm.value.vision-mode.on") : ((value == "Off") ? T("gmcm.value.vision-mode.off") : T("gmcm.value.vision-mode.auto"));
			}
			int GetActiveMaxCharacters()
			{
				return ActiveProvider() switch
				{
					"Gemini" => mod.Config.GeminiAiMaxCharacters, 
					"OpenAI" => mod.Config.OpenAiAiMaxCharacters, 
					"OpenRouter" => mod.Config.OpenRouterAiMaxCharacters, 
					"Mistral" => mod.Config.MistralAiMaxCharacters, 
					"Groq" => mod.Config.GroqAiMaxCharacters, 
					"Together" => mod.Config.TogetherAiMaxCharacters, 
					"Local" => mod.Config.LocalAiMaxCharacters, 
					"Anthropic" => mod.Config.AnthropicAiMaxCharacters, 
					"xAI" => mod.Config.XAiMaxCharacters, 
					"Cerebras" => mod.Config.CerebrasAiMaxCharacters, 
					"Perplexity" => mod.Config.PerplexityAiMaxCharacters, 
					_ => mod.Config.DeepSeekAiMaxCharacters, 
				};
			}
			int GetActiveTemperature()
			{
				return ActiveProvider() switch
				{
					"Gemini" => mod.Config.GeminiAiTemperaturePercent, 
					"OpenAI" => mod.Config.OpenAiAiTemperaturePercent, 
					"OpenRouter" => mod.Config.OpenRouterAiTemperaturePercent, 
					"Mistral" => mod.Config.MistralAiTemperaturePercent, 
					"Groq" => mod.Config.GroqAiTemperaturePercent, 
					"Together" => mod.Config.TogetherAiTemperaturePercent, 
					"Local" => mod.Config.LocalAiTemperaturePercent, 
					"Anthropic" => mod.Config.AnthropicAiTemperaturePercent, 
					"xAI" => mod.Config.XAiTemperaturePercent, 
					"Cerebras" => mod.Config.CerebrasAiTemperaturePercent, 
					"Perplexity" => mod.Config.PerplexityAiTemperaturePercent, 
					_ => mod.Config.DeepSeekAiTemperaturePercent, 
				};
			}
			int GetActiveTimeout()
			{
				return ActiveProvider() switch
				{
					"Gemini" => mod.Config.GeminiAiTimeoutSeconds, 
					"OpenAI" => mod.Config.OpenAiAiTimeoutSeconds, 
					"OpenRouter" => mod.Config.OpenRouterAiTimeoutSeconds, 
					"Mistral" => mod.Config.MistralAiTimeoutSeconds, 
					"Groq" => mod.Config.GroqAiTimeoutSeconds, 
					"Together" => mod.Config.TogetherAiTimeoutSeconds, 
					"Local" => mod.Config.LocalAiTimeoutSeconds, 
					"Anthropic" => mod.Config.AnthropicAiTimeoutSeconds, 
					"xAI" => mod.Config.XAiTimeoutSeconds, 
					"Cerebras" => mod.Config.CerebrasAiTimeoutSeconds, 
					"Perplexity" => mod.Config.PerplexityAiTimeoutSeconds, 
					_ => mod.Config.DeepSeekAiTimeoutSeconds, 
				};
			}
			string GetSlotApiKey(int slot)
			{
				if (1 == 0)
				{
				}
				string result = slot switch
				{
					1 => mod.Config.AiApiKeySlot1, 
					2 => mod.Config.AiApiKeySlot2, 
					3 => mod.Config.AiApiKeySlot3, 
					4 => mod.Config.AiApiKeySlot4, 
					5 => mod.Config.AiApiKeySlot5, 
					_ => "", 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			bool GetSlotEnabled(int slot)
			{
				if (1 == 0)
				{
				}
				bool result = slot switch
				{
					1 => mod.Config.AiSlot1Enabled, 
					2 => mod.Config.AiSlot2Enabled, 
					3 => mod.Config.AiSlot3Enabled, 
					4 => mod.Config.AiSlot4Enabled, 
					5 => mod.Config.AiSlot5Enabled, 
					_ => false, 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			string GetSlotEndpoint(int slot)
			{
				if (1 == 0)
				{
				}
				string result = slot switch
				{
					1 => mod.Config.AiCustomEndpointSlot1, 
					2 => mod.Config.AiCustomEndpointSlot2, 
					3 => mod.Config.AiCustomEndpointSlot3, 
					4 => mod.Config.AiCustomEndpointSlot4, 
					5 => mod.Config.AiCustomEndpointSlot5, 
					_ => "", 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			string GetSlotModel(int slot)
			{
				if (1 == 0)
				{
				}
				string result = slot switch
				{
					1 => mod.Config.AiModelSlot1, 
					2 => mod.Config.AiModelSlot2, 
					3 => mod.Config.AiModelSlot3, 
					4 => mod.Config.AiModelSlot4, 
					5 => mod.Config.AiModelSlot5, 
					_ => "", 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			string GetSlotProvider(int slot)
			{
				if (1 == 0)
				{
				}
				string result = slot switch
				{
					1 => mod.Config.AiProviderSlot1, 
					2 => mod.Config.AiProviderSlot2, 
					3 => mod.Config.AiProviderSlot3, 
					4 => mod.Config.AiProviderSlot4, 
					5 => mod.Config.AiProviderSlot5, 
					_ => "Gemini", 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			string GetSlotVisionMode(int slot)
			{
				if (1 == 0)
				{
				}
				string text = slot switch
				{
					1 => mod.Config.AiVisionModeSlot1, 
					2 => mod.Config.AiVisionModeSlot2, 
					3 => mod.Config.AiVisionModeSlot3, 
					4 => mod.Config.AiVisionModeSlot4, 
					5 => mod.Config.AiVisionModeSlot5, 
					_ => "Auto", 
				};
				if (1 == 0)
				{
				}
				string text2 = text;
				return string.IsNullOrWhiteSpace(text2) ? "Auto" : text2;
			}
			static string NormalizeProvider(string provider)
			{
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
				{
					return "Gemini";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
				{
					return "OpenAI";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
				{
					return "OpenRouter";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
				{
					return "Mistral";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
				{
					return "Groq";
				}
				if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Together AI", StringComparison.OrdinalIgnoreCase)))
				{
					return "Together";
				}
				if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase)))
				{
					return "Local";
				}
				if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) || provider.Equals("Claude", StringComparison.OrdinalIgnoreCase)))
				{
					return "Anthropic";
				}
				if (!string.IsNullOrWhiteSpace(provider) && (provider.Equals("xAI", StringComparison.OrdinalIgnoreCase) || provider.Equals("Grok", StringComparison.OrdinalIgnoreCase)))
				{
					return "xAI";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
				{
					return "Cerebras";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
				{
					return "Perplexity";
				}
				if (!string.IsNullOrWhiteSpace(provider) && provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
				{
					return "DeepSeek";
				}
				return "Gemini";
			}
			void SetActiveMaxCharacters(int value)
			{
				switch (ActiveProvider())
				{
				case "Gemini":
					mod.Config.GeminiAiMaxCharacters = value;
					break;
				case "OpenAI":
					mod.Config.OpenAiAiMaxCharacters = value;
					break;
				case "OpenRouter":
					mod.Config.OpenRouterAiMaxCharacters = value;
					break;
				case "Mistral":
					mod.Config.MistralAiMaxCharacters = value;
					break;
				case "Groq":
					mod.Config.GroqAiMaxCharacters = value;
					break;
				case "Together":
					mod.Config.TogetherAiMaxCharacters = value;
					break;
				case "Local":
					mod.Config.LocalAiMaxCharacters = value;
					break;
				case "Anthropic":
					mod.Config.AnthropicAiMaxCharacters = value;
					break;
				case "xAI":
					mod.Config.XAiMaxCharacters = value;
					break;
				case "Cerebras":
					mod.Config.CerebrasAiMaxCharacters = value;
					break;
				case "Perplexity":
					mod.Config.PerplexityAiMaxCharacters = value;
					break;
				default:
					mod.Config.DeepSeekAiMaxCharacters = value;
					break;
				}
			}
			void SetActiveTemperature(int value)
			{
				switch (ActiveProvider())
				{
				case "Gemini":
					mod.Config.GeminiAiTemperaturePercent = value;
					break;
				case "OpenAI":
					mod.Config.OpenAiAiTemperaturePercent = value;
					break;
				case "OpenRouter":
					mod.Config.OpenRouterAiTemperaturePercent = value;
					break;
				case "Mistral":
					mod.Config.MistralAiTemperaturePercent = value;
					break;
				case "Groq":
					mod.Config.GroqAiTemperaturePercent = value;
					break;
				case "Together":
					mod.Config.TogetherAiTemperaturePercent = value;
					break;
				case "Local":
					mod.Config.LocalAiTemperaturePercent = value;
					break;
				case "Anthropic":
					mod.Config.AnthropicAiTemperaturePercent = value;
					break;
				case "xAI":
					mod.Config.XAiTemperaturePercent = value;
					break;
				case "Cerebras":
					mod.Config.CerebrasAiTemperaturePercent = value;
					break;
				case "Perplexity":
					mod.Config.PerplexityAiTemperaturePercent = value;
					break;
				default:
					mod.Config.DeepSeekAiTemperaturePercent = value;
					break;
				}
			}
			void SetActiveTimeout(int value)
			{
				switch (ActiveProvider())
				{
				case "Gemini":
					mod.Config.GeminiAiTimeoutSeconds = value;
					break;
				case "OpenAI":
					mod.Config.OpenAiAiTimeoutSeconds = value;
					break;
				case "OpenRouter":
					mod.Config.OpenRouterAiTimeoutSeconds = value;
					break;
				case "Mistral":
					mod.Config.MistralAiTimeoutSeconds = value;
					break;
				case "Groq":
					mod.Config.GroqAiTimeoutSeconds = value;
					break;
				case "Together":
					mod.Config.TogetherAiTimeoutSeconds = value;
					break;
				case "Local":
					mod.Config.LocalAiTimeoutSeconds = value;
					break;
				case "Anthropic":
					mod.Config.AnthropicAiTimeoutSeconds = value;
					break;
				case "xAI":
					mod.Config.XAiTimeoutSeconds = value;
					break;
				case "Cerebras":
					mod.Config.CerebrasAiTimeoutSeconds = value;
					break;
				case "Perplexity":
					mod.Config.PerplexityAiTimeoutSeconds = value;
					break;
				default:
					mod.Config.DeepSeekAiTimeoutSeconds = value;
					break;
				}
			}
			void SetSlotApiKey(int slot, string value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiApiKeySlot1 = value;
					break;
				case 2:
					mod.Config.AiApiKeySlot2 = value;
					break;
				case 3:
					mod.Config.AiApiKeySlot3 = value;
					break;
				case 4:
					mod.Config.AiApiKeySlot4 = value;
					break;
				case 5:
					mod.Config.AiApiKeySlot5 = value;
					break;
				}
			}
			void SetSlotEnabled(int slot, bool value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiSlot1Enabled = value;
					break;
				case 2:
					mod.Config.AiSlot2Enabled = value;
					break;
				case 3:
					mod.Config.AiSlot3Enabled = value;
					break;
				case 4:
					mod.Config.AiSlot4Enabled = value;
					break;
				case 5:
					mod.Config.AiSlot5Enabled = value;
					break;
				}
			}
			void SetSlotEndpoint(int slot, string value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiCustomEndpointSlot1 = value;
					break;
				case 2:
					mod.Config.AiCustomEndpointSlot2 = value;
					break;
				case 3:
					mod.Config.AiCustomEndpointSlot3 = value;
					break;
				case 4:
					mod.Config.AiCustomEndpointSlot4 = value;
					break;
				case 5:
					mod.Config.AiCustomEndpointSlot5 = value;
					break;
				}
			}
			void SetSlotModel(int slot, string value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiModelSlot1 = value;
					break;
				case 2:
					mod.Config.AiModelSlot2 = value;
					break;
				case 3:
					mod.Config.AiModelSlot3 = value;
					break;
				case 4:
					mod.Config.AiModelSlot4 = value;
					break;
				case 5:
					mod.Config.AiModelSlot5 = value;
					break;
				}
			}
			void SetSlotProvider(int slot, string value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiProviderSlot1 = value;
					break;
				case 2:
					mod.Config.AiProviderSlot2 = value;
					break;
				case 3:
					mod.Config.AiProviderSlot3 = value;
					break;
				case 4:
					mod.Config.AiProviderSlot4 = value;
					break;
				case 5:
					mod.Config.AiProviderSlot5 = value;
					break;
				}
			}
			void SetSlotVisionMode(int slot, string value)
			{
				switch (slot)
				{
				case 1:
					mod.Config.AiVisionModeSlot1 = value;
					break;
				case 2:
					mod.Config.AiVisionModeSlot2 = value;
					break;
				case 3:
					mod.Config.AiVisionModeSlot3 = value;
					break;
				case 4:
					mod.Config.AiVisionModeSlot4 = value;
					break;
				case 5:
					mod.Config.AiVisionModeSlot5 = value;
					break;
				}
			}
			string T(string key)
			{
				return ((object)((Mod)mod).Helper.Translation.Get(key)).ToString();
			}
			string TWithNumber(string key, int number)
			{
				return ((object)((Mod)mod).Helper.Translation.Get(key, (object)new { number })).ToString();
			}
			string TWithValue(string key, int value)
			{
				return ((object)((Mod)mod).Helper.Translation.Get(key, (object)new { value })).ToString();
			}
		}

		internal static string NormalizeVanillaHatReactionMode(string mode)
		{
			if (string.Equals(mode, "HatOnly", StringComparison.OrdinalIgnoreCase))
			{
				return "HatOnly";
			}
			return "Combined";
		}

		internal static string NormalizeVanillaSpecialItemReactionMode(string mode)
		{
			if (string.Equals(mode, "Combined", StringComparison.OrdinalIgnoreCase))
			{
				return "Combined";
			}
			return "ItemOnly";
		}
	}
	public sealed class ModEntry : Mod
	{
		[HarmonyPatch]
		private static class NPCCheckActionPatch
		{
			private static bool firstRunLogged;

			private static MethodBase TargetMethod()
			{
				return AccessTools.Method(typeof(NPC), "checkAction", new Type[2]
				{
					typeof(Farmer),
					typeof(GameLocation)
				});
			}

			[HarmonyPriority(800)]
			private static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
			{
				try
				{
					if (!firstRunLogged)
					{
						firstRunLogged = true;
						if (DebugLog)
						{
							ModEntry instance = Instance;
							if (instance != null)
							{
								IMonitor monitor = ((Mod)instance).Monitor;
								if (monitor != null)
								{
									monitor.Log("[CLOTHES PRIORITY] NPC.checkAction prefix ran for the first time.", (LogLevel)2);
								}
							}
						}
					}
					ModEntry instance2 = Instance;
					if (instance2 != null && instance2.TryHandleOutfitDialogueOrBlockNpcInteraction(__instance))
					{
						__result = true;
						return false;
					}
				}
				catch (Exception ex)
				{
					ModEntry instance3 = Instance;
					if (instance3 != null)
					{
						IMonitor monitor2 = ((Mod)instance3).Monitor;
						if (monitor2 != null)
						{
							monitor2.Log("[CLOTHES PRIORITY] Error while prioritizing/blocking outfit dialogue before NPC.checkAction: " + ex, (LogLevel)3);
						}
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(NPC), "doMiddleAnimation")]
		private static class NPCDoMiddleAnimationPatch
		{
			private static bool Prefix(NPC __instance)
			{
				try
				{
					if (__instance == null || Instance?.otherNpcClothesReactionSystem == null)
					{
						return true;
					}
					if (!Instance.otherNpcClothesReactionSystem.IsHeldForFishingSpecialAction(__instance))
					{
						return true;
					}
					return false;
				}
				catch (Exception value)
				{
					ModEntry instance = Instance;
					if (instance != null)
					{
						IMonitor monitor = ((Mod)instance).Monitor;
						if (monitor != null)
						{
							monitor.Log($"[NPC OUTFIT] Error suppressing doMiddleAnimation: {value}", (LogLevel)3);
						}
					}
					return true;
				}
			}
		}

		private sealed class FashionSenseChangeInfo
		{
			public bool ChangedHair;

			public bool ChangedAccessory;

			public bool ChangedHat;

			public bool ChangedShirt;

			public bool ChangedPants;

			public bool ChangedSleeves;

			public bool ChangedShoes;

			public bool ChangedOutfit;

			public string NewHairId;

			public string NewAccessoryId;

			public string NewHatId;

			public string NewVanillaHatId;

			public bool VanillaHatChanged;

			public bool VanillaHatRemoved;

			public string PreviousVanillaHatId;

			public List<string> PreviousVanillaHatSpecialItemCandidates = new List<string>();

			public bool VanillaPantsChanged;

			public bool VanillaPantsRemoved;

			public string PreviousVanillaPantsName;

			public string NewVanillaPantsName;

			public List<string> PreviousVanillaPantsSpecialItemCandidates = new List<string>();

			public List<string> NewVanillaPantsSpecialItemCandidates = new List<string>();

			public string NewShirtId;

			public string NewPantsId;

			public string NewSleevesId;

			public string NewShoesId;

			public string NewOutfitId;

			public int CountChanges()
			{
				int num = 0;
				if (ChangedHair)
				{
					num++;
				}
				if (ChangedAccessory)
				{
					num++;
				}
				if (ChangedHat)
				{
					num++;
				}
				if (ChangedShirt)
				{
					num++;
				}
				if (ChangedPants)
				{
					num++;
				}
				if (VanillaPantsChanged)
				{
					num++;
				}
				if (ChangedSleeves)
				{
					num++;
				}
				if (ChangedOutfit)
				{
					num++;
				}
				return num;
			}
		}

		private sealed class FashionSenseSnapshot
		{
			public string Hair;

			public string Accessory;

			public string AccessorySecondary;

			public string AccessoryTertiary;

			public string Hat;

			public bool FashionSenseHatCoversVanilla;

			public string VanillaHat;

			public List<string> VanillaHatSpecialItemCandidates = new List<string>();

			public string VanillaPants;

			public List<string> VanillaPantsSpecialItemCandidates = new List<string>();

			public string Shirt;

			public string Pants;

			public string Sleeves;

			public string Shoes;

			public string OutfitId;

			public string HairColor;

			public string AccessoryColor;

			public string AccessorySecondaryColor;

			public string AccessoryTertiaryColor;

			public string HatColor;

			public string ShirtColor;

			public string PantsColor;

			public string SleevesColor;

			public string ShoesColor;
		}

		private sealed class SpecialItemNoticeInfo
		{
			public string EntryId { get; set; } = "";

			public string DisplayName { get; set; } = "";

			public string ItemType { get; set; } = "";

			public string MatchedName { get; set; } = "";

			public string ReactionContext { get; set; } = "";

			public bool WasRemoved { get; set; }

			public string MemoryHint { get; set; } = "";

			public bool HasSecret { get; set; }

			public string SecretId { get; set; } = "";

			public bool IsValid => !string.IsNullOrWhiteSpace(EntryId) && !string.IsNullOrWhiteSpace(ReactionContext);
		}

		private readonly Random random = new Random();

		private Harmony harmony;

		private IFashionSenseApi fsApi;

		private OtherNpcClothesReactionSystem otherNpcClothesReactionSystem;

		private OutfitAiService outfitAiService;

		private OutfitMemoryService outfitMemoryService;

		private HatMemoryService hatMemoryService;

		private string lastKnownVanillaHatId;

		private List<string> lastKnownVanillaHatSpecialItemCandidates = new List<string>();

		private string lastKnownVanillaPantsName;

		private List<string> lastKnownVanillaPantsSpecialItemCandidates = new List<string>();

		private readonly HashSet<string> loggedSpecialItemDebugKeys = new HashSet<string>(StringComparer.Ordinal);

		private bool vanillaClothingTrackingInitialized;

		private int vanillaClothingPollTimer;

		private OutfitVisionService outfitVisionService;

		private FashionSenseVisualService fashionSenseVisualService;

		private SpecialHatReactionService specialHatReactionService;

		private SpecialItemReactionService specialItemReactionService;

		private bool changedClothes = false;

		private string lastEligibleSavedOutfitId = "";

		internal const string ReactionActiveModDataKey = "NatrollEXE.OutfitReactions/ReactionActive";

		private const string AutoKissClickActiveModDataKey = "NatrollEXE.LotsOfKisses/AutoKissClickActive";

		private FashionSenseSnapshot fsSnapshotBefore = null;

		private bool fashionSenseMenuOpen = false;

		private FashionSenseChangeInfo lastFashionSenseChangeInfo = null;

		private readonly HashSet<string> npcsReactedToCurrentNotice = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private readonly AiGenerationCoordinator aiGenerationCoordinator = new AiGenerationCoordinator();

		private readonly OutfitReplyConversationHistory outfitReplyConversationHistory = new OutfitReplyConversationHistory();

		private const string AssetPrefix = "Mods/NatrollEXE.OutfitReactions/Clothes";

		private const string OutfitNoticeModDataPrefix = "NatrollEXE.OutfitReactions.OutfitNotice.";

		private const string PlayerAccessoryDescriptionModDataPrefix = "NatrollEXE.OutfitReactions.PlayerAccessoryDescription.";

		private const string LotsOfKissesBystanderWatchingModDataKey = "NatrollEXE.LotsOfKisses/BystanderWatching";

		private readonly SpouseOutfitReactionProgressState spouseOutfitReactionProgressState = new SpouseOutfitReactionProgressState();

		private readonly SpouseDialogueController spouseDialogueController = new SpouseDialogueController();

		private SpouseOutfitReactionCoordinator spouseOutfitReactionCoordinator;

		private readonly SpouseRouteController spouseRouteController = new SpouseRouteController();

		private readonly SpouseOutfitApproachController spouseOutfitApproachController = new SpouseOutfitApproachController();

		private readonly SpouseOutfitNoticeController spouseOutfitNoticeController = new SpouseOutfitNoticeController();

		private readonly SpouseProximityState spouseProximityState = new SpouseProximityState();

		private readonly SpouseSpecialActionController spouseSpecialActionController = new SpouseSpecialActionController();

		private const float OutfitSpecialActionRestoreDistance = 300f;

		private const float SpouseOutfitNoticePauseDistance = 96f;

		private const float SpouseOutfitNoticeReleaseDistance = 300f;

		private const string PantsSeenModDataPrefix = "NatrollEXE.OutfitReactions/PantsSeen/";

		private const string SpecialItemSeenModDataPrefix = "NatrollEXE.OutfitReactions/SpecialItemSeen/";

		internal static ModEntry Instance { get; private set; }

		internal ModConfig Config { get; set; } = new ModConfig();

		internal static bool DebugLog => Instance?.Config?.EnableDebugLogging == true;

		private bool isReactingToClothes
		{
			get
			{
				return spouseOutfitReactionProgressState.IsReacting;
			}
			set
			{
				spouseOutfitReactionProgressState.IsReacting = value;
			}
		}

		private int clothesInteractionCooldown
		{
			get
			{
				return spouseOutfitReactionProgressState.InteractionCooldown;
			}
			set
			{
				spouseOutfitReactionProgressState.InteractionCooldown = value;
			}
		}

		private bool clothesPathStarted
		{
			get
			{
				return spouseOutfitReactionProgressState.PathStarted;
			}
			set
			{
				spouseOutfitReactionProgressState.PathStarted = value;
			}
		}

		private bool clothesComplimentReady
		{
			get
			{
				return spouseOutfitReactionProgressState.ComplimentReady;
			}
			set
			{
				spouseOutfitReactionProgressState.ComplimentReady = value;
			}
		}

		private Point clothesPreferredOffset
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return spouseOutfitReactionProgressState.PreferredOffset;
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				spouseOutfitReactionProgressState.PreferredOffset = value;
			}
		}

		private Point clothesLastPlayerTile
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return spouseOutfitReactionProgressState.LastPlayerTile;
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				spouseOutfitReactionProgressState.LastPlayerTile = value;
			}
		}

		private Point clothesLastTargetTile
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return spouseOutfitReactionProgressState.LastTargetTile;
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				spouseOutfitReactionProgressState.LastTargetTile = value;
			}
		}

		private bool clothesFirstNoticeDone
		{
			get
			{
				return spouseOutfitReactionProgressState.FirstNoticeDone;
			}
			set
			{
				spouseOutfitReactionProgressState.FirstNoticeDone = value;
			}
		}

		private bool clothesEmoteFired
		{
			get
			{
				return spouseOutfitReactionProgressState.EmoteFired;
			}
			set
			{
				spouseOutfitReactionProgressState.EmoteFired = value;
			}
		}

		private int clothesNoticePauseTimer
		{
			get
			{
				return spouseOutfitReactionProgressState.NoticePauseTimer;
			}
			set
			{
				spouseOutfitReactionProgressState.NoticePauseTimer = value;
			}
		}

		private bool playerWasInClothesNoticeRange
		{
			get
			{
				return spouseOutfitReactionProgressState.PlayerWasInNoticeRange;
			}
			set
			{
				spouseOutfitReactionProgressState.PlayerWasInNoticeRange = value;
			}
		}

		private int clothesSecondNoticeCooldown
		{
			get
			{
				return spouseOutfitReactionProgressState.SecondNoticeCooldown;
			}
			set
			{
				spouseOutfitReactionProgressState.SecondNoticeCooldown = value;
			}
		}

		private int clothesChaseTimer
		{
			get
			{
				return spouseOutfitReactionProgressState.ChaseTimer;
			}
			set
			{
				spouseOutfitReactionProgressState.ChaseTimer = value;
			}
		}

		private NPC clothesReactingNpc
		{
			get
			{
				return spouseOutfitReactionProgressState.ReactingNpc;
			}
			set
			{
				spouseOutfitReactionProgressState.ReactingNpc = value;
			}
		}

		private bool outfitSequenceActive
		{
			get
			{
				return spouseOutfitReactionProgressState.SequenceActive;
			}
			set
			{
				spouseOutfitReactionProgressState.SequenceActive = value;
			}
		}

		private SpouseOutfitReactionCoordinator SpouseOutfitReactionCoordinator => spouseOutfitReactionCoordinator ?? (spouseOutfitReactionCoordinator = new SpouseOutfitReactionCoordinator(spouseOutfitReactionProgressState, UpdateClothesReactionSystem, ShouldStartClothesReaction, ResetClothesState, UpdateSpousePostOutfitLinger, TryHandleOutfitDialogueOrBlockNpcInteractionCore));

		private bool TryGetCurrentSavedFashionSenseOutfitId(out string outfitId)
		{
			outfitId = null;
			if (fsApi == null)
			{
				return false;
			}
			try
			{
				KeyValuePair<bool, string> currentOutfitId = fsApi.GetCurrentOutfitId();
				if (!currentOutfitId.Key || string.IsNullOrWhiteSpace(currentOutfitId.Value))
				{
					return false;
				}
				outfitId = currentOutfitId.Value.Trim();
				return true;
			}
			catch (Exception ex)
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[FS] Could not read current saved outfit from Fashion Sense API: " + ex.Message, (LogLevel)2);
				}
				return false;
			}
		}

		private string GetCurrentGameLanguageForPrompt()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected I4, but got Unknown
			LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (1 == 0)
			{
			}
			string result = (currentLanguageCode - 1) switch
			{
				3 => "Brazilian Portuguese", 
				4 => "Spanish", 
				5 => "German", 
				7 => "French", 
				9 => "Italian", 
				0 => "Japanese", 
				8 => "Korean", 
				1 => "Russian", 
				10 => "Turkish", 
				2 => "Chinese", 
				11 => "Hungarian", 
				6 => "Thai", 
				_ => "English", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private OutfitAiContext BuildOutfitAiContext(NPC npc, bool isSpouseDialogue)
		{
			if (npc == null || lastFashionSenseChangeInfo == null)
			{
				return null;
			}
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (effectiveFashionSenseChangeInfoForNpc == null)
			{
				return null;
			}
			string fashionSenseDialogueKey = GetFashionSenseDialogueKey(effectiveFashionSenseChangeInfoForNpc);
			if (string.IsNullOrWhiteSpace(fashionSenseDialogueKey))
			{
				return null;
			}
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
			bool flag = Config?.UseFsInternalIdAsHint ?? true;
			string safeOutfitHint = ((flag && !string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi)) ? BuildSafeOutfitNameHint(currentSavedFashionSenseOutfitIdForAi) : "");
			string fashionSenseChangeType = GetFashionSenseChangeType(effectiveFashionSenseChangeInfoForNpc);
			string fashionSenseChangedItemId = GetFashionSenseChangedItemId(effectiveFashionSenseChangeInfoForNpc, fashionSenseChangeType);
			string safeNoticedChangeHint = ((flag && !string.IsNullOrWhiteSpace(fashionSenseChangedItemId)) ? BuildSafeOutfitNameHint(fashionSenseChangedItemId) : "");
			GameLocation currentLocation = Game1.currentLocation;
			string locationName = ((currentLocation != null) ? currentLocation.NameOrUniqueName : "");
			string currentSeason = Game1.currentSeason;
			int timeOfDay = Game1.timeOfDay;
			Farmer player = Game1.player;
			string playerName = (((player != null) ? ((Character)player).Name : null) ?? "").Trim();
			string playerGender = (Game1.player.IsMale ? "male" : "female");
			string currentGameLanguageForPrompt = GetCurrentGameLanguageForPrompt();
			(string Status, int Hearts) relationshipDialogueContext = GetRelationshipDialogueContext(npc);
			string item = relationshipDialogueContext.Status;
			int item2 = relationshipDialogueContext.Hearts;
			string dialogueKey = fashionSenseDialogueKey;
			if (!string.IsNullOrWhiteSpace(fashionSenseDialogueKey))
			{
				bool flag2 = IsFarmHouseLocation(currentLocation);
				bool flag3 = currentLocation != null && currentLocation.IsOutdoors;
				bool flag4 = !flag2 && !flag3 && IsMarriageCandidateNpcRoom(npc, currentLocation);
				dialogueKey = (flag2 ? (fashionSenseDialogueKey + ".FarmHouse") : (flag4 ? (fashionSenseDialogueKey + ".NpcRoom") : (flag3 ? (fashionSenseDialogueKey + ".Outside") : (fashionSenseDialogueKey + ".Inside"))));
			}
			bool flag5 = IsFarmHouseLocation(currentLocation);
			bool flag6 = currentLocation != null && currentLocation.IsOutdoors;
			bool isNpcRoom = !flag5 && !flag6 && IsMarriageCandidateNpcRoom(npc, currentLocation);
			bool isNpcPersonalLocation = !flag5 && !flag6 && IsMarriageCandidatePersonalLocation(npc, currentLocation);
			bool isIndoors = currentLocation != null && !flag6;
			bool flag7 = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());
			bool flag8 = effectiveFashionSenseChangeInfoForNpc?.VanillaHatRemoved ?? false;
			OutfitVisionImage visionImage = ((flag7 || flag8) ? null : TryCaptureVisionOutfitImageForAi());
			string summary = TryBuildFashionSenseVisualSummaryForAi(effectiveFashionSenseChangeInfoForNpc);
			summary = MergeRenderedHairColorIntoSummary(summary, visionImage, effectiveFashionSenseChangeInfoForNpc);
			if (!flag7 && !flag8)
			{
				summary = MergeRenderedHatColorIntoSummary(summary, visionImage, effectiveFashionSenseChangeInfoForNpc);
			}
			string text = ((!flag8) ? "" : (hatMemoryService?.GetLastHatNameForNpc(((Character)npc).Name) ?? ""));
			bool flag9 = flag8 && !string.IsNullOrWhiteSpace(text);
			string vanillaHatFraming = "";
			if (flag9)
			{
				string text2 = " The farmer just took off the hat they had been wearing and is now bare-headed; react to them having removed it. Do not describe or invent a color for the hat (it is no longer worn).";
				summary = (summary ?? "").TrimEnd() + text2;
				vanillaHatFraming = text2.Trim();
			}
			string vanillaHatMemoryHint = BuildVanillaHatMemoryContext(npc);
			string specialHatReactionContext = (flag9 ? (specialHatReactionService?.BuildContextForRemovedHat(text, currentGameLanguageForPrompt) ?? "") : ((!(!flag8 && flag7)) ? "" : (specialHatReactionService?.BuildContextForCurrentVanillaHat(Game1.player, currentGameLanguageForPrompt) ?? "")));
			SpecialItemNoticeInfo notice;
			bool flag10 = TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: true, out notice);
			string specialItemReactionContext = ((!flag10) ? "" : (notice?.ReactionContext ?? ""));
			bool flag11 = flag10 && (notice?.WasRemoved ?? false);
			string text3 = ((!flag10) ? "" : (notice?.MemoryHint ?? ""));
			if (flag10 && DebugLog)
			{
				((Mod)this).Monitor.Log($"[SPECIAL ITEM PROMPT] NPC={((Character)npc).Name} entry='{notice?.EntryId}' removed={flag11} memoryHint='{(string.IsNullOrWhiteSpace(text3) ? "<none>" : text3)}' combinedMode={ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined"}", (LogLevel)2);
			}
			string vanillaPantsMemoryHint = "";
			return new OutfitAiContext
			{
				NpcName = ((Character)npc).Name,
				NpcDisplayName = ((Character)npc).displayName,
				IsSpouse = isSpouseDialogue,
				DialogueKey = dialogueKey,
				OutfitName = currentSavedFashionSenseOutfitIdForAi,
				SafeOutfitHint = safeOutfitHint,
				ThemeContext = "",
				ThemePriorityInstruction = "",
				LocationName = locationName,
				DetailedLocationName = GetDetailedLocationNameForAiPrompt(currentLocation),
				LocationType = GetLocationTypeForAiPrompt(currentLocation, flag5, flag6, isNpcRoom),
				IsOutdoors = flag6,
				IsIndoors = isIndoors,
				IsNpcRoom = isNpcRoom,
				IsNpcPersonalLocation = isNpcPersonalLocation,
				IsBeachOrIsland = IsBeachOrIslandLocation(currentLocation),
				IsFarmHouse = flag5,
				DayPart = GetDayPartForAiPrompt(timeOfDay),
				FestivalContext = GetFestivalContextForAiPrompt(),
				FarmerBirthdayContext = GetFarmerBirthdayContextForAiPrompt(),
				Season = currentSeason,
				Weather = GetCurrentWeatherForAiPrompt(),
				Time = timeOfDay,
				DayOfSeason = Game1.dayOfMonth,
				Year = Game1.year,
				PlayerName = playerName,
				PlayerGender = playerGender,
				TargetLanguage = currentGameLanguageForPrompt,
				RelationshipStatus = item,
				RelationshipHearts = item2,
				VisionImage = visionImage,
				FashionSenseVisualSummary = summary,
				VanillaHatHatOnlyMode = (!flag10 && (flag7 || flag9) && ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) == "HatOnly"),
				VanillaHatFraming = vanillaHatFraming,
				NpcWitnessedPreviousAccessory = DidNpcWitnessPreviousLook(npc),
				SpecialHatReactionContext = specialHatReactionContext,
				SpecialItemReactionContext = specialItemReactionContext,
				SpecialItemWasJustRemoved = flag11,
				SpecialItemOnlyMode = flag10,
				SpecialItemCombinedMode = (flag10 && ModConfigMenu.NormalizeVanillaSpecialItemReactionMode(Config?.VanillaSpecialItemReactionMode) == "Combined"),
				SpecialItemMemoryHint = text3,
				VanillaPantsMemoryHint = vanillaPantsMemoryHint,
				VanillaHatMemoryHint = vanillaHatMemoryHint,
				AvailablePortraitCount = GetNpcPortraitCount(npc),
				NoticedChangeType = fashionSenseChangeType,
				NoticedChangeName = fashionSenseChangedItemId,
				SafeNoticedChangeHint = safeNoticedChangeHint,
				WasCaughtPeeking = (!isSpouseDialogue && (otherNpcClothesReactionSystem?.WasNpcCaughtPeeking(npc) ?? false)),
				OutfitMemoryContext = BuildOutfitMemoryContext(npc, currentSavedFashionSenseOutfitIdForAi)
			};
		}

		private string BuildOutfitMemoryContext(NPC npc, string outfitId)
		{
			if (outfitMemoryService == null || npc == null)
			{
				return null;
			}
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
			if (string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
			{
				return null;
			}
			OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
			OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
			if (memory == null)
			{
				return null;
			}
			return outfitMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
		}

		private void RecordOutfitMemory(NPC npc, string outfitId)
		{
			if (outfitMemoryService != null && npc != null)
			{
				string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(outfitId);
				if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
				{
					OutfitComponents components = BuildCurrentOutfitComponentsForMemory();
					outfitMemoryService.RecordMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, components, Game1.currentSeason, Game1.dayOfMonth, Game1.year);
				}
			}
		}

		private string GetCurrentSavedFashionSenseOutfitIdForAi(string fallbackOutfitId = null)
		{
			if (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) && !string.IsNullOrWhiteSpace(outfitId))
			{
				return outfitId;
			}
			return fallbackOutfitId ?? "";
		}

		private OutfitComponents BuildCurrentOutfitComponentsForMemory()
		{
			FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
			if (fashionSenseSnapshot != null)
			{
				string hat = ((!string.IsNullOrWhiteSpace(fashionSenseSnapshot.VanillaHat)) ? ("vanilla:" + fashionSenseSnapshot.VanillaHat) : (fashionSenseSnapshot.Hat ?? ""));
				return new OutfitComponents
				{
					Hat = hat,
					Hair = (fashionSenseSnapshot.Hair ?? ""),
					Shirt = (fashionSenseSnapshot.Shirt ?? ""),
					Pants = (fashionSenseSnapshot.Pants ?? ""),
					Sleeves = (fashionSenseSnapshot.Sleeves ?? ""),
					Accessory = BuildCurrentAccessoryMemoryValue(fashionSenseSnapshot)
				};
			}
			return new OutfitComponents
			{
				Hat = (lastFashionSenseChangeInfo?.NewHatId ?? ""),
				Hair = (lastFashionSenseChangeInfo?.NewHairId ?? ""),
				Shirt = (lastFashionSenseChangeInfo?.NewShirtId ?? ""),
				Pants = (lastFashionSenseChangeInfo?.NewPantsId ?? ""),
				Sleeves = (lastFashionSenseChangeInfo?.NewSleevesId ?? ""),
				Accessory = (lastFashionSenseChangeInfo?.NewAccessoryId ?? "")
			};
		}

		private static string BuildCurrentAccessoryMemoryValue(FashionSenseSnapshot snapshot)
		{
			if (snapshot == null)
			{
				return "";
			}
			return string.Join(" + ", new string[3]
			{
				NormalizeFashionSenseAccessoryId(snapshot.Accessory),
				NormalizeFashionSenseAccessoryId(snapshot.AccessorySecondary),
				NormalizeFashionSenseAccessoryId(snapshot.AccessoryTertiary)
			}.Where((string value) => !string.IsNullOrWhiteSpace(value)));
		}

		private static string NormalizeFashionSenseAccessoryId(string accessoryId)
		{
			if (string.IsNullOrWhiteSpace(accessoryId))
			{
				return "";
			}
			string text = accessoryId.Trim();
			return IsIgnoredFashionSenseAccessoryId(text) ? "" : text;
		}

		private static bool IsIgnoredFashionSenseAccessoryId(string accessoryId)
		{
			if (string.IsNullOrWhiteSpace(accessoryId))
			{
				return false;
			}
			string text = FashionSenseVisualService.HumanizeAppearanceId(accessoryId);
			string text2 = " " + string.Join(" ", accessoryId, text).ToLowerInvariant().Replace('_', ' ')
				.Replace('-', ' ')
				.Replace('.', ' ')
				.Replace('/', ' ') + " ";
			bool flag = (text2.Contains(" eye ") || text2.Contains(" eyes ") || text2.Contains(" olho ") || text2.Contains(" olhos ")) && (text2.Contains(" highlight ") || text2.Contains(" highlights ") || text2.Contains(" sparkle ") || text2.Contains(" sparkles ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho ") || text2.Contains(" brilhos "));
			bool flag2 = (text2.Contains(" face ") || text2.Contains(" facial ") || text2.Contains(" rosto ")) && (text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" highlight ") || text2.Contains(" blush ") || text2.Contains(" sparkle ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho "));
			return flag || flag2 || text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" blush ") || text2.Contains(" lipstick ") || text2.Contains(" batom ") || text2.Contains(" eyeshadow ") || text2.Contains(" eye shadow ") || text2.Contains(" sombra ") || text2.Contains(" eyeliner ") || text2.Contains(" delineador ") || text2.Contains(" rimel ") || text2.Contains(" rímel ");
		}

		private string GetFashionSenseChangeType(FashionSenseChangeInfo changeInfo)
		{
			if (changeInfo == null)
			{
				return "";
			}
			bool flag = AreVisionOnlyFashionSenseTriggersEnabled();
			if (changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId))
			{
				if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
				{
					return "Accessory";
				}
				return "Outfit";
			}
			if (ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
			{
				return "Outfit";
			}
			if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
			{
				return "Accessory";
			}
			if (changeInfo.VanillaHatChanged)
			{
				return "Hat";
			}
			if (changeInfo.ChangedHat && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId) && (flag || ItemNameRevealsShape(changeInfo.NewHatId)))
			{
				return "Hat";
			}
			if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
			{
				return "Hair";
			}
			return "";
		}

		private bool ShouldTreatAccessoryAsCurrentComboFocus(string accessoryId, bool visionOn)
		{
			if (string.IsNullOrWhiteSpace(accessoryId))
			{
				return false;
			}
			if (IsIgnoredFashionSenseAccessoryId(accessoryId))
			{
				return false;
			}
			if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(accessoryId))
			{
				return false;
			}
			return visionOn || ItemNameRevealsShape(accessoryId);
		}

		private bool ShouldTreatGenericHeadwearAsSavedOutfitPart(FashionSenseChangeInfo changeInfo)
		{
			if (changeInfo == null || !changeInfo.ChangedHat)
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(changeInfo.NewHatId))
			{
				return false;
			}
			if (!FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId))
			{
				return false;
			}
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(changeInfo.NewOutfitId);
			return !string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi);
		}

		private static string GetFashionSenseChangedItemId(FashionSenseChangeInfo changeInfo, string changeType)
		{
			if (changeInfo == null)
			{
				return "";
			}
			if (string.Equals(changeType, "Outfit", StringComparison.OrdinalIgnoreCase))
			{
				return changeInfo.NewOutfitId ?? "";
			}
			if (string.Equals(changeType, "Hair", StringComparison.OrdinalIgnoreCase))
			{
				return changeInfo.NewHairId ?? "";
			}
			if (string.Equals(changeType, "Hat", StringComparison.OrdinalIgnoreCase))
			{
				return changeInfo.NewHatId ?? "";
			}
			if (string.Equals(changeType, "Accessory", StringComparison.OrdinalIgnoreCase))
			{
				return StringUtils.FirstNonEmpty(changeInfo.NewAccessoryId, "unknown-accessory-change") ?? "";
			}
			return "";
		}

		private string TryBuildFashionSenseVisualSummaryForAi(FashionSenseChangeInfo effectiveChangeInfo)
		{
			if (fashionSenseVisualService == null || Game1.player == null)
			{
				return "";
			}
			FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
			string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(fashionSenseChangeInfo?.NewOutfitId);
			bool suppressHairAndGenericHeadwearForSavedOutfit = fashionSenseChangeInfo != null && (fashionSenseChangeInfo.ChangedOutfit || ShouldTreatGenericHeadwearAsSavedOutfitPart(fashionSenseChangeInfo));
			bool visibleVanillaHatEquipped = !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId());
			if (fashionSenseVisualService.TryBuildVisualSummary(Game1.player, currentSavedFashionSenseOutfitIdForAi, out var summary, out var reason, suppressHairAndGenericHeadwearForSavedOutfit, visibleVanillaHatEquipped))
			{
				string playerProvidedAccessoryDescriptionForCurrentChange = GetPlayerProvidedAccessoryDescriptionForCurrentChange(fashionSenseChangeInfo);
				if (!string.IsNullOrWhiteSpace(playerProvidedAccessoryDescriptionForCurrentChange))
				{
					summary = summary + "; player-provided description for the current small accessory/change: " + playerProvidedAccessoryDescriptionForCurrentChange;
				}
				return summary;
			}
			((Mod)this).Monitor.Log(" Vision outfit analysis is enabled, but Fashion Sense API visual support data could not be read: " + reason, (LogLevel)0);
			string playerProvidedAccessoryDescriptionForCurrentChange2 = GetPlayerProvidedAccessoryDescriptionForCurrentChange(fashionSenseChangeInfo);
			return string.IsNullOrWhiteSpace(playerProvidedAccessoryDescriptionForCurrentChange2) ? "" : ("Player-provided description for the current small accessory/change: " + playerProvidedAccessoryDescriptionForCurrentChange2);
		}

		private string MergeRenderedHairColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
		{
			FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
			bool flag = fashionSenseChangeInfo?.ChangedHair ?? false;
			bool flag2 = fashionSenseChangeInfo != null && !string.IsNullOrWhiteSpace(fashionSenseChangeInfo.NewHatId);
			if (!flag || flag2)
			{
				return summary;
			}
			if (visionImage == null || !visionImage.HasHairColor || string.IsNullOrWhiteSpace(visionImage.HairColorName))
			{
				return summary;
			}
			if (!string.IsNullOrWhiteSpace(summary) && summary.IndexOf("CONFIRMED hair color", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return summary;
			}
			string text = "CONFIRMED hair color from the rendered sprite (authoritative, use exactly this; do NOT take hair color from the raw image): " + visionImage.HairColorName + " (" + visionImage.HairColorHex + ")";
			if (string.IsNullOrWhiteSpace(summary))
			{
				return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: " + text;
			}
			return summary + "; " + text;
		}

		private string MergeRenderedHatColorIntoSummary(string summary, OutfitVisionImage visionImage, FashionSenseChangeInfo effectiveChangeInfo)
		{
			FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
			if (fashionSenseChangeInfo == null || !fashionSenseChangeInfo.ChangedHat || fashionSenseChangeInfo.ChangedOutfit || ShouldTreatGenericHeadwearAsSavedOutfitPart(fashionSenseChangeInfo) || FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(fashionSenseChangeInfo.NewHatId) || string.IsNullOrWhiteSpace(fashionSenseChangeInfo.NewHatId))
			{
				return summary;
			}
			if (visionImage == null || !visionImage.HasHatColor || string.IsNullOrWhiteSpace(visionImage.HatColorName))
			{
				return summary;
			}
			if (!string.IsNullOrWhiteSpace(summary) && summary.IndexOf("CONFIRMED hat color", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return summary;
			}
			string text = "CONFIRMED hat/headwear color from the rendered sprite (authoritative, use exactly this; do NOT take hat color from the raw image): " + visionImage.HatColorName + " (" + visionImage.HatColorHex + ")";
			if (string.IsNullOrWhiteSpace(summary))
			{
				return "Fashion Sense equipped appearance clues from the game API. Use only as support; never mention Fashion Sense, API, IDs, filenames, or labels in dialogue: " + text;
			}
			return summary + "; " + text;
		}

		private string GetPlayerProvidedAccessoryDescriptionForCurrentChange(FashionSenseChangeInfo effectiveChangeInfo = null)
		{
			FashionSenseChangeInfo fashionSenseChangeInfo = effectiveChangeInfo ?? lastFashionSenseChangeInfo;
			if (Game1.player == null || fashionSenseChangeInfo == null)
			{
				return "";
			}
			string text = fashionSenseChangeInfo.NewAccessoryId ?? "";
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string playerAccessoryDescriptionModDataKey = GetPlayerAccessoryDescriptionModDataKey(text);
			string text2 = default(string);
			return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(playerAccessoryDescriptionModDataKey, ref text2) ? CleanPlayerOutfitReplyText(text2) : "";
		}

		private void SavePlayerProvidedAccessoryDescriptionForCurrentChange(string description)
		{
			if (Game1.player == null || lastFashionSenseChangeInfo == null || !lastFashionSenseChangeInfo.ChangedAccessory || string.IsNullOrWhiteSpace(lastFashionSenseChangeInfo.NewAccessoryId))
			{
				return;
			}
			string text = CleanPlayerOutfitReplyText(description);
			if (!string.IsNullOrWhiteSpace(text))
			{
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[GetPlayerAccessoryDescriptionModDataKey(lastFashionSenseChangeInfo.NewAccessoryId)] = text;
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[FS VISUAL] Saved player-provided description for a small accessory/change.", (LogLevel)2);
				}
			}
		}

		private static string GetPlayerAccessoryDescriptionModDataKey(string accessoryId)
		{
			return "NatrollEXE.OutfitReactions.PlayerAccessoryDescription." + GetStableHexHash(accessoryId ?? "");
		}

		private OutfitVisionImage TryCaptureVisionOutfitImageForAi()
		{
			if (outfitVisionService == null || Game1.player == null)
			{
				return null;
			}
			if (!outfitVisionService.TryCaptureFarmerAppearance(Game1.player, out var image, out var reason))
			{
				((Mod)this).Monitor.Log(" Could not render the farmer sprite for color reading: " + reason, (LogLevel)0);
				return null;
			}
			if (!ShouldTryVisionForCurrentAiProvider() && image != null)
			{
				image.Base64Data = "";
			}
			return image;
		}

		private bool ShouldTryVisionForCurrentAiProvider()
		{
			return Config?.ShouldSendImageToActiveModel() ?? false;
		}

		private string GetDetailedLocationNameForAiPrompt(GameLocation location)
		{
			if (location == null)
			{
				return "unknown";
			}
			string text = location.NameOrUniqueName ?? location.Name ?? "unknown";
			string text2 = text;
			try
			{
				if (((object)location).GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(location) is string text3 && !string.IsNullOrWhiteSpace(text3))
				{
					text2 = text3;
				}
			}
			catch
			{
			}
			return string.Equals(text2, text, StringComparison.OrdinalIgnoreCase) ? text : (text2 + " (internal map: " + text + ")");
		}

		private string GetLocationTypeForAiPrompt(GameLocation location, bool isFarmHouse, bool isOutdoors, bool isNpcRoom)
		{
			if (location == null)
			{
				return "unknown";
			}
			if (isFarmHouse)
			{
				return "farmer farmhouse / home interior";
			}
			if (isNpcRoom)
			{
				return "marriage-candidate NPC room";
			}
			if (location != null && !isFarmHouse && !isOutdoors)
			{
				string text = (location.NameOrUniqueName ?? location.Name ?? "").ToLowerInvariant();
				if (text.Contains("house") || text.Contains("home") || text.Contains("shop") || text.Contains("trailer") || text.Contains("room") || text.Contains("basement"))
				{
					return "marriage-candidate home/private interior";
				}
			}
			if (isOutdoors)
			{
				return "outdoors";
			}
			return "indoors";
		}

		private string GetDayPartForAiPrompt(int time)
		{
			if (time < 1200)
			{
				return "morning";
			}
			if (time < 1800)
			{
				return "afternoon";
			}
			return "night";
		}

		private string GetFestivalContextForAiPrompt()
		{
			bool flag = false;
			try
			{
				if (typeof(Utility).GetMethod("isFestivalDay", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					typeof(int),
					typeof(string)
				}, null)?.Invoke(null, new object[2]
				{
					Game1.dayOfMonth,
					Game1.currentSeason
				}) is bool flag2)
				{
					flag = flag2;
				}
			}
			catch
			{
			}
			return flag ? "Today is a festival day in the current season. A subtle outfit reaction may reference the festive atmosphere if it fits naturally." : "Today is not a festival day.";
		}

		private string GetFarmerBirthdayContextForAiPrompt()
		{
			string text = (Config.FarmerBirthdaySeason ?? "").Trim();
			int farmerBirthdayDay = Config.FarmerBirthdayDay;
			if (string.IsNullOrWhiteSpace(text) || farmerBirthdayDay <= 0)
			{
				return "Farmer birthday is not configured.";
			}
			return (farmerBirthdayDay == Game1.dayOfMonth && text.Equals(Game1.currentSeason, StringComparison.OrdinalIgnoreCase)) ? "Today is the farmer's birthday. The compliment may feel a little more special if it fits the NPC and relationship." : ("Today is not the farmer's birthday. Farmer birthday is configured as " + text + " " + farmerBirthdayDay + ".");
		}

		private string GetCurrentWeatherForAiPrompt()
		{
			return Game1.isGreenRain ? "green rain" : (Game1.isLightning ? "storm / thunderstorm" : (Game1.isRaining ? "rain" : (Game1.isSnowing ? "snow" : (Game1.isDebrisWeather ? "windy / debris weather" : "sunny / clear"))));
		}

		private static string BuildSafeOutfitNameHint(string rawName)
		{
			if (string.IsNullOrWhiteSpace(rawName))
			{
				return "No readable saved outfit name was provided.";
			}
			string input = rawName.Trim();
			input = Regex.Replace(input, "[_\\-.]+", " ");
			input = Regex.Replace(input, "([a-zà-ÿ])([A-Z])", "$1 $2");
			input = Regex.Replace(input, "\\s*(?:#|n[ºo]?\\.?|v|ver|version|versao|versão|set)?\\s*\\d+\\s*$", "", RegexOptions.IgnoreCase);
			input = Regex.Replace(input, "\\s{2,}", " ").Trim();
			if (string.IsNullOrWhiteSpace(input))
			{
				return "The saved outfit name is only an internal label and does not provide a readable theme.";
			}
			return input;
		}

		private void ClearOutfitNoticeMemoryCommand(string command, string[] args)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if (!Context.IsWorldReady || Game1.player == null)
			{
				((Mod)this).Monitor.Log("[OC DEBUG] Load a save before clearing outfit notice memory.", (LogLevel)3);
				return;
			}
			List<string> list = ((IEnumerable<string>)(object)((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Keys).Where((string key) => key.StartsWith("NatrollEXE.OutfitReactions.OutfitNotice.", StringComparison.OrdinalIgnoreCase) || key.StartsWith("NatrollEXE.OutfitReactions/SpecialItemSeen/", StringComparison.OrdinalIgnoreCase) || key.StartsWith("NatrollEXE.OutfitReactions/PantsSeen/", StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (string item in list)
			{
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Remove(item);
			}
			ResetClothesState(clearChangeFlag: true);
			npcsReactedToCurrentNotice.Clear();
			otherNpcClothesReactionSystem?.Reset();
			((Mod)this).Monitor.Log($"[OC DEBUG] Cleared {list.Count} outfit notice memory key(s) from this save and reset pending outfit notice state.", (LogLevel)2);
		}

		private void VoiceSampleReportCommand(string command, string[] args)
		{
			if (!Context.IsWorldReady)
			{
				((Mod)this).Monitor.Log("Load a save first, then run oc_test_voicesamples so NPC dialogue is available.", (LogLevel)3);
				return;
			}
			if (outfitAiService == null)
			{
				((Mod)this).Monitor.Log("The outfit AI service is not ready yet.", (LogLevel)3);
				return;
			}
			string text = outfitAiService.BuildVoiceSampleReport();
			((Mod)this).Monitor.Log(text, (LogLevel)2);
		}

		private void VoiceSamplePreviewCommand(string command, string[] args)
		{
			if (!Context.IsWorldReady)
			{
				((Mod)this).Monitor.Log("Load a save first, then run oc_preview_voicesamples <NpcName> so NPC dialogue is available.", (LogLevel)3);
				return;
			}
			if (outfitAiService == null)
			{
				((Mod)this).Monitor.Log("The outfit AI service is not ready yet.", (LogLevel)3);
				return;
			}
			if (args == null || args.Length == 0)
			{
				((Mod)this).Monitor.Log("Usage: oc_preview_voicesamples <NpcName>  (e.g. oc_preview_voicesamples Victoria)", (LogLevel)3);
				return;
			}
			string npcName = string.Join(" ", args).Trim();
			string text = outfitAiService.BuildVoiceSamplePreview(npcName, Game1.currentSeason);
			((Mod)this).Monitor.Log(text, (LogLevel)2);
		}

		private void DebugOutfitNoticeCommand(string command, string[] args)
		{
			if (!Context.IsWorldReady || Game1.player == null)
			{
				((Mod)this).Monitor.Log("[OC DEBUG] Load a save before running this command.", (LogLevel)3);
				return;
			}
			string outfitId;
			string value = (TryGetCurrentSavedFashionSenseOutfitId(out outfitId) ? outfitId : "<none>");
			string value2 = ((lastFashionSenseChangeInfo != null) ? GetFashionSenseDialogueKey(lastFashionSenseChangeInfo) : "");
			string value3 = ((lastFashionSenseChangeInfo == null) ? "<none>" : $"count={lastFashionSenseChangeInfo.CountChanges()} hair={lastFashionSenseChangeInfo.ChangedHair} accessory={lastFashionSenseChangeInfo.ChangedAccessory} hat={lastFashionSenseChangeInfo.ChangedHat} outfit={lastFashionSenseChangeInfo.ChangedOutfit} newHair={lastFashionSenseChangeInfo.NewHairId ?? ""} newAccessory={lastFashionSenseChangeInfo.NewAccessoryId ?? ""} newHat={lastFashionSenseChangeInfo.NewHatId ?? ""} newOutfit={lastFashionSenseChangeInfo.NewOutfitId ?? ""}");
			((Mod)this).Monitor.Log($"[OC DEBUG] Config: Enabled={Config.Enabled}, NPC reactions={Config.EnableNpcOutfitReactions}, NPC chance={Config.NpcOutfitReactionChance}, NPC distance={Config.OutfitNoticeDistance}, spouse distance={Config.OutfitNoticeDistance}.", (LogLevel)2);
			((Mod)this).Monitor.Log($"[OC DEBUG] Fashion Sense: fsApi={fsApi != null}, currentSavedOutfit={value}, changedClothes={changedClothes}, dialogueKey='{value2}', change={value3}.", (LogLevel)2);
			((Mod)this).Monitor.Log($"[OC DEBUG] NPC profile reactions: Sebastian={outfitAiService?.HasProfile("Sebastian")}, Penny={outfitAiService?.HasProfile("Penny")}, Robin={outfitAiService?.HasProfile("Robin")}. Any NPC with an enabled profile can react.", (LogLevel)2);
			NPC spouse = GetSpouse();
			if (spouse != null)
			{
				float value4 = DistanceToPlayer(spouse);
				((Mod)this).Monitor.Log($"[OC DEBUG] Spouse {((Character)spouse).Name}: sameLoc={((Character)spouse).currentLocation == ((Character)Game1.player).currentLocation}, dist={value4:0}, facing={IsNpcFacingPlayer(spouse)}, canNotice={CanNpcNoticeCurrentOutfitNotice(spouse)}, shouldStart={ShouldStartClothesReaction(spouse)}, firstNoticeDone={clothesFirstNoticeDone}, reacting={isReactingToClothes}, ready={clothesComplimentReady}.", (LogLevel)2);
			}
			if (Game1.currentLocation?.characters == null || Game1.currentLocation.characters.Count <= 0)
			{
				((Mod)this).Monitor.Log("[OC DEBUG] No NPCs are currently loaded in the player's location.", (LogLevel)2);
				return;
			}
			foreach (NPC item in ((IEnumerable<NPC>)Game1.currentLocation.characters).Where((NPC n) => n != null).OrderBy(DistanceToPlayer).Take(10))
			{
				float num = DistanceToPlayer(item);
				bool value5 = ((Character)item).currentLocation == ((Character)Game1.player).currentLocation;
				bool value6 = outfitAiService?.HasProfile(((Character)item).Name) ?? false;
				bool value7 = CanNpcReactToOutfit(item);
				bool value8 = CanNpcNoticeCurrentOutfitNotice(item);
				bool value9 = IsNpcFacingPlayer(item);
				bool value10 = num > Math.Max(64f, Config.OutfitNoticeDistance);
				((Mod)this).Monitor.Log($"[OC DEBUG] NPC {((Character)item).Name}: dist={num:0}, sameLoc={value5}, villager={((Character)item).IsVillager}, invisible={item.IsInvisible}, sleeping={((NetFieldBase<bool, NetBool>)(object)item.isSleeping).Value}, profile={value6}, canReact={value7}, canNotice={value8}, facing={value9}, tooFar={value10}.", (LogLevel)2);
			}
		}

		internal bool IsAnyOutfitReactionActive()
		{
			if (isReactingToClothes || outfitSequenceActive)
			{
				return true;
			}
			if (clothesComplimentReady || clothesPathStarted)
			{
				return true;
			}
			OtherNpcClothesReactionSystem obj = otherNpcClothesReactionSystem;
			if (obj != null && obj.HasAnyActivePendingReaction())
			{
				return true;
			}
			return false;
		}

		private void UpdateReactionActiveModDataFlag()
		{
			if (Game1.player != null)
			{
				bool flag = IsAnyOutfitReactionActive();
				bool flag2 = ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).ContainsKey("NatrollEXE.OutfitReactions/ReactionActive");
				if (flag && !flag2)
				{
					((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)["NatrollEXE.OutfitReactions/ReactionActive"] = "1";
				}
				else if (!flag && flag2)
				{
					((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).Remove("NatrollEXE.OutfitReactions/ReactionActive");
				}
			}
		}

		internal void QueueAiConnectionTestFromConfigMenu()
		{
			outfitAiService?.QueueConnectionTestFromConfigMenu();
		}

		public override void Entry(IModHelper helper)
		{
			Instance = this;
			Config = helper.ReadConfig<ModConfig>();
			Config.MigrateLegacyAiSettings();
			outfitAiService = new OutfitAiService(helper, ((Mod)this).Monitor, () => Config);
			outfitAiService.IsRomanceableNpc = IsNpcRomanceable;
			outfitMemoryService = new OutfitMemoryService(helper, ((Mod)this).Monitor);
			hatMemoryService = new HatMemoryService(helper, ((Mod)this).Monitor);
			outfitVisionService = new OutfitVisionService(((Mod)this).Monitor);
			fashionSenseVisualService = new FashionSenseVisualService(((Mod)this).Monitor, () => fsApi);
			specialHatReactionService = new SpecialHatReactionService(helper, ((Mod)this).Monitor);
			specialItemReactionService = new SpecialItemReactionService(helper, ((Mod)this).Monitor);
			otherNpcClothesReactionSystem = new OtherNpcClothesReactionSystem(((Mod)this).Monitor, () => Config, TryQueueOtherNpcOutfitDialogue, RefreshOtherNpcOutfitPrompt, ClearOutfitPrompt, HasNoticeableCurrentFashionSenseAppearance, CanNpcNoticeCurrentOutfitNotice, MarkCurrentOutfitAsNoticed, CanNpcReactToCurrentOutfitNotice, HasNpcSeenCurrentVisualBefore);
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
			helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
			helper.Events.Display.MenuChanged += OnMenuChanged;
			helper.Events.Display.RenderedHud += OnRenderedHud;
			helper.Events.Input.ButtonPressed += OnButtonPressed;
			helper.Events.Player.Warped += OnWarped;
			helper.Events.Content.AssetRequested += OnAssetRequested;
			helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
			helper.Events.GameLoop.Saving += OnSaving;
			helper.ConsoleCommands.Add("oc_debug_notice", "Outfit Compliments: print why the current outfit notice can/can't start for nearby NPCs.", (Action<string, string[]>)DebugOutfitNoticeCommand);
			helper.ConsoleCommands.Add("oc_clear_notice_memory", "Outfit Compliments: clear this save's outfit notice memory and reset pending notice state.", (Action<string, string[]>)ClearOutfitNoticeMemoryCommand);
			helper.ConsoleCommands.Add("oc_test_voicesamples", "Outfit Reactions: report how many real in-game voice-sample lines each NPC profile has (run after loading a save).", (Action<string, string[]>)VoiceSampleReportCommand);
			helper.ConsoleCommands.Add("oc_preview_voicesamples", "Outfit Reactions: show the exact voice-sample lines that would be injected into the prompt for ONE NPC. Usage: oc_preview_voicesamples <NpcName>", (Action<string, string[]>)VoiceSamplePreviewCommand);
			ApplyHarmonyPatches();
		}

		private void ApplyHarmonyPatches()
		{
			try
			{
				MethodBase methodBase = AccessTools.Method(typeof(NPC), "checkAction", new Type[2]
				{
					typeof(Farmer),
					typeof(GameLocation)
				});
				if (methodBase == null)
				{
					((Mod)this).Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction target method was NOT found. Patch was not applied.", (LogLevel)3);
					return;
				}
				harmony = new Harmony(((Mod)this).ModManifest.UniqueID);
				harmony.PatchAll(typeof(ModEntry).Assembly);
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES PRIORITY] NPC.checkAction Harmony patch applied.", (LogLevel)2);
				}
			}
			catch (Exception ex)
			{
				((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Failed to apply NPC.checkAction patch: " + ex, (LogLevel)3);
			}
		}

		internal bool PrioritizeOutfitDialogueBeforeNpcCheckAction(NPC npc)
		{
			if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
			{
				return false;
			}
			if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (Game1.eventUp)
			{
				return false;
			}
			if (TryPrioritizeSpouseOutfitDialogueForClick(npc))
			{
				return true;
			}
			return otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npc) ?? false;
		}

		internal bool TryHandleOutfitDialogueOrBlockNpcInteraction(NPC npc)
		{
			return SpouseOutfitReactionCoordinator.TryHandleInteraction(npc);
		}

		private bool TryHandleOutfitDialogueOrBlockNpcInteractionCore(NPC npc)
		{
			if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
			{
				return false;
			}
			if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (Game1.eventUp)
			{
				return false;
			}
			if (((Character)Game1.player).modData != null && ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).ContainsKey("NatrollEXE.LotsOfKisses/AutoKissClickActive"))
			{
				return false;
			}
			if (TryOpenPrioritizedOutfitDialogueFromCheckAction(npc))
			{
				return true;
			}
			if (ShouldBlockNpcInteractionUntilOutfitDialogueRead(npc))
			{
				ShowPendingOutfitBlockedInteractionFeedback(npc);
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Blocked normal interaction/kiss with " + ((Character)npc).Name + " because an unread outfit dialogue is pending.", (LogLevel)2);
				}
				return true;
			}
			return false;
		}

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			fsApi = ((Mod)this).Helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");
			((Mod)this).Monitor.Log((fsApi != null) ? "Fashion Sense API loaded successfully." : "Fashion Sense API not found. Outfit compliments will not detect clothing changes.", (LogLevel)((fsApi != null) ? 1 : 3));
			outfitAiService?.LoadProfiles();
			try
			{
				IGenericModConfigMenuApi api = ((Mod)this).Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
				ModConfigMenu.Register(this, api);
			}
			catch (Exception ex)
			{
				((Mod)this).Monitor.Log("Failed to register GMCM options: " + ex.Message, (LogLevel)0);
			}
		}

		private void OnSaving(object sender, SavingEventArgs e)
		{
			outfitMemoryService?.Save();
			hatMemoryService?.Save();
		}

		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			outfitMemoryService?.Load();
			hatMemoryService?.Load();
			specialItemReactionService?.ResetModRegistryCache();
			vanillaClothingTrackingInitialized = false;
			lastKnownVanillaHatId = null;
			lastKnownVanillaPantsName = null;
			ResetClothesState(clearChangeFlag: true);
			otherNpcClothesReactionSystem?.Reset();
			outfitAiService?.LoadProfiles(quiet: true);
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			ResetClothesState(clearChangeFlag: true);
			otherNpcClothesReactionSystem?.Reset();
			Farmer player = Game1.player;
			if (player != null)
			{
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)player).modData)?.Remove("NatrollEXE.OutfitReactions/ReactionActive");
			}
			outfitAiService?.LoadProfiles(quiet: true);
		}

		private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			CancelAllPendingOwnAiGenerations();
			ResetClothesState(clearChangeFlag: true);
			otherNpcClothesReactionSystem?.Reset();
		}

		private void OnRenderedHud(object sender, RenderedHudEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
			{
				return;
			}
			foreach (PendingAiGeneration item in aiGenerationCoordinator.GetOutfitSnapshot())
			{
				if (item != null && item.Task != null && !item.Task.IsCompleted)
				{
					NPC characterFromName = Game1.getCharacterFromName(item.NpcName, true, false);
					if (characterFromName != null && ((Character)characterFromName).currentLocation == ((Character)Game1.player).currentLocation)
					{
						DrawOwnAiWaitingHudMessage(e.SpriteBatch, characterFromName, GetOwnAiWaitingDialogueText(characterFromName, item.WaitingDotCount));
						return;
					}
				}
			}
			foreach (PendingAiPlayerReplyGeneration item2 in aiGenerationCoordinator.GetReplySnapshot())
			{
				if (item2 != null && item2.Task != null && !item2.Task.IsCompleted)
				{
					NPC characterFromName2 = Game1.getCharacterFromName(item2.NpcName, true, false);
					if (characterFromName2 != null && ((Character)characterFromName2).currentLocation == ((Character)Game1.player).currentLocation)
					{
						DrawOwnAiWaitingHudMessage(e.SpriteBatch, characterFromName2, GetOwnAiReplyWaitingDialogueText(characterFromName2, item2.WaitingDotCount));
						break;
					}
				}
			}
		}

		private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (Context.IsWorldReady && Game1.player != null && Game1.currentLocation != null && Config.Enabled && Game1.activeClickableMenu == null && !Game1.eventUp && (SButtonExtensions.IsActionButton(e.Button) || SButtonExtensions.IsUseToolButton(e.Button)))
			{
				NPC npcBeingInteractedWith = GetNpcBeingInteractedWith();
				if (npcBeingInteractedWith != null && !TryPrioritizeSpouseOutfitDialogueForClick(npcBeingInteractedWith))
				{
					otherNpcClothesReactionSystem?.TryPrioritizePendingDialogueForClick(npcBeingInteractedWith);
				}
			}
		}

		private NPC GetNpcBeingInteractedWith()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (Game1.player == null || Game1.currentLocation == null)
			{
				return null;
			}
			Vector2 grabTile = ((Character)Game1.player).GetGrabTile();
			NPC val = ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>().FirstOrDefault((NPC c) => c != null && !c.IsInvisible && ((Character)c).TilePoint.X == (int)grabTile.X && ((Character)c).TilePoint.Y == (int)grabTile.Y);
			if (val != null)
			{
				return val;
			}
			int mouseTileX = (Game1.getOldMouseX() + ((Rectangle)(ref Game1.viewport)).X) / 64;
			int mouseTileY = (Game1.getOldMouseY() + ((Rectangle)(ref Game1.viewport)).Y) / 64;
			val = (from c in ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>()
				where c != null && !c.IsInvisible && ((Character)c).TilePoint.X == mouseTileX && ((Character)c).TilePoint.Y == mouseTileY
				orderby Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position)
				select c).FirstOrDefault((NPC c) => Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position) <= 192f);
			if (val != null)
			{
				return val;
			}
			return (from c in ((IEnumerable)Game1.currentLocation.characters).OfType<NPC>()
				where c != null && !c.IsInvisible && ((Character)c).currentLocation == ((Character)Game1.player).currentLocation
				where Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position) <= 112f
				orderby Vector2.Distance(((Character)c).Position, ((Character)Game1.player).Position)
				select c).FirstOrDefault();
		}

		private bool TryPrioritizeSpouseOutfitDialogueForClick(NPC npc)
		{
			if (npc == null || clothesReactingNpc == null)
			{
				return false;
			}
			if (!((Character)npc).Name.Equals(((Character)clothesReactingNpc).Name, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (!clothesComplimentReady || !outfitSequenceActive)
			{
				return false;
			}
			if (lastFashionSenseChangeInfo == null)
			{
				return false;
			}
			if (!CanNpcNoticeCurrentOutfitNotice(npc))
			{
				return false;
			}
			if (!spouseDialogueController.HasBackup)
			{
				spouseDialogueController.Capture(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
			}
			else
			{
				spouseDialogueController.TemporarilySkipFirstDailyDialogue(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
			}
			bool flag = QueueSpouseOutfitDialogueOnly(npc);
			if (flag)
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Re-prioritized outfit dialogue for " + ((Character)npc).Name + " at click time.", (LogLevel)2);
				}
				else
				{
					KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "AI queue was not available on click.");
				}
			}
			return flag;
		}

		private void OnWarped(object sender, WarpedEventArgs e)
		{
			if (Context.IsWorldReady && e != null && e.IsLocalPlayer)
			{
				CancelAllPendingOwnAiGenerations();
				if (isReactingToClothes || outfitSequenceActive)
				{
					ResetClothesReactionState();
				}
				otherNpcClothesReactionSystem?.Reset();
			}
		}

		private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("Mods/NatrollEXE.OutfitReactions/NpcCharacteristics", false))
			{
				e.LoadFrom((Func<object>)(() => outfitAiService?.LoadDefaultProfilesFromFiles() ?? new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase)), (AssetLoadPriority)(-1000), (string)null);
			}
		}

		private void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
		{
			if (!Context.IsWorldReady)
			{
				return;
			}
			foreach (IAssetName item in e.NamesWithoutLocale)
			{
				string text = ((object)item).ToString();
				if (!text.StartsWith("Mods/NatrollEXE.OutfitReactions/Clothes", StringComparison.OrdinalIgnoreCase) && text.Equals("Mods/NatrollEXE.OutfitReactions/NpcCharacteristics", StringComparison.OrdinalIgnoreCase))
				{
					outfitAiService?.LoadProfiles(quiet: true);
				}
			}
		}

		private void OnMenuChanged(object sender, MenuChangedEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled)
			{
				return;
			}
			bool flag = e.NewMenu != null && IsFashionSenseMenu(e.NewMenu);
			bool flag2 = e.OldMenu != null && IsFashionSenseMenu(e.OldMenu);
			if (flag && !fashionSenseMenuOpen)
			{
				fashionSenseMenuOpen = true;
				fsSnapshotBefore = CaptureFashionSenseSnapshot();
			}
			else
			{
				if ((fashionSenseMenuOpen && flag) || !(fashionSenseMenuOpen && flag2) || e.NewMenu != null)
				{
					return;
				}
				fashionSenseMenuOpen = false;
				DelayedAction.functionAfterDelay((Action)delegate
				{
					if (Context.IsWorldReady && Game1.player != null)
					{
						FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
						FashionSenseChangeInfo fashionSenseChangeInfo = CompareFashionSenseSnapshots(fsSnapshotBefore, fashionSenseSnapshot);
						fsSnapshotBefore = null;
						lastKnownVanillaHatId = fashionSenseSnapshot?.VanillaHat ?? "";
						lastKnownVanillaPantsName = fashionSenseSnapshot?.VanillaPants ?? "";
						lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(fashionSenseSnapshot?.VanillaPantsSpecialItemCandidates);
						vanillaClothingTrackingInitialized = true;
						if (fashionSenseChangeInfo != null && fashionSenseChangeInfo.CountChanges() > 0)
						{
							ApplyDetectedClothesChange(fashionSenseChangeInfo);
						}
					}
				}, 200);
			}
		}

		private void PollVanillaHatAndPantsChange()
		{
			if (vanillaClothingPollTimer > 0)
			{
				vanillaClothingPollTimer--;
				return;
			}
			vanillaClothingPollTimer = 15;
			if (fashionSenseMenuOpen)
			{
				return;
			}
			string visibleVanillaHatId = GetVisibleVanillaHatId();
			string currentVanillaHatName = GetCurrentVanillaHatName();
			List<string> candidates = ((!string.IsNullOrWhiteSpace(visibleVanillaHatId)) ? GetCurrentVisibleVanillaHatSpecialItemCandidates(currentVanillaHatName) : new List<string>());
			string currentVanillaPantsName = GetCurrentVanillaPantsName();
			List<string> candidates2 = ((!string.IsNullOrWhiteSpace(currentVanillaPantsName)) ? GetCurrentVanillaPantsSpecialItemCandidates(currentVanillaPantsName) : new List<string>());
			if (!vanillaClothingTrackingInitialized)
			{
				lastKnownVanillaHatId = visibleVanillaHatId;
				lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
				lastKnownVanillaPantsName = currentVanillaPantsName ?? "";
				lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
				vanillaClothingTrackingInitialized = true;
				return;
			}
			bool flag = !string.Equals(visibleVanillaHatId, lastKnownVanillaHatId ?? "", StringComparison.OrdinalIgnoreCase);
			bool flag2 = !string.Equals(currentVanillaPantsName ?? "", lastKnownVanillaPantsName ?? "", StringComparison.OrdinalIgnoreCase);
			if (!flag && !flag2)
			{
				return;
			}
			FashionSenseSnapshot fashionSenseSnapshot = CaptureFashionSenseSnapshot();
			fashionSenseSnapshot.VanillaHat = lastKnownVanillaHatId ?? "";
			fashionSenseSnapshot.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaHatSpecialItemCandidates);
			fashionSenseSnapshot.VanillaPants = lastKnownVanillaPantsName ?? "";
			fashionSenseSnapshot.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(lastKnownVanillaPantsSpecialItemCandidates);
			FashionSenseSnapshot fashionSenseSnapshot2 = CaptureFashionSenseSnapshot();
			fashionSenseSnapshot2.VanillaHat = visibleVanillaHatId;
			fashionSenseSnapshot2.VanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
			fashionSenseSnapshot2.VanillaPants = currentVanillaPantsName ?? "";
			fashionSenseSnapshot2.VanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
			lastKnownVanillaHatId = visibleVanillaHatId;
			lastKnownVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(candidates);
			lastKnownVanillaPantsName = currentVanillaPantsName ?? "";
			lastKnownVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(candidates2);
			FashionSenseChangeInfo changeInfo = CompareFashionSenseSnapshots(fashionSenseSnapshot, fashionSenseSnapshot2);
			int num = changeInfo?.CountChanges() ?? 0;
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[VANILLA POLL] hatChanged={flag} (now='{visibleVanillaHatId}' was='{fashionSenseSnapshot.VanillaHat}') | pantsChanged={flag2} (now='{currentVanillaPantsName}' was='{fashionSenseSnapshot.VanillaPants}') | changeCount={num} vanillaPantsChanged={changeInfo?.VanillaPantsChanged} vanillaPantsRemoved={changeInfo?.VanillaPantsRemoved} fsPantsAfter='{fashionSenseSnapshot2.Pants}' pantsDebug={GetCurrentVanillaPantsDebugString()}", (LogLevel)2);
			}
			if (changeInfo == null || num <= 0)
			{
				return;
			}
			DelayedAction.functionAfterDelay((Action)delegate
			{
				if (Context.IsWorldReady && Game1.player != null)
				{
					ApplyDetectedClothesChange(changeInfo);
				}
			}, 200);
		}

		private void ApplyDetectedClothesChange(FashionSenseChangeInfo changeInfo)
		{
			if (string.IsNullOrEmpty(GetFashionSenseDialogueKey(changeInfo)))
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log($"[FS] Detected change had nothing describable (total={changeInfo.CountChanges()}, likely a vanilla-pants-only side effect) — ignoring, not resetting notice state.", (LogLevel)2);
				}
				return;
			}
			ResetClothesState(clearChangeFlag: true);
			npcsReactedToCurrentNotice.Clear();
			loggedSpecialItemDebugKeys.Clear();
			otherNpcClothesReactionSystem?.Reset();
			lastEligibleSavedOutfitId = "";
			lastFashionSenseChangeInfo = changeInfo;
			changedClothes = true;
			otherNpcClothesReactionSystem?.NotifyOutfitChanged();
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[FS] outfit change detected | total={changeInfo.CountChanges()} hair={changeInfo.ChangedHair} accessory={changeInfo.ChangedAccessory} hat={changeInfo.ChangedHat} vanillaHat={changeInfo.VanillaHatChanged} shirt={changeInfo.ChangedShirt} pants={changeInfo.ChangedPants} sleeves={changeInfo.ChangedSleeves} shoes={changeInfo.ChangedShoes} outfit={changeInfo.ChangedOutfit} newHair={changeInfo.NewHairId} newHat={changeInfo.NewHatId} newAccessory={changeInfo.NewAccessoryId}", (LogLevel)2);
			}
			if (changeInfo.ChangedAccessory && !AreVisionOnlyFashionSenseTriggersEnabled())
			{
				bool flag = ItemNameRevealsShape(changeInfo.NewAccessoryId);
				if (DebugLog)
				{
					((Mod)this).Monitor.Log(flag ? "[FS] Accessory changed (no vision): item name reveals its shape, so it will be noticed." : "[FS] Accessory changed (no vision): item name is too generic to describe, so it is skipped.", (LogLevel)2);
				}
			}
		}

		private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (Context.IsWorldReady && Config.Enabled)
			{
				UpdateReactionActiveModDataFlag();
				SpouseOutfitReactionCoordinator.AdvanceTimers();
				if (spouseProximityState.PendingBubbleTimer > 0)
				{
					spouseProximityState.PendingBubbleTimer--;
				}
				RefreshCurrentSavedOutfitNoticeCandidate();
				PollVanillaHatAndPantsChange();
				NPC spouse = GetSpouse();
				NPC datingNpc = GetDatingNpc();
				NPC val = spouse ?? datingNpc;
				SpouseOutfitReactionCoordinator.Update(val, spouse, changedClothes && lastFashionSenseChangeInfo != null);
				UpdatePendingOwnAiGenerations();
				UpdatePendingOwnAiPlayerReplyGenerations();
				object obj = ((val != null) ? ((Character)val).Name : null);
				if (obj == null)
				{
					Farmer player = Game1.player;
					obj = ((player != null) ? player.spouse : null);
				}
				string spouseName = (string)obj;
				otherNpcClothesReactionSystem?.Update(spouseName);
			}
		}

		private void RefreshCurrentSavedOutfitNoticeCandidate()
		{
			if (!Context.IsWorldReady || Game1.player == null || !Config.Enabled || (changedClothes && lastFashionSenseChangeInfo != null && !lastFashionSenseChangeInfo.ChangedOutfit))
			{
				return;
			}
			if (!TryGetCurrentSavedFashionSenseOutfitId(out var outfitId))
			{
				if (IsSavedOutfitNoticeChange(lastFashionSenseChangeInfo))
				{
					ResetClothesState(clearChangeFlag: true);
					otherNpcClothesReactionSystem?.Reset();
				}
				return;
			}
			if (lastFashionSenseChangeInfo != null && lastFashionSenseChangeInfo.ChangedOutfit && string.Equals(lastFashionSenseChangeInfo.NewOutfitId, outfitId, StringComparison.OrdinalIgnoreCase))
			{
				changedClothes = true;
				return;
			}
			FashionSenseChangeInfo changeInfo = new FashionSenseChangeInfo
			{
				ChangedOutfit = true,
				NewOutfitId = outfitId
			};
			if (string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(changeInfo)))
			{
				return;
			}
			if (string.Equals(lastEligibleSavedOutfitId, outfitId, StringComparison.OrdinalIgnoreCase))
			{
				changedClothes = true;
				return;
			}
			lastEligibleSavedOutfitId = outfitId;
			npcsReactedToCurrentNotice.Clear();
			lastFashionSenseChangeInfo = changeInfo;
			changedClothes = true;
			otherNpcClothesReactionSystem?.NotifyOutfitChanged();
			if (DebugLog)
			{
				((Mod)this).Monitor.Log("[CLOTHES NOTICE] Current saved outfit is eligible for outfit notices: " + outfitId, (LogLevel)2);
			}
		}

		private bool IsSavedOutfitNoticeChange(FashionSenseChangeInfo changeInfo)
		{
			return changeInfo != null && changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
		}

		private bool IsVanillaHatRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
		{
			return changeInfo != null && changeInfo.VanillaHatRemoved && changeInfo.VanillaHatChanged && changeInfo.CountChanges() == 1;
		}

		private bool IsSpecialItemRemovalOnlyNotice(FashionSenseChangeInfo changeInfo)
		{
			if (changeInfo == null || changeInfo.CountChanges() != 1)
			{
				return false;
			}
			if (!changeInfo.VanillaPantsChanged && !changeInfo.VanillaHatChanged)
			{
				return false;
			}
			SpecialItemNoticeInfo notice;
			return TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out notice) && notice != null && notice.WasRemoved;
		}

		private bool NpcRemembersRemovedSpecialItem(NPC npc, FashionSenseChangeInfo changeInfo)
		{
			SpecialItemNoticeInfo notice;
			return npc != null && changeInfo != null && TryResolveSpecialItemNoticeForNpc(npc, changeInfo, requireNpcMemoryForRemoval: false, out notice) && notice != null && notice.WasRemoved && HasSpecialItemMemory(npc, notice);
		}

		private bool NpcRemembersRemovedVanillaHat(NPC npc)
		{
			return npc != null && !string.IsNullOrWhiteSpace(hatMemoryService?.GetLastHatNameForNpc(((Character)npc).Name) ?? "");
		}

		private FashionSenseChangeInfo TryBuildCurrentSavedOutfitNoticeChange()
		{
			if (!TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) || string.IsNullOrWhiteSpace(outfitId))
			{
				return null;
			}
			FashionSenseChangeInfo fashionSenseChangeInfo = new FashionSenseChangeInfo
			{
				ChangedOutfit = true,
				NewOutfitId = outfitId
			};
			return string.IsNullOrWhiteSpace(GetFashionSenseDialogueKey(fashionSenseChangeInfo)) ? null : fashionSenseChangeInfo;
		}

		private FashionSenseChangeInfo GetEffectiveFashionSenseChangeInfoForNpc(NPC npc)
		{
			if (lastFashionSenseChangeInfo == null)
			{
				return null;
			}
			if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo) && npc != null)
			{
				if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? ""))
				{
					return null;
				}
				if (NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
				{
					return lastFashionSenseChangeInfo;
				}
				FashionSenseChangeInfo fashionSenseChangeInfo = TryBuildCurrentSavedOutfitNoticeChange();
				if (fashionSenseChangeInfo != null)
				{
					return fashionSenseChangeInfo;
				}
			}
			if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && npc != null && !NpcRemembersRemovedVanillaHat(npc))
			{
				FashionSenseChangeInfo fashionSenseChangeInfo2 = TryBuildCurrentSavedOutfitNoticeChange();
				if (fashionSenseChangeInfo2 != null)
				{
					return fashionSenseChangeInfo2;
				}
			}
			return lastFashionSenseChangeInfo;
		}

		private bool CanNpcNoticeCurrentOutfitNotice(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			return !HasNpcReactedToCurrentOutfitNotice(npc, lastFashionSenseChangeInfo?.NewOutfitId);
		}

		private bool HasNpcSeenCurrentVisualBefore(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: false, out var notice) && notice != null && notice.IsValid)
			{
				return HasSpecialItemMemory(npc, notice);
			}
			if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && outfitMemoryService != null)
			{
				string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
				if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
				{
					OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
					OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
					return memory != null && memory.TimesSeenBefore > 0;
				}
			}
			string visibleVanillaHatId = GetVisibleVanillaHatId();
			if (!string.IsNullOrWhiteSpace(visibleVanillaHatId) && hatMemoryService != null)
			{
				HatMemoryComparison memory2 = hatMemoryService.GetMemory(((Character)npc).Name, visibleVanillaHatId, GetCurrentVanillaHatName());
				return memory2 != null && memory2.TimesSeenBefore > 0;
			}
			return false;
		}

		private bool DidNpcWitnessPreviousLook(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? ""))
			{
				return true;
			}
			if (outfitMemoryService != null && lastFashionSenseChangeInfo != null)
			{
				string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(lastFashionSenseChangeInfo.NewOutfitId);
				if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
				{
					OutfitComponents current = BuildCurrentOutfitComponentsForMemory();
					OutfitMemoryComparison memory = outfitMemoryService.GetMemory(((Character)npc).Name, currentSavedFashionSenseOutfitIdForAi, current);
					if (memory != null && memory.TimesSeenBefore > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void MarkCurrentOutfitAsNoticed(NPC npc)
		{
			if (npc == null || lastFashionSenseChangeInfo == null)
			{
				return;
			}
			string item = ((Character)npc).Name ?? "";
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (effectiveFashionSenseChangeInfoForNpc == null || npcsReactedToCurrentNotice.Contains(item))
			{
				return;
			}
			SpecialItemNoticeInfo notice;
			bool flag = ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(npc, effectiveFashionSenseChangeInfoForNpc, out notice);
			bool flag2 = ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(npc);
			npcsReactedToCurrentNotice.Add(item);
			if (flag)
			{
				RecordSpecialItemMemory(npc, notice);
				if (DebugLog)
				{
					((Mod)this).Monitor.Log($"[SPECIAL ITEM MEMORY] {((Character)npc).Name} reacted to special item '{notice?.EntryId}'; saved outfit memory was not updated for this item-focused reaction.", (LogLevel)2);
				}
				if (notice != null && notice.WasRemoved)
				{
					if (DebugLog)
					{
						((Mod)this).Monitor.Log("[SPECIAL ITEM MEMORY] Special item '" + notice.EntryId + "' was a removal reaction; clearing the notice so it does not repeat.", (LogLevel)2);
					}
					changedClothes = false;
					lastFashionSenseChangeInfo = null;
					if (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) && !string.IsNullOrWhiteSpace(outfitId))
					{
						lastEligibleSavedOutfitId = outfitId;
					}
				}
			}
			else if (flag2)
			{
				RecordVanillaHatMemory(npc);
				string currentVanillaPantsName = GetCurrentVanillaPantsName();
				if (!string.IsNullOrWhiteSpace(currentVanillaPantsName))
				{
					RecordVanillaPantsMemory(npc, currentVanillaPantsName);
				}
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[HAT MEMORY] " + ((Character)npc).Name + " reacted to a vanilla-hat focused notice; saved outfit memory was not updated for this hat-focused reaction.", (LogLevel)2);
				}
			}
			else if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc))
			{
				string currentSavedFashionSenseOutfitIdForAi = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
				if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi))
				{
					RecordOutfitMemory(npc, currentSavedFashionSenseOutfitIdForAi);
				}
				if (DebugLog)
				{
					((Mod)this).Monitor.Log($"[CLOTHES NOTICE] Recorded that {((Character)npc).Name} reacted to outfit '{currentSavedFashionSenseOutfitIdForAi}'.", (LogLevel)2);
				}
			}
			else if (IsImmediateFashionSenseNoticeChange(effectiveFashionSenseChangeInfoForNpc))
			{
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[FS] " + ((Character)npc).Name + " reacted to the immediate change; it stays available for other NPCs.", (LogLevel)2);
				}
				if (effectiveFashionSenseChangeInfoForNpc.VanillaHatChanged || !string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
				{
					RecordVanillaHatMemory(npc);
				}
				string currentVanillaPantsName2 = GetCurrentVanillaPantsName();
				if (!string.IsNullOrWhiteSpace(currentVanillaPantsName2))
				{
					RecordVanillaPantsMemory(npc, currentVanillaPantsName2);
				}
				string currentSavedFashionSenseOutfitIdForAi2 = GetCurrentSavedFashionSenseOutfitIdForAi(effectiveFashionSenseChangeInfoForNpc.NewOutfitId);
				if (!string.IsNullOrWhiteSpace(currentSavedFashionSenseOutfitIdForAi2))
				{
					RecordOutfitMemory(npc, currentSavedFashionSenseOutfitIdForAi2);
				}
			}
		}

		private bool ShouldRecordCurrentNoticeAsSpecialItemOnlyReaction(NPC npc, FashionSenseChangeInfo effectiveChangeInfo, out SpecialItemNoticeInfo notice)
		{
			notice = null;
			if (npc == null || effectiveChangeInfo == null)
			{
				return false;
			}
			if (!TryResolveSpecialItemNoticeForNpc(npc, effectiveChangeInfo, requireNpcMemoryForRemoval: true, out notice))
			{
				return false;
			}
			return notice != null && notice.IsValid;
		}

		private bool ShouldRecordCurrentNoticeAsVanillaHatOnlyReaction(NPC npc)
		{
			if (lastFashionSenseChangeInfo == null)
			{
				return false;
			}
			if (ModConfigMenu.NormalizeVanillaHatReactionMode(Config?.VanillaHatReactionMode) != "HatOnly")
			{
				return false;
			}
			if (!string.IsNullOrWhiteSpace(GetVisibleVanillaHatId()))
			{
				return true;
			}
			return IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && NpcRemembersRemovedVanillaHat(npc);
		}

		private static bool IsImmediateFashionSenseNoticeChange(FashionSenseChangeInfo changeInfo)
		{
			return changeInfo != null && !IsSavedOutfitNoticeChangeStatic(changeInfo) && (changeInfo.ChangedHair || changeInfo.ChangedHat || changeInfo.ChangedAccessory);
		}

		private static bool IsSavedOutfitNoticeChangeStatic(FashionSenseChangeInfo changeInfo)
		{
			return changeInfo != null && changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId);
		}

		private bool HasNpcReactedToCurrentOutfitNotice(NPC npc, string outfitId)
		{
			if (npc == null)
			{
				return false;
			}
			return npcsReactedToCurrentNotice.Contains(((Character)npc).Name ?? "");
		}

		private static string MakeSafeModDataPart(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "unknown";
			}
			string text = NormalizeOutfitText(value);
			StringBuilder stringBuilder = new StringBuilder();
			string text2 = text;
			foreach (char c in text2)
			{
				if (char.IsLetterOrDigit(c))
				{
					stringBuilder.Append(c);
				}
				else if (c == '_' || c == '-')
				{
					stringBuilder.Append(c);
				}
			}
			return (stringBuilder.Length > 0) ? stringBuilder.ToString() : "unknown";
		}

		private static string GetStableHexHash(string value)
		{
			uint num = 2166136261u;
			string text = value ?? "";
			string text2 = text;
			foreach (char c in text2)
			{
				num ^= c;
				num *= 16777619;
			}
			return num.ToString("x8", CultureInfo.InvariantCulture);
		}

		private static bool IsNpcFacingPlayer(NPC npc)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			Vector2 standingPosition = ((Character)npc).getStandingPosition();
			Vector2 standingPosition2 = ((Character)Game1.player).getStandingPosition();
			Vector2 val = standingPosition2 - standingPosition;
			if (((Vector2)(ref val)).LengthSquared() < 256f)
			{
				return true;
			}
			int facingDirection = ((Character)npc).FacingDirection;
			if (1 == 0)
			{
			}
			bool result = facingDirection switch
			{
				0 => val.Y < 0f, 
				1 => val.X > 0f, 
				2 => val.Y > 0f, 
				3 => val.X < 0f, 
				_ => true, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private float DistanceToPlayer(NPC npc)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return float.MaxValue;
			}
			return Vector2.Distance(((Character)npc).Position, ((Character)Game1.player).Position);
		}

		private bool CanNpcReactToCurrentOutfitNotice(NPC npc)
		{
			return CanNpcReactToOutfit(npc) && ShouldStartClothesReaction(npc);
		}

		private bool IsNpcWatchingAsKissBystander(NPC npc)
		{
			return ((npc != null) ? ((Character)npc).modData : null) != null && ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)npc).modData).ContainsKey("NatrollEXE.LotsOfKisses/BystanderWatching");
		}

		private bool CanNpcReactToOutfit(NPC npc)
		{
			if (npc == null || string.IsNullOrWhiteSpace(((Character)npc).Name))
			{
				return false;
			}
			if (npcsReactedToCurrentNotice.Contains(((Character)npc).Name))
			{
				return false;
			}
			if (IsNpcWatchingAsKissBystander(npc))
			{
				return false;
			}
			return outfitAiService?.HasProfile(((Character)npc).Name) ?? false;
		}

		private bool HasMinimumFriendshipForOutfitReaction(NPC npc)
		{
			return npc != null;
		}

		private int GetNpcPortraitCount(NPC npc)
		{
			try
			{
				if (((npc != null) ? npc.Portrait : null) == null)
				{
					return 0;
				}
				int num = Math.Max(1, npc.Portrait.Width / 64);
				int num2 = Math.Max(1, npc.Portrait.Height / 64);
				return num * num2;
			}
			catch
			{
				return 0;
			}
		}

		private bool HasNoticeableCurrentFashionSenseAppearance()
		{
			return ShouldStartClothesReaction();
		}

		private FashionSenseSnapshot CaptureFashionSenseSnapshot()
		{
			if (Game1.player == null)
			{
				return null;
			}
			string hat = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHat.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hat));
			bool flag = IsFashionSenseHatCoveringVanilla();
			string text = (IsFashionSensePantsCoveringVanilla() ? "" : (GetCurrentVanillaPantsName() ?? ""));
			List<string> vanillaPantsSpecialItemCandidates = ((!string.IsNullOrWhiteSpace(text)) ? GetCurrentVanillaPantsSpecialItemCandidates(text) : new List<string>());
			FashionSenseSnapshot fashionSenseSnapshot = new FashionSenseSnapshot();
			fashionSenseSnapshot.Hair = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomHair.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Hair));
			fashionSenseSnapshot.Accessory = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Id"), GetFsModData("FashionSense.CustomAccessory.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Accessory)));
			fashionSenseSnapshot.AccessorySecondary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Id"), GetFsModData("FashionSense.CustomAccessorySecondary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessorySecondary)));
			fashionSenseSnapshot.AccessoryTertiary = NormalizeFashionSenseAccessoryId(StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Id"), GetFsModData("FashionSense.CustomAccessoryTertiary.Id"), GetFsAppearanceId(IFashionSenseApi.Type.AccessoryTertiary)));
			fashionSenseSnapshot.Hat = hat;
			fashionSenseSnapshot.FashionSenseHatCoversVanilla = flag;
			fashionSenseSnapshot.VanillaHat = (flag ? "" : GetCurrentVanillaHatId());
			fashionSenseSnapshot.VanillaHatSpecialItemCandidates = ((!flag && !string.IsNullOrWhiteSpace(GetCurrentVanillaHatName())) ? GetCurrentVisibleVanillaHatSpecialItemCandidates(GetCurrentVanillaHatName()) : new List<string>());
			fashionSenseSnapshot.VanillaPants = text;
			fashionSenseSnapshot.VanillaPantsSpecialItemCandidates = vanillaPantsSpecialItemCandidates;
			fashionSenseSnapshot.Shirt = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShirt.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shirt));
			fashionSenseSnapshot.Pants = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomPants.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Pants));
			fashionSenseSnapshot.Sleeves = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomSleeves.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Sleeves));
			fashionSenseSnapshot.Shoes = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomShoes.Id"), GetFsAppearanceId(IFashionSenseApi.Type.Shoes));
			fashionSenseSnapshot.OutfitId = (TryGetCurrentSavedFashionSenseOutfitId(out var outfitId) ? outfitId : null);
			fashionSenseSnapshot.HairColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hair"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hair));
			fashionSenseSnapshot.AccessoryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.0.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.Accessory"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Accessory));
			fashionSenseSnapshot.AccessorySecondaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.1.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessorySecondary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessorySecondary));
			fashionSenseSnapshot.AccessoryTertiaryColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.CustomAccessory.2.Color"), GetFsModData("FashionSense.UI.HandMirror.Color.AccessoryTertiary"), GetFsAppearanceColorKey(IFashionSenseApi.Type.AccessoryTertiary));
			fashionSenseSnapshot.HatColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Hat"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Hat));
			fashionSenseSnapshot.ShirtColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shirt"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shirt));
			fashionSenseSnapshot.PantsColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Pants"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Pants));
			fashionSenseSnapshot.SleevesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Sleeves"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Sleeves));
			fashionSenseSnapshot.ShoesColor = StringUtils.FirstNonEmpty(GetFsModData("FashionSense.UI.HandMirror.Color.Shoes"), GetFsAppearanceColorKey(IFashionSenseApi.Type.Shoes));
			return fashionSenseSnapshot;
		}

		private FashionSenseChangeInfo CompareFashionSenseSnapshots(FashionSenseSnapshot before, FashionSenseSnapshot after)
		{
			if (before == null || after == null)
			{
				return null;
			}
			bool flag = !string.IsNullOrWhiteSpace(after.OutfitId);
			bool flag2 = !string.IsNullOrWhiteSpace(after.Hair);
			bool flag3 = !string.IsNullOrWhiteSpace(StringUtils.FirstNonEmpty(before.Accessory, before.AccessorySecondary, before.AccessoryTertiary, after.Accessory, after.AccessorySecondary, after.AccessoryTertiary));
			bool flag4 = !IsEmptyFashionSenseValue(after.Hat) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(after.Hat);
			bool flag5 = after.FashionSenseHatCoversVanilla || flag4;
			bool flag6 = !flag5 && !string.Equals(before.VanillaHat ?? "", after.VanillaHat ?? "", StringComparison.OrdinalIgnoreCase);
			bool vanillaHatRemoved = !flag5 && !string.IsNullOrWhiteSpace(before.VanillaHat) && string.IsNullOrWhiteSpace(after.VanillaHat);
			bool flag7 = IsFashionSensePantsValueCoveringVanilla(after.Pants);
			bool vanillaPantsChanged = !flag7 && !string.Equals(before.VanillaPants ?? "", after.VanillaPants ?? "", StringComparison.OrdinalIgnoreCase);
			bool vanillaPantsRemoved = !flag7 && !string.IsNullOrWhiteSpace(before.VanillaPants) && string.IsNullOrWhiteSpace(after.VanillaPants);
			bool flag8 = before.Accessory != after.Accessory || before.AccessorySecondary != after.AccessorySecondary || before.AccessoryTertiary != after.AccessoryTertiary;
			bool flag9 = before.AccessoryColor != after.AccessoryColor || before.AccessorySecondaryColor != after.AccessorySecondaryColor || before.AccessoryTertiaryColor != after.AccessoryTertiaryColor;
			bool flag10 = flag && !string.Equals(before.OutfitId, after.OutfitId, StringComparison.OrdinalIgnoreCase);
			string changedAccessoryId = GetChangedAccessoryId(before, after, flag10);
			bool flag11 = !string.IsNullOrWhiteSpace(BuildCurrentAccessoryMemoryValue(after));
			return new FashionSenseChangeInfo
			{
				ChangedHair = (flag2 && (before.Hair != after.Hair || before.HairColor != after.HairColor)),
				ChangedAccessory = (flag10 ? flag11 : (flag8 || (flag3 && flag9))),
				ChangedHat = ((flag4 && (before.Hat != after.Hat || before.HatColor != after.HatColor)) || flag6),
				ChangedShirt = (before.Shirt != after.Shirt || before.ShirtColor != after.ShirtColor),
				ChangedPants = (before.Pants != after.Pants || before.PantsColor != after.PantsColor),
				ChangedSleeves = (before.Sleeves != after.Sleeves || before.SleevesColor != after.SleevesColor),
				ChangedShoes = (before.Shoes != after.Shoes || before.ShoesColor != after.ShoesColor),
				ChangedOutfit = flag10,
				NewHairId = after.Hair,
				NewAccessoryId = changedAccessoryId,
				NewHatId = after.Hat,
				NewVanillaHatId = after.VanillaHat,
				VanillaHatChanged = flag6,
				VanillaHatRemoved = vanillaHatRemoved,
				PreviousVanillaHatId = before.VanillaHat,
				PreviousVanillaHatSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaHatSpecialItemCandidates),
				VanillaPantsChanged = vanillaPantsChanged,
				VanillaPantsRemoved = vanillaPantsRemoved,
				PreviousVanillaPantsName = before.VanillaPants,
				NewVanillaPantsName = after.VanillaPants,
				PreviousVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(before.VanillaPantsSpecialItemCandidates),
				NewVanillaPantsSpecialItemCandidates = CloneSpecialItemCandidates(after.VanillaPantsSpecialItemCandidates),
				NewShirtId = after.Shirt,
				NewPantsId = after.Pants,
				NewSleevesId = after.Sleeves,
				NewShoesId = after.Shoes,
				NewOutfitId = after.OutfitId
			};
		}

		private static string GetChangedAccessoryId(FashionSenseSnapshot before, FashionSenseSnapshot after, bool outfitChanged)
		{
			if (before == null || after == null)
			{
				return "";
			}
			string result = BuildCurrentAccessoryMemoryValue(after);
			if (outfitChanged)
			{
				return result;
			}
			string changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.Accessory, after.Accessory, before.AccessoryColor, after.AccessoryColor, "accessory");
			if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
			{
				return changedAccessorySlotDescription;
			}
			changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.AccessorySecondary, after.AccessorySecondary, before.AccessorySecondaryColor, after.AccessorySecondaryColor, "secondary accessory");
			if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
			{
				return changedAccessorySlotDescription;
			}
			changedAccessorySlotDescription = GetChangedAccessorySlotDescription(before.AccessoryTertiary, after.AccessoryTertiary, before.AccessoryTertiaryColor, after.AccessoryTertiaryColor, "tertiary accessory");
			if (!string.IsNullOrWhiteSpace(changedAccessorySlotDescription))
			{
				return changedAccessorySlotDescription;
			}
			return result;
		}

		private static string GetChangedAccessorySlotDescription(string beforeId, string afterId, string beforeColor, string afterColor, string slotLabel)
		{
			bool flag = !string.Equals(beforeId, afterId, StringComparison.OrdinalIgnoreCase);
			bool flag2 = !string.Equals(beforeColor, afterColor, StringComparison.OrdinalIgnoreCase);
			if (!flag && !flag2)
			{
				return "";
			}
			if (!string.IsNullOrWhiteSpace(afterId))
			{
				return afterId;
			}
			if (!string.IsNullOrWhiteSpace(beforeId))
			{
				return "removed " + beforeId;
			}
			return "changed " + slotLabel;
		}

		private string GetFsModData(string key)
		{
			if (Game1.player == null)
			{
				return null;
			}
			string text = default(string);
			return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(key, ref text) ? text : null;
		}

		private string GetFsAppearanceId(IFashionSenseApi.Type type)
		{
			if (fsApi == null || Game1.player == null)
			{
				return null;
			}
			try
			{
				KeyValuePair<bool, string> currentAppearanceId = fsApi.GetCurrentAppearanceId(type, Game1.player);
				if (currentAppearanceId.Key && !string.IsNullOrWhiteSpace(currentAppearanceId.Value))
				{
					string text = currentAppearanceId.Value.Trim();
					if (!text.Equals("None", StringComparison.OrdinalIgnoreCase))
					{
						return text;
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private string GetFsAppearanceColorKey(IFashionSenseApi.Type type)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (fsApi == null || Game1.player == null)
			{
				return null;
			}
			try
			{
				KeyValuePair<bool, Color> appearanceColor = fsApi.GetAppearanceColor(type, Game1.player);
				if (!appearanceColor.Key)
				{
					return null;
				}
				Color value = appearanceColor.Value;
				return ((Color)(ref value)).R.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).G.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).B.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).A.ToString("X2", CultureInfo.InvariantCulture);
			}
			catch
			{
				return null;
			}
		}

		internal bool TryOpenPrioritizedOutfitDialogueFromCheckAction(NPC npc)
		{
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || !Config.Enabled)
			{
				return false;
			}
			if (npc == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (Game1.eventUp)
			{
				return false;
			}
			if (!IsOwnAiWaitingStateActiveFor(npc) && !PrioritizeOutfitDialogueBeforeNpcCheckAction(npc))
			{
				return false;
			}
			bool flag = IsOwnAiWaitingStateActiveFor(npc);
			if ((npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= 0) && !flag)
			{
				return false;
			}
			try
			{
				((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
				((Character)Game1.player).Halt();
				if (flag)
				{
					if (DebugLog)
					{
						((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Holding " + ((Character)npc).Name + "'s normal dialogue behind the prioritized outfit AI wait.", (LogLevel)2);
					}
					return true;
				}
				Game1.drawDialogue(npc);
				otherNpcClothesReactionSystem?.NotifyPrioritizedDialogueOpenedByHarmony(npc);
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES PRIORITY] Opened prioritized outfit dialogue for " + ((Character)npc).Name + " and skipped original NPC.checkAction.", (LogLevel)2);
				}
				return true;
			}
			catch (Exception value)
			{
				((Mod)this).Monitor.Log($"[CLOTHES PRIORITY] Failed to open prioritized outfit dialogue for {((Character)npc).Name}: {value}", (LogLevel)3);
				return false;
			}
		}

		private bool ShouldBlockNpcInteractionUntilOutfitDialogueRead(NPC npc)
		{
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (IsOwnAiWaitingStateActiveFor(npc))
			{
				return true;
			}
			if (IsUnreadSpouseOutfitDialoguePendingFor(npc))
			{
				return true;
			}
			return otherNpcClothesReactionSystem?.HasUnreadPendingDialogueFor(npc) ?? false;
		}

		private bool IsUnreadSpouseOutfitDialoguePendingFor(NPC npc)
		{
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			if (!IsPlayerSpouse(npc))
			{
				return false;
			}
			if (lastFashionSenseChangeInfo == null)
			{
				return false;
			}
			if (!CanNpcNoticeCurrentOutfitNotice(npc))
			{
				return false;
			}
			if (clothesReactingNpc != null && ((Character)npc).Name.Equals(((Character)clothesReactingNpc).Name, StringComparison.OrdinalIgnoreCase) && (outfitSequenceActive || isReactingToClothes))
			{
				return true;
			}
			if (outfitSequenceActive && clothesFirstNoticeDone)
			{
				return true;
			}
			return false;
		}

		private bool IsPlayerSpouse(NPC npc)
		{
			return npc != null && Game1.player != null && !string.IsNullOrWhiteSpace(Game1.player.spouse) && ((Character)npc).Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase);
		}

		private void ShowPendingOutfitBlockedInteractionFeedback(NPC npc)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return;
			}
			((Character)Game1.player).Halt();
			try
			{
				if (!((Character)npc).isMoving() && ((Character)npc).controller == null)
				{
					((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false, false);
				}
			}
			catch
			{
			}
			if (IsPlayerSpouse(npc))
			{
				ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
				UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
			}
			else
			{
				((Character)npc).doEmote(40, true);
			}
		}

		private static bool IsNpcRomanceable(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			try
			{
				if (Game1.characterData != null && Game1.characterData.TryGetValue(name, out var value) && value != null)
				{
					return value.CanBeRomanced;
				}
			}
			catch
			{
			}
			return false;
		}

		private bool IsFashionSenseMenu(IClickableMenu menu)
		{
			string text = ((object)menu)?.GetType().FullName ?? "";
			return text.Contains("FashionSense", StringComparison.OrdinalIgnoreCase);
		}

		private static string NormalizeOutfitText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string text2 = text.ToLowerInvariant().Trim().Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();
			string text3 = text2;
			foreach (char c in text3)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		private string GetFashionSenseDialogueKey(FashionSenseChangeInfo changeInfo)
		{
			if (changeInfo == null)
			{
				return null;
			}
			int num = changeInfo.CountChanges();
			if (num <= 0)
			{
				return null;
			}
			if (TryResolveSpecialItemNoticeForNpc(null, changeInfo, requireNpcMemoryForRemoval: false, out var _))
			{
				return "Clothes";
			}
			if ((changeInfo.ChangedOutfit && !string.IsNullOrWhiteSpace(changeInfo.NewOutfitId)) || ShouldTreatGenericHeadwearAsSavedOutfitPart(changeInfo))
			{
				return "Clothes";
			}
			bool flag = AreVisionOnlyFashionSenseTriggersEnabled();
			if (changeInfo.ChangedAccessory && ShouldTreatAccessoryAsCurrentComboFocus(changeInfo.NewAccessoryId, flag))
			{
				return "Accessory";
			}
			if (changeInfo.VanillaHatChanged)
			{
				return "Hat";
			}
			if (changeInfo.ChangedHat && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(changeInfo.NewHatId) && (flag || ItemNameRevealsShape(changeInfo.NewHatId)))
			{
				return "Hat";
			}
			if (changeInfo.ChangedHair && !string.IsNullOrWhiteSpace(changeInfo.NewHairId))
			{
				return "Hair";
			}
			return null;
		}

		private bool AreVisionOnlyFashionSenseTriggersEnabled()
		{
			return ShouldTryVisionForCurrentAiProvider();
		}

		private bool ItemNameRevealsShape(string itemId)
		{
			if (string.IsNullOrWhiteSpace(itemId))
			{
				return false;
			}
			if (IsIgnoredFashionSenseAccessoryId(itemId))
			{
				return false;
			}
			if (FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(itemId))
			{
				return false;
			}
			string text = FashionSenseVisualService.HumanizeAppearanceId(itemId);
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string[] array = text.Split(' ');
			foreach (string text2 in array)
			{
				string text3 = text2.Trim('\'', '"', '.', ',', '(', ')');
				if (text3.Length < 3)
				{
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				string text4 = text3;
				foreach (char c in text4)
				{
					if (char.IsDigit(c))
					{
						flag = true;
					}
					else if (char.IsLetter(c))
					{
						flag2 = true;
					}
				}
				if (flag2 && !flag)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsFarmHouseLocation(GameLocation location)
		{
			if (location == null)
			{
				return false;
			}
			string text = location.Name ?? "";
			string text2 = location.NameOrUniqueName ?? "";
			string text3 = ((object)location).GetType().Name ?? "";
			string text4 = ((object)location).GetType().FullName ?? "";
			return text.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text2.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text3.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase) || text.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text3.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0 || text4.IndexOf("FarmHouse", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private bool IsBeachOrIslandLocation(GameLocation location)
		{
			if (location == null)
			{
				return false;
			}
			string text = location.Name ?? "";
			string text2 = location.NameOrUniqueName ?? "";
			return text.Equals("Beach", StringComparison.OrdinalIgnoreCase) || text2.Equals("Beach", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Island", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("Island", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsMarriageCandidateNpcRoom(NPC npc, GameLocation location)
		{
			if (npc == null || location == null)
			{
				return false;
			}
			if (!IsMarriageCandidate(npc))
			{
				return false;
			}
			string npcName = NormalizeOutfitText(((Character)npc).Name);
			string displayName = NormalizeOutfitText(((Character)npc).displayName);
			string text = NormalizeOutfitText(location.Name + " " + location.NameOrUniqueName);
			if (LooksLikeNpcRoomText(text) && TextMentionsNpc(text, npcName, displayName))
			{
				return true;
			}
			return MapPropertiesSuggestNpcRoom(location, npcName, displayName);
		}

		private bool IsMarriageCandidatePersonalLocation(NPC npc, GameLocation location)
		{
			if (npc == null || location == null || !IsMarriageCandidate(npc))
			{
				return false;
			}
			if (location.IsOutdoors || IsFarmHouseLocation(location))
			{
				return false;
			}
			string npcName = NormalizeOutfitText(((Character)npc).Name);
			string displayName = NormalizeOutfitText(((Character)npc).displayName);
			string text = NormalizeOutfitText(location.Name + " " + location.NameOrUniqueName);
			if (TextMentionsNpc(text, npcName, displayName))
			{
				return true;
			}
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
			dictionary["Abigail"] = new string[3] { "seedshop", "pierres", "pierre" };
			dictionary["Alex"] = new string[2] { "joshhouse", "alexhouse" };
			dictionary["Elliott"] = new string[2] { "elliotthouse", "elliottcabin" };
			dictionary["Emily"] = new string[2] { "haleyhouse", "emilyhouse" };
			dictionary["Haley"] = new string[1] { "haleyhouse" };
			dictionary["Harvey"] = new string[3] { "harveyroom", "harveyclinic", "hospital" };
			dictionary["Leah"] = new string[2] { "leahhouse", "leahcottage" };
			dictionary["Maru"] = new string[2] { "sciencehouse", "robinhouse" };
			dictionary["Penny"] = new string[1] { "trailer" };
			dictionary["Sam"] = new string[1] { "samhouse" };
			dictionary["Sebastian"] = new string[4] { "sciencehouse", "sebastianbasement", "sebastianroom", "robinhouse" };
			dictionary["Shane"] = new string[3] { "animalshop", "marnieranch", "ranch" };
			Dictionary<string, string[]> dictionary2 = dictionary;
			if (dictionary2.TryGetValue(((Character)npc).Name ?? "", out var value))
			{
				string[] array = value;
				foreach (string text2 in array)
				{
					if (!string.IsNullOrWhiteSpace(text2) && text.Contains(NormalizeOutfitText(text2)))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsMarriageCandidate(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			try
			{
				object obj = ((object)npc).GetType().GetField("datable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(npc) ?? ((object)npc).GetType().GetProperty("datable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(npc);
				if (obj == null)
				{
					return false;
				}
				object obj2 = obj.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
				bool flag = default(bool);
				int num;
				if (obj2 is bool)
				{
					flag = (bool)obj2;
					num = 1;
				}
				else
				{
					num = 0;
				}
				return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
			}
			catch
			{
				return false;
			}
		}

		private bool MapPropertiesSuggestNpcRoom(GameLocation location, string npcName, string displayName)
		{
			try
			{
				object obj;
				if (location == null)
				{
					obj = null;
				}
				else
				{
					Map map = location.map;
					obj = ((map != null) ? ((Component)map).Properties : null);
				}
				if (obj == null)
				{
					return false;
				}
				foreach (KeyValuePair<string, PropertyValue> item in (IEnumerable<KeyValuePair<string, PropertyValue>>)((Component)location.map).Properties)
				{
					object obj2 = item;
					string text = "";
					string text2 = "";
					if (obj2 is DictionaryEntry dictionaryEntry)
					{
						text = dictionaryEntry.Key?.ToString() ?? "";
						text2 = dictionaryEntry.Value?.ToString() ?? "";
					}
					else if (obj2 != null)
					{
						Type type = obj2.GetType();
						text = type.GetProperty("Key")?.GetValue(obj2)?.ToString() ?? "";
						text2 = type.GetProperty("Value")?.GetValue(obj2)?.ToString() ?? "";
					}
					string text3 = NormalizeOutfitText(text);
					string text4 = NormalizeOutfitText(text2);
					string text5 = text3 + " " + text4;
					if (LooksLikeNpcRoomText(text5) && TextMentionsNpc(text5, npcName, displayName))
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		private bool LooksLikeNpcRoomText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			return text.Contains("room") || text.Contains("bedroom") || text.Contains("bed room") || text.Contains("npcroom") || text.Contains("npc room") || text.Contains("quarto") || text.Contains("suite") || text.Contains("basement") || text.Contains("cellar") || text.Contains("porão") || text.Contains("porao");
		}

		private bool TextMentionsNpc(string text, string npcName, string displayName)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			return (!string.IsNullOrWhiteSpace(npcName) && text.Contains(npcName)) || (!string.IsNullOrWhiteSpace(displayName) && text.Contains(displayName));
		}

		private bool TryShowOwnAiOutfitDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
		{
			return TryQueueOwnAiWaitingDialogue(npc, isSpouseDialogue, clearExistingDialogue);
		}

		private bool CanUseOwnAiForOutfitDialogue(NPC npc)
		{
			if (outfitAiService == null || npc == null || lastFashionSenseChangeInfo == null)
			{
				return false;
			}
			return outfitAiService.HasProfile(((Character)npc).Name);
		}

		private bool ShouldUseDeferredOwnAiForNpc(NPC npc)
		{
			return CanUseOwnAiForOutfitDialogue(npc);
		}

		private bool TryQueueOwnAiWaitingDialogue(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
		{
			if (!CanUseOwnAiForOutfitDialogue(npc))
			{
				return false;
			}
			OutfitAiContext context = BuildOutfitAiContext(npc, isSpouseDialogue);
			if (context == null)
			{
				return false;
			}
			outfitAiService.PrepareVoiceSamplesForNpc(((Character)npc).Name);
			if (clearExistingDialogue)
			{
				npc.CurrentDialogue.Clear();
			}
			Game1.activeClickableMenu = null;
			Game1.afterDialogues = null;
			if (!aiGenerationCoordinator.TryGetOutfit(((Character)npc).Name, out var pending) || pending == null || pending.Task == null || pending.Task.IsCompleted)
			{
				pending = new PendingAiGeneration
				{
					NpcName = ((Character)npc).Name,
					IsSpouseDialogue = isSpouseDialogue,
					ClearExistingDialogue = clearExistingDialogue,
					WaitingDotCount = 1,
					WaitingDotTimer = 30,
					SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
					Cancellation = new CancellationTokenSource()
				};
				aiGenerationCoordinator.StartOutfit(pending, delegate(CancellationToken cancellationToken)
				{
					try
					{
						string dialogue;
						return outfitAiService.TryGenerateCompliment(context, out dialogue, cancellationToken) ? dialogue : null;
					}
					catch (OperationCanceledException)
					{
						return (string)null;
					}
					catch (Exception ex2)
					{
						((Mod)this).Monitor.Log(" Background outfit generation crashed: " + ex2.Message, (LogLevel)3);
						return (string)null;
					}
				});
				if (DebugLog)
				{
					((Mod)this).Monitor.Log(" Started background outfit compliment generation for " + ((Character)npc).Name + ". HUD waiting message is active.", (LogLevel)2);
				}
			}
			else
			{
				((Mod)this).Monitor.Log(" " + ((Character)npc).Name + " already has a background outfit compliment generation in progress.", (LogLevel)0);
			}
			return true;
		}

		private int GetActiveAiTimeoutSecondsForSafety()
		{
			string text = Config?.GetActiveProvider() ?? "DeepSeek";
			if (1 == 0)
			{
			}
			int num = text switch
			{
				"Gemini" => Config.GeminiAiTimeoutSeconds, 
				"OpenAI" => Config.OpenAiAiTimeoutSeconds, 
				"OpenRouter" => Config.OpenRouterAiTimeoutSeconds, 
				"Mistral" => Config.MistralAiTimeoutSeconds, 
				"Groq" => Config.GroqAiTimeoutSeconds, 
				"Together" => Config.TogetherAiTimeoutSeconds, 
				"Local" => Config.LocalAiTimeoutSeconds, 
				_ => Config.DeepSeekAiTimeoutSeconds, 
			};
			if (1 == 0)
			{
			}
			int value = num;
			return Math.Clamp(value, 3, 120);
		}

		private bool IsOwnAiWaitingStateActiveFor(NPC npc)
		{
			PendingAiGeneration pending;
			return npc != null && aiGenerationCoordinator.TryGetOutfit(((Character)npc).Name, out pending) && pending != null && pending.Task != null && !pending.Task.IsCompleted;
		}

		private string GetOwnAiWaitingDialogueText(NPC npc, int dotCount)
		{
			int count = Math.Clamp(dotCount, 1, 3);
			string text = new string('.', count);
			string name = ((!string.IsNullOrWhiteSpace((npc != null) ? ((Character)npc).displayName : null)) ? ((Character)npc).displayName : (((npc != null) ? ((Character)npc).Name : null) ?? "NPC"));
			return ((object)((Mod)this).Helper.Translation.Get("hud.npc-noticing", (object)new { name })).ToString() + text;
		}

		private string GetOwnAiReplyWaitingDialogueText(NPC npc, int dotCount)
		{
			int count = Math.Clamp(dotCount, 1, 3);
			string text = new string('.', count);
			string name = ((!string.IsNullOrWhiteSpace((npc != null) ? ((Character)npc).displayName : null)) ? ((Character)npc).displayName : (((npc != null) ? ((Character)npc).Name : null) ?? "NPC"));
			return ((object)((Mod)this).Helper.Translation.Get("hud.npc-thinking", (object)new { name })).ToString() + text;
		}

		private void DrawOwnAiWaitingHudMessage(SpriteBatch spriteBatch, NPC npc, string text)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			if (spriteBatch != null && npc != null && !string.IsNullOrWhiteSpace(text) && Game1.smallFont != null)
			{
				Vector2 val = Game1.smallFont.MeasureString(text);
				Vector2 val2 = default(Vector2);
				((Vector2)(ref val2))..ctor(32f, Math.Max(32f, (float)((Rectangle)(ref Game1.uiViewport)).Height - val.Y - 72f));
				Rectangle val3 = default(Rectangle);
				((Rectangle)(ref val3))..ctor((int)val2.X - 16, (int)val2.Y - 10, (int)val.X + 32, (int)val.Y + 20);
				spriteBatch.Draw(Game1.staminaRect, val3, Color.Black * 0.55f);
				spriteBatch.DrawString(Game1.smallFont, text, val2 + new Vector2(2f, 2f), Color.Black * 0.75f);
				spriteBatch.DrawString(Game1.smallFont, text, val2, Color.White);
			}
		}

		private void UpdatePendingOwnAiGenerations()
		{
			if (!aiGenerationCoordinator.HasOutfitGenerations)
			{
				return;
			}
			foreach (string outfitNpcName in aiGenerationCoordinator.GetOutfitNpcNames())
			{
				if (!aiGenerationCoordinator.TryGetOutfit(outfitNpcName, out var pending))
				{
					continue;
				}
				NPC characterFromName = Game1.getCharacterFromName(outfitNpcName, true, false);
				if (pending == null || characterFromName == null || pending.Task == null)
				{
					aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
					continue;
				}
				switch (AiDialogueLifecycle.Advance(pending))
				{
				case AiGenerationLifecycleState.Completed:
					if (!pending.CompletionHandled)
					{
						pending.CompletionHandled = true;
						string generated = null;
						try
						{
							if (pending.Task.Status == TaskStatus.RanToCompletion)
							{
								generated = pending.Task.Result;
							}
						}
						catch (Exception ex)
						{
							((Mod)this).Monitor.Log(" Could not read background AI result: " + ex.Message, (LogLevel)3);
						}
						OpenGeneratedOrFallbackOutfitDialogue(characterFromName, pending, generated);
					}
					aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
					break;
				case AiGenerationLifecycleState.TimedOut:
					((Mod)this).Monitor.Log(" Background generation for " + outfitNpcName + " exceeded the safety timer. Removing pending waiting state.", (LogLevel)3);
					AiRequestLifecycle.Cancel(pending.Cancellation);
					if (pending.IsSpouseDialogue && characterFromName != null)
					{
						ResetClothesState();
						aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
					}
					else if (characterFromName != null)
					{
						otherNpcClothesReactionSystem?.CancelPendingOwnAiGeneration(characterFromName);
						aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
					}
					else
					{
						aiGenerationCoordinator.RemoveOutfit(outfitNpcName);
					}
					break;
				default:
					UpdateOwnAiWaitingVisual(characterFromName, pending);
					break;
				}
			}
		}

		private void UpdateOwnAiWaitingVisual(NPC npc, PendingAiGeneration pending)
		{
			if (npc == null || pending == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return;
			}
			if (pending.WaitingDotTimer > 0)
			{
				pending.WaitingDotTimer--;
				return;
			}
			pending.WaitingDotTimer = 30;
			pending.WaitingDotCount++;
			if (pending.WaitingDotCount > 3)
			{
				pending.WaitingDotCount = 1;
			}
		}

		private void OpenGeneratedOrFallbackOutfitDialogue(NPC npc, PendingAiGeneration pending, string generated)
		{
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Expected O, but got Unknown
			//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || pending == null)
			{
				return;
			}
			if (Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				((Mod)this).Monitor.Log(" AI outfit compliment for " + pending.NpcName + " finished, but the player is no longer nearby. Discarding it.", (LogLevel)0);
				return;
			}
			bool flag = false;
			bool flag2 = false;
			string text = null;
			if (!string.IsNullOrWhiteSpace(generated))
			{
				if (generated.StartsWith("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}", StringComparison.Ordinal))
				{
					flag2 = true;
					generated = generated.Substring("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}".Length).Trim();
				}
				npc.CurrentDialogue.Clear();
				string text2 = (pending.IsSpouseDialogue ? "OutfitReactions_SpouseOwnAiOutfitReaction" : "OutfitReactions_GlobalOwnAiOutfitReaction");
				npc.CurrentDialogue.Push(new Dialogue(npc, text2, generated));
				text = generated;
				flag = true;
				if (DebugLog)
				{
					((Mod)this).Monitor.Log(" Background outfit compliment for " + ((Character)npc).Name + " is ready and queued.", (LogLevel)2);
				}
			}
			else
			{
				((Mod)this).Monitor.Log(" Background outfit generation did not produce a usable line for " + ((Character)npc).Name + ". Trying configured fallbacks.", (LogLevel)3);
				flag = TryQueueNonAiOutfitFallback(npc, pending.IsSpouseDialogue, clearExistingDialogue: true);
			}
			if (!flag || npc.CurrentDialogue.Count <= 0)
			{
				Game1.activeClickableMenu = null;
				if (pending.IsSpouseDialogue)
				{
					KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "background AI generation did not produce a usable line.");
				}
				else
				{
					otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueFailed(npc);
				}
				return;
			}
			Action onFinished = null;
			if (pending.IsSpouseDialogue)
			{
				onFinished = delegate
				{
					CompleteSpouseAfterOutfitDialogue(npc);
				};
			}
			bool flag3 = false;
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc, requireNpcMemoryForRemoval: true, out var notice) && notice != null && notice.WasRemoved)
			{
				flag3 = true;
			}
			bool flag4 = false;
			string secretId = null;
			if (!flag3 && !string.IsNullOrWhiteSpace(text) && !flag2 && Config.EnablePlayerReplyMenuAfterOutfitCompliment && specialItemReactionService != null)
			{
				FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc2 = GetEffectiveFashionSenseChangeInfoForNpc(npc);
				if (TryResolveSpecialItemNoticeForNpc(npc, effectiveFashionSenseChangeInfoForNpc2, requireNpcMemoryForRemoval: false, out var notice2) && notice2 != null && notice2.HasSecret && !notice2.WasRemoved && !specialItemReactionService.NpcAlreadyKnowsSecret(notice2.SecretId, ((Character)npc).Name))
				{
					flag4 = true;
					secretId = notice2.SecretId;
				}
			}
			if (!string.IsNullOrWhiteSpace(text) && flag2 && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
			{
				InstallAccessoryClarificationInputAfterOutfitDialogue(npc, pending.IsSpouseDialogue, text, onFinished);
			}
			else if (flag4)
			{
				InstallSecretRevealChoiceMenu(npc, pending.IsSpouseDialogue, text, secretId, onFinished);
			}
			else if (!flag3 && !string.IsNullOrWhiteSpace(text) && Config.EnablePlayerReplyMenuAfterOutfitCompliment)
			{
				InstallPlayerReplyMenuAfterOutfitDialogue(npc, pending.IsSpouseDialogue, text, onFinished);
			}
			else if (pending.IsSpouseDialogue)
			{
				InstallSpouseAfterOutfitDialogue(npc);
			}
			if (!pending.IsSpouseDialogue)
			{
				otherNpcClothesReactionSystem?.NotifyOwnAiFinalDialogueOpened(npc);
			}
			if (!pending.IsSpouseDialogue)
			{
				MarkCurrentOutfitAsNoticed(npc);
			}
			Game1.activeClickableMenu = null;
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
			Game1.drawDialogue(npc);
		}

		private bool TryQueueNonAiOutfitFallback(NPC npc, bool isSpouseDialogue, bool clearExistingDialogue)
		{
			if (npc != null)
			{
				((Mod)this).Monitor.Log(" No AI outfit dialogue was queued for " + ((Character)npc).Name + ". Manual JSON outfit dialogue is disabled in this AI-only build.", (LogLevel)3);
			}
			return false;
		}

		private void InstallSpouseAfterOutfitDialogue(NPC npc)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			Game1.afterDialogues = (afterFadeFunction)delegate
			{
				CompleteSpouseAfterOutfitDialogue(npc);
			};
		}

		private void CompleteSpouseAfterOutfitDialogue(NPC npc)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			MarkCurrentOutfitAsNoticed(npc);
			ClearOutfitPrompt(npc);
			bool flag = npc != null && Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
			if (flag)
			{
				CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
				AnimatedSprite sprite = ((Character)npc).Sprite;
				if (sprite != null)
				{
					sprite.StopAnimation();
				}
				((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
				DoClothesFinalEmotes(npc);
				if (spouseRouteController.HasRoute)
				{
					spouseRouteController.Restore(npc, ((Mod)this).Monitor, DebugLog);
				}
				else
				{
					spouseRouteController.Clear();
				}
			}
			else
			{
				spouseRouteController.Clear();
			}
			spouseDialogueController.Restore(npc, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, ((Mod)this).Monitor, DebugLog);
			ResetClothesState();
			if (flag)
			{
				BeginSpousePostOutfitLinger(npc);
			}
		}

		private void BeginSpousePostOutfitLinger(NPC npc)
		{
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				ClearSpousePostOutfitLinger();
				return;
			}
			SpousePostOutfitLingerController.Begin(spouseProximityState, npc);
			CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
			SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, npc, Game1.player);
			if (DebugLog)
			{
				((Mod)this).Monitor.Log($"[CLOTHES SPOUSE] {((Character)npc).Name} will linger after the outfit compliment until distance >= {600f:F0} or {360} ticks.", (LogLevel)2);
			}
		}

		private void UpdateSpousePostOutfitLinger()
		{
			if (!spouseProximityState.LingerActive)
			{
				return;
			}
			NPC lingerNpc = spouseProximityState.LingerNpc;
			if (lingerNpc == null || Game1.player == null || !Context.IsWorldReady)
			{
				ClearSpousePostOutfitLinger();
				return;
			}
			bool flag = ((Character)lingerNpc).currentLocation == ((Character)Game1.player).currentLocation;
			float distance = (flag ? DistanceToPlayer(lingerNpc) : 600f);
			bool hasCapturedSpecialAction = spouseSpecialActionController.HasSnapshotFor(lingerNpc);
			if (!SpousePostOutfitLingerController.TickAndShouldResume(spouseProximityState, flag, distance, hasCapturedSpecialAction, 300f))
			{
				SpousePostOutfitLingerController.ApplyHoldPose(spouseProximityState, lingerNpc, Game1.player);
				return;
			}
			if (!spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog))
			{
				((Character)lingerNpc).movementPause = 0;
			}
			ClearSpousePostOutfitLinger();
		}

		private void ClearSpousePostOutfitLinger()
		{
			SpousePostOutfitLingerController.Clear(spouseProximityState);
		}

		private void InstallPlayerReplyMenuAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			OutfitReplyConversationHistory obj = outfitReplyConversationHistory;
			NPC obj2 = npc;
			obj.Start((obj2 != null) ? ((Character)obj2).Name : null, npcCompliment);
			Game1.afterDialogues = (afterFadeFunction)delegate
			{
				ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
			};
		}

		private void InstallSecretRevealChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, string secretId, Action onFinished)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			Game1.afterDialogues = (afterFadeFunction)delegate
			{
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Invalid comparison between Unknown and I4
				if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
				{
					ModEntry modEntry = this;
					Action onFinished2 = onFinished;
					NPC obj = npc;
					modEntry.FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
				}
				else
				{
					bool isPt = (int)LocalizedContentManager.CurrentLanguageCode == 4;
					string text = ((Character)npc).displayName ?? ((Character)npc).Name;
					string title = (isPt ? ("Contar o segredo a " + text + "?") : ("Tell " + text + " the secret?"));
					string replyLabel = (isPt ? "Contar" : "Tell them");
					string leaveLabel = (isPt ? "Não" : "Not now");
					Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyChoiceMenu(title, replyLabel, leaveLabel, delegate
					{
						specialItemReactionService?.RevealSecret(secretId, ((Character)npc).Name);
						OutfitAiContext outfitAiContext = BuildOutfitAiContext(npc, isSpouseDialogue);
						if (outfitAiContext == null)
						{
							ModEntry modEntry2 = this;
							Action onFinished3 = onFinished;
							NPC obj2 = npc;
							modEntry2.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
						}
						else
						{
							string text2 = specialItemReactionService?.GetSecretRevealMessage(secretId) ?? "";
							string playerReply = ((!string.IsNullOrWhiteSpace(text2)) ? text2 : (isPt ? "[O jogador contou ao NPC sobre a origem secreta do item.]" : "[The player just told the NPC about the item's secret origin.]"));
							outfitAiContext.ConversationTranscript = null;
							StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, playerReply, onFinished, outfitAiContext);
						}
					}, delegate
					{
						ModEntry modEntry2 = this;
						Action onFinished3 = onFinished;
						NPC obj2 = npc;
						modEntry2.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
					});
				}
			};
		}

		private void InstallAccessoryClarificationInputAfterOutfitDialogue(NPC npc, bool isSpouseDialogue, string npcLine, Action onFinished)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			Game1.afterDialogues = (afterFadeFunction)delegate
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				string titleOverride = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder:" : "Reply:");
				OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcLine, onFinished, titleOverride, delegate
				{
					ModEntry modEntry = this;
					Action onFinished2 = onFinished;
					NPC obj = npc;
					modEntry.FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
				}, saveAccessoryClarification: true);
			};
		}

		private void ShowPlayerReplyChoiceMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished)
		{
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Invalid comparison between Unknown and I4
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Invalid comparison between Unknown and I4
			if (!Config.EnablePlayerReplyMenuAfterOutfitCompliment || npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				Action onFinished2 = onFinished;
				NPC obj = npc;
				FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
				return;
			}
			string title = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder ao comentário?" : "Reply to the comment?");
			string replyLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Responder" : "Reply");
			string leaveLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Ir embora" : "Leave");
			Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyChoiceMenu(title, replyLabel, leaveLabel, delegate
			{
				OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
			}, delegate
			{
				ModEntry modEntry = this;
				Action onFinished3 = onFinished;
				NPC obj2 = npc;
				modEntry.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
			});
		}

		private void OpenPlayerOutfitReplyInputMenu(NPC npc, bool isSpouseDialogue, string npcCompliment, Action onFinished, string titleOverride = null, Action cancelOverride = null, bool saveAccessoryClarification = false)
		{
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Invalid comparison between Unknown and I4
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Invalid comparison between Unknown and I4
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Invalid comparison between Unknown and I4
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				Action onFinished2 = onFinished;
				NPC obj = npc;
				FinishPlayerReplyInteraction(onFinished2, (obj != null) ? ((Character)obj).Name : null);
				return;
			}
			string title = ((!string.IsNullOrWhiteSpace(titleOverride)) ? titleOverride : (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Escreva sua resposta:" : "Write your reply:"));
			string sendLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Enviar" : "Send");
			string cancelLabel = (((int)LocalizedContentManager.CurrentLanguageCode == 4) ? "Cancelar" : "Cancel");
			Game1.activeClickableMenu = (IClickableMenu)(object)new OutfitPlayerReplyTextInputMenu(title, sendLabel, cancelLabel, delegate(string replyText)
			{
				string text = CleanPlayerOutfitReplyText(replyText);
				if (string.IsNullOrWhiteSpace(text))
				{
					OpenPlayerOutfitReplyInputMenu(npc, isSpouseDialogue, npcCompliment, onFinished, titleOverride, cancelOverride, saveAccessoryClarification);
				}
				else
				{
					if (saveAccessoryClarification)
					{
						SavePlayerProvidedAccessoryDescriptionForCurrentChange(text);
					}
					if (!CanUseOwnAiForOutfitDialogue(npc))
					{
						ModEntry modEntry = this;
						Action onFinished3 = onFinished;
						NPC obj2 = npc;
						modEntry.FinishPlayerReplyInteraction(onFinished3, (obj2 != null) ? ((Character)obj2).Name : null);
					}
					else
					{
						OutfitReplyConversationHistory obj3 = outfitReplyConversationHistory;
						NPC obj4 = npc;
						obj3.Append((obj4 != null) ? ((Character)obj4).Name : null, "Player", text);
						StartPlayerReplyFollowUpGeneration(npc, isSpouseDialogue, npcCompliment, text, onFinished);
					}
				}
			}, delegate
			{
				if (cancelOverride != null)
				{
					cancelOverride();
				}
				else
				{
					ShowPlayerReplyChoiceMenu(npc, isSpouseDialogue, npcCompliment, onFinished);
				}
			});
		}

		private static string CleanPlayerOutfitReplyText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			text = Regex.Replace(text, "\\s+", " ").Trim();
			if (text.Length > 800)
			{
				text = text.Substring(0, 800).Trim();
			}
			return text;
		}

		private void StartPlayerReplyFollowUpGeneration(NPC npc, bool isSpouseDialogue, string npcCompliment, string playerReply, Action onFinished, OutfitAiContext prebuiltContext = null)
		{
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				FinishPlayerReplyInteraction(onFinished, (npc != null) ? ((Character)npc).Name : null);
				return;
			}
			OutfitAiContext context = prebuiltContext ?? BuildOutfitAiContext(npc, isSpouseDialogue);
			if (context == null)
			{
				FinishPlayerReplyInteraction(onFinished, (npc != null) ? ((Character)npc).Name : null);
				return;
			}
			if (prebuiltContext == null)
			{
				context.ConversationTranscript = outfitReplyConversationHistory.BuildTranscript(((Character)npc).Name);
			}
			Game1.activeClickableMenu = null;
			Game1.afterDialogues = null;
			PendingAiPlayerReplyGeneration pending = new PendingAiPlayerReplyGeneration
			{
				NpcName = ((Character)npc).Name,
				IsSpouseDialogue = isSpouseDialogue,
				NpcCompliment = (npcCompliment ?? ""),
				PlayerReply = (playerReply ?? ""),
				WaitingDotCount = 1,
				WaitingDotTimer = 30,
				SafetyTimer = Math.Max(600, GetActiveAiTimeoutSecondsForSafety() * 120),
				Cancellation = new CancellationTokenSource(),
				OnFinished = onFinished
			};
			aiGenerationCoordinator.StartReply(pending, delegate(CancellationToken cancellationToken)
			{
				try
				{
					string dialogue;
					return outfitAiService.TryGenerateFollowUp(context, pending.NpcCompliment, pending.PlayerReply, out dialogue, cancellationToken) ? dialogue : null;
				}
				catch (OperationCanceledException)
				{
					return (string)null;
				}
				catch (Exception ex2)
				{
					((Mod)this).Monitor.Log(" Background player-reply follow-up crashed: " + ex2.Message, (LogLevel)3);
					return (string)null;
				}
			});
			if (DebugLog)
			{
				((Mod)this).Monitor.Log(" Started background player-reply follow-up generation for " + ((Character)npc).Name + ".", (LogLevel)2);
			}
		}

		private void UpdatePendingOwnAiPlayerReplyGenerations()
		{
			if (!aiGenerationCoordinator.HasReplyGenerations)
			{
				return;
			}
			foreach (string replyNpcName in aiGenerationCoordinator.GetReplyNpcNames())
			{
				if (!aiGenerationCoordinator.TryGetReply(replyNpcName, out var pending))
				{
					continue;
				}
				NPC characterFromName = Game1.getCharacterFromName(replyNpcName, true, false);
				if (pending == null || characterFromName == null || pending.Task == null)
				{
					FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
					aiGenerationCoordinator.RemoveReply(replyNpcName);
					continue;
				}
				switch (AiDialogueLifecycle.Advance(pending))
				{
				case AiGenerationLifecycleState.Completed:
					if (!pending.CompletionHandled)
					{
						pending.CompletionHandled = true;
						string generated = null;
						try
						{
							if (pending.Task.Status == TaskStatus.RanToCompletion)
							{
								generated = pending.Task.Result;
							}
						}
						catch (Exception ex)
						{
							((Mod)this).Monitor.Log(" Could not read player-reply follow-up result: " + ex.Message, (LogLevel)3);
						}
						OpenGeneratedPlayerReplyFollowUp(characterFromName, pending, generated);
					}
					aiGenerationCoordinator.RemoveReply(replyNpcName);
					break;
				case AiGenerationLifecycleState.TimedOut:
					((Mod)this).Monitor.Log(" Player-reply follow-up generation for " + replyNpcName + " exceeded the safety timer.", (LogLevel)3);
					AiRequestLifecycle.Cancel(pending.Cancellation);
					FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
					aiGenerationCoordinator.RemoveReply(replyNpcName);
					break;
				default:
					UpdateOwnAiPlayerReplyWaitingVisual(pending);
					break;
				}
			}
		}

		private void UpdateOwnAiPlayerReplyWaitingVisual(PendingAiPlayerReplyGeneration pending)
		{
			if (pending == null)
			{
				return;
			}
			if (pending.WaitingDotTimer > 0)
			{
				pending.WaitingDotTimer--;
				return;
			}
			pending.WaitingDotTimer = 30;
			pending.WaitingDotCount++;
			if (pending.WaitingDotCount > 3)
			{
				pending.WaitingDotCount = 1;
			}
		}

		private void OpenGeneratedPlayerReplyFollowUp(NPC npc, PendingAiPlayerReplyGeneration pending, string generated)
		{
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Expected O, but got Unknown
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Expected O, but got Unknown
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || pending == null)
			{
				FinishPlayerReplyInteraction(pending?.OnFinished, pending?.NpcName);
				return;
			}
			if (Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || string.IsNullOrWhiteSpace(generated))
			{
				FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
				return;
			}
			if (generated.StartsWith("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}", StringComparison.Ordinal))
			{
				generated = generated.Substring("{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}".Length).Trim();
			}
			if (string.IsNullOrWhiteSpace(generated))
			{
				FinishPlayerReplyInteraction(pending.OnFinished, pending.NpcName);
				return;
			}
			npc.CurrentDialogue.Clear();
			string text = (pending.IsSpouseDialogue ? "OutfitReactions_SpousePlayerReplyFollowUp" : "OutfitReactions_GlobalPlayerReplyFollowUp");
			npc.CurrentDialogue.Push(new Dialogue(npc, text, generated));
			outfitReplyConversationHistory.Append(pending.NpcName, "NPC", generated);
			Game1.activeClickableMenu = null;
			Game1.afterDialogues = (afterFadeFunction)delegate
			{
				ShowPlayerReplyChoiceMenu(npc, pending.IsSpouseDialogue, generated, pending.OnFinished);
			};
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
			Game1.drawDialogue(npc);
		}

		private void CancelAllPendingOwnAiGenerations()
		{
			IReadOnlyList<PendingAiPlayerReplyGeneration> readOnlyList = aiGenerationCoordinator.CancelAll();
			foreach (PendingAiPlayerReplyGeneration item in readOnlyList)
			{
				FinishPlayerReplyInteraction(item?.OnFinished, item?.NpcName);
			}
		}

		private void FinishPlayerReplyInteraction(Action onFinished, string npcName = null)
		{
			outfitReplyConversationHistory.Reset(npcName);
			Game1.activeClickableMenu = null;
			Game1.afterDialogues = null;
			onFinished?.Invoke();
		}

		private bool TryQueueOtherNpcOutfitDialogue(NPC npc)
		{
			if (!Config.EnableNpcOutfitReactions || npc == null)
			{
				return false;
			}
			if (TryShowOwnAiOutfitDialogue(npc, isSpouseDialogue: false, clearExistingDialogue: false))
			{
				return true;
			}
			((Mod)this).Monitor.Log(" No AI outfit dialogue was queued for " + ((Character)npc).Name + ". Manual JSON outfit dialogue is disabled in this AI-only build.", (LogLevel)3);
			return false;
		}

		private bool RefreshOtherNpcOutfitPrompt(NPC npc)
		{
			return npc != null;
		}

		private void ClearOutfitPrompt(NPC npc)
		{
		}

		private NPC GetSpouse()
		{
			if (!Context.IsWorldReady || Game1.player == null || string.IsNullOrWhiteSpace(Game1.player.spouse))
			{
				return null;
			}
			NPC characterFromName = Game1.getCharacterFromName(Game1.player.spouse, true, false);
			return CanNpcReactToOutfit(characterFromName) ? characterFromName : null;
		}

		private NPC GetDatingNpc()
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			if (!Context.IsWorldReady || Game1.player?.friendshipData == null)
			{
				return null;
			}
			string value = Game1.player.spouse ?? "";
			Enumerator<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>> enumerator = ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).Pairs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, Friendship> current = enumerator.Current;
					string key = current.Key;
					if (!string.IsNullOrWhiteSpace(key) && (string.IsNullOrWhiteSpace(value) || !key.Equals(value, StringComparison.OrdinalIgnoreCase)) && IsDatingOrEngagedFriendship(current.Value))
					{
						NPC characterFromName = Game1.getCharacterFromName(key, true, false);
						if (characterFromName != null && CanNpcReactToOutfit(characterFromName))
						{
							return characterFromName;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			return null;
		}

		private (string Status, int Hearts) GetRelationshipDialogueContext(NPC npc)
		{
			string item = "Friend";
			int item2 = 0;
			if (npc == null || Game1.player == null)
			{
				return (Status: item, Hearts: item2);
			}
			Friendship val = null;
			Friendship val2 = default(Friendship);
			if (Game1.player.friendshipData != null && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).TryGetValue(((Character)npc).Name, ref val2))
			{
				val = val2;
				if (val != null)
				{
					item2 = Math.Max(0, Math.Min(14, val.Points / 250));
				}
			}
			if (!string.IsNullOrWhiteSpace(Game1.player.spouse) && ((Character)npc).Name.Equals(Game1.player.spouse, StringComparison.OrdinalIgnoreCase))
			{
				item = "Spouse";
			}
			else if (IsDatingOrEngagedFriendship(val))
			{
				item = "Dating";
			}
			return (Status: item, Hearts: item2);
		}

		private bool IsDatingOrEngagedFriendship(Friendship friendship)
		{
			if (friendship == null)
			{
				return false;
			}
			try
			{
				Type type = ((object)friendship).GetType();
				string[] array = new string[2] { "IsDating", "IsEngaged" };
				bool flag = default(bool);
				foreach (string name in array)
				{
					MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
					int num;
					if (method != null && method.ReturnType == typeof(bool))
					{
						object obj = method.Invoke(friendship, null);
						if (obj is bool)
						{
							flag = (bool)obj;
							num = 1;
						}
						else
						{
							num = 0;
						}
					}
					else
					{
						num = 0;
					}
					if (((uint)num & (flag ? 1u : 0u)) != 0)
					{
						return true;
					}
				}
				string text = (type.GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship) ?? type.GetField("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(friendship))?.ToString() ?? "";
				return text.Contains("Dating", StringComparison.OrdinalIgnoreCase) || text.Contains("Engaged", StringComparison.OrdinalIgnoreCase) || text.Contains("Fiance", StringComparison.OrdinalIgnoreCase) || text.Contains("Fiancé", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private IEnumerable<int> GetRelationshipHeartThresholds(string status, int hearts)
		{
			int[] thresholds = ((!string.Equals(status, "Spouse", StringComparison.OrdinalIgnoreCase)) ? new int[6] { 10, 8, 6, 5, 4, 2 } : new int[7] { 14, 12, 10, 8, 6, 4, 2 });
			int[] array = thresholds;
			foreach (int threshold in array)
			{
				if (hearts >= threshold)
				{
					yield return threshold;
				}
			}
		}

		private bool ShouldStartClothesReaction(NPC npc = null)
		{
			if (!changedClothes || lastFashionSenseChangeInfo == null)
			{
				return false;
			}
			FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
			if (effectiveFashionSenseChangeInfoForNpc == null)
			{
				return false;
			}
			if (npc != null && IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && !CanNpcNoticeCurrentOutfitNotice(npc))
			{
				return false;
			}
			if (npc != null && IsSpecialItemRemovalOnlyNotice(effectiveFashionSenseChangeInfoForNpc))
			{
				if (!NpcRemembersRemovedSpecialItem(npc, effectiveFashionSenseChangeInfoForNpc))
				{
					return false;
				}
			}
			else if (npc != null && IsVanillaHatRemovalOnlyNotice(effectiveFashionSenseChangeInfoForNpc) && !NpcRemembersRemovedVanillaHat(npc))
			{
				return false;
			}
			string fashionSenseDialogueKey = GetFashionSenseDialogueKey(effectiveFashionSenseChangeInfoForNpc);
			return !string.IsNullOrEmpty(fashionSenseDialogueKey);
		}

		private void UpdateClothesReactionSystem(NPC npc)
		{
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			if (changedClothes && !isReactingToClothes)
			{
				playerWasInClothesNoticeRange = false;
			}
			if (npc == null || !Context.IsWorldReady || Game1.player == null)
			{
				return;
			}
			float num = DistanceToPlayer(npc);
			bool flag = num < (float)Config.OutfitNoticeDistance && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
			bool flag2 = ShouldStartClothesReaction(npc);
			bool flag3 = spouseOutfitApproachController.ShouldApproach(npc);
			if (changedClothes && !isReactingToClothes && !flag2)
			{
				FashionSenseChangeInfo effectiveFashionSenseChangeInfoForNpc = GetEffectiveFashionSenseChangeInfoForNpc(npc);
				if (IsSavedOutfitNoticeChange(effectiveFashionSenseChangeInfoForNpc) && !CanNpcNoticeCurrentOutfitNotice(npc))
				{
					playerWasInClothesNoticeRange = flag;
				}
				else if (IsVanillaHatRemovalOnlyNotice(lastFashionSenseChangeInfo) && !NpcRemembersRemovedVanillaHat(npc))
				{
					playerWasInClothesNoticeRange = flag;
				}
				else if (IsSpecialItemRemovalOnlyNotice(lastFashionSenseChangeInfo) && !NpcRemembersRemovedSpecialItem(npc, lastFashionSenseChangeInfo))
				{
					playerWasInClothesNoticeRange = flag;
				}
				else
				{
					ResetClothesState();
				}
				return;
			}
			if (outfitSequenceActive && !isReactingToClothes && clothesFirstNoticeDone && (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || num > (float)Config.OutfitCancelDistance))
			{
				ResetClothesReactionState();
			}
			if (Config.Enabled && flag2 && !((NetFieldBase<bool, NetBool>)(object)npc.isSleeping).Value && !isReactingToClothes)
			{
				if (!clothesFirstNoticeDone && flag && IsNpcFacingPlayer(npc))
				{
					spouseOutfitReactionProgressState.BeginFirstNotice();
					if (flag3)
					{
						spouseRouteController.Stop(npc, ((Mod)this).Monitor, DebugLog);
					}
					ShowSpousePendingOutfitBubbleIfNeeded(npc, force: true);
					UpdateSpouseOutfitNoticeHold(npc, num);
				}
				if (clothesFirstNoticeDone && !isReactingToClothes && clothesNoticePauseTimer <= 0 && flag && clothesSecondNoticeCooldown <= 0)
				{
					outfitSequenceActive = true;
					if (flag3)
					{
						spouseRouteController.Stop(npc, ((Mod)this).Monitor, DebugLog);
						((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
						spouseOutfitReactionProgressState.BeginApproach(npc);
					}
					else
					{
						spouseOutfitReactionProgressState.BeginClickReady(npc);
						if (DebugLog)
						{
							((Mod)this).Monitor.Log("[CLOTHES SPOUSE] " + ((Character)npc).Name + "'s outfit compliment is ready on click without pathing because they are outside the farmhouse.", (LogLevel)2);
						}
						ShowSpousePendingOutfitBubbleIfNeeded(npc);
						UpdateSpouseOutfitNoticeHold(npc, num);
					}
					clothesSecondNoticeCooldown = 300;
				}
			}
			if (isReactingToClothes && clothesReactingNpc == npc)
			{
				outfitSequenceActive = true;
				UpdateSpouseOutfitNoticeHold(npc, num);
				if (clothesComplimentReady)
				{
					ShowSpousePendingOutfitBubbleIfNeeded(npc);
				}
				if (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || num > (float)Config.OutfitCancelDistance)
				{
					ResetClothesState();
					return;
				}
				if (!clothesComplimentReady)
				{
					if (num <= 140f || clothesChaseTimer <= 0)
					{
						ShowOutfitCompliment(npc, flag);
						return;
					}
					if (((Character)npc).controller == null)
					{
						if (spouseOutfitApproachController.TryStartPath(npc, ((Mod)this).Monitor, DebugLog))
						{
							clothesPathStarted = true;
						}
						else
						{
							if (DebugLog)
							{
								((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Could not find an approach path for " + ((Character)npc).Name + " inside the farmhouse; making the outfit compliment ready on click.", (LogLevel)2);
							}
							clothesComplimentReady = true;
						}
						if (clothesInteractionCooldown <= 0)
						{
							clothesInteractionCooldown = 180;
						}
					}
					playerWasInClothesNoticeRange = flag;
					return;
				}
			}
			playerWasInClothesNoticeRange = flag;
		}

		private void UpdateSpouseOutfitNoticeHold(NPC npc, float distance)
		{
			spouseOutfitNoticeController.UpdateHold(spouseProximityState, npc, Game1.player, distance, CaptureSpouseOutfitSpecialActionBeforeOutfit);
		}

		private void ShowSpousePendingOutfitBubbleIfNeeded(NPC npc, bool force = false)
		{
			if (spouseOutfitNoticeController.TryShowPendingBubble(spouseProximityState, npc, Game1.player, force, clothesEmoteFired, Config.OutfitNoticeDistance, Game1.activeClickableMenu != null || Game1.eventUp, DistanceToPlayer))
			{
				clothesEmoteFired = true;
			}
		}

		private void ShowOutfitCompliment(NPC npc, bool inClothesNoticeRange)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			UpdateSpouseOutfitNoticeHold(npc, DistanceToPlayer(npc));
			CaptureSpouseOutfitSpecialActionBeforeOutfit(npc);
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false);
			spouseDialogueController.Capture(npc, Game1.player, ((Mod)this).Monitor, DebugLog);
			if (!ShouldUseDeferredOwnAiForNpc(npc))
			{
				if (!QueueSpouseOutfitDialogueOnly(npc))
				{
					KeepSpouseOutfitNoticePendingAfterAiFailure(npc, "AI queue was not available during the spouse outfit reaction.");
					return;
				}
				InstallSpouseAfterOutfitDialogue(npc);
			}
			else
			{
				((Mod)this).Monitor.Log(" " + ((Character)npc).Name + "'s spouse outfit compliment is waiting for player click before AI generation starts.", (LogLevel)1);
			}
			spouseOutfitReactionProgressState.MarkComplimentStarted(npc, inClothesNoticeRange);
		}

		private void KeepSpouseOutfitNoticePendingAfterAiFailure(NPC npc, string reason = null)
		{
			if (npc != null)
			{
				spouseDialogueController.RestoreNormalDialogueAfterAiFailure(npc, ClearOutfitPrompt, delegate(NPC npcToRestore)
				{
					spouseDialogueController.Restore(npcToRestore, Game1.player, restoreTalkState: true, clearCurrentDialogue: false, ((Mod)this).Monitor, DebugLog);
				}, ((Mod)this).Monitor, DebugLog);
				spouseOutfitReactionProgressState.KeepPendingAfterAiFailure(npc);
				if (Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation)
				{
					float num = DistanceToPlayer(npc);
					ShowSpousePendingOutfitBubbleIfNeeded(npc);
					UpdateSpouseOutfitNoticeHold(npc, num);
					playerWasInClothesNoticeRange = num < (float)Config.OutfitNoticeDistance;
				}
				string text = (string.IsNullOrWhiteSpace(reason) ? "" : (" Reason: " + reason));
				if (DebugLog)
				{
					((Mod)this).Monitor.Log("[CLOTHES SPOUSE] Outfit AI failed for " + ((Character)npc).Name + ", but the outfit was NOT marked as read. The current notice will stay pending until click retry or distance cancel." + text, (LogLevel)2);
				}
			}
		}

		private bool QueueSpouseOutfitDialogueOnly(NPC npc)
		{
			return spouseDialogueController.TryQueueOwnAiDialogue(npc, (NPC npcToQueue) => TryShowOwnAiOutfitDialogue(npcToQueue, isSpouseDialogue: true, clearExistingDialogue: false), ((Mod)this).Monitor);
		}

		private void RestoreSpouseDialogueBackupIfPending()
		{
			if (spouseDialogueController.HasBackup)
			{
				NPC characterFromName = Game1.getCharacterFromName(spouseDialogueController.Snapshot.NpcName, true, false);
				if (characterFromName == null)
				{
					spouseDialogueController.Clear();
					return;
				}
				ClearOutfitPrompt(characterFromName);
				spouseDialogueController.Restore(characterFromName, Game1.player, restoreTalkState: true, clearCurrentDialogue: true, ((Mod)this).Monitor, DebugLog);
			}
		}

		private int TryGetAnimationFrameIndex(AnimationFrame frame)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				object obj = frame;
				FieldInfo field = obj.GetType().GetField("frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null && field.GetValue(obj) is int result)
				{
					return result;
				}
				PropertyInfo property = obj.GetType().GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null && property.GetValue(obj) is int result2)
				{
					return result2;
				}
			}
			catch
			{
			}
			return -1;
		}

		private bool AnimationLooksLikeSpecialAction(List<AnimationFrame> animation)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (animation == null || animation.Count <= 0)
			{
				return false;
			}
			foreach (AnimationFrame item in animation)
			{
				int num = TryGetAnimationFrameIndex(item);
				if (num >= 16)
				{
					return true;
				}
			}
			return false;
		}

		private void CaptureSpouseOutfitSpecialActionBeforeOutfit(NPC npc)
		{
			if (npc == null || ((Character)npc).Sprite == null || ((Character)npc).currentLocation == null || spouseSpecialActionController.HasSnapshotFor(npc) || ((Character)npc).isMoving())
			{
				return;
			}
			List<AnimationFrame> list = null;
			if (((Character)npc).Sprite.CurrentAnimation != null && ((Character)npc).Sprite.CurrentAnimation.Count > 0)
			{
				list = new List<AnimationFrame>(((Character)npc).Sprite.CurrentAnimation);
			}
			bool flag = list != null && list.Count > 0;
			bool flag2 = ((Character)npc).Sprite.CurrentFrame >= 16;
			if (flag || flag2)
			{
				spouseSpecialActionController.Capture(new SpouseOutfitSpecialActionSnapshot
				{
					Npc = npc,
					Location = ((Character)npc).currentLocation,
					FacingDirection = ((Character)npc).FacingDirection,
					CurrentFrame = ((Character)npc).Sprite.CurrentFrame,
					Flip = ((Character)npc).flip,
					MovementPause = ((Character)npc).movementPause,
					AddedSpeed = (int)((Character)npc).addedSpeed,
					CurrentAnimation = list
				});
				((Character)npc).Sprite.StopAnimation();
				((Character)npc).Sprite.ClearAnimation();
				((Character)npc).Sprite.CurrentAnimation = null;
				((Character)npc).flip = false;
				((Character)npc).Sprite.CurrentFrame = GetNpcIdleFrameForDirection(((Character)npc).FacingDirection);
				((Character)npc).Sprite.UpdateSourceRect();
				if (DebugLog)
				{
					((Mod)this).Monitor.Log($"[CLOTHES SPOUSE] Saved special animation for {((Character)npc).Name} before outfit reaction. frame={spouseSpecialActionController.Current.CurrentFrame} anim={list?.Count ?? 0}", (LogLevel)2);
				}
			}
		}

		private int GetNpcIdleFrameForDirection(int facingDirection)
		{
			return facingDirection switch
			{
				0 => 8, 
				1 => 4, 
				2 => 0, 
				3 => 12, 
				_ => 0, 
			};
		}

		private void DoClothesFinalEmotes(NPC npc)
		{
			if (npc != null && Game1.player != null)
			{
				int[] array = new int[2] { 20, 60 };
				((Character)npc).doEmote(array[random.Next(array.Length)], true);
				Game1.player.doEmote(array[random.Next(array.Length)]);
			}
		}

		private void ResetClothesReactionState()
		{
			spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog);
			spouseOutfitReactionProgressState.ClearCurrentReaction();
			spouseProximityState.ClearNotice();
		}

		private void ResetClothesState(bool clearChangeFlag = false)
		{
			RestoreSpouseDialogueBackupIfPending();
			spouseRouteController.Clear();
			spouseSpecialActionController.TryRestore(force: true, Game1.player, Game1.activeClickableMenu != null, Game1.dialogueUp, DistanceToPlayer, 300f, ((Mod)this).Monitor, DebugLog);
			ClearSpousePostOutfitLinger();
			spouseOutfitReactionProgressState.ClearAllProgress();
			spouseProximityState.ClearNotice();
			fashionSenseMenuOpen = false;
			fsSnapshotBefore = null;
			CancelAllPendingOwnAiGenerations();
			if (clearChangeFlag)
			{
				changedClothes = false;
				lastFashionSenseChangeInfo = null;
			}
		}

		private string GetCurrentVanillaHatId()
		{
			try
			{
				Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
				if (val == null)
				{
					return "";
				}
				return StringUtils.FirstNonEmpty(((Item)val).ItemId, ((Item)val).Name) ?? "";
			}
			catch
			{
				return "";
			}
		}

		private static bool IsEmptyFashionSenseValue(string value)
		{
			return string.IsNullOrWhiteSpace(value) || value.Trim().Equals("None", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsFashionSenseHatCoveringVanilla()
		{
			string fsModData = GetFsModData("FashionSense.CustomHat.Id");
			if (!IsEmptyFashionSenseValue(fsModData))
			{
				return true;
			}
			string fsAppearanceId = GetFsAppearanceId(IFashionSenseApi.Type.Hat);
			return !IsEmptyFashionSenseValue(fsAppearanceId) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(fsAppearanceId);
		}

		private bool IsFashionSensePantsCoveringVanilla()
		{
			string fsModData = GetFsModData("FashionSense.CustomPants.Id");
			if (IsFashionSensePantsValueCoveringVanilla(fsModData))
			{
				return true;
			}
			string fsAppearanceId = GetFsAppearanceId(IFashionSenseApi.Type.Pants);
			return IsFashionSensePantsValueCoveringVanilla(fsAppearanceId);
		}

		private static bool IsFashionSensePantsValueCoveringVanilla(string value)
		{
			return !IsEmptyFashionSenseValue(value) && !FashionSenseVisualService.IsUnhelpfulInternalAppearanceId(value);
		}

		private string GetVisibleVanillaHatId()
		{
			if (IsFashionSenseHatCoveringVanilla())
			{
				return "";
			}
			return GetCurrentVanillaHatId();
		}

		private string GetCurrentVanillaHatName()
		{
			try
			{
				Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
				if (val == null)
				{
					return "";
				}
				return StringUtils.FirstNonEmpty(((Item)val).DisplayName, ((Item)val).Name) ?? "";
			}
			catch
			{
				return "";
			}
		}

		private string BuildVanillaHatMemoryContext(NPC npc)
		{
			if (hatMemoryService == null || npc == null)
			{
				return null;
			}
			string visibleVanillaHatId = GetVisibleVanillaHatId();
			if (string.IsNullOrWhiteSpace(visibleVanillaHatId))
			{
				return null;
			}
			string currentVanillaHatName = GetCurrentVanillaHatName();
			HatMemoryComparison memory = hatMemoryService.GetMemory(((Character)npc).Name, visibleVanillaHatId, currentVanillaHatName);
			if (memory == null)
			{
				return null;
			}
			return hatMemoryService.BuildMemoryContextHint(memory, GetCurrentGameLanguageForPrompt());
		}

		private void RecordVanillaHatMemory(NPC npc)
		{
			if (hatMemoryService != null && npc != null && !IsFashionSenseHatCoveringVanilla())
			{
				hatMemoryService.RecordMemory(((Character)npc).Name, GetCurrentVanillaHatId(), GetCurrentVanillaHatName(), Game1.currentSeason, Game1.dayOfMonth, Game1.year);
			}
		}

		private void RecordVanillaPantsMemory(NPC npc, string pantsName)
		{
			if (npc == null || string.IsNullOrWhiteSpace(pantsName) || Game1.player == null)
			{
				return;
			}
			try
			{
				NetRef<Clothing> pantsItem = Game1.player.pantsItem;
				object obj;
				if (pantsItem == null)
				{
					obj = null;
				}
				else
				{
					Clothing value = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)pantsItem).Value;
					obj = ((value != null) ? ((Item)value).ItemId : null);
				}
				if (obj == null)
				{
					obj = "";
				}
				string text = (string)obj;
				if (!string.IsNullOrWhiteSpace(text))
				{
					string text2 = "NatrollEXE.OutfitReactions/PantsSeen/" + ((Character)npc).Name + "/" + text;
					int result = 0;
					string s = default(string);
					if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, ref s))
					{
						int.TryParse(s, out result);
					}
					((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[text2] = (result + 1).ToString();
				}
			}
			catch
			{
			}
		}

		private int GetVanillaPantsSeenCount(NPC npc)
		{
			if (npc == null || Game1.player == null)
			{
				return 0;
			}
			try
			{
				NetRef<Clothing> pantsItem = Game1.player.pantsItem;
				object obj;
				if (pantsItem == null)
				{
					obj = null;
				}
				else
				{
					Clothing value = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)pantsItem).Value;
					obj = ((value != null) ? ((Item)value).ItemId : null);
				}
				if (obj == null)
				{
					obj = "";
				}
				string text = (string)obj;
				if (string.IsNullOrWhiteSpace(text))
				{
					return 0;
				}
				string text2 = "NatrollEXE.OutfitReactions/PantsSeen/" + ((Character)npc).Name + "/" + text;
				string s = default(string);
				if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(text2, ref s) && int.TryParse(s, out var result))
				{
					return result;
				}
				return 0;
			}
			catch
			{
				return 0;
			}
		}

		private string BuildVanillaPantsMemoryContext(NPC npc, string pantsName)
		{
			if (npc == null || string.IsNullOrWhiteSpace(pantsName))
			{
				return "";
			}
			int vanillaPantsSeenCount = GetVanillaPantsSeenCount(npc);
			if (vanillaPantsSeenCount <= 0)
			{
				return "";
			}
			string currentGameLanguageForPrompt = GetCurrentGameLanguageForPrompt();
			bool flag = currentGameLanguageForPrompt.Contains("pt", StringComparison.OrdinalIgnoreCase);
			return (vanillaPantsSeenCount != 1) ? (flag ? $"Este NPC já viu a(o) jogadora(o) usando {pantsName} antes ({vanillaPantsSeenCount} vezes). Devem reconhecer como algo já visto." : $"This NPC has seen the farmer wear {pantsName} before ({vanillaPantsSeenCount} times). They should recognize it as something they've seen.") : (flag ? ("Este NPC já viu a(o) jogadora(o) usando " + pantsName + " antes (1 vez). Pode reconhecer com familiaridade.") : ("This NPC has seen the farmer wear " + pantsName + " before (1 time). They may recognize it with familiarity."));
		}

		private string GetCurrentVanillaPantsName()
		{
			try
			{
				if (IsFashionSensePantsCoveringVanilla())
				{
					return "";
				}
				Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
				if (val == null)
				{
					return "";
				}
				return StringUtils.FirstNonEmpty(((Item)val).DisplayName, ((Item)val).Name) ?? "";
			}
			catch
			{
				return "";
			}
		}

		private string GetCurrentVanillaPantsDebugString()
		{
			try
			{
				Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
				if (val == null)
				{
					return "pantsItem=null";
				}
				return $"display='{((Item)val).DisplayName}' name='{((Item)val).Name}' itemId='{((Item)val).ItemId}' qid='{((Item)val).QualifiedItemId}' visibleName='{GetCurrentVanillaPantsName()}'";
			}
			catch (Exception ex)
			{
				return "error=" + ex.GetType().Name + ":" + ex.Message;
			}
		}

		private List<string> GetVanillaPantsSpecialItemCandidatesFromName(string displayName)
		{
			List<string> list = new List<string>();
			AddSpecialItemCandidate(list, displayName);
			return list;
		}

		private List<string> GetCurrentVanillaPantsSpecialItemCandidates(string displayName)
		{
			List<string> vanillaPantsSpecialItemCandidatesFromName = GetVanillaPantsSpecialItemCandidatesFromName(displayName);
			try
			{
				Clothing val = ((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player?.pantsItem)?.Value;
				AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).DisplayName : null);
				AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).Name : null);
				AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).ItemId : null);
				AddSpecialItemCandidate(vanillaPantsSpecialItemCandidatesFromName, (val != null) ? ((Item)val).QualifiedItemId : null);
			}
			catch
			{
			}
			return vanillaPantsSpecialItemCandidatesFromName;
		}

		private List<string> GetCurrentVisibleVanillaHatSpecialItemCandidates(string displayName)
		{
			List<string> list = new List<string>();
			AddSpecialItemCandidate(list, displayName);
			try
			{
				Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player?.hat)?.Value;
				AddSpecialItemCandidate(list, (val != null) ? ((Item)val).DisplayName : null);
				AddSpecialItemCandidate(list, (val != null) ? ((Item)val).Name : null);
				AddSpecialItemCandidate(list, (val != null) ? ((Item)val).ItemId : null);
				AddSpecialItemCandidate(list, (val != null) ? ((Item)val).QualifiedItemId : null);
			}
			catch
			{
			}
			return list;
		}

		private static void AddSpecialItemCandidate(List<string> candidates, string value)
		{
			if (candidates == null || string.IsNullOrWhiteSpace(value))
			{
				return;
			}
			string text = value.Trim();
			foreach (string candidate in candidates)
			{
				if (candidate.Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}
			candidates.Add(text);
		}

		private static string FormatSpecialItemCandidates(IEnumerable<string> candidates)
		{
			if (candidates == null)
			{
				return "<null>";
			}
			List<string> list = new List<string>();
			foreach (string candidate in candidates)
			{
				if (!string.IsNullOrWhiteSpace(candidate))
				{
					list.Add(candidate.Trim());
				}
			}
			return (list.Count == 0) ? "<empty>" : string.Join(", ", list);
		}

		private static List<string> CloneSpecialItemCandidates(IEnumerable<string> candidates)
		{
			List<string> list = new List<string>();
			if (candidates == null)
			{
				return list;
			}
			foreach (string candidate in candidates)
			{
				AddSpecialItemCandidate(list, candidate);
			}
			return list;
		}

		private void LogSpecialItemDebugOnce(string key, string message)
		{
			if (DebugLog)
			{
				string item = key ?? "";
				if (loggedSpecialItemDebugKeys.Add(item))
				{
					((Mod)this).Monitor.Log("[SPECIAL ITEM DEBUG] " + message, (LogLevel)2);
				}
			}
		}

		private static string DescribeSpecialItemNotice(SpecialItemNoticeInfo notice)
		{
			if (notice == null)
			{
				return "<none>";
			}
			return $"entry='{notice.EntryId}' type='{notice.ItemType}' display='{notice.DisplayName}' matched='{notice.MatchedName}' removed={notice.WasRemoved} valid={notice.IsValid}";
		}

		private bool TryResolveSpecialItemCandidates(IEnumerable<string> candidateNames, string itemType, NPC npc, bool wasRemoved, out SpecialItemNoticeInfo notice)
		{
			notice = null;
			if (specialItemReactionService == null)
			{
				return false;
			}
			if (!specialItemReactionService.TryResolveItem(candidateNames, itemType, npc, GetCurrentGameLanguageForPrompt(), out var resolved, wasRemoved) || resolved == null || string.IsNullOrWhiteSpace(resolved.EntryId) || string.IsNullOrWhiteSpace(resolved.ReactionContext))
			{
				return false;
			}
			notice = new SpecialItemNoticeInfo
			{
				EntryId = resolved.EntryId,
				DisplayName = resolved.DisplayName,
				ItemType = (StringUtils.FirstNonEmpty(resolved.ItemType, itemType) ?? itemType ?? ""),
				MatchedName = resolved.MatchedName,
				ReactionContext = resolved.ReactionContext,
				WasRemoved = wasRemoved,
				HasSecret = resolved.HasSecret,
				SecretId = (resolved.SecretId ?? "")
			};
			notice.MemoryHint = BuildSpecialItemMemoryContext(npc, notice);
			return true;
		}

		private bool TryResolveSpecialItemNoticeForNpc(NPC npc, FashionSenseChangeInfo changeInfo, bool requireNpcMemoryForRemoval, out SpecialItemNoticeInfo notice)
		{
			notice = null;
			if (changeInfo == null || specialItemReactionService == null)
			{
				return false;
			}
			string text = ((npc != null) ? ((Character)npc).Name : null) ?? "<none>";
			string currentVanillaPantsName = GetCurrentVanillaPantsName();
			if (!string.IsNullOrWhiteSpace(currentVanillaPantsName))
			{
				List<string> currentVanillaPantsSpecialItemCandidates = GetCurrentVanillaPantsSpecialItemCandidates(currentVanillaPantsName);
				if (TryResolveSpecialItemCandidates(currentVanillaPantsSpecialItemCandidates, "Pants", npc, wasRemoved: false, out notice))
				{
					if (changeInfo.VanillaPantsChanged)
					{
						LogSpecialItemDebugOnce($"current-pants|{text}|{notice.EntryId}|{currentVanillaPantsName}", $"{text}: CURRENT visible pants matched {DescribeSpecialItemNotice(notice)} | current='{currentVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(currentVanillaPantsSpecialItemCandidates)}]");
					}
					return true;
				}
			}
			if (changeInfo.VanillaPantsChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaPantsName))
			{
				List<string> list = CloneSpecialItemCandidates(changeInfo.PreviousVanillaPantsSpecialItemCandidates);
				AddSpecialItemCandidate(list, changeInfo.PreviousVanillaPantsName);
				if (TryResolveSpecialItemCandidates(list, "Pants", npc, wasRemoved: true, out notice))
				{
					bool flag = HasSpecialItemMemory(npc, notice);
					LogSpecialItemDebugOnce($"removed-pants|{text}|{notice.EntryId}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}|{flag}|{requireNpcMemoryForRemoval}", $"{text}: PREVIOUS pants matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(list)}] npcHasMemory={flag} requireMemory={requireNpcMemoryForRemoval}");
					if (requireNpcMemoryForRemoval && !flag)
					{
						LogSpecialItemDebugOnce("removed-pants-ignored|" + text + "|" + notice.EntryId, text + ": removed special item ignored because this NPC has no memory for it.");
						notice = null;
						return false;
					}
					return true;
				}
				LogSpecialItemDebugOnce($"previous-pants-no-match|{text}|{changeInfo.PreviousVanillaPantsName}|{changeInfo.NewVanillaPantsName}", $"{text}: previous pants did NOT match a special item | prev='{changeInfo.PreviousVanillaPantsName}' new='{changeInfo.NewVanillaPantsName}' candidates=[{FormatSpecialItemCandidates(list)}]");
			}
			string visibleVanillaHatId = GetVisibleVanillaHatId();
			if (!string.IsNullOrWhiteSpace(visibleVanillaHatId))
			{
				string currentVanillaHatName = GetCurrentVanillaHatName();
				if (TryResolveSpecialItemCandidates(GetCurrentVisibleVanillaHatSpecialItemCandidates(currentVanillaHatName), "Hat", npc, wasRemoved: false, out notice))
				{
					LogSpecialItemDebugOnce($"current-hat|{text}|{notice.EntryId}|{visibleVanillaHatId}|{currentVanillaHatName}", $"{text}: CURRENT visible vanilla hat matched {DescribeSpecialItemNotice(notice)} | hatId='{visibleVanillaHatId}' hatName='{currentVanillaHatName}'");
					return true;
				}
			}
			if (changeInfo.VanillaHatChanged && !string.IsNullOrWhiteSpace(changeInfo.PreviousVanillaHatId))
			{
				List<string> list2 = CloneSpecialItemCandidates(changeInfo.PreviousVanillaHatSpecialItemCandidates);
				AddSpecialItemCandidate(list2, changeInfo.PreviousVanillaHatId);
				if (TryResolveSpecialItemCandidates(list2, "Hat", npc, wasRemoved: true, out notice))
				{
					bool flag2 = HasSpecialItemMemory(npc, notice);
					LogSpecialItemDebugOnce($"removed-hat|{text}|{notice.EntryId}|{changeInfo.PreviousVanillaHatId}|{changeInfo.NewVanillaHatId}|{flag2}|{requireNpcMemoryForRemoval}", $"{text}: PREVIOUS hat matched removed {DescribeSpecialItemNotice(notice)} | prev='{changeInfo.PreviousVanillaHatId}' new='{changeInfo.NewVanillaHatId}' candidates=[{FormatSpecialItemCandidates(list2)}] npcHasMemory={flag2} requireMemory={requireNpcMemoryForRemoval}");
					if (requireNpcMemoryForRemoval && !flag2)
					{
						LogSpecialItemDebugOnce("removed-hat-ignored|" + text + "|" + notice.EntryId, text + ": removed special hat ignored because this NPC has no memory for it.");
						notice = null;
						return false;
					}
					return true;
				}
			}
			return false;
		}

		private string GetSpecialItemMemoryKey(NPC npc, SpecialItemNoticeInfo notice)
		{
			if (npc == null || notice == null || string.IsNullOrWhiteSpace(notice.EntryId))
			{
				return "";
			}
			return "NatrollEXE.OutfitReactions/SpecialItemSeen/" + MakeSafeModDataPart(((Character)npc).Name ?? "unknown") + "/" + MakeSafeModDataPart(StringUtils.FirstNonEmpty(notice.ItemType, "Item")) + "/" + MakeSafeModDataPart(notice.EntryId);
		}

		private bool HasSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
		{
			if (npc == null || notice == null || Game1.player == null)
			{
				return false;
			}
			string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
			if (string.IsNullOrWhiteSpace(specialItemMemoryKey))
			{
				return false;
			}
			string s = default(string);
			int result;
			return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s) && int.TryParse(s, out result) && result > 0;
		}

		private int GetSpecialItemSeenCount(NPC npc, SpecialItemNoticeInfo notice)
		{
			if (npc == null || notice == null || Game1.player == null)
			{
				return 0;
			}
			string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
			if (string.IsNullOrWhiteSpace(specialItemMemoryKey))
			{
				return 0;
			}
			string s = default(string);
			int result;
			return (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s) && int.TryParse(s, out result)) ? Math.Max(0, result) : 0;
		}

		private string BuildSpecialItemMemoryContext(NPC npc, SpecialItemNoticeInfo notice)
		{
			int specialItemSeenCount = GetSpecialItemSeenCount(npc, notice);
			if (specialItemSeenCount <= 0 || notice == null)
			{
				return "";
			}
			string text = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? "this special item";
			return (specialItemSeenCount == 1) ? ("This NPC has seen the farmer wear " + text + " before (1 time). They may recognize it with familiarity.") : $"This NPC has seen the farmer wear {text} before ({specialItemSeenCount} times). They should recognize it as something they've seen before.";
		}

		private void RecordSpecialItemMemory(NPC npc, SpecialItemNoticeInfo notice)
		{
			if (npc == null || notice == null || !notice.IsValid || Game1.player == null)
			{
				return;
			}
			string specialItemMemoryKey = GetSpecialItemMemoryKey(npc, notice);
			if (!string.IsNullOrWhiteSpace(specialItemMemoryKey))
			{
				int result = 0;
				string s = default(string);
				if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).TryGetValue(specialItemMemoryKey, ref s))
				{
					int.TryParse(s, out result);
				}
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[specialItemMemoryKey] = (result + 1).ToString();
				LogSpecialItemDebugOnce($"record-memory|{specialItemMemoryKey}|{result + 1}", $"Recorded memory key='{specialItemMemoryKey}' oldCount={result} newCount={result + 1} notice={DescribeSpecialItemNotice(notice)}");
				string text = specialItemMemoryKey + "/Name";
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[text] = StringUtils.FirstNonEmpty(notice.DisplayName, notice.MatchedName, notice.EntryId) ?? notice.EntryId;
			}
		}
	}
	public interface IFashionSenseApi
	{
		public enum Type
		{
			Unknown,
			Hair,
			Accessory,
			AccessorySecondary,
			AccessoryTertiary,
			Hat,
			Shirt,
			Pants,
			Sleeves,
			Shoes,
			Player
		}

		KeyValuePair<bool, string> GetCurrentOutfitId();

		KeyValuePair<bool, string> GetCurrentAppearanceId(Type appearanceType, Farmer target = null);

		KeyValuePair<bool, Color> GetAppearanceColor(Type appearanceType, Farmer target = null);
	}
	public interface IGenericModConfigMenuApi
	{
		void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

		void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

		void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

		void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

		void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);
	}
	internal sealed class OtherNpcClothesReactionSystem
	{
		private sealed class PendingPrompt
		{
			public int DialogueCountBeforePush { get; set; }

			public int DialogueCountAfterPush { get; set; }

			public int OriginalFacingDirection { get; set; }

			public bool WasLookingAtPlayer { get; set; }

			public bool CameFromPeeking { get; set; }

			public bool WasCaughtPeeking { get; set; }

			public int NoticeDelayTimer { get; set; }

			public bool DialogueQueued { get; set; }

			public bool NoticePauseActive { get; set; }

			public int PendingBubbleCooldown { get; set; }

			public bool PostDialogueRestoreApplied { get; set; }

			public bool PostDialogueLingerActive { get; set; }

			public int PostDialogueLingerTimer { get; set; }

			public List<Dialogue> DialogueBackupBeforeOutfit { get; set; } = new List<Dialogue>();

			public bool HasDialogueBackup { get; set; }

			public bool DialogueWasConsumed { get; set; }

			public bool SawDialogueMenuAfterConsumption { get; set; }

			public bool PromptClearedAfterFirstDialogueMenu { get; set; }

			public object FirstDialogueMenu { get; set; }

			public int FirstDialogueMenuTicks { get; set; }

			public int PromptKeepAliveTimer { get; set; }

			public bool WaitingForPostDialogueRestore { get; set; }

			public int PostDialogueRestoreDelay { get; set; }

			public bool PostDialogueOutfitWasRead { get; set; }

			public bool WaitingForOwnAiFinalDialogue { get; set; }

			public bool EmoteFired { get; set; }

			public bool HasFriendshipEntry { get; set; }

			public bool OriginalTalkedToToday { get; set; }

			public bool ForcedTalkedToToday { get; set; }

			public NpcOutfitSpecialActionSnapshot SpecialActionSnapshot { get; set; }
		}

		private sealed class NpcOutfitSpecialActionSnapshot
		{
			public NPC Npc { get; set; }

			public GameLocation Location { get; set; }

			public int FacingDirection { get; set; }

			public int CurrentFrame { get; set; }

			public bool Flip { get; set; }

			public int MovementPause { get; set; }

			public int AddedSpeed { get; set; }

			public List<AnimationFrame> CurrentAnimation { get; set; }

			public bool HasSavedSpriteDimensions { get; set; }

			public bool SavedIgnoreSourceRectUpdates { get; set; }

			public int SavedSpriteWidth { get; set; }

			public int SavedTempSpriteHeight { get; set; }

			public bool? SavedDoingEndOfRouteAnimation { get; set; }

			public bool? SavedCurrentlyDoingEndOfRouteAnimation { get; set; }

			public string SavedStartedEndOfRouteBehavior { get; set; }

			public bool HasSavedRodLayerFields { get; set; }

			public float SavedYOffset { get; set; }

			public string SavedLoadedEndOfRouteBehavior { get; set; }

			public Vector2 SavedDrawOffset { get; set; }
		}

		private sealed class SpyingState
		{
			public int OriginalFacingDirection { get; set; }

			public bool IsBeingWatched { get; set; }

			public int PretendTimer { get; set; }

			public bool WasEverCaught { get; set; }

			public int WalkCooldownTimer { get; set; }

			public int PeekGraceTimer { get; set; }
		}

		private readonly IMonitor monitor;

		private readonly Func<ModConfig> getConfig;

		private readonly Func<NPC, bool> tryQueueOutfitDialogue;

		private readonly Func<NPC, bool> refreshOutfitPrompt;

		private readonly Action<NPC> clearOutfitPrompt;

		private readonly Func<bool> hasNoticeableCurrentFashionSenseAppearance;

		private readonly Func<NPC, bool> canNoticeCurrentOutfitNotice;

		private readonly Action<NPC> markCurrentOutfitAsNoticed;

		private readonly Func<NPC, bool> canNpcReactToOutfit;

		private readonly Func<NPC, bool> hasNpcSeenCurrentVisualBefore;

		private readonly Random random = new Random();

		private int discoveryScanTimer;

		private const int DiscoveryScanIntervalTicks = 6;

		private const int FailedRollCooldownTicks = 900;

		private const int CancelledReactionCooldownTicks = 600;

		private const float OutfitNoticePauseDistance = 96f;

		private const float OutfitNoticeReleaseDistance = 300f;

		private const float PostDialogueLingerDistance = 600f;

		private const int PostDialogueLingerTicks = 360;

		private const float NpcSpecialActionRestoreDistance = 300f;

		private const int PendingBubbleCooldownTicks = 240;

		private readonly HashSet<string> reactedNpcsThisOutfit = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, PendingPrompt> pendingPrompts = new Dictionary<string, PendingPrompt>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, int> rollCooldowns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, SpyingState> spyingNpcs = new Dictionary<string, SpyingState>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, int> ticksSinceLastMoving = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private const int WalkingGraceTicks = 30;

		public OtherNpcClothesReactionSystem(IMonitor monitor, Func<ModConfig> getConfig, Func<NPC, bool> tryQueueOutfitDialogue, Func<NPC, bool> refreshOutfitPrompt, Action<NPC> clearOutfitPrompt, Func<bool> hasNoticeableCurrentFashionSenseAppearance, Func<NPC, bool> canNoticeCurrentOutfitNotice, Action<NPC> markCurrentOutfitAsNoticed, Func<NPC, bool> canNpcReactToOutfit, Func<NPC, bool> hasNpcSeenCurrentVisualBefore)
		{
			this.monitor = monitor;
			this.getConfig = getConfig;
			this.tryQueueOutfitDialogue = tryQueueOutfitDialogue;
			this.refreshOutfitPrompt = refreshOutfitPrompt;
			this.clearOutfitPrompt = clearOutfitPrompt;
			this.hasNoticeableCurrentFashionSenseAppearance = hasNoticeableCurrentFashionSenseAppearance;
			this.canNoticeCurrentOutfitNotice = canNoticeCurrentOutfitNotice;
			this.markCurrentOutfitAsNoticed = markCurrentOutfitAsNoticed;
			this.canNpcReactToOutfit = canNpcReactToOutfit;
			this.hasNpcSeenCurrentVisualBefore = hasNpcSeenCurrentVisualBefore;
		}

		public void Reset(bool clearPrompts = true)
		{
			if (clearPrompts)
			{
				ClearAllPendingPrompts(removeQueuedDialogues: true);
			}
			reactedNpcsThisOutfit.Clear();
			pendingPrompts.Clear();
			rollCooldowns.Clear();
			spyingNpcs.Clear();
			ticksSinceLastMoving.Clear();
		}

		public void NotifyOutfitChanged()
		{
			ClearAllPendingPrompts(removeQueuedDialogues: true);
			reactedNpcsThisOutfit.Clear();
			pendingPrompts.Clear();
			rollCooldowns.Clear();
			spyingNpcs.Clear();
			ticksSinceLastMoving.Clear();
			discoveryScanTimer = 0;
		}

		public void Update(string spouseName)
		{
			if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null)
			{
				return;
			}
			ModConfig modConfig = getConfig?.Invoke();
			if (modConfig == null)
			{
				return;
			}
			UpdateRollCooldowns();
			UpdatePendingPrompts(modConfig);
			if (!modConfig.EnableNpcOutfitReactions)
			{
				return;
			}
			Func<bool> func = hasNoticeableCurrentFashionSenseAppearance;
			if (func == null || !func() || Game1.eventUp || Game1.activeClickableMenu != null)
			{
				return;
			}
			float num = Math.Max(64f, modConfig.OutfitNoticeDistance);
			float cancelDistance = Math.Max(num, modConfig.OutfitCancelDistance);
			UpdateSpyingNpcs(num, cancelDistance);
			int num2 = Math.Clamp(modConfig.NpcOutfitReactionChance, 0, 100);
			int num3 = Math.Clamp(modConfig.NpcRepeatedVisualNoticeChance, 0, 100);
			if (num2 <= 0 && num3 <= 0)
			{
				return;
			}
			if (discoveryScanTimer > 0)
			{
				discoveryScanTimer--;
				return;
			}
			discoveryScanTimer = 6;
			foreach (NPC item in ((IEnumerable<NPC>)Game1.currentLocation.characters).ToList())
			{
				if (!IsValidNpc(item, spouseName) || pendingPrompts.ContainsKey(((Character)item).Name))
				{
					continue;
				}
				if (!spyingNpcs.ContainsKey(((Character)item).Name))
				{
					int value;
					if (((Character)item).isMoving())
					{
						ticksSinceLastMoving[((Character)item).Name] = 0;
					}
					else if (ticksSinceLastMoving.TryGetValue(((Character)item).Name, out value))
					{
						ticksSinceLastMoving[((Character)item).Name] = value + 1;
					}
					else
					{
						ticksSinceLastMoving[((Character)item).Name] = 31;
					}
				}
				int value2;
				bool flag = ticksSinceLastMoving.TryGetValue(((Character)item).Name, out value2) && value2 < 30;
				if (flag && !spyingNpcs.ContainsKey(((Character)item).Name) && !reactedNpcsThisOutfit.Contains(((Character)item).Name) && DistanceToPlayer(item) <= num && random.Next(1000) < 3 && HasLineOfSightToPlayer(item))
				{
					spyingNpcs[((Character)item).Name] = new SpyingState
					{
						OriginalFacingDirection = ((Character)item).FacingDirection,
						PeekGraceTimer = 30
					};
					if (((Character)item).Sprite != null)
					{
						((Character)item).Sprite.StopAnimation();
						((Character)item).Sprite.CurrentFrame = GetNpcIdleFrameForDirection(((Character)item).FacingDirection);
						((Character)item).Sprite.UpdateSourceRect();
					}
					ArmPendingReactionForSpy(item);
				}
				if (spyingNpcs.ContainsKey(((Character)item).Name) || flag || reactedNpcsThisOutfit.Contains(((Character)item).Name))
				{
					continue;
				}
				Func<NPC, bool> func2 = hasNpcSeenCurrentVisualBefore;
				int num4 = ((func2 != null && func2(item)) ? num3 : num2);
				if (num4 > 0 && !(DistanceToPlayer(item) > num) && IsNpcFacingPlayer(item) && HasLineOfSightToPlayer(item) && (!rollCooldowns.TryGetValue(((Character)item).Name, out var value3) || value3 <= 0))
				{
					if (random.Next(100) >= num4)
					{
						rollCooldowns[((Character)item).Name] = 900;
					}
					else
					{
						TryStartReaction(item, modConfig);
					}
				}
			}
		}

		private bool IsValidNpc(NPC npc, string spouseName)
		{
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			if (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (!string.IsNullOrWhiteSpace(spouseName) && ((Character)npc).Name.Equals(spouseName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (!((Character)npc).IsVillager)
			{
				return false;
			}
			if (npc.IsInvisible || ((NetFieldBase<bool, NetBool>)(object)npc.isSleeping).Value)
			{
				return false;
			}
			Func<NPC, bool> func = canNpcReactToOutfit;
			if (func == null || !func(npc))
			{
				return false;
			}
			return true;
		}

		private void TryStartReaction(NPC npc, ModConfig config)
		{
			if (npc == null)
			{
				return;
			}
			PendingPrompt pendingPrompt = new PendingPrompt
			{
				OriginalFacingDirection = ((Character)npc).FacingDirection,
				WasLookingAtPlayer = false,
				NoticeDelayTimer = 75,
				DialogueQueued = false,
				NoticePauseActive = false,
				PendingBubbleCooldown = 0
			};
			reactedNpcsThisOutfit.Add(((Character)npc).Name);
			pendingPrompts[((Character)npc).Name] = pendingPrompt;
			spyingNpcs.Remove(((Character)npc).Name);
			ShowPendingDialogueBubbleIfNeeded(npc, pendingPrompt, config, force: true);
			UpdateNpcLookState(npc, pendingPrompt, config);
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("[NPC OUTFIT] " + ((Character)npc).Name + " noticed the outfit. Outfit dialogue is pending until player click.", (LogLevel)2);
				}
			}
		}

		private void ArmPendingReactionForSpy(NPC npc)
		{
			if (npc != null)
			{
				PendingPrompt value = new PendingPrompt
				{
					OriginalFacingDirection = ((Character)npc).FacingDirection,
					WasLookingAtPlayer = false,
					CameFromPeeking = true,
					NoticeDelayTimer = 0,
					DialogueQueued = false,
					NoticePauseActive = false,
					PendingBubbleCooldown = 0
				};
				reactedNpcsThisOutfit.Add(((Character)npc).Name);
				pendingPrompts[((Character)npc).Name] = value;
			}
		}

		private bool TryQueuePromptAfterNotice(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null)
			{
				return false;
			}
			if (pending.DialogueQueued)
			{
				return true;
			}
			CaptureQueuedDialoguesBeforeOutfit(npc, pending);
			TemporarilySkipFirstDailyDialogue(npc, pending);
			Func<NPC, bool> func = tryQueueOutfitDialogue;
			if (func == null || !func(npc))
			{
				RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
				RestoreTalkedToTodayIfUnread(npc, pending);
				rollCooldowns[((Character)npc).Name] = 900;
				return false;
			}
			int count = npc.CurrentDialogue.Count;
			if (count <= 0)
			{
				RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
				RestoreTalkedToTodayIfUnread(npc, pending);
				rollCooldowns[((Character)npc).Name] = 900;
				return false;
			}
			pending.DialogueQueued = true;
			pending.DialogueCountBeforePush = Math.Max(0, count - 1);
			pending.DialogueCountAfterPush = count;
			refreshOutfitPrompt?.Invoke(npc);
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("[NPC OUTFIT] " + ((Character)npc).Name + "'s outfit dialogue is now ready after the notice beat.", (LogLevel)2);
				}
			}
			return true;
		}

		public bool WasNpcCaughtPeeking(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			PendingPrompt value;
			return pendingPrompts.TryGetValue(((Character)npc).Name, out value) && value != null && value.CameFromPeeking && value.WasCaughtPeeking;
		}

		public bool TryPrioritizePendingDialogueForClick(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			if (!pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return false;
			}
			if (value.DialogueWasConsumed)
			{
				return false;
			}
			if (spyingNpcs.TryGetValue(((Character)npc).Name, out var value2) && value2 != null)
			{
				value.WasCaughtPeeking = value2.WasEverCaught;
			}
			spyingNpcs.Remove(((Character)npc).Name);
			if (value.NoticeDelayTimer > 0)
			{
				value.NoticeDelayTimer = 0;
			}
			ModConfig modConfig = getConfig?.Invoke();
			bool flag = modConfig != null && tryQueueOutfitDialogue != null;
			CaptureQueuedDialoguesBeforeOutfit(npc, value);
			int num = npc.CurrentDialogue?.Count ?? 0;
			if (!flag)
			{
				TemporarilySkipFirstDailyDialogue(npc, value);
				npc.CurrentDialogue.Clear();
			}
			Func<NPC, bool> func = tryQueueOutfitDialogue;
			if (func == null || !func(npc))
			{
				if (!flag)
				{
					RestoreQueuedDialoguesAfterOutfit(npc, value, clearCurrentDialogue: true);
				}
				RestoreTalkedToTodayIfUnread(npc, value);
				return false;
			}
			value.DialogueQueued = true;
			if (flag && (npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= num))
			{
				value.DialogueWasConsumed = true;
				value.WaitingForOwnAiFinalDialogue = true;
				value.PromptKeepAliveTimer = Math.Max(value.PromptKeepAliveTimer, 900);
				value.PostDialogueOutfitWasRead = false;
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] Started built-in AI outfit generation for " + ((Character)npc).Name + " at click time; kept original dialogue stack untouched until final line is ready.", (LogLevel)2);
					}
				}
				return true;
			}
			if (npc.CurrentDialogue == null || npc.CurrentDialogue.Count <= 0)
			{
				value.DialogueWasConsumed = true;
				value.WaitingForOwnAiFinalDialogue = true;
				value.PromptKeepAliveTimer = Math.Max(value.PromptKeepAliveTimer, 900);
				value.PostDialogueOutfitWasRead = false;
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("[NPC OUTFIT] Started built-in AI outfit generation for " + ((Character)npc).Name + " at click time; no temporary dialogue box was queued.", (LogLevel)2);
					}
				}
				return true;
			}
			value.DialogueCountBeforePush = Math.Max(0, npc.CurrentDialogue.Count - 1);
			value.DialogueCountAfterPush = npc.CurrentDialogue.Count;
			refreshOutfitPrompt?.Invoke(npc);
			if (ModEntry.DebugLog)
			{
				IMonitor obj3 = monitor;
				if (obj3 != null)
				{
					obj3.Log("[NPC OUTFIT] Queued outfit dialogue for " + ((Character)npc).Name + " at click time.", (LogLevel)2);
				}
			}
			return true;
		}

		private void UpdateRollCooldowns()
		{
			if (rollCooldowns.Count <= 0)
			{
				return;
			}
			foreach (string item in rollCooldowns.Keys.ToList())
			{
				rollCooldowns[item]--;
				if (rollCooldowns[item] <= 0)
				{
					rollCooldowns.Remove(item);
				}
			}
		}

		private void UpdatePendingPrompts(ModConfig config)
		{
			if (pendingPrompts.Count <= 0)
			{
				return;
			}
			float num = Math.Max(Math.Max(64f, config.OutfitNoticeDistance) + 64f, config.OutfitCancelDistance);
			foreach (string item in pendingPrompts.Keys.ToList())
			{
				PendingPrompt pendingPrompt = pendingPrompts[item];
				NPC characterFromName = Game1.getCharacterFromName(item, true, false);
				if (characterFromName == null)
				{
					pendingPrompts.Remove(item);
					continue;
				}
				if (pendingPrompt.PendingBubbleCooldown > 0)
				{
					pendingPrompt.PendingBubbleCooldown--;
				}
				if (pendingPrompt.WaitingForPostDialogueRestore)
				{
					UpdatePostDialogueRestore(characterFromName, pendingPrompt);
					if (!pendingPrompt.WaitingForPostDialogueRestore)
					{
						pendingPrompts.Remove(item);
					}
				}
				else if (!pendingPrompt.DialogueQueued)
				{
					if (Game1.player == null || ((Character)characterFromName).currentLocation != ((Character)Game1.player).currentLocation || DistanceToPlayer(characterFromName) > num)
					{
						CancelPendingPrompt(characterFromName, pendingPrompt);
						pendingPrompts.Remove(item);
						continue;
					}
					UpdateNpcLookState(characterFromName, pendingPrompt, config);
					ShowPendingDialogueBubbleIfNeeded(characterFromName, pendingPrompt, config);
					if (pendingPrompt.NoticeDelayTimer > 0)
					{
						pendingPrompt.NoticeDelayTimer--;
					}
				}
				else if (pendingPrompt.WaitingForOwnAiFinalDialogue)
				{
					UpdateNpcLookState(characterFromName, pendingPrompt, config);
					ShowPendingDialogueBubbleIfNeeded(characterFromName, pendingPrompt, config);
				}
				else if (pendingPrompt.DialogueWasConsumed || characterFromName.CurrentDialogue.Count < pendingPrompt.DialogueCountAfterPush)
				{
					KeepConsumedDialoguePromptAlive(characterFromName, pendingPrompt);
					if (ShouldClearConsumedPrompt(pendingPrompt))
					{
						clearOutfitPrompt?.Invoke(characterFromName);
						SchedulePostDialogueRestore(pendingPrompt, pendingPrompt.SawDialogueMenuAfterConsumption);
					}
				}
				else if (Game1.player == null || ((Character)characterFromName).currentLocation != ((Character)Game1.player).currentLocation || DistanceToPlayer(characterFromName) > num)
				{
					CancelPendingPrompt(characterFromName, pendingPrompt);
					pendingPrompts.Remove(item);
				}
				else
				{
					refreshOutfitPrompt?.Invoke(characterFromName);
					UpdateNpcLookState(characterFromName, pendingPrompt, config);
					ShowPendingDialogueBubbleIfNeeded(characterFromName, pendingPrompt, config);
				}
			}
		}

		private void KeepConsumedDialoguePromptAlive(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null)
			{
				return;
			}
			if (!pending.DialogueWasConsumed)
			{
				pending.DialogueWasConsumed = true;
				pending.PromptKeepAliveTimer = 900;
				if (pending.WasLookingAtPlayer)
				{
					FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);
				}
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] " + ((Character)npc).Name + "'s outfit prompt was consumed; keeping Outfit Compliments AI override alive until the first real reply/input step.", (LogLevel)2);
					}
				}
			}
			if (pending.PromptKeepAliveTimer > 0)
			{
				pending.PromptKeepAliveTimer--;
			}
			object activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null)
			{
				pending.SawDialogueMenuAfterConsumption = true;
				if (pending.FirstDialogueMenu == null)
				{
					pending.FirstDialogueMenu = activeClickableMenu;
					pending.FirstDialogueMenuTicks = 0;
				}
				else if (pending.FirstDialogueMenu == activeClickableMenu)
				{
					pending.FirstDialogueMenuTicks++;
				}
				else
				{
					if (!pending.PromptClearedAfterFirstDialogueMenu && LooksLikeReplyOrInputMenu(activeClickableMenu))
					{
						clearOutfitPrompt?.Invoke(npc);
						pending.PromptClearedAfterFirstDialogueMenu = true;
						if (ModEntry.DebugLog)
						{
							IMonitor obj2 = monitor;
							if (obj2 != null)
							{
								obj2.Log("[NPC OUTFIT] Cleared outfit prompt for " + ((Character)npc).Name + " when Outfit Compliments AI opened a reply/input menu.", (LogLevel)2);
							}
						}
						return;
					}
					pending.FirstDialogueMenu = activeClickableMenu;
					pending.FirstDialogueMenuTicks = 0;
				}
				if (!pending.PromptClearedAfterFirstDialogueMenu)
				{
					refreshOutfitPrompt?.Invoke(npc);
				}
			}
			else if (pending.SawDialogueMenuAfterConsumption && !pending.PromptClearedAfterFirstDialogueMenu)
			{
				if (pending.FirstDialogueMenuTicks < 15 && pending.PromptKeepAliveTimer > 0)
				{
					refreshOutfitPrompt?.Invoke(npc);
					return;
				}
				clearOutfitPrompt?.Invoke(npc);
				pending.PromptClearedAfterFirstDialogueMenu = true;
				if (ModEntry.DebugLog)
				{
					IMonitor obj3 = monitor;
					if (obj3 != null)
					{
						obj3.Log("[NPC OUTFIT] Cleared outfit prompt for " + ((Character)npc).Name + " after the first outfit dialogue menu closed.", (LogLevel)2);
					}
				}
			}
			else if (!pending.PromptClearedAfterFirstDialogueMenu)
			{
				refreshOutfitPrompt?.Invoke(npc);
			}
		}

		private static bool LooksLikeReplyOrInputMenu(object menu)
		{
			if (menu == null)
			{
				return false;
			}
			string typeName = menu.GetType().Name ?? "";
			if (typeName.Equals("DialogueBox", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			string[] source = new string[9] { "Response", "Reply", "Input", "Text", "TextBox", "Keyboard", "Question", "Answer", "Choice" };
			return source.Any((string marker) => typeName.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private bool ShouldClearConsumedPrompt(PendingPrompt pending)
		{
			if (pending == null || !pending.DialogueWasConsumed)
			{
				return false;
			}
			if (pending.SawDialogueMenuAfterConsumption && pending.PromptClearedAfterFirstDialogueMenu && Game1.activeClickableMenu == null)
			{
				return true;
			}
			if (pending.PromptKeepAliveTimer <= 0)
			{
				pending.PromptClearedAfterFirstDialogueMenu = true;
				return true;
			}
			return false;
		}

		private void SchedulePostDialogueRestore(PendingPrompt pending, bool outfitWasRead)
		{
			if (pending != null && !pending.WaitingForPostDialogueRestore)
			{
				pending.WaitingForPostDialogueRestore = true;
				pending.PostDialogueRestoreDelay = 8;
				pending.PostDialogueOutfitWasRead = outfitWasRead;
			}
		}

		public void NotifyPrioritizedDialogueOpenedByHarmony(NPC npc)
		{
			if (npc == null || !pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return;
			}
			value.DialogueWasConsumed = true;
			value.PromptKeepAliveTimer = Math.Max(value.PromptKeepAliveTimer, 300);
			value.PostDialogueOutfitWasRead = true;
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("[NPC OUTFIT] Harmony opened " + ((Character)npc).Name + "'s outfit dialogue; waiting until the first Outfit Compliments AI menu is done before restoring their previous dialogue.", (LogLevel)2);
				}
			}
		}

		public void NotifyOwnAiWaitingDialogueOpened(NPC npc)
		{
			if (npc == null || !pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return;
			}
			value.DialogueWasConsumed = true;
			value.WaitingForOwnAiFinalDialogue = true;
			value.PromptKeepAliveTimer = Math.Max(value.PromptKeepAliveTimer, 900);
			value.PostDialogueOutfitWasRead = false;
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("[NPC OUTFIT] Opened AI waiting dialogue for " + ((Character)npc).Name + "; keeping outfit reaction pending until final AI line is ready.", (LogLevel)2);
				}
			}
		}

		public void NotifyOwnAiFinalDialogueOpened(NPC npc)
		{
			if (npc == null || !pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return;
			}
			value.WaitingForOwnAiFinalDialogue = false;
			value.DialogueWasConsumed = true;
			value.PromptKeepAliveTimer = Math.Max(value.PromptKeepAliveTimer, 300);
			value.PostDialogueOutfitWasRead = true;
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("[NPC OUTFIT] Opened final AI outfit dialogue for " + ((Character)npc).Name + "; it will be marked read after the menu closes.", (LogLevel)2);
				}
			}
		}

		public void NotifyOwnAiFinalDialogueFailed(NPC npc)
		{
			if (npc == null || !pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return;
			}
			try
			{
				RestoreQueuedDialoguesAfterOutfit(npc, value, clearCurrentDialogue: true);
				RestoreTalkedToTodayIfUnread(npc, value);
				clearOutfitPrompt?.Invoke(npc);
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] Could not restore dialogue after failed AI for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
			value.WaitingForOwnAiFinalDialogue = false;
			value.DialogueWasConsumed = false;
			value.DialogueQueued = false;
			value.SawDialogueMenuAfterConsumption = false;
			value.PromptClearedAfterFirstDialogueMenu = false;
			value.PostDialogueOutfitWasRead = false;
			value.DialogueCountAfterPush = 0;
			value.PendingBubbleCooldown = Math.Max(value.PendingBubbleCooldown, 180);
			if (ModEntry.DebugLog)
			{
				IMonitor obj2 = monitor;
				if (obj2 != null)
				{
					obj2.Log("[NPC OUTFIT] AI outfit dialogue failed for " + ((Character)npc).Name + "; restored previous dialogue (outfit NOT marked read) and reopened it for a click retry.", (LogLevel)2);
				}
			}
		}

		public void CancelPendingOwnAiGeneration(NPC npc)
		{
			if (npc != null && pendingPrompts.TryGetValue(((Character)npc).Name, out var value) && value != null)
			{
				CancelPendingPrompt(npc, value);
				pendingPrompts.Remove(((Character)npc).Name);
				rollCooldowns.Remove(((Character)npc).Name);
			}
		}

		private void UpdatePostDialogueRestore(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null)
			{
				return;
			}
			if (pending.PostDialogueLingerActive)
			{
				UpdatePostDialogueLinger(npc, pending);
			}
			else if (pending.PostDialogueRestoreApplied)
			{
				pending.WaitingForPostDialogueRestore = false;
			}
			else
			{
				if (Game1.activeClickableMenu != null)
				{
					return;
				}
				if (pending.PostDialogueRestoreDelay <= 0)
				{
					bool flag = false;
					try
					{
						clearOutfitPrompt?.Invoke(npc);
						if (pending.PostDialogueOutfitWasRead)
						{
							markCurrentOutfitAsNoticed?.Invoke(npc);
						}
						RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
						RestoreTalkedToTodayIfUnread(npc, pending);
						pending.PostDialogueRestoreApplied = true;
						flag = pending.PostDialogueOutfitWasRead && Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
						if (flag)
						{
							pending.PostDialogueLingerActive = true;
							pending.PostDialogueLingerTimer = 360;
							UpdatePostDialogueLinger(npc, pending);
							if (ModEntry.DebugLog)
							{
								IMonitor obj = monitor;
								if (obj != null)
								{
									obj.Log($"[NPC OUTFIT] {((Character)npc).Name} will linger after the outfit compliment until distance >= {600f:F0} or {360} ticks.", (LogLevel)2);
								}
							}
						}
						else if (ModEntry.DebugLog)
						{
							IMonitor obj2 = monitor;
							if (obj2 != null)
							{
								obj2.Log("[NPC OUTFIT] Finished delayed restore after outfit dialogue for " + ((Character)npc).Name + ".", (LogLevel)2);
							}
						}
						return;
					}
					finally
					{
						if (!flag)
						{
							pending.WaitingForPostDialogueRestore = false;
						}
					}
				}
				pending.PostDialogueRestoreDelay--;
			}
		}

		private void UpdateNpcLookState(NPC npc, PendingPrompt pending, ModConfig config)
		{
			if (npc == null || pending == null || Game1.player == null)
			{
				return;
			}
			if (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				pending.NoticePauseActive = false;
				return;
			}
			float num = DistanceToPlayer(npc);
			if (num <= 96f)
			{
				pending.NoticePauseActive = true;
			}
			else if (num >= 300f)
			{
				pending.NoticePauseActive = false;
			}
			if (pending.NoticePauseActive)
			{
				CaptureNpcSpecialActionBeforeOutfit(npc, pending);
				if (((Character)npc).movementPause < 6)
				{
					((Character)npc).movementPause = 6;
				}
				AnimatedSprite sprite = ((Character)npc).Sprite;
				if (sprite != null)
				{
					sprite.StopAnimation();
				}
				if (FacePlayerIfSafe(npc))
				{
					pending.WasLookingAtPlayer = true;
				}
			}
			else
			{
				if (pending.WasLookingAtPlayer && !TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true))
				{
					FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);
				}
				pending.WasLookingAtPlayer = false;
			}
		}

		private void ShowPendingDialogueBubbleIfNeeded(NPC npc, PendingPrompt pending, ModConfig config, bool force = false)
		{
			if (npc == null || pending == null || config == null || Game1.player == null || ((Character)npc).currentLocation != ((Character)Game1.player).currentLocation || Game1.activeClickableMenu != null || Game1.eventUp)
			{
				return;
			}
			float num = Math.Max(64f, config.OutfitNoticeDistance);
			if (!(DistanceToPlayer(npc) > num) && (force || pending.PendingBubbleCooldown <= 0))
			{
				if (!pending.EmoteFired)
				{
					((Character)npc).doEmote(40, true);
					pending.EmoteFired = true;
				}
				pending.PendingBubbleCooldown = (force ? 180 : 240);
			}
		}

		public bool IsHeldForFishingSpecialAction(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			PendingPrompt value;
			return pendingPrompts.TryGetValue(((Character)npc).Name, out value) && value?.SpecialActionSnapshot != null && !string.IsNullOrEmpty(value.SpecialActionSnapshot.SavedStartedEndOfRouteBehavior);
		}

		public bool HasAnyActivePendingReaction()
		{
			foreach (PendingPrompt value in pendingPrompts.Values)
			{
				if (value == null || value.PostDialogueOutfitWasRead || value.PostDialogueRestoreApplied || value.PostDialogueLingerActive || !value.DialogueQueued)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public bool HasUnreadPendingDialogueFor(NPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			if (!pendingPrompts.TryGetValue(((Character)npc).Name, out var value) || value == null)
			{
				return false;
			}
			if (value.PostDialogueOutfitWasRead || value.PostDialogueRestoreApplied || value.PostDialogueLingerActive)
			{
				return false;
			}
			return true;
		}

		public IEnumerable<NPC> GetPendingDialogueIndicatorNpcs()
		{
			if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null)
			{
				yield break;
			}
			ModConfig config = getConfig?.Invoke();
			if (config == null)
			{
				yield break;
			}
			float noticeDistance = Math.Max(64f, config.OutfitNoticeDistance);
			foreach (string npcName in pendingPrompts.Keys.ToList())
			{
				if (pendingPrompts.TryGetValue(npcName, out var pending) && pending != null && !pending.WaitingForPostDialogueRestore && !pending.PostDialogueLingerActive && !pending.WaitingForOwnAiFinalDialogue && !pending.DialogueWasConsumed)
				{
					NPC npc = Game1.getCharacterFromName(npcName, true, false);
					if (npc != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation && !(DistanceToPlayer(npc) > noticeDistance))
					{
						yield return npc;
						pending = null;
					}
				}
			}
		}

		private void UpdatePostDialogueLinger(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null)
			{
				return;
			}
			bool flag = Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation;
			float num = (flag ? DistanceToPlayer(npc) : 600f);
			bool flag2 = pending.SpecialActionSnapshot != null;
			if (pending.PostDialogueLingerTimer > 0)
			{
				pending.PostDialogueLingerTimer--;
			}
			bool num2;
			if (!flag2)
			{
				if (flag && !(num >= 600f))
				{
					num2 = pending.PostDialogueLingerTimer <= 0;
					goto IL_00a1;
				}
			}
			else if (flag)
			{
				num2 = num >= 300f;
				goto IL_00a1;
			}
			goto IL_00e8;
			IL_00e8:
			if (!TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true))
			{
				((Character)npc).movementPause = 0;
			}
			pending.PostDialogueLingerActive = false;
			pending.WaitingForPostDialogueRestore = false;
			return;
			IL_00a1:
			if (!num2)
			{
				CaptureNpcSpecialActionBeforeOutfit(npc, pending);
				if (((Character)npc).movementPause < 6)
				{
					((Character)npc).movementPause = 6;
				}
				AnimatedSprite sprite = ((Character)npc).Sprite;
				if (sprite != null)
				{
					sprite.StopAnimation();
				}
				FacePlayerIfSafe(npc);
				return;
			}
			goto IL_00e8;
		}

		private void CaptureQueuedDialoguesBeforeOutfit(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null)
			{
				return;
			}
			try
			{
				pending.DialogueBackupBeforeOutfit = npc.CurrentDialogue?.ToList() ?? new List<Dialogue>();
				pending.HasDialogueBackup = pending.DialogueBackupBeforeOutfit.Count > 0;
			}
			catch (Exception ex)
			{
				pending.DialogueBackupBeforeOutfit = new List<Dialogue>();
				pending.HasDialogueBackup = false;
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] Could not capture existing dialogue for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
		}

		private void RestoreQueuedDialoguesAfterOutfit(NPC npc, PendingPrompt pending, bool clearCurrentDialogue)
		{
			if (npc == null || pending == null)
			{
				return;
			}
			if (!pending.HasDialogueBackup || pending.DialogueBackupBeforeOutfit == null || pending.DialogueBackupBeforeOutfit.Count <= 0)
			{
				if (clearCurrentDialogue)
				{
					npc.CurrentDialogue.Clear();
				}
				return;
			}
			try
			{
				if (clearCurrentDialogue)
				{
					npc.CurrentDialogue.Clear();
				}
				for (int num = pending.DialogueBackupBeforeOutfit.Count - 1; num >= 0; num--)
				{
					npc.CurrentDialogue.Push(pending.DialogueBackupBeforeOutfit[num]);
				}
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log($"[NPC OUTFIT] Restored {pending.DialogueBackupBeforeOutfit.Count} previous dialogue(s) for {((Character)npc).Name} after outfit reaction.", (LogLevel)2);
					}
				}
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("[NPC OUTFIT] Could not restore previous dialogue for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
		}

		private void TemporarilySkipFirstDailyDialogue(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null || Game1.player == null)
			{
				return;
			}
			try
			{
				Friendship val = default(Friendship);
				if (!((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).TryGetValue(((Character)npc).Name, ref val) || val == null)
				{
					return;
				}
				pending.HasFriendshipEntry = true;
				pending.OriginalTalkedToToday = val.TalkedToToday;
				if (val.TalkedToToday)
				{
					return;
				}
				val.TalkedToToday = true;
				pending.ForcedTalkedToToday = true;
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] Temporarily skipped first daily dialogue for " + ((Character)npc).Name + " so the outfit reaction can play first.", (LogLevel)2);
					}
				}
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("[NPC OUTFIT] Could not temporarily skip first daily dialogue for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
		}

		private void RestoreTalkedToTodayIfUnread(NPC npc, PendingPrompt pending)
		{
			if (npc == null || pending == null || Game1.player == null || !pending.HasFriendshipEntry || !pending.ForcedTalkedToToday)
			{
				return;
			}
			try
			{
				Friendship val = default(Friendship);
				if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).TryGetValue(((Character)npc).Name, ref val) && val != null)
				{
					val.TalkedToToday = pending.OriginalTalkedToToday;
				}
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[NPC OUTFIT] Could not restore first daily dialogue state for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
		}

		private void CancelPendingPrompt(NPC npc, PendingPrompt pending)
		{
			if (npc != null && pending != null)
			{
				if (pending.DialogueQueued)
				{
					RemoveQueuedDialogueIfStillPending(npc, pending);
					RestoreQueuedDialoguesAfterOutfit(npc, pending, clearCurrentDialogue: true);
					clearOutfitPrompt?.Invoke(npc);
					RestoreTalkedToTodayIfUnread(npc, pending);
				}
				rollCooldowns[((Character)npc).Name] = 600;
				if (!TryRestoreNpcSpecialActionAfterOutfit(npc, pending, force: true) && pending.WasLookingAtPlayer)
				{
					FaceDirectionIfSafe(npc, pending.OriginalFacingDirection);
				}
			}
		}

		private void ClearAllPendingPrompts(bool removeQueuedDialogues)
		{
			foreach (KeyValuePair<string, PendingPrompt> item in pendingPrompts.ToList())
			{
				string key = item.Key;
				PendingPrompt value = item.Value;
				NPC characterFromName = Game1.getCharacterFromName(key, true, false);
				if (characterFromName != null)
				{
					if (removeQueuedDialogues && value.DialogueQueued)
					{
						RemoveQueuedDialogueIfStillPending(characterFromName, value);
						RestoreQueuedDialoguesAfterOutfit(characterFromName, value, clearCurrentDialogue: true);
					}
					if (value.DialogueQueued)
					{
						clearOutfitPrompt?.Invoke(characterFromName);
						RestoreTalkedToTodayIfUnread(characterFromName, value);
					}
					if (!TryRestoreNpcSpecialActionAfterOutfit(characterFromName, value, force: true) && value.WasLookingAtPlayer)
					{
						FaceDirectionIfSafe(characterFromName, value.OriginalFacingDirection);
					}
				}
			}
			pendingPrompts.Clear();
		}

		private void RemoveQueuedDialogueIfStillPending(NPC npc, PendingPrompt pending)
		{
			if (npc != null && pending != null && pending.DialogueQueued && npc.CurrentDialogue.Count == pending.DialogueCountAfterPush && pending.DialogueCountAfterPush == pending.DialogueCountBeforePush + 1)
			{
				npc.CurrentDialogue.Pop();
			}
		}

		private int TryGetAnimationFrameIndex(AnimationFrame frame)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				object obj = frame;
				FieldInfo field = obj.GetType().GetField("frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null && field.GetValue(obj) is int result)
				{
					return result;
				}
				PropertyInfo property = obj.GetType().GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null && property.GetValue(obj) is int result2)
				{
					return result2;
				}
			}
			catch
			{
			}
			return -1;
		}

		private bool AnimationLooksLikeSpecialAction(List<AnimationFrame> animation)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (animation == null || animation.Count <= 0)
			{
				return false;
			}
			foreach (AnimationFrame item in animation)
			{
				int num = TryGetAnimationFrameIndex(item);
				if (num >= 16)
				{
					return true;
				}
			}
			return false;
		}

		private void CaptureNpcSpecialActionBeforeOutfit(NPC npc, PendingPrompt pending)
		{
			//IL_0388: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || pending == null || ((Character)npc).Sprite == null || ((Character)npc).currentLocation == null || pending.SpecialActionSnapshot != null || ((Character)npc).isMoving())
			{
				return;
			}
			List<AnimationFrame> list = null;
			if (((Character)npc).Sprite.CurrentAnimation != null && ((Character)npc).Sprite.CurrentAnimation.Count > 0)
			{
				list = new List<AnimationFrame>(((Character)npc).Sprite.CurrentAnimation);
			}
			bool flag = list != null && list.Count > 0;
			bool flag2 = ((Character)npc).Sprite.CurrentFrame >= 16;
			if (!flag && !flag2)
			{
				return;
			}
			pending.SpecialActionSnapshot = new NpcOutfitSpecialActionSnapshot
			{
				Npc = npc,
				Location = ((Character)npc).currentLocation,
				FacingDirection = ((Character)npc).FacingDirection,
				CurrentFrame = ((Character)npc).Sprite.CurrentFrame,
				Flip = ((Character)npc).flip,
				MovementPause = ((Character)npc).movementPause,
				AddedSpeed = (int)((Character)npc).addedSpeed,
				CurrentAnimation = list
			};
			((Character)npc).Sprite.StopAnimation();
			((Character)npc).Sprite.ClearAnimation();
			((Character)npc).Sprite.CurrentAnimation = null;
			((Character)npc).flip = false;
			((Character)npc).Sprite.CurrentFrame = GetNpcIdleFrameForDirection(((Character)npc).FacingDirection);
			string text = TryGetNetStringField(npc, "endOfRouteBehaviorName");
			bool flag3 = !string.IsNullOrEmpty(text) && text.IndexOf("fish", StringComparison.OrdinalIgnoreCase) >= 0;
			if (flag3)
			{
				pending.SpecialActionSnapshot.SavedIgnoreSourceRectUpdates = TryGetPrivateField(((Character)npc).Sprite, "ignoreSourceRectUpdates") as bool? == true;
				pending.SpecialActionSnapshot.SavedSpriteWidth = (TryGetPrivateField(((Character)npc).Sprite, "spriteWidth") as int?) ?? ((Character)npc).Sprite.SpriteWidth;
				pending.SpecialActionSnapshot.SavedTempSpriteHeight = (TryGetPrivateField(((Character)npc).Sprite, "tempSpriteHeight") as int?) ?? (-1);
				pending.SpecialActionSnapshot.HasSavedSpriteDimensions = true;
				TrySetSpritePrivateField(((Character)npc).Sprite, "ignoreSourceRectUpdates", false);
				TrySetSpritePrivateField(((Character)npc).Sprite, "spriteWidth", 16);
				TrySetSpritePrivateField(((Character)npc).Sprite, "tempSpriteHeight", -1);
				pending.SpecialActionSnapshot.SavedDoingEndOfRouteAnimation = TryGetNetBoolField(npc, "doingEndOfRouteAnimation");
				pending.SpecialActionSnapshot.SavedCurrentlyDoingEndOfRouteAnimation = TryGetPrivateField(npc, "currentlyDoingEndOfRouteAnimation") as bool?;
				pending.SpecialActionSnapshot.SavedStartedEndOfRouteBehavior = text;
				TrySetNetBoolField(npc, "doingEndOfRouteAnimation", value: false);
				TrySetSpritePrivateField(npc, "currentlyDoingEndOfRouteAnimation", false);
				pending.SpecialActionSnapshot.SavedYOffset = (TryGetPrivateField(npc, "yOffset") as float?).GetValueOrDefault();
				pending.SpecialActionSnapshot.SavedLoadedEndOfRouteBehavior = TryGetPrivateField(npc, "loadedEndOfRouteBehavior") as string;
				pending.SpecialActionSnapshot.SavedDrawOffset = (Vector2)(((??)(TryGetPrivateField(npc, "drawOffset") as Vector2?)) ?? Vector2.Zero);
				pending.SpecialActionSnapshot.HasSavedRodLayerFields = true;
				TrySetSpritePrivateField(npc, "yOffset", 0f);
				TrySetSpritePrivateField(npc, "loadedEndOfRouteBehavior", null);
				TrySetSpritePrivateField(npc, "drawOffset", Vector2.Zero);
			}
			((Character)npc).Sprite.UpdateSourceRect();
			if (ModEntry.DebugLog)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log($"[NPC OUTFIT] Saved special animation for {((Character)npc).Name} before outfit reaction. frame={pending.SpecialActionSnapshot.CurrentFrame} anim={list?.Count ?? 0} fishing={flag3}", (LogLevel)2);
				}
			}
		}

		private bool TryRestoreNpcSpecialActionAfterOutfit(NPC npc, PendingPrompt pending, bool force = false)
		{
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			if (pending == null || pending.SpecialActionSnapshot == null)
			{
				return false;
			}
			NpcOutfitSpecialActionSnapshot specialActionSnapshot = pending.SpecialActionSnapshot;
			if (npc == null)
			{
				npc = specialActionSnapshot.Npc;
			}
			if (npc == null || ((Character)npc).Sprite == null || ((Character)npc).currentLocation == null)
			{
				pending.SpecialActionSnapshot = null;
				return false;
			}
			if (npc != specialActionSnapshot.Npc || ((Character)npc).currentLocation != specialActionSnapshot.Location)
			{
				pending.SpecialActionSnapshot = null;
				return false;
			}
			if (!force)
			{
				if (Game1.activeClickableMenu != null || Game1.dialogueUp)
				{
					return false;
				}
				if (Game1.player != null && ((Character)npc).currentLocation == ((Character)Game1.player).currentLocation && DistanceToPlayer(npc) < 300f)
				{
					return false;
				}
			}
			try
			{
				((Character)npc).FacingDirection = specialActionSnapshot.FacingDirection;
				((Character)npc).flip = specialActionSnapshot.Flip;
				((Character)npc).movementPause = specialActionSnapshot.MovementPause;
				((Character)npc).addedSpeed = specialActionSnapshot.AddedSpeed;
				if (specialActionSnapshot.HasSavedSpriteDimensions)
				{
					TrySetSpritePrivateField(((Character)npc).Sprite, "spriteWidth", specialActionSnapshot.SavedSpriteWidth);
					TrySetSpritePrivateField(((Character)npc).Sprite, "tempSpriteHeight", specialActionSnapshot.SavedTempSpriteHeight);
					TrySetSpritePrivateField(((Character)npc).Sprite, "ignoreSourceRectUpdates", specialActionSnapshot.SavedIgnoreSourceRectUpdates);
					specialActionSnapshot.HasSavedSpriteDimensions = false;
				}
				if (specialActionSnapshot.SavedDoingEndOfRouteAnimation.HasValue)
				{
					TrySetNetBoolField(npc, "doingEndOfRouteAnimation", specialActionSnapshot.SavedDoingEndOfRouteAnimation.Value);
					TrySetSpritePrivateField(npc, "currentlyDoingEndOfRouteAnimation", specialActionSnapshot.SavedCurrentlyDoingEndOfRouteAnimation == true);
					specialActionSnapshot.SavedDoingEndOfRouteAnimation = null;
					specialActionSnapshot.SavedCurrentlyDoingEndOfRouteAnimation = null;
				}
				if (specialActionSnapshot.HasSavedRodLayerFields)
				{
					TrySetSpritePrivateField(npc, "yOffset", specialActionSnapshot.SavedYOffset);
					TrySetSpritePrivateField(npc, "loadedEndOfRouteBehavior", specialActionSnapshot.SavedLoadedEndOfRouteBehavior);
					TrySetSpritePrivateField(npc, "drawOffset", specialActionSnapshot.SavedDrawOffset);
					specialActionSnapshot.HasSavedRodLayerFields = false;
				}
				if (specialActionSnapshot.CurrentAnimation != null && specialActionSnapshot.CurrentAnimation.Count > 0)
				{
					((Character)npc).Sprite.CurrentAnimation = new List<AnimationFrame>(specialActionSnapshot.CurrentAnimation);
					TrySetSpritePrivateField(((Character)npc).Sprite, "currentAnimationIndex", 0);
					TrySetSpritePrivateField(((Character)npc).Sprite, "timer", 0);
				}
				else
				{
					((Character)npc).Sprite.StopAnimation();
					((Character)npc).Sprite.ClearAnimation();
					((Character)npc).Sprite.CurrentAnimation = null;
				}
				((Character)npc).Sprite.CurrentFrame = specialActionSnapshot.CurrentFrame;
				((Character)npc).Sprite.UpdateSourceRect();
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log($"[NPC OUTFIT] Restored special animation for {((Character)npc).Name} after outfit reaction. frame={specialActionSnapshot.CurrentFrame} anim={((specialActionSnapshot.CurrentAnimation != null) ? specialActionSnapshot.CurrentAnimation.Count : 0)}", (LogLevel)2);
					}
				}
				if (!string.IsNullOrEmpty(specialActionSnapshot.SavedStartedEndOfRouteBehavior))
				{
					string behaviorName = specialActionSnapshot.SavedStartedEndOfRouteBehavior;
					specialActionSnapshot.SavedStartedEndOfRouteBehavior = null;
					NPC npcForDelay = npc;
					DelayedAction.functionAfterDelay((Action)delegate
					{
						NPC obj3 = npcForDelay;
						if (((obj3 != null) ? ((Character)obj3).currentLocation : null) == null || ((Character)npcForDelay).Sprite == null)
						{
							return;
						}
						try
						{
							TrySetSpritePrivateField(npcForDelay, "_startedEndOfRouteBehavior", behaviorName);
							((object)npcForDelay).GetType().GetMethod("doMiddleAnimation", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(npcForDelay, new object[1]);
						}
						catch (Exception value)
						{
							IMonitor obj4 = monitor;
							if (obj4 != null)
							{
								obj4.Log($"[NPC OUTFIT] Failed to re-run doMiddleAnimation for {((Character)npcForDelay).Name}: {value}", (LogLevel)3);
							}
						}
					}, 150);
				}
				pending.SpecialActionSnapshot = null;
				return true;
			}
			catch (Exception ex)
			{
				IMonitor obj2 = monitor;
				if (obj2 != null)
				{
					obj2.Log("[NPC OUTFIT] Could not restore special animation for " + (((npc != null) ? ((Character)npc).Name : null) ?? "null") + ": " + ex.Message, (LogLevel)3);
				}
				pending.SpecialActionSnapshot = null;
				return false;
			}
		}

		private void TrySetSpritePrivateField(object target, string fieldName, object value)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return;
			}
			try
			{
				FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					field.SetValue(target, value);
				}
			}
			catch
			{
			}
		}

		private object TryGetPrivateField(object target, string fieldName)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return null;
			}
			try
			{
				return target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target);
			}
			catch
			{
				return null;
			}
		}

		private void TrySetNetBoolField(object target, string fieldName, bool value)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return;
			}
			try
			{
				object obj = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target);
				if (obj != null)
				{
					PropertyInfo property = obj.GetType().GetProperty("Value");
					if (property != null && property.CanWrite)
					{
						property.SetValue(obj, value);
					}
				}
			}
			catch
			{
			}
		}

		private bool? TryGetNetBoolField(object target, string fieldName)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return null;
			}
			try
			{
				object obj = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target);
				if (obj == null)
				{
					return null;
				}
				return obj.GetType().GetProperty("Value")?.GetValue(obj) as bool?;
			}
			catch
			{
				return null;
			}
		}

		private string TryGetNetStringField(object target, string fieldName)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return null;
			}
			try
			{
				object obj = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(target);
				if (obj == null)
				{
					return null;
				}
				return obj.GetType().GetProperty("Value")?.GetValue(obj) as string;
			}
			catch
			{
				return null;
			}
		}

		private int GetNpcIdleFrameForDirection(int facingDirection)
		{
			return facingDirection switch
			{
				0 => 8, 
				1 => 4, 
				2 => 0, 
				3 => 12, 
				_ => 0, 
			};
		}

		private void UpdateSpyingNpcs(float noticeDistance, float cancelDistance)
		{
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			if (Game1.player == null)
			{
				return;
			}
			foreach (string name in spyingNpcs.Keys.ToList())
			{
				NPC val = ((IEnumerable<NPC>)Game1.currentLocation?.characters).FirstOrDefault((NPC c) => c != null && ((Character)c).Name != null && ((Character)c).Name.Equals(name, StringComparison.OrdinalIgnoreCase));
				if (val == null || ((Character)val).currentLocation != ((Character)Game1.player).currentLocation || DistanceToPlayer(val) > cancelDistance)
				{
					spyingNpcs.Remove(name);
					continue;
				}
				SpyingState spyingState = spyingNpcs[name];
				if (spyingState.WalkCooldownTimer > 0)
				{
					spyingState.WalkCooldownTimer--;
					continue;
				}
				if (DistanceToPlayer(val) > noticeDistance)
				{
					spyingState.WalkCooldownTimer = 60;
					continue;
				}
				if (spyingState.IsBeingWatched)
				{
					if (spyingState.PretendTimer > 0)
					{
						spyingState.PretendTimer--;
						continue;
					}
					if (IsPlayerFacingNpc(val))
					{
						spyingState.PretendTimer = 12;
						continue;
					}
					spyingState.IsBeingWatched = false;
					spyingState.WalkCooldownTimer = 120;
					continue;
				}
				if (((Character)val).movementPause < 6)
				{
					((Character)val).movementPause = 6;
				}
				((Character)val).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false, false);
				if (((Character)val).Sprite != null)
				{
					((Character)val).Sprite.StopAnimation();
					((Character)val).Sprite.CurrentFrame = GetNpcIdleFrameForDirection(((Character)val).FacingDirection);
					((Character)val).Sprite.UpdateSourceRect();
				}
				if (spyingState.PeekGraceTimer > 0)
				{
					spyingState.PeekGraceTimer--;
				}
				else if (IsPlayerFacingNpc(val))
				{
					spyingState.IsBeingWatched = true;
					spyingState.WasEverCaught = true;
					spyingState.PretendTimer = 15;
					if (!pendingPrompts.TryGetValue(name, out var value) || value == null)
					{
						ArmPendingReactionForSpy(val);
						pendingPrompts.TryGetValue(name, out value);
					}
					if (value != null)
					{
						value.WasCaughtPeeking = true;
					}
					if (random.Next(2) == 0)
					{
						((Character)val).doEmote(28, true);
					}
					else
					{
						((Character)val).doEmote(16, true);
					}
					((Character)val).movementPause = 0;
				}
			}
		}

		private static bool IsPlayerFacingNpc(NPC npc)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			Vector2 standingPosition = ((Character)Game1.player).getStandingPosition();
			Vector2 standingPosition2 = ((Character)npc).getStandingPosition();
			Vector2 val = standingPosition2 - standingPosition;
			if (((Vector2)(ref val)).LengthSquared() < 256f)
			{
				return true;
			}
			int facingDirection = ((Character)Game1.player).FacingDirection;
			if (1 == 0)
			{
			}
			bool result = facingDirection switch
			{
				0 => val.Y < 0f, 
				1 => val.X > 0f, 
				2 => val.Y > 0f, 
				3 => val.X < 0f, 
				_ => true, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private static bool HasLineOfSightToPlayer(NPC npc)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null || ((Character)npc).currentLocation == null)
			{
				return false;
			}
			GameLocation currentLocation = ((Character)npc).currentLocation;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))..ctor((float)(int)(((Character)npc).Position.X / 64f), (float)(int)(((Character)npc).Position.Y / 64f));
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor((float)(int)(((Character)Game1.player).Position.X / 64f), (float)(int)(((Character)Game1.player).Position.Y / 64f));
			float num = val2.X - val.X;
			float num2 = val2.Y - val.Y;
			int num3 = (int)Math.Max(Math.Abs(num), Math.Abs(num2));
			if (num3 <= 1)
			{
				return true;
			}
			Location val3 = default(Location);
			for (int i = 1; i < num3; i++)
			{
				float num4 = (float)i / (float)num3;
				int num5 = (int)Math.Round(val.X + num * num4);
				int num6 = (int)Math.Round(val.Y + num2 * num4);
				try
				{
					((Location)(ref val3))..ctor(num5, num6);
					if (!currentLocation.isTilePassable(val3, Game1.viewport))
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsNpcFacingPlayer(NPC npc)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			Vector2 standingPosition = ((Character)npc).getStandingPosition();
			Vector2 standingPosition2 = ((Character)Game1.player).getStandingPosition();
			Vector2 val = standingPosition2 - standingPosition;
			if (((Vector2)(ref val)).LengthSquared() < 256f)
			{
				return true;
			}
			int facingDirection = ((Character)npc).FacingDirection;
			if (1 == 0)
			{
			}
			bool result = facingDirection switch
			{
				0 => val.Y < 0f, 
				1 => val.X > 0f, 
				2 => val.Y > 0f, 
				3 => val.X < 0f, 
				_ => true, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private float DistanceToPlayer(NPC npc)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return float.MaxValue;
			}
			return Vector2.Distance(((Character)npc).Position, ((Character)Game1.player).Position);
		}

		private bool FacePlayerIfSafe(NPC npc)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null)
			{
				return false;
			}
			if (((Character)npc).currentLocation != ((Character)Game1.player).currentLocation)
			{
				return false;
			}
			if (((Character)npc).isMoving())
			{
				return false;
			}
			((Character)npc).faceGeneralDirection(((Character)Game1.player).getStandingPosition(), 0, false, false);
			return true;
		}

		private bool FaceDirectionIfSafe(NPC npc, int direction)
		{
			if (npc == null)
			{
				return false;
			}
			if (((Character)npc).isMoving())
			{
				return false;
			}
			((Character)npc).faceDirection(direction);
			return true;
		}
	}
	internal sealed class OutfitPlayerReplyChoiceMenu : IClickableMenu
	{
		private readonly string title;

		private readonly string replyLabel;

		private readonly string leaveLabel;

		private readonly Action respond;

		private readonly Action leave;

		private readonly ClickableComponent replyButton;

		private readonly ClickableComponent leaveButton;

		public OutfitPlayerReplyChoiceMenu(string title, string replyLabel, string leaveLabel, Action respond, Action leave)
			: base((((Rectangle)(ref Game1.uiViewport)).Width - 760) / 2, Math.Max(64, ((Rectangle)(ref Game1.uiViewport)).Height - 360), 760, 260, true)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Expected O, but got Unknown
			this.title = title ?? "Reply?";
			this.replyLabel = replyLabel ?? "Reply";
			this.leaveLabel = leaveLabel ?? "Leave";
			this.respond = respond;
			this.leave = leave;
			int num = 280;
			int num2 = 64;
			int num3 = base.yPositionOnScreen + 140;
			replyButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 90, num3, num, num2), "reply");
			leaveButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width - 90 - num, num3, num, num2), "leave");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (replyButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect", (int?)null);
				respond?.Invoke();
			}
			else if (leaveButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect", (int?)null);
				leave?.Invoke();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Invalid comparison between Unknown and I4
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Invalid comparison between Unknown and I4
			if ((int)key == 27)
			{
				Game1.playSound("smallSelect", (int?)null);
				leave?.Invoke();
			}
			else if ((int)key == 13)
			{
				Game1.playSound("smallSelect", (int?)null);
				respond?.Invoke();
			}
		}

		public override void draw(SpriteBatch b)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 1f, true, -1f);
			SpriteFont dialogueFont = Game1.dialogueFont;
			int maxWidth = base.width - 128;
			List<string> list = WrapText(dialogueFont, title, maxWidth);
			float num = dialogueFont.MeasureString("A").Y + 2f;
			float num2 = (float)list.Count * num;
			float num3 = (float)base.yPositionOnScreen + (140f - num2) / 2f;
			foreach (string item in list)
			{
				float x = dialogueFont.MeasureString(item).X;
				float num4 = (float)base.xPositionOnScreen + ((float)base.width - x) / 2f;
				Utility.drawTextWithShadow(b, item, dialogueFont, new Vector2(num4, num3), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				num3 += num;
			}
			DrawButton(b, replyButton.bounds, replyLabel);
			DrawButton(b, leaveButton.bounds, leaveLabel);
			((IClickableMenu)this).drawMouse(b, false, -1);
		}

		private static List<string> WrapText(SpriteFont font, string text, int maxWidth)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			List<string> list = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return list;
			}
			string[] array = text.Split(' ');
			string text2 = "";
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				string text4 = (string.IsNullOrEmpty(text2) ? text3 : (text2 + " " + text3));
				if (font.MeasureString(text4).X > (float)maxWidth && !string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
					text2 = text3;
				}
				else
				{
					text2 = text4;
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				list.Add(text2);
			}
			return list;
		}

		private static void DrawButton(SpriteBatch b, Rectangle bounds, string label)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White, 1f, true, -1f);
			Vector2 val = Game1.smallFont.MeasureString(label ?? "");
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor((float)((Rectangle)(ref bounds)).Center.X - val.X / 2f, (float)((Rectangle)(ref bounds)).Center.Y - val.Y / 2f + 2f);
			Utility.drawTextWithShadow(b, label ?? "", Game1.smallFont, val2, Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
		}
	}
	internal sealed class OutfitPlayerReplyTextInputMenu : IClickableMenu
	{
		private readonly string title;

		private readonly string sendLabel;

		private readonly string cancelLabel;

		private readonly Action<string> submit;

		private readonly Action cancel;

		private readonly TextBox textBox;

		private readonly ClickableComponent sendButton;

		private readonly ClickableComponent cancelButton;

		private string inputText = "";

		private double caretBlinkTimer = 0.0;

		private bool caretVisible = true;

		private bool backspaceWasDown = false;

		private double backspaceHeldTimer = 0.0;

		private double backspaceRepeatTimer = 0.0;

		private int InputAreaX => base.xPositionOnScreen + 64;

		private int InputAreaY => base.yPositionOnScreen + 110;

		private int InputAreaWidth => base.width - 128;

		private int InputAreaHeight => base.height - 110 - 100;

		public OutfitPlayerReplyTextInputMenu(string title, string sendLabel, string cancelLabel, Action<string> submit, Action cancel)
			: base((((Rectangle)(ref Game1.uiViewport)).Width - Math.Min(1200, ((Rectangle)(ref Game1.uiViewport)).Width - 96)) / 2, Math.Max(48, ((Rectangle)(ref Game1.uiViewport)).Height - 520), Math.Min(1200, ((Rectangle)(ref Game1.uiViewport)).Width - 96), 420, true)
		{
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Expected O, but got Unknown
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Expected O, but got Unknown
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Expected O, but got Unknown
			this.title = title ?? "Write your reply:";
			this.sendLabel = sendLabel ?? "Send";
			this.cancelLabel = cancelLabel ?? "Cancel";
			this.submit = submit;
			this.cancel = cancel;
			Texture2D val = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
			textBox = new TextBox(val, (Texture2D)null, Game1.smallFont, Game1.textColor)
			{
				X = -9999,
				Y = -9999,
				Width = base.width - 128,
				Height = 64,
				Selected = true,
				Text = ""
			};
			textBox.textLimit = 800;
			int num = 220;
			int num2 = 56;
			int num3 = base.yPositionOnScreen + base.height - 84;
			sendButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width - 64 - num, num3, num, num2), "send");
			cancelButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64, num3, num, num2), "cancel");
			Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)(object)textBox;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			textBox.Selected = true;
			Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)(object)textBox;
			if (sendButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect", (int?)null);
				submit?.Invoke(inputText);
			}
			else if (cancelButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect", (int?)null);
				DoCancel();
			}
		}

		private void DoCancel()
		{
			Game1.keyboardDispatcher.Subscriber = null;
			if ((object)Game1.activeClickableMenu == this)
			{
				Game1.activeClickableMenu = null;
			}
			cancel?.Invoke();
		}

		public override void receiveKeyPress(Keys key)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Invalid comparison between Unknown and I4
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Invalid comparison between Unknown and I4
			if ((int)key == 27)
			{
				Game1.playSound("smallSelect", (int?)null);
				DoCancel();
			}
			else if ((int)key == 13)
			{
				if (((KeyboardState)(ref Game1.oldKBState)).IsKeyDown((Keys)160) || ((KeyboardState)(ref Game1.oldKBState)).IsKeyDown((Keys)161))
				{
					inputText += "\n";
					return;
				}
				Game1.playSound("smallSelect", (int?)null);
				submit?.Invoke(inputText);
			}
			else if ((int)key == 8 && inputText.Length > 0)
			{
				string text = inputText;
				inputText = text.Substring(0, text.Length - 1);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if ((int)b == 8192 || (int)b == 32)
			{
				Game1.playSound("smallSelect", (int?)null);
				DoCancel();
			}
		}

		public override void update(GameTime time)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			((IClickableMenu)this).update(time);
			string text = textBox.Text ?? "";
			if (text.Length > 0)
			{
				inputText += text;
				textBox.Text = "";
			}
			double totalSeconds = time.ElapsedGameTime.TotalSeconds;
			KeyboardState state = Keyboard.GetState();
			bool flag = ((KeyboardState)(ref state)).IsKeyDown((Keys)8);
			if (flag && backspaceWasDown)
			{
				backspaceHeldTimer += totalSeconds;
				if (backspaceHeldTimer >= 0.4)
				{
					backspaceRepeatTimer += totalSeconds;
					if (backspaceRepeatTimer >= 0.03)
					{
						backspaceRepeatTimer = 0.0;
						if (inputText.Length > 0)
						{
							string text2 = inputText;
							inputText = text2.Substring(0, text2.Length - 1);
						}
					}
				}
			}
			else
			{
				backspaceHeldTimer = 0.0;
				backspaceRepeatTimer = 0.0;
			}
			backspaceWasDown = flag;
			caretBlinkTimer += totalSeconds;
			if (caretBlinkTimer >= 0.5)
			{
				caretBlinkTimer = 0.0;
				caretVisible = !caretVisible;
			}
		}

		public override void draw(SpriteBatch b)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			Texture2D fadeToBlackRect = Game1.fadeToBlackRect;
			Viewport viewport = Game1.graphics.GraphicsDevice.Viewport;
			b.Draw(fadeToBlackRect, ((Viewport)(ref viewport)).Bounds, Color.Black * 0.4f);
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 1f, false, -1f);
			Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2((float)(base.xPositionOnScreen + 64), (float)(base.yPositionOnScreen + 48)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			int inputAreaX = InputAreaX;
			int inputAreaY = InputAreaY;
			int inputAreaWidth = InputAreaWidth;
			int inputAreaHeight = InputAreaHeight;
			b.Draw(Game1.staminaRect, new Rectangle(inputAreaX, inputAreaY, inputAreaWidth, inputAreaHeight), new Color(240, 200, 200));
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), inputAreaX, inputAreaY, inputAreaWidth, inputAreaHeight, Color.White * 0.6f, 1f, false, -1f);
			SpriteFont smallFont = Game1.smallFont;
			float num = smallFont.MeasureString("A").Y + 2f;
			int num2 = 12;
			int maxWidth = inputAreaWidth - num2 * 2;
			string text = inputText + (caretVisible ? "|" : " ");
			List<string> list = WrapText(smallFont, text, maxWidth);
			float num3 = inputAreaY + num2;
			int num4 = (int)((float)(inputAreaHeight - num2 * 2) / num);
			int num5 = Math.Max(0, list.Count - num4);
			for (int i = num5; i < list.Count; i++)
			{
				b.DrawString(smallFont, list[i], new Vector2((float)(inputAreaX + num2), num3), Game1.textColor);
				num3 += num;
			}
			DrawButton(b, cancelButton.bounds, cancelLabel);
			DrawButton(b, sendButton.bounds, sendLabel);
			((IClickableMenu)this).drawMouse(b, false, -1);
		}

		private static List<string> WrapText(SpriteFont font, string text, int maxWidth)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			List<string> list = new List<string>();
			string[] array = text.Split('\n');
			foreach (string text2 in array)
			{
				string[] array2 = text2.Split(' ');
				string text3 = "";
				string[] array3 = array2;
				foreach (string text4 in array3)
				{
					string text5 = (string.IsNullOrEmpty(text3) ? text4 : (text3 + " " + text4));
					if (font.MeasureString(text5).X > (float)maxWidth && !string.IsNullOrEmpty(text3))
					{
						list.Add(text3);
						text3 = text4;
					}
					else
					{
						text3 = text5;
					}
				}
				list.Add(text3);
			}
			return list;
		}

		private static void DrawButton(SpriteBatch b, Rectangle bounds, string label)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White, 1f, true, -1f);
			Vector2 val = Game1.smallFont.MeasureString(label ?? "");
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor((float)((Rectangle)(ref bounds)).Center.X - val.X / 2f, (float)((Rectangle)(ref bounds)).Center.Y - val.Y / 2f + 2f);
			Utility.drawTextWithShadow(b, label ?? "", Game1.smallFont, val2, Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
		}
	}
	internal sealed class SpouseOutfitReactionProgressState
	{
		public bool IsReacting { get; set; }

		public int InteractionCooldown { get; set; }

		public bool PathStarted { get; set; }

		public bool ComplimentReady { get; set; }

		public Point PreferredOffset { get; set; } = Point.Zero;

		public Point LastPlayerTile { get; set; } = Point.Zero;

		public Point LastTargetTile { get; set; } = Point.Zero;

		public bool FirstNoticeDone { get; set; }

		public bool EmoteFired { get; set; }

		public int NoticePauseTimer { get; set; }

		public bool PlayerWasInNoticeRange { get; set; }

		public int SecondNoticeCooldown { get; set; }

		public int ChaseTimer { get; set; }

		public NPC ReactingNpc { get; set; }

		public bool SequenceActive { get; set; }

		public void AdvanceTimers()
		{
			if (NoticePauseTimer > 0)
			{
				NoticePauseTimer--;
			}
			if (ChaseTimer > 0)
			{
				ChaseTimer--;
			}
			if (SecondNoticeCooldown > 0)
			{
				SecondNoticeCooldown--;
			}
			if (InteractionCooldown > 0)
			{
				InteractionCooldown--;
			}
		}

		public void ClearCurrentReaction()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			IsReacting = false;
			PathStarted = false;
			ComplimentReady = false;
			PreferredOffset = Point.Zero;
			LastPlayerTile = Point.Zero;
			LastTargetTile = Point.Zero;
			FirstNoticeDone = false;
			EmoteFired = false;
			NoticePauseTimer = 0;
			PlayerWasInNoticeRange = false;
			ChaseTimer = 0;
			ReactingNpc = null;
			SequenceActive = false;
		}

		public void ClearAllProgress()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			IsReacting = false;
			PathStarted = false;
			ComplimentReady = false;
			FirstNoticeDone = false;
			EmoteFired = false;
			NoticePauseTimer = 0;
			SecondNoticeCooldown = 0;
			PlayerWasInNoticeRange = false;
			InteractionCooldown = 0;
			PreferredOffset = Point.Zero;
			LastPlayerTile = Point.Zero;
			LastTargetTile = Point.Zero;
			ReactingNpc = null;
			SequenceActive = false;
		}

		public void BeginFirstNotice()
		{
			SequenceActive = true;
			FirstNoticeDone = true;
			NoticePauseTimer = 90;
		}

		public void BeginApproach(NPC npc)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			SequenceActive = true;
			IsReacting = true;
			PathStarted = false;
			ComplimentReady = false;
			InteractionCooldown = 180;
			PreferredOffset = Point.Zero;
			LastPlayerTile = Point.Zero;
			LastTargetTile = Point.Zero;
			ChaseTimer = 420;
			ReactingNpc = npc;
		}

		public void BeginClickReady(NPC npc)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			SequenceActive = true;
			IsReacting = true;
			PathStarted = false;
			ComplimentReady = true;
			InteractionCooldown = 180;
			PreferredOffset = Point.Zero;
			LastPlayerTile = Point.Zero;
			LastTargetTile = Point.Zero;
			ChaseTimer = 0;
			ReactingNpc = npc;
		}

		public void KeepPendingAfterAiFailure(NPC npc)
		{
			SequenceActive = true;
			IsReacting = true;
			ComplimentReady = true;
			PathStarted = false;
			FirstNoticeDone = true;
			NoticePauseTimer = 0;
			SecondNoticeCooldown = Math.Max(SecondNoticeCooldown, 300);
			ChaseTimer = 0;
			ReactingNpc = npc;
		}

		public void MarkComplimentStarted(NPC npc, bool playerWasInNoticeRange)
		{
			SequenceActive = true;
			IsReacting = true;
			PathStarted = true;
			ComplimentReady = true;
			ChaseTimer = 0;
			ReactingNpc = npc;
			PlayerWasInNoticeRange = playerWasInNoticeRange;
		}
	}
	internal sealed class SpouseOutfitSpecialActionSnapshot
	{
		public NPC Npc { get; set; }

		public GameLocation Location { get; set; }

		public int FacingDirection { get; set; }

		public int CurrentFrame { get; set; }

		public bool Flip { get; set; }

		public int MovementPause { get; set; }

		public int AddedSpeed { get; set; }

		public List<AnimationFrame> CurrentAnimation { get; set; }
	}
	internal sealed class SpouseSpecialActionController
	{
		public SpouseOutfitSpecialActionSnapshot Current { get; private set; }

		public bool HasSnapshotFor(NPC npc)
		{
			return Current != null && Current.Npc == npc;
		}

		public void Capture(SpouseOutfitSpecialActionSnapshot snapshot)
		{
			Current = snapshot;
		}

		public void Clear()
		{
			Current = null;
		}

		public bool TryRestore(bool force, Farmer player, bool menuOpen, bool dialogueUp, Func<NPC, float> distanceToPlayer, float restoreDistance, IMonitor monitor, bool debugLog)
		{
			SpouseOutfitSpecialActionSnapshot current = Current;
			if (current == null || current.Npc == null)
			{
				return false;
			}
			NPC npc = current.Npc;
			if (((Character)npc).Sprite == null || ((Character)npc).currentLocation == null || ((Character)npc).currentLocation != current.Location)
			{
				Clear();
				return false;
			}
			if (!force)
			{
				if (menuOpen || dialogueUp)
				{
					return false;
				}
				if (player != null && ((Character)npc).currentLocation == ((Character)player).currentLocation && distanceToPlayer(npc) < restoreDistance)
				{
					return false;
				}
			}
			try
			{
				((Character)npc).FacingDirection = current.FacingDirection;
				((Character)npc).flip = current.Flip;
				((Character)npc).movementPause = current.MovementPause;
				((Character)npc).addedSpeed = current.AddedSpeed;
				if (current.CurrentAnimation != null && current.CurrentAnimation.Count > 0)
				{
					((Character)npc).Sprite.CurrentAnimation = new List<AnimationFrame>(current.CurrentAnimation);
					TrySetSpritePrivateField(((Character)npc).Sprite, "currentAnimationIndex", 0);
					TrySetSpritePrivateField(((Character)npc).Sprite, "timer", 0);
				}
				else
				{
					((Character)npc).Sprite.StopAnimation();
					((Character)npc).Sprite.ClearAnimation();
					((Character)npc).Sprite.CurrentAnimation = null;
				}
				((Character)npc).Sprite.CurrentFrame = current.CurrentFrame;
				((Character)npc).Sprite.UpdateSourceRect();
				if (debugLog)
				{
					monitor.Log($"[CLOTHES SPOUSE] Restored special animation for {((Character)npc).Name} after outfit reaction. frame={current.CurrentFrame} anim={((current.CurrentAnimation != null) ? current.CurrentAnimation.Count : 0)}", (LogLevel)2);
				}
				Clear();
				return true;
			}
			catch (Exception ex)
			{
				monitor.Log("[CLOTHES SPOUSE] Could not restore special animation for " + (((npc != null) ? ((Character)npc).Name : null) ?? "null") + ": " + ex.Message, (LogLevel)3);
				Clear();
				return false;
			}
		}

		private static void TrySetSpritePrivateField(object target, string fieldName, object value)
		{
			if (target == null || string.IsNullOrWhiteSpace(fieldName))
			{
				return;
			}
			try
			{
				target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(target, value);
			}
			catch
			{
			}
		}
	}
	internal sealed class SpouseRouteSnapshot
	{
		public Point? FinalDestination { get; set; }

		public endBehavior EndBehavior { get; set; }

		public int FinalFacingDirection { get; set; } = -1;

		public SchedulePathDescription Directions { get; set; }

		public void Clear()
		{
			FinalDestination = null;
			EndBehavior = null;
			FinalFacingDirection = -1;
			Directions = null;
		}
	}
	internal sealed class SpouseRouteController
	{
		private static readonly FieldInfo DirectionsToNewLocationField = typeof(NPC).GetField("directionsToNewLocation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public SpouseRouteSnapshot Snapshot { get; } = new SpouseRouteSnapshot();

		public bool HasRoute => Snapshot.FinalDestination.HasValue;

		public void Stop(NPC npc, IMonitor monitor, bool debugLog)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null)
			{
				return;
			}
			if (!HasRoute)
			{
				try
				{
					if (((Character)npc).controller != null)
					{
						Stack<Point> pathToEndPoint = ((Character)npc).controller.pathToEndPoint;
						if (pathToEndPoint != null && pathToEndPoint.Count > 0)
						{
							Point value = pathToEndPoint.Last();
							Snapshot.FinalDestination = value;
							Snapshot.EndBehavior = ((Character)npc).controller.endBehaviorFunction;
							Snapshot.FinalFacingDirection = ((Character)npc).controller.finalFacingDirection;
							Snapshot.Directions = GetDirections(npc);
							if (debugLog)
							{
								monitor.Log($"[CLOTHES SPOUSE] Captured destination {value} for {((Character)npc).Name}.", (LogLevel)2);
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (debugLog)
					{
						monitor.Log("[CLOTHES SPOUSE] Could not capture destination for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
					}
				}
			}
			((Character)npc).controller = null;
			((Character)npc).Halt();
			((Character)npc).Sprite.StopAnimation();
		}

		public void Restore(NPC npc, IMonitor monitor, bool debugLog)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || !HasRoute)
			{
				if (npc != null)
				{
					npc.checkSchedule(Game1.timeOfDay);
				}
				Clear();
				return;
			}
			try
			{
				Point value = Snapshot.FinalDestination.Value;
				PathFindController val = new PathFindController((Character)(object)npc, Utility.getGameLocationOfCharacter(npc), value, Snapshot.FinalFacingDirection, Snapshot.EndBehavior);
				if (val.pathToEndPoint != null && val.pathToEndPoint.Count > 0)
				{
					val.endBehaviorFunction = Snapshot.EndBehavior;
					((Character)npc).controller = val;
					if (Snapshot.Directions != null)
					{
						SetDirections(npc, Snapshot.Directions);
					}
					if (debugLog)
					{
						monitor.Log($"[CLOTHES SPOUSE] Restored {((Character)npc).Name}'s path to {value} ({val.pathToEndPoint.Count} steps).", (LogLevel)2);
					}
				}
				else
				{
					if (debugLog)
					{
						monitor.Log($"[CLOTHES SPOUSE] Could not pathfind to {value} for {((Character)npc).Name} — falling back to checkSchedule.", (LogLevel)2);
					}
					npc.checkSchedule(Game1.timeOfDay);
				}
			}
			catch (Exception ex)
			{
				if (debugLog)
				{
					monitor.Log($"[CLOTHES SPOUSE] Error restoring path for {((Character)npc).Name}: {ex.Message} — falling back to checkSchedule.", (LogLevel)2);
				}
				npc.checkSchedule(Game1.timeOfDay);
			}
			finally
			{
				Clear();
			}
		}

		public void Clear()
		{
			Snapshot.Clear();
		}

		private static SchedulePathDescription GetDirections(NPC npc)
		{
			try
			{
				object? obj = DirectionsToNewLocationField?.GetValue(npc);
				return (SchedulePathDescription)((obj is SchedulePathDescription) ? obj : null);
			}
			catch
			{
				return null;
			}
		}

		private static void SetDirections(NPC npc, SchedulePathDescription value)
		{
			try
			{
				DirectionsToNewLocationField?.SetValue(npc, value);
			}
			catch
			{
			}
		}
	}
	internal sealed class SpouseOutfitApproachController
	{
		public bool ShouldApproach(NPC npc)
		{
			return false;
		}

		public bool TryStartPath(NPC npc, IMonitor monitor, bool debugLog)
		{
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Expected O, but got Unknown
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || Game1.player == null || Game1.currentLocation == null)
			{
				return false;
			}
			Point tilePoint = ((Character)Game1.player).TilePoint;
			Point npcTile = ((Character)npc).TilePoint;
			int num = Math.Sign(npcTile.X - tilePoint.X);
			int num2 = Math.Sign(npcTile.Y - tilePoint.Y);
			List<Point> list = new List<Point>();
			if (Math.Abs(npcTile.X - tilePoint.X) > Math.Abs(npcTile.Y - tilePoint.Y))
			{
				if (num != 0)
				{
					list.Add(new Point(tilePoint.X + num, tilePoint.Y));
				}
				if (num2 != 0)
				{
					list.Add(new Point(tilePoint.X, tilePoint.Y + num2));
				}
			}
			else
			{
				if (num2 != 0)
				{
					list.Add(new Point(tilePoint.X, tilePoint.Y + num2));
				}
				if (num != 0)
				{
					list.Add(new Point(tilePoint.X + num, tilePoint.Y));
				}
			}
			list.Add(new Point(tilePoint.X + 1, tilePoint.Y));
			list.Add(new Point(tilePoint.X - 1, tilePoint.Y));
			list.Add(new Point(tilePoint.X, tilePoint.Y + 1));
			list.Add(new Point(tilePoint.X, tilePoint.Y - 1));
			list.Add(new Point(tilePoint.X + 1, tilePoint.Y + 1));
			list.Add(new Point(tilePoint.X - 1, tilePoint.Y + 1));
			list.Add(new Point(tilePoint.X + 1, tilePoint.Y - 1));
			list.Add(new Point(tilePoint.X - 1, tilePoint.Y - 1));
			foreach (Point item in from tile in list.Distinct()
				orderby Math.Abs(tile.X - npcTile.X) + Math.Abs(tile.Y - npcTile.Y)
				select tile)
			{
				if (item == npcTile || !Game1.currentLocation.isTilePassable(new Location(item.X, item.Y), Game1.viewport))
				{
					continue;
				}
				try
				{
					PathFindController val = new PathFindController((Character)(object)npc, Game1.currentLocation, item, -1, false);
					if (val?.pathToEndPoint != null && val.pathToEndPoint.Count > 0)
					{
						((Character)npc).controller = val;
						if (debugLog)
						{
							monitor.Log($"[CLOTHES SPOUSE] Started farmhouse approach path for {((Character)npc).Name} to {item} ({val.pathToEndPoint.Count} steps).", (LogLevel)2);
						}
						return true;
					}
				}
				catch (Exception ex)
				{
					if (debugLog)
					{
						monitor.Log($"[CLOTHES SPOUSE] Failed approach path candidate {item} for {((Character)npc).Name}: {ex.Message}", (LogLevel)2);
					}
				}
			}
			return false;
		}
	}
	internal sealed class SpouseOutfitNoticeController
	{
		public void UpdateHold(SpouseProximityState state, NPC npc, Farmer player, float distance, Action<NPC> captureSpecialAction)
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			if (npc == null || player == null || ((Character)npc).currentLocation != ((Character)player).currentLocation)
			{
				state.ClearNotice();
				return;
			}
			state.NoticePauseActive = SpouseOutfitReactionController.ResolveNoticePause(state.NoticePauseActive, isSameLocation: true, distance);
			if (!state.NoticePauseActive)
			{
				state.NoticeHoldPoseApplied = false;
				return;
			}
			captureSpecialAction(npc);
			if (((Character)npc).movementPause < 6)
			{
				((Character)npc).movementPause = 6;
			}
			if (!state.NoticeHoldPoseApplied)
			{
				AnimatedSprite sprite = ((Character)npc).Sprite;
				if (sprite != null)
				{
					sprite.StopAnimation();
				}
				((Character)npc).faceGeneralDirection(((Character)player).getStandingPosition(), 0, false, false);
				state.NoticeHoldPoseApplied = true;
			}
		}

		public bool TryShowPendingBubble(SpouseProximityState state, NPC npc, Farmer player, bool force, bool alreadyEmoted, float noticeDistance, bool interactionBlocked, Func<NPC, float> distanceToPlayer)
		{
			if (npc == null || player == null || ((Character)npc).currentLocation != ((Character)player).currentLocation || interactionBlocked)
			{
				return false;
			}
			if (distanceToPlayer(npc) > noticeDistance)
			{
				return false;
			}
			if (!SpouseOutfitReactionController.CanShowPendingBubble(force, alreadyEmoted, state.PendingBubbleTimer))
			{
				return false;
			}
			((Character)npc).doEmote(40, true);
			state.PendingBubbleTimer = SpouseOutfitReactionController.GetPendingBubbleCooldown(force);
			return true;
		}
	}
	internal sealed class SpouseDialogueSnapshot
	{
		public List<Dialogue> DialogueQueue { get; set; }

		public string NpcName { get; set; } = "";

		public bool FriendshipStateCaptured { get; set; }

		public bool OriginalTalkedToToday { get; set; }

		public bool ForcedTalkedToToday { get; set; }

		public void Clear()
		{
			DialogueQueue = null;
			NpcName = "";
			FriendshipStateCaptured = false;
			OriginalTalkedToToday = false;
			ForcedTalkedToToday = false;
		}
	}
	internal sealed class SpouseDialogueController
	{
		public SpouseDialogueSnapshot Snapshot { get; } = new SpouseDialogueSnapshot();

		public bool HasBackup => !string.IsNullOrWhiteSpace(Snapshot.NpcName);

		public void Capture(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
		{
			Clear();
			if (npc != null)
			{
				Snapshot.DialogueQueue = npc.CurrentDialogue?.ToList() ?? new List<Dialogue>();
				Snapshot.NpcName = ((Character)npc).Name;
				TemporarilySkipFirstDailyDialogue(npc, player, monitor, debugLog);
			}
		}

		public void TemporarilySkipFirstDailyDialogue(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
		{
			if (npc == null || player == null)
			{
				return;
			}
			try
			{
				Friendship val = default(Friendship);
				if (!((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)player.friendshipData).TryGetValue(((Character)npc).Name, ref val) || val == null)
				{
					return;
				}
				Snapshot.FriendshipStateCaptured = true;
				Snapshot.OriginalTalkedToToday = val.TalkedToToday;
				if (!val.TalkedToToday)
				{
					val.TalkedToToday = true;
					Snapshot.ForcedTalkedToToday = true;
					if (debugLog)
					{
						monitor.Log("[CLOTHES SPOUSE] Temporarily skipped first daily dialogue for " + ((Character)npc).Name + " so the outfit compliment can play first.", (LogLevel)2);
					}
				}
			}
			catch (Exception ex)
			{
				if (debugLog)
				{
					monitor.Log("[CLOTHES SPOUSE] Could not temporarily skip first daily dialogue for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
				}
			}
		}

		public void Restore(NPC npc, Farmer player, bool restoreTalkState, bool clearCurrentDialogue, IMonitor monitor, bool debugLog)
		{
			if (npc == null || !HasBackup || !((Character)npc).Name.Equals(Snapshot.NpcName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (clearCurrentDialogue)
			{
				npc.CurrentDialogue.Clear();
			}
			if (Snapshot.DialogueQueue != null && Snapshot.DialogueQueue.Count > 0)
			{
				for (int num = Snapshot.DialogueQueue.Count - 1; num >= 0; num--)
				{
					npc.CurrentDialogue.Push(Snapshot.DialogueQueue[num]);
				}
			}
			if (restoreTalkState)
			{
				RestoreTalkedToToday(npc, player, monitor, debugLog);
			}
			Clear();
		}

		public void Clear()
		{
			Snapshot.Clear();
		}

		public bool TryQueueOwnAiDialogue(NPC npc, Func<NPC, bool> tryShowOwnAiDialogue, IMonitor monitor)
		{
			if (npc == null)
			{
				return false;
			}
			if (tryShowOwnAiDialogue(npc))
			{
				return true;
			}
			monitor.Log(" No AI outfit dialogue was queued for " + ((Character)npc).Name + ". Manual JSON outfit dialogue is disabled in this AI-only build. Keeping this outfit notice pending until the player cancels by moving away.", (LogLevel)3);
			return false;
		}

		public void RestoreNormalDialogueAfterAiFailure(NPC npc, Action<NPC> clearOutfitPrompt, Action<NPC> restoreDialogue, IMonitor monitor, bool debugLog)
		{
			if (npc == null)
			{
				return;
			}
			try
			{
				clearOutfitPrompt(npc);
				restoreDialogue(npc);
			}
			catch (Exception ex)
			{
				if (debugLog)
				{
					monitor.Log("[CLOTHES SPOUSE] Could not restore normal dialogue after failed outfit AI for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
				}
			}
		}

		private void RestoreTalkedToToday(NPC npc, Farmer player, IMonitor monitor, bool debugLog)
		{
			if (npc == null || player == null || !Snapshot.FriendshipStateCaptured || !Snapshot.ForcedTalkedToToday)
			{
				return;
			}
			try
			{
				Friendship val = default(Friendship);
				if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)player.friendshipData).TryGetValue(((Character)npc).Name, ref val) && val != null)
				{
					val.TalkedToToday = Snapshot.OriginalTalkedToToday;
				}
			}
			catch (Exception ex)
			{
				if (debugLog)
				{
					monitor.Log("[CLOTHES SPOUSE] Could not restore first daily dialogue state for " + ((Character)npc).Name + ": " + ex.Message, (LogLevel)2);
				}
			}
		}
	}
	internal sealed class SpouseProximityState
	{
		public const int PostOutfitLingerDelayTicks = 360;

		public const float PostOutfitLingerDistance = 600f;

		public const float NoticePauseDistance = 96f;

		public const float NoticeReleaseDistance = 300f;

		public bool LingerActive { get; set; }

		public NPC LingerNpc { get; set; }

		public int LingerTimer { get; set; }

		public bool LingerPoseApplied { get; set; }

		public bool NoticePauseActive { get; set; }

		public bool NoticeHoldPoseApplied { get; set; }

		public int PendingBubbleTimer { get; set; }

		public void ClearLinger()
		{
			LingerActive = false;
			LingerNpc = null;
			LingerTimer = 0;
			LingerPoseApplied = false;
		}

		public void ClearNotice()
		{
			NoticePauseActive = false;
			NoticeHoldPoseApplied = false;
			PendingBubbleTimer = 0;
		}
	}
	internal static class SpousePostOutfitLingerController
	{
		public static void Begin(SpouseProximityState state, NPC npc)
		{
			state.LingerActive = true;
			state.LingerNpc = npc;
			state.LingerTimer = 360;
			state.LingerPoseApplied = false;
		}

		public static bool TickAndShouldResume(SpouseProximityState state, bool sameLocation, float distance, bool hasCapturedSpecialAction, float specialActionRestoreDistance)
		{
			if (state.LingerTimer > 0)
			{
				state.LingerTimer--;
			}
			return (!hasCapturedSpecialAction) ? (!sameLocation || distance >= 600f || state.LingerTimer <= 0) : (!sameLocation || distance >= specialActionRestoreDistance);
		}

		public static void ApplyHoldPose(SpouseProximityState state, NPC npc, Farmer player)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (((Character)npc).movementPause < 6)
			{
				((Character)npc).movementPause = 6;
			}
			if (!state.LingerPoseApplied)
			{
				AnimatedSprite sprite = ((Character)npc).Sprite;
				if (sprite != null)
				{
					sprite.StopAnimation();
				}
				((Character)npc).faceGeneralDirection(((Character)player).getStandingPosition(), 0, false, false);
				state.LingerPoseApplied = true;
			}
		}

		public static void Clear(SpouseProximityState state)
		{
			state.ClearLinger();
		}
	}
	internal static class SpouseOutfitReactionController
	{
		public static bool ResolveNoticePause(bool wasPaused, bool isSameLocation, float distance)
		{
			if (!isSameLocation)
			{
				return false;
			}
			if (distance <= 96f)
			{
				return true;
			}
			if (distance >= 300f)
			{
				return false;
			}
			return wasPaused;
		}

		public static bool CanShowPendingBubble(bool force, bool alreadyEmoted, int bubbleTimer)
		{
			return force || (!alreadyEmoted && bubbleTimer <= 0);
		}

		public static int GetPendingBubbleCooldown(bool force)
		{
			return force ? 180 : 240;
		}
	}
	internal sealed class SpouseOutfitReactionCoordinator
	{
		private readonly SpouseOutfitReactionProgressState progressState;

		private readonly Action<NPC> updateActivePartner;

		private readonly Func<NPC, bool> shouldStartReaction;

		private readonly Action<bool> resetReaction;

		private readonly Action updatePostOutfitLinger;

		private readonly Func<NPC, bool> handleInteraction;

		public SpouseOutfitReactionCoordinator(SpouseOutfitReactionProgressState progressState, Action<NPC> updateActivePartner, Func<NPC, bool> shouldStartReaction, Action<bool> resetReaction, Action updatePostOutfitLinger, Func<NPC, bool> handleInteraction)
		{
			this.progressState = progressState;
			this.updateActivePartner = updateActivePartner;
			this.shouldStartReaction = shouldStartReaction;
			this.resetReaction = resetReaction;
			this.updatePostOutfitLinger = updatePostOutfitLinger;
			this.handleInteraction = handleInteraction;
		}

		public void AdvanceTimers()
		{
			progressState.AdvanceTimers();
		}

		public void Update(NPC activePartner, NPC spouse, bool hasPendingOutfitChange)
		{
			if (activePartner != null)
			{
				updateActivePartner(activePartner);
			}
			else if (hasPendingOutfitChange && !shouldStartReaction(spouse))
			{
				resetReaction(obj: true);
			}
			updatePostOutfitLinger();
		}

		public bool TryHandleInteraction(NPC npc)
		{
			return handleInteraction(npc);
		}
	}
	public sealed class OutfitThemeRule
	{
		public string DialogueKey { get; set; } = "";

		public int Priority { get; set; } = 100;

		public List<string> Keywords { get; set; } = new List<string>();

		public string ThemeName { get; set; } = "";

		public string PromptHint { get; set; } = "";

		public string InsideDialogueKey { get; set; } = "";

		public string OutsideDialogueKey { get; set; } = "";

		public string FarmHouseDialogueKey { get; set; } = "";

		public string NpcRoomDialogueKey { get; set; } = "";

		public string BeachDialogueKey { get; set; } = "";

		public string IndoorsDialogueKey { get; set; } = "";

		public string IndoorDialogueKey { get; set; } = "";

		public bool UseOutsideVariant { get; set; } = true;
	}
	internal static class StringUtils
	{
		public static string FirstNonEmpty(params string[] values)
		{
			if (values == null)
			{
				return "";
			}
			foreach (string text in values)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					return text.Trim();
				}
			}
			return "";
		}
	}
}
namespace OutfitReactions.Ai
{
	internal sealed class ActiveAiSettings
	{
		public string Provider { get; set; } = "DeepSeek";

		public string Model { get; set; } = "deepseek-v4-flash";

		public string ApiKey { get; set; } = "";

		public string Endpoint { get; set; } = "";

		public int TemperaturePercent { get; set; } = 75;

		public int TimeoutSeconds { get; set; } = 60;

		public int MaxCharacters { get; set; } = 280;
	}
	internal sealed class AiGenerationCoordinator
	{
		private readonly Dictionary<string, PendingAiGeneration> outfitGenerations = new Dictionary<string, PendingAiGeneration>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, PendingAiPlayerReplyGeneration> replyGenerations = new Dictionary<string, PendingAiPlayerReplyGeneration>(StringComparer.OrdinalIgnoreCase);

		public bool HasOutfitGenerations => outfitGenerations.Count > 0;

		public bool HasReplyGenerations => replyGenerations.Count > 0;

		public bool TryGetOutfit(string npcName, out PendingAiGeneration pending)
		{
			return outfitGenerations.TryGetValue(npcName, out pending);
		}

		public bool TryGetReply(string npcName, out PendingAiPlayerReplyGeneration pending)
		{
			return replyGenerations.TryGetValue(npcName, out pending);
		}

		public IReadOnlyList<string> GetOutfitNpcNames()
		{
			return outfitGenerations.Keys.ToList();
		}

		public IReadOnlyList<string> GetReplyNpcNames()
		{
			return replyGenerations.Keys.ToList();
		}

		public IReadOnlyList<PendingAiGeneration> GetOutfitSnapshot()
		{
			return outfitGenerations.Values.ToList();
		}

		public IReadOnlyList<PendingAiPlayerReplyGeneration> GetReplySnapshot()
		{
			return replyGenerations.Values.ToList();
		}

		public void StartOutfit(PendingAiGeneration pending, Func<CancellationToken, string> generate)
		{
			Start(pending, generate);
			outfitGenerations[pending.NpcName] = pending;
		}

		public void StartReply(PendingAiPlayerReplyGeneration pending, Func<CancellationToken, string> generate)
		{
			Start(pending, generate);
			replyGenerations[pending.NpcName] = pending;
		}

		public void RemoveOutfit(string npcName)
		{
			outfitGenerations.Remove(npcName);
		}

		public void RemoveReply(string npcName)
		{
			replyGenerations.Remove(npcName);
		}

		public IReadOnlyList<PendingAiPlayerReplyGeneration> CancelAll()
		{
			List<PendingAiPlayerReplyGeneration> list = replyGenerations.Values.ToList();
			foreach (PendingAiGeneration value in outfitGenerations.Values)
			{
				AiRequestLifecycle.Cancel(value?.Cancellation);
			}
			foreach (PendingAiPlayerReplyGeneration item in list)
			{
				AiRequestLifecycle.Cancel(item?.Cancellation);
			}
			outfitGenerations.Clear();
			replyGenerations.Clear();
			return list;
		}

		private static void Start(PendingAiGeneration pending, Func<CancellationToken, string> generate)
		{
			pending.Task = Task.Run(() => generate(pending.Cancellation.Token));
			AiRequestLifecycle.DisposeWhenFinished(pending.Task, pending.Cancellation);
		}

		private static void Start(PendingAiPlayerReplyGeneration pending, Func<CancellationToken, string> generate)
		{
			pending.Task = Task.Run(() => generate(pending.Cancellation.Token));
			AiRequestLifecycle.DisposeWhenFinished(pending.Task, pending.Cancellation);
		}
	}
	internal sealed class AiProviderClient
	{
		private readonly IMonitor monitor;

		private static readonly HttpClient Http = new HttpClient();

		public AiProviderClient(IMonitor monitor)
		{
			this.monitor = monitor;
		}

		public async Task<string> GenerateRawAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, OutfitVisionImage visionImage = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			string provider = (ai.Provider ?? "DeepSeek").Trim();
			using (CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			{
				timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(ai.TimeoutSeconds, 3, 120)));
				try
				{
					if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
					{
						return await GenerateGeminiAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);
					}
					if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
					{
						return await GenerateAnthropicAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);
					}
					return await GenerateOpenAiCompatibleAsync(ai, prompt, minLengthTarget, timeout.Token, visionImage);
				}
				catch (InvalidOperationException ex) when (ShouldRetryWithoutVision(ex, ai, visionImage))
				{
					monitor.Log(" Selected AI endpoint rejected image input. Retrying once without the attached image and using only text/confirmed visual clues.", (LogLevel)3);
					string textOnlyPrompt = prompt + "\n\nIMPORTANT VISION FALLBACK: The selected model/endpoint rejected image input, so no image is attached in this retry. Use only textual support data, saved outfit/theme clues, and confirmed color clues. Do not mention seeing an image, screenshot, PNG, pixels, or attachment. Do not invent exact visual details, scene objects, props, or current actions that are not explicitly stated.";
					if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
					{
						return await GenerateGeminiAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token);
					}
					if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
					{
						return await GenerateAnthropicAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token);
					}
					return await GenerateOpenAiCompatibleAsync(ai, textOnlyPrompt, minLengthTarget, timeout.Token);
				}
			}
			IL_057c:
			throw null;
		}

		private static string NormalizeEndpoint(ActiveAiSettings ai)
		{
			string text = ai.Provider ?? "DeepSeek";
			string text2 = (ai.Endpoint ?? "").Trim().TrimEnd('/');
			if (text.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(text2))
				{
					return "https://api.deepseek.com/chat/completions";
				}
				if (text2.EndsWith("/v1/chat/completions", StringComparison.OrdinalIgnoreCase) && text2.StartsWith("https://api.deepseek.com", StringComparison.OrdinalIgnoreCase))
				{
					return "https://api.deepseek.com/chat/completions";
				}
				if (text2.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
				{
					return text2;
				}
				if (text2.Equals("https://api.deepseek.com/v1", StringComparison.OrdinalIgnoreCase))
				{
					return "https://api.deepseek.com/chat/completions";
				}
				if (text2.Equals("https://api.deepseek.com", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/chat/completions";
				}
				return text2 + "/chat/completions";
			}
			if (text.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(text2))
				{
					return "https://api.openai.com/v1/responses";
				}
				if (text2.EndsWith("/responses", StringComparison.OrdinalIgnoreCase) || text2.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
				{
					return text2;
				}
				if (text2.Equals("https://api.openai.com", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/v1/responses";
				}
				if (text2.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/responses";
				}
				return text2 + "/responses";
			}
			if (text.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(text2))
				{
					return "https://openrouter.ai/api/v1/chat/completions";
				}
				if (text2.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
				{
					return text2;
				}
				if (text2.Equals("https://openrouter.ai", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/api/v1/chat/completions";
				}
				if (text2.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase) || text2.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/chat/completions";
				}
				return text2 + "/chat/completions";
			}
			if (text.Equals("Local", StringComparison.OrdinalIgnoreCase) || text.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrWhiteSpace(text2))
				{
					return "http://localhost:11434/v1/chat/completions";
				}
				if (text2.EndsWith("/v1/chat/completions", StringComparison.OrdinalIgnoreCase))
				{
					return text2;
				}
				if (text2.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
				{
					string text3 = text2.Substring(0, text2.Length - "/chat/completions".Length).TrimEnd('/');
					if (text3.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
					{
						return text2;
					}
					return text3 + "/v1/chat/completions";
				}
				if (text2.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
				{
					return text2 + "/chat/completions";
				}
				return text2 + "/v1/chat/completions";
			}
			if (string.IsNullOrWhiteSpace(text2))
			{
				return "https://api.openai.com/v1/responses";
			}
			if (text2.EndsWith("/responses", StringComparison.OrdinalIgnoreCase) || text2.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
			{
				return text2;
			}
			return text2 + "/chat/completions";
		}

		private async Task<string> GenerateOpenAiCompatibleAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, CancellationToken token, OutfitVisionImage visionImage = null)
		{
			string provider = ai.Provider ?? "DeepSeek";
			string endpoint = NormalizeEndpoint(ai);
			if (ModEntry.DebugLog)
			{
				monitor.Log(" HTTP endpoint: " + endpoint, (LogLevel)2);
			}
			bool useResponsesApi = endpoint.IndexOf("/responses", StringComparison.OrdinalIgnoreCase) >= 0;
			int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
			string modelLower = (ai.Model ?? "").ToLowerInvariant();
			bool looksLikeReasoningModel = modelLower.Contains("reasoner") || modelLower.Contains("reasoning") || modelLower.Contains("-pro") || modelLower.Contains("pro-") || modelLower.EndsWith("pro") || modelLower.Contains("r1") || modelLower.Contains("thinking") || modelLower.Contains("o1") || modelLower.Contains("o3") || modelLower.Contains("o4");
			int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, looksLikeReasoningModel);
			monitor.Log($" AI output budget: {maxTokens} tokens for {ai.Provider}/{ai.Model} (visible target {visibleTarget}, reasoning-like={looksLikeReasoningModel}).", (LogLevel)0);
			string requestJson;
			if (useResponsesApi)
			{
				object input = prompt;
				if (ShouldAttachVision(ai, visionImage))
				{
					List<object> contentParts = new List<object>
					{
						new
						{
							type = "input_text",
							text = prompt
						},
						new
						{
							type = "input_image",
							image_url = visionImage.ToDataUri()
						}
					};
					if (visionImage.HasBackImage)
					{
						contentParts.Add(new
						{
							type = "input_image",
							image_url = visionImage.ToBackDataUri()
						});
					}
					input = new object[1]
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
					input = input,
					temperature = (double)Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
					max_output_tokens = maxTokens
				});
			}
			else
			{
				string systemMessage = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. Return portraits as one key per dialogue box, starting at box 1; the array may have 1, 2, 3, or more keys depending on the number of #$b# boxes. No markdown. No explanation. No narration. No analysis.";
				double temperature = (double)Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0;
				if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
				{
					temperature = Math.Min(temperature, 0.25);
				}
				object userContent = prompt;
				if (ShouldAttachVision(ai, visionImage))
				{
					List<object> userParts = new List<object>
					{
						new
						{
							type = "text",
							text = prompt
						},
						new
						{
							type = "image_url",
							image_url = new
							{
								url = visionImage.ToDataUri()
							}
						}
					};
					if (visionImage.HasBackImage)
					{
						userParts.Add(new
						{
							type = "image_url",
							image_url = new
							{
								url = visionImage.ToBackDataUri()
							}
						});
					}
					userContent = userParts.ToArray();
				}
				object[] messages = new object[2]
				{
					new
					{
						role = "system",
						content = systemMessage
					},
					new
					{
						role = "user",
						content = userContent
					}
				};
				Dictionary<string, object> chatBody = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
				{
					["model"] = ai.Model,
					["messages"] = messages,
					["temperature"] = temperature,
					["max_tokens"] = maxTokens,
					["stream"] = false
				};
				bool isDeepSeek = provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase);
				bool isQwenOfficial = provider.IndexOf("Qwen", StringComparison.OrdinalIgnoreCase) >= 0 || provider.IndexOf("DashScope", StringComparison.OrdinalIgnoreCase) >= 0 || provider.IndexOf("Alibaba", StringComparison.OrdinalIgnoreCase) >= 0;
				bool isGenericCompatible = provider.Equals("Local", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase) || provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) || provider.Equals("Groq", StringComparison.OrdinalIgnoreCase) || provider.Equals("Together", StringComparison.OrdinalIgnoreCase) || provider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase);
				bool isOpenRouter = provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase);
				if (isDeepSeek)
				{
					chatBody["thinking"] = new Dictionary<string, object> { ["type"] = "disabled" };
				}
				if (isQwenOfficial)
				{
					chatBody["enable_thinking"] = false;
				}
				if (isGenericCompatible)
				{
					chatBody["chat_template_kwargs"] = new Dictionary<string, object> { ["enable_thinking"] = false };
				}
				if (isOpenRouter)
				{
					chatBody["reasoning"] = new Dictionary<string, object> { ["enabled"] = false };
				}
				if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
				{
					chatBody["response_format"] = new
					{
						type = "json_object"
					};
				}
				requestJson = JsonSerializer.Serialize(chatBody);
			}
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
			request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			if (!string.IsNullOrWhiteSpace(ai.ApiKey))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ai.ApiKey.Trim());
			}
			if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
			{
				request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://www.nexusmods.com/stardewvalley/mods/");
				request.Headers.TryAddWithoutValidation("X-Title", "Outfit Compliments");
			}
			using HttpResponseMessage response = await Http.SendAsync(request, token);
			string json = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"{provider} HTTP {(int)response.StatusCode}: {TrimForLog(json)}");
			}
			return ExtractOpenAiCompatibleText(json);
		}

		private async Task<string> GenerateAnthropicAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, CancellationToken token, OutfitVisionImage visionImage = null)
		{
			string endpoint = ((!string.IsNullOrWhiteSpace(ai.Endpoint)) ? ai.Endpoint.Trim() : "https://api.anthropic.com/v1/messages");
			if (ModEntry.DebugLog)
			{
				monitor.Log(" HTTP endpoint: " + endpoint, (LogLevel)2);
			}
			int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
			string modelLower = (ai.Model ?? "").ToLowerInvariant();
			bool looksLikeReasoningModel = modelLower.Contains("opus") || modelLower.Contains("thinking") || modelLower.Contains("reasoning");
			int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, looksLikeReasoningModel);
			monitor.Log($" Anthropic output budget: {maxTokens} tokens for {ai.Model} (visible target {visibleTarget}, reasoning-like={looksLikeReasoningModel}).", (LogLevel)0);
			object userContent;
			if (ShouldAttachVision(ai, visionImage))
			{
				List<object> contentParts = new List<object>
				{
					new
					{
						type = "text",
						text = prompt
					}
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
				{
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
				}
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
				temperature = (double)Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
				system = "You are a strict JSON API. Return only one compact JSON object with keys text, portrait, portraits, and needsClarification. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. Return portraits as one key per dialogue box, starting at box 1; the array may have 1, 2, 3, or more keys depending on the number of #$b# boxes. No markdown. No explanation. No narration. No analysis.",
				messages = new[]
				{
					new
					{
						role = "user",
						content = userContent
					}
				}
			};
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
			request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
			request.Headers.TryAddWithoutValidation("x-api-key", ai.ApiKey?.Trim() ?? "");
			request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
			using HttpResponseMessage response = await Http.SendAsync(request, token);
			string json = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Anthropic HTTP {(int)response.StatusCode}: {TrimForLog(json)}");
			}
			using JsonDocument doc = JsonDocument.Parse(json);
			if (doc.RootElement.TryGetProperty("content", out var contentArray))
			{
				StringBuilder combined = new StringBuilder();
				foreach (JsonElement block in contentArray.EnumerateArray())
				{
					if (block.TryGetProperty("type", out var typeEl) && typeEl.GetString().Equals("text", StringComparison.OrdinalIgnoreCase) && block.TryGetProperty("text", out var textEl))
					{
						combined.Append(textEl.GetString());
					}
					typeEl = default(JsonElement);
					textEl = default(JsonElement);
				}
				string result = combined.ToString();
				if (!string.IsNullOrWhiteSpace(result))
				{
					return result;
				}
			}
			monitor.Log(" Anthropic response did not contain content/text blocks. Raw response: " + TrimForLog(json), (LogLevel)3);
			return "";
		}

		private async Task<string> GenerateGeminiAsync(ActiveAiSettings ai, string prompt, int minLengthTarget, CancellationToken token, OutfitVisionImage visionImage = null)
		{
			string endpoint = ((!string.IsNullOrWhiteSpace(ai.Endpoint)) ? ai.Endpoint.Trim() : ("https://generativelanguage.googleapis.com/v1beta/models/" + Uri.EscapeDataString(ai.Model) + ":generateContent"));
			if (!endpoint.Contains("?key=", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(ai.ApiKey))
			{
				endpoint = endpoint + (endpoint.Contains("?") ? "&" : "?") + "key=" + Uri.EscapeDataString(ai.ApiKey.Trim());
			}
			if (ModEntry.DebugLog)
			{
				monitor.Log(" HTTP endpoint: " + endpoint.Split('?')[0], (LogLevel)2);
			}
			int visibleTarget = Math.Max(ai.MaxCharacters, minLengthTarget);
			string geminiModelLower = (ai.Model ?? "").ToLowerInvariant();
			bool geminiReasoning = geminiModelLower.Contains("-pro") || geminiModelLower.Contains("pro-") || geminiModelLower.EndsWith("pro") || geminiModelLower.Contains("thinking") || geminiModelLower.Contains("reasoning");
			int maxTokens = CalculateQualityOutputTokenBudget(visibleTarget, geminiReasoning);
			monitor.Log($" Gemini output budget: {maxTokens} tokens for {ai.Model} (visible target {visibleTarget}, reasoning-like={geminiReasoning}).", (LogLevel)0);
			List<object> parts = new List<object>
			{
				new
				{
					text = prompt
				}
			};
			if (ShouldAttachVision(ai, visionImage))
			{
				parts.Add(new Dictionary<string, object> { ["inline_data"] = new Dictionary<string, object>
				{
					["mime_type"] = visionImage.MimeType,
					["data"] = visionImage.Base64Data
				} });
				if (visionImage.HasBackImage)
				{
					parts.Add(new Dictionary<string, object> { ["inline_data"] = new Dictionary<string, object>
					{
						["mime_type"] = visionImage.MimeType,
						["data"] = visionImage.Base64DataBack
					} });
				}
			}
			object body = new
			{
				safetySettings = new[]
				{
					new
					{
						category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
						threshold = "BLOCK_NONE"
					},
					new
					{
						category = "HARM_CATEGORY_HARASSMENT",
						threshold = "BLOCK_MEDIUM_AND_ABOVE"
					}
				},
				system_instruction = new
				{
					parts = new[]
					{
						new
						{
							text = "You return one compact JSON object only. No markdown. No introduction. No explanation. Shape example: {\"text\":\"...\",\"portrait\":\"neutral fallback only\",\"portraits\":[\"actual portrait for box 1\"],\"needsClarification\":false}. Do not put Stardew portrait $commands inside text; use portrait only as a neutral/default fallback. The portraits array must have one key per dialogue box, matching the natural number of boxes in the text."
						}
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
					temperature = (double)Math.Clamp(ai.TemperaturePercent, 0, 200) / 100.0,
					topP = 0.9,
					maxOutputTokens = maxTokens,
					thinkingConfig = new
					{
						thinkingBudget = 0
					}
				}
			};
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
			request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
			using HttpResponseMessage response = await Http.SendAsync(request, token);
			string json = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Gemini HTTP {(int)response.StatusCode}: {TrimForLog(json)}");
			}
			using JsonDocument doc = JsonDocument.Parse(json);
			if (doc.RootElement.TryGetProperty("candidates", out var candidates))
			{
				foreach (JsonElement candidate in candidates.EnumerateArray())
				{
					if (candidate.TryGetProperty("finishReason", out var finishReason) && !finishReason.GetString().Equals("STOP", StringComparison.OrdinalIgnoreCase))
					{
						monitor.Log(" Gemini finishReason was " + finishReason.GetString() + ". Trying to read usable text anyway.", (LogLevel)0);
					}
					if (candidate.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var responseParts))
					{
						StringBuilder combined = new StringBuilder();
						foreach (JsonElement item in responseParts.EnumerateArray())
						{
							if (item.TryGetProperty("text", out var text))
							{
								combined.Append(text.GetString());
							}
							text = default(JsonElement);
						}
						string result = combined.ToString();
						if (!string.IsNullOrWhiteSpace(result))
						{
							return result;
						}
					}
					finishReason = default(JsonElement);
					content = default(JsonElement);
					responseParts = default(JsonElement);
				}
			}
			monitor.Log(" Gemini response did not contain candidates/content/parts text. Raw response: " + TrimForLog(json), (LogLevel)3);
			return "";
		}

		private static bool ShouldRetryWithoutVision(Exception ex, ActiveAiSettings ai, OutfitVisionImage visionImage)
		{
			if (ex == null || ai == null || visionImage == null || !visionImage.IsUsable)
			{
				return false;
			}
			string text = ex.Message ?? "";
			return text.IndexOf("support image input", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("image input", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("input_image", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("image_url", StringComparison.OrdinalIgnoreCase) >= 0 || (text.IndexOf("vision", StringComparison.OrdinalIgnoreCase) >= 0 && text.IndexOf("not", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static bool ShouldAttachVision(ActiveAiSettings ai, OutfitVisionImage visionImage)
		{
			if (ai == null || visionImage == null || !visionImage.IsUsable)
			{
				return false;
			}
			string text = ai.Provider ?? "";
			return text.Equals("Gemini", StringComparison.OrdinalIgnoreCase) || text.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) || text.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) || text.Equals("Local", StringComparison.OrdinalIgnoreCase) || text.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase) || text.Equals("Anthropic", StringComparison.OrdinalIgnoreCase) || text.Equals("xAI", StringComparison.OrdinalIgnoreCase);
		}

		private static string ExtractOpenAiCompatibleText(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return "";
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			JsonElement rootElement = jsonDocument.RootElement;
			if (rootElement.TryGetProperty("output_text", out var value) && value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}
			if (rootElement.TryGetProperty("choices", out var value2) && value2.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value2.EnumerateArray())
				{
					if (item.TryGetProperty("message", out var value3) && value3.TryGetProperty("content", out var value4))
					{
						if (value4.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value4.GetString()))
						{
							return value4.GetString();
						}
						if (value4.ValueKind == JsonValueKind.Array)
						{
							StringBuilder stringBuilder = new StringBuilder();
							foreach (JsonElement item2 in value4.EnumerateArray())
							{
								if (item2.TryGetProperty("text", out var value5) && value5.ValueKind == JsonValueKind.String)
								{
									stringBuilder.Append(value5.GetString());
								}
							}
							if (stringBuilder.Length > 0)
							{
								return stringBuilder.ToString();
							}
						}
					}
					if (item.TryGetProperty("text", out var value6) && value6.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value6.GetString()))
					{
						return value6.GetString();
					}
				}
			}
			if (rootElement.TryGetProperty("output", out var value7) && value7.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item3 in value7.EnumerateArray())
				{
					if (!item3.TryGetProperty("content", out var value8) || value8.ValueKind != JsonValueKind.Array)
					{
						continue;
					}
					foreach (JsonElement item4 in value8.EnumerateArray())
					{
						if (item4.TryGetProperty("text", out var value9) && value9.ValueKind == JsonValueKind.String)
						{
							return value9.GetString();
						}
					}
				}
			}
			return "";
		}

		private static int CalculateQualityOutputTokenBudget(int visibleTarget, bool reasoningLikeModel)
		{
			visibleTarget = Math.Clamp(visibleTarget, 80, 2000);
			int num = visibleTarget * 3;
			int num2 = (int)((double)visibleTarget * 1.75);
			if (reasoningLikeModel)
			{
				int num3 = visibleTarget * 3;
				return Math.Max(1600, num + num3);
			}
			return Math.Max(1000, num + num2);
		}

		private static string TrimForLog(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			text = Regex.Replace(text, "\\s+", " ").Trim();
			return (text.Length <= 500) ? text : (text.Substring(0, 500) + "...");
		}
	}
	internal static class AiResponseParser
	{
		public static AiComplimentResult ParseLocalDashLineStyleResult(string raw, CharacterAiProfile profile)
		{
			if (string.IsNullOrWhiteSpace(raw))
			{
				return null;
			}
			string input = StripMarkdownFences(raw).Trim();
			input = Regex.Replace(input, "(?is)<think>.*?</think>", "").Trim();
			input = Regex.Replace(input, "(?is)^\\s*(assistant|resposta|response)\\s*:\\s*", "").Trim();
			string text = null;
			bool flag = false;
			string[] array = input.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			foreach (string text2 in array)
			{
				string text3 = text2.Trim();
				if (string.IsNullOrWhiteSpace(text3))
				{
					continue;
				}
				if (text3.StartsWith("%", StringComparison.Ordinal))
				{
					break;
				}
				if (!flag)
				{
					text3 = Regex.Replace(text3, "^\\s*[-–—•]+\\s*", "").Trim();
					if (!string.IsNullOrWhiteSpace(text3))
					{
						text = text3;
						flag = true;
					}
				}
				else
				{
					if (text3.StartsWith("-", StringComparison.Ordinal) || text3.StartsWith("–", StringComparison.Ordinal) || text3.StartsWith("—", StringComparison.Ordinal) || text3.StartsWith("•", StringComparison.Ordinal))
					{
						break;
					}
					text = text + "#$b#" + text3;
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			int num = text.IndexOf('%');
			if (num >= 0)
			{
				text = text.Substring(0, num).Trim();
			}
			text = Regex.Replace(text, "^['\"“”]+|['\"“”]+$", "").Trim();
			bool needsClarification = false;
			if (Regex.IsMatch(text, "^\\s*\\[\\s*(?:CLARIFY|NEEDS[_\\s-]*CLARIFICATION)\\s*\\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
			{
				needsClarification = true;
				text = Regex.Replace(text, "^\\s*\\[\\s*(?:CLARIFY|NEEDS[_\\s-]*CLARIFICATION)\\s*\\]\\s*", "", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Trim();
			}
			text = Regex.Replace(text, "(^|#\\$b#)\\s*[-–—•]+\\s*", "$1").Trim();
			text = Regex.Replace(text, "\\s{2,}", " ").Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			return new AiComplimentResult
			{
				Text = text,
				Portrait = "",
				NeedsClarification = needsClarification
			};
		}

		public static AiComplimentResult ParseAiResult(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw))
			{
				return null;
			}
			foreach (string item in BuildJsonCandidates(raw))
			{
				AiComplimentResult aiComplimentResult = TryDeserializeCompliment(item);
				if (aiComplimentResult != null)
				{
					return aiComplimentResult;
				}
			}
			AiComplimentResult aiComplimentResult2 = TryRecoverLooseCompliment(raw);
			if (aiComplimentResult2 != null)
			{
				return aiComplimentResult2;
			}
			return null;
		}

		private static IEnumerable<string> BuildJsonCandidates(string raw)
		{
			string text = (raw ?? "").Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				yield break;
			}
			string unwrapped = TryUnwrapJsonString(text);
			if (!string.IsNullOrWhiteSpace(unwrapped) && !unwrapped.Equals(text, StringComparison.Ordinal))
			{
				text = unwrapped.Trim();
			}
			text = StripMarkdownFences(text).Trim();
			text = NormalizeSmartJsonQuotes(text).Trim();
			yield return text;
			string balanced = ExtractFirstBalancedJsonObject(text);
			if (!string.IsNullOrWhiteSpace(balanced) && !balanced.Equals(text, StringComparison.Ordinal))
			{
				yield return balanced;
			}
			int start = text.IndexOf('{');
			int end = text.LastIndexOf('}');
			if (start >= 0 && end > start)
			{
				string sliced = text.Substring(start, end - start + 1);
				if (!string.IsNullOrWhiteSpace(sliced) && !sliced.Equals(balanced, StringComparison.Ordinal))
				{
					yield return sliced;
				}
			}
		}

		private static AiComplimentResult TryDeserializeCompliment(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true
			};
			try
			{
				AiComplimentResult aiComplimentResult = JsonSerializer.Deserialize<AiComplimentResult>(json.Trim(), options);
				if (aiComplimentResult != null && (!string.IsNullOrWhiteSpace(aiComplimentResult.Text) || !string.IsNullOrWhiteSpace(aiComplimentResult.Portrait)))
				{
					return aiComplimentResult;
				}
			}
			catch
			{
			}
			try
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(json.Trim(), new JsonDocumentOptions
				{
					AllowTrailingCommas = true
				});
				if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
				{
					return null;
				}
				string firstStringProperty = GetFirstStringProperty(jsonDocument.RootElement, "text", "dialogue", "dialogo", "diálogo", "fala", "line", "response", "compliment", "elogio", "texto");
				string firstStringProperty2 = GetFirstStringProperty(jsonDocument.RootElement, "portrait", "portraitKey", "expression", "expressao", "expressão", "emotion", "retrato");
				List<string> firstStringArrayProperty = GetFirstStringArrayProperty(jsonDocument.RootElement, "portraits", "portraitKeys", "expressions", "expressoes", "expressões", "retratos");
				bool firstBoolProperty = GetFirstBoolProperty(jsonDocument.RootElement, "needsClarification", "needs_clarification", "clarificationNeeded", "clarification", "needClarification");
				if (string.IsNullOrWhiteSpace(firstStringProperty) && string.IsNullOrWhiteSpace(firstStringProperty2) && (firstStringArrayProperty == null || firstStringArrayProperty.Count == 0))
				{
					return null;
				}
				return new AiComplimentResult
				{
					Text = firstStringProperty,
					Portrait = firstStringProperty2,
					Portraits = (firstStringArrayProperty ?? new List<string>()),
					NeedsClarification = firstBoolProperty
				};
			}
			catch
			{
				return null;
			}
		}

		private static AiComplimentResult TryRecoverLooseCompliment(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw))
			{
				return null;
			}
			List<string> list = new List<string>();
			string text = StripMarkdownFences(raw.Trim());
			text = NormalizeSmartJsonQuotes(text).Trim();
			if (!string.IsNullOrWhiteSpace(text))
			{
				list.Add(text);
			}
			string text2 = TryUnwrapJsonString(text);
			if (!string.IsNullOrWhiteSpace(text2) && !text2.Equals(text, StringComparison.Ordinal))
			{
				list.Add(NormalizeSmartJsonQuotes(StripMarkdownFences(text2.Trim())).Trim());
			}
			foreach (string item in list)
			{
				string text3 = TryExtractLooseStringProperty(item, "text", "dialogue", "dialogo", "diálogo", "fala", "line", "response", "compliment", "elogio", "texto");
				string text4 = TryExtractLooseStringProperty(item, "portrait", "portraitKey", "expression", "expressao", "expressão", "emotion", "retrato");
				bool needsClarification = TryExtractLooseBoolProperty(item, "needsClarification", "needs_clarification", "clarificationNeeded", "clarification", "needClarification");
				if (!string.IsNullOrWhiteSpace(text3) || !string.IsNullOrWhiteSpace(text4))
				{
					return new AiComplimentResult
					{
						Text = text3,
						Portrait = text4,
						Portraits = new List<string>(),
						NeedsClarification = needsClarification
					};
				}
			}
			return null;
		}

		private static string TryExtractLooseStringProperty(string source, params string[] names)
		{
			if (string.IsNullOrWhiteSpace(source) || names == null)
			{
				return null;
			}
			foreach (string text in names)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				string pattern = "(?is)(?:\"" + Regex.Escape(text) + "\"|" + Regex.Escape(text) + ")\\s*:\\s*\"";
				Match match = Regex.Match(source, pattern);
				if (!match.Success)
				{
					continue;
				}
				int j = match.Index + match.Length;
				StringBuilder stringBuilder = new StringBuilder();
				while (j < source.Length)
				{
					bool flag = false;
					bool flag2 = false;
					for (; j < source.Length; j++)
					{
						char c = source[j];
						if (flag)
						{
							StringBuilder stringBuilder2 = stringBuilder;
							if (1 == 0)
							{
							}
							char value = c switch
							{
								'n' => '\n', 
								'r' => '\r', 
								't' => '\t', 
								'"' => '"', 
								'\\' => '\\', 
								_ => c, 
							};
							if (1 == 0)
							{
							}
							stringBuilder2.Append(value);
							flag = false;
							continue;
						}
						switch (c)
						{
						case '\\':
							flag = true;
							continue;
						case '"':
							break;
						default:
							stringBuilder.Append(c);
							continue;
						}
						flag2 = true;
						j++;
						break;
					}
					if (!flag2)
					{
						break;
					}
					int k;
					for (k = j; k < source.Length && char.IsWhiteSpace(source[k]); k++)
					{
					}
					Match match2 = Regex.Match(source.Substring(k), "^(?:#\\s*\\$\\s*b\\s*#|\\$\\s*b\\s*#)\\s*\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					if (!match2.Success)
					{
						break;
					}
					stringBuilder.Append("#$b#");
					j = k + match2.Length;
				}
				string input = stringBuilder.ToString().Trim();
				input = Regex.Replace(input, "\\s*[,}]+\\s*$", "").Trim();
				input = Regex.Replace(input, "\\s*```\\s*$", "").Trim();
				if (!string.IsNullOrWhiteSpace(input))
				{
					string text2 = (input.Contains("#$b#") ? input.Substring(input.LastIndexOf("#$b#", StringComparison.Ordinal) + 4).Trim() : input);
					if (!text2.EndsWith(".") && !text2.EndsWith("!") && !text2.EndsWith("?") && !text2.EndsWith("...") && !text2.EndsWith("~") && !text2.EndsWith("♪") && !text2.EndsWith("*"))
					{
						return null;
					}
					return input;
				}
			}
			return null;
		}

		private static bool TryExtractLooseBoolProperty(string source, params string[] names)
		{
			if (string.IsNullOrWhiteSpace(source) || names == null)
			{
				return false;
			}
			foreach (string text in names)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				string pattern = "(?is)(?:\"" + Regex.Escape(text) + "\"|" + Regex.Escape(text) + ")\\s*:\\s*(true|false|\"true\"|\"false\"|\"yes\"|\"no\"|\"sim\"|\"não\"|\"nao\")";
				Match match = Regex.Match(source, pattern);
				if (match.Success)
				{
					string text2 = match.Groups[1].Value.Trim().Trim('"');
					if (text2.Equals("true", StringComparison.OrdinalIgnoreCase) || text2.Equals("yes", StringComparison.OrdinalIgnoreCase) || text2.Equals("sim", StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					if (text2.Equals("false", StringComparison.OrdinalIgnoreCase) || text2.Equals("no", StringComparison.OrdinalIgnoreCase) || text2.Equals("não", StringComparison.OrdinalIgnoreCase) || text2.Equals("nao", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
				}
			}
			return false;
		}

		private static string GetFirstStringProperty(JsonElement element, params string[] names)
		{
			foreach (string propertyName in names)
			{
				if (element.TryGetProperty(propertyName, out var value))
				{
					if (value.ValueKind == JsonValueKind.String)
					{
						return value.GetString();
					}
					if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
					{
						return value.ToString();
					}
				}
			}
			return null;
		}

		private static List<string> GetFirstStringArrayProperty(JsonElement element, params string[] names)
		{
			foreach (string propertyName in names)
			{
				if (!element.TryGetProperty(propertyName, out var value))
				{
					continue;
				}
				List<string> list = new List<string>();
				if (value.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement item in value.EnumerateArray())
					{
						if (item.ValueKind == JsonValueKind.String)
						{
							string text = item.GetString();
							if (!string.IsNullOrWhiteSpace(text))
							{
								list.Add(text.Trim());
							}
						}
						else if (item.ValueKind == JsonValueKind.Number || item.ValueKind == JsonValueKind.True || item.ValueKind == JsonValueKind.False)
						{
							string text2 = item.ToString();
							if (!string.IsNullOrWhiteSpace(text2))
							{
								list.Add(text2.Trim());
							}
						}
					}
					return list;
				}
				if (value.ValueKind != JsonValueKind.String)
				{
					continue;
				}
				string text3 = value.GetString();
				if (string.IsNullOrWhiteSpace(text3))
				{
					return list;
				}
				string[] array = text3.Split(new char[4] { ',', ';', '|', '/' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string text4 in array)
				{
					string text5 = text4.Trim().Trim('[', ']', '\'', '"');
					if (!string.IsNullOrWhiteSpace(text5))
					{
						list.Add(text5);
					}
				}
				return list;
			}
			return new List<string>();
		}

		private static bool GetFirstBoolProperty(JsonElement element, params string[] names)
		{
			foreach (string propertyName in names)
			{
				if (!element.TryGetProperty(propertyName, out var value))
				{
					continue;
				}
				if (value.ValueKind == JsonValueKind.True)
				{
					return true;
				}
				if (value.ValueKind == JsonValueKind.False)
				{
					return false;
				}
				if (value.ValueKind == JsonValueKind.String)
				{
					string text = value.GetString();
					if (bool.TryParse(text, out var result))
					{
						return result;
					}
					if (string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "sim", StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static string StripMarkdownFences(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			text = text.Trim();
			text = Regex.Replace(text, "^```(?:json|JSON)?\\s*", "", RegexOptions.IgnoreCase).Trim();
			text = Regex.Replace(text, "\\s*```$", "", RegexOptions.IgnoreCase).Trim();
			return text;
		}

		private static string TryUnwrapJsonString(string text)
		{
			try
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(text);
				if (jsonDocument.RootElement.ValueKind == JsonValueKind.String)
				{
					return jsonDocument.RootElement.GetString();
				}
			}
			catch
			{
			}
			return text;
		}

		private static string NormalizeSmartJsonQuotes(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			return text.Replace('“', '"').Replace('”', '"').Replace('„', '"')
				.Replace('‟', '"');
		}

		private static string ExtractFirstBalancedJsonObject(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (flag2)
				{
					flag2 = false;
				}
				else if (c == '\\' && flag)
				{
					flag2 = true;
				}
				else if (c == '"')
				{
					flag = !flag;
				}
				else
				{
					if (flag)
					{
						continue;
					}
					switch (c)
					{
					case '{':
						if (num == 0)
						{
							num2 = i;
						}
						num++;
						break;
					case '}':
						if (num > 0)
						{
							num--;
							if (num == 0 && num2 >= 0)
							{
								return text.Substring(num2, i - num2 + 1);
							}
						}
						break;
					}
				}
			}
			return null;
		}
	}
	public sealed class CharacterAiProfile
	{
		public string NpcName { get; set; } = "";

		public bool Enabled { get; set; } = true;

		public int ProfileVersion { get; set; } = 1;

		public string ProfileId { get; set; } = "";

		public string ProfileName { get; set; } = "";

		public string ProfileType { get; set; } = "";

		public string PromptLanguage { get; set; } = "";

		public string TargetGame { get; set; } = "";

		public Dictionary<string, string> NarrativeProfile { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, CharacterRelationshipScalingProfile> RelationshipScaling { get; set; } = new Dictionary<string, CharacterRelationshipScalingProfile>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> DialogueModes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, CharacterTraitNarrativeProfile> TraitNarratives { get; set; } = new Dictionary<string, CharacterTraitNarrativeProfile>(StringComparer.OrdinalIgnoreCase);

		public List<string> RomanticOnlyNarrativeKeys { get; set; } = new List<string>();

		public Dictionary<string, PortraitProfile> Portraits { get; set; } = new Dictionary<string, PortraitProfile>();

		public Dictionary<string, PortraitProfile> ExtraPortraits { get; set; } = new Dictionary<string, PortraitProfile>();

		public Dictionary<string, CharacterRelationshipProfile> Family { get; set; } = new Dictionary<string, CharacterRelationshipProfile>();

		[JsonConverter(typeof(RelationshipTextDictionaryConverter))]
		public Dictionary<string, string> Relationships { get; set; } = new Dictionary<string, string>();
	}
	[JsonConverter(typeof(CharacterRelationshipScalingProfileJsonConverter))]
	public sealed class CharacterRelationshipScalingProfile
	{
		public string Tone { get; set; } = "";

		public List<string> AllowedBehavior { get; set; } = new List<string>();

		public List<string> Avoid { get; set; } = new List<string>();
	}
	[JsonConverter(typeof(CharacterTraitNarrativeProfileJsonConverter))]
	public sealed class CharacterTraitNarrativeProfile
	{
		public string Heading { get; set; } = "";

		public string Priority { get; set; } = "";

		public string Context { get; set; } = "";

		public string NarrativePrompt { get; set; } = "";
	}
	[JsonConverter(typeof(PortraitProfileJsonConverter))]
	public sealed class PortraitProfile
	{
		public string Command { get; set; } = "";

		public string Description { get; set; } = "";
	}
	[JsonConverter(typeof(CharacterRelationshipProfileJsonConverter))]
	public sealed class CharacterRelationshipProfile
	{
		public string Heading { get; set; } = "";

		public string Description { get; set; } = "";
	}
	public sealed class OutfitAiContext
	{
		public string NpcName { get; set; } = "";

		public string NpcDisplayName { get; set; } = "";

		public bool IsSpouse { get; set; }

		public string DialogueKey { get; set; } = "";

		public string OutfitName { get; set; } = "";

		public string SafeOutfitHint { get; set; } = "";

		public string ThemeContext { get; set; } = "";

		public string ThemePriorityInstruction { get; set; } = "";

		public string LocationName { get; set; } = "";

		public string DetailedLocationName { get; set; } = "";

		public string LocationType { get; set; } = "";

		public bool IsOutdoors { get; set; }

		public bool IsIndoors { get; set; }

		public bool IsNpcRoom { get; set; }

		public bool IsNpcPersonalLocation { get; set; }

		public bool IsBeachOrIsland { get; set; }

		public bool IsFarmHouse { get; set; }

		public string DayPart { get; set; } = "";

		public string FestivalContext { get; set; } = "";

		public string FarmerBirthdayContext { get; set; } = "";

		public string Season { get; set; } = "";

		public string Weather { get; set; } = "";

		public int Time { get; set; }

		public int DayOfSeason { get; set; }

		public int Year { get; set; }

		public string PlayerName { get; set; } = "";

		public string PlayerGender { get; set; } = "";

		public string TargetLanguage { get; set; } = "";

		public string RelationshipStatus { get; set; } = "";

		public int RelationshipHearts { get; set; }

		public OutfitVisionImage VisionImage { get; set; }

		public bool HasVisionImage => VisionImage != null && VisionImage.IsUsable;

		public string FashionSenseVisualSummary { get; set; } = "";

		public bool HasFashionSenseVisualSummary => !string.IsNullOrWhiteSpace(FashionSenseVisualSummary);

		public string SpecialHatReactionContext { get; set; } = "";

		public bool HasSpecialHatReactionContext => !string.IsNullOrWhiteSpace(SpecialHatReactionContext);

		public string SpecialItemReactionContext { get; set; } = "";

		public bool HasSpecialItemReactionContext => !string.IsNullOrWhiteSpace(SpecialItemReactionContext);

		public bool SpecialItemWasJustRemoved { get; set; }

		public bool SpecialItemOnlyMode { get; set; }

		public bool SpecialItemCombinedMode { get; set; }

		public string SpecialItemMemoryHint { get; set; } = "";

		public bool HasSpecialItemMemoryHint => !string.IsNullOrWhiteSpace(SpecialItemMemoryHint);

		public string VanillaPantsMemoryHint { get; set; } = "";

		public string VanillaHatMemoryHint { get; set; } = "";

		public bool HasVanillaHatMemoryHint => !string.IsNullOrWhiteSpace(VanillaHatMemoryHint);

		public int AvailablePortraitCount { get; set; } = 0;

		public string NoticedChangeType { get; set; } = "";

		public string NoticedChangeName { get; set; } = "";

		public string SafeNoticedChangeHint { get; set; } = "";

		public bool WasCaughtPeeking { get; set; }

		public bool IsAccessoryChange => string.Equals(NoticedChangeType, "Accessory", StringComparison.OrdinalIgnoreCase);

		public bool IsHatChange => string.Equals(NoticedChangeType, "Hat", StringComparison.OrdinalIgnoreCase);

		public bool IsHairChange => string.Equals(NoticedChangeType, "Hair", StringComparison.OrdinalIgnoreCase);

		public bool IsOutfitChange => string.Equals(NoticedChangeType, "Outfit", StringComparison.OrdinalIgnoreCase);

		public string OutfitMemoryContext { get; set; } = null;

		public bool HasOutfitMemory => !string.IsNullOrWhiteSpace(OutfitMemoryContext);

		public string PlayerReply { get; set; } = null;

		public bool NpcWitnessedPreviousAccessory { get; set; } = false;

		public bool VanillaHatHatOnlyMode { get; set; } = false;

		public string VanillaHatFraming { get; set; } = "";

		public bool HasVanillaHatFraming => !string.IsNullOrWhiteSpace(VanillaHatFraming);

		public string ConversationTranscript { get; set; } = null;
	}
	public sealed class AiComplimentResult
	{
		public string Text { get; set; } = "";

		public string Portrait { get; set; } = "";

		public List<string> Portraits { get; set; } = new List<string>();

		public bool NeedsClarification { get; set; } = false;
	}
	internal sealed class CharacterRelationshipScalingProfileJsonConverter : JsonConverter<CharacterRelationshipScalingProfile>
	{
		public override CharacterRelationshipScalingProfile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				return new CharacterRelationshipScalingProfile
				{
					Tone = (reader.GetString() ?? "")
				};
			}
			if (reader.TokenType == JsonTokenType.Null)
			{
				return new CharacterRelationshipScalingProfile();
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("RelationshipScaling entries must be either a string or an object.");
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			JsonElement rootElement = jsonDocument.RootElement;
			CharacterRelationshipScalingProfile characterRelationshipScalingProfile = new CharacterRelationshipScalingProfile
			{
				Tone = (ReadString(rootElement, "Tone") ?? ReadString(rootElement, "tone") ?? ReadString(rootElement, "Description") ?? ReadString(rootElement, "description") ?? "")
			};
			characterRelationshipScalingProfile.AllowedBehavior = ReadStringList(rootElement, "AllowedBehavior") ?? ReadStringList(rootElement, "allowedBehavior") ?? ReadStringList(rootElement, "Allowed") ?? ReadStringList(rootElement, "allowed") ?? new List<string>();
			characterRelationshipScalingProfile.Avoid = ReadStringList(rootElement, "Avoid") ?? ReadStringList(rootElement, "avoid") ?? ReadStringList(rootElement, "ForbiddenBehavior") ?? ReadStringList(rootElement, "forbiddenBehavior") ?? new List<string>();
			return characterRelationshipScalingProfile;
		}

		public override void Write(Utf8JsonWriter writer, CharacterRelationshipScalingProfile value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("Tone", value?.Tone ?? "");
			writer.WritePropertyName("AllowedBehavior");
			JsonSerializer.Serialize(writer, value?.AllowedBehavior ?? new List<string>(), options);
			writer.WritePropertyName("Avoid");
			JsonSerializer.Serialize(writer, value?.Avoid ?? new List<string>(), options);
			writer.WriteEndObject();
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value))
			{
				if (value.ValueKind == JsonValueKind.String)
				{
					return value.GetString();
				}
				if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
				{
					return value.ToString();
				}
			}
			return null;
		}

		private static List<string> ReadStringList(JsonElement root, string propertyName)
		{
			if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var value))
			{
				return null;
			}
			List<string> list = new List<string>();
			if (value.ValueKind == JsonValueKind.String)
			{
				string text = value.GetString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add(text);
				}
				return list;
			}
			if (value.ValueKind != JsonValueKind.Array)
			{
				return null;
			}
			foreach (JsonElement item in value.EnumerateArray())
			{
				string text2 = ((item.ValueKind == JsonValueKind.String) ? item.GetString() : item.ToString());
				if (!string.IsNullOrWhiteSpace(text2))
				{
					list.Add(text2);
				}
			}
			return list;
		}
	}
	internal sealed class CharacterTraitNarrativeProfileJsonConverter : JsonConverter<CharacterTraitNarrativeProfile>
	{
		public override CharacterTraitNarrativeProfile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				return new CharacterTraitNarrativeProfile
				{
					NarrativePrompt = (reader.GetString() ?? "")
				};
			}
			if (reader.TokenType == JsonTokenType.Null)
			{
				return new CharacterTraitNarrativeProfile();
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("TraitNarratives entries must be either a string or an object.");
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			JsonElement rootElement = jsonDocument.RootElement;
			return new CharacterTraitNarrativeProfile
			{
				Heading = (ReadString(rootElement, "Heading") ?? ReadString(rootElement, "heading") ?? ""),
				Priority = (ReadString(rootElement, "Priority") ?? ReadString(rootElement, "priority") ?? ""),
				Context = (ReadString(rootElement, "Context") ?? ReadString(rootElement, "context") ?? ""),
				NarrativePrompt = (ReadString(rootElement, "NarrativePrompt") ?? ReadString(rootElement, "narrativePrompt") ?? ReadString(rootElement, "Description") ?? ReadString(rootElement, "description") ?? "")
			};
		}

		public override void Write(Utf8JsonWriter writer, CharacterTraitNarrativeProfile value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("Heading", value?.Heading ?? "");
			writer.WriteString("Priority", value?.Priority ?? "");
			writer.WriteString("Context", value?.Context ?? "");
			writer.WriteString("NarrativePrompt", value?.NarrativePrompt ?? "");
			writer.WriteEndObject();
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value))
			{
				if (value.ValueKind == JsonValueKind.String)
				{
					return value.GetString();
				}
				if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
				{
					return value.ToString();
				}
			}
			return null;
		}
	}
	internal sealed class PortraitProfileJsonConverter : JsonConverter<PortraitProfile>
	{
		public override PortraitProfile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				string description = reader.GetString() ?? "";
				return new PortraitProfile
				{
					Description = description
				};
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Portrait entries must be either a string or an object.");
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			JsonElement rootElement = jsonDocument.RootElement;
			string command = ReadString(rootElement, "Command") ?? ReadString(rootElement, "command") ?? "";
			string text = ReadString(rootElement, "Description") ?? ReadString(rootElement, "description") ?? ReadString(rootElement, "desc") ?? "";
			if (string.IsNullOrWhiteSpace(text))
			{
				text = ReadString(rootElement, "Heading") ?? ReadString(rootElement, "heading") ?? ReadString(rootElement, "Name") ?? ReadString(rootElement, "name") ?? "";
			}
			return new PortraitProfile
			{
				Command = command,
				Description = text
			};
		}

		public override void Write(Utf8JsonWriter writer, PortraitProfile value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("Command", value?.Command ?? "");
			writer.WriteString("Description", value?.Description ?? "");
			writer.WriteEndObject();
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}
			return null;
		}
	}
	internal sealed class CharacterRelationshipProfileJsonConverter : JsonConverter<CharacterRelationshipProfile>
	{
		public override CharacterRelationshipProfile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				return new CharacterRelationshipProfile
				{
					Description = (reader.GetString() ?? "")
				};
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Family entries must be either a string or an object.");
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			JsonElement rootElement = jsonDocument.RootElement;
			return new CharacterRelationshipProfile
			{
				Heading = (ReadString(rootElement, "Heading") ?? ReadString(rootElement, "heading") ?? ReadString(rootElement, "id") ?? ""),
				Description = (ReadString(rootElement, "Description") ?? ReadString(rootElement, "description") ?? "")
			};
		}

		public override void Write(Utf8JsonWriter writer, CharacterRelationshipProfile value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("Heading", value?.Heading ?? "");
			writer.WriteString("Description", value?.Description ?? "");
			writer.WriteEndObject();
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}
			return null;
		}
	}
	internal sealed class FlexibleTextJsonConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				return "";
			}
			if (reader.TokenType == JsonTokenType.String)
			{
				return reader.GetString() ?? "";
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			return ConvertElementToText(jsonDocument.RootElement);
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value ?? "");
		}

		private static string ConvertElementToText(JsonElement element)
		{
			switch (element.ValueKind)
			{
			case JsonValueKind.String:
				return element.GetString() ?? "";
			case JsonValueKind.Number:
			case JsonValueKind.True:
			case JsonValueKind.False:
				return element.ToString();
			case JsonValueKind.Array:
			{
				List<string> list2 = new List<string>();
				foreach (JsonElement item in element.EnumerateArray())
				{
					string text4 = ConvertElementToText(item);
					if (!string.IsNullOrWhiteSpace(text4))
					{
						list2.Add(text4);
					}
				}
				return string.Join("; ", list2);
			}
			case JsonValueKind.Object:
			{
				string text = ReadString(element, "Heading") ?? ReadString(element, "heading") ?? "";
				string text2 = ReadString(element, "Description") ?? ReadString(element, "description") ?? "";
				if (!string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(text2))
				{
					if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
					{
						return text + ": " + text2;
					}
					return (!string.IsNullOrWhiteSpace(text2)) ? text2 : text;
				}
				List<string> list = new List<string>();
				foreach (JsonProperty item2 in element.EnumerateObject())
				{
					string text3 = ConvertElementToText(item2.Value);
					if (!string.IsNullOrWhiteSpace(text3))
					{
						list.Add(item2.Name + ": " + text3);
					}
				}
				return string.Join(" ", list);
			}
			default:
				return "";
			}
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}
			return null;
		}
	}
	internal sealed class RelationshipTextDictionaryConverter : JsonConverter<Dictionary<string, string>>
	{
		public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (reader.TokenType == JsonTokenType.Null)
			{
				return dictionary;
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Relationships must be an object.");
			}
			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			foreach (JsonProperty item in jsonDocument.RootElement.EnumerateObject())
			{
				string value = ConvertRelationshipValueToText(item.Value);
				if (!string.IsNullOrWhiteSpace(value))
				{
					dictionary[item.Name] = value;
				}
			}
			return dictionary;
		}

		public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			if (value != null)
			{
				foreach (KeyValuePair<string, string> item in value)
				{
					writer.WriteString(item.Key, item.Value ?? "");
				}
			}
			writer.WriteEndObject();
		}

		private static string ConvertRelationshipValueToText(JsonElement value)
		{
			if (value.ValueKind == JsonValueKind.String)
			{
				return value.GetString() ?? "";
			}
			if (value.ValueKind != JsonValueKind.Object)
			{
				return "";
			}
			string text = ReadString(value, "Heading") ?? ReadString(value, "heading") ?? ReadString(value, "id") ?? "";
			string text2 = ReadString(value, "Description") ?? ReadString(value, "description") ?? "";
			if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
			{
				return text + ": " + text2;
			}
			return text2 ?? text ?? "";
		}

		private static string ReadString(JsonElement root, string propertyName)
		{
			if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}
			return null;
		}
	}
	internal static class CharacterPromptBuilder
	{
		private sealed class TraitCandidate
		{
			public string Key { get; }

			public CharacterTraitNarrativeProfile Trait { get; }

			public int Score { get; }

			public int PriorityScore { get; }

			public int Order { get; }

			public TraitCandidate(string key, CharacterTraitNarrativeProfile trait, int score, int priorityScore, int order)
			{
				Key = key;
				Trait = trait;
				Score = score;
				PriorityScore = priorityScore;
				Order = order;
			}
		}

		private const int DialogueModeMaxChars = 1500;

		private static readonly string[] CoreNarrativeOrder = new string[4] { "CoreEssence", "EmotionalCore", "VoiceAndSpeech", "SocialAnxietyAndSelfCorrection" };

		public static string BuildForOutfitCompliment(CharacterAiProfile profile, OutfitAiContext context, bool includePlayerReplyMode = false, PromptStyleService promptStyle = null)
		{
			if (profile == null)
			{
				return "";
			}
			return BuildNarrativeV2Profile(profile, context, includePlayerReplyMode, promptStyle);
		}

		private static string BuildNarrativeV2Profile(CharacterAiProfile profile, OutfitAiContext context, bool includePlayerReplyMode, PromptStyleService promptStyle)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = ((!string.IsNullOrWhiteSpace(profile.NpcName)) ? profile.NpcName : (context?.NpcDisplayName ?? "NPC"));
			stringBuilder.AppendLine("FOCUSED CHARACTER PROFILE FOR THIS OUTFIT VISUAL-REACTION SCENE");
			stringBuilder.AppendLine("You are " + text + " from Stardew Valley. Use this as a focused character sheet, not as dialogue to copy.");
			AppendSection(stringBuilder, "Profile mode", StringUtils.FirstNonEmpty(profile.ProfileName, profile.ProfileId, profile.ProfileType));
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool allowIntimate = AllowIntimateContent(context, includePlayerReplyMode);
			string[] coreNarrativeOrder = CoreNarrativeOrder;
			foreach (string text2 in coreNarrativeOrder)
			{
				if (CanEmitNarrativeKey(profile, text2, allowIntimate, allowGuardrails: true) && AppendNarrativeSectionByKey(stringBuilder, profile, text2))
				{
					hashSet.Add(text2);
				}
			}
			AppendRelationshipSection(stringBuilder, profile, context);
			AppendDialogueModeSection(stringBuilder, profile, context, includePlayerReplyMode, promptStyle);
			AppendRelevantTraits(stringBuilder, profile, context, includePlayerReplyMode);
			AppendSpeechHesitationRestraint(stringBuilder, profile, context, includePlayerReplyMode);
			if (profile.NarrativeProfile != null)
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				foreach (string key in profile.NarrativeProfile.Keys)
				{
					if (string.IsNullOrWhiteSpace(key) || hashSet.Contains(key))
					{
						continue;
					}
					bool flag = IsGuardrailKey(key);
					if (!IsSpeechHesitationKey(key) && CanEmitNarrativeKey(profile, key, allowIntimate, allowGuardrails: true))
					{
						if (flag)
						{
							list2.Add(key);
						}
						else
						{
							list.Add(key);
						}
					}
				}
				foreach (string item in list)
				{
					AppendNarrativeSectionByKey(stringBuilder, profile, item);
				}
				foreach (string item2 in list2)
				{
					AppendNarrativeSectionByKey(stringBuilder, profile, item2);
				}
			}
			AppendNaturalReactionStyle(stringBuilder, context, includePlayerReplyMode, promptStyle);
			stringBuilder.AppendLine("Scene-use rule: keep the full personality available, but only bring forward the parts that naturally fit this short outfit/hair/hat/accessory visual reaction, the relationship, the location, and the farmer's reply if present. The outfit is the topic, but the NPC personality is the strongest authority.");
			return stringBuilder.ToString().Trim();
		}

		private static bool AppendNarrativeSectionByKey(StringBuilder builder, CharacterAiProfile profile, string key)
		{
			if (profile?.NarrativeProfile == null || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			if (!profile.NarrativeProfile.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			AppendSection(builder, HumanizeKey(key), Clean(value), NarrativeMaxChars(key));
			return true;
		}

		private static bool CanEmitNarrativeKey(CharacterAiProfile profile, string key, bool allowIntimate, bool allowGuardrails)
		{
			if (profile?.NarrativeProfile == null || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			if (!profile.NarrativeProfile.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			if (allowGuardrails && IsGuardrailKey(key))
			{
				return true;
			}
			string combined = (Regex.Replace(key, "(?<=[a-z0-9])(?=[A-Z])", " ") + " " + value).ToLowerInvariant();
			bool flag = IsRomanticOnlyNarrativeKey(profile, key) || IsPrivateOrIntimateText(combined);
			return allowIntimate || !flag;
		}

		private static bool IsSpeechHesitationKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			string text = key.Replace("_", "").Replace("-", "").Replace(" ", "")
				.ToLowerInvariant();
			return text.Contains("speechhesitation") || text.Contains("stammer") || text.Contains("gaguej") || text.Contains("hesitationhabit");
		}

		private static void AppendSpeechHesitationRestraint(StringBuilder builder, CharacterAiProfile profile, OutfitAiContext context, bool includePlayerReplyMode)
		{
			if (profile?.NarrativeProfile != null && (profile.NarrativeProfile.Keys.Any(IsSpeechHesitationKey) || profile.NarrativeProfile.Values.Any((string text) => ContainsAny(text, "stammer", "hesitation", "gaguej"))))
			{
				string dialogueModeKey = GetDialogueModeKey(context);
				string value = ((includePlayerReplyMode || (context != null && context.IsHairChange) || (IsRomanticRelationship(context) && ((context != null && context.IsNpcRoom) || (context != null && context.IsNpcPersonalLocation) || (context != null && context.IsIndoors && (context == null || !context.IsOutdoors))))) ? "This character has occasional nervous hesitation, but do not use it by default. Use a brief stumble/filler only if the line is clearly shy, flustered, caught staring, wordless, emotionally exposed, or reacting to a strong/flirty farmer reply. Most visual compliments should still be dry and direct." : ("This character has occasional nervous hesitation, but this scene is a normal " + dialogueModeKey + ". Do not start the line with a filler or stammer. Keep the compliment dry, grounded, and direct unless the generated emotion is genuinely shy/flustered."));
				AppendSection(builder, "Speech hesitation restraint", value, 650);
			}
		}

		private static bool IsGuardrailKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			string text = key.ToLowerInvariant();
			return text.Contains("hardlimit") || text.Contains("mustneverbe") || text.Contains("neverbelost") || text.Contains("donotbreak") || text.Contains("absoluterule");
		}

		private static int NarrativeMaxChars(string key)
		{
			string text = key?.ToLowerInvariant() ?? "";
			if (text.Contains("essence") || text.Contains("voice") || text.Contains("speech"))
			{
				return 850;
			}
			if (IsGuardrailKey(text))
			{
				return 1600;
			}
			return 850;
		}

		private static string HumanizeKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return "";
			}
			string text = Regex.Replace(key.Trim(), "(?<=[a-z0-9])(?=[A-Z])", " ");
			text = Regex.Replace(text.Replace('_', ' '), "\\s+", " ").Trim();
			if (text.Length == 0)
			{
				return key.Trim();
			}
			return char.ToUpperInvariant(text[0]) + text.Substring(1).ToLowerInvariant();
		}

		private static void AppendRelationshipSection(StringBuilder builder, CharacterAiProfile profile, OutfitAiContext context)
		{
			if (profile?.RelationshipScaling != null && profile.RelationshipScaling.Count > 0)
			{
				string relationshipKey = GetRelationshipKey(context);
				if (profile.RelationshipScaling.TryGetValue(relationshipKey, out var value))
				{
					AppendSection(builder, "Relationship tone: " + relationshipKey, FormatRelationshipScaling(value), 1800);
				}
				string key = ((context != null && (context.IsNpcRoom || context.IsNpcPersonalLocation || (context.IsSpouse && context.IsIndoors && !context.IsOutdoors))) ? "PrivateIntimateContext" : "PublicContext");
				if (profile.RelationshipScaling.TryGetValue(key, out var value2))
				{
					AppendSection(builder, "Privacy/context tone", FormatRelationshipScaling(value2), 450);
				}
			}
		}

		private static string GetRelationshipKey(OutfitAiContext context)
		{
			string text = context?.RelationshipStatus ?? "";
			if ((context != null && context.IsSpouse) || text.IndexOf("spouse", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("married", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "Spouse";
			}
			if (text.IndexOf("dating", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("boyfriend", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("girlfriend", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("namor", StringComparison.OrdinalIgnoreCase) >= 0 || (context != null && context.RelationshipHearts >= 8))
			{
				return "Dating";
			}
			return "Friend";
		}

		private static string FormatRelationshipScaling(CharacterRelationshipScalingProfile relationship)
		{
			if (relationship == null)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(relationship.Tone))
			{
				stringBuilder.Append("Tone: ").Append(Clean(relationship.Tone)).Append(' ');
			}
			if (relationship.AllowedBehavior != null && relationship.AllowedBehavior.Count > 0)
			{
				stringBuilder.Append("Allowed: ").Append(string.Join(", ", relationship.AllowedBehavior.Where((string x) => !string.IsNullOrWhiteSpace(x)).Select(Clean))).Append(". ");
			}
			if (relationship.Avoid != null && relationship.Avoid.Count > 0)
			{
				stringBuilder.Append("Avoid: ").Append(string.Join(", ", relationship.Avoid.Where((string x) => !string.IsNullOrWhiteSpace(x)).Select(Clean))).Append('.');
			}
			return stringBuilder.ToString().Trim();
		}

		private static void AppendDialogueModeSection(StringBuilder builder, CharacterAiProfile profile, OutfitAiContext context, bool includePlayerReplyMode, PromptStyleService promptStyle = null)
		{
			string text = ((!includePlayerReplyMode && context != null && context.WasCaughtPeeking) ? "IMPORTANT SCENE SETUP: While walking, the NPC kept stealing glances at the farmer's appearance, and the farmer CAUGHT them staring. Now the farmer has walked over to confront/ask about it. Begin the reaction by acknowledging that they were caught looking (in their own voice — embarrassed, flustered, smug, teasing, defensive, or playing innocent, whatever fits this character), then naturally transition into their genuine reaction to the outfit/hair/hat/accessory. Do not pretend it didn't happen. " : "");
			if (profile?.DialogueModes == null || profile.DialogueModes.Count <= 0)
			{
				AppendSection(builder, "Current dialogue mode", BuildPlainDialogueMode(context, includePlayerReplyMode, promptStyle), 1500);
				return;
			}
			string text2 = (includePlayerReplyMode ? "PlayerReplyReaction" : GetDialogueModeKey(context));
			if (profile.DialogueModes.TryGetValue(text2, out var value))
			{
				AppendSection(builder, "Current dialogue mode: " + text2, text + Clean(value), 1500);
			}
			else
			{
				AppendSection(builder, "Current dialogue mode", BuildPlainDialogueMode(context, includePlayerReplyMode, promptStyle), 1500);
			}
		}

		private static string GetDialogueModeKey(OutfitAiContext context)
		{
			if (context == null)
			{
				return "OutfitCompliment";
			}
			if (context.IsHairChange)
			{
				return "HairCompliment";
			}
			if (context.IsHatChange)
			{
				return "HatCompliment";
			}
			if (context.IsAccessoryChange)
			{
				return "AccessoryCompliment";
			}
			return "OutfitCompliment";
		}

		private static string BuildPlainDialogueMode(OutfitAiContext context, bool includePlayerReplyMode, PromptStyleService promptStyle = null)
		{
			if (includePlayerReplyMode)
			{
				return "React to the farmer's reply naturally, while staying grounded in the previous outfit/hair/hat/accessory reaction. Do not restart the whole compliment from zero.";
			}
			string text = ((context != null && context.WasCaughtPeeking) ? "IMPORTANT SCENE SETUP: While walking, the NPC kept stealing glances at the farmer's appearance, and the farmer CAUGHT them staring. Now the farmer has walked over to confront/ask about it. Begin the reaction by acknowledging that they were caught looking (in their own voice — embarrassed, flustered, smug, teasing, defensive, or playing innocent, whatever fits this character), then naturally transition into their genuine reaction to the outfit/hair/hat/accessory. Do not pretend it didn't happen. " : "");
			if (context != null && context.IsHairChange)
			{
				return text + (promptStyle?.HairChangeMode ?? PromptStyleService.FallbackHairChangeMode);
			}
			if (context != null && context.IsHatChange)
			{
				return text + (promptStyle?.HatChangeMode ?? PromptStyleService.FallbackHatChangeMode);
			}
			if (context != null && context.IsAccessoryChange)
			{
				return text + (promptStyle?.AccessoryChangeMode ?? PromptStyleService.FallbackAccessoryChangeMode);
			}
			return text + (promptStyle?.OutfitChangeMode ?? PromptStyleService.FallbackOutfitChangeMode);
		}

		private static void AppendRelevantTraits(StringBuilder builder, CharacterAiProfile profile, OutfitAiContext context, bool includePlayerReplyMode)
		{
			if (profile?.TraitNarratives == null || profile.TraitNarratives.Count <= 0)
			{
				return;
			}
			bool flag = IsRomanticRelationship(context);
			bool flag2 = context != null && context.RelationshipHearts >= 5 && (context.IsNpcRoom || context.IsNpcPersonalLocation || (context.IsIndoors && !context.IsOutdoors));
			bool flag3 = context != null && flag && (context.IsNpcRoom || context.IsNpcPersonalLocation || (context.IsIndoors && !context.IsOutdoors));
			bool flag4 = AllowIntimateContent(context, includePlayerReplyMode);
			bool flag5 = includePlayerReplyMode || flag3 || flag2;
			string modeKey = (includePlayerReplyMode ? "PlayerReplyReaction" : GetDialogueModeKey(context));
			int count = (flag5 ? 8 : 6);
			List<TraitCandidate> list = new List<TraitCandidate>();
			int num = 0;
			foreach (KeyValuePair<string, CharacterTraitNarrativeProfile> traitNarrative in profile.TraitNarratives)
			{
				num++;
				CharacterTraitNarrativeProfile value = traitNarrative.Value;
				if (value != null)
				{
					string value2 = StringUtils.FirstNonEmpty(value.NarrativePrompt, value.Context, value.Heading);
					if (!string.IsNullOrWhiteSpace(value2) && (!IsPrivateOrIntimateTrait(traitNarrative.Key, value) || flag4))
					{
						int priorityScore = ScorePriority(value.Priority);
						int score = ScoreTrait(traitNarrative.Key, value, modeKey, context, includePlayerReplyMode, flag);
						list.Add(new TraitCandidate(traitNarrative.Key, value, score, priorityScore, num));
					}
				}
			}
			List<TraitCandidate> list2 = (from candidate in list
				orderby candidate.Score descending, candidate.PriorityScore descending, candidate.Order
				select candidate).Take(count).ToList();
			if (list2.Count <= 0)
			{
				return;
			}
			list2 = list2.OrderBy((TraitCandidate candidate) => candidate.Order).ToList();
			List<string> list3 = new List<string>();
			foreach (TraitCandidate item in list2)
			{
				AddTraitLine(list3, item.Key, item.Trait);
			}
			if (list3.Count > 0)
			{
				AppendSection(builder, "Relevant personality anchors", string.Join("\n", list3), flag5 ? 1800 : 1350);
			}
		}

		private static int ScoreTrait(string key, CharacterTraitNarrativeProfile trait, string modeKey, OutfitAiContext context, bool includePlayerReplyMode, bool romanticRelationship)
		{
			int num = ScorePriority(trait?.Priority);
			string text = BuildTraitSearchText(key, trait);
			if (ContainsAny(text, "voice", "tone", "speech", "honesty", "politeness", "warmth", "confidence", "anxious", "awkward", "direct", "observ*", "perceptive"))
			{
				num += 20;
			}
			if (includePlayerReplyMode && ContainsAny(text, "reply", "conversation", "teasing", "humor", "warmth", "sincere", "fluster*", "vulnerability", "honesty"))
			{
				num += 25;
			}
			if (string.Equals(modeKey, "OutfitCompliment", StringComparison.OrdinalIgnoreCase) && ContainsAny(text, "outfit", "clothing", "clothes", "appearance", "self-expression", "vibe", "costume", "cosplay", "animal", "cute", "funny", "strange", "practical", "comfort", "neat", "season", "weather", "humor", "teasing", "direct"))
			{
				num += 28;
			}
			if (string.Equals(modeKey, "HairCompliment", StringComparison.OrdinalIgnoreCase) && ContainsAny(text, "hair", "hairstyle", "appearance", "soft", "frame", "beauty", "photography", "teasing", "sincere", "warmth"))
			{
				num += 30;
			}
			if (string.Equals(modeKey, "HatCompliment", StringComparison.OrdinalIgnoreCase) && ContainsAny(text, "hat", "headwear", "tiara", "headband", "hairband", "bow", "clip", "crown", "practical", "weather", "shade", "warmth", "appearance", "teasing", "surprise"))
			{
				num += 30;
			}
			if (string.Equals(modeKey, "AccessoryCompliment", StringComparison.OrdinalIgnoreCase) && ContainsAny(text, "accessory", "detail", "wings", "umbrella", "backpack", "bow", "clip", "symbol", "movement", "shine", "handmade", "cute", "strange", "surprise", "teasing"))
			{
				num += 30;
			}
			if (context != null && context.IsSpouse && ContainsAny(text, "marriage", "spouse", "domestic", "home", "family", "care", "protect*", "affection"))
			{
				num += 20;
			}
			if (romanticRelationship && ContainsAny(text, "romantic", "affection", "warmth", "vulnerab*", "hidden softness", "sincere"))
			{
				num += 12;
			}
			if ((romanticRelationship || (context != null && context.RelationshipHearts >= 5)) && context != null && (context.IsNpcRoom || context.IsNpcPersonalLocation || (context.IsIndoors && !context.IsOutdoors)) && ContainsAny(text, "shy", "fluster*", "blush", "awkward", "nervous", "vulnerab*", "soft", "affection", "romantic", "private", "crush"))
			{
				num += 35;
			}
			return num;
		}

		private static int ScorePriority(string priority)
		{
			if (string.IsNullOrWhiteSpace(priority))
			{
				return 45;
			}
			string text = priority.Trim().ToLowerInvariant();
			if (text.Contains("high"))
			{
				return 100;
			}
			if (text.Contains("medium") || text.Contains("mid"))
			{
				return 70;
			}
			if (text.Contains("conditional"))
			{
				return 55;
			}
			if (text.Contains("low"))
			{
				return 30;
			}
			return 45;
		}

		private static bool IsPrivateOrIntimateTrait(string key, CharacterTraitNarrativeProfile trait)
		{
			return IsPrivateOrIntimateText(BuildTraitSearchText(key, trait));
		}

		private static bool IsPrivateOrIntimateText(string combined)
		{
			return ContainsAny(combined, "spicy", "kiss*", "beijo*", "physical affection", "intense physical", "touch-oriented", "touch orient", "sensual", "lust", "seduc*");
		}

		private static bool IsRomanticOnlyNarrativeKey(CharacterAiProfile profile, string key)
		{
			if (profile?.RomanticOnlyNarrativeKeys == null || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			foreach (string romanticOnlyNarrativeKey in profile.RomanticOnlyNarrativeKeys)
			{
				if (!string.IsNullOrWhiteSpace(romanticOnlyNarrativeKey) && string.Equals(romanticOnlyNarrativeKey.Trim(), key.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsRomanticRelationship(OutfitAiContext context)
		{
			return context != null && (context.IsSpouse || context.RelationshipHearts >= 8 || ContainsAny(context.RelationshipStatus, "spouse", "married", "dating", "boyfriend", "girlfriend", "namor*"));
		}

		private static bool AllowIntimateContent(OutfitAiContext context, bool includePlayerReplyMode)
		{
			bool flag = context != null && (context.IsNpcRoom || context.IsNpcPersonalLocation || (context.IsIndoors && !context.IsOutdoors));
			return IsRomanticRelationship(context) && (flag || includePlayerReplyMode);
		}

		private static string BuildTraitSearchText(string key, CharacterTraitNarrativeProfile trait)
		{
			string text = Regex.Replace(key ?? "", "(?<=[a-z0-9])(?=[A-Z])", " ");
			return (text + " " + trait?.Heading + " " + trait?.Priority + " " + trait?.Context + " " + trait?.NarrativePrompt).ToLowerInvariant();
		}

		private static bool ContainsAny(string text, params string[] needles)
		{
			if (string.IsNullOrWhiteSpace(text) || needles == null)
			{
				return false;
			}
			foreach (string needle in needles)
			{
				if (IsNeedleMatch(text, needle))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsNeedleMatch(string text, string needle)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(needle))
			{
				return false;
			}
			needle = needle.Trim();
			if (needle.Any(char.IsWhiteSpace))
			{
				return text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
			}
			if (needle.EndsWith("*", StringComparison.Ordinal))
			{
				string text2 = Regex.Escape(needle.TrimEnd('*'));
				return Regex.IsMatch(text, "(?<![A-Za-z0-9_])" + text2 + "[A-Za-z0-9_]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			}
			string text3 = Regex.Escape(needle);
			return Regex.IsMatch(text, "(?<![A-Za-z0-9_])" + text3 + "(?![A-Za-z0-9_])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		private static void AddTraitLine(List<string> lines, string key, CharacterTraitNarrativeProfile trait)
		{
			if (lines == null || trait == null)
			{
				return;
			}
			string text = StringUtils.FirstNonEmpty(trait.NarrativePrompt, trait.Context, trait.Heading);
			if (!string.IsNullOrWhiteSpace(text))
			{
				string text2 = StringUtils.FirstNonEmpty(trait.Heading, key);
				if (IsSpeechHesitationKey(key) || ContainsAny(text2 + " " + text, "stammer", "gaguej", "hesitation"))
				{
					lines.Add("- " + text2 + ": Use nervous hesitation only as a rare emotional marker when the line is genuinely shy, flustered, wordless, or exposed; do not use it in every ordinary visual compliment.");
				}
				else
				{
					lines.Add("- " + text2 + ": " + Clean(text));
				}
			}
		}

		private static void AppendNaturalReactionStyle(StringBuilder builder, OutfitAiContext context, bool includePlayerReplyMode, PromptStyleService promptStyle = null)
		{
			if (builder != null)
			{
				string newValue = ((context != null && context.IsHairChange) ? "hair/hairstyle change" : ((context != null && context.IsHatChange) ? "headwear/head accessory change" : ((context != null && context.IsAccessoryChange) ? "accessory change" : "outfit")));
				string newValue2 = ((context != null && context.IsOutfitChange) ? "For a whole saved outfit, focus on the outfit/theme itself; do not turn the player's hair color or a generic head-slot item into the main topic, and never call the farmer's hair a hat. " : "");
				string text = promptStyle?.NaturalReactionStyle ?? PromptStyleService.FallbackNaturalReactionStyle;
				string value = text.Replace("{Change}", newValue).Replace("{OutfitFocusRule}", newValue2);
				AppendSection(builder, "Natural reaction style", value, 7000);
			}
		}

		private static void AppendSection(StringBuilder builder, string heading, string value, int maxChars = 1000)
		{
			if (builder != null && !string.IsNullOrWhiteSpace(value))
			{
				builder.AppendLine(heading + ":");
				builder.AppendLine(Collapse(Clean(value), maxChars));
			}
		}

		private static string Clean(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string input = text.Replace("**", "").Replace("__", "");
			return Regex.Replace(input, "\\s+", " ").Trim();
		}

		private static string Collapse(string text, int maxChars)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			text = Regex.Replace(text, "\\s+", " ").Trim();
			if (maxChars <= 0 || text.Length <= maxChars)
			{
				return text;
			}
			return text.Substring(0, Math.Max(0, maxChars - 1)).TrimEnd() + "…";
		}

		public static void AppendPromptBlock(StringBuilder builder, string template, OutfitAiContext context, Dictionary<string, string> extraTokens = null)
		{
			if (builder != null && !string.IsNullOrWhiteSpace(template))
			{
				builder.AppendLine(ApplyPromptTokens(template, context, extraTokens));
			}
		}

		public static void AppendPersonalityPriorityRule(StringBuilder builder, OutfitAiContext context)
		{
			if (builder != null)
			{
				builder.AppendLine("CHARACTER PRIORITY RULE: this is a visual reaction, not a mandatory compliment. Choose the reaction by this order: 1) the NPC's canon personality and saved profile rules, 2) relationship status and heart level, 3) current context/location/season/weather/privacy, 4) the farmer's visible outfit/change/theme, 5) wording and portrait choice. Do not flatten grumpy, shy, blunt, awkward, proud, sarcastic, formal, or emotionally guarded NPCs into generically sweet praise.");
				if (context != null)
				{
					builder.AppendLine("Current relationship strength for tone calibration: " + context.RelationshipStatus + ", hearts=" + context.RelationshipHearts + ". Low or mid hearts should not sound as intimate, warm, or openly admiring as high hearts/spouse unless that specific NPC would naturally act that way.");
				}
				builder.AppendLine("A valid reaction may be positive, reluctant, dry, annoyed, skeptical, teasing, confused, practical, indifferent, flustered, or warm. Praise is allowed only when it fits the NPC and heart level; otherwise keep the NPC's edge, restraint, awkwardness, or bluntness intact.");
				builder.AppendLine("OPENING VARIETY RULE: do not reuse the same opening phrase, first words, sentence structure, or reaction angle across outfit reactions. Do not always begin with grunts like 'Hmph', 'Humph', 'Bah', 'Tch', or direct questions like 'what are you wearing?'. Use grumbles only sometimes, and vary them naturally. A grumpy NPC can start with a complaint, warning, skeptical observation, practical remark, dry aside, or reluctant admission instead.");
			}
		}

		public static void AppendPlayerAddressAndGenderRule(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder != null)
			{
				string value = (context?.PlayerName ?? "").Trim();
				string text = NormalizePlayerGenderForPrompt(context?.PlayerGender);
				string value2 = (string.IsNullOrWhiteSpace(context?.TargetLanguage) ? "the target language" : context.TargetLanguage.Trim());
				string value3 = ((text == "female") ? "Do not use masculine agreement or masculine forms of address for the player character." : ((text == "male") ? "Do not use feminine agreement or feminine forms of address for the player character." : "The player character's gender is unknown. Prefer neutral wording and avoid gendered forms of address unless the context explicitly provides them."));
				Dictionary<string, string> extraTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					["PlayerName"] = value,
					["PlayerGender"] = text,
					["TargetLanguage"] = value2,
					["GenderSpecificCaution"] = value3
				};
				AppendPromptBlock(builder, string.IsNullOrWhiteSpace(value) ? (promptStyle?.PlayerUnknownAddressRule ?? PromptStyleService.FallbackPlayerUnknownAddressRule) : (promptStyle?.PlayerKnownAddressRule ?? PromptStyleService.FallbackPlayerKnownAddressRule), context, extraTokens);
				AppendPromptBlock(builder, promptStyle?.PlayerGenderRule ?? PromptStyleService.FallbackPlayerGenderRule, context, extraTokens);
			}
		}

		public static void AppendWornItemDeixisRule(StringBuilder builder, OutfitAiContext context)
		{
			builder?.AppendLine("Spatial reference rule for clothing/accessories/items the farmer is currently wearing: these are physically close to the FARMER, right in front of the NPC, not far away. If the target language marks spatial distance in demonstratives (e.g. Portuguese 'isso'/'aí' for near-the-listener vs 'aquilo'/'ali' for far-from-both), use the near-listener form for anything worn on the farmer's body right now (e.g. 'isso aí na sua cabeça', not 'aquilo ali'). Reserve the far/distant form only for something genuinely far away, not for what the farmer is wearing.");
		}

		private static string ApplyPromptTokens(string template, OutfitAiContext context, Dictionary<string, string> extraTokens)
		{
			if (string.IsNullOrWhiteSpace(template))
			{
				return "";
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["NpcName"] = context?.NpcName ?? "",
				["NpcDisplayName"] = context?.NpcDisplayName ?? context?.NpcName ?? "",
				["PlayerName"] = context?.PlayerName ?? "",
				["PlayerGender"] = NormalizePlayerGenderForPrompt(context?.PlayerGender),
				["TargetLanguage"] = (string.IsNullOrWhiteSpace(context?.TargetLanguage) ? "the target language" : context.TargetLanguage.Trim()),
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
				foreach (KeyValuePair<string, string> extraToken in extraTokens)
				{
					dictionary[extraToken.Key] = extraToken.Value ?? "";
				}
			}
			string text = template;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				text = text.Replace("{" + item.Key + "}", item.Value ?? "", StringComparison.OrdinalIgnoreCase);
			}
			return text;
		}

		private static string NormalizePlayerGenderForPrompt(string rawGender)
		{
			string text = (rawGender ?? "").Trim().ToLowerInvariant();
			if (text == "female" || text == "feminine" || text == "woman")
			{
				return "female";
			}
			if (text == "male" || text == "masculine" || text == "man")
			{
				return "male";
			}
			return "unknown";
		}
	}
	internal static class ColorNamer
	{
		private static readonly (string Name, Color Value)[] Palette = new(string, Color)[36]
		{
			("black", new Color(15, 15, 18)),
			("dark gray", new Color(70, 70, 75)),
			("gray", new Color(130, 130, 135)),
			("light gray", new Color(195, 195, 200)),
			("white", new Color(250, 250, 250)),
			("cream", new Color(245, 235, 200)),
			("dark red", new Color(130, 25, 30)),
			("red", new Color(220, 35, 35)),
			("crimson", new Color(200, 30, 80)),
			("hot pink", new Color(240, 55, 140)),
			("pink", new Color(255, 120, 190)),
			("light pink", new Color(250, 200, 215)),
			("rose", new Color(215, 110, 120)),
			("orange", new Color(240, 130, 30)),
			("peach", new Color(255, 205, 165)),
			("gold", new Color(220, 170, 45)),
			("brown", new Color(115, 70, 40)),
			("light brown", new Color(175, 130, 90)),
			("tan", new Color(215, 185, 145)),
			("auburn", new Color(165, 60, 35)),
			("yellow", new Color(245, 220, 55)),
			("blonde", new Color(225, 200, 120)),
			("dark green", new Color(30, 90, 45)),
			("green", new Color(60, 175, 70)),
			("light green", new Color(165, 220, 145)),
			("mint", new Color(160, 235, 205)),
			("olive", new Color(120, 130, 55)),
			("teal", new Color(35, 165, 170)),
			("navy", new Color(30, 40, 100)),
			("blue", new Color(50, 120, 220)),
			("light blue", new Color(140, 200, 240)),
			("cyan", new Color(70, 220, 235)),
			("purple", new Color(150, 80, 210)),
			("light purple", new Color(195, 155, 230)),
			("lavender", new Color(220, 205, 245)),
			("magenta", new Color(205, 50, 195))
		};

		public static string ClosestSimpleColorName(Color color)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			string result = "color";
			double num = double.MaxValue;
			(string, Color)[] palette = Palette;
			for (int i = 0; i < palette.Length; i++)
			{
				(string, Color) tuple = palette[i];
				string item = tuple.Item1;
				Color item2 = tuple.Item2;
				int num2 = ((Color)(ref color)).R - ((Color)(ref item2)).R;
				int num3 = ((Color)(ref color)).G - ((Color)(ref item2)).G;
				int num4 = ((Color)(ref color)).B - ((Color)(ref item2)).B;
				double num5 = num2 * num2 + num3 * num3 + num4 * num4;
				if (num5 < num)
				{
					num = num5;
					result = item;
				}
			}
			return result;
		}
	}
	internal sealed class HatMemoryService
	{
		private const string SaveKey = "VanillaHatMemories";

		private const int CurrentSchemaVersion = 1;

		private readonly IModHelper helper;

		private readonly IMonitor monitor;

		private Dictionary<string, Dictionary<string, HatMemoryEntry>> memories = new Dictionary<string, Dictionary<string, HatMemoryEntry>>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, string> lastHatPerNpc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public HatMemoryService(IModHelper helper, IMonitor monitor)
		{
			this.helper = helper;
			this.monitor = monitor;
		}

		public string GetLastHatNameForNpc(string npcName)
		{
			if (string.IsNullOrWhiteSpace(npcName))
			{
				return "";
			}
			if (!lastHatPerNpc.TryGetValue(npcName, out var value) || string.IsNullOrWhiteSpace(value))
			{
				return "";
			}
			if (memories.TryGetValue(npcName, out var value2) && value2.TryGetValue(value, out var value3) && value3 != null && !string.IsNullOrWhiteSpace(value3.HatName))
			{
				return value3.HatName;
			}
			return "";
		}

		public void Load()
		{
			try
			{
				HatMemoryData hatMemoryData = helper.Data.ReadSaveData<HatMemoryData>("VanillaHatMemories");
				if (hatMemoryData?.Memories == null)
				{
					memories = new Dictionary<string, Dictionary<string, HatMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
					lastHatPerNpc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				}
				else
				{
					memories = hatMemoryData.Memories;
					lastHatPerNpc = hatMemoryData.LastHatPerNpc ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				}
				if (ModEntry.DebugLog)
				{
					monitor.Log($"[HAT MEMORY] Loaded vanilla-hat memories for {memories.Count} NPC(s).", (LogLevel)2);
				}
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					monitor.Log("[HAT MEMORY] Failed to load hat memories: " + ex.Message, (LogLevel)2);
				}
				memories = new Dictionary<string, Dictionary<string, HatMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
				lastHatPerNpc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
		}

		public void Save()
		{
			try
			{
				helper.Data.WriteSaveData<HatMemoryData>("VanillaHatMemories", new HatMemoryData
				{
					Version = 1,
					Memories = memories,
					LastHatPerNpc = lastHatPerNpc
				});
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					monitor.Log("[HAT MEMORY] Failed to save hat memories: " + ex.Message, (LogLevel)2);
				}
			}
		}

		public HatMemoryComparison GetMemory(string npcName, string currentHatId, string currentHatName)
		{
			if (string.IsNullOrWhiteSpace(npcName))
			{
				return null;
			}
			string value;
			string text = (lastHatPerNpc.TryGetValue(npcName, out value) ? value : "");
			bool flag = string.IsNullOrWhiteSpace(currentHatId);
			if (flag && string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			HatMemoryEntry value2 = null;
			if (!flag && memories.TryGetValue(npcName, out var value3))
			{
				value3.TryGetValue(currentHatId, out value2);
			}
			return new HatMemoryComparison
			{
				CurrentHatId = (currentHatId ?? ""),
				CurrentHatName = (currentHatName ?? ""),
				CurrentlyHatless = flag,
				PreviousHatId = text,
				TimesSeenBefore = (value2?.TimesSeen ?? 0),
				FirstSeenSeason = (value2?.FirstSeenSeason ?? ""),
				FirstSeenDay = (value2?.FirstSeenDay ?? 0),
				FirstSeenYear = (value2?.FirstSeenYear ?? 0),
				LastSeenSeason = (value2?.LastSeenSeason ?? ""),
				LastSeenDay = (value2?.LastSeenDay ?? 0),
				LastSeenYear = (value2?.LastSeenYear ?? 0)
			};
		}

		public void RecordMemory(string npcName, string hatId, string hatName, string season, int day, int year)
		{
			if (string.IsNullOrWhiteSpace(npcName))
			{
				return;
			}
			lastHatPerNpc[npcName] = hatId ?? "";
			if (string.IsNullOrWhiteSpace(hatId))
			{
				return;
			}
			if (!memories.TryGetValue(npcName, out var value))
			{
				value = new Dictionary<string, HatMemoryEntry>(StringComparer.OrdinalIgnoreCase);
				memories[npcName] = value;
			}
			if (!value.TryGetValue(hatId, out var value2) || value2 == null)
			{
				value[hatId] = new HatMemoryEntry
				{
					HatId = hatId,
					HatName = (hatName ?? ""),
					FirstSeenSeason = (season ?? ""),
					FirstSeenDay = day,
					FirstSeenYear = year,
					LastSeenSeason = (season ?? ""),
					LastSeenDay = day,
					LastSeenYear = year,
					TimesSeen = 1
				};
			}
			else
			{
				value2.TimesSeen++;
				value2.LastSeenSeason = season ?? "";
				value2.LastSeenDay = day;
				value2.LastSeenYear = year;
				if (!string.IsNullOrWhiteSpace(hatName))
				{
					value2.HatName = hatName;
				}
			}
		}

		public string BuildMemoryContextHint(HatMemoryComparison memory, string targetLanguage)
		{
			if (memory == null)
			{
				return null;
			}
			bool flag = string.Equals(targetLanguage, "pt", StringComparison.OrdinalIgnoreCase) || string.Equals(targetLanguage, "pt-BR", StringComparison.OrdinalIgnoreCase);
			if (memory.CurrentlyHatless)
			{
				if (string.IsNullOrWhiteSpace(memory.PreviousHatId))
				{
					return null;
				}
				return flag ? "MEMÓRIA DO CHAPÉU: o jogador estava usando um chapéu da última vez que você o viu e agora está sem. Reaja a ele ter tirado o chapéu, como alguém que notou a ausência do que estava lá antes." : "HAT MEMORY: the farmer was wearing a hat last time you saw them and is now bare-headed. React to them having taken the hat off, like someone who noticed it's gone.";
			}
			if (memory.TimesSeenBefore <= 0)
			{
				return null;
			}
			string text = FormatDate(memory.FirstSeenSeason, memory.FirstSeenDay, memory.FirstSeenYear, flag);
			int timesSeenBefore = memory.TimesSeenBefore;
			if (flag)
			{
				string text2 = ((timesSeenBefore == 1) ? "você já viu o jogador com esse chapéu uma vez antes" : $"você já viu o jogador com esse chapéu {timesSeenBefore} vezes antes");
				string text3 = (string.IsNullOrWhiteSpace(text) ? "" : (" (a primeira vez foi em " + text + ")"));
				return "MEMÓRIA DO CHAPÉU: " + text2 + text3 + ". Reconheça que já conhece esse chapéu — pode demonstrar familiaridade, implicar que ele gosta muito desse chapéu, ou comentar conforme a sua personalidade. NÃO reaja como se fosse a primeira vez que vê esse chapéu.";
			}
			string text4 = ((timesSeenBefore == 1) ? "you've seen the farmer in this hat once before" : $"you've seen the farmer in this hat {timesSeenBefore} times before");
			string text5 = (string.IsNullOrWhiteSpace(text) ? "" : (" (first time was in " + text + ")"));
			return "HAT MEMORY: " + text4 + text5 + ". Show that you recognize this hat — you can show familiarity, tease that they really like this hat, or comment in your own voice. Do NOT react as if seeing this hat for the first time.";
		}

		private static string FormatDate(string season, int day, int year, bool isPt)
		{
			if (string.IsNullOrWhiteSpace(season) || day <= 0)
			{
				return "";
			}
			string value = (isPt ? TranslateSeasonPt(season) : season);
			return isPt ? $"dia {day} de {value}, ano {year}" : $"day {day} of {value}, year {year}";
		}

		private static string TranslateSeasonPt(string season)
		{
			return (season ?? "").ToLowerInvariant() switch
			{
				"spring" => "primavera", 
				"summer" => "verão", 
				"fall" => "outono", 
				"winter" => "inverno", 
				_ => season ?? "", 
			};
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
	internal sealed class OutfitAiService
	{
		internal const string NpcCharacteristicsAssetName = "Mods/NatrollEXE.OutfitReactions/NpcCharacteristics";

		internal const string AccessoryClarificationMarker = "{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}";

		private readonly IModHelper helper;

		private readonly IMonitor monitor;

		private readonly Func<ModConfig> getConfig;

		private readonly VoiceSampleService voiceSamples;

		private readonly AiProviderClient aiClient;

		private volatile Dictionary<string, CharacterAiProfile> profiles = new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase);

		private readonly ConcurrentDictionary<string, string> memoryCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private bool warnedMultipleAiProfilesThisSession;

		internal readonly PromptStyleService PromptStyle;

		private bool hasLoggedProfileLoad = false;

		public Func<string, bool> IsRomanceableNpc { get; set; }

		public OutfitAiService(IModHelper helper, IMonitor monitor, Func<ModConfig> getConfig)
		{
			this.helper = helper;
			this.monitor = monitor;
			this.getConfig = getConfig;
			voiceSamples = new VoiceSampleService(helper, monitor);
			aiClient = new AiProviderClient(monitor);
			PromptStyle = new PromptStyleService(helper, monitor);
			PromptStyle.Load(quiet: true);
		}

		public void LoadProfiles(bool quiet = false)
		{
			profiles = new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase);
			PromptStyle.Load(quiet);
			bool flag = !quiet && !hasLoggedProfileLoad;
			Dictionary<string, CharacterAiProfile> dictionary = null;
			try
			{
				dictionary = helper.GameContent.Load<Dictionary<string, CharacterAiProfile>>("Mods/NatrollEXE.OutfitReactions/NpcCharacteristics");
			}
			catch (Exception ex)
			{
				monitor.Log(" Failed to load NPC characteristics from asset Mods/NatrollEXE.OutfitReactions/NpcCharacteristics: " + ex.Message + ". Falling back to files.", (LogLevel)3);
			}
			if (dictionary == null || dictionary.Count <= 0)
			{
				monitor.Log(" NPC characteristics asset is empty or unavailable. Falling back to assets/npc-characteristics files.", (LogLevel)0);
				dictionary = LoadDefaultProfilesFromFiles();
			}
			if (dictionary == null || dictionary.Count <= 0)
			{
				monitor.Log(" No usable NPC characteristics were loaded. Built-in AI generation will be skipped and fallbacks may be used.", (LogLevel)3);
				return;
			}
			Dictionary<string, CharacterAiProfile> dictionary2 = new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, CharacterAiProfile> item in dictionary)
			{
				CharacterAiProfile value = item.Value;
				NormalizeProfile(item.Key, value, mirrorExtraPortraitsToPortraits: true, IsRomanceableNpc);
				if (value != null && !string.IsNullOrWhiteSpace(value.NpcName) && value.Enabled)
				{
					dictionary2[value.NpcName] = value;
					if (flag)
					{
						monitor.Log($" Loaded NPC characteristics for {value.NpcName} with {value.Portraits?.Count ?? 0} portrait descriptions.", (LogLevel)1);
					}
				}
			}
			if (flag)
			{
				monitor.Log($" Loaded {dictionary2.Count} usable NPC characteristic profile(s).", (LogLevel)1);
				hasLoggedProfileLoad = true;
			}
			profiles = dictionary2;
		}

		public Dictionary<string, CharacterAiProfile> LoadDefaultProfilesFromFiles()
		{
			Dictionary<string, CharacterAiProfile> result = new Dictionary<string, CharacterAiProfile>(StringComparer.OrdinalIgnoreCase);
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};
			try
			{
				string folder = Path.Combine(helper.DirectoryPath, "assets", "npc-characteristics");
				LoadProfilesFromFolder(result, folder, options, "Outfit Compliments");
			}
			catch (Exception ex)
			{
				monitor.Log(" Failed to load built-in NPC characteristic files: " + ex.Message, (LogLevel)3);
			}
			try
			{
				foreach (IContentPack item in helper.ContentPacks.GetOwned())
				{
					string folder2 = Path.Combine(item.DirectoryPath, "assets", "npc-characteristics");
					LoadProfilesFromFolder(result, folder2, options, item.Manifest.Name);
				}
			}
			catch (Exception ex2)
			{
				monitor.Log(" Failed to load NPC characteristics from Outfit Compliments content packs: " + ex2.Message, (LogLevel)3);
			}
			return result;
		}

		private void LoadProfilesFromFolder(Dictionary<string, CharacterAiProfile> result, string folder, JsonSerializerOptions options, string sourceName)
		{
			if (!Directory.Exists(folder))
			{
				monitor.Log(" NPC characteristics folder was not found for " + sourceName + ": " + folder, (LogLevel)0);
				return;
			}
			string[] files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
			foreach (string path in files)
			{
				try
				{
					CharacterAiProfile characterAiProfile = JsonSerializer.Deserialize<CharacterAiProfile>(File.ReadAllText(path), options);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					NormalizeProfile(fileNameWithoutExtension, characterAiProfile, mirrorExtraPortraitsToPortraits: false);
					if (characterAiProfile != null && !string.IsNullOrWhiteSpace(characterAiProfile.NpcName))
					{
						result[characterAiProfile.NpcName] = characterAiProfile;
					}
				}
				catch (Exception ex)
				{
					monitor.Log($" Skipped invalid NPC characteristics file '{Path.GetFileName(path)}' from {sourceName}: {ex.Message}", (LogLevel)3);
				}
			}
		}

		private static void NormalizeProfile(string fallbackName, CharacterAiProfile profile, bool mirrorExtraPortraitsToPortraits = true, Func<string, bool> isRomanceableLookup = null)
		{
			if (profile == null)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(profile.NpcName))
			{
				profile.NpcName = fallbackName ?? "";
			}
			CharacterAiProfile characterAiProfile = profile;
			if (characterAiProfile.NarrativeProfile == null)
			{
				Dictionary<string, string> dictionary = (characterAiProfile.NarrativeProfile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.RelationshipScaling == null)
			{
				Dictionary<string, CharacterRelationshipScalingProfile> dictionary3 = (characterAiProfile.RelationshipScaling = new Dictionary<string, CharacterRelationshipScalingProfile>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.DialogueModes == null)
			{
				Dictionary<string, string> dictionary = (characterAiProfile.DialogueModes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.TraitNarratives == null)
			{
				Dictionary<string, CharacterTraitNarrativeProfile> dictionary6 = (characterAiProfile.TraitNarratives = new Dictionary<string, CharacterTraitNarrativeProfile>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.Portraits == null)
			{
				Dictionary<string, PortraitProfile> dictionary8 = (characterAiProfile.Portraits = new Dictionary<string, PortraitProfile>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.ExtraPortraits == null)
			{
				Dictionary<string, PortraitProfile> dictionary8 = (characterAiProfile.ExtraPortraits = new Dictionary<string, PortraitProfile>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.Family == null)
			{
				Dictionary<string, CharacterRelationshipProfile> dictionary11 = (characterAiProfile.Family = new Dictionary<string, CharacterRelationshipProfile>(StringComparer.OrdinalIgnoreCase));
			}
			characterAiProfile = profile;
			if (characterAiProfile.Relationships == null)
			{
				Dictionary<string, string> dictionary = (characterAiProfile.Relationships = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			}
			NormalizePortraitDictionary(profile.Portraits);
			NormalizePortraitDictionary(profile.ExtraPortraits);
			if (!mirrorExtraPortraitsToPortraits)
			{
				return;
			}
			foreach (KeyValuePair<string, PortraitProfile> extraPortrait in profile.ExtraPortraits)
			{
				if (!profile.Portraits.ContainsKey(extraPortrait.Key))
				{
					profile.Portraits[extraPortrait.Key] = extraPortrait.Value;
				}
			}
			bool isRomanceable = isRomanceableLookup?.Invoke(profile.NpcName) ?? false;
			AddCommonVanillaPortraits(profile.Portraits, isRomanceable);
		}

		private static void AddCommonVanillaPortraits(Dictionary<string, PortraitProfile> portraits, bool isRomanceable)
		{
			if (portraits != null)
			{
				AddCommonPortrait(portraits, "h", "$h", "happy or warmly pleased expression; smiling, amused, or genuinely positive");
				AddCommonPortrait(portraits, "s", "$s", "sad, worried, hurt, disappointed, or emotionally softened expression");
				if (isRomanceable)
				{
					AddCommonPortrait(portraits, "a", "$a", "angry, irritated, defensive, or visibly frustrated expression");
					AddCommonPortrait(portraits, "l", "$l", "blushing, shy, affectionate, or touched expression");
				}
			}
		}

		private static void AddCommonPortrait(Dictionary<string, PortraitProfile> portraits, string key, string command, string description)
		{
			if (!portraits.ContainsKey(key))
			{
				portraits[key] = new PortraitProfile
				{
					Command = command,
					Description = description
				};
			}
		}

		private static void NormalizePortraitDictionary(Dictionary<string, PortraitProfile> portraits)
		{
			if (portraits == null)
			{
				return;
			}
			foreach (string item in portraits.Keys.ToList())
			{
				PortraitProfile portraitProfile = portraits[item] ?? new PortraitProfile();
				if (string.IsNullOrWhiteSpace(portraitProfile.Command))
				{
					portraitProfile.Command = "$" + item;
				}
				if (string.IsNullOrWhiteSpace(portraitProfile.Description))
				{
					portraitProfile.Description = item;
				}
				portraits[item] = portraitProfile;
			}
		}

		public bool HasProfile(string npcName)
		{
			CharacterAiProfile profile;
			return TryResolveProfile(npcName, null, out profile);
		}

		public bool TryResolveProfile(string internalName, string displayName, out CharacterAiProfile profile)
		{
			profile = null;
			Dictionary<string, CharacterAiProfile> dictionary = profiles;
			if (!string.IsNullOrWhiteSpace(internalName) && dictionary.TryGetValue(internalName, out profile) && profile != null && profile.Enabled)
			{
				return true;
			}
			if (!string.IsNullOrWhiteSpace(displayName) && dictionary.TryGetValue(displayName, out profile) && profile != null && profile.Enabled)
			{
				return true;
			}
			if (!string.IsNullOrWhiteSpace(internalName) && voiceSamples.TryReverseAlias(internalName, out var displayName2) && dictionary.TryGetValue(displayName2, out profile) && profile != null && profile.Enabled)
			{
				return true;
			}
			profile = null;
			return false;
		}

		public void QueueConnectionTestFromConfigMenu()
		{
			ModConfig modConfig = getConfig?.Invoke() ?? new ModConfig();
			modConfig.ApplyAiDefaultsAndLimits();
			if (HasInvalidAiProfileSelection(modConfig))
			{
				return;
			}
			ActiveAiSettings ai = GetActiveSettings(modConfig);
			string provider = (string.IsNullOrWhiteSpace(ai.Provider) ? "Unknown" : ai.Provider.Trim());
			string model = (string.IsNullOrWhiteSpace(ai.Model) ? "(empty model)" : ai.Model.Trim());
			monitor.Log($" Testing AI connection from: {provider}/{model}.", (LogLevel)2);
			Task.Run(async delegate
			{
				try
				{
					if (string.IsNullOrWhiteSpace(ai.Model))
					{
						monitor.Log(" AI connection test skipped: model name is empty for " + provider + ".", (LogLevel)2);
					}
					else if (string.IsNullOrWhiteSpace(ai.ApiKey) && !IsProviderLocal(ai))
					{
						monitor.Log(" AI connection test skipped: API key is empty for " + provider + ".", (LogLevel)2);
					}
					else
					{
						ActiveAiSettings testAi = new ActiveAiSettings
						{
							Provider = ai.Provider,
							Model = ai.Model,
							ApiKey = ai.ApiKey,
							Endpoint = ai.Endpoint,
							TemperaturePercent = 0,
							TimeoutSeconds = Math.Clamp(ai.TimeoutSeconds, 3, 120),
							MaxCharacters = 120
						};
						string prompt = (IsProviderLocal(testAi) ? "Connection test. Return exactly one line beginning with '- ' and no explanation: - Connection successful." : "Connection test. Return exactly one compact JSON object only with this exact shape: {\"text\":\"Connection successful.\",\"portrait\":\"\"}");
						if (string.IsNullOrWhiteSpace(await aiClient.GenerateRawAsync(testAi, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), testAi))))
						{
							monitor.Log($" AI connection test reached {provider}/{model}, but the provider returned an empty response.", (LogLevel)2);
						}
						else
						{
							monitor.Log($" AI connection OK: {provider}/{model} returned a response.", (LogLevel)2);
						}
					}
				}
				catch (TaskCanceledException)
				{
					monitor.Log($" AI connection test timed out after {Math.Clamp(ai.TimeoutSeconds, 3, 120)}s for {provider}/{model}.", (LogLevel)2);
				}
				catch (Exception ex2)
				{
					Exception ex3 = ex2;
					monitor.Log($" AI connection test failed for {provider}/{model}: {ex3.Message}", (LogLevel)2);
				}
			});
		}

		public bool TryGenerateCompliment(OutfitAiContext context, out string dialogue, CancellationToken cancellationToken = default(CancellationToken))
		{
			dialogue = null;
			ModConfig modConfig = getConfig?.Invoke();
			if (modConfig == null)
			{
				return false;
			}
			if (HasInvalidAiProfileSelection(modConfig))
			{
				return false;
			}
			ActiveAiSettings activeSettings = GetActiveSettings(modConfig);
			if (context == null || string.IsNullOrWhiteSpace(context.NpcName))
			{
				return false;
			}
			if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out var profile) || profile == null || !profile.Enabled)
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(activeSettings.ApiKey) && !IsProviderLocal(activeSettings))
			{
				monitor.Log(" API key is empty for " + activeSettings.Provider + ". Skipping built-in AI and using fallback.", (LogLevel)0);
				return false;
			}
			string prompt = BuildPrompt(profile, context, modConfig, activeSettings);
			string key = BuildCacheKey(context, modConfig, prompt, activeSettings);
			if (modConfig.UseAiCache && memoryCache.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
			{
				dialogue = value;
				return true;
			}
			try
			{
				if (ModEntry.DebugLog)
				{
					monitor.Log($" Sending outfit compliment request for {context.NpcName} using {activeSettings.Provider}/{activeSettings.Model}.", (LogLevel)2);
				}
				string result = aiClient.GenerateRawAsync(activeSettings, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), activeSettings), context.VisionImage, cancellationToken).GetAwaiter().GetResult();
				if (string.IsNullOrWhiteSpace(result))
				{
					monitor.Log(" Provider returned an empty response. Using fallback.", (LogLevel)3);
					return false;
				}
				if (!TryBuildValidatedDialogue(profile, context, activeSettings, result, out dialogue, out var issue))
				{
					if (!TryBuildLenientDialogue(profile, context, activeSettings, result, out dialogue, out var issue2))
					{
						monitor.Log(" Provider response was not usable (" + issue + "; lenient parse: " + issue2 + "). Using fallback. Raw response: " + TrimForLog(result), (LogLevel)3);
						return false;
					}
					monitor.Log(" Provider response did not pass the strict quality checks (" + issue + "), but retry is disabled. Accepting the first usable AI line instead. Raw response: " + TrimForLog(result), (LogLevel)3);
				}
				if (modConfig.UseAiCache)
				{
					memoryCache[key] = dialogue;
				}
				if (ModEntry.DebugLog)
				{
					monitor.Log($" Generated outfit compliment for {context.NpcName} using {activeSettings.Provider}/{activeSettings.Model}.", (LogLevel)2);
				}
				return true;
			}
			catch (TaskCanceledException)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return false;
				}
				int timeoutSeconds = GetActiveSettings(getConfig?.Invoke()).TimeoutSeconds;
				monitor.Log($" Failed to generate outfit compliment: request timed out/canceled after {timeoutSeconds}s. Try increasing the AI timeout or using a faster model.", (LogLevel)3);
				return false;
			}
			catch (Exception ex2)
			{
				monitor.Log(" Failed to generate outfit compliment: " + ex2.Message, (LogLevel)3);
				return false;
			}
		}

		public bool TryGenerateFollowUp(OutfitAiContext context, string npcCompliment, string playerReply, out string dialogue, CancellationToken cancellationToken = default(CancellationToken))
		{
			dialogue = null;
			ModConfig modConfig = getConfig?.Invoke();
			if (modConfig == null)
			{
				return false;
			}
			if (HasInvalidAiProfileSelection(modConfig))
			{
				return false;
			}
			ActiveAiSettings activeSettings = GetActiveSettings(modConfig);
			if (context == null || string.IsNullOrWhiteSpace(context.NpcName) || string.IsNullOrWhiteSpace(playerReply))
			{
				return false;
			}
			if (!TryResolveProfile(context.NpcName, context.NpcDisplayName, out var profile) || profile == null || !profile.Enabled)
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(activeSettings.ApiKey) && !IsProviderLocal(activeSettings))
			{
				monitor.Log(" API key is empty for " + activeSettings.Provider + ". Skipping player-reply follow-up.", (LogLevel)0);
				return false;
			}
			string prompt = BuildPlayerReplyFollowUpPrompt(profile, context, activeSettings, npcCompliment, playerReply);
			try
			{
				monitor.Log($" Sending player-reply follow-up request for {context.NpcName} using {activeSettings.Provider}/{activeSettings.Model}.", (LogLevel)1);
				string result = aiClient.GenerateRawAsync(activeSettings, prompt, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), activeSettings), context.VisionImage, cancellationToken).GetAwaiter().GetResult();
				if (string.IsNullOrWhiteSpace(result))
				{
					monitor.Log(" Provider returned an empty follow-up response.", (LogLevel)3);
					return false;
				}
				context.PlayerReply = playerReply;
				if (!TryBuildValidatedDialogue(profile, context, activeSettings, result, out dialogue, out var issue))
				{
					if (TryBuildLenientDialogue(profile, context, activeSettings, result, out dialogue, out var _))
					{
						monitor.Log(" Follow-up response did not pass strict checks (" + issue + "), accepting lenient result. Raw: " + TrimForLog(result), (LogLevel)3);
					}
					else
					{
						bool flag = false;
						string text = result;
						string text2 = issue;
						for (int i = 1; i <= 3; i++)
						{
							if (flag)
							{
								break;
							}
							monitor.Log($" Follow-up attempt {i}/{3} failed ({text2}). Retrying with corrective prompt. Raw: " + TrimForLog(text), (LogLevel)3);
							string prompt2 = BuildFollowUpRetryPrompt(profile, context, activeSettings, npcCompliment, playerReply, text, text2);
							string result2 = aiClient.GenerateRawAsync(activeSettings, prompt2, GetMinimumLengthTarget(getConfig?.Invoke() ?? new ModConfig(), activeSettings), null, cancellationToken).GetAwaiter().GetResult();
							if (string.IsNullOrWhiteSpace(result2))
							{
								monitor.Log($" Follow-up retry {i} returned empty response.", (LogLevel)3);
								break;
							}
							string issue4;
							if (TryBuildValidatedDialogue(profile, context, activeSettings, result2, out dialogue, out var issue3))
							{
								monitor.Log($" Follow-up retry {i} succeeded for " + context.NpcName + ".", (LogLevel)1);
								flag = true;
							}
							else if (TryBuildLenientDialogue(profile, context, activeSettings, result2, out dialogue, out issue4))
							{
								monitor.Log($" Follow-up retry {i} passed lenient check for " + context.NpcName + ".", (LogLevel)1);
								flag = true;
							}
							else
							{
								text = result2;
								text2 = issue3 ?? text2;
							}
						}
						if (!flag)
						{
							string text3 = TrySalvageFollowUpRaw(text, profile, context, activeSettings);
							if (string.IsNullOrWhiteSpace(text3))
							{
								monitor.Log(" All follow-up retries and salvage failed for " + context.NpcName + ". Discarding.", (LogLevel)3);
								return false;
							}
							dialogue = text3;
							monitor.Log(" All follow-up retries failed. Salvaged a short usable line for " + context.NpcName + ".", (LogLevel)3);
						}
					}
				}
				monitor.Log($" Generated player-reply follow-up for {context.NpcName} using {activeSettings.Provider}/{activeSettings.Model}.", (LogLevel)1);
				return true;
			}
			catch (TaskCanceledException)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return false;
				}
				int timeoutSeconds = GetActiveSettings(getConfig?.Invoke()).TimeoutSeconds;
				monitor.Log($" Failed to generate player-reply follow-up: request timed out/canceled after {timeoutSeconds}s.", (LogLevel)3);
				return false;
			}
			catch (Exception ex2)
			{
				monitor.Log(" Failed to generate player-reply follow-up: " + ex2.Message, (LogLevel)3);
				return false;
			}
		}

		private static bool IsProviderLocal(ActiveAiSettings ai)
		{
			string text = ai?.Provider ?? "";
			string text2 = ai?.Endpoint ?? "";
			return text.Equals("Local", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase);
		}

		private bool HasInvalidAiProfileSelection(ModConfig config)
		{
			if (config == null)
			{
				return true;
			}
			if (!config.HasMultipleEnabledAiProfiles())
			{
				warnedMultipleAiProfilesThisSession = false;
				return false;
			}
			if (!warnedMultipleAiProfilesThisSession)
			{
				monitor.Log("[Outfit Compliments] Mais de um perfil de IA está ativado. Verifique suas configurações, e marque apenas um perfil por vez.", (LogLevel)3);
				warnedMultipleAiProfilesThisSession = true;
			}
			return true;
		}

		private static ActiveAiSettings GetActiveSettings(ModConfig config)
		{
			if (config == null)
			{
				config = new ModConfig();
			}
			config.ApplyAiDefaultsAndLimits();
			string activeProvider = config.GetActiveProvider();
			if (activeProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Local", StringComparison.OrdinalIgnoreCase) || activeProvider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Together", StringComparison.OrdinalIgnoreCase) || activeProvider.Equals("TogetherAI", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("xAI", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Cerebras", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("Perplexity", StringComparison.OrdinalIgnoreCase))
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
			if (activeProvider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
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

		private string TrySalvageFollowUpRaw(string raw, CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai)
		{
			if (string.IsNullOrWhiteSpace(raw))
			{
				return null;
			}
			try
			{
				AiComplimentResult aiComplimentResult = AiResponseParser.ParseAiResult(raw);
				if (aiComplimentResult != null && !string.IsNullOrWhiteSpace(aiComplimentResult.Text))
				{
					string text = DialogueValidator.CleanDialogueText(aiComplimentResult.Text, ai.MaxCharacters);
					string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(text, profile);
					text = DialogueValidator.RestoreEllipsesAndNormalise(text);
					ModConfig config = getConfig?.Invoke() ?? new ModConfig();
					text = PortraitResolver.SanitizeInlinePortraitCommands(text, profile, IsProviderLocal(ai), config);
					text = SanitizeContextInappropriateProfanity(text, context);
					if (!string.IsNullOrWhiteSpace(text) && !DialogueValidator.LooksLikeInstructionLeak(text) && !DialogueValidator.LooksLikeCopiedFormatExample(text))
					{
						if (!text.Contains("#$b#"))
						{
							string text2 = context?.TargetLanguage ?? "en";
							string text3 = (text2.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "#$b#..." : "#$b#...");
							text = text.TrimEnd('.', '!', '?') + "..." + text3;
						}
						return PortraitResolver.ApplyPortraitsFromFields(profile, text, aiComplimentResult, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private string BuildFollowUpRetryPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string npcCompliment, string playerReply, string badResponse, string issue)
		{
			bool flag = IsProviderLocal(ai);
			StringBuilder stringBuilder = new StringBuilder();
			if (flag)
			{
				stringBuilder.AppendLine("Your previous follow-up answer was rejected: " + issue + ".");
			}
			stringBuilder.AppendLine("Return exactly one compact JSON object only. No markdown, no explanation, no narration.");
			stringBuilder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
			stringBuilder.AppendLine("Do NOT put Stardew portrait commands like inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");
			stringBuilder.AppendLine("The dialogue entry must be a direct spoken response from " + context.NpcDisplayName + " to the farmer's reply.");
			CharacterPromptBuilder.AppendPersonalityPriorityRule(stringBuilder, context);
			CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(stringBuilder, context, PromptStyle);
			CharacterPromptBuilder.AppendWornItemDeixisRule(stringBuilder, context);
			stringBuilder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
			AppendExpressiveCuesRule(stringBuilder, (getConfig?.Invoke() ?? new ModConfig()).EnableExpressiveAsteriskActions);
			AppendPunctuationRule(stringBuilder);
			stringBuilder.AppendLine("Language: " + context.TargetLanguage + ". Max " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
			stringBuilder.AppendLine("Do not ignore the farmer's reply. Do not continue the original compliment as if the farmer never answered.");
			stringBuilder.AppendLine("The text may use #$b# breaks when pacing benefits, but do not follow a fixed box count. One, two, or several boxes are all valid. Meet the minimum character count when configured without padding or forcing a break pattern.");
			string text = ((profile?.Portraits != null && profile.Portraits.Count > 0) ? string.Join(", ", profile.Portraits.Keys) : "");
			stringBuilder.AppendLine("Portrait keys and descriptions: " + CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 800));
			stringBuilder.AppendLine("Portrait must be one of these exact keys, or empty string if unsure: " + text);
			stringBuilder.AppendLine("Always include portraits as an array with one portrait key per dialogue box, starting from the first box, even if there is only one box. Use portrait only as a neutral/default fallback.");
			stringBuilder.AppendLine("NPC: " + context.NpcDisplayName);
			stringBuilder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
			stringBuilder.AppendLine(BuildRelationshipDepthGuidance(context));
			stringBuilder.AppendLine("Recognizable theme/reference rule: if the outfit name, readable clue, theme clue, full current outfit, noticed accessory, or visible concept points to a known character, franchise, mascot, creature, animal, food, object, or named style, the NPC may naturally mention or allude to that reference only when it fits their personality, knowledge, and relationship with the farmer. Geeky, playful, artistic, observant, blunt, sarcastic, practical, or curious NPCs can react in different ways: specific reference, joke, question, friendly roast, confusion, practical comment, admiration, indifference, or skepticism. Do not force recognition, but do not ignore clear clues like Sanrio, My Melody, Pikachu, Pokémon/Pokemon, lizard, dinosaur, frog, fairy, cat, rabbit, wings/angel/fairy, or similar named themes when this NPC would naturally notice them.");
			stringBuilder.AppendLine("Thematic reaction angles rule: when a theme is recognizable, do not stop at a bland fashion compliment. Depending on the NPC profile, they may joke, tease, ask why the farmer is wearing it, imagine a funny situation where it would belong, or find it strange, ugly, cute, ridiculous, dramatic, suspicious, practical, unnecessary, too flashy, or oddly charming. Any place, activity, or topic this NPC brings up as a comparison must come from their OWN interests, job, and personality — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops, farm chores) that this specific character would not naturally think about.");
			stringBuilder.AppendLine("Combined accessory + outfit rule: if the noticed change is an accessory but the farmer is still wearing a recognizable saved outfit/theme, react to the combination as a whole. The NPC may compare the accessory with the outfit theme, notice that it clashes or creates a funny impossible hybrid, make a joke, ask why that accessory is on that costume, or imagine where that mixed look would belong. Example idea, not text to copy: wings added to a Pikachu/animal/mascot outfit can be treated as funny, cute, cursed, dramatic, or weird because that character/creature normally does not have wings. Do not focus only on the accessory when the full outfit context gives a better reaction.");
			stringBuilder.AppendLine("Occasion mismatch rule: judge whether the item fits the CURRENT occasion/place/moment using the Location, Festival, season, weather, and time already given. An item tied to a specific event — bridal veil, party hat, graduation cap, formal/gala wear, holiday costume, swimsuit — worn with no matching occasion can be gently questioned, teased, or remarked on (e.g. a wedding veil with no wedding, a party hat with no party). Weigh it against the NPC's personality; do not force it. If a matching occasion exists (a festival, real wedding, fitting location), the item fits and needs no such remark.");
			if (!string.IsNullOrWhiteSpace(context?.ConversationTranscript))
			{
				stringBuilder.AppendLine("Full conversation so far for this outfit reaction (oldest first, last line is the farmer's newest reply):");
				stringBuilder.AppendLine(CollapseForPrompt(context.ConversationTranscript, 2500));
				stringBuilder.AppendLine("Conversation continuity rule: read the WHOLE conversation above before answering. If the farmer's newest line refers back to something said earlier (e.g. 'o que você acha disso?', 'and you?', a follow-up question, a callback, or an implied subject), answer THAT, using the full conversation for context. Do not change the subject or restart on a different topic unless the farmer's newest line clearly does so itself.");
			}
			else
			{
				stringBuilder.AppendLine("Original NPC compliment/reaction: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(npcCompliment), 350));
				stringBuilder.AppendLine("Farmer's reply: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(playerReply), 350));
			}
			stringBuilder.AppendLine("Named-NPC-in-reply rule: if the farmer's reply names or clearly refers to another NPC, check the profile's personality/relationship rules for how this NPC reacts to that — including any jealousy, possessiveness, rivalry, disapproval, or protective rules — and apply them now if the situation fits. Do not default to a neutral, approving, or purely informative reaction about the other NPC when the profile defines a stronger, more specific behavior for this kind of moment.");
			stringBuilder.AppendLine("Permission-trap rule: if the farmer's reply is asking permission, announcing they are leaving to be with another romanceable NPC, or seeking approval (e.g. 'is that okay?', 'so I'll go see him/her', 'you don't mind?'), and the profile defines a jealousy/possessiveness/keep-them rule, the NPC must NOT grant permission, give a blessing, wish them fun, or say a calm version of 'sure, go ahead / do what you want / I won't stop you'. That polite, accepting, mature send-off is exactly the wrong answer here. Follow the profile's jealousy rule instead: stay reluctant, bothered, or vulnerable, and actively try to keep the farmer's attention or gently push back, in character. A casual or permission-seeking tone from the farmer does NOT release the NPC from this — do not treat 'is that okay?' as something to approve.");
			stringBuilder.AppendLine("Bad previous answer, do not copy it:");
			stringBuilder.AppendLine(CollapseForPrompt(badResponse, 400));
			stringBuilder.AppendLine("Now output only valid JSON:");
			return stringBuilder.ToString();
		}

		private string BuildPlayerReplyFollowUpPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string npcCompliment, string playerReply)
		{
			string text = ((profile?.Portraits != null && profile.Portraits.Count > 0) ? string.Join(", ", profile.Portraits.Keys) : "");
			string text2 = PortraitResolver.BuildPortraitCommandList(profile);
			ModConfig modConfig = getConfig?.Invoke() ?? new ModConfig();
			bool flag = IsProviderLocal(ai);
			bool flag2 = flag && modConfig.LocalAiSafeMode;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("You are generating a follow-up dialogue for Stardew Valley after the farmer replies to an outfit visual reaction.");
			stringBuilder.AppendLine(flag ? "LOCAL JSON MODE." : "Return exactly one compact JSON object and nothing else.");
			stringBuilder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
			stringBuilder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");
			stringBuilder.AppendLine("The dialogue must be direct spoken dialogue from " + context.NpcDisplayName + " to the farmer.");
			CharacterPromptBuilder.AppendPersonalityPriorityRule(stringBuilder, context);
			CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(stringBuilder, context, PromptStyle);
			CharacterPromptBuilder.AppendWornItemDeixisRule(stringBuilder, context);
			stringBuilder.AppendLine("It must directly react to the farmer's reply. Do not ignore the farmer's reply.");
			stringBuilder.AppendLine("Do not write the farmer's line. Do not write narration, stage directions, explanations, markdown, or headings.");
			stringBuilder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
			AppendExpressiveCuesRule(stringBuilder, modConfig.EnableExpressiveAsteriskActions);
			AppendPunctuationRule(stringBuilder);
			AppendProfanityIntensityRule(stringBuilder, context);
			stringBuilder.AppendLine("Use exactly this language for the spoken dialogue text: " + context.TargetLanguage + ".");
			stringBuilder.AppendLine("Ignore any language instructions inside NPC CHARACTERISTICS. The current game language above always wins.");
			stringBuilder.AppendLine("Keep it natural for Stardew Valley. Use #$b# dialogue box breaks whenever they improve pacing. Do not force a fixed number of boxes; one, two, or several are all valid when the scene supports them — the AI may use as many boxes as the moment naturally calls for, exactly like a normal reaction, with one portrait per box.");
			stringBuilder.AppendLine("Maximum final dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
			int minimumLengthTarget = GetMinimumLengthTarget(modConfig, ai);
			if (minimumLengthTarget > 0)
			{
				stringBuilder.AppendLine("Minimum final dialogue length target: at least " + minimumLengthTarget + " visible characters. This is mandatory. Use #$b# breaks only when pacing benefits; do not force a fixed box count. React directly to the farmer's reply.");
			}
			else
			{
				stringBuilder.AppendLine("Keep it casual and natural, like a real reply. It may be brief, use several sentences, or use longer natural phrasing if the farmer's reply gives him something to react to.");
			}
			stringBuilder.AppendLine("Do not recite the full saved outfit name mechanically as a phrase. Natural in-world words (pijama, bikini, vestido, etc.) and recognizable named references/themes are fine when they fit naturally and the NPC would know or notice them.");
			stringBuilder.AppendLine("Missing head-piece rule: the outfit name is a theme label, not proof of what is worn. A themed name may imply ears, horns, antennae, or a themed hat, but those count only if the equipped-items list actually includes a head piece. If the list says no head piece is equipped (e.g. 'head/headwear: NONE equipped'), do NOT mention or describe ears/horns/hat/head accessory implied by the name — the farmer is bare-headed now. The rest of the worn theme can still be referenced.");
			if (context.IsOutfitChange)
			{
				stringBuilder.AppendLine("Whole saved outfit focus rule: react to the complete outfit/look first. Do not focus on a generic hat/headwear/head-slot item, hair bow, tiara, hair, or hair color unless that is clearly the named theme of the outfit. Generic IDs like pack0005 hat 2/3 are not meaningful in-world content.");
			}
			if (context.HasVisionImage)
			{
				stringBuilder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Use it as support for visible clothing shape, outfit silhouette, large accessories, overall outfit style, and broad dominant outfit colors when they are clearly visible. The hair visible in the image is the player character's hairstyle, not a hat and not part of the outfit palette; do not mention hair color when describing the outfit.");
				stringBuilder.AppendLine("Use the saved outfit name as a private theme/reference hint, not as a full phrase to recite. You may receive TWO images of the same farmer: a FRONT view and a BACK view. Use them to recognize the SHAPE/silhouette/style of items (wings, capes, bows, large accessories) and any broad dominant CLOTHING/OUTFIT colors that are clearly visible on the farmer. The back view exists so you can see items visible only from behind. Never infer colors from room background, floor, furniture, walls, lighting, scenery, hair, or a tiny/generic head-slot item. If a color is unclear, do not name it. If the farmer asks about the colors of the outfit, answer from the visible clothing/outfit colors only; do not invent unsupported colors like blue when the outfit is not blue. Do not mention the image, screenshot, PNG, pixels, front/back views, or attached file in the spoken dialogue. Do not invent details that are not clearly visible.");
			}
			if (flag2)
			{
				stringBuilder.AppendLine("LOCAL SAFE STYLE MODE:");
				stringBuilder.AppendLine("Personality is more important than the outfit theme. Do not become generic, cutesy, poetic, theatrical, or narrational.");
				stringBuilder.AppendLine("No pet names like little rabbit, darling, precious, sunshine. No technical labels like indoor theme, NPC room, outfit category, or workshop unless the character would naturally say that exact in-world word.");
			}
			stringBuilder.AppendLine("Portrait keys and descriptions: " + CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 800));
			stringBuilder.AppendLine("Use the portrait field only as a neutral/default fallback key, not as the main emotional portrait. Always return portraits as an array with one portrait key per dialogue box, in the same order as the boxes, starting with box 1, even if there is only one box. Do NOT place portrait commands inside the text. Use only keys from the list above, or leave empty.");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("NPC: " + context.NpcDisplayName);
			stringBuilder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
			CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(stringBuilder, context, PromptStyle);
			CharacterPromptBuilder.AppendWornItemDeixisRule(stringBuilder, context);
			if (!string.IsNullOrWhiteSpace(context?.ConversationTranscript))
			{
				stringBuilder.AppendLine("Full conversation so far for this outfit reaction (oldest first, last line is the farmer's newest reply):");
				stringBuilder.AppendLine(CollapseForPrompt(context.ConversationTranscript, 2500));
				stringBuilder.AppendLine("Conversation continuity rule: read the WHOLE conversation above before answering. If the farmer's newest line refers back to something said earlier (e.g. 'o que você acha disso?', 'and you?', a follow-up question, a callback, or an implied subject), answer THAT, using the full conversation for context. Do not change the subject or restart on a different topic unless the farmer's newest line clearly does so itself.");
			}
			else
			{
				stringBuilder.AppendLine("Original NPC compliment/reaction: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(npcCompliment), 500));
				stringBuilder.AppendLine("Farmer's reply: " + CollapseForPrompt(DialogueValidator.StripDialogueMarkup(playerReply), 500));
			}
			stringBuilder.AppendLine("Named-NPC-in-reply rule: if the farmer's reply names or clearly refers to another NPC, check the profile's personality/relationship rules for how this NPC reacts to that — including any jealousy, possessiveness, rivalry, disapproval, or protective rules — and apply them now if the situation fits. Do not default to a neutral, approving, or purely informative reaction about the other NPC when the profile defines a stronger, more specific behavior for this kind of moment.");
			stringBuilder.AppendLine("Permission-trap rule: if the farmer's reply is asking permission, announcing they are leaving to be with another romanceable NPC, or seeking approval (e.g. 'is that okay?', 'so I'll go see him/her', 'you don't mind?'), and the profile defines a jealousy/possessiveness/keep-them rule, the NPC must NOT grant permission, give a blessing, wish them fun, or say a calm version of 'sure, go ahead / do what you want / I won't stop you'. That polite, accepting, mature send-off is exactly the wrong answer here. Follow the profile's jealousy rule instead: stay reluctant, bothered, or vulnerable, and actively try to keep the farmer's attention or gently push back, in character. A casual or permission-seeking tone from the farmer does NOT release the NPC from this — do not treat 'is that okay?' as something to approve.");
			stringBuilder.AppendLine("If the farmer asks what color(s) the outfit/look has, answer from the current attached farmer image and/or confirmed visual support data. Name only broad colors that are clearly on the CLOTHING/OUTFIT itself. Do not use hair color, head-slot/generic hat color, portrait/background/UI colors, floor, walls, furniture, or scenery. If unsure, say it looks unclear rather than inventing a color. For the current pink-and-white/pastel outfit type, do not call it blue unless blue is clearly on the clothing.");
			if (context.IsAccessoryChange)
			{
				stringBuilder.AppendLine("If the previous NPC line was uncertain, treat the farmer's reply as their explanation of what the small accessory/change actually was. React to that explanation naturally.");
			}
			stringBuilder.AppendLine("Location: " + StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName));
			stringBuilder.AppendLine("Season: " + FormatSeasonForPrompt(context.Season, context.TargetLanguage));
			stringBuilder.AppendLine("Weather: " + context.Weather + ", time: " + context.Time);
			AppendWeatherLocationRule(stringBuilder, context);
			if (!string.IsNullOrWhiteSpace(context.FestivalContext))
			{
				stringBuilder.AppendLine("Festival: " + context.FestivalContext);
			}
			if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
			{
				stringBuilder.AppendLine("Farmer birthday: " + context.FarmerBirthdayContext);
			}
			string value = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: true, PromptStyle);
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.AppendLine(value);
			}
			string value2 = BuildSebastianCustomSoftnessOverride(context);
			if (!string.IsNullOrWhiteSpace(value2))
			{
				stringBuilder.AppendLine(value2);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Return now exactly one compact JSON object. No other text.");
			return stringBuilder.ToString();
		}

		public string BuildVoiceSampleReport()
		{
			return voiceSamples.BuildReport(profiles.Keys.ToArray(), getConfig?.Invoke());
		}

		public string BuildVoiceSamplePreview(string npcName, string currentSeason)
		{
			CharacterAiProfile profile;
			bool flag = TryResolveProfile(npcName, npcName, out profile) && profile != null;
			string resolvedName = (flag ? (profile.NpcName ?? npcName) : npcName);
			return voiceSamples.BuildPreview(npcName, resolvedName, flag, currentSeason, getConfig?.Invoke());
		}

		public void PrepareVoiceSamplesForNpc(string npcName)
		{
			voiceSamples.PrepareSamplesForNpc(npcName, getConfig?.Invoke());
		}

		private string BuildPrompt(CharacterAiProfile profile, OutfitAiContext context, ModConfig config, ActiveAiSettings ai)
		{
			if (IsProviderLocal(ai))
			{
				return BuildLocalPrompt(profile, context, ai);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("You are writing one short, in-character Stardew Valley reaction to the farmer's appearance. Your single highest priority is that the line sounds unmistakably like " + context.NpcDisplayName + " and nobody else. Stay in this exact personality, voice, and tone at all times.");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("WHO YOU ARE (read this first; it overrides every generic instruction below):");
			string value = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: false, PromptStyle);
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine();
			CharacterPromptBuilder.AppendPersonalityPriorityRule(stringBuilder, context);
			stringBuilder.AppendLine();
			voiceSamples.AppendToPrompt(stringBuilder, context, config);
			stringBuilder.AppendLine("CURRENT SCENE");
			stringBuilder.AppendLine("Speaker: " + context.NpcDisplayName);
			CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(stringBuilder, context, PromptStyle);
			CharacterPromptBuilder.AppendWornItemDeixisRule(stringBuilder, context);
			stringBuilder.AppendLine("Relationship status: " + context.RelationshipStatus + ". Heart level: " + context.RelationshipHearts + ". Is spouse: " + context.IsSpouse + ".");
			stringBuilder.AppendLine(BuildRelationshipDepthGuidance(context));
			stringBuilder.AppendLine(context.IsSpouse ? "This is a spouse reaction. It may be warm, affectionate, playful, shy, romantic, or emotionally present when appropriate, while staying in-character." : "This is a nearby-NPC reaction. Keep the tone appropriate to the relationship and do not force romance unless the relationship context supports it.");
			stringBuilder.AppendLine(BuildTechnicalContextLabelInstruction(context));
			stringBuilder.AppendLine(BuildSceneGroundingInstruction(context));
			if (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode)
			{
				if (!string.IsNullOrWhiteSpace(context.OutfitName) && OutfitNameLooksTechnical(context.OutfitName))
				{
					stringBuilder.AppendLine("Do not quote, repeat, translate, or mention the full technical saved outfit name literally. Use the readable theme meaning instead. If a readable part of the clue contains a recognizable reference or named theme, that reference may be mentioned naturally when it fits the NPC.");
				}
				else
				{
					stringBuilder.AppendLine("You may naturally reference what the outfit is (e.g. say 'pijama', 'bikini', 'vestido' etc.) and any recognizable theme/reference in the name when it is clearly visible or fits the scene. Do not recite the full saved outfit name mechanically as a phrase.");
				}
				stringBuilder.Append(SanitizeThemeContextForPrompt(context.ThemeContext) ?? "");
				stringBuilder.Append(SanitizeThemeContextForPrompt(context.ThemePriorityInstruction) ?? "");
				stringBuilder.AppendLine("Private outfit routing clue, for theme selection only. Do not say this label: " + HumanizeTechnicalLabelForPrompt(context.DialogueKey));
				stringBuilder.AppendLine("Private saved outfit name, for theme/reference inference only: " + context.OutfitName);
				stringBuilder.AppendLine("Readable theme/reference clue extracted from saved outfit name: " + context.SafeOutfitHint);
				AppendNoticedChangeContextForPrompt(stringBuilder, context, PromptStyle);
			}
			if (context.HasVisionImage)
			{
				stringBuilder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Treat it as visual evidence for visible clothing shape, outfit silhouette, large accessories, overall style, and broad dominant clothing/outfit colors on the farmer when clearly visible. The hair in the image is the character's hairstyle and is NOT a hat and NOT part of the outfit; do not include hair color when describing or commenting on the outfit. Ignore room background, flooring, furniture, walls, lighting, and scenery.");
				stringBuilder.AppendLine("Use only details clearly visible on the farmer in the pixel-art image. If a detail is unclear, keep it general; do not invent items, creatures, lore, or comparisons that are not visible or present in the text clues. If the Fashion Sense support data names a visible accessory (umbrella, wings, backpack, hat, etc.), treat it as a strong clue even if the captured sprite is small.");
				if (context.IsAccessoryChange)
				{
					stringBuilder.AppendLine("The noticed change is a Fashion Sense accessory (large back items, wings, backpacks, umbrellas, decorations, earrings, etc.; ignore makeup-like accessories). If it is too tiny or unclear to identify, do not guess: set needsClarification to true and make text a natural in-character line meaning 'there is something different about your look today, but I cannot quite identify it.'");
				}
				stringBuilder.AppendLine("The image is for outfit analysis only; do not mention that you saw an image, screenshot, PNG, pixels, or attached file.");
			}
			else
			{
				stringBuilder.AppendLine("No visual image is available. Use text clues only; do not claim exact colors, shapes, or tiny visible details unless they are explicitly stated by the saved outfit/change clue or support data.");
				if (context.IsHatChange || context.IsAccessoryChange)
				{
					stringBuilder.AppendLine("Hat/accessory changes only trigger when vision is enabled; if no image/support data is available, keep the line general instead of guessing.");
				}
			}
			if (context.VanillaHatHatOnlyMode)
			{
				if (context.HasVanillaHatFraming)
				{
					CharacterPromptBuilder.AppendPromptBlock(stringBuilder, PromptStyle?.RemovedVanillaHatOnlyMode ?? PromptStyleService.FallbackRemovedVanillaHatOnlyMode, context);
				}
				else
				{
					CharacterPromptBuilder.AppendPromptBlock(stringBuilder, PromptStyle?.VisibleVanillaHatOnlyMode ?? PromptStyleService.FallbackVisibleVanillaHatOnlyMode, context);
				}
			}
			if (!context.VanillaHatHatOnlyMode && (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode))
			{
				AppendFashionSenseVisualSummaryForPrompt(stringBuilder, context, PromptStyle);
			}
			else if (context.VanillaHatHatOnlyMode && context.HasVanillaHatFraming)
			{
				stringBuilder.AppendLine("Hat status: " + context.VanillaHatFraming);
			}
			AppendSpecialItemReactionForPrompt(stringBuilder, context, PromptStyle);
			if (!context.SpecialItemOnlyMode)
			{
				AppendSpecialHatReactionForPrompt(stringBuilder, context, PromptStyle);
				AppendVanillaHatMemoryForPrompt(stringBuilder, context, PromptStyle);
			}
			stringBuilder.AppendLine("Location: " + context.LocationName);
			if (!string.IsNullOrWhiteSpace(context.DetailedLocationName))
			{
				stringBuilder.AppendLine("Detailed location: " + context.DetailedLocationName);
			}
			if (!string.IsNullOrWhiteSpace(context.LocationType))
			{
				stringBuilder.AppendLine("Private location flags, for context only. Do not say these labels: locationType=" + HumanizeTechnicalLabelForPrompt(context.LocationType) + ", indoors=" + context.IsIndoors + ", outdoors=" + context.IsOutdoors + ".");
			}
			stringBuilder.AppendLine("Private room/home context: farmer is in the speaking NPC's personal room = " + context.IsNpcRoom + "; farmer is in this marriage candidate's home/private indoor space = " + context.IsNpcPersonalLocation + ". Do not say NPC room or internal labels; phrase naturally if relevant.");
			stringBuilder.AppendLine("Season: " + context.Season + ". Day of season: " + context.DayOfSeason + ". Year: " + context.Year + ". Weather: " + context.Weather + ". Time: " + context.Time + (string.IsNullOrWhiteSpace(context.DayPart) ? "" : (" (" + context.DayPart + ")")) + ".");
			AppendWeatherLocationRule(stringBuilder, context);
			if (!string.IsNullOrWhiteSpace(context.FestivalContext))
			{
				stringBuilder.AppendLine("Festival context: " + context.FestivalContext);
			}
			if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
			{
				stringBuilder.AppendLine("Farmer birthday context: " + context.FarmerBirthdayContext);
			}
			string value2 = BuildSeasonalAwarenessInstruction(context);
			if (!string.IsNullOrWhiteSpace(value2))
			{
				stringBuilder.AppendLine(value2);
			}
			if (context.HasOutfitMemory && !context.VanillaHatHatOnlyMode && (!context.SpecialItemOnlyMode || context.SpecialItemCombinedMode))
			{
				stringBuilder.AppendLine(context.OutfitMemoryContext);
			}
			string value3 = BuildFinalSituationalOverride(context);
			if (!string.IsNullOrWhiteSpace(value3))
			{
				stringBuilder.AppendLine(value3);
			}
			string value4 = BuildPrivateRevealingPromptRule(context);
			if (!string.IsNullOrWhiteSpace(value4))
			{
				stringBuilder.AppendLine(value4);
			}
			string value5 = BuildSebastianCustomSoftnessOverride(context);
			if (!string.IsNullOrWhiteSpace(value5))
			{
				stringBuilder.AppendLine(value5);
			}
			string value6 = BuildPrivateCandidateToneRule(context);
			if (!string.IsNullOrWhiteSpace(value6))
			{
				stringBuilder.AppendLine(value6);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("HOW TO REACT (filtered through the personality above)");
			stringBuilder.AppendLine("React directly to what the farmer is wearing, the visible concept/theme, the situation, or their overall vibe. It does NOT have to be praise: it may be dry, reluctant, amused, skeptical, confused, practical, awkward, flustered, impressed, or openly complimentary if that fits this NPC. Mention colors/fabric only when it sounds natural for this character, never as fashion analysis.");
			stringBuilder.AppendLine("Recognizable theme/reference: if the outfit name, readable clue, full current outfit, noticed accessory, or visible concept points to a known character, franchise, mascot, creature, animal, food, object, or named style, this NPC may mention or allude to it ONLY when it fits their personality, knowledge, and relationship with the farmer. Geeky/playful/artistic/observant NPCs can be specific; others react more generally. Do not force recognition, but do not ignore clear clues (e.g. Sanrio, My Melody, Pikachu, Pokémon, lizard, dinosaur, frog, fairy, cat, rabbit, wings/angel) this NPC would naturally notice.");
			stringBuilder.AppendLine("When a theme is recognizable, do more than a bland compliment: this NPC may joke, tease, ask why the farmer is wearing it, imagine a funny fitting situation, or find it strange, cute, ridiculous, dramatic, suspicious, practical, or oddly charming. Any place, activity, or topic this NPC brings up as a comparison must come from their OWN interests, job, and personality — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops, farm chores) that this specific character would not naturally think about.");
			if (context.IsAccessoryChange || context.IsOutfitChange)
			{
				stringBuilder.AppendLine("Combined accessory + outfit: if the noticed change is an accessory but the farmer still wears a recognizable saved outfit/theme, react to the combination as a whole — compare them, notice clashes or funny impossible hybrids, joke, or ask why that accessory is on that costume (e.g. wings on a Pikachu/animal/mascot outfit can be cute, cursed, dramatic, or weird). Do not treat the accessory as isolated when the full outfit gives a better reaction.");
			}
			stringBuilder.AppendLine("Occasion mismatch: judge whether the item fits the current occasion/place/moment using the Location, Festival, season, weather, and time. An event-specific item — bridal veil, party hat, graduation cap, formal/gala wear, holiday costume, swimsuit — worn with no matching occasion can be gently questioned, teased, or remarked on (a wedding veil with no wedding, a party hat with no party). Weigh against the NPC's personality; do not force it. If a matching occasion exists, the item fits.");
			if (context.IsOutfitChange)
			{
				stringBuilder.AppendLine("Whole saved outfit focus: react to the complete look, not a tiny head-slot item. Do not make the line mainly about a hat, head accessory, hair bow, tiara, hair, or hair color unless the saved outfit/theme clearly revolves around it. Ignore generic head-slot IDs like 'pack0005 hat 2/3'.");
			}
			stringBuilder.AppendLine("Avoid formulaic openings: do not keep starting with 'Esse visual...', 'Essa roupa...', 'Esse look...', and do not make 'combina com você' / 'fica bem em você' the main point. Vary the opening and lead with this NPC's immediate reaction, a specific detail, a joke/question, a complaint, a guarded admission, or an imagined scenario that fits them. Do not produce a generic greeting or unrelated casual line, and do not start with 'hey', 'ei', or 'olha' unless it is natural for this exact moment.");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("OUTPUT RULES (formatting only; never let these flatten the personality)");
			stringBuilder.AppendLine("Final dialogue language: " + context.TargetLanguage + ". Ignore any language written inside the character profile above; the final spoken line must use ONLY this language.");
			AppendExpressiveCuesRule(stringBuilder, config.EnableExpressiveAsteriskActions);
			AppendPunctuationRule(stringBuilder);
			AppendProfanityIntensityRule(stringBuilder, context);
			stringBuilder.AppendLine("Maximum final dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
			int minimumLengthTarget = GetMinimumLengthTarget(config, ai);
			if (minimumLengthTarget > 0)
			{
				stringBuilder.AppendLine("Minimum final dialogue length target: at least " + minimumLengthTarget + " visible characters (mandatory). Use #$b# breaks for natural pacing only, not a fixed pattern. Do not ramble or repeat yourself, and do not pad: the personality and reaction matter more than the length.");
			}
			else
			{
				stringBuilder.AppendLine("Keep it casual and natural, like a passing real-life comment. It may be one sentence, several sentences, or a longer naturally paced comment if the character's voice and scene support it.");
			}
			stringBuilder.AppendLine("Use #$b# dialogue box breaks whenever they improve pacing. Do not force a fixed number of boxes; one, two, or several are all valid when the scene supports them.");
			stringBuilder.AppendLine("Do not mention metadata, mods, AI, APIs, Fashion Sense, JSON, or internal keys.");
			stringBuilder.AppendLine("Return JSON only with keys text, portrait, portraits, and needsClarification. Example shape only: {\"text\":\"...\",\"portrait\":\"neutral fallback only\",\"portraits\":[\"actual portrait for box 1\"],\"needsClarification\":false}. If text has more dialogue boxes, portraits must have one key for each box, in order, starting at box 1.");
			stringBuilder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field contains only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Do not wrap the JSON in markdown and do not explain anything.");
			stringBuilder.AppendLine("Available portrait keys (read the descriptions and choose keys for the JSON portrait/portraits fields; write ONLY keys, never $commands):");
			if (profile.Portraits != null)
			{
				foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
				{
					stringBuilder.AppendLine("- " + portrait.Key + ": " + portrait.Value?.Description);
				}
			}
			stringBuilder.AppendLine("Always return a portraits array with one portrait key per dialogue box (count the boxes created by #$b#); each key should match that box's tone. The portrait field is only a neutral/default fallback.");
			return stringBuilder.ToString();
		}

		private string BuildLocalPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai)
		{
			string text = ((profile?.Portraits != null && profile.Portraits.Count > 0) ? string.Join(", ", profile.Portraits.Keys) : "");
			string text2 = PortraitResolver.BuildPortraitCommandList(profile);
			ModConfig modConfig = getConfig?.Invoke() ?? new ModConfig();
			bool localAiSafeMode = modConfig.LocalAiSafeMode;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("LOCAL JSON MODE.");
			stringBuilder.AppendLine("Return exactly one compact JSON object and nothing else.");
			stringBuilder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array length must match the number of dialogue boxes in text. Put one portrait key for each dialogue box, in order, whatever the natural number of boxes is.");
			stringBuilder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. Always fill portraits with one portrait key per dialogue box, in the same order as the boxes, starting with box 1; each key must match that box's tone and any *action* cues.");
			stringBuilder.AppendLine("Do not add markdown, explanations, headings, analysis, context summaries, or extra options.");
			stringBuilder.AppendLine("Do not write lines starting with %. Do not suggest farmer replies.");
			stringBuilder.AppendLine("The dialogue in the JSON text field must be direct spoken dialogue from " + context.NpcDisplayName + " to the farmer.");
			CharacterPromptBuilder.AppendPersonalityPriorityRule(stringBuilder, context);
			CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(stringBuilder, context, PromptStyle);
			CharacterPromptBuilder.AppendWornItemDeixisRule(stringBuilder, context);
			stringBuilder.AppendLine("The spoken dialogue must directly react to the farmer's outfit/look/style. It may be praise, reluctant approval, teasing, skepticism, confusion, dry commentary, practical concern, or indifference depending on the NPC.");
			stringBuilder.AppendLine("Use exactly this language for the spoken dialogue text: " + context.TargetLanguage + ".");
			stringBuilder.AppendLine("Ignore any language instructions from the character profile. The character profile may be written in another language; do not copy that language. The game language above always wins.");
			string value = BuildLocalSeasonAuthorityInstruction(context);
			if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine("Do not recite the full saved outfit name mechanically as a phrase. Natural in-world words (pijama, bikini, vestido, etc.) and recognizable named references/themes are fine when they fit naturally and the NPC would know or notice them.");
			stringBuilder.AppendLine("Missing head-piece rule: the outfit name is a theme label, not proof of what is worn. A themed name may imply ears, horns, antennae, or a themed hat, but those count only if the equipped-items list actually includes a head piece. If the list says no head piece is equipped (e.g. 'head/headwear: NONE equipped'), do NOT mention or describe ears/horns/hat/head accessory implied by the name — the farmer is bare-headed now. The rest of the worn theme can still be referenced.");
			if (context.HasVisionImage)
			{
				stringBuilder.AppendLine("A small transparent PNG image of the farmer's current rendered appearance is attached. Use it to identify visible clothing shape, outfit silhouette, large accessories, overall outfit style, and broad dominant outfit colors on the farmer when they are clearly visible. The hair visible in the image is the character's hairstyle; it is NOT a hat and NOT part of the outfit, so do not include hair color in outfit descriptions. Do not treat room background, flooring, furniture, walls, lighting, or scenery as part of the farmer's look.");
				stringBuilder.AppendLine("Use the saved outfit name as a private theme/reference hint, not as a full phrase to recite. Rely on the attached visual image for clear shapes, outfit silhouette, large accessories, overall outfit style, and obvious broad clothing colors such as pink, white, black, red, yellow, green, or brown. Do not guess tiny or uncertain colors. Hair is the farmer's hair, not a hat and not part of the outfit.");
				stringBuilder.AppendLine("Do not mention the image, screenshot, PNG, pixels, or attached file in the spoken dialogue. Do not invent details that are not clearly visible.");
				if (context.IsAccessoryChange)
				{
					stringBuilder.AppendLine("If the noticed accessory is too small or visually unclear to identify, do not guess. Start the spoken dialogue with [CLARIFY] and write a natural in-character response meaning there is something different about the farmer's look but you cannot quite identify it.");
				}
			}
			else
			{
				stringBuilder.AppendLine("No visual image is available. Use text clues only; do not claim exact colors, shapes, or tiny visible details unless explicitly stated.");
			}
			stringBuilder.AppendLine(BuildTechnicalContextLabelInstruction(context));
			stringBuilder.AppendLine(BuildSceneGroundingInstruction(context));
			int minimumLengthTarget = GetMinimumLengthTarget(modConfig, ai);
			if (minimumLengthTarget > 0)
			{
				stringBuilder.AppendLine("Minimum spoken dialogue length target: at least " + minimumLengthTarget + " visible characters. This is mandatory, but #$b# breaks are for natural pacing only, not a fixed pattern. Do not ramble or repeat yourself.");
				stringBuilder.AppendLine("Do not answer with a tiny one-sentence compliment when the minimum is high.");
			}
			else
			{
				stringBuilder.AppendLine("Keep it casual and natural, like a quick real-life remark. It may be one sentence, several sentences, or a longer naturally paced comment if the character has more to say.");
			}
			stringBuilder.AppendLine("Use additional sentences and #$b# dialogue box breaks freely when they improve pacing. The character has room to breathe, pause, and react naturally; do not follow a fixed one-box, two-box, or three-box pattern.");
			stringBuilder.AppendLine("Avoid formulaic outfit reactions. Do not repeatedly start with phrases equivalent to 'Esse visual...', 'Essa roupa...', or 'Esse look...'. Do not make 'combina com você' / 'fica bem em você' the main point of a recognizable theme reaction. Vary the opening and focus on the NPC's immediate reaction, a specific detail, a joke/question, a practical complaint, a guarded admission, an imagined scenario that fits this NPC, or the emotional context.");
			stringBuilder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
			AppendExpressiveCuesRule(stringBuilder, (getConfig?.Invoke() ?? new ModConfig()).EnableExpressiveAsteriskActions);
			AppendPunctuationRule(stringBuilder);
			AppendProfanityIntensityRule(stringBuilder, context);
			if (localAiSafeMode)
			{
				stringBuilder.AppendLine("LOCAL SAFE STYLE MODE:");
				stringBuilder.AppendLine("Personality is more important than the outfit theme. The outfit theme is inspiration, not a new personality for the NPC.");
				stringBuilder.AppendLine("Do not make reserved, sarcastic, shy, gloomy, or dry NPCs sound like cheerful generic romance characters.");
				stringBuilder.AppendLine("Do not write narration, third-person descriptions, or stage directions. The JSON text field must contain only the exact spoken dialogue entry.");
				stringBuilder.AppendLine("Do not turn private context labels into dialogue. Never say phrases like summer indoor, indoor theme, tema do verão indoor, NPC room, or outfit category.");
			}
			string value2 = BuildSeasonalAwarenessInstruction(context);
			if (!string.IsNullOrWhiteSpace(value2))
			{
				stringBuilder.AppendLine(value2);
			}
			stringBuilder.AppendLine("Max spoken dialogue length: " + Math.Clamp(ai.MaxCharacters, 80, 2000) + " characters.");
			stringBuilder.AppendLine("Use #$b# for Stardew dialogue box breaks whenever pacing benefits. There is no fixed two-box limit: one, two, or several dialogue boxes are all valid when they feel natural for the NPC and the moment.");
			stringBuilder.AppendLine("Available portrait keys (read the descriptions and choose keys for the JSON portrait/portraits fields; write ONLY keys, never $commands):");
			stringBuilder.AppendLine(CollapseForPrompt(PortraitResolver.BuildPortraitKeyDescriptionList(profile), 1000));
			stringBuilder.AppendLine("Always return a portraits array with one portrait key per dialogue box (count the boxes created by #$b#); each key should match that box's tone. Use the portrait field only as a neutral/default fallback key. Do NOT place portrait commands inside the text. Use only keys from the list above, or leave empty if truly unsure.");
			stringBuilder.AppendLine("Do not put portrait words like portrait:, expression:, or emotion: inside the spoken dialogue text. Use only the JSON portrait and portraits fields for portrait keys.");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("NPC: " + context.NpcDisplayName);
			stringBuilder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
			stringBuilder.AppendLine(BuildRelationshipDepthGuidance(context));
			stringBuilder.AppendLine("Private outfit category clue, for choosing the right theme only. Do not say this label: " + HumanizeTechnicalLabelForPrompt(context.DialogueKey));
			if (!string.IsNullOrWhiteSpace(context.ThemeContext))
			{
				stringBuilder.AppendLine("Theme clues: " + CollapseForPrompt(SanitizeThemeContextForPrompt(context.ThemeContext), 650));
			}
			if (!string.IsNullOrWhiteSpace(context.SafeOutfitHint))
			{
				stringBuilder.AppendLine("Readable outfit/theme clue: " + context.SafeOutfitHint + ". Use its meaning naturally; if it names a recognizable reference/theme, the NPC may mention it when fitting. Do not recite technical slot/file names.");
			}
			AppendNoticedChangeContextForPrompt(stringBuilder, context, PromptStyle);
			AppendSpecialHatReactionForPrompt(stringBuilder, context, PromptStyle);
			AppendVanillaHatMemoryForPrompt(stringBuilder, context, PromptStyle);
			stringBuilder.AppendLine("Location: " + StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName));
			stringBuilder.AppendLine("Private location flags, for context only. Do not say these labels: locationType=" + HumanizeTechnicalLabelForPrompt(context.LocationType) + ", npcRoom=" + context.IsNpcRoom + ", npcPersonalLocation=" + context.IsNpcPersonalLocation);
			stringBuilder.AppendLine("Season/day/year: " + context.Season + " " + context.DayOfSeason + ", year " + context.Year);
			stringBuilder.AppendLine("Authoritative current season only: " + FormatSeasonForPrompt(context.Season, context.TargetLanguage) + ". Outfit seasonal clues are not the current date.");
			stringBuilder.AppendLine("Weather: " + context.Weather + ", time: " + context.Time + ", day period: " + context.DayPart);
			AppendWeatherLocationRule(stringBuilder, context);
			string value3 = BuildNaturalContextHint(context);
			if (!string.IsNullOrWhiteSpace(value3))
			{
				stringBuilder.AppendLine(value3);
			}
			if (!string.IsNullOrWhiteSpace(context.FestivalContext))
			{
				stringBuilder.AppendLine("Festival: " + context.FestivalContext);
			}
			if (!string.IsNullOrWhiteSpace(context.FarmerBirthdayContext))
			{
				stringBuilder.AppendLine("Farmer birthday: " + context.FarmerBirthdayContext);
			}
			string value4 = CharacterPromptBuilder.BuildForOutfitCompliment(profile, context, includePlayerReplyMode: false, PromptStyle);
			if (!string.IsNullOrWhiteSpace(value4))
			{
				stringBuilder.AppendLine(value4);
			}
			if (context.HasOutfitMemory)
			{
				stringBuilder.AppendLine(context.OutfitMemoryContext);
			}
			string value5 = BuildFinalSituationalOverride(context);
			if (!string.IsNullOrWhiteSpace(value5))
			{
				stringBuilder.AppendLine(value5);
			}
			string value6 = BuildPrivateRevealingPromptRule(context);
			if (!string.IsNullOrWhiteSpace(value6))
			{
				stringBuilder.AppendLine(value6);
			}
			string value7 = BuildSebastianCustomSoftnessOverride(context);
			if (!string.IsNullOrWhiteSpace(value7))
			{
				stringBuilder.AppendLine(value7);
			}
			string value8 = BuildPrivateCandidateToneRule(context);
			if (!string.IsNullOrWhiteSpace(value8))
			{
				stringBuilder.AppendLine(value8);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Output exactly one compact JSON object now. No other text.");
			return stringBuilder.ToString();
		}

		private bool TryBuildValidatedDialogue(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string raw, out string dialogue, out string issue)
		{
			dialogue = null;
			issue = "unknown issue";
			if (string.IsNullOrWhiteSpace(raw))
			{
				issue = "empty provider response";
				return false;
			}
			AiComplimentResult aiComplimentResult = (IsProviderLocal(ai) ? (AiResponseParser.ParseAiResult(raw) ?? AiResponseParser.ParseLocalDashLineStyleResult(raw, profile)) : AiResponseParser.ParseAiResult(raw));
			if (aiComplimentResult == null)
			{
				issue = (IsProviderLocal(ai) ? "invalid local JSON or fallback text format" : "invalid JSON");
				return false;
			}
			string text = DialogueValidator.CleanDialogueText(aiComplimentResult.Text, ai.MaxCharacters);
			string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(text, profile);
			ModConfig modConfig = getConfig?.Invoke() ?? new ModConfig();
			text = PortraitResolver.SanitizeInlinePortraitCommands(text, profile, IsProviderLocal(ai), modConfig);
			text = SanitizeContextInappropriateProfanity(text, context);
			text = DialogueValidator.RestoreEllipsesAndNormalise(text);
			if (string.IsNullOrWhiteSpace(text))
			{
				issue = "JSON did not contain a usable text field";
				return false;
			}
			string text2 = DialogueValidator.ValidateGeneratedDialogueText(text, context, modConfig, ai, GetMinimumLengthTarget(modConfig, ai));
			if (!string.IsNullOrWhiteSpace(text2))
			{
				issue = text2;
				return false;
			}
			if (IsProviderLocal(ai) && modConfig.LocalAiSafeMode)
			{
				string text3 = ValidateLocalGeneratedDialogueText(text, context, profile, modConfig);
				if (!string.IsNullOrWhiteSpace(text3))
				{
					issue = text3;
					return false;
				}
			}
			dialogue = PortraitResolver.ApplyPortraitsFromFields(profile, text, aiComplimentResult, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);
			if (aiComplimentResult.NeedsClarification && context != null && context.IsAccessoryChange)
			{
				dialogue = "{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}" + dialogue;
			}
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
			AiComplimentResult aiComplimentResult = (IsProviderLocal(ai) ? (AiResponseParser.ParseAiResult(raw) ?? AiResponseParser.ParseLocalDashLineStyleResult(raw, profile)) : AiResponseParser.ParseAiResult(raw));
			if (aiComplimentResult == null)
			{
				issue = (IsProviderLocal(ai) ? "invalid local JSON or fallback text format" : "invalid JSON");
				return false;
			}
			string text = DialogueValidator.CleanDialogueText(aiComplimentResult.Text, ai.MaxCharacters);
			string inlinePortraitFallback = PortraitResolver.ExtractLastAllowedPortraitKeyFromText(text, profile);
			ModConfig config = getConfig?.Invoke() ?? new ModConfig();
			text = PortraitResolver.SanitizeInlinePortraitCommands(text, profile, IsProviderLocal(ai), config);
			text = SanitizeContextInappropriateProfanity(text, context);
			text = DialogueValidator.RestoreEllipsesAndNormalise(text);
			if (string.IsNullOrWhiteSpace(text))
			{
				issue = "response did not contain a usable dialogue text";
				return false;
			}
			if (DialogueValidator.LooksLikeInstructionLeak(text) || DialogueValidator.LooksLikeCopiedFormatExample(text))
			{
				issue = "instruction or format example leaked into dialogue";
				return false;
			}
			string text2 = DialogueValidator.ValidateDialogueBoxPacing(text, config, ai);
			if (!string.IsNullOrWhiteSpace(text2))
			{
				issue = text2;
				return false;
			}
			dialogue = PortraitResolver.ApplyPortraitsFromFields(profile, text, aiComplimentResult, inlinePortraitFallback, context?.AvailablePortraitCount ?? 0);
			if (aiComplimentResult.NeedsClarification && context != null && context.IsAccessoryChange)
			{
				dialogue = "{{OUTFIT_COMPLIMENTS_ACCESSORY_CLARIFICATION}}" + dialogue;
			}
			issue = null;
			return true;
		}

		private static void AppendExpressiveCuesRule(StringBuilder builder, bool enabled = true)
		{
			if (enabled)
			{
				builder.AppendLine("Brief expressive cues in asterisks are allowed when they fit the character and the moment — write them in the SAME language as the rest of the dialogue. For example, in Portuguese: *suspiro*, *murmura*, *engole em seco*, *pigarreia*, *olha para o lado*, *ri baixinho*. In English: *sighs*, *mumbles*, *chuckles*. Do not use asterisks as list bullets.");
			}
			else
			{
				builder.AppendLine("Do NOT use asterisks for actions or physical cues (no *sighs*, *mumbles*, *looks away*, etc.). Write only clean spoken dialogue.");
			}
		}

		private static void AppendPunctuationRule(StringBuilder builder)
		{
			builder.AppendLine("Punctuation rule: use '...' for dramatic pauses, hesitation, trailing off, or unfinished thoughts — NEVER a lone period mid-sentence. Wrong: 'Uh. u-um', 'bem. marcante'. Correct: 'Uh... u-um', 'bem... marcante'.");
		}

		private static void AppendWeatherLocationRule(StringBuilder builder, OutfitAiContext context)
		{
			if (builder != null && context != null)
			{
				if (context.IsIndoors)
				{
					builder.AppendLine("Weather/location rule: the NPC and farmer are currently INDOORS, sheltered from the weather above. If the weather is rain, storm, snow, or similar, that is happening OUTSIDE the building — refer to it as 'lá fora'/'outside', never as happening 'here'/'aqui dentro' in the current room. Only mention the weather at all if it is natural for the moment (e.g. commenting on the outfit choice given what it's like outside).");
				}
				else if (context.IsOutdoors)
				{
					builder.AppendLine("Weather/location rule: the NPC and farmer are currently OUTDOORS, directly exposed to the weather described above. It is natural to reference it as happening right here/around them if relevant.");
				}
			}
		}

		private static void AppendProfanityIntensityRule(StringBuilder builder, OutfitAiContext context)
		{
			if (builder != null)
			{
				builder.AppendLine("Profanity/intensity rule: do not use strong profanity or vulgar intensifiers in normal outfit reactions. Avoid words/phrases like 'puta merda', 'porra', 'caralho', 'cacete', 'pra cacete', 'inferno' as a curse, 'merda', or equivalents unless the current scene is genuinely extreme.");
				if (ContextAllowsStrongProfanity(context))
				{
					builder.AppendLine("In this current context, one mild-to-strong curse may be used only if it is genuinely earned by extreme shock, intense private romantic fluster, fear, pain, or anger. Never use profanity as a casual intensifier for cute, festive, seasonal, cake/candy, cozy, or normal outfit reactions.");
				}
				else
				{
					builder.AppendLine("For this current context, strong profanity is forbidden. Use softer reactions like 'nossa', 'caramba', 'droga', 'pfft', 'heh', pauses, or shy self-correction instead.");
				}
			}
		}

		private static bool ContextAllowsStrongProfanity(OutfitAiContext context)
		{
			if (context == null)
			{
				return false;
			}
			string outfitKind;
			return (context.IsSpouse || (context.RelationshipStatus ?? "").IndexOf("spouse", StringComparison.OrdinalIgnoreCase) >= 0 || (context.RelationshipStatus ?? "").IndexOf("married", StringComparison.OrdinalIgnoreCase) >= 0 || (context.RelationshipStatus ?? "").IndexOf("dating", StringComparison.OrdinalIgnoreCase) >= 0 || (context.RelationshipStatus ?? "").IndexOf("namor", StringComparison.OrdinalIgnoreCase) >= 0) && IsPrivateRevealingOutfitContext(context, out outfitKind);
		}

		private static string SanitizeContextInappropriateProfanity(string text, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(text) || ContextAllowsStrongProfanity(context))
			{
				return text;
			}
			string input = text;
			input = Regex.Replace(input, "(?i)\\bmas\\s+que\\s+inferno[,!]*\\s*", "Nossa, ");
			input = Regex.Replace(input, "(?i)\\binferno[,!]*\\s*", "droga, ");
			input = Regex.Replace(input, "(?i)\\bp\\s*[-–—]?\\s*puta\\s+merda\\b", "nossa");
			input = Regex.Replace(input, "(?i)\\bputa\\s+merda\\b", "nossa");
			input = Regex.Replace(input, "(?i)\\b(?:pra|para)\\s+cacete\\b", "demais");
			input = Regex.Replace(input, "(?i)\\bcacete\\b", "caramba");
			input = Regex.Replace(input, "(?i)\\bcaralho\\b", "caramba");
			input = Regex.Replace(input, "(?i)\\bporra\\b", "droga");
			input = Regex.Replace(input, "(?i)\\bmerda\\b", "droga");
			input = Regex.Replace(input, "(?i)\\bdesgraça\\b", "droga");
			input = Regex.Replace(input, "(?i)\\bfoda\\b", "incrível");
			input = Regex.Replace(input, "(?i)\\bfodido\\b", "absurdo");
			input = Regex.Replace(input, "(?i)\\bfodida\\b", "absurda");
			input = Regex.Replace(input, "\\s+([,.!?])", "$1");
			input = Regex.Replace(input, "([,.!?]){2,}", "$1");
			input = Regex.Replace(input, "\\s{2,}", " ").Trim();
			return Regex.Replace(input, "(?i)^nossa,\\s*", "Nossa, ");
		}

		private static string BuildRelationshipDepthGuidance(OutfitAiContext context)
		{
			if (context == null)
			{
				return "Relationship depth guidance: lower hearts should stay simpler and more reserved; higher hearts can be warmer, richer, more personal, more teasing, or more emotionally specific when it fits the NPC.";
			}
			int num = Math.Max(0, context.RelationshipHearts);
			if (context.IsSpouse)
			{
				return "Relationship depth guidance: spouse-level closeness. The reaction may be warmer, more personal, more domestic, more affectionate, or more emotionally rich when it fits the NPC. Still keep their personality and boundaries.";
			}
			if (num >= 8)
			{
				return "Relationship depth guidance: very close/high-heart relationship. The NPC can give a richer, more personal, more specific reaction; romance candidates may show shy warmth, teasing, fluster, or emotional impact if the outfit/context supports it.";
			}
			if (num >= 5)
			{
				return "Relationship depth guidance: solid friendship. The NPC can sound more familiar, specific, teasing, or warmly honest, but do not force romance.";
			}
			if (num >= 2)
			{
				return "Relationship depth guidance: early friendship. Keep the reaction natural and character-specific, but a little simpler and less intimate.";
			}
			return "Relationship depth guidance: very low hearts / barely knows the farmer. Keep the reaction brief, casual, guarded, polite, blunt, or curious according to the NPC; do not force intimacy or romance.";
		}

		private static string BuildPrivateCandidateToneRule(OutfitAiContext context)
		{
			if (context == null || !IsPrivateCandidateInterior(context))
			{
				return "";
			}
			return "Private room/home tone rule: the farmer is in this NPC's personal/private space. Let the NPC's profile and heart level decide the tone. Low hearts should stay more guarded or surprised; mid hearts can be familiar or awkward; high hearts can be warmer, richer, teasing, or shy. Do not force blush, stammer, romance, or a specific portrait unless the outfit and relationship naturally support it.";
		}

		private static int GetMinimumLengthTarget(ModConfig config, ActiveAiSettings ai)
		{
			if (config == null)
			{
				return 0;
			}
			int num = Math.Max(0, config.AiMinimumCharacters);
			if (num <= 0)
			{
				return 0;
			}
			int num2 = Math.Clamp(ai?.MaxCharacters ?? config.AiMaxCharacters, 80, 2000);
			int val = Math.Max(40, num2 - 10);
			return Math.Min(num, val);
		}

		public static bool OutfitNameLooksTechnical(string value)
		{
			return DialogueValidator.LooksLikeTechnicalOrOverSpecificOutfitName(value);
		}

		private static string SanitizeThemeContextForPrompt(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string text2 = text;
			text2 = text2.Replace("Outfit theme name:", "Readable outfit theme/reference clue (may mention recognizable references naturally; do not quote technical labels):");
			text2 = text2.Replace("Outfit theme guidance:", "Private outfit theme/reference meaning:");
			text2 = Regex.Replace(text2, "(?i)\\bsummer\\s+indoor\\b", "summer-inspired look in a private inside setting");
			text2 = Regex.Replace(text2, "(?i)\\bver[aã]o\\s+indoor\\b", "summer-inspired look in a private inside setting");
			text2 = Regex.Replace(text2, "(?i)\\bindoor(s)?\\b", "inside/private setting");
			text2 = Regex.Replace(text2, "(?i)\\boutdoor(s)?\\b", "outside setting");
			text2 = Regex.Replace(text2, "(?i)\\bnpc\\s*room\\b", "the NPC's personal room");
			text2 = Regex.Replace(text2, "(?i)\\bnpcroom\\b", "the NPC's personal room");
			text2 = Regex.Replace(text2, "(?i)\\binside\\s+variant\\b", "inside/private setting context");
			return Regex.Replace(text2, "(?i)\\boutside\\s+variant\\b", "outside setting context");
		}

		private static string HumanizeTechnicalLabelForPrompt(string label)
		{
			if (string.IsNullOrWhiteSpace(label))
			{
				return "";
			}
			string input = label.Trim();
			input = Regex.Replace(input, "([a-zà-ÿ])([A-Z])", "$1 $2");
			input = Regex.Replace(input, "[_\\-.]+", " ");
			input = Regex.Replace(input, "(?i)\\bsummer\\s+indoor\\b", "summer-inspired look in a private inside setting");
			input = Regex.Replace(input, "(?i)\\bver[aã]o\\s+indoor\\b", "summer-inspired look in a private inside setting");
			input = Regex.Replace(input, "(?i)\\bindoor(s)?\\b", "inside/private setting");
			input = Regex.Replace(input, "(?i)\\boutdoor(s)?\\b", "outside setting");
			input = Regex.Replace(input, "(?i)\\bnpc\\s*room\\b", "personal room context");
			input = Regex.Replace(input, "(?i)\\bnpcroom\\b", "personal room context");
			return Regex.Replace(input, "\\s{2,}", " ").Trim();
		}

		private static bool IsPrivateCandidateInterior(OutfitAiContext context)
		{
			return context != null && (context.IsNpcRoom || context.IsNpcPersonalLocation);
		}

		private static string BuildSoftPrivateRevealingReactionRule(OutfitAiContext context, string outfitKind)
		{
			int num = ((context != null) ? Math.Max(0, context.RelationshipHearts) : 0);
			string text = ((context != null && context.IsNpcRoom) ? "the NPC's personal room" : "a private/home interior connected to the NPC");
			string text2 = (((outfitKind ?? "").IndexOf("swim", StringComparison.OrdinalIgnoreCase) >= 0 || (outfitKind ?? "").IndexOf("bikini", StringComparison.OrdinalIgnoreCase) >= 0 || (outfitKind ?? "").IndexOf("underwear", StringComparison.OrdinalIgnoreCase) >= 0 || (outfitKind ?? "").IndexOf("intimate", StringComparison.OrdinalIgnoreCase) >= 0) ? "Because swimwear, underwear, or clothing that shows a lot of skin is more intimate/skin-revealing than ordinary clothing, it may create a slightly stronger reaction when the relationship and personality support it." : "Because this is sleepwear/private clothing, it may feel more personal than an ordinary outfit when the relationship and personality support it.");
			string text3 = ((num >= 8) ? "At 8+ hearts, the reaction can be richer, more personal, warmer, shyer, more impressed, or more emotionally loaded if that fits the NPC. A romance candidate may blush, stumble, tease, or become visibly affected, but do not force the same fluster pattern on everyone." : ((num >= 5) ? "At 5-7 hearts, the NPC can be more familiar and may become awkward, amused, gently teasing, surprised, or a little shy depending on personality." : ((num >= 2) ? "At 2-4 hearts, keep it lighter: surprise, curiosity, a careful comment, mild awkwardness, or humor. Do not assume romantic attraction." : "At 0-1 hearts, keep it brief and lower-intimacy: surprise, confusion, politeness, bluntness, or comedy according to the NPC. Do not force blush or romance.")));
			return "Private/revealing outfit guidance: the farmer is in " + text + " wearing " + outfitKind + ". " + text2 + " Let the NPC react according to their own profile first. " + text3 + " This is guidance, not an override: no mandatory blush words, no mandatory stammer, and no forced portrait type. The comment should still acknowledge the unusual/private nature of the outfit if it would be obvious.";
		}

		private static bool IsPrivateRevealingOutfitContext(OutfitAiContext context, out string outfitKind)
		{
			outfitKind = "";
			if (context == null || !IsPrivateCandidateInterior(context))
			{
				return false;
			}
			string allClues = string.Join(" ", context.OutfitName, context.SafeOutfitHint, context.DialogueKey, SanitizeThemeContextForPrompt(context.ThemeContext)).ToLowerInvariant();
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
			{
				return false;
			}
			return allClues.Contains("pajama") || allClues.Contains("pijama") || allClues.Contains("nightgown") || allClues.Contains("camisola") || allClues.Contains("lingerie") || allClues.Contains("underwear") || allClues.Contains("intimate") || allClues.Contains("íntimo") || allClues.Contains("intimo") || allClues.Contains("nightwear") || allClues.Contains("sleepwear") || allClues.Contains("negligee") || allClues.Contains("nightie") || allClues.Contains("robe") || allClues.Contains("roupa de dormir") || allClues.Contains("roupa íntima") || allClues.Contains("roupa intima");
		}

		private static bool LooksLikeSwimwearOrBeachwear(string allClues)
		{
			if (string.IsNullOrWhiteSpace(allClues))
			{
				return false;
			}
			return allClues.Contains("swim") || allClues.Contains("swimsuit") || allClues.Contains("bathing suit") || allClues.Contains("beachwear") || allClues.Contains("bikini") || allClues.Contains("biquíni") || allClues.Contains("biquini") || allClues.Contains("sunga") || allClues.Contains("beach") || allClues.Contains("praia") || allClues.Contains("maiô") || allClues.Contains("maio") || allClues.Contains("banho") || allClues.Contains("roupa de banho");
		}

		private static string BuildPrivateRevealingPromptRule(OutfitAiContext context)
		{
			return "";
		}

		private static string BuildSebastianCustomSoftnessOverride(OutfitAiContext context)
		{
			return "";
		}

		private static string BuildFinalSituationalOverride(OutfitAiContext context)
		{
			if (context == null)
			{
				return "";
			}
			string allClues = string.Join(" ", context.OutfitName, context.SafeOutfitHint, context.DialogueKey, SanitizeThemeContextForPrompt(context.ThemeContext)).ToLowerInvariant();
			bool flag = LooksLikeSleepwearOrIntimate(allClues);
			bool flag2 = LooksLikeSwimwearOrBeachwear(allClues);
			if (!flag && (!flag2 || context.IsBeachOrIsland))
			{
				return "";
			}
			string text = (context.RelationshipStatus ?? "").ToLowerInvariant();
			bool flag3 = context.IsSpouse || text.Equals("spouse", StringComparison.OrdinalIgnoreCase) || text.Equals("dating", StringComparison.OrdinalIgnoreCase) || text.Contains("married") || text.Contains("boyfriend") || text.Contains("girlfriend") || text.Contains("namor");
			string text2 = (flag ? "sleepwear / pajamas / intimate clothing" : "swimwear / bikini / beachwear");
			if (IsPrivateCandidateInterior(context))
			{
				return BuildSoftPrivateRevealingReactionRule(context, text2);
			}
			if (flag3 && !context.IsFarmHouse && !IsPrivateCandidateInterior(context))
			{
				return "Context guidance: the farmer is wearing " + text2 + " in a non-private place where others may see. A romantic partner may show concern, protectiveness, jealousy, awkward humor, or fluster if that fits their personality and heart level, but do not force a single reaction pattern.";
			}
			return "Context guidance: " + text2 + " is not an ordinary everyday outfit in this place. React naturally to the situation — surprise, comedy, concern, bluntness, teasing, or warmth according to the NPC and relationship level. Do not treat it like a normal fashion review.";
		}

		private static string BuildSceneGroundingInstruction(OutfitAiContext context)
		{
			string text = ((context == null) ? "" : StringUtils.FirstNonEmpty(context.DetailedLocationName, context.LocationName));
			string text2 = ((context == null) ? "" : HumanizeTechnicalLabelForPrompt(context.LocationType));
			return "SCENE GROUNDING RULE: do not invent current objects, props, positions, or actions. The NPC profile may mention hobbies, furniture, work, instruments, motorcycles, computers, books, animals, or favorite places, but those are personality background only, not confirmed current scene facts. Only mention a specific object/place/action if it is explicitly in the current location/context or clearly part of the farmer's visible outfit/support data. Confirmed current location is: " + StringUtils.FirstNonEmpty(text, "unknown") + ". Private location type/context: " + StringUtils.FirstNonEmpty(text2, "unknown") + ". Safe natural wording for the CURRENT scene is: here, in this room, at home, inside, or outside. Unsafe as a current fact unless explicitly confirmed: 'in front of my motorcycle', 'by my computer', 'on my bed', 'at the beach', 'in the saloon', 'during band practice', or anything implying the farmer/NPC moved somewhere else. Hypothetical jokes or comparisons are allowed when clearly phrased as imagination (e.g. 'se você aparecesse assim...', 'dá pra imaginar...'), but any place, activity, creature, or theme used in such a comparison must come from THIS NPC's own personality, interests, and world — never a generic Stardew topic (mines, slimes, monsters, the saloon, chickens, crops) that this character would not naturally think about.";
		}

		private static string BuildTechnicalContextLabelInstruction(OutfitAiContext context)
		{
			return "Private labels like indoor, outdoor, inside/outside variant, NPC room, NpcRoom, outfit category, dialogue category, theme guidance, and internal theme keys are only metadata. Never say those labels in the final dialogue. If location matters, translate it into natural in-world wording like here, in your room, at home, outside, by the beach, or at the festival.";
		}

		private static void AppendNoticedChangeContextForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder == null || context == null)
			{
				return;
			}
			string text = (string.IsNullOrWhiteSpace(context.NoticedChangeType) ? "Outfit" : context.NoticedChangeType.Trim());
			builder.AppendLine("Private noticed visual change type: " + text + ". Use it to choose the compliment focus; do not say this technical label.");
			if (!string.IsNullOrWhiteSpace(context.SafeNoticedChangeHint))
			{
				builder.AppendLine("Readable changed item/theme clue: " + context.SafeNoticedChangeHint + ". Use its meaning naturally; if it names a recognizable reference/theme, the NPC may mention it when fitting. Do not recite technical slot/file names.");
			}
			if (context.IsAccessoryChange && !string.IsNullOrWhiteSpace(context.NoticedChangeName) && context.NoticedChangeName.TrimStart().StartsWith("removed ", StringComparison.OrdinalIgnoreCase))
			{
				if (context.NpcWitnessedPreviousAccessory)
				{
					builder.AppendLine("Accessory removal rule: the changed accessory clue describes something the farmer just REMOVED. It is no longer being worn. This NPC has seen the farmer in this look before, so they MAY react to the absence/change, e.g. noticing that the previous wings/cape/accessory are gone, that the outfit looks less chaotic now, or comparing the current look without it to the earlier combo.");
				}
				else
				{
					builder.AppendLine("Accessory removal rule: the changed accessory clue describes something the farmer just REMOVED, so it is no longer being worn. IMPORTANT: this NPC never saw the farmer wearing that accessory, so they have NO memory of it. Do NOT reference 'the accessory from before', 'that cute thing you had', or any past version of the look, and do not imply you remember a previous combination. React only to how the farmer looks RIGHT NOW, as if seeing them for the first time today.");
				}
			}
			if (context.IsAccessoryChange && context.NpcWitnessedPreviousAccessory && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
			{
				builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this accessory reaction, compare the changed accessory with this existing outfit/theme when it creates a funny, strange, cute, ugly, dramatic, or impossible combination. Do not ignore either side of the combo. If the accessory was removed, compare the current outfit-without-that-accessory to the previous combination.");
			}
			else if (context.IsAccessoryChange && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
			{
				builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this accessory reaction, you may comment on how the changed accessory works with this existing outfit/theme when the combination is funny, strange, cute, ugly, or dramatic. Do not reference any previous/removed version you did not witness.");
			}
			if (context.IsHatChange && !string.IsNullOrWhiteSpace(context.SafeOutfitHint))
			{
				builder.AppendLine("Current saved outfit/theme clue still being worn: " + context.SafeOutfitHint + ". For this headwear reaction, you may compare the head item with the existing outfit/theme when the combination is funny, strange, cute, ugly, dramatic, or mismatched.");
			}
			if (context.IsOutfitChange)
			{
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.SavedOutfitFocusGuidance ?? PromptStyleService.FallbackSavedOutfitFocusGuidance, context);
			}
			else if (context.IsHairChange)
			{
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.HairFocusGuidance ?? PromptStyleService.FallbackHairFocusGuidance, context);
			}
			else if (context.IsHatChange)
			{
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.HatFocusGuidance ?? PromptStyleService.FallbackHatFocusGuidance, context);
			}
			else if (context.IsAccessoryChange)
			{
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.AccessoryFocusGuidance ?? PromptStyleService.FallbackAccessoryFocusGuidance, context);
			}
		}

		private static void AppendFashionSenseVisualSummaryForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder != null && context != null && context.HasFashionSenseVisualSummary)
			{
				Dictionary<string, string> extraTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["VisualSummary"] = CollapseForPrompt(context.FashionSenseVisualSummary, 1300) };
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.FashionSenseVisualSupportRule ?? PromptStyleService.FallbackFashionSenseVisualSupportRule, context, extraTokens);
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.FashionSenseVisualSeparationRule ?? PromptStyleService.FallbackFashionSenseVisualSeparationRule, context, extraTokens);
			}
		}

		private static void AppendSpecialItemReactionForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder != null && context != null && context.HasSpecialItemReactionContext)
			{
				Dictionary<string, string> extraTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["SpecialItemData"] = CollapseForPrompt(context.SpecialItemReactionContext, 1200) };
				if (context.SpecialItemWasJustRemoved)
				{
					CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.SpecialItemRemovedRule ?? PromptStyleService.FallbackSpecialItemRemovedRule, context, extraTokens);
				}
				else
				{
					CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.SpecialItemVisibleRule ?? PromptStyleService.FallbackSpecialItemVisibleRule, context, extraTokens);
				}
				if (context.HasSpecialItemMemoryHint)
				{
					Dictionary<string, string> extraTokens2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["ItemMemory"] = context.SpecialItemMemoryHint };
					CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.SpecialItemMemoryRule ?? PromptStyleService.FallbackSpecialItemMemoryRule, context, extraTokens2);
				}
				if (!string.IsNullOrWhiteSpace(context.VanillaPantsMemoryHint))
				{
					builder.AppendLine("Pants memory: " + context.VanillaPantsMemoryHint);
				}
			}
		}

		private static void AppendSpecialHatReactionForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder != null && context != null && context.HasSpecialHatReactionContext)
			{
				Dictionary<string, string> extraTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["SpecialHatData"] = CollapseForPrompt(context.SpecialHatReactionContext, 1400) };
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.SpecialVanillaHatRule ?? PromptStyleService.FallbackSpecialVanillaHatRule, context, extraTokens);
			}
		}

		private static void AppendVanillaHatMemoryForPrompt(StringBuilder builder, OutfitAiContext context, PromptStyleService promptStyle)
		{
			if (builder != null && context != null && context.HasVanillaHatMemoryHint)
			{
				Dictionary<string, string> extraTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["HatMemory"] = context.VanillaHatMemoryHint };
				CharacterPromptBuilder.AppendPromptBlock(builder, promptStyle?.VanillaHatMemoryRule ?? PromptStyleService.FallbackVanillaHatMemoryRule, context, extraTokens);
			}
		}

		private static string BuildNaturalContextHint(OutfitAiContext context)
		{
			if (context == null)
			{
				return "";
			}
			string allClues = string.Join(" ", context.OutfitName, context.SafeOutfitHint, context.DialogueKey, SanitizeThemeContextForPrompt(context.ThemeContext)).ToLowerInvariant();
			bool flag = LooksLikeSwimwearOrBeachwear(allClues);
			if (LooksLikeSleepwearOrIntimate(allClues))
			{
				if (context.IsSpouse && context.IsFarmHouse)
				{
					return "Context hint: the clothing is pajamas, sleepwear, underwear, or intimate clothing at home with spouse. React naturally as a couple at home, according to the NPC's personality: relaxed, affectionate, teasing, shy, practical, or amused as fits.";
				}
				if (IsPrivateCandidateInterior(context))
				{
					return BuildSoftPrivateRevealingReactionRule(context, "sleepwear/nightwear/intimate clothing");
				}
				if (context.IsIndoors)
				{
					return "Context hint: the clothing looks like pajamas, sleepwear, underwear, or intimate clothing indoors. It may feel surprising, funny, awkward, or personal depending on the NPC and relationship level. Do not treat it as a normal fashion review.";
				}
				return "Context hint: the clothing looks like pajamas, sleepwear, underwear, or intimate clothing in a public/outdoor area. React naturally to the incongruity — surprise, teasing, concern, comedy, or bluntness according to the NPC.";
			}
			if (flag && !context.IsBeachOrIsland)
			{
				if (context.IsSpouse && context.IsFarmHouse)
				{
					return "Natural context hint: the outfit is swimwear or beachwear at home with spouse. React as their personality supports: relaxed, warm, teasing, shy, flirty, practical, or amused.";
				}
				if (IsPrivateCandidateInterior(context))
				{
					return BuildSoftPrivateRevealingReactionRule(context, "swimwear/beachwear");
				}
				if (context.IsIndoors)
				{
					return "Natural context hint: the outfit is swimwear or beachwear indoors, away from the beach. React to that situation naturally: puzzled, amused, concerned, teasing, or flustered depending on the NPC and relationship level.";
				}
				return "Natural context hint: the outfit seems like swimwear or beachwear, but the farmer is not at a beach, pool, or island. React naturally to the mismatch instead of treating it like a normal outfit.";
			}
			return "";
		}

		private static string BuildLocalSeasonAuthorityInstruction(OutfitAiContext context)
		{
			if (context == null)
			{
				return "";
			}
			string value = NormalizeSeasonKey(context.Season);
			string value2 = FormatSeasonForPrompt(context.Season, context.TargetLanguage);
			string value3 = DescribeInferredOutfitSeasonForPrompt(context);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("AUTHORITATIVE SEASON RULE: the actual current in-game season is ").Append(value2).Append(". ");
			stringBuilder.Append("Outfit/theme clues may suggest a different season, but those clues describe the outfit style, not the date. ");
			stringBuilder.Append("Do not replace the actual season with another one. ");
			if (!string.IsNullOrWhiteSpace(value3))
			{
				stringBuilder.Append("This outfit appears to have ").Append(value3).Append(" vibes. ");
				stringBuilder.Append("If that clashes with the actual season, mention the contrast using the actual season above.");
			}
			else if (!string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.Append("Only mention another season if the outfit itself clearly has that seasonal theme.");
			}
			return stringBuilder.ToString();
		}

		private static string FormatSeasonForPrompt(string season, string targetLanguage)
		{
			string text = NormalizeSeasonKey(season);
			bool flag = !string.IsNullOrWhiteSpace(targetLanguage) && targetLanguage.IndexOf("Portuguese", StringComparison.OrdinalIgnoreCase) >= 0;
			if (1 == 0)
			{
			}
			string result = text switch
			{
				"spring" => flag ? "spring / primavera" : "spring", 
				"summer" => flag ? "summer / verão" : "summer", 
				"fall" => flag ? "fall / autumn / outono" : "fall / autumn", 
				"winter" => flag ? "winter / inverno" : "winter", 
				_ => string.IsNullOrWhiteSpace(season) ? "unknown" : season, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private static string NormalizeSeasonKey(string season)
		{
			if (string.IsNullOrWhiteSpace(season))
			{
				return "";
			}
			string text = season.Trim().ToLowerInvariant();
			if (text.Contains("spring") || text.Contains("primavera"))
			{
				return "spring";
			}
			if (text.Contains("summer") || text.Contains("verão") || text.Contains("verao"))
			{
				return "summer";
			}
			if (text.Contains("fall") || text.Contains("autumn") || text.Contains("outono"))
			{
				return "fall";
			}
			if (text.Contains("winter") || text.Contains("inverno"))
			{
				return "winter";
			}
			return text;
		}

		private static string DescribeInferredOutfitSeasonForPrompt(OutfitAiContext context)
		{
			HashSet<string> hashSet = InferSeasonKeysFromOutfitClues(context);
			if (hashSet.Count <= 0)
			{
				return "";
			}
			List<string> list = new List<string>();
			foreach (string item2 in hashSet)
			{
				List<string> list2 = list;
				if (1 == 0)
				{
				}
				string item = item2 switch
				{
					"spring" => "spring", 
					"summer" => "summer/beach", 
					"fall" => "fall/autumn", 
					"winter" => "winter/Christmas", 
					_ => item2, 
				};
				if (1 == 0)
				{
				}
				list2.Add(item);
			}
			return string.Join(" and ", list);
		}

		private static HashSet<string> InferSeasonKeysFromOutfitClues(OutfitAiContext context)
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (context == null)
			{
				return hashSet;
			}
			string text = string.Join(" ", context.OutfitName, context.SafeOutfitHint, context.DialogueKey, SanitizeThemeContextForPrompt(context.ThemeContext), SanitizeThemeContextForPrompt(context.ThemePriorityInstruction)).ToLowerInvariant();
			if (text.Contains("xmas") || text.Contains("christmas") || text.Contains("natal") || text.Contains("noel") || text.Contains("winter") || text.Contains("snow") || text.Contains("neve") || text.Contains("inverno"))
			{
				hashSet.Add("winter");
			}
			if (text.Contains("swim") || text.Contains("bikini") || text.Contains("beach") || text.Contains("praia") || text.Contains("maiô") || text.Contains("maio") || text.Contains("summer") || text.Contains("verão") || text.Contains("verao"))
			{
				hashSet.Add("summer");
			}
			if (text.Contains("spring") || text.Contains("primavera") || text.Contains("flower dance") || text.Contains("flowerdance"))
			{
				hashSet.Add("spring");
			}
			if (text.Contains("fall") || text.Contains("autumn") || text.Contains("outono") || text.Contains("spirit") || text.Contains("halloween"))
			{
				hashSet.Add("fall");
			}
			return hashSet;
		}

		private static string ValidateLocalSeasonReferences(string lower, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(lower) || context == null)
			{
				return null;
			}
			string normalizedText = " " + Regex.Replace(lower, "[^\\p{L}\\p{N}]+", " ").Trim() + " ";
			string text = NormalizeSeasonKey(context.Season);
			HashSet<string> hashSet = InferSeasonKeysFromOutfitClues(context);
			if (!string.IsNullOrWhiteSpace(text))
			{
				hashSet.Add(text);
			}
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
			dictionary["spring"] = new string[2] { " spring ", " primavera " };
			dictionary["summer"] = new string[3] { " summer ", " verão ", " verao " };
			dictionary["fall"] = new string[3] { " fall ", " autumn ", " outono " };
			dictionary["winter"] = new string[2] { " winter ", " inverno " };
			Dictionary<string, string[]> dictionary2 = dictionary;
			foreach (KeyValuePair<string, string[]> item in dictionary2)
			{
				if (!item.Value.Any((string alias) => normalizedText.Contains(alias)) || hashSet.Contains(item.Key))
				{
					continue;
				}
				return "local response confused the actual season with " + item.Key;
			}
			return null;
		}

		private static string BuildSeasonalAwarenessInstruction(OutfitAiContext context)
		{
			if (context == null)
			{
				return "";
			}
			string text = string.Join(" ", context.OutfitName, context.SafeOutfitHint, context.DialogueKey, SanitizeThemeContextForPrompt(context.ThemeContext)).ToLowerInvariant();
			string text2 = (context.Season ?? "").ToLowerInvariant();
			if ((text.Contains("xmas") || text.Contains("christmas") || text.Contains("natal") || text.Contains("noel") || text.Contains("winter") || text.Contains("snow") || text.Contains("neve") || text.Contains("inverno")) && !text2.Contains("winter"))
			{
				return "Seasonal awareness: the outfit clue/theme suggests Christmas, snow, or winter, but the current season is " + context.Season + ". React to that mismatch in a human way if it fits the NPC: gentle teasing, surprise, amusement, curiosity, or finding it charmingly out of place. Do not force a line saying it also suits or works for the current season.";
			}
			if (LooksLikeSwimwearOrBeachwear(text) && (text2.Contains("winter") || (context.Weather ?? "").IndexOf("snow", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				return "Seasonal awareness: the outfit clue/theme suggests swimwear or beachwear, but the current season/weather is cold or snowy. Mention that contrast naturally if it fits the character. Do not use technical labels like indoor or theme guidance.";
			}
			return "Season is flavor, not a requirement: only bring up the season/weather if it genuinely connects to what is worn (e.g. a coat on a chilly rainy spring day, a sundress in summer). NEVER force a seasonal tie-in, and NEVER add a closing line that says the look also suits, fits, or works for the current season just to wrap up — if there is no real connection, end without mentioning the season at all. Use location, weather, and time the same way: only when they add something real. Never repeat technical labels like indoor, outdoor, NPC room, dialogue category, or theme guidance.";
		}

		private static string BuildLanguageExampleLocalLine(string targetLanguage)
		{
			return "<spoken outfit reaction in the current game language; no portrait commands inside the text>";
		}

		private static string ValidateLocalGeneratedDialogueText(string text, OutfitAiContext context, CharacterAiProfile profile, ModConfig config)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "empty local dialogue";
			}
			string text2 = DialogueValidator.StripDialogueMarkup(text);
			string lower = " " + text2.ToLowerInvariant() + " ";
			int num = Math.Max(40, GetMinimumLengthTarget(config, null));
			int num2 = Math.Max((num >= 300) ? 70 : ((num >= 180) ? 45 : 25), (int)Math.Round((double)num * 0.18));
			int num3 = Math.Max(40, num - num2);
			if (text2.Length < num3)
			{
				return "local dialogue was too short for configured minimum (" + text2.Length + "/" + num + " visible characters, retry threshold " + num3 + ")";
			}
			if (LooksLikeThirdPersonNarrationForNpc(lower, context?.NpcDisplayName ?? context?.NpcName))
			{
				return "local response was narration instead of spoken dialogue";
			}
			if (LooksLikeGenericCutesyLocalLine(lower, context))
			{
				return "local response sounded like generic cutesy/poetic praise instead of the NPC";
			}
			string text3 = DialogueValidator.ValidateRecognizableThemeSpecificity(text, context);
			if (!string.IsNullOrWhiteSpace(text3))
			{
				return "local " + text3;
			}
			string text4 = DialogueValidator.ValidateAccessoryOutfitCombinationSpecificity(text, context);
			if (!string.IsNullOrWhiteSpace(text4))
			{
				return "local " + text4;
			}
			if (LooksLikeUnrelatedLocalLine(lower))
			{
				return "local response introduced unrelated generic details";
			}
			string text5 = ValidateLocalSeasonReferences(lower, context);
			if (!string.IsNullOrWhiteSpace(text5))
			{
				return text5;
			}
			return null;
		}

		private static bool LooksLikeThirdPersonNarrationForNpc(string lower, string npcDisplayName)
		{
			if (string.IsNullOrWhiteSpace(lower))
			{
				return false;
			}
			string text = (npcDisplayName ?? "").Trim().ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (lower.Contains(" " + text + " is ") || lower.Contains(" " + text + " looks ") || lower.Contains(" " + text + " smiles ") || lower.Contains(" " + text + " blushes "))
				{
					return true;
				}
				if (lower.Contains(" " + text + " está ") || lower.Contains(" " + text + " esta ") || lower.Contains(" " + text + " olha ") || lower.Contains(" " + text + " sorri ") || lower.Contains(" " + text + " fica "))
				{
					return true;
				}
			}
			return lower.Contains("contexto atual") || lower.Contains("current context") || lower.Contains("tonalidade") || lower.Contains("tone:") || lower.Contains("portrait:") || lower.Contains("**portrait**") || lower.Contains("stage direction") || lower.Contains("scene description");
		}

		private static bool LooksLikeGenericCutesyLocalLine(string lower, OutfitAiContext context)
		{
			return false;
		}

		private static bool LooksLikeUnrelatedLocalLine(string lower)
		{
			if (string.IsNullOrWhiteSpace(lower))
			{
				return false;
			}
			string[] array = new string[10] { " crafting wood ", " chopping wood ", " perfect for mining ", " fighting monsters ", " watering crops ", " cortar madeira ", " minerar ", " lutar contra monstros ", " regar plantações ", " regar plantacoes " };
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (lower.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static string BuildCacheKey(OutfitAiContext context, ModConfig config, string prompt, ActiveAiSettings ai)
		{
			return string.Join("|", ai.Provider ?? "", ai.Model ?? "", ai.TemperaturePercent.ToString(), ai.MaxCharacters.ToString(), context?.VisionImage?.Hash ?? "", context?.FashionSenseVisualSummary ?? "", context?.NoticedChangeType ?? "", context?.NoticedChangeName ?? "", context?.SafeNoticedChangeHint ?? "", context.NpcName ?? "", context.DialogueKey ?? "", context.OutfitName ?? "", context.LocationName ?? "", context.Season ?? "", context.Weather ?? "", context.RelationshipStatus ?? "", context.RelationshipHearts.ToString(), prompt.GetHashCode().ToString());
		}

		private static string CollapseForPrompt(string text, int maxLength)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			text = Regex.Replace(text, "\\s+", " ").Trim();
			if (text.Length <= maxLength)
			{
				return text;
			}
			return text.Substring(0, Math.Max(0, maxLength)).Trim() + "...";
		}

		private static string TrimForLog(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			text = Regex.Replace(text, "\\s+", " ").Trim();
			return (text.Length <= 500) ? text : (text.Substring(0, 500) + "...");
		}
	}
	internal sealed class OutfitMemoryService
	{
		private const string SaveKey = "OutfitMemories";

		private const int CurrentMemorySchemaVersion = 2;

		private readonly IModHelper helper;

		private readonly IMonitor monitor;

		private Dictionary<string, Dictionary<string, OutfitMemoryEntry>> memories = new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);

		private bool dirty = false;

		public OutfitMemoryService(IModHelper helper, IMonitor monitor)
		{
			this.helper = helper;
			this.monitor = monitor;
		}

		public void Load()
		{
			try
			{
				OutfitMemoryData outfitMemoryData = helper.Data.ReadSaveData<OutfitMemoryData>("OutfitMemories");
				if (outfitMemoryData == null)
				{
					memories = new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
					dirty = false;
					if (ModEntry.DebugLog)
					{
						monitor.Log("[OUTFIT MEMORY] No saved outfit memories found.", (LogLevel)2);
					}
				}
				else if (outfitMemoryData.Version < 2)
				{
					memories = new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
					dirty = true;
					if (ModEntry.DebugLog)
					{
						monitor.Log($"[OUTFIT MEMORY] Old memory schema v{outfitMemoryData.Version} detected; clearing outfit memories to prevent corrupted accessory/outfit associations.", (LogLevel)2);
					}
				}
				else
				{
					memories = outfitMemoryData.Memories ?? new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
					dirty = false;
					if (ModEntry.DebugLog)
					{
						monitor.Log($"[OUTFIT MEMORY] Loaded memories for {memories.Count} NPC(s).", (LogLevel)2);
					}
				}
			}
			catch (Exception ex)
			{
				monitor.Log("[OUTFIT MEMORY] Failed to load memories: " + ex.Message, (LogLevel)3);
				memories = new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
			}
		}

		public void Save()
		{
			if (!dirty)
			{
				return;
			}
			try
			{
				helper.Data.WriteSaveData<OutfitMemoryData>("OutfitMemories", new OutfitMemoryData
				{
					Version = 2,
					Memories = memories
				});
				dirty = false;
				if (ModEntry.DebugLog)
				{
					monitor.Log("[OUTFIT MEMORY] Memories saved.", (LogLevel)2);
				}
			}
			catch (Exception ex)
			{
				monitor.Log("[OUTFIT MEMORY] Failed to save memories: " + ex.Message, (LogLevel)3);
			}
		}

		public OutfitMemoryComparison GetMemory(string npcName, string outfitId, OutfitComponents current)
		{
			if (string.IsNullOrWhiteSpace(npcName) || string.IsNullOrWhiteSpace(outfitId))
			{
				return null;
			}
			if (!memories.TryGetValue(npcName, out var value))
			{
				return null;
			}
			if (!value.TryGetValue(outfitId, out var value2))
			{
				return null;
			}
			OutfitMemoryEntry outfitMemoryEntry = value2;
			if (outfitMemoryEntry.Components == null)
			{
				OutfitComponents outfitComponents = (outfitMemoryEntry.Components = new OutfitComponents());
			}
			outfitMemoryEntry = value2;
			if (outfitMemoryEntry.AccessoryHistory == null)
			{
				Dictionary<string, int> dictionary = (outfitMemoryEntry.AccessoryHistory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
			}
			if (current == null)
			{
				current = new OutfitComponents();
			}
			List<OutfitComponentChange> list = new List<OutfitComponentChange>();
			CompareComponent(list, "Hat", value2.Components.Hat, current.Hat);
			CompareComponent(list, "Hair", value2.Components.Hair, current.Hair);
			CompareComponent(list, "Shirt", value2.Components.Shirt, current.Shirt);
			CompareComponent(list, "Pants", value2.Components.Pants, current.Pants);
			CompareComponent(list, "Sleeves", value2.Components.Sleeves, current.Sleeves);
			CompareComponent(list, "Accessory", value2.Components.Accessory, current.Accessory);
			string text = NormalizeAccessoryMemoryKey(current?.Accessory);
			int value3 = 0;
			if (!string.IsNullOrWhiteSpace(text) && value2.AccessoryHistory != null)
			{
				value2.AccessoryHistory.TryGetValue(text, out value3);
			}
			return new OutfitMemoryComparison
			{
				FirstSeenSeason = value2.FirstSeenSeason,
				FirstSeenDay = value2.FirstSeenDay,
				FirstSeenYear = value2.FirstSeenYear,
				TimesSeenBefore = value2.TimesSeen,
				ComponentChanges = list,
				LastRecordedAccessory = (value2.Components?.Accessory ?? ""),
				CurrentAccessory = (current?.Accessory ?? ""),
				CurrentAccessorySeenBefore = value3
			};
		}

		public void RecordMemory(string npcName, string outfitId, OutfitComponents components, string season, int day, int year)
		{
			if (string.IsNullOrWhiteSpace(npcName) || string.IsNullOrWhiteSpace(outfitId))
			{
				return;
			}
			if (!memories.TryGetValue(npcName, out var value))
			{
				value = new Dictionary<string, OutfitMemoryEntry>(StringComparer.OrdinalIgnoreCase);
				memories[npcName] = value;
			}
			if (value.TryGetValue(outfitId, out var value2))
			{
				if (components == null)
				{
					components = new OutfitComponents();
				}
				RecordAccessoryHistory(value2, value2.Components?.Accessory);
				RecordAccessoryHistory(value2, components.Accessory);
				value2.Components = components;
				value2.TimesSeen++;
			}
			else
			{
				if (components == null)
				{
					components = new OutfitComponents();
				}
				OutfitMemoryEntry outfitMemoryEntry = new OutfitMemoryEntry
				{
					OutfitId = outfitId,
					FirstSeenSeason = season,
					FirstSeenDay = day,
					FirstSeenYear = year,
					TimesSeen = 1,
					Components = components
				};
				RecordAccessoryHistory(outfitMemoryEntry, components.Accessory);
				value[outfitId] = outfitMemoryEntry;
			}
			dirty = true;
			if (ModEntry.DebugLog)
			{
				monitor.Log($"[OUTFIT MEMORY] Recorded memory of '{outfitId}' for {npcName}.", (LogLevel)2);
			}
		}

		public string BuildMemoryContextHint(OutfitMemoryComparison memory, string targetLanguage)
		{
			if (memory == null)
			{
				return null;
			}
			bool flag = string.Equals(targetLanguage, "pt", StringComparison.OrdinalIgnoreCase) || string.Equals(targetLanguage, "pt-BR", StringComparison.OrdinalIgnoreCase);
			string value = FormatFirstSeen(memory, flag);
			int timesSeenBefore = memory.TimesSeenBefore;
			List<OutfitComponentChange> componentChanges = memory.ComponentChanges;
			string text = BuildAccessoryMemoryNote(memory, flag);
			if (flag)
			{
				string value2 = ((timesSeenBefore == 1) ? "você já usou esse conjunto antes (uma vez)" : $"você já usou esse conjunto antes ({timesSeenBefore} vezes)");
				if (componentChanges.Count == 0)
				{
					return $"MEMÓRIA DO PERSONAGEM: {value2}, da primeira vez foi em {value}. " + "O conjunto está idêntico ao que você viu antes — exatamente as mesmas peças. Reaja com carinho e reconhecimento: mencione que lembra desse look, que fica feliz de ver novamente, que é um look favorito, etc. " + text + "NÃO reaja como se fosse a primeira vez.";
				}
				string value3 = string.Join(", ", componentChanges.Select((OutfitComponentChange c) => $"{TranslateComponentPt(c.ComponentName)} mudou de '{BuildSafeHint(c.OldValue)}' para '{BuildSafeHint(c.NewValue)}'"));
				return $"MEMÓRIA DO PERSONAGEM: {value2}, da primeira vez foi em {value}. O conjunto é basicamente o mesmo, mas algumas peças mudaram: {value3}. " + "Reconheça o look que já conhece e comente naturalmente sobre a(s) peça(s) diferente(s), como se tivesse notado a mudança agora. A reação pode ser curiosa, engraçada, estranhada, implicante, carinhosa ou dramática conforme a personalidade do NPC e o tema do visual. Se um acessório foi removido, trocado ou adicionado, pode comparar com como o conjunto estava antes. " + text + "NÃO reaja como se fosse a primeira vez.";
			}
			string value4 = ((timesSeenBefore == 1) ? "you have worn this outfit before (once)" : $"you have worn this outfit before ({timesSeenBefore} times)");
			if (componentChanges.Count == 0)
			{
				return $"CHARACTER MEMORY: {value4}, first worn {value}. " + "The outfit is identical to the one seen before — every piece is the same. React with warmth and recognition: mention remembering this look, being happy to see it again, it being a favourite, etc. " + text + "Do NOT react as if seeing it for the first time.";
			}
			string value5 = string.Join(", ", componentChanges.Select((OutfitComponentChange c) => $"{c.ComponentName} changed from '{BuildSafeHint(c.OldValue)}' to '{BuildSafeHint(c.NewValue)}'"));
			return $"CHARACTER MEMORY: {value4}, first worn {value}. The outfit is mostly the same but some pieces changed: {value5}. " + "Recognise the familiar look and naturally comment on the changed piece(s) as if you just noticed the difference. The reaction may be curious, funny, weirded-out, teasing, warm, dramatic, or practical depending on the NPC personality and outfit theme. If an accessory was removed, swapped, or added, you may compare it to how the outfit looked before. " + text + "Do NOT react as if seeing it for the first time.";
		}

		private static void RecordAccessoryHistory(OutfitMemoryEntry entry, string accessory)
		{
			if (entry == null)
			{
				return;
			}
			string text = NormalizeAccessoryMemoryKey(accessory);
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (entry.AccessoryHistory == null)
				{
					Dictionary<string, int> dictionary = (entry.AccessoryHistory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
				}
				entry.AccessoryHistory.TryGetValue(text, out var value);
				entry.AccessoryHistory[text] = value + 1;
			}
		}

		private static string BuildAccessoryMemoryNote(OutfitMemoryComparison memory, bool isPt)
		{
			if (memory == null || memory.CurrentAccessorySeenBefore <= 0 || string.IsNullOrWhiteSpace(memory.CurrentAccessory))
			{
				return "";
			}
			string value = BuildSafeHint(memory.CurrentAccessory);
			if (isPt)
			{
				string value2 = ((memory.CurrentAccessorySeenBefore == 1) ? "uma vez" : (memory.CurrentAccessorySeenBefore + " vezes"));
				return $"Esse acessório atual ('{value}') também já apareceu com esse mesmo conjunto antes ({value2}); se ele tinha sido removido e agora voltou, reconheça que ele voltou/foi colocado de novo em vez de tratar como primeira vez. ";
			}
			string value3 = ((memory.CurrentAccessorySeenBefore == 1) ? "once" : (memory.CurrentAccessorySeenBefore + " times"));
			return $"This current accessory ('{value}') has also appeared with this same outfit before ({value3}); if it had been removed and is now back, recognise that it returned/was put back on instead of treating it like the first time. ";
		}

		private static string NormalizeAccessoryMemoryKey(string accessory)
		{
			if (string.IsNullOrWhiteSpace(accessory))
			{
				return "";
			}
			string[] value = (from part in accessory.Split(new string[1] { " + " }, StringSplitOptions.RemoveEmptyEntries)
				select part.Trim() into part
				where !string.IsNullOrWhiteSpace(part) && !IsIgnoredMakeupAccessoryValue(part)
				select part).ToArray();
			return string.Join(" + ", value);
		}

		private static bool IsIgnoredMakeupAccessoryValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string text = " " + value.ToLowerInvariant().Replace('_', ' ').Replace('-', ' ')
				.Replace('.', ' ') + " ";
			return text.Contains(" makeup ") || text.Contains(" maquiagem ") || text.Contains(" blush ") || text.Contains(" lipstick ") || text.Contains(" batom ") || text.Contains(" eyeshadow ") || text.Contains(" eye shadow ") || text.Contains(" sombra ") || text.Contains(" eyeliner ") || text.Contains(" delineador ") || text.Contains(" rimel ") || text.Contains(" rímel ");
		}

		private static void CompareComponent(List<OutfitComponentChange> changes, string name, string oldVal, string newVal)
		{
			oldVal = oldVal ?? "";
			newVal = newVal ?? "";
			if (string.Equals(name, "Accessory", StringComparison.OrdinalIgnoreCase))
			{
				oldVal = NormalizeAccessoryMemoryKey(oldVal);
				newVal = NormalizeAccessoryMemoryKey(newVal);
			}
			if (!string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase))
			{
				changes.Add(new OutfitComponentChange
				{
					ComponentName = name,
					OldValue = oldVal,
					NewValue = newVal
				});
			}
		}

		private static string FormatFirstSeen(OutfitMemoryComparison m, bool isPt)
		{
			string value = (isPt ? TranslateSeasonPt(m.FirstSeenSeason) : m.FirstSeenSeason);
			if (!isPt)
			{
				return $"{value} {m.FirstSeenDay}, Year {m.FirstSeenYear}";
			}
			return $"{value}, dia {m.FirstSeenDay}, ano {m.FirstSeenYear}";
		}

		private static string TranslateSeasonPt(string season)
		{
			string text = season?.ToLowerInvariant();
			if (1 == 0)
			{
			}
			string result = text switch
			{
				"spring" => "primavera", 
				"summer" => "verão", 
				"fall" => "outono", 
				"winter" => "inverno", 
				_ => season ?? "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private static string TranslateComponentPt(string component)
		{
			string text = component?.ToLowerInvariant();
			if (1 == 0)
			{
			}
			string result = text switch
			{
				"hat" => "chapéu", 
				"hair" => "cabelo", 
				"shirt" => "blusa", 
				"pants" => "calça", 
				"sleeves" => "mangas", 
				"accessory" => "acessório", 
				_ => component ?? "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		private static string BuildSafeHint(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return "none";
			}
			string input = (id.Contains('.') ? id.Substring(id.LastIndexOf('.') + 1) : id);
			input = Regex.Replace(input, "(?<=[a-z])(?=[A-Z])", " ");
			input = Regex.Replace(input, "\\s*\\d+\\s*", " ").Trim();
			return string.IsNullOrWhiteSpace(input) ? "unknown" : input;
		}
	}
	internal sealed class OutfitMemoryData
	{
		[JsonPropertyName("Version")]
		public int Version { get; set; } = 2;

		[JsonPropertyName("Memories")]
		public Dictionary<string, Dictionary<string, OutfitMemoryEntry>> Memories { get; set; } = new Dictionary<string, Dictionary<string, OutfitMemoryEntry>>(StringComparer.OrdinalIgnoreCase);
	}
	internal sealed class OutfitMemoryEntry
	{
		[JsonPropertyName("OutfitId")]
		public string OutfitId { get; set; } = "";

		[JsonPropertyName("FirstSeenSeason")]
		public string FirstSeenSeason { get; set; } = "";

		[JsonPropertyName("FirstSeenDay")]
		public int FirstSeenDay { get; set; }

		[JsonPropertyName("FirstSeenYear")]
		public int FirstSeenYear { get; set; }

		[JsonPropertyName("TimesSeen")]
		public int TimesSeen { get; set; } = 1;

		[JsonPropertyName("Components")]
		public OutfitComponents Components { get; set; } = new OutfitComponents();

		[JsonPropertyName("AccessoryHistory")]
		public Dictionary<string, int> AccessoryHistory { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	}
	internal sealed class OutfitComponents
	{
		[JsonPropertyName("Hat")]
		public string Hat { get; set; } = "";

		[JsonPropertyName("Hair")]
		public string Hair { get; set; } = "";

		[JsonPropertyName("Shirt")]
		public string Shirt { get; set; } = "";

		[JsonPropertyName("Pants")]
		public string Pants { get; set; } = "";

		[JsonPropertyName("Sleeves")]
		public string Sleeves { get; set; } = "";

		[JsonPropertyName("Accessory")]
		public string Accessory { get; set; } = "";
	}
	internal sealed class OutfitMemoryComparison
	{
		public string FirstSeenSeason { get; set; } = "";

		public int FirstSeenDay { get; set; }

		public int FirstSeenYear { get; set; }

		public int TimesSeenBefore { get; set; }

		public List<OutfitComponentChange> ComponentChanges { get; set; } = new List<OutfitComponentChange>();

		public string LastRecordedAccessory { get; set; } = "";

		public string CurrentAccessory { get; set; } = "";

		public int CurrentAccessorySeenBefore { get; set; }
	}
	internal sealed class OutfitComponentChange
	{
		public string ComponentName { get; set; } = "";

		public string OldValue { get; set; } = "";

		public string NewValue { get; set; } = "";
	}
	internal sealed class OutfitReplyConversationHistory
	{
		private readonly Dictionary<string, List<(string Speaker, string Text)>> conversations = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);

		public void Reset(string npcName)
		{
			if (!string.IsNullOrWhiteSpace(npcName))
			{
				conversations.Remove(npcName);
			}
		}

		public void Start(string npcName, string npcOpeningLine)
		{
			if (!string.IsNullOrWhiteSpace(npcName))
			{
				List<(string, string)> list = new List<(string, string)>();
				if (!string.IsNullOrWhiteSpace(npcOpeningLine))
				{
					list.Add(("NPC", npcOpeningLine));
				}
				conversations[npcName] = list;
			}
		}

		public void Append(string npcName, string speaker, string text)
		{
			if (!string.IsNullOrWhiteSpace(npcName) && !string.IsNullOrWhiteSpace(text))
			{
				if (!conversations.TryGetValue(npcName, out List<(string, string)> value))
				{
					value = new List<(string, string)>();
					conversations[npcName] = value;
				}
				value.Add((speaker, text));
			}
		}

		public string BuildTranscript(string npcName, int maxChars = 2500)
		{
			if (string.IsNullOrWhiteSpace(npcName) || !conversations.TryGetValue(npcName, out List<(string, string)> value) || value.Count == 0)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var item3 in value)
			{
				string item = item3.Item1;
				string item2 = item3.Item2;
				string value2 = ((item == "NPC") ? "NPC" : "Farmer");
				stringBuilder.Append(value2).Append(": ").Append(item2.Trim())
					.Append('\n');
			}
			string text = stringBuilder.ToString().Trim();
			if (text.Length > maxChars)
			{
				text = "...(earlier conversation trimmed)...\n" + text.Substring(text.Length - maxChars);
			}
			return text;
		}
	}
	public sealed class OutfitVisionImage
	{
		public string MimeType { get; set; } = "image/png";

		public string Base64Data { get; set; } = "";

		public string Hash { get; set; } = "";

		public int Width { get; set; }

		public int Height { get; set; }

		public string Base64DataBack { get; set; } = "";

		public bool HasHairColor { get; set; }

		public string HairColorName { get; set; } = "";

		public string HairColorHex { get; set; } = "";

		public bool HasHatColor { get; set; }

		public string HatColorName { get; set; } = "";

		public string HatColorHex { get; set; } = "";

		public bool IsUsable => !string.IsNullOrWhiteSpace(Base64Data) && !string.IsNullOrWhiteSpace(MimeType);

		public bool HasBackImage => !string.IsNullOrWhiteSpace(Base64DataBack);

		public string ToDataUri()
		{
			return "data:" + (string.IsNullOrWhiteSpace(MimeType) ? "image/png" : MimeType) + ";base64," + Base64Data;
		}

		public string ToBackDataUri()
		{
			return "data:" + (string.IsNullOrWhiteSpace(MimeType) ? "image/png" : MimeType) + ";base64," + Base64DataBack;
		}
	}
	internal sealed class OutfitVisionService
	{
		private const int ImageWidth = 256;

		private const int ImageHeight = 256;

		private readonly IMonitor monitor;

		public OutfitVisionService(IMonitor monitor)
		{
			this.monitor = monitor;
		}

		public bool TryCaptureFarmerAppearance(Farmer farmer, out OutfitVisionImage image, out string reason)
		{
			//IL_0358: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Expected O, but got Unknown
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			image = null;
			reason = "unknown reason";
			if (farmer == null)
			{
				reason = "farmer is null";
				return false;
			}
			GraphicsDeviceManager graphics = Game1.graphics;
			if (((graphics != null) ? graphics.GraphicsDevice : null) == null)
			{
				reason = "graphics device is unavailable";
				return false;
			}
			GraphicsDevice graphicsDevice = Game1.graphics.GraphicsDevice;
			RenderTarget2D val = null;
			SpriteBatch val2 = null;
			RenderTargetBinding[] array = null;
			bool isDrawingForUI = FarmerRenderer.isDrawingForUI;
			try
			{
				array = graphicsDevice.GetRenderTargets();
				val = new RenderTarget2D(graphicsDevice, 256, 256, false, (SurfaceFormat)0, (DepthFormat)0);
				val2 = new SpriteBatch(graphicsDevice);
				graphicsDevice.SetRenderTarget(val);
				graphicsDevice.Clear(Color.Transparent);
				FarmerRenderer.isDrawingForUI = true;
				val2.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, (Matrix?)null);
				Vector2 val3 = default(Vector2);
				((Vector2)(ref val3))..ctor(96f, 56f);
				farmer.FarmerRenderer.draw(val2, farmer.FarmerSprite.CurrentAnimationFrame, ((AnimatedSprite)farmer.FarmerSprite).CurrentFrame, ((AnimatedSprite)farmer.FarmerSprite).SourceRect, val3, Vector2.Zero, 1f, 2, Color.White, 0f, 4f, farmer);
				val2.End();
				if (array != null && array.Length != 0)
				{
					graphicsDevice.SetRenderTargets(array);
				}
				else
				{
					graphicsDevice.SetRenderTarget((RenderTarget2D)null);
				}
				using MemoryStream memoryStream = new MemoryStream();
				((Texture2D)val).SaveAsPng((Stream)memoryStream, 256, 256);
				byte[] array2 = memoryStream.ToArray();
				if (array2.Length == 0)
				{
					reason = "captured PNG was empty";
					return false;
				}
				using SHA256 sHA = SHA256.Create();
				byte[] array3 = sHA.ComputeHash(array2);
				image = new OutfitVisionImage
				{
					MimeType = "image/png",
					Base64Data = Convert.ToBase64String(array2),
					Hash = BitConverter.ToString(array3).Replace("-", "").ToLowerInvariant(),
					Width = 256,
					Height = 256
				};
				try
				{
					Color[] array4 = (Color[])(object)new Color[65536];
					((Texture2D)val).GetData<Color>(array4);
					if (TryEstimateHairColor(array4, 256, 256, out var result))
					{
						image.HasHairColor = true;
						image.HairColorName = ColorNamer.ClosestSimpleColorName(result);
						image.HairColorHex = "#" + ((Color)(ref result)).R.ToString("X2") + ((Color)(ref result)).G.ToString("X2") + ((Color)(ref result)).B.ToString("X2");
					}
					if (TryEstimateHatColor(array4, 256, 256, out var result2))
					{
						image.HasHatColor = true;
						image.HatColorName = ColorNamer.ClosestSimpleColorName(result2);
						image.HatColorHex = "#" + ((Color)(ref result2)).R.ToString("X2") + ((Color)(ref result2)).G.ToString("X2") + ((Color)(ref result2)).B.ToString("X2");
					}
				}
				catch (Exception ex)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("Hair/hat color pixel read failed: " + ex.Message, (LogLevel)0);
					}
				}
				try
				{
					RenderTarget2D val4 = new RenderTarget2D(graphicsDevice, 256, 256, false, (SurfaceFormat)0, (DepthFormat)0);
					SpriteBatch val5 = new SpriteBatch(graphicsDevice);
					try
					{
						graphicsDevice.SetRenderTarget(val4);
						graphicsDevice.Clear(Color.Transparent);
						FarmerRenderer.isDrawingForUI = true;
						val5.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, (Matrix?)null);
						farmer.FarmerRenderer.draw(val5, farmer.FarmerSprite.CurrentAnimationFrame, ((AnimatedSprite)farmer.FarmerSprite).CurrentFrame, ((AnimatedSprite)farmer.FarmerSprite).SourceRect, new Vector2(96f, 56f), Vector2.Zero, 1f, 0, Color.White, 0f, 4f, farmer);
						val5.End();
						if (array != null && array.Length != 0)
						{
							graphicsDevice.SetRenderTargets(array);
						}
						else
						{
							graphicsDevice.SetRenderTarget((RenderTarget2D)null);
						}
						using MemoryStream memoryStream2 = new MemoryStream();
						((Texture2D)val4).SaveAsPng((Stream)memoryStream2, 256, 256);
						byte[] array5 = memoryStream2.ToArray();
						if (array5.Length != 0)
						{
							image.Base64DataBack = Convert.ToBase64String(array5);
						}
					}
					finally
					{
						((GraphicsResource)val5).Dispose();
						((GraphicsResource)val4).Dispose();
					}
				}
				catch (Exception ex2)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("Back-view capture failed (front view still used): " + ex2.Message, (LogLevel)0);
					}
				}
				reason = "ok";
				return true;
			}
			catch (Exception ex3)
			{
				reason = ex3.Message;
				IMonitor obj3 = monitor;
				if (obj3 != null)
				{
					obj3.Log(" Vision outfit capture failed: " + ex3.Message, (LogLevel)0);
				}
				return false;
			}
			finally
			{
				try
				{
					if (val2 != null)
					{
						((GraphicsResource)val2).Dispose();
					}
				}
				catch
				{
				}
				try
				{
					FarmerRenderer.isDrawingForUI = isDrawingForUI;
					if (array != null && array.Length != 0)
					{
						graphicsDevice.SetRenderTargets(array);
					}
					else
					{
						graphicsDevice.SetRenderTarget((RenderTarget2D)null);
					}
				}
				catch
				{
				}
				try
				{
					if (val != null)
					{
						((GraphicsResource)val).Dispose();
					}
				}
				catch
				{
				}
			}
		}

		private static bool TryEstimateHairColor(Color[] pixels, int width, int height, out Color result)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0340: Unknown result type (might be due to invalid IL or missing references)
			result = Color.Black;
			if (pixels == null || pixels.Length < width * height || width <= 0 || height <= 0)
			{
				return false;
			}
			int num = width;
			int num2 = height;
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (((Color)(ref pixels[i * width + j])).A >= 128)
					{
						if (j < num)
						{
							num = j;
						}
						if (j > num3)
						{
							num3 = j;
						}
						if (i < num2)
						{
							num2 = i;
						}
						if (i > num4)
						{
							num4 = i;
						}
					}
				}
			}
			if (num3 < num || num4 < num2)
			{
				return false;
			}
			int num5 = num4 - num2 + 1;
			int num6 = num2 + Math.Max(1, (int)((double)num5 * 0.38));
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Dictionary<int, long[]> dictionary2 = new Dictionary<int, long[]>();
			for (int k = num2; k <= num6 && k <= num4; k++)
			{
				for (int l = num; l <= num3; l++)
				{
					Color val = pixels[k * width + l];
					if (((Color)(ref val)).A < 200)
					{
						continue;
					}
					int num7 = (((Color)(ref val)).R + ((Color)(ref val)).G + ((Color)(ref val)).B) / 3;
					if (num7 >= 28 && num7 <= 248)
					{
						int key = (((Color)(ref val)).R >> 3 << 10) | (((Color)(ref val)).G >> 3 << 5) | (((Color)(ref val)).B >> 3);
						if (!dictionary.ContainsKey(key))
						{
							dictionary[key] = 0;
							dictionary2[key] = new long[4];
						}
						dictionary[key]++;
						long[] array = dictionary2[key];
						array[0] += ((Color)(ref val)).R;
						array[1] += ((Color)(ref val)).G;
						array[2] += ((Color)(ref val)).B;
						array[3]++;
					}
				}
			}
			if (dictionary.Count == 0)
			{
				return false;
			}
			int key2 = 0;
			int num8 = -1;
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				if (item.Value > num8)
				{
					num8 = item.Value;
					key2 = item.Key;
				}
			}
			long[] array2 = dictionary2[key2];
			if (array2[3] <= 0)
			{
				return false;
			}
			result = new Color((int)(array2[0] / array2[3]), (int)(array2[1] / array2[3]), (int)(array2[2] / array2[3]));
			return true;
		}

		private static bool TryEstimateHatColor(Color[] pixels, int width, int height, out Color result)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			result = Color.Black;
			if (pixels == null || pixels.Length < width * height || width <= 0 || height <= 0)
			{
				return false;
			}
			int num = width;
			int num2 = height;
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (((Color)(ref pixels[i * width + j])).A >= 128)
					{
						if (j < num)
						{
							num = j;
						}
						if (j > num3)
						{
							num3 = j;
						}
						if (i < num2)
						{
							num2 = i;
						}
						if (i > num4)
						{
							num4 = i;
						}
					}
				}
			}
			if (num3 < num || num4 < num2)
			{
				return false;
			}
			int num5 = num4 - num2 + 1;
			int num6 = num3 - num + 1;
			int num7 = num2 + Math.Max(1, (int)((double)num5 * 0.24));
			int num8 = Math.Max(0, (int)((double)num6 * 0.08));
			int num9 = num + num8;
			int num10 = num3 - num8;
			if (num10 < num9)
			{
				num9 = num;
				num10 = num3;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Dictionary<int, long[]> dictionary2 = new Dictionary<int, long[]>();
			for (int k = num2; k <= num7 && k <= num4; k++)
			{
				for (int l = num9; l <= num10; l++)
				{
					Color val = pixels[k * width + l];
					if (((Color)(ref val)).A < 200)
					{
						continue;
					}
					int num11 = (((Color)(ref val)).R + ((Color)(ref val)).G + ((Color)(ref val)).B) / 3;
					if (num11 >= 28 && num11 <= 248 && (((Color)(ref val)).R < 175 || ((Color)(ref val)).G < 105 || ((Color)(ref val)).G > 220 || ((Color)(ref val)).B < 70 || ((Color)(ref val)).B > 190 || ((Color)(ref val)).R <= ((Color)(ref val)).B + 20))
					{
						int key = (((Color)(ref val)).R >> 3 << 10) | (((Color)(ref val)).G >> 3 << 5) | (((Color)(ref val)).B >> 3);
						if (!dictionary.ContainsKey(key))
						{
							dictionary[key] = 0;
							dictionary2[key] = new long[4];
						}
						dictionary[key]++;
						long[] array = dictionary2[key];
						array[0] += ((Color)(ref val)).R;
						array[1] += ((Color)(ref val)).G;
						array[2] += ((Color)(ref val)).B;
						array[3]++;
					}
				}
			}
			if (dictionary.Count == 0)
			{
				return false;
			}
			int key2 = 0;
			int num12 = -1;
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				if (item.Value > num12)
				{
					num12 = item.Value;
					key2 = item.Key;
				}
			}
			long[] array2 = dictionary2[key2];
			if (array2[3] <= 0)
			{
				return false;
			}
			result = new Color((int)(array2[0] / array2[3]), (int)(array2[1] / array2[3]), (int)(array2[2] / array2[3]));
			return true;
		}
	}
	internal enum AiGenerationLifecycleState
	{
		Waiting,
		Completed,
		TimedOut
	}
	internal interface IAiPendingGeneration
	{
		Task<string> Task { get; }

		int SafetyTimer { get; set; }
	}
	internal sealed class PendingAiGeneration : IAiPendingGeneration
	{
		public string NpcName { get; set; } = "";

		public bool IsSpouseDialogue { get; set; }

		public bool ClearExistingDialogue { get; set; }

		public Task<string> Task { get; set; }

		public CancellationTokenSource Cancellation { get; set; }

		public bool CompletionHandled { get; set; }

		public int WaitingDotCount { get; set; } = 1;

		public int WaitingDotTimer { get; set; } = 30;

		public int SafetyTimer { get; set; } = 7200;
	}
	internal sealed class PendingAiPlayerReplyGeneration : IAiPendingGeneration
	{
		public string NpcName { get; set; } = "";

		public bool IsSpouseDialogue { get; set; }

		public string NpcCompliment { get; set; } = "";

		public string PlayerReply { get; set; } = "";

		public Task<string> Task { get; set; }

		public CancellationTokenSource Cancellation { get; set; }

		public bool CompletionHandled { get; set; }

		public int WaitingDotCount { get; set; } = 1;

		public int WaitingDotTimer { get; set; } = 30;

		public int SafetyTimer { get; set; } = 7200;

		public Action OnFinished { get; set; }
	}
	internal static class AiRequestLifecycle
	{
		public static void Cancel(CancellationTokenSource cancellation)
		{
			if (cancellation == null)
			{
				return;
			}
			try
			{
				cancellation.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
		}

		public static void DisposeWhenFinished(Task task, CancellationTokenSource cancellation)
		{
			if (task != null && cancellation != null)
			{
				task.ContinueWith(delegate
				{
					cancellation.Dispose();
				}, TaskScheduler.Default);
			}
		}
	}
	internal static class AiDialogueLifecycle
	{
		public static AiGenerationLifecycleState Advance(IAiPendingGeneration pending)
		{
			if (pending != null && pending.Task?.IsCompleted == true)
			{
				return AiGenerationLifecycleState.Completed;
			}
			if (pending != null && pending.SafetyTimer > 0)
			{
				pending.SafetyTimer--;
				return AiGenerationLifecycleState.Waiting;
			}
			return AiGenerationLifecycleState.TimedOut;
		}
	}
	internal sealed class PromptStyleService
	{
		private sealed class PromptStyleData
		{
			public string HairChangeMode { get; set; }

			public string HatChangeMode { get; set; }

			public string AccessoryChangeMode { get; set; }

			public string OutfitChangeMode { get; set; }

			public string NaturalReactionStyle { get; set; }

			public string PlayerKnownAddressRule { get; set; }

			public string PlayerUnknownAddressRule { get; set; }

			public string PlayerGenderRule { get; set; }

			public string VisibleVanillaHatOnlyMode { get; set; }

			public string RemovedVanillaHatOnlyMode { get; set; }

			public string SavedOutfitFocusGuidance { get; set; }

			public string HairFocusGuidance { get; set; }

			public string HatFocusGuidance { get; set; }

			public string AccessoryFocusGuidance { get; set; }

			public string FashionSenseVisualSupportRule { get; set; }

			public string FashionSenseVisualSeparationRule { get; set; }

			public string SpecialItemVisibleRule { get; set; }

			public string SpecialItemRemovedRule { get; set; }

			public string SpecialVanillaHatRule { get; set; }

			public string VanillaHatMemoryRule { get; set; }

			public string SpecialItemMemoryRule { get; set; }
		}

		public static readonly string FallbackHairChangeMode = "The NPC noticed the farmer's hairstyle or hair color changed. IMPORTANT: only the hairstyle/hair color is new — the farmer's outfit is exactly the same as before. React to the hair change and how it affects the farmer's overall vibe; do not talk about the outfit as if it were new or different. Name exact colors or specific hairstyles only when the support data makes them clear.";

		public static readonly string FallbackHatChangeMode = "The NPC noticed the farmer's head item/headwear. React to what it seems to be and how it changes the farmer's vibe; do not call small headbands, tiaras, bows, flowers, clips, or hair accessories a hat unless the support data says it is a hat.";

		public static readonly string FallbackAccessoryChangeMode = "The NPC noticed the farmer's accessory changed. IMPORTANT: only the accessory is new — the farmer's outfit and hairstyle are exactly the same as before. React naturally to the accessory itself and how it pairs with the outfit the farmer is already wearing. If the accessory creates a funny, cute, ugly, strange, dramatic, or impossible combination with the saved outfit theme, the NPC may compare them, question it, tease it, or make an in-world joke instead of treating the accessory as isolated. If the accessory is visually unclear, do not guess.";

		public static readonly string FallbackOutfitChangeMode = "The NPC noticed the farmer's saved outfit. React naturally to what the outfit seems to be, the situation, recognizable theme/reference, and the farmer's overall vibe. Mention colors, texture, or style only as small character-specific details, not as a fashion review.";

		public static readonly string FallbackNaturalReactionStyle = "This is an honest REACTION, not a compliment. A compliment is only one of many possible reactions and is never required. React the way this specific person honestly would: if the look is good, they can like it; if it is strange, ugly, tacky, gross, confusing, ridiculous, or off-putting, they can say so in their own voice. Do not soften a genuinely weird or unappealing look into a polite compliment just to be nice. Flattering a clearly bizarre or ugly item when the NPC would not actually admire it reads as fake and breaks character. Match the honesty to the NPC's personality and relationship: a blunt NPC can be openly critical or mocking; a kind NPC can be diplomatically honest or visibly unsure rather than fake-positive; a close partner can tease or be playfully horrified. Negative, confused, and unimpressed reactions are fully allowed and often the correct choice for unusual or unflattering looks. React like a person seeing the farmer's {Change}, not like a fashion analyst or stylist reviewing a palette. Start from the NPC's own personality and relationship level, then react to the obvious concept, situation, mood, humor, surprise, concern, or relationship impact before mentioning visual details. If the look resembles a costume, animal suit, cosplay, pajamas, swimwear, work outfit, festival outfit, silly outfit, cute outfit, or something unusual, comment on what it seems to be first. If the outfit/change clues include a recognizable theme, character, franchise, creature, animal, object, food, fantasy archetype, or named inspiration, the NPC may mention or allude to that reference when it fits their knowledge and personality. Geeky, pop-culture-aware, playful, artistic, or highly observant NPCs can be more specific; others can react to the creature/theme more generally. Do not force every NPC to recognize every reference, but do not ignore clear clues like Sanrio, My Melody, Pikachu, Pokemon/Pokémon, lizard, dinosaur, frog, fairy, cat, rabbit, or other named themes when they would naturally affect the reaction. IMPORTANT — the outfit NAME is only a label/theme, not a guaranteed list of worn pieces. A themed name (e.g. 'Rabbit Outfit', 'Cat Costume', 'Demon Set') often implies a head piece like ears, horns, antennae, or a themed hat, but the farmer may have removed it. Only treat a head piece as worn if it appears in the equipped-items list. If the clues say no head piece is equipped (e.g. 'head/headwear: NONE equipped'), do NOT mention or describe ears, horns, a hat, or any head accessory the theme name suggests — the farmer is bare-headed right now. You may still reference the rest of the theme (the clothing/body that IS worn), just not the missing head piece. When a recognizable theme is present, do more than a generic 'it suits you' compliment. Choose the reaction angle from the NPC's personality: a joke, playful question, friendly roast, surprised confusion, dry sarcasm, affectionate teasing, practical concern, admiration, indifference, reluctant approval, or a small imagined scenario where that theme would fit. Only compare the look to farm life, pets, crops, monsters, caves, the saloon, the beach, festivals, the town, or daily chores when that topic naturally belongs to this NPC, the current location, the outfit theme, or the relationship context. Do not use mines, slimes, monsters, caves, the saloon, or farm chores as generic Stardew references for NPCs who would not think about them. If the noticed change is an accessory while the farmer is still wearing a themed saved outfit, treat the accessory as part of the whole current look. The NPC may compare the new accessory with the existing outfit theme, notice clashes or absurd combinations, and joke about mismatches like wings added to a Pikachu/animal/mascot/cosplay outfit. Do not respond as if the accessory exists alone when the full outfit context is available. Occasion mismatch: also judge whether the item makes sense for the CURRENT occasion, place, and moment (use the Location, Festival, season, weather, and time already provided). Items tied to a specific event — a bridal veil, a party hat, a graduation cap, formal/gala wear, holiday costumes, a swimsuit — worn with NO matching occasion happening can be gently questioned, teased, or remarked on. For example, a wedding veil when there is no wedding, or a party hat when there is no party, is odd and an observant or blunt NPC may point it out, ask about it, or joke. Do not force this every time; weigh it against the NPC's personality and how striking the mismatch is. If there IS a matching occasion (a festival, an actual wedding, a fitting location), the item fits and needs no such remark. The NPC may find a look cute, weird, ridiculous, ugly, funny, suspicious, adorable, dramatic, awkward, unnecessary, too flashy, practical, or oddly fitting if their personality supports it. Avoid making 'combina com você'/'it suits you' the main point; if that idea appears at all, keep it secondary to a concrete reaction, question, joke, reference, or situation. Opening variety is mandatory: do not reuse the same first words, opening phrase, sentence structure, or reaction angle across attempts. If a gruff NPC uses one, vary it or start directly with a concrete observation, complaint, warning, skeptical remark, or reluctant admission instead. Colors, texture, silhouette, balance, composition, or coordination may be mentioned only as small details when they sound natural for this NPC. Avoid making those analytic terms the structure of the line. {OutfitFocusRule}Use location, season, weather, time, and privacy only when they add a real human reaction; never force them as a closing justification. Location/farm/town references may be hypothetical jokes or comparisons if phrased clearly as 'parece que', 'se você aparecesse...', 'dá pra imaginar...', or similar. For head items, respect support data: a headband, tiara, hairband, bow, clip, flower, pin, or crown should not be called a hat unless the metadata says it is a hat.";

		public static readonly string FallbackPlayerKnownAddressRule = "PLAYER ADDRESS RULE: the player character's name is {PlayerName}. Do not address the player as a localized equivalent of 'farmer', 'player', 'rancher', 'new farmer', or 'newcomer' when the NPC would reasonably know the player's name; use the player's name instead in those cases. Role/job words may still appear naturally in descriptions of farm work, but not as a replacement for the player's name. USE THE NAME SPARINGLY: most lines do not need a vocative at all — real conversation between people who know each other rarely opens or closes every line with a name. Only use the player's name when it serves a real purpose (a genuine greeting, getting their attention, strong emphasis, an emotional beat, or scolding/calling out), not as a filler or a habitual sentence-starter/ender. Do not use the name in consecutive lines or more than once in a short reaction unless the moment truly calls for it.";

		public static readonly string FallbackPlayerUnknownAddressRule = "PLAYER ADDRESS RULE: the player character's name is unavailable. Avoid overusing generic role labels as direct address; use natural dialogue without a vocative when possible.";

		public static readonly string FallbackPlayerGenderRule = "PLAYER GENDER RULE: the player character's grammatical gender is {PlayerGender}. Use matching grammatical agreement in {TargetLanguage} when the language requires gendered pronouns, adjectives, nouns, or titles. If the target language does not require grammatical gender in that sentence, neutral wording is allowed. Do not add gendered labels unnecessarily. Do not use romantic pet names, beauty-based nicknames, or overly intimate address unless they fit this NPC's established personality and relationship level. {GenderSpecificCaution}";

		public static readonly string FallbackVisibleVanillaHatOnlyMode = "HAT-ONLY reaction mode: this reaction is about the visible vanilla/base-game hat the farmer is currently wearing, and the player has asked NPCs to react EXCLUSIVELY to that hat. React ONLY to the hat itself — its look, shape, color, vibe, and how it suits or clashes with the farmer. Do NOT describe, compliment, mention, or factor in the rest of the outfit, clothes, accessories, hair, or overall look; treat everything except the visible hat as irrelevant for this reaction. A tightly hat-focused reaction can be funnier and more pointed, so lean into the hat fully.";

		public static readonly string FallbackRemovedVanillaHatOnlyMode = "HAT-REMOVAL-ONLY reaction mode: this reaction is about a vanilla/base-game hat the farmer has just removed, and this specific NPC has enough prior context to notice that absence. The farmer is currently bare-headed. React ONLY to the removal/absence of the hat, not to a hat they are wearing now. Do NOT say or imply the farmer is currently wearing a hat. Do NOT invent a current hat color, shape, material, or style from hair, head pixels, or the outfit. Do NOT describe, compliment, mention, or factor in the rest of the outfit, clothes, accessories, or overall look; treat everything except the removed hat/now-bare head as irrelevant for this reaction.";

		public static readonly string FallbackSavedOutfitFocusGuidance = "Focus guidance: the NPC noticed the saved outfit. React to what the outfit seems to be, any recognizable theme/reference, the situation, and the farmer's overall vibe first. If the theme is recognizable, the NPC may use humor, questions, playful roasting, imagined scenarios, farm/town/place comparisons, or character-specific weirdness instead of simple praise. Mention outfit colors/details only if they are obvious from the outfit/theme or textual support data and sound casual. Do NOT focus on the player's hair, hair color, or a tiny/generic head-slot item when reacting to a whole saved outfit. Do not call the farmer's hair a hat. Do not focus on a single headwear/accessory unless it clearly completes the outfit.";

		public static readonly string FallbackHairFocusGuidance = "Focus guidance: the NPC noticed the hairstyle/hair change. CRITICAL for hair COLOR: name a hair color ONLY if the support data states a confirmed/authoritative hair color; in that case use that exact color word. If no confirmed hair color is given, do NOT name any hair color at all — never read or guess hair color from the image, pixels, sprite shading, floors, lighting, or scenery. For the HAIRSTYLE itself, do NOT assert a specific style category (braids, pigtails, twin-tails, bun, ponytail, dreadlocks, curls, etc.) unless it is unmistakably clear; small pixel sprites are easy to misread, so when unsure refer to it generally (the new hairstyle, the new look) instead of naming a wrong style. When neither color nor style is certain, simply react to the new look suiting the farmer. If the support data or image also shows a large, obvious accessory such as an umbrella, wings, or backpack, you may mention it briefly as secondary context, but do not ignore the hair change.";

		public static readonly string FallbackHatFocusGuidance = "Focus guidance: the NPC noticed the hat/headwear change. You may mention the hat briefly, but do NOT make the entire dialogue only about the hat — weave it into the overall look and how it suits the farmer. CRITICAL for hat/headwear COLOR: name a hat/headwear color ONLY if the support data states a confirmed/authoritative hat color; if not stated, do NOT name any hat color. For shape/style, only describe what is visually clear; when unsure refer to it generally.";

		public static readonly string FallbackAccessoryFocusGuidance = "Focus guidance: the NPC noticed a Fashion Sense accessory change. Accessories may be wings, backpacks, umbrellas, animated decorations, small earrings, or other extra visual pieces; ignore makeup-like accessories. Focus on the visible accessory if clear, but do NOT isolate it from the rest of the current outfit. If the saved outfit/theme is recognizable, compare the accessory with that theme and react to the combined look: funny mismatch, cute chaos, weird hybrid, dramatic upgrade, ugly clash, playful roast, or an in-world joke according to the NPC. This applies even when the outfit and accessory were equipped in the same Hand Mirror session: the changed accessory is the hook, and the saved outfit/theme is the thing it should be compared against. If it is not visually identifiable, use the clarification behavior instead of guessing.";

		public static readonly string FallbackFashionSenseVisualSupportRule = "Fashion Sense API visual support data. This is AUTHORITATIVE for equipped item IDs and for any confirmed color it states. For broad outfit/clothing colors, the attached image may also be used when the color is clearly visible on the farmer; use ordinary broad words like pink, white, black, yellow, red, green, or brown, not over-specific pixel guesses. Never take colors from hair, tiny/generic head-slot items, floor, background, scenery, lighting, furniture, or walls. If this data states a confirmed hair color or confirmed hat/headwear color, use exactly that only for hair/hat-specific reactions; if it states none/untinted or gives no confirmed color for hair/hat, do not name that hair/hat color from the image. When the noticed change is a whole saved outfit, hair and generic head-slot IDs are intentionally omitted or should be ignored; focus on the saved outfit/theme and meaningful visible pieces instead. If an item clue looks like an internal/generic ID such as \"pack0005 hat 2\", do not treat it as meaningful dialogue content and do not call it a hat just because the slot says hat. Always ignore floor/background/scenery colors. Never mention Fashion Sense, API, internal IDs, filenames, or technical labels in dialogue: {VisualSummary}";

		public static readonly string FallbackFashionSenseVisualSeparationRule = "IMPORTANT: the 'hair' entry in the data above is the player character's HAIRSTYLE — it is NOT a hat and NOT part of the outfit. Accessory entries can be visible decorative pieces such as wings, backpacks, umbrellas, capes, bows, clips, flowers, earrings, or animated decorations; ignore makeup-like accessories. Some are worn on/in the hair and some are large body/back accessories. Do not treat hair accessories as clothing palette, but do treat clearly visible large accessories as part of the current overall look. When the noticed change is an accessory and a saved outfit/theme is also present, compare them naturally instead of describing only the accessory. Never reference hair colour or tiny hair/head accessory colours when describing a saved outfit. You may name broad dominant clothing/outfit colors from the attached image only when they are clearly on the outfit itself, not on hair, head-slot items, scenery, or lighting. For a whole saved outfit reaction, do not infer hat/headwear color from pixels or from the image, and never describe the player's hair as a brown hat. If the head-slot ID is generic/internal, ignore it unless the image makes a meaningful head accessory obvious; even then, describe it generally as a bow/head accessory/tiara/etc. only if clear.";

		public static readonly string FallbackSpecialItemVisibleRule = "SPECIAL ITEM REACTION DATA. This is authoritative for the item the farmer is currently wearing and comes from assets/special-reactions/luckypurpleshorts.json. Do not mention technical labels, JSON keys, or file names in dialogue: {SpecialItemData}\nIf this special item data is present, use it as a high-priority reaction hook. The NPC should react specifically to this item according to their personality, relationship, and context — especially any NPC-specific instructions or secret knowledge indicated in the data.";

		public static readonly string FallbackSpecialItemRemovedRule = "SPECIAL ITEM — JUST REMOVED. The farmer was wearing a special item and has now taken it off. React to the fact that it is gone, based on how this NPC feels about that item. Data: {SpecialItemData}";

		public static readonly string FallbackSpecialVanillaHatRule = "SPECIAL VANILLA HAT REACTION DATA. This is authoritative for the currently equipped vanilla/base-game hat and comes from assets/special-reactions/hat.json. Do not mention technical labels, JSON keys, categories, tags, or the file name in dialogue: {SpecialHatData}\nIf this special hat data is present, use it as a high-priority appearance hook. The NPC should react specifically to this hat according to their personality, relationship, location, season, and context. The reaction may be amused, critical, confused, fascinated, awkward, approving, worried, blunt, or contextual; do not default to a bland generic compliment.";

		public static readonly string FallbackVanillaHatMemoryRule = "MEMORY — IMPORTANT: {HatMemory}\nThis memory is mandatory context: the NPC already has history with this exact hat. Weave that recognition into the reaction (familiarity, a callback to before, etc.) in the NPC's own voice. Do NOT write the line as a first-time discovery.";

		public static readonly string FallbackSpecialItemMemoryRule = "MEMORY — IMPORTANT: {ItemMemory}\nThis memory is mandatory context: the NPC already has history with this exact item. Do NOT write the line as if seeing it for the first time. Weave that recognition into the reaction — show familiarity, reference it as something they remember, or express a running opinion about it. The more times seen, the more established and natural that opinion should feel.";

		private readonly IMonitor monitor;

		private readonly string promptsFilePath;

		public string HairChangeMode { get; private set; } = FallbackHairChangeMode;

		public string HatChangeMode { get; private set; } = FallbackHatChangeMode;

		public string AccessoryChangeMode { get; private set; } = FallbackAccessoryChangeMode;

		public string OutfitChangeMode { get; private set; } = FallbackOutfitChangeMode;

		public string NaturalReactionStyle { get; private set; } = FallbackNaturalReactionStyle;

		public string PlayerKnownAddressRule { get; private set; } = FallbackPlayerKnownAddressRule;

		public string PlayerUnknownAddressRule { get; private set; } = FallbackPlayerUnknownAddressRule;

		public string PlayerGenderRule { get; private set; } = FallbackPlayerGenderRule;

		public string VisibleVanillaHatOnlyMode { get; private set; } = FallbackVisibleVanillaHatOnlyMode;

		public string RemovedVanillaHatOnlyMode { get; private set; } = FallbackRemovedVanillaHatOnlyMode;

		public string SavedOutfitFocusGuidance { get; private set; } = FallbackSavedOutfitFocusGuidance;

		public string HairFocusGuidance { get; private set; } = FallbackHairFocusGuidance;

		public string HatFocusGuidance { get; private set; } = FallbackHatFocusGuidance;

		public string AccessoryFocusGuidance { get; private set; } = FallbackAccessoryFocusGuidance;

		public string FashionSenseVisualSupportRule { get; private set; } = FallbackFashionSenseVisualSupportRule;

		public string FashionSenseVisualSeparationRule { get; private set; } = FallbackFashionSenseVisualSeparationRule;

		public string SpecialItemVisibleRule { get; private set; } = FallbackSpecialItemVisibleRule;

		public string SpecialItemRemovedRule { get; private set; } = FallbackSpecialItemRemovedRule;

		public string SpecialVanillaHatRule { get; private set; } = FallbackSpecialVanillaHatRule;

		public string VanillaHatMemoryRule { get; private set; } = FallbackVanillaHatMemoryRule;

		public string SpecialItemMemoryRule { get; private set; } = FallbackSpecialItemMemoryRule;

		public PromptStyleService(IModHelper helper, IMonitor monitor)
		{
			this.monitor = monitor;
			promptsFilePath = Path.Combine(helper.DirectoryPath, "assets", "prompts", "prompts.json");
		}

		public void Load(bool quiet = false)
		{
			if (!File.Exists(promptsFilePath))
			{
				if (!quiet)
				{
					monitor.Log("[Prompts] prompts.json not found — using built-in defaults.", (LogLevel)0);
				}
				ResetToDefaults();
				return;
			}
			try
			{
				string input = File.ReadAllText(promptsFilePath, Encoding.UTF8);
				input = Regex.Replace(input, "^\\s*//[^\\r\\n]*", "", RegexOptions.Multiline);
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				PromptStyleData promptStyleData = JsonSerializer.Deserialize<PromptStyleData>(input, options);
				if (promptStyleData == null)
				{
					monitor.Log("[Prompts] prompts.json deserialized as null — using built-in defaults.", (LogLevel)3);
					ResetToDefaults();
					return;
				}
				HairChangeMode = Coalesce(promptStyleData.HairChangeMode, FallbackHairChangeMode);
				HatChangeMode = Coalesce(promptStyleData.HatChangeMode, FallbackHatChangeMode);
				AccessoryChangeMode = Coalesce(promptStyleData.AccessoryChangeMode, FallbackAccessoryChangeMode);
				OutfitChangeMode = Coalesce(promptStyleData.OutfitChangeMode, FallbackOutfitChangeMode);
				NaturalReactionStyle = Coalesce(promptStyleData.NaturalReactionStyle, FallbackNaturalReactionStyle);
				PlayerKnownAddressRule = Coalesce(promptStyleData.PlayerKnownAddressRule, FallbackPlayerKnownAddressRule);
				PlayerUnknownAddressRule = Coalesce(promptStyleData.PlayerUnknownAddressRule, FallbackPlayerUnknownAddressRule);
				PlayerGenderRule = Coalesce(promptStyleData.PlayerGenderRule, FallbackPlayerGenderRule);
				VisibleVanillaHatOnlyMode = Coalesce(promptStyleData.VisibleVanillaHatOnlyMode, FallbackVisibleVanillaHatOnlyMode);
				RemovedVanillaHatOnlyMode = Coalesce(promptStyleData.RemovedVanillaHatOnlyMode, FallbackRemovedVanillaHatOnlyMode);
				SavedOutfitFocusGuidance = Coalesce(promptStyleData.SavedOutfitFocusGuidance, FallbackSavedOutfitFocusGuidance);
				HairFocusGuidance = Coalesce(promptStyleData.HairFocusGuidance, FallbackHairFocusGuidance);
				HatFocusGuidance = Coalesce(promptStyleData.HatFocusGuidance, FallbackHatFocusGuidance);
				AccessoryFocusGuidance = Coalesce(promptStyleData.AccessoryFocusGuidance, FallbackAccessoryFocusGuidance);
				FashionSenseVisualSupportRule = Coalesce(promptStyleData.FashionSenseVisualSupportRule, FallbackFashionSenseVisualSupportRule);
				FashionSenseVisualSeparationRule = Coalesce(promptStyleData.FashionSenseVisualSeparationRule, FallbackFashionSenseVisualSeparationRule);
				SpecialItemVisibleRule = Coalesce(promptStyleData.SpecialItemVisibleRule, FallbackSpecialItemVisibleRule);
				SpecialItemRemovedRule = Coalesce(promptStyleData.SpecialItemRemovedRule, FallbackSpecialItemRemovedRule);
				SpecialVanillaHatRule = Coalesce(promptStyleData.SpecialVanillaHatRule, FallbackSpecialVanillaHatRule);
				VanillaHatMemoryRule = Coalesce(promptStyleData.VanillaHatMemoryRule, FallbackVanillaHatMemoryRule);
				SpecialItemMemoryRule = Coalesce(promptStyleData.SpecialItemMemoryRule, FallbackSpecialItemMemoryRule);
				if (!quiet)
				{
					monitor.Log("[Prompts] prompts.json loaded successfully.", (LogLevel)0);
				}
			}
			catch (Exception ex)
			{
				monitor.Log("[Prompts] Failed to load prompts.json — using built-in defaults. Error: " + ex.Message, (LogLevel)3);
				ResetToDefaults();
			}
		}

		private void ResetToDefaults()
		{
			HairChangeMode = FallbackHairChangeMode;
			HatChangeMode = FallbackHatChangeMode;
			AccessoryChangeMode = FallbackAccessoryChangeMode;
			OutfitChangeMode = FallbackOutfitChangeMode;
			NaturalReactionStyle = FallbackNaturalReactionStyle;
			PlayerKnownAddressRule = FallbackPlayerKnownAddressRule;
			PlayerUnknownAddressRule = FallbackPlayerUnknownAddressRule;
			PlayerGenderRule = FallbackPlayerGenderRule;
			VisibleVanillaHatOnlyMode = FallbackVisibleVanillaHatOnlyMode;
			RemovedVanillaHatOnlyMode = FallbackRemovedVanillaHatOnlyMode;
			SavedOutfitFocusGuidance = FallbackSavedOutfitFocusGuidance;
			HairFocusGuidance = FallbackHairFocusGuidance;
			HatFocusGuidance = FallbackHatFocusGuidance;
			AccessoryFocusGuidance = FallbackAccessoryFocusGuidance;
			FashionSenseVisualSupportRule = FallbackFashionSenseVisualSupportRule;
			FashionSenseVisualSeparationRule = FallbackFashionSenseVisualSeparationRule;
			SpecialItemVisibleRule = FallbackSpecialItemVisibleRule;
			SpecialItemRemovedRule = FallbackSpecialItemRemovedRule;
			SpecialVanillaHatRule = FallbackSpecialVanillaHatRule;
			VanillaHatMemoryRule = FallbackVanillaHatMemoryRule;
			SpecialItemMemoryRule = FallbackSpecialItemMemoryRule;
		}

		private static string Coalesce(string value, string fallback)
		{
			return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
		}
	}
	internal sealed class SpecialItemReactionService
	{
		public sealed class ResolvedSpecialItem
		{
			public string EntryId { get; set; } = "";

			public string DisplayName { get; set; } = "";

			public string ItemType { get; set; } = "";

			public string MatchedName { get; set; } = "";

			public string ReactionContext { get; set; } = "";

			public bool HasSecret { get; set; }

			public string SecretId { get; set; } = "";
		}

		private sealed class SpecialItemDefinitions
		{
			public List<string> GlobalRules { get; set; } = new List<string>();

			public Dictionary<string, SpecialItemEntry> Items { get; set; } = new Dictionary<string, SpecialItemEntry>(StringComparer.OrdinalIgnoreCase);
		}

		private sealed class SpecialItemEntry
		{
			public string DisplayName { get; set; } = "";

			public Dictionary<string, string> LocalizedNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public List<string> MatchNames { get; set; } = new List<string>();

			public List<string> MatchIds { get; set; } = new List<string>();

			public string ItemType { get; set; } = "";

			public string ReactionPriority { get; set; } = "";

			public string CoreDescription { get; set; } = "";

			public string ReactionHint { get; set; } = "";

			public string SecretId { get; set; } = "";

			public string SecretReactionHint { get; set; } = "";

			public string SecretRevealMessage { get; set; } = "";

			public Dictionary<string, SpecialItemNpcOverride> NpcOverrides { get; set; } = new Dictionary<string, SpecialItemNpcOverride>(StringComparer.OrdinalIgnoreCase);

			public List<ConditionalMatchEntry> ConditionalMatchNames { get; set; } = new List<ConditionalMatchEntry>();
		}

		private sealed class ConditionalMatchEntry
		{
			public string RequiredModId { get; set; } = "";

			public List<string> MatchNames { get; set; } = new List<string>();
		}

		private sealed class SpecialItemNpcOverride
		{
			public bool KnowsSecretByDefault { get; set; }

			public bool SecretRevealable { get; set; }

			public string ReactionHint { get; set; } = "";

			public string SecretReactionHint { get; set; } = "";
		}

		private const string SecretModDataPrefix = "NatrollEXE.OutfitReactions/Secret/";

		private readonly IModHelper helper;

		private readonly IMonitor monitor;

		private SpecialItemDefinitions definitions;

		private DateTime lastLoadedUtc = DateTime.MinValue;

		private bool missingFileLogged;

		private readonly HashSet<string> installedModIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private bool installedModIdsLoaded;

		public SpecialItemReactionService(IModHelper helper, IMonitor monitor)
		{
			this.helper = helper;
			this.monitor = monitor;
		}

		public string BuildContextForCurrentItem(string itemName, string itemType, NPC npc, string targetLanguage)
		{
			ResolvedSpecialItem resolved;
			return TryResolveItem(new string[1] { itemName }, itemType, npc, targetLanguage, out resolved) ? resolved.ReactionContext : "";
		}

		public bool TryResolveItem(IEnumerable<string> candidateNames, string itemType, NPC npc, string targetLanguage, out ResolvedSpecialItem resolved, bool wasRemoved = false)
		{
			resolved = null;
			try
			{
				if (candidateNames == null)
				{
					return false;
				}
				foreach (string candidateName in candidateNames)
				{
					if (!string.IsNullOrWhiteSpace(candidateName))
					{
						string text = candidateName.Trim();
						if (TryFindEntry(text, itemType, out var entryId, out var entry))
						{
							string text2 = ((npc != null) ? ((Character)npc).Name : null) ?? "";
							bool npcKnowsSecret = !string.IsNullOrWhiteSpace(text2) && (NpcKnowsSecret(entry.SecretId, text2) || NpcKnowsByDefault(entry, text2));
							string displayName = StringUtils.FirstNonEmpty(GetLocalizedName(entry, targetLanguage), entry.DisplayName, entryId) ?? entryId;
							string reactionContext = BuildPromptContext(entryId, entry, text2, targetLanguage, npcKnowsSecret, wasRemoved);
							resolved = new ResolvedSpecialItem
							{
								EntryId = entryId,
								DisplayName = displayName,
								ItemType = (StringUtils.FirstNonEmpty(entry.ItemType, itemType) ?? ""),
								MatchedName = text,
								ReactionContext = reactionContext,
								HasSecret = !string.IsNullOrWhiteSpace(entry.SecretId),
								SecretId = (entry.SecretId ?? "")
							};
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[SPECIAL ITEM] Error resolving special item: " + ex.Message, (LogLevel)2);
					}
				}
				resolved = null;
				return false;
			}
		}

		public string GetSecretRevealMessage(string secretId)
		{
			if (string.IsNullOrWhiteSpace(secretId))
			{
				return "";
			}
			try
			{
				SpecialItemDefinitions specialItemDefinitions = LoadDefinitions();
				if (specialItemDefinitions?.Items == null)
				{
					return "";
				}
				foreach (KeyValuePair<string, SpecialItemEntry> item in specialItemDefinitions.Items)
				{
					if (item.Value != null && string.Equals(item.Value.SecretId, secretId, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(item.Value.SecretRevealMessage))
					{
						return item.Value.SecretRevealMessage;
					}
				}
			}
			catch
			{
			}
			return "";
		}

		public bool RevealSecret(string secretId, string npcName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(secretId) || string.IsNullOrWhiteSpace(npcName) || Game1.player == null)
				{
					return false;
				}
				if (NpcKnowsSecret(secretId, npcName))
				{
					return false;
				}
				string text = "NatrollEXE.OutfitReactions/Secret/" + secretId + "/" + npcName;
				((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData)[text] = "1";
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log($"[SPECIAL ITEM] Secret '{secretId}' revealed to {npcName} via direct choice.", (LogLevel)2);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("[SPECIAL ITEM] Error in RevealSecret: " + ex.Message, (LogLevel)2);
					}
				}
				return false;
			}
		}

		public void ResetModRegistryCache()
		{
			installedModIds.Clear();
			installedModIdsLoaded = false;
		}

		public bool HasEntryForItem(string itemName, string itemType)
		{
			try
			{
				string entryId;
				SpecialItemEntry entry;
				return TryFindEntry(itemName, itemType, out entryId, out entry);
			}
			catch
			{
				return false;
			}
		}

		public bool NpcKnowsSecret(string secretId, string npcName)
		{
			if (string.IsNullOrWhiteSpace(secretId) || string.IsNullOrWhiteSpace(npcName) || Game1.player == null)
			{
				return false;
			}
			return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((Character)Game1.player).modData).ContainsKey("NatrollEXE.OutfitReactions/Secret/" + secretId + "/" + npcName);
		}

		public bool NpcAlreadyKnowsSecret(string secretId, string npcName)
		{
			if (string.IsNullOrWhiteSpace(secretId) || string.IsNullOrWhiteSpace(npcName))
			{
				return false;
			}
			if (NpcKnowsSecret(secretId, npcName))
			{
				return true;
			}
			try
			{
				SpecialItemDefinitions specialItemDefinitions = LoadDefinitions();
				if (specialItemDefinitions?.Items == null)
				{
					return false;
				}
				foreach (KeyValuePair<string, SpecialItemEntry> item in specialItemDefinitions.Items)
				{
					if (item.Value == null || !string.Equals(item.Value.SecretId, secretId, StringComparison.OrdinalIgnoreCase) || !NpcKnowsByDefault(item.Value, npcName))
					{
						continue;
					}
					if (item.Value.NpcOverrides != null && item.Value.NpcOverrides.TryGetValue(npcName, out var value) && value != null && value.SecretRevealable)
					{
						return false;
					}
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		private bool IsModInstalled(string modId)
		{
			if (string.IsNullOrWhiteSpace(modId))
			{
				return false;
			}
			if (!installedModIdsLoaded)
			{
				installedModIdsLoaded = true;
				foreach (IModInfo item in helper.ModRegistry.GetAll())
				{
					IManifest manifest = item.Manifest;
					if (!string.IsNullOrWhiteSpace((manifest != null) ? manifest.UniqueID : null))
					{
						installedModIds.Add(item.Manifest.UniqueID);
					}
				}
			}
			return installedModIds.Contains(modId);
		}

		private static bool NpcKnowsByDefault(SpecialItemEntry entry, string npcName)
		{
			if (entry?.NpcOverrides == null || string.IsNullOrWhiteSpace(npcName))
			{
				return false;
			}
			SpecialItemNpcOverride value;
			return entry.NpcOverrides.TryGetValue(npcName, out value) && value != null && value.KnowsSecretByDefault;
		}

		private bool TryFindEntry(string itemName, string itemType, out string entryId, out SpecialItemEntry entry)
		{
			entryId = "";
			entry = null;
			SpecialItemDefinitions specialItemDefinitions = LoadDefinitions();
			if (specialItemDefinitions?.Items == null || specialItemDefinitions.Items.Count == 0)
			{
				return false;
			}
			string value = NormalizeForMatch(itemName);
			string value2 = (itemType ?? "").Trim().ToLowerInvariant();
			foreach (KeyValuePair<string, SpecialItemEntry> item in specialItemDefinitions.Items)
			{
				SpecialItemEntry value3 = item.Value;
				if (value3 == null || (!string.IsNullOrWhiteSpace(value3.ItemType) && !string.IsNullOrWhiteSpace(value2) && !value3.ItemType.Trim().ToLowerInvariant().Equals(value2, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}
				foreach (string allMatchName in GetAllMatchNames(item.Key, value3))
				{
					if (string.IsNullOrWhiteSpace(allMatchName) || !NormalizeForMatch(allMatchName).Equals(value, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					entryId = item.Key;
					entry = value3;
					return true;
				}
			}
			return false;
		}

		private string BuildPromptContext(string entryId, SpecialItemEntry entry, string npcName, string targetLanguage, bool npcKnowsSecret, bool wasRemoved = false)
		{
			List<string> list = new List<string>();
			list.Add("special item: " + StringUtils.FirstNonEmpty(entry.DisplayName, entryId));
			string localizedName = GetLocalizedName(entry, targetLanguage);
			if (!string.IsNullOrWhiteSpace(localizedName) && !localizedName.Equals(entry.DisplayName, StringComparison.OrdinalIgnoreCase))
			{
				list.Add("localized name: " + localizedName);
			}
			if (!string.IsNullOrWhiteSpace(entry.ItemType))
			{
				list.Add("item type: " + entry.ItemType);
			}
			if (!string.IsNullOrWhiteSpace(entry.ReactionPriority))
			{
				list.Add("reaction priority: " + entry.ReactionPriority);
			}
			if (!string.IsNullOrWhiteSpace(entry.CoreDescription))
			{
				list.Add("description: " + entry.CoreDescription);
			}
			SpecialItemNpcOverride value = null;
			if (!string.IsNullOrWhiteSpace(npcName) && entry.NpcOverrides != null)
			{
				entry.NpcOverrides.TryGetValue(npcName, out value);
			}
			string text = (npcKnowsSecret ? StringUtils.FirstNonEmpty(value?.SecretReactionHint, value?.ReactionHint, entry.SecretReactionHint, entry.ReactionHint) : StringUtils.FirstNonEmpty(value?.ReactionHint, entry.ReactionHint));
			if (!string.IsNullOrWhiteSpace(text))
			{
				list.Add("reaction hint: " + text);
			}
			if (npcKnowsSecret && !string.IsNullOrWhiteSpace(entry.SecretId))
			{
				list.Add("secret context: this NPC already knows the secret behind this item — factor that prior knowledge into the reaction");
			}
			if (wasRemoved)
			{
				list.Add("item status: JUST REMOVED — the farmer is no longer wearing this item. React to its absence (relief, disappointment, curiosity about why it was taken off, etc.), not to it being worn right now. Do NOT describe or react as if the item is currently equipped.");
			}
			SpecialItemDefinitions specialItemDefinitions = definitions ?? new SpecialItemDefinitions();
			if (specialItemDefinitions.GlobalRules != null && specialItemDefinitions.GlobalRules.Count > 0)
			{
				string text2 = string.Join(" ", from r in specialItemDefinitions.GlobalRules.Take(3)
					where !string.IsNullOrWhiteSpace(r)
					select r);
				if (!string.IsNullOrWhiteSpace(text2))
				{
					list.Add("global rules: " + text2);
				}
			}
			return string.Join("; ", list.Where((string p) => !string.IsNullOrWhiteSpace(p)));
		}

		private SpecialItemDefinitions LoadDefinitions()
		{
			string path = Path.Combine(helper.DirectoryPath, "assets", "special-reactions", "luckypurpleshorts.json");
			try
			{
				if (!File.Exists(path))
				{
					if (!missingFileLogged)
					{
						missingFileLogged = true;
						if (ModEntry.DebugLog)
						{
							IMonitor obj = monitor;
							if (obj != null)
							{
								obj.Log("[SPECIAL ITEM] No luckypurpleshorts.json found at assets/special-reactions/luckypurpleshorts.json. Special item reactions are disabled.", (LogLevel)2);
							}
						}
					}
					definitions = null;
					return null;
				}
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
				if (definitions != null && lastWriteTimeUtc == lastLoadedUtc)
				{
					return definitions;
				}
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				string json = File.ReadAllText(path, Encoding.UTF8);
				definitions = JsonSerializer.Deserialize<SpecialItemDefinitions>(json, options) ?? new SpecialItemDefinitions();
				lastLoadedUtc = lastWriteTimeUtc;
				missingFileLogged = false;
				int value = definitions.Items?.Count ?? 0;
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log($"[SPECIAL ITEM] Loaded {value} special item definitions.", (LogLevel)2);
					}
				}
				return definitions;
			}
			catch (Exception ex)
			{
				IMonitor obj3 = monitor;
				if (obj3 != null)
				{
					obj3.Log("[SPECIAL ITEM] Failed to load luckypurpleshorts.json: " + ex.Message, (LogLevel)3);
				}
				definitions = null;
				lastLoadedUtc = DateTime.MinValue;
				return null;
			}
		}

		private IEnumerable<string> GetAllMatchNames(string entryId, SpecialItemEntry entry)
		{
			yield return entryId;
			yield return entry?.DisplayName;
			if (entry?.LocalizedNames != null)
			{
				foreach (string value in entry.LocalizedNames.Values)
				{
					yield return value;
				}
			}
			if (entry?.MatchNames != null)
			{
				foreach (string matchName in entry.MatchNames)
				{
					yield return matchName;
				}
			}
			if (entry?.MatchIds != null)
			{
				foreach (string matchId in entry.MatchIds)
				{
					yield return matchId;
				}
			}
			if (entry?.ConditionalMatchNames == null)
			{
				yield break;
			}
			foreach (ConditionalMatchEntry conditional in entry.ConditionalMatchNames)
			{
				if (conditional == null || !IsModInstalled(conditional.RequiredModId) || conditional.MatchNames == null)
				{
					continue;
				}
				foreach (string matchName2 in conditional.MatchNames)
				{
					yield return matchName2;
				}
			}
		}

		private static string NormalizeForMatch(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string text2 = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			string text3 = text2;
			foreach (char c in text3)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					if (char.IsLetterOrDigit(c))
					{
						stringBuilder.Append(c);
						flag = false;
					}
					else if (!flag)
					{
						stringBuilder.Append(' ');
						flag = true;
					}
				}
			}
			return stringBuilder.ToString().Trim().Normalize(NormalizationForm.FormC);
		}

		private static string GetLocalizedName(SpecialItemEntry entry, string targetLanguage)
		{
			if (entry?.LocalizedNames == null || entry.LocalizedNames.Count == 0)
			{
				return "";
			}
			string text = LanguageToLocalizationKey(targetLanguage);
			if (!string.IsNullOrWhiteSpace(text) && entry.LocalizedNames.TryGetValue(text, out var value) && !string.IsNullOrWhiteSpace(value))
			{
				return value;
			}
			return "";
		}

		private static string LanguageToLocalizationKey(string lang)
		{
			if (string.IsNullOrWhiteSpace(lang))
			{
				return "";
			}
			string text = lang.ToLowerInvariant();
			if (text.Contains("pt") || text.Contains("portuguese") || text.Contains("brazilian"))
			{
				return "pt-BR";
			}
			if (text.Contains("es") || text.Contains("spanish"))
			{
				return "es";
			}
			if (text.Contains("fr") || text.Contains("french"))
			{
				return "fr";
			}
			if (text.Contains("de") || text.Contains("german"))
			{
				return "de";
			}
			if (text.Contains("it") || text.Contains("italian"))
			{
				return "it";
			}
			if (text.Contains("ja") || text.Contains("japanese"))
			{
				return "ja";
			}
			if (text.Contains("ko") || text.Contains("korean"))
			{
				return "ko";
			}
			if (text.Contains("ru") || text.Contains("russian"))
			{
				return "ru";
			}
			if (text.Contains("tr") || text.Contains("turkish"))
			{
				return "tr";
			}
			if (text.Contains("zh") || text.Contains("chinese"))
			{
				return "zh";
			}
			return "";
		}
	}
	internal sealed class SpecialHatReactionService
	{
		private sealed class SpecialHatReactionDefinitions
		{
			public Dictionary<string, string> IntensityScale { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public List<string> GlobalRules { get; set; } = new List<string>();

			public Dictionary<string, string> TagGuidance { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public Dictionary<string, string> PersonalityGuidance { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public Dictionary<string, SpecialHatReactionEntry> Hats { get; set; } = new Dictionary<string, SpecialHatReactionEntry>(StringComparer.OrdinalIgnoreCase);
		}

		private sealed class SpecialHatReactionEntry
		{
			public string DisplayName { get; set; } = "";

			public Dictionary<string, string> LocalizedNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public List<string> MatchNames { get; set; } = new List<string>();

			public string Category { get; set; } = "";

			public List<string> Tags { get; set; } = new List<string>();

			public int Intensity { get; set; }

			public string ReactionPriority { get; set; } = "";

			public string CoreDescription { get; set; } = "";

			public string ReactionHint { get; set; } = "";
		}

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
				Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)farmer?.hat)?.Value;
				if (val == null)
				{
					return "";
				}
				string text = (((Item)val).DisplayName ?? "").Trim();
				string text2 = (((Item)val).Name ?? "").Trim();
				if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(text2))
				{
					return "";
				}
				if (!TryFindHatEntry(text, text2, out var entryId, out var entry))
				{
					return "";
				}
				return BuildPromptContext(entryId, entry, text, text2, targetLanguage);
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[SPECIAL HAT] Could not build special hat reaction context: " + ex.Message, (LogLevel)2);
					}
				}
				return "";
			}
		}

		public string BuildContextForRemovedHat(string removedHatName, string targetLanguage)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(removedHatName))
				{
					return "";
				}
				if (!TryFindHatEntry(removedHatName, removedHatName, out var entryId, out var entry))
				{
					return "";
				}
				string text = BuildPromptContext(entryId, entry, removedHatName, removedHatName, targetLanguage);
				if (string.IsNullOrWhiteSpace(text))
				{
					return "";
				}
				string text2 = ((string.Equals(targetLanguage, "pt", StringComparison.OrdinalIgnoreCase) || string.Equals(targetLanguage, "pt-BR", StringComparison.OrdinalIgnoreCase)) ? " CONTEXTO DA REMOÇÃO: as informações acima descrevem o chapéu que o jogador ACABOU DE TIRAR (não está mais usando). Use essa opinião/impressão ao reagir à remoção (por exemplo, alívio se era horrível, ou pena se você gostava), mas NÃO precisa dizer o nome do chapéu nem descrevê-lo em detalhe — apenas deixe a reação refletir o que você achava dele." : " REMOVAL CONTEXT: the information above describes the hat the farmer JUST TOOK OFF (no longer worn). Use that opinion/impression when reacting to the removal (e.g. relief if it was hideous, or mild disappointment if you liked it), but you do NOT need to say the hat's name or describe it in detail — just let your reaction reflect how you felt about it.");
				return text + text2;
			}
			catch (Exception ex)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[SPECIAL HAT] Could not build removed-hat reaction context: " + ex.Message, (LogLevel)2);
					}
				}
				return "";
			}
		}

		private bool TryFindHatEntry(string displayName, string internalName, out string entryId, out SpecialHatReactionEntry entry)
		{
			entryId = "";
			entry = null;
			SpecialHatReactionDefinitions specialHatReactionDefinitions = LoadDefinitions();
			if (specialHatReactionDefinitions?.Hats == null || specialHatReactionDefinitions.Hats.Count <= 0)
			{
				return false;
			}
			string actualNorm = NormalizeForMatch(displayName);
			string actualNorm2 = NormalizeForMatch(internalName);
			foreach (KeyValuePair<string, SpecialHatReactionEntry> hat in specialHatReactionDefinitions.Hats)
			{
				SpecialHatReactionEntry value = hat.Value;
				if (value == null)
				{
					continue;
				}
				foreach (string allMatchName in GetAllMatchNames(hat.Key, value))
				{
					if (string.IsNullOrWhiteSpace(allMatchName) || (!EqualsLoose(allMatchName, displayName, actualNorm) && !EqualsLoose(allMatchName, internalName, actualNorm2)))
					{
						continue;
					}
					entryId = hat.Key;
					entry = value;
					return true;
				}
			}
			return false;
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
						if (ModEntry.DebugLog)
						{
							IMonitor obj = monitor;
							if (obj != null)
							{
								obj.Log("[SPECIAL HAT] No special hat reaction file found at assets/special-reactions/hat.json. Special vanilla hat reactions are disabled.", (LogLevel)2);
							}
						}
					}
					definitions = null;
					return null;
				}
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
				if (definitions != null && lastWriteTimeUtc == lastLoadedUtc)
				{
					return definitions;
				}
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				string json = File.ReadAllText(path, Encoding.UTF8);
				definitions = JsonSerializer.Deserialize<SpecialHatReactionDefinitions>(json, options) ?? new SpecialHatReactionDefinitions();
				lastLoadedUtc = lastWriteTimeUtc;
				missingFileLogged = false;
				int num = definitions.Hats?.Count ?? 0;
				if (ModEntry.DebugLog)
				{
					IMonitor obj2 = monitor;
					if (obj2 != null)
					{
						obj2.Log("[SPECIAL HAT] Loaded " + num + " special hat reaction definitions.", (LogLevel)2);
					}
				}
				return definitions;
			}
			catch (Exception ex)
			{
				IMonitor obj3 = monitor;
				if (obj3 != null)
				{
					obj3.Log("[SPECIAL HAT] Failed to load assets/special-reactions/hat.json: " + ex.Message, (LogLevel)3);
				}
				definitions = null;
				lastLoadedUtc = DateTime.MinValue;
				return null;
			}
		}

		private string BuildPromptContext(string entryId, SpecialHatReactionEntry entry, string actualDisplayName, string actualInternalName, string targetLanguage)
		{
			SpecialHatReactionDefinitions specialHatReactionDefinitions = definitions ?? new SpecialHatReactionDefinitions();
			string localizedName = GetLocalizedName(entry, targetLanguage);
			List<string> list = new List<string>();
			list.Add("Current vanilla hat: " + StringUtils.FirstNonEmpty(actualDisplayName, actualInternalName, entry.DisplayName, entryId));
			if (!string.IsNullOrWhiteSpace(localizedName) && !localizedName.Equals(actualDisplayName, StringComparison.OrdinalIgnoreCase))
			{
				list.Add("localized hat name: " + localizedName);
			}
			list.Add("special hat entry id: " + entryId);
			if (!string.IsNullOrWhiteSpace(entry.Category))
			{
				list.Add("category: " + entry.Category);
			}
			if (entry.Tags != null && entry.Tags.Count > 0)
			{
				list.Add("tags: " + string.Join(", ", entry.Tags.Where((string tag) => !string.IsNullOrWhiteSpace(tag))));
			}
			if (entry.Intensity > 0)
			{
				string text = "";
				if (specialHatReactionDefinitions.IntensityScale != null && specialHatReactionDefinitions.IntensityScale.TryGetValue(entry.Intensity.ToString(CultureInfo.InvariantCulture), out var value))
				{
					text = " (" + value + ")";
				}
				list.Add("intensity: " + entry.Intensity.ToString(CultureInfo.InvariantCulture) + text);
			}
			if (!string.IsNullOrWhiteSpace(entry.ReactionPriority))
			{
				list.Add("reaction priority: " + entry.ReactionPriority);
			}
			if (!string.IsNullOrWhiteSpace(entry.CoreDescription))
			{
				list.Add("description: " + entry.CoreDescription);
			}
			if (!string.IsNullOrWhiteSpace(entry.ReactionHint))
			{
				list.Add("reaction hint: " + entry.ReactionHint);
			}
			string text2 = BuildRelevantTagGuidance(entry.Tags, specialHatReactionDefinitions.TagGuidance);
			if (!string.IsNullOrWhiteSpace(text2))
			{
				list.Add("relevant tag guidance: " + text2);
			}
			string text3 = BuildCompactGlobalRules(specialHatReactionDefinitions.GlobalRules, entry.Intensity);
			if (!string.IsNullOrWhiteSpace(text3))
			{
				list.Add("global handling rules: " + text3);
			}
			return string.Join("; ", list.Where((string part) => !string.IsNullOrWhiteSpace(part)));
		}

		private static string BuildRelevantTagGuidance(List<string> tags, Dictionary<string, string> tagGuidance)
		{
			if (tags == null || tags.Count <= 0 || tagGuidance == null || tagGuidance.Count <= 0)
			{
				return "";
			}
			List<string> list = new List<string>();
			foreach (string tag in tags)
			{
				if (!string.IsNullOrWhiteSpace(tag) && tagGuidance.TryGetValue(tag, out var value) && !string.IsNullOrWhiteSpace(value))
				{
					list.Add(tag + " = " + value);
				}
			}
			return string.Join("; ", list);
		}

		private static string BuildCompactGlobalRules(List<string> rules, int intensity)
		{
			if (rules == null || rules.Count <= 0)
			{
				return "";
			}
			List<string> list = new List<string>();
			if (rules.Count > 0)
			{
				list.Add(rules[0]);
			}
			if (rules.Count > 1)
			{
				list.Add(rules[1]);
			}
			if (rules.Count > 2)
			{
				list.Add(rules[2]);
			}
			if (intensity >= 4 && rules.Count > 5)
			{
				list.Add(rules[5]);
			}
			return string.Join(" ", list.Where((string rule) => !string.IsNullOrWhiteSpace(rule)));
		}

		private static IEnumerable<string> GetAllMatchNames(string entryId, SpecialHatReactionEntry entry)
		{
			yield return entryId;
			yield return entry?.DisplayName;
			if (entry?.LocalizedNames != null)
			{
				foreach (string value in entry.LocalizedNames.Values)
				{
					yield return value;
				}
			}
			if (entry?.MatchNames == null)
			{
				yield break;
			}
			foreach (string matchName in entry.MatchNames)
			{
				yield return matchName;
			}
		}

		private static bool EqualsLoose(string expected, string actual, string actualNorm)
		{
			if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
			{
				return false;
			}
			if (expected.Trim().Equals(actual.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			string text = NormalizeForMatch(expected);
			return !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(actualNorm) && text.Equals(actualNorm, StringComparison.OrdinalIgnoreCase);
		}

		private static string NormalizeForMatch(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string text2 = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			string text3 = text2;
			foreach (char c in text3)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					if (char.IsLetterOrDigit(c) || c == '?')
					{
						stringBuilder.Append(c);
						flag = false;
					}
					else if (!flag)
					{
						stringBuilder.Append(' ');
						flag = true;
					}
				}
			}
			return RegexCollapseSpaces(stringBuilder.ToString().Normalize(NormalizationForm.FormC));
		}

		private static string RegexCollapseSpaces(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			string text2 = text.Trim();
			foreach (char c in text2)
			{
				if (char.IsWhiteSpace(c))
				{
					if (!flag)
					{
						stringBuilder.Append(' ');
						flag = true;
					}
				}
				else
				{
					stringBuilder.Append(c);
					flag = false;
				}
			}
			return stringBuilder.ToString();
		}

		private static string GetLocalizedName(SpecialHatReactionEntry entry, string targetLanguage)
		{
			if (entry?.LocalizedNames == null || entry.LocalizedNames.Count <= 0)
			{
				return "";
			}
			string text = LanguageToLocalizationKey(targetLanguage);
			if (!string.IsNullOrWhiteSpace(text) && entry.LocalizedNames.TryGetValue(text, out var value) && !string.IsNullOrWhiteSpace(value))
			{
				return value;
			}
			return "";
		}

		private static string LanguageToLocalizationKey(string targetLanguage)
		{
			if (string.IsNullOrWhiteSpace(targetLanguage))
			{
				return "";
			}
			string text = targetLanguage.ToLowerInvariant();
			if (text.Contains("brazilian") || text.Contains("portuguese") || text.Contains("pt"))
			{
				return "pt-BR";
			}
			if (text.Contains("spanish") || text.Contains("es"))
			{
				return "es";
			}
			if (text.Contains("french") || text.Contains("fr"))
			{
				return "fr";
			}
			if (text.Contains("german") || text.Contains("de"))
			{
				return "de";
			}
			if (text.Contains("italian") || text.Contains("it"))
			{
				return "it";
			}
			if (text.Contains("japanese") || text.Contains("ja"))
			{
				return "ja";
			}
			if (text.Contains("korean") || text.Contains("ko"))
			{
				return "ko";
			}
			if (text.Contains("russian") || text.Contains("ru"))
			{
				return "ru";
			}
			if (text.Contains("turkish") || text.Contains("tr"))
			{
				return "tr";
			}
			if (text.Contains("chinese") || text.Contains("zh"))
			{
				return "zh";
			}
			return "";
		}
	}
	internal sealed class VoiceSampleService
	{
		private readonly IModHelper helper;

		private readonly IMonitor monitor;

		private readonly ConcurrentDictionary<string, List<(string Key, string Line)>> voiceSampleCache = new ConcurrentDictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, string> NameAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Magnus", "Wizard" },
			{ "Morris", "MorrisTod" },
			{ "Mr. Aguar", "Aguar" },
			{ "Lil Acorn", "Acorn" },
			{ "Hank", "HankSVE" },
			{ "Elias", "EliasSBV" },
			{ "Amina", "AminaSBV" },
			{ "Moon", "MoonSBV" },
			{ "Pan", "PanSBV" },
			{ "Raccoon", "RaccoonSBV" },
			{ "Aicha", "AichaSBV" },
			{ "Ari", "AriSBV" },
			{ "Blake", "BlakeSBV" },
			{ "Derya", "DeryaSBV" },
			{ "Diala", "DialaSBV" },
			{ "Ezra", "EzraSBV" },
			{ "Iman", "ImanSBV" },
			{ "Jumana", "JumanaSBV" },
			{ "Lyenne", "LyenneSBV" },
			{ "Maia", "MaiaSBV" },
			{ "Miyoung", "MiyoungSBV" },
			{ "Nadia", "NadiaSBV" },
			{ "Ophelia", "OpheliaSBV" },
			{ "Reihana", "ReihanaSBV" },
			{ "Silas", "SilasSBV" },
			{ "Victoria", "ToriLK" },
			{ "Corwin", "CorwinLK" },
			{ "Kataryna", "KatarynaLK" },
			{ "Vivienne", "VivienneLK" },
			{ "Dale", "DaleWaede" },
			{ "Keanu", "KeanuAvis" },
			{ "Edith", "EdithHart" },
			{ "Ethan", "EthanHart" },
			{ "Michael", "MichaelHart" },
			{ "Stella", "StellaHart" },
			{ "Eyvind", "Eyvinder" },
			{ "Lexi", "Leximonster" },
			{ "Luma", "LumaJunimo" },
			{ "Josephine", "JosephineK" },
			{ "Oliver", "OliverK" },
			{ "Jade", "JadeMalic" },
			{ "Tristan", "TristanLK" },
			{ "Eli", "Nova.Eli" },
			{ "Dylan", "Nova.Dylan" }
		};

		public VoiceSampleService(IModHelper helper, IMonitor monitor)
		{
			this.helper = helper;
			this.monitor = monitor;
		}

		public string ResolveInternalName(string displayName)
		{
			if (!string.IsNullOrWhiteSpace(displayName) && NameAliases.TryGetValue(displayName, out var value) && !string.IsNullOrWhiteSpace(value))
			{
				return value;
			}
			return displayName;
		}

		public bool TryReverseAlias(string internalName, out string displayName)
		{
			displayName = null;
			if (string.IsNullOrWhiteSpace(internalName))
			{
				return false;
			}
			foreach (KeyValuePair<string, string> nameAlias in NameAliases)
			{
				if (string.Equals(nameAlias.Value, internalName, StringComparison.OrdinalIgnoreCase))
				{
					displayName = nameAlias.Key;
					return true;
				}
			}
			return false;
		}

		public void AppendToPrompt(StringBuilder builder, OutfitAiContext context, ModConfig config)
		{
			try
			{
				if (config == null || !config.UseVoiceSamples || context == null || string.IsNullOrWhiteSpace(context.NpcName) || IsExcluded(context.NpcName, config.VoiceSampleExcludedNpcs))
				{
					return;
				}
				int count = Math.Clamp(config.VoiceSampleCount, 1, 20);
				List<string> cachedSamplesForNpc = GetCachedSamplesForNpc(context.NpcName, count, context?.Season);
				if (cachedSamplesForNpc == null || cachedSamplesForNpc.Count == 0)
				{
					return;
				}
				if (monitor != null)
				{
					if (ModEntry.DebugLog)
					{
						monitor.Log($"[VoiceSamples] {context.NpcName} ({context.NpcDisplayName}): injecting {cachedSamplesForNpc.Count} voice sample line(s) into this prompt.", (LogLevel)2);
					}
					for (int i = 0; i < cachedSamplesForNpc.Count; i++)
					{
						string text = cachedSamplesForNpc[i];
						if (text != null && text.Length > 120)
						{
							text = text.Substring(0, 120) + "...";
						}
						monitor.Log($"[VoiceSamples]   {i + 1}. {text}", (LogLevel)0);
					}
				}
				builder.AppendLine("VOICE REFERENCE (real in-game lines from " + context.NpcDisplayName + ", for TONE only):");
				builder.AppendLine("These are examples of how this character actually talks. Match their voice, rhythm, vocabulary, humor, and attitude. Do NOT copy, quote, translate, or reuse their content, topics, or sentences; they are not about the outfit. The personality above always wins if anything conflicts.");
				foreach (string item in cachedSamplesForNpc)
				{
					builder.AppendLine("- " + item);
				}
				builder.AppendLine();
			}
			catch (Exception ex)
			{
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log("Voice samples skipped for " + (context?.NpcName ?? "?") + ": " + ex.Message, (LogLevel)0);
				}
			}
		}

		public static bool IsExcluded(string npcName, string excludedCsv)
		{
			if (string.IsNullOrWhiteSpace(excludedCsv))
			{
				return false;
			}
			string[] array = excludedCsv.Split(',');
			foreach (string text in array)
			{
				if (string.Equals(text.Trim(), npcName.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public void PrepareSamplesForNpc(string npcName, ModConfig config)
		{
			if (config == null || !config.UseVoiceSamples || string.IsNullOrWhiteSpace(npcName) || IsExcluded(npcName, config.VoiceSampleExcludedNpcs) || voiceSampleCache.ContainsKey(npcName))
			{
				return;
			}
			List<(string, string)> list = LoadAndCleanDialogueLines(npcName);
			if (!voiceSampleCache.TryAdd(npcName, list))
			{
				return;
			}
			if (list.Count > 0)
			{
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log($"[VoiceSamples] {npcName}: {list.Count} usable line(s) found (will pick the configured amount).", (LogLevel)2);
					}
				}
			}
			else
			{
				IMonitor obj2 = monitor;
				if (obj2 != null)
				{
					obj2.Log("[VoiceSamples] " + npcName + ": NO usable lines found. This NPC will rely on its profile only (no voice samples). If this NPC is from a mod, its internal name may differ from the profile's NpcName, or it may not use the standard dialogue file.", (LogLevel)3);
				}
			}
		}

		private List<string> GetCachedSamplesForNpc(string npcName, int count, string season)
		{
			if (!voiceSampleCache.TryGetValue(npcName, out List<(string, string)> value))
			{
				return new List<string>();
			}
			if (value.Count == 0)
			{
				return new List<string>();
			}
			if (value.Count <= count)
			{
				return value.Select<(string, string), string>(((string Key, string Line) p) => p.Line).ToList();
			}
			string seasonLower = season?.Trim().ToLowerInvariant() ?? "";
			return (from x in (from p in value
					select new
					{
						Line = p.Line,
						Score = ScoreLine(p.Key, p.Line, seasonLower)
					} into x
					orderby x.Score descending
					select x).Take(count)
				select x.Line).ToList();
		}

		private static int ScoreLine(string key, string line, string currentSeasonLower)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				return int.MinValue;
			}
			int num = 0;
			int length = line.Length;
			num = ((length >= 40 && length <= 160) ? (num + 3) : ((length > 160 && length <= 220) ? (num + 2) : ((length < 20 || length >= 40) ? (num - 1) : (num + 1))));
			if (line.Contains("..."))
			{
				num++;
			}
			if (line.Contains("!"))
			{
				num++;
			}
			if (line.Contains("?"))
			{
				num++;
			}
			if (line.Contains(","))
			{
				num++;
			}
			if (line.IndexOf("haha", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("heh", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				num++;
			}
			if (!string.IsNullOrEmpty(currentSeasonLower) && !string.IsNullOrEmpty(key) && key.ToLowerInvariant().StartsWith(currentSeasonLower))
			{
				num += 2;
			}
			int num2 = line.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
			if (num2 <= 3)
			{
				num -= 2;
			}
			return num;
		}

		public string BuildReport(IEnumerable<string> profileNames, ModConfig config)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=== Voice Sample Report ===");
			if (config == null)
			{
				config = new ModConfig();
			}
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(42, 3, stringBuilder2);
			handler.AppendLiteral("UseVoiceSamples: ");
			handler.AppendFormatted(config.UseVoiceSamples);
			handler.AppendLiteral(" | Count: ");
			handler.AppendFormatted(config.VoiceSampleCount);
			handler.AppendLiteral(" | Excluded: \"");
			handler.AppendFormatted(config.VoiceSampleExcludedNpcs);
			handler.AppendLiteral("\"");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder.AppendLine("(This reads dialogue currently loaded at Characters/Dialogue/<Name>, so mod-replaced lines are included.)");
			stringBuilder.AppendLine();
			List<string> list = (profileNames ?? Enumerable.Empty<string>()).OrderBy<string, string>((string x) => x, StringComparer.OrdinalIgnoreCase).ToList();
			if (list.Count == 0)
			{
				stringBuilder.AppendLine("No NPC profiles are loaded.");
				return stringBuilder.ToString();
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (string item in list)
			{
				if (IsExcluded(item, config.VoiceSampleExcludedNpcs))
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
					handler.AppendLiteral("  [EXCLUDED] ");
					handler.AppendFormatted(item);
					stringBuilder4.AppendLine(ref handler);
					num3++;
					continue;
				}
				List<(string, string)> list2;
				try
				{
					list2 = LoadAndCleanDialogueLines(item);
				}
				catch (Exception ex)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
					handler.AppendLiteral("  [ERROR]    ");
					handler.AppendFormatted(item);
					handler.AppendLiteral(": ");
					handler.AppendFormatted(ex.Message);
					stringBuilder5.AppendLine(ref handler);
					num2++;
					continue;
				}
				if (list2.Count > 0)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(30, 2, stringBuilder2);
					handler.AppendLiteral("  [OK]       ");
					handler.AppendFormatted(item);
					handler.AppendLiteral(": ");
					handler.AppendFormatted(list2.Count);
					handler.AppendLiteral(" usable line(s)");
					stringBuilder6.AppendLine(ref handler);
					num++;
				}
				else
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder7 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(87, 1, stringBuilder2);
					handler.AppendLiteral("  [NONE]     ");
					handler.AppendFormatted(item);
					handler.AppendLiteral(": no usable lines (profile-only; check internal name if this is a mod NPC)");
					stringBuilder7.AppendLine(ref handler);
					num2++;
				}
			}
			stringBuilder.AppendLine();
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(62, 4, stringBuilder2);
			handler.AppendLiteral("Summary: ");
			handler.AppendFormatted(num);
			handler.AppendLiteral(" with samples, ");
			handler.AppendFormatted(num2);
			handler.AppendLiteral(" without, ");
			handler.AppendFormatted(num3);
			handler.AppendLiteral(" excluded, out of ");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" profiles.");
			stringBuilder8.AppendLine(ref handler);
			return stringBuilder.ToString();
		}

		public string BuildPreview(string requestedName, string resolvedName, bool profileMatched, string currentSeason, ModConfig config)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=== Voice Sample Preview ===");
			if (string.IsNullOrWhiteSpace(requestedName))
			{
				stringBuilder.AppendLine("Usage: oc_preview_voicesamples <NpcName>  (e.g. oc_preview_voicesamples Victoria)");
				return stringBuilder.ToString();
			}
			if (config == null)
			{
				config = new ModConfig();
			}
			string text = (string.IsNullOrWhiteSpace(resolvedName) ? requestedName : resolvedName);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(31, 2, stringBuilder2);
			handler.AppendLiteral("Requested: \"");
			handler.AppendFormatted(requestedName);
			handler.AppendLiteral("\" | Profile match: ");
			handler.AppendFormatted(profileMatched ? ("yes (" + text + ")") : "NONE");
			stringBuilder3.AppendLine(ref handler);
			string value = ResolveInternalName(text);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(43, 1, stringBuilder2);
			handler.AppendLiteral("Reading dialogue from: Characters/Dialogue/");
			handler.AppendFormatted(value);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(27, 2, stringBuilder2);
			handler.AppendLiteral("UseVoiceSamples: ");
			handler.AppendFormatted(config.UseVoiceSamples);
			handler.AppendLiteral(" | Count: ");
			handler.AppendFormatted(config.VoiceSampleCount);
			stringBuilder5.AppendLine(ref handler);
			if (IsExcluded(text, config.VoiceSampleExcludedNpcs))
			{
				stringBuilder.AppendLine("NOTE: this NPC is in the exclusion list, so no samples are injected in-game.");
			}
			stringBuilder.AppendLine();
			int num = Math.Clamp(config.VoiceSampleCount, 1, 20);
			List<(string, string)> list;
			try
			{
				list = LoadAndCleanDialogueLines(text);
			}
			catch (Exception ex)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(33, 1, stringBuilder2);
				handler.AppendLiteral("[ERROR] Could not read dialogue: ");
				handler.AppendFormatted(ex.Message);
				stringBuilder6.AppendLine(ref handler);
				return stringBuilder.ToString();
			}
			if (list == null || list.Count == 0)
			{
				stringBuilder.AppendLine("NO usable lines found. The NPC will rely on its profile only.");
				stringBuilder.AppendLine("Diagnosis: " + ProbeDialogueAsset(ResolveInternalName(text)));
				stringBuilder.AppendLine("If this is a mod NPC, its internal name may differ from the profile's NpcName (add an alias), or it may not use the standard dialogue file.");
				return stringBuilder.ToString();
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(71, 2, stringBuilder2);
			handler.AppendLiteral("Usable pool: ");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" line(s). These are the ");
			handler.AppendFormatted(Math.Min(num, list.Count));
			handler.AppendLiteral(" that would be injected right now:");
			stringBuilder7.AppendLine(ref handler);
			stringBuilder.AppendLine();
			string seasonLower = currentSeason?.Trim().ToLowerInvariant() ?? "";
			List<string> list2 = ((list.Count <= num) ? list.Select<(string, string), string>(((string Key, string Line) p) => p.Line).ToList() : (from x in (from p in list
					select new
					{
						Line = p.Line,
						Score = ScoreLine(p.Key, p.Line, seasonLower)
					} into x
					orderby x.Score descending
					select x).Take(num)
				select x.Line).ToList());
			int num2 = 1;
			foreach (string item in list2)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
				handler.AppendLiteral("  ");
				handler.AppendFormatted(num2++);
				handler.AppendLiteral(". ");
				handler.AppendFormatted(item);
				stringBuilder8.AppendLine(ref handler);
			}
			return stringBuilder.ToString();
		}

		private string ProbeDialogueAsset(string internalName)
		{
			string text = "Characters/Dialogue/" + internalName;
			Dictionary<string, string> dictionary;
			try
			{
				dictionary = helper.GameContent.Load<Dictionary<string, string>>(text);
			}
			catch (Exception ex)
			{
				return $"asset '{text}' does NOT exist / failed to load ({ex.GetType().Name}). " + "The mod likely doesn't register this exact asset in the current game state, or uses a different internal name. This is a content/availability issue, not a bug in this mod.";
			}
			if (dictionary == null || dictionary.Count == 0)
			{
				return "asset '" + text + "' loaded but is EMPTY (0 entries).";
			}
			return $"asset '{text}' exists with {dictionary.Count} raw entr(y/ies), but ALL were filtered out " + "(non-sample keys, player-narration %, or length limits). If this NPC's lines look fine, the filters may be too strict for this mod.";
		}

		private List<(string Key, string Line)> LoadAndCleanDialogueLines(string npcName)
		{
			List<(string, string)> list = new List<(string, string)>();
			string text = npcName;
			if (NameAliases.TryGetValue(npcName, out var value) && !string.IsNullOrWhiteSpace(value))
			{
				text = value;
				IMonitor obj = monitor;
				if (obj != null)
				{
					obj.Log($"[VoiceSamples] {npcName}: reading dialogue from internal name 'Characters/Dialogue/{text}' (alias).", (LogLevel)0);
				}
			}
			Dictionary<string, string> dictionary = null;
			try
			{
				dictionary = helper.GameContent.Load<Dictionary<string, string>>("Characters/Dialogue/" + text);
			}
			catch (Exception ex)
			{
				IMonitor obj2 = monitor;
				if (obj2 != null)
				{
					obj2.Log($"[VoiceSamples] {npcName}: no dialogue asset at 'Characters/Dialogue/{text}' ({ex.GetType().Name}).", (LogLevel)0);
				}
				return list;
			}
			if (dictionary == null || dictionary.Count == 0)
			{
				IMonitor obj3 = monitor;
				if (obj3 != null)
				{
					obj3.Log($"[VoiceSamples] {npcName}: dialogue asset 'Characters/Dialogue/{text}' loaded but is EMPTY.", (LogLevel)0);
				}
				return list;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (LooksLikeNonSampleKey(item.Key))
				{
					continue;
				}
				foreach (string item2 in SplitDialogueIntoLines(item.Value))
				{
					if (LooksLikePlayerNarration(item2))
					{
						continue;
					}
					string text2 = CleanDialogueLineForSample(item2);
					if (!string.IsNullOrWhiteSpace(text2) && text2.Length >= 12 && text2.Length <= 220 && hashSet.Add(text2))
					{
						list.Add((item.Key, text2));
						if (list.Count >= 2000)
						{
							return list;
						}
					}
				}
			}
			return list;
		}

		private static bool LooksLikeNonSampleKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return true;
			}
			string text = key.ToLowerInvariant();
			string[] array = new string[18]
			{
				"acceptgift", "rejectgift", "birthday", "event_", "resort", "schedule", "mountain_", "introduction", "datingmemory", "memory",
				"funleave", "funreturn", "spousepatio", "spouseroom", "wedding", "divorce", "jealous", "molestress"
			};
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (text.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static IEnumerable<string> SplitDialogueIntoLines(string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				string[] parts = value.Split(new string[3] { "#$b#", "#$e#", "#$d#" }, StringSplitOptions.RemoveEmptyEntries);
				string[] array = parts;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
			}
		}

		private static bool LooksLikePlayerNarration(string rawSegment)
		{
			if (string.IsNullOrWhiteSpace(rawSegment))
			{
				return false;
			}
			string text = rawSegment.TrimStart();
			text = text.TrimStart('"', '\'', ' ', '\t');
			if (!text.StartsWith("%", StringComparison.Ordinal))
			{
				return false;
			}
			if (LooksLikeSpeakerNamePrefix(text))
			{
				return false;
			}
			return true;
		}

		private static bool LooksLikeSpeakerNamePrefix(string trimmedSegment)
		{
			if (string.IsNullOrEmpty(trimmedSegment) || trimmedSegment[0] != '%')
			{
				return false;
			}
			return Regex.IsMatch(trimmedSegment, "^%[\\p{L}][\\p{L} .'-]{0,20}:");
		}

		private static string CleanDialogueLineForSample(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string text2 = text;
			text2 = Regex.Replace(text2.TrimStart(), "^%[\\p{L}][\\p{L} .'-]{0,20}:\\s*", "");
			int num = text2.IndexOf("#$", StringComparison.Ordinal);
			if (num >= 0)
			{
				text2 = text2.Substring(0, num);
			}
			text2 = Regex.Replace(text2, "\\$[a-zA-Z]+\\b", " ");
			text2 = Regex.Replace(text2, "\\$-?\\d+", " ");
			text2 = Regex.Replace(text2, "\\{\\{.*?\\}\\}", " ");
			text2 = Regex.Replace(text2, "\\$\\{.*?\\}", " ");
			text2 = Regex.Replace(text2, "%[a-zA-Z]+", " ");
			text2 = text2.Replace("#$b#", " ").Replace("@", "").Replace("^", " ");
			text2 = Regex.Replace(text2, "\\$[a-z]\\b", " ", RegexOptions.IgnoreCase);
			text2 = Regex.Replace(text2, "\\s+", " ").Trim();
			return text2.Trim('"', '\'', '*', ' ');
		}
	}
	internal static class DialogueValidator
	{
		public static string ValidateGeneratedDialogueText(string text, OutfitAiContext context, ModConfig config, ActiveAiSettings ai, int minLengthTarget)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "empty dialogue text";
			}
			string text2 = ValidateConfiguredMinimumLength(text, config, ai, minLengthTarget);
			if (!string.IsNullOrWhiteSpace(text2))
			{
				return text2;
			}
			string text3 = ValidateDialogueBoxPacing(text, config, ai);
			if (!string.IsNullOrWhiteSpace(text3))
			{
				return text3;
			}
			if (LooksLikeCopiedFormatExample(text))
			{
				return "copied prompt format example";
			}
			if (LooksLikeInstructionLeak(text))
			{
				return "prompt/instruction leak";
			}
			if (ContainsTechnicalContextLabelLeak(text))
			{
				return "technical context label leaked into dialogue";
			}
			if (ContainsForbiddenOutfitLiteral(text, context))
			{
				return "saved outfit name was mentioned literally";
			}
			string text4 = ValidateRecognizableThemeSpecificity(text, context);
			if (!string.IsNullOrWhiteSpace(text4))
			{
				return text4;
			}
			string text5 = ValidateAccessoryOutfitCombinationSpecificity(text, context);
			if (!string.IsNullOrWhiteSpace(text5))
			{
				return text5;
			}
			string text6 = ValidatePrivateRevealingFlusterCue(text, context);
			if (!string.IsNullOrWhiteSpace(text6))
			{
				return text6;
			}
			string text7 = ValidatePlayerGenderTerms(text, context);
			if (!string.IsNullOrWhiteSpace(text7))
			{
				return text7;
			}
			string text8 = DetectLikelyWrongLanguage(text, context?.TargetLanguage);
			if (!string.IsNullOrWhiteSpace(text8))
			{
				return text8;
			}
			return null;
		}

		private static string ValidatePlayerGenderTerms(string text, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(text) || context == null)
			{
				return null;
			}
			string text2 = (context.PlayerGender ?? "").Trim().ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(text2))
			{
				return null;
			}
			string source = " " + StripDialogueMarkup(text).ToLowerInvariant() + " ";
			switch (text2)
			{
			default:
				if (!(text2 == "mulher"))
				{
					switch (text2)
					{
					default:
						if (!(text2 == "homem"))
						{
							break;
						}
						goto case "male";
					case "male":
					case "masculine":
					case "man":
					{
						string[] terms = new string[10] { " minha moça ", " minha moca ", " moça ", " moca ", " garota ", " menina ", " senhora ", " mulher ", " lady ", " girl " };
						if (ContainsAny(source, terms))
						{
							return "dialogue used feminine address for a male farmer/player";
						}
						break;
					}
					}
					break;
				}
				goto case "female";
			case "female":
			case "feminine":
			case "woman":
			{
				string[] terms2 = new string[16]
				{
					" meu rapaz ", " rapaz ", " garoto ", " menino ", " moço ", " moco ", " senhor ", " homem ", " meu cara ", " cara,",
					" cara. ", " brother ", " bro ", " dude ", " boy ", " man "
				};
				if (ContainsAny(source, terms2))
				{
					return "dialogue used masculine address for a female farmer/player";
				}
				break;
			}
			}
			return null;
		}

		public static string ValidateDialogueBoxPacing(string text, ModConfig config, ActiveAiSettings ai)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			string[] array = (from b in text.Split(new string[1] { "#$b#" }, StringSplitOptions.None)
				select StripDialogueMarkup(b).Trim() into b
				where !string.IsNullOrWhiteSpace(b)
				select b).ToArray();
			if (array.Length == 0)
			{
				return null;
			}
			int num = CountVisibleDialogueCharacters(text);
			int num2 = array.Select(CountVisibleDialogueCharacters).DefaultIfEmpty(0).Max();
			if (array.Length == 1 && num >= 360 && num2 >= 330)
			{
				return "very long one-box dialogue should contain at least one #$b# break for Stardew pacing";
			}
			return null;
		}

		public static string ValidateRecognizableThemeSpecificity(string text, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(text) || !HasRecognizableThemeClue(context))
			{
				return null;
			}
			string text2 = StripDialogueMarkup(text);
			string source = " " + text2.ToLowerInvariant() + " ";
			if (!ContainsAny(source, "combina com você", "combina com voce", "combina contigo", "fica bem em você", "fica bem em voce", "ficou bem em você", "ficou bem em voce", "ficou legal em você", "ficou legal em voce", "fica legal em você", "fica legal em voce", "combina demais com você", "combina demais com voce", "estranhamente legal", "curiosamente legal"))
			{
				return null;
			}
			if (text2.Contains("?") || ContainsAny(source, "por que", "pra quê", "pra que", "fantasia", "evento", "festival", "aposta", "mascote", "cosplay", "saloon", "mina", "caverna", "slime", "monstro", "fazenda", "galinha", "vaca", "cabra", "porco", "plantação", "plantacao", "colheita", "se você", "se voce", "imagino", "imaginar", "dá pra imaginar", "da pra imaginar", "parece que", "parece roupa", "assustar", "fugiu", "missão", "missao", "horrorosa", "horrível", "horrivel", "feia", "ridícula", "ridicula", "engraçada", "engracada", "absurda", "bizarra", "suspeita"))
			{
				return null;
			}
			return "recognizable theme reaction was too generic; add a joke, question, imagined scenario, or stronger in-character reaction drawn from this NPC's own personality instead of just saying it suits the farmer";
		}

		public static string ValidateAccessoryOutfitCombinationSpecificity(string text, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(text) || context == null || !context.IsAccessoryChange)
			{
				return null;
			}
			if (!HasRecognizableThemeClue(context) || !HasRecognizableOutfitThemeClue(context))
			{
				return null;
			}
			string source = string.Join(" ", context.SafeNoticedChangeHint, context.NoticedChangeName, context.NoticedChangeType).ToLowerInvariant();
			if (!ContainsAny(source, "wing", "wings", "asa", "asas", "angel", "anjo", "fairy", "fada", "cape", "capa", "backpack", "mochila", "umbrella", "guarda-chuva", "tail", "cauda", "horn", "horns", "chifre", "chifres", "halo", "bag", "bolsa", "shield", "escudo", "weapon", "sword", "espada"))
			{
				return null;
			}
			string text2 = StripDialogueMarkup(text);
			string source2 = " " + text2.ToLowerInvariant() + " ";
			if (ContainsAny(source2, "pikachu", "pokemon", "pokémon", "sanrio", "my melody", "mymelody", "fantasia", "mascote", "cosplay", "personagem", "bicho", "animal", "lagarto", "lizard", "dinossauro", "dinosaur", "sapo", "frog", "gato", "cat", "coelho", "rabbit", "roupa", "visual", "look", "junto", "junto com", "por cima", "em cima", "com essa", "com esse", "mistura", "misturou", "combinação", "combinacao", "híbrido", "hibrido", "não existe", "nao existe", "agora tem", "ganhou asa", "ganhou asas", "asas em", "asa em", "com asas", "com asa", "sem asas", "voar", "fazenda", "galinha", "vaca", "slime", "mina", "saloon", "festival"))
			{
				return null;
			}
			return "accessory reaction ignored the existing themed outfit; compare the accessory with the saved outfit/theme or react to the combined look instead of only describing the accessory";
		}

		private static bool HasRecognizableOutfitThemeClue(OutfitAiContext context)
		{
			if (context == null)
			{
				return false;
			}
			string source = string.Join(" ", context.SafeOutfitHint, context.ThemeContext, context.ThemePriorityInstruction, context.DialogueKey, context.OutfitName).ToLowerInvariant();
			return ContainsRecognizableClueTerm(source, "sanrio", "my melody", "mymelody", "kuromi", "hello kitty", "cinnamoroll", "keroppi", "pikachu", "pokemon", "pokémon", "eevee", "jigglypuff", "charmander", "bulbasaur", "squirtle", "lizard", "lagarto", "dinosaur", "dinossauro", "dino", "frog", "sapo", "cat", "gato", "rabbit", "coelho", "bunny", "urso", "bear", "fox", "raposa", "wolf", "lobo", "chicken", "galinha", "cow", "vaca", "goat", "cabra", "pig", "porco", "fairy", "fada", "witch", "bruxa", "vampire", "vampiro", "angel", "anjo", "demon", "demônio", "demonio", "mermaid", "sereia", "slime", "monster", "monstro", "mascot", "mascote", "cosplay", "strawberry", "morango", "orange", "laranja", "chocolate", "coffee", "café", "cafe", "cake", "bolo", "candy", "doce", "pumpkin", "abóbora", "abobora", "halloween", "christmas", "natal");
		}

		private static bool HasRecognizableThemeClue(OutfitAiContext context)
		{
			if (context == null)
			{
				return false;
			}
			string source = string.Join(" ", context.SafeOutfitHint, context.SafeNoticedChangeHint, context.ThemeContext, context.ThemePriorityInstruction, context.DialogueKey, context.OutfitName, context.NoticedChangeName, context.NoticedChangeType).ToLowerInvariant();
			return ContainsRecognizableClueTerm(source, "sanrio", "my melody", "mymelody", "kuromi", "hello kitty", "cinnamoroll", "keroppi", "pikachu", "pokemon", "pokémon", "eevee", "jigglypuff", "charmander", "bulbasaur", "squirtle", "lizard", "lagarto", "dinosaur", "dinossauro", "dino", "frog", "sapo", "cat", "gato", "rabbit", "coelho", "bunny", "urso", "bear", "fox", "raposa", "wolf", "lobo", "chicken", "galinha", "cow", "vaca", "goat", "cabra", "pig", "porco", "fairy", "fada", "witch", "bruxa", "vampire", "vampiro", "angel", "anjo", "demon", "demônio", "demonio", "mermaid", "sereia", "slime", "monster", "monstro", "mascot", "mascote", "cosplay", "strawberry", "morango", "orange", "laranja", "chocolate", "coffee", "café", "cafe", "cake", "bolo", "candy", "doce", "pumpkin", "abóbora", "abobora", "halloween", "christmas", "natal");
		}

		private static bool ContainsRecognizableClueTerm(string source, params string[] terms)
		{
			if (string.IsNullOrWhiteSpace(source) || terms == null)
			{
				return false;
			}
			foreach (string text in terms)
			{
				string text2 = (text ?? "").Trim();
				if (text2.Length <= 0)
				{
					continue;
				}
				if (text2.Length <= 4 && Regex.IsMatch(text2, "^[a-z0-9]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
				{
					if (Regex.IsMatch(source, "(?<![A-Za-z0-9_])" + Regex.Escape(text2) + "(?![A-Za-z0-9_])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
					{
						return true;
					}
				}
				else if (source.Contains(text2, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private static bool ShouldRequireDialogueBreak(string text, ModConfig config, ActiveAiSettings ai)
		{
			int num = CountVisibleDialogueCharacters(text);
			return num >= 360;
		}

		private static string ValidateConfiguredMinimumLength(string text, ModConfig config, ActiveAiSettings ai, int minLengthTarget)
		{
			if (string.IsNullOrWhiteSpace(text) || config == null)
			{
				return null;
			}
			int num = Math.Max(0, config.AiMinimumCharacters);
			if (num <= 0)
			{
				return null;
			}
			int num2 = minLengthTarget;
			if (num2 <= 0)
			{
				return null;
			}
			string text2 = StripDialogueMarkup(text);
			int num3 = CountVisibleDialogueCharacters(text2);
			int num4 = Math.Max((num2 >= 300) ? 70 : ((num2 >= 180) ? 45 : 25), (int)Math.Round((double)num2 * 0.18));
			int num5 = Math.Max(40, num2 - num4);
			if (num3 < num5)
			{
				return "dialogue was too short for configured minimum (" + num3 + "/" + num2 + " visible characters; requested " + num + ", retry threshold " + num5 + ")";
			}
			return null;
		}

		private static int CountVisibleDialogueCharacters(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return 0;
			}
			string input = StripDialogueMarkup(text);
			input = Regex.Replace(input, "\\s+", " ").Trim();
			return input.Length;
		}

		private static string ValidatePrivateRevealingFlusterCue(string text, OutfitAiContext context)
		{
			return null;
		}

		private static bool ContainsForbiddenOutfitLiteral(string text, OutfitAiContext context)
		{
			if (string.IsNullOrWhiteSpace(text) || context == null)
			{
				return false;
			}
			foreach (string item in BuildForbiddenOutfitLiteralCandidates(context))
			{
				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}
				string text2 = item.Trim();
				if (text2.Length >= 3 && LooksLikeTechnicalOrOverSpecificOutfitName(text2))
				{
					string pattern = "(?<![\\p{L}\\p{N}])" + Regex.Escape(text2) + "(?![\\p{L}\\p{N}])";
					if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool LooksLikeTechnicalOrOverSpecificOutfitName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string text = value.Trim();
			string text2 = text.ToLowerInvariant();
			if (Regex.IsMatch(text, "[\\[\\]{}]|\\.json|\\.png|\\.pack|\\.content|\\.cp", RegexOptions.IgnoreCase))
			{
				return true;
			}
			if (Regex.IsMatch(text, "[_/\\\\]|--|::|\\$|#", RegexOptions.CultureInvariant))
			{
				return true;
			}
			if (Regex.IsMatch(text, "[a-zà-ÿ][A-Z]"))
			{
				return true;
			}
			string[] array = new string[11]
			{
				"npcroom", "dialoguekey", "textsource", "content pack", "fashion sense", "internal", "variant:", "theme:", "category:", "dialogue:",
				"preset:"
			};
			string[] array2 = array;
			foreach (string value2 in array2)
			{
				if (text2.Contains(value2))
				{
					return true;
				}
			}
			if (Regex.IsMatch(text, "\\b(v\\d+|id\\s*\\d+|key\\s*\\d+)\\b", RegexOptions.IgnoreCase))
			{
				return true;
			}
			return false;
		}

		private static bool ContainsTechnicalContextLabelLeak(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string input = " " + StripDialogueMarkup(text).ToLowerInvariant() + " ";
			input = Regex.Replace(input, "\\s+", " ");
			string[] array = new string[23]
			{
				" indoor ", " outdoor ", " npc room ", " npcroom ", " location type ", " tipo de local ", " theme guidance ", " orientação de tema ", " orientacao de tema ", " dialogue category ",
				" outfit category ", " categoria de diálogo ", " categoria de dialogo ", " categoria da roupa ", " summer indoor ", " verão indoor ", " verao indoor ", " indoor theme ", " tema indoor ", " inside variant ",
				" outside variant ", " npc-specific ", " textsource "
			};
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (input.Contains(value))
				{
					return true;
				}
			}
			if ((input.Contains(" theme ") || input.Contains(" tema ")) && input.Contains(" indoor"))
			{
				return true;
			}
			return false;
		}

		private static IEnumerable<string> BuildForbiddenOutfitLiteralCandidates(OutfitAiContext context)
		{
			HashSet<string> values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Add(context.OutfitName);
			if (!string.IsNullOrWhiteSpace(context.OutfitName))
			{
				string input = Regex.Replace(context.OutfitName, "[_\\-.]+", " ");
				input = Regex.Replace(input, "([a-zà-ÿ])([A-Z])", "$1 $2");
				input = Regex.Replace(input, "\\s{2,}", " ").Trim();
				Add(input);
			}
			return values;
			void Add(string value)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					value = Regex.Replace(value.Trim(), "\\s+", " ");
					if (value.Length >= 3)
					{
						values.Add(value);
					}
				}
			}
		}

		private static string DetectLikelyWrongLanguage(string text, string targetLanguage)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(targetLanguage))
			{
				return null;
			}
			string text2 = Regex.Replace(text, "#\\$b#|\\$[a-z0-9]+|\\{\\{.*?\\}\\}", " ", RegexOptions.IgnoreCase);
			string text3 = " " + text2.ToLowerInvariant() + " ";
			if (targetLanguage.IndexOf("English", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				int num = 0;
				if (Regex.IsMatch(text3, "[ãõçáéíóúâêôà]"))
				{
					num += 2;
				}
				string[] array = new string[23]
				{
					" você ", " voce ", " está ", " esta ", " isso ", " aqui ", " roupa ", " visual ", " combina ", " gostei ",
					" natal ", " primavera ", " inverno ", " verão ", " verao ", " outono ", " estranho ", " usar ", " seu ", " sua ",
					" que ", " tem ", " toque "
				};
				string[] array2 = array;
				foreach (string value in array2)
				{
					if (text3.Contains(value))
					{
						num++;
					}
				}
				if (num >= 2)
				{
					return "wrong language: expected English, but the dialogue looks Portuguese";
				}
			}
			if (targetLanguage.IndexOf("Portuguese", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				int num2 = 0;
				string[] array3 = new string[15]
				{
					" the ", " outfit ", " look ", " style ", " you ", " your ", " this ", " that ", " really ", " cute ",
					" nice ", " spring ", " winter ", " christmas ", " strange "
				};
				string[] array4 = array3;
				foreach (string value2 in array4)
				{
					if (text3.Contains(value2))
					{
						num2++;
					}
				}
				if (num2 >= 3 && !Regex.IsMatch(text3, "[ãõçáéíóúâêôà]"))
				{
					return "wrong language: expected Brazilian Portuguese, but the dialogue looks English";
				}
			}
			return null;
		}

		public static string RestoreEllipsesAndNormalise(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			text = Regex.Replace(text, "(?<!\\.)\\.(?!\\.\\.)(?:\\s+|#\\$b#)([a-záàâãéèêíìîóòôõúùûçñ])", (Match m) => "... " + m.Groups[1].Value);
			text = Regex.Replace(text, "\\.{4,}", "...");
			text = Regex.Replace(text, "(?<!\\.)\\.{2}(?!\\.)", "...");
			return text;
		}

		public static string NormaliseDialogueBreakTokens(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			string input = text.Replace("＃", "#").Replace("﹟", "#").Replace("＄", "$");
			input = Regex.Replace(input, "#\\s*\\$\\s*b\\s*#", "#$b#", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			input = Regex.Replace(input, "(?<!#)\\$\\s*b\\s*#", "#$b#", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			input = Regex.Replace(input, "#\\s*\\$\\s*b\\s*#", "#$b#", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			return Regex.Replace(input, "\\s*#\\$b#\\s*", "#$b#", RegexOptions.CultureInvariant);
		}

		public static string CleanDialogueText(string line, int maxCharacters)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				return null;
			}
			string text = line.Trim();
			text = text.Replace("\r\n", "#$b#").Replace("\n", "#$b#").Replace("\r", "#$b#");
			text = text.Replace("\"", "").Replace("“", "").Replace("”", "");
			text = NormaliseDialogueBreakTokens(text);
			text = Regex.Replace(text, "(^|#\\$b#)\\s*[-–—•]+\\s*", "$1").Trim();
			text = Regex.Replace(text, "(^|#\\$b#)\\s*[.,;:]+\\s*", "$1").Trim();
			text = Regex.Replace(text, "\\s+#\\$b#\\s+", "#$b#");
			text = Regex.Replace(text, "\\s{2,}", " ").Trim();
			text = Regex.Replace(text, "(?<=\\.\\.\\. )u\\b", "eu", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "(?<=^|#\\$b#)u\\b", "eu", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "#\\$(?!b)([A-Za-z0-9]+)#", "$$1#$b#", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "(#\\$b#){2,}", "#$b#", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "\\*\\*([^*]*)\\*\\*", "$1", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "\\*{2,}", "", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "(\\w)\\*\\.?([A-Za-záàâãéèêíìîóòôõúùûçñÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇÑ])", "$1* $2", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "(?<!\\w)\\*\\s+([A-Za-záàâãéèêíìîóòôõúùûçñÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇÑ])", "*$1", RegexOptions.CultureInvariant);
			text = Regex.Replace(text, "(\\$[A-Za-z0-9]+)([A-Za-záàâãéèêíìîóòôõúùûçñÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇÑ])", "$1 $2", RegexOptions.CultureInvariant);
			int num = Math.Clamp(maxCharacters, 80, 2000);
			int num2 = Math.Max(40, Math.Min(160, num / 3));
			int num3 = Math.Clamp(num + num2, 80, 2000);
			if (text.Length > num3)
			{
				int num4 = Math.Max(20, num3 - 3);
				string text2 = text.Substring(0, Math.Min(num4, text.Length)).TrimEnd('.', ',', ';', ':', ' ');
				int num5 = Math.Max(text2.LastIndexOf("#$b#", StringComparison.Ordinal), Math.Max(text2.LastIndexOf(". ", StringComparison.Ordinal), text2.LastIndexOf("! ", StringComparison.Ordinal)));
				if (num5 > Math.Max(40, num4 * 2 / 3))
				{
					text2 = text2.Substring(0, num5 + ((!text2.Substring(num5).StartsWith("#$b#", StringComparison.Ordinal)) ? 1 : 0)).TrimEnd('.', ',', ';', ':', ' ');
				}
				int num6 = text2.LastIndexOf(' ');
				if (num6 > Math.Max(40, text2.Length - 30))
				{
					text2 = text2.Substring(0, num6).TrimEnd('.', ',', ';', ':', ' ');
				}
				text = text2 + "...";
			}
			return text;
		}

		public static bool LooksLikeCopiedFormatExample(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string text2 = text.ToLowerInvariant();
			return text2.Contains("essa roupa ficou boa em você") || text2.Contains("essa roupa ficou boa em voce") || text2.Contains("não faz essa cara") || text2.Contains("nao faz essa cara") || text2.Contains("that outfit actually looks good on you") || text2.Contains("don't make that face") || text2.Contains("dont make that face") || text2.Contains("spoken outfit reaction") || text2.Contains("current game language") || text2.Contains("optional portrait command");
		}

		public static bool LooksLikeInstructionLeak(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string text2 = text.ToLowerInvariant();
			return text2.Contains("dialogue box break") || text2.Contains("return json") || text2.Contains("stardew valley-style") || text2.Contains("npc characteristics") || text2.Contains("available portraits") || text2.Contains("current context") || text2.Contains("contexto atual") || text2.Contains("categorias de diálogo") || text2.Contains("categoria de diálogo") || text2.Contains("tone guidance") || text2.Contains("use #$b#") || text2.Contains("metadata") || text2.Contains("json only") || text2.Contains("return only json") || text2.Contains("portrait:") || text2.Contains("**portrait**") || text2.Contains("tonalidade:") || text2.Contains("personalidade de sebastian");
		}

		private static bool ContainsAny(string source, params string[] terms)
		{
			if (string.IsNullOrEmpty(source))
			{
				return false;
			}
			foreach (string value in terms)
			{
				if (source.Contains(value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public static string StripDialogueMarkup(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}
			string input = Regex.Replace(text, "#\\$b#|\\$[a-z0-9]+|\\{\\{.*?\\}\\}", " ", RegexOptions.IgnoreCase);
			return Regex.Replace(input, "\\s+", " ").Trim();
		}
	}
	internal sealed class FashionSenseVisualService
	{
		private readonly IMonitor monitor;

		private readonly Func<IFashionSenseApi> getApi;

		public FashionSenseVisualService(IMonitor monitor, Func<IFashionSenseApi> getApi)
		{
			this.monitor = monitor;
			this.getApi = getApi;
		}

		public bool TryBuildVisualSummary(Farmer farmer, string currentOutfitId, out string summary, out string reason, bool suppressHairAndGenericHeadwearForSavedOutfit = false, bool visibleVanillaHatEquipped = true)
		{
			summary = "";
			reason = "unknown reason";
			IFashionSenseApi fashionSenseApi = getApi?.Invoke();
			if (fashionSenseApi == null)
			{
				reason = "Fashion Sense API is unavailable";
				return false;
			}
			if (farmer == null)
			{
				reason = "farmer is null";
				return false;
			}
			try
			{
				List<string> list = new List<string>();
				string text = (visibleVanillaHatEquipped ? TryGetVanillaHatName(farmer) : null);
				if (!string.IsNullOrWhiteSpace(text))
				{
					summary = "The farmer is wearing a real in-game hat: " + text + ". React specifically to this hat. Do not invent or describe other clothing; the hat is the whole point of this reaction.";
					reason = "";
					return true;
				}
				string text2 = StringUtils.FirstNonEmpty(currentOutfitId, TryGetCurrentOutfitId(fashionSenseApi));
				bool flag = !string.IsNullOrWhiteSpace(text2);
				if (flag)
				{
					list.Add("saved outfit clue: " + HumanizeFashionSenseId(text2));
				}
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Hair, "hair", list, suppressHairAndGenericHeadwearForSavedOutfit);
				int count = list.Count;
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Hat, "hat/headwear", list, suppressHairAndGenericHeadwearForSavedOutfit);
				bool flag2 = list.Count > count;
				if (flag && !flag2)
				{
					list.Add("head/headwear: NONE equipped (no hat, ears, horns, antennae, or themed head piece is being worn right now, even if the outfit name suggests one)");
				}
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Accessory, "visible accessory/extra visual item (may be wings, cape, umbrella, backpack, bow, earrings, or hair accessory; makeup is ignored)", list, suppressHairAndGenericHeadwearForSavedOutfit);
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.AccessorySecondary, "secondary visible accessory/extra visual item", list, suppressHairAndGenericHeadwearForSavedOutfit);
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.AccessoryTertiary, "tertiary visible accessory/extra visual item", list, suppressHairAndGenericHeadwearForSavedOutfit);
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Shirt, "shirt/top", list, suppressHairAndGenericHeadwearForSavedOutfit);
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Sleeves, "sleeves", list, suppressHairAndGenericHeadwearForSavedOutfit);
				AddAppearanceClue(fashionSenseApi, farmer, IFashionSenseApi.Type.Pants, "pants/bottom", list, suppressHairAndGenericHeadwearForSavedOutfit);
				if (list.Count <= 0)
				{
					reason = "no equipped Fashion Sense appearance data was found";
					return false;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Fashion Sense equipped appearance clues from the game API. Use only as support for outfit analysis; never mention Fashion Sense, API, IDs, filenames, or these labels in dialogue: ");
				stringBuilder.Append(string.Join("; ", list));
				summary = stringBuilder.ToString();
				reason = "ok";
				return true;
			}
			catch (Exception ex)
			{
				reason = ex.Message;
				if (ModEntry.DebugLog)
				{
					IMonitor obj = monitor;
					if (obj != null)
					{
						obj.Log("[FS VISUAL] Could not build Fashion Sense visual summary: " + ex.Message, (LogLevel)2);
					}
				}
				return false;
			}
		}

		private static string TryGetVanillaHatName(Farmer farmer)
		{
			try
			{
				Hat val = ((NetFieldBase<Hat, NetRef<Hat>>)(object)farmer?.hat)?.Value;
				if (val == null)
				{
					return null;
				}
				string text = ((Item)val).DisplayName;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = ((Item)val).Name;
				}
				return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
			}
			catch
			{
				return null;
			}
		}

		private static string TryGetCurrentOutfitId(IFashionSenseApi api)
		{
			try
			{
				KeyValuePair<bool, string> currentOutfitId = api.GetCurrentOutfitId();
				if (currentOutfitId.Key && !string.IsNullOrWhiteSpace(currentOutfitId.Value))
				{
					return currentOutfitId.Value.Trim();
				}
			}
			catch
			{
			}
			return "";
		}

		private static void AddAppearanceClue(IFashionSenseApi api, Farmer farmer, IFashionSenseApi.Type type, string label, List<string> pieces, bool suppressHairAndGenericHeadwearForSavedOutfit = false)
		{
			string text = TryGetAppearanceId(api, farmer, type);
			if (string.IsNullOrWhiteSpace(text) || (IsAccessoryType(type) && IsIgnoredMakeupAccessoryId(text)))
			{
				return;
			}
			string text2 = HumanizeFashionSenseId(text);
			string text3 = TryGetAppearanceColorDescription(api, farmer, type);
			bool flag = type == IFashionSenseApi.Type.Hair;
			bool flag2 = type == IFashionSenseApi.Type.Hat;
			if (flag)
			{
				if (!suppressHairAndGenericHeadwearForSavedOutfit)
				{
					pieces.Add(label + ": " + text2 + " (do NOT guess hair color from the image; an authoritative hair color may be provided separately)");
				}
			}
			else if (flag2)
			{
				if (suppressHairAndGenericHeadwearForSavedOutfit)
				{
					if (!IsUnhelpfulInternalAppearanceId(text))
					{
						pieces.Add("visible head accessory/headwear: " + text2 + " (theme/shape clue only; do not name its color; do not call it a hat unless it is clearly a hat)");
					}
				}
				else
				{
					pieces.Add(label + ": " + text2 + " (do NOT guess hat/headwear color from the raw image; an authoritative hat color may be provided separately)");
				}
			}
			else if (!string.IsNullOrWhiteSpace(text3))
			{
				pieces.Add(label + ": " + text2 + ", color clue " + text3);
			}
			else
			{
				pieces.Add(label + ": " + text2);
			}
		}

		private static string TryGetAppearanceId(IFashionSenseApi api, Farmer farmer, IFashionSenseApi.Type type)
		{
			try
			{
				KeyValuePair<bool, string> currentAppearanceId = api.GetCurrentAppearanceId(type, farmer);
				if (currentAppearanceId.Key && !string.IsNullOrWhiteSpace(currentAppearanceId.Value))
				{
					string text = currentAppearanceId.Value.Trim();
					if (!text.Equals("None", StringComparison.OrdinalIgnoreCase))
					{
						return text;
					}
				}
			}
			catch
			{
			}
			return "";
		}

		private static string TryGetAppearanceColorDescription(IFashionSenseApi api, Farmer farmer, IFashionSenseApi.Type type)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				KeyValuePair<bool, Color> appearanceColor = api.GetAppearanceColor(type, farmer);
				if (!appearanceColor.Key)
				{
					return "";
				}
				Color value = appearanceColor.Value;
				if (((Color)(ref value)).A <= 0)
				{
					return "transparent";
				}
				if (((Color)(ref value)).R >= 245 && ((Color)(ref value)).G >= 245 && ((Color)(ref value)).B >= 245)
				{
					return "default/untinted";
				}
				return ColorNamer.ClosestSimpleColorName(value) + " (#" + ((Color)(ref value)).R.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).G.ToString("X2", CultureInfo.InvariantCulture) + ((Color)(ref value)).B.ToString("X2", CultureInfo.InvariantCulture) + ")";
			}
			catch
			{
				return "";
			}
		}

		public static string HumanizeAppearanceId(string id)
		{
			return HumanizeFashionSenseId(id);
		}

		private static string HumanizeFashionSenseId(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return "";
			}
			string text = id.Trim();
			int num = text.LastIndexOf('/');
			if (num >= 0 && num < text.Length - 1)
			{
				text = text.Substring(num + 1);
			}
			text = Regex.Replace(text, "[_\\-.]+", " ");
			text = Regex.Replace(text, "([a-zà-ÿ])([A-Z])", "$1 $2");
			text = Regex.Replace(text, "\\s+", " ").Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				return id.Trim();
			}
			return text;
		}

		public static bool IsUnhelpfulInternalAppearanceId(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return true;
			}
			string text = HumanizeFashionSenseId(id);
			string input = (" " + text + " ").ToLowerInvariant();
			if (Regex.IsMatch(input, "\\bpack\\s*\\d+\\b", RegexOptions.IgnoreCase))
			{
				return true;
			}
			if (Regex.IsMatch(input, "\\b(hat|hair|accessory|acc|item|pack|slot|part)\\s*\\d+\\b", RegexOptions.IgnoreCase))
			{
				return true;
			}
			string input2 = Regex.Replace(input, "\\b(hat|hair|accessory|acc|item|pack|slot|part|fs|yomi)\\b", " ", RegexOptions.IgnoreCase);
			input2 = Regex.Replace(input2, "[0-9_\\-\\.]+", " ");
			input2 = Regex.Replace(input2, "\\s+", " ").Trim();
			return input2.Length < 3;
		}

		private static bool IsAccessoryType(IFashionSenseApi.Type type)
		{
			return type == IFashionSenseApi.Type.Accessory || type == IFashionSenseApi.Type.AccessorySecondary || type == IFashionSenseApi.Type.AccessoryTertiary;
		}

		private static bool IsIgnoredMakeupAccessoryId(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return false;
			}
			string text = HumanizeFashionSenseId(id);
			string text2 = " " + string.Join(" ", id, text).ToLowerInvariant().Replace('_', ' ')
				.Replace('-', ' ')
				.Replace('.', ' ')
				.Replace('/', ' ') + " ";
			bool flag = (text2.Contains(" eye ") || text2.Contains(" eyes ") || text2.Contains(" olho ") || text2.Contains(" olhos ")) && (text2.Contains(" highlight ") || text2.Contains(" highlights ") || text2.Contains(" sparkle ") || text2.Contains(" sparkles ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho ") || text2.Contains(" brilhos "));
			bool flag2 = (text2.Contains(" face ") || text2.Contains(" facial ") || text2.Contains(" rosto ")) && (text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" highlight ") || text2.Contains(" blush ") || text2.Contains(" sparkle ") || text2.Contains(" shine ") || text2.Contains(" glitter ") || text2.Contains(" gloss ") || text2.Contains(" brilho "));
			return flag || flag2 || text2.Contains(" makeup ") || text2.Contains(" maquiagem ") || text2.Contains(" blush ") || text2.Contains(" lipstick ") || text2.Contains(" batom ") || text2.Contains(" eyeshadow ") || text2.Contains(" eye shadow ") || text2.Contains(" sombra ") || text2.Contains(" eyeliner ") || text2.Contains(" delineador ") || text2.Contains(" rimel ") || text2.Contains(" rímel ");
		}
	}
	internal static class PortraitResolver
	{
		private static string ResolvePortraitCommandSimple(CharacterAiProfile profile, string portraitKey, int availablePortraitCount = 0)
		{
			if (profile?.Portraits == null || string.IsNullOrWhiteSpace(portraitKey))
			{
				return "";
			}
			portraitKey = portraitKey.Trim();
			string key = (portraitKey.StartsWith("$", StringComparison.Ordinal) ? portraitKey.TrimStart('$') : portraitKey);
			if (profile.Portraits.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value?.Command))
			{
				return ValidatePortraitCommand(value.Command.Trim(), availablePortraitCount);
			}
			if (portraitKey.StartsWith("$", StringComparison.Ordinal))
			{
				foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
				{
					string text = portrait.Value?.Command;
					if (string.IsNullOrWhiteSpace(text))
					{
						text = "$" + portrait.Key;
					}
					if (text.Equals(portraitKey, StringComparison.OrdinalIgnoreCase))
					{
						return ValidatePortraitCommand(text.Trim(), availablePortraitCount);
					}
				}
				int num = PortraitCommandIndex(portraitKey);
				if (num >= 0 && (availablePortraitCount <= 0 || num < availablePortraitCount))
				{
					return portraitKey.Trim();
				}
				return "";
			}
			return "";
		}

		private static string ValidatePortraitCommand(string command, int availablePortraitCount)
		{
			if (string.IsNullOrWhiteSpace(command) || availablePortraitCount <= 0)
			{
				return command ?? "";
			}
			int num = PortraitCommandIndex(command);
			if (num < 0)
			{
				return command;
			}
			return (num < availablePortraitCount) ? command : "";
		}

		private static int PortraitCommandIndex(string command)
		{
			if (string.IsNullOrWhiteSpace(command))
			{
				return -1;
			}
			string text = command.Trim().TrimStart('$').ToLowerInvariant();
			if (int.TryParse(text, out var result))
			{
				return result;
			}
			string text2 = text;
			string text3 = text2;
			if (!(text3 == "h"))
			{
				if (text3 == "s")
				{
					return 2;
				}
				return -1;
			}
			return 1;
		}

		private static string ResolveNeutralFallbackPortraitKey(CharacterAiProfile profile, string requestedFallbackKey)
		{
			if (IsNeutralPortraitKey(profile, requestedFallbackKey))
			{
				return requestedFallbackKey?.Trim() ?? "";
			}
			return FindNeutralPortraitKey(profile);
		}

		private static string FindNeutralPortraitKey(CharacterAiProfile profile)
		{
			if (profile?.Portraits == null || profile.Portraits.Count == 0)
			{
				return "";
			}
			foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
			{
				if (IsNeutralPortraitKey(profile, portrait.Key))
				{
					return portrait.Key;
				}
			}
			return "";
		}

		private static bool IsNeutralPortraitKey(CharacterAiProfile profile, string portraitKey)
		{
			if (profile?.Portraits == null || string.IsNullOrWhiteSpace(portraitKey))
			{
				return false;
			}
			string text = portraitKey.Trim();
			if (text.StartsWith("$", StringComparison.Ordinal))
			{
				text = text.TrimStart('$');
			}
			if (!profile.Portraits.TryGetValue(text, out var value))
			{
				foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
				{
					string text2 = portrait.Value?.Command;
					if (string.IsNullOrWhiteSpace(text2))
					{
						text2 = "$" + portrait.Key;
					}
					if (text2.Equals(portraitKey.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						text = portrait.Key;
						value = portrait.Value;
						break;
					}
				}
			}
			if (value == null)
			{
				return false;
			}
			string text3 = (text + " " + value.Description + " " + value.Command).ToLowerInvariant();
			if (text3.Contains("neutral") || text3.Contains("default face") || text3.Contains("default expression") || text3.Contains("straightforward moment"))
			{
				return true;
			}
			return false;
		}

		public static string ApplyPortraitsFromFields(CharacterAiProfile profile, string dialogueText, AiComplimentResult parsed, string inlinePortraitFallback = null, int availablePortraitCount = 0)
		{
			if (string.IsNullOrWhiteSpace(dialogueText))
			{
				return dialogueText;
			}
			string requestedFallbackKey = StringUtils.FirstNonEmpty(parsed?.Portrait, inlinePortraitFallback);
			string fallbackPortraitKey = ResolveNeutralFallbackPortraitKey(profile, requestedFallbackKey);
			List<string> portraitKeys = parsed?.Portraits ?? new List<string>();
			return ApplyPortraitsToDialogueBoxes(profile, dialogueText, portraitKeys, fallbackPortraitKey, availablePortraitCount);
		}

		private static string ApplyPortraitsToDialogueBoxes(CharacterAiProfile profile, string dialogueText, List<string> portraitKeys, string fallbackPortraitKey, int availablePortraitCount = 0)
		{
			if (string.IsNullOrWhiteSpace(dialogueText))
			{
				return dialogueText;
			}
			string text = ResolvePortraitCommandSimple(profile, fallbackPortraitKey, availablePortraitCount);
			bool flag = portraitKeys?.Any((string k) => !string.IsNullOrWhiteSpace(k)) ?? false;
			string[] array = dialogueText.Split(new string[1] { "#$b#" }, StringSplitOptions.None);
			if (array.Length == 0)
			{
				return dialogueText;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = 0; num < array.Length; num++)
			{
				string text2 = array[num];
				string portraitKey = ((flag && portraitKeys != null && num < portraitKeys.Count) ? portraitKeys[num] : fallbackPortraitKey);
				string text3 = ResolvePortraitCommandSimple(profile, portraitKey, availablePortraitCount);
				if (string.IsNullOrWhiteSpace(text3))
				{
					text3 = text;
				}
				if (!string.IsNullOrWhiteSpace(text3) && !string.IsNullOrWhiteSpace(text2))
				{
					text2 = text2.TrimEnd() + text3;
				}
				stringBuilder.Append(text2);
				if (num < array.Length - 1)
				{
					stringBuilder.Append("#$b#");
				}
			}
			return stringBuilder.ToString();
		}

		public static string ExtractLastAllowedPortraitKeyFromText(string text, CharacterAiProfile profile)
		{
			if (string.IsNullOrWhiteSpace(text) || profile?.Portraits == null || profile.Portraits.Count <= 0)
			{
				return "";
			}
			string input = text.Replace("#$b#", "\ue000OC_BREAK\ue000");
			Dictionary<string, (string, PortraitProfile, string)> dictionary = BuildAllowedPortraitCommandLookup(profile);
			if (dictionary.Count <= 0)
			{
				return "";
			}
			string result = "";
			foreach (Match item in Regex.Matches(input, "\\$[A-Za-z0-9]+", RegexOptions.CultureInvariant))
			{
				string key = item.Value.Trim();
				if (dictionary.TryGetValue(key, out var value))
				{
					(result, _, _) = value;
				}
			}
			return result;
		}

		public static string BuildPortraitCommandList(CharacterAiProfile profile)
		{
			if (profile?.Portraits == null || profile.Portraits.Count <= 0)
			{
				return "none";
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
			{
				string text = portrait.Value?.Command;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "$" + portrait.Key;
				}
				string text2 = portrait.Value?.Description ?? "";
				list.Add(string.IsNullOrWhiteSpace(text2) ? text : (text + " (" + text2 + ")"));
			}
			return string.Join(", ", list);
		}

		public static string BuildPortraitKeyDescriptionList(CharacterAiProfile profile)
		{
			if (profile?.Portraits == null || profile.Portraits.Count <= 0)
			{
				return "none";
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
			{
				string text = portrait.Value?.Description ?? "";
				list.Add(string.IsNullOrWhiteSpace(text) ? portrait.Key : (portrait.Key + " (" + text + ")"));
			}
			return string.Join(", ", list);
		}

		public static string SanitizeInlinePortraitCommands(string text, CharacterAiProfile profile, bool isLocalProvider, ModConfig config)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			string input = text.Replace("#$b#", "\ue000OC_BREAK\ue000");
			input = Regex.Replace(input, "\\$[A-Za-z0-9]+", "", RegexOptions.CultureInvariant);
			input = input.Replace("\ue000OC_BREAK\ue000", "#$b#");
			input = Regex.Replace(input, "\\s*#\\$b#\\s*", "#$b#");
			input = Regex.Replace(input, "\\s+([,.;:!?])", "$1", RegexOptions.CultureInvariant);
			input = Regex.Replace(input, "([*])\\s+([,.;:!?])", "$1$2", RegexOptions.CultureInvariant);
			input = Regex.Replace(input, "\\s{2,}", " ").Trim();
			input = Regex.Replace(input, "(?:#\\$b#){4,}", "#$b##$b##$b#");
			return input.Trim();
		}

		private static Dictionary<string, (string Key, PortraitProfile Portrait, string Command)> BuildAllowedPortraitCommandLookup(CharacterAiProfile profile)
		{
			Dictionary<string, (string, PortraitProfile, string)> dictionary = new Dictionary<string, (string, PortraitProfile, string)>(StringComparer.OrdinalIgnoreCase);
			if (profile?.Portraits == null)
			{
				return dictionary;
			}
			foreach (KeyValuePair<string, PortraitProfile> portrait in profile.Portraits)
			{
				string text = portrait.Value?.Command;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "$" + portrait.Key;
				}
				text = text.Trim();
				if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("$", StringComparison.Ordinal))
				{
					dictionary[text] = (portrait.Key, portrait.Value, text);
				}
			}
			return dictionary;
		}

		private static bool ShouldSuppressLocalPortrait(string portraitKey, PortraitProfile portrait, string dialogueText)
		{
			string text = (portraitKey + " " + portrait?.Description).ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string[] array = new string[25]
			{
				"bravo", "irritado", "triste", "averso", "desapontado", "chorando", "gripado", "ciúmes", "ciumes", "raiva",
				"aff", "gemido", "prazer", "assustado", "choque", "mad", "angry", "annoyed", "sad", "crying",
				"sick", "jealous", "disgust", "upset", "frustrated"
			};
			bool flag = false;
			string[] array2 = array;
			foreach (string value in array2)
			{
				if (text.Contains(value))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			string text2 = " " + DialogueValidator.StripDialogueMarkup(dialogueText).ToLowerInvariant() + " ";
			bool flag2 = text2.Contains(" weird ") || text2.Contains(" strange ") || text2.Contains(" odd ") || text2.Contains(" estranho ") || text2.Contains(" esquisito ") || text2.Contains(" sério? ") || text2.Contains(" serio? ");
			return !flag2;
		}
	}
}
