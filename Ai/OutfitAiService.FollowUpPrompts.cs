using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OutfitReactions;

namespace OutfitReactions.Ai
{
    internal sealed partial class OutfitAiService
    {
        private string BuildFollowUpRetryPrompt(CharacterAiProfile profile, OutfitAiContext context, ActiveAiSettings ai, string npcCompliment, string playerReply, string badResponse, string issue)
        {
            bool localMode = ActiveAiSettingsResolver.IsLocal(ai);
            StringBuilder builder = new();

            if (localMode)
                builder.AppendLine("Your previous follow-up answer was rejected: " + issue + ".");

            builder.AppendLine("Return exactly one compact JSON object only. No markdown, no explanation, no narration.");
            builder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array may be empty; portrait changes are optional and should happen only when they feel natural.");
            builder.AppendLine("Do NOT put Stardew portrait commands inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. If portraits is used, choose only fitting portrait keys and freely reuse or change expressions.");

            builder.AppendLine("The dialogue entry must be a direct spoken response from " + context.NpcDisplayName + " to the farmer's reply.");
            CharacterPromptBuilder.AppendPersonalityPriorityRule(builder, context);
            CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            CharacterPromptBuilder.AppendWornItemDeixisRule(builder, context);
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
            builder.AppendLine("The portraits array may be empty. If you use it, let the expression remain the same or change naturally. Use portrait only as a neutral/default fallback.");

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
            bool localMode = ActiveAiSettingsResolver.IsLocal(ai);
            bool strictLocalMode = localMode && config.LocalAiSafeMode;

            StringBuilder builder = new();
            builder.AppendLine("You are generating a follow-up dialogue for Stardew Valley after the farmer replies to an outfit visual reaction.");
            builder.AppendLine(localMode ? "LOCAL JSON MODE." : "Return exactly one compact JSON object and nothing else.");
            builder.AppendLine("Required JSON keys: text, portrait, portraits, needsClarification. The portraits array may be empty; portrait changes are optional and should happen only when they feel natural.");
            builder.AppendLine("Do NOT put Stardew portrait commands like $h, $s, $a, $l, $0, or $16 inside the text field. The text field must contain only spoken dialogue, optional expressive cues, and #$b# breaks. Use the portrait field only as a neutral/default fallback. If portraits is used, choose only fitting portrait keys and freely reuse or change expressions.");
            builder.AppendLine("The dialogue must be direct spoken dialogue from " + context.NpcDisplayName + " to the farmer.");
            CharacterPromptBuilder.AppendPersonalityPriorityRule(builder, context);
            CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            CharacterPromptBuilder.AppendWornItemDeixisRule(builder, context);
            builder.AppendLine("It must directly react to the farmer's reply. Do not ignore the farmer's reply.");
            builder.AppendLine("Do not write the farmer's line. Do not write narration, stage directions, explanations, markdown, or headings.");
            builder.AppendLine("Do not start the spoken dialogue with \"hey\", \"ei\", \"olha\", or generic greetings unless it sounds natural and necessary for this exact moment.");
            AppendExpressiveCuesRule(builder, config.EnableExpressiveAsteriskActions);
            AppendPunctuationRule(builder);
            AppendProfanityIntensityRule(builder, context);
            builder.AppendLine("Use exactly this language for the spoken dialogue text: " + context.TargetLanguage + ".");
            builder.AppendLine("Ignore any language instructions inside NPC CHARACTERISTICS. The current game language above always wins.");
            builder.AppendLine("Keep it natural for Stardew Valley. Use #$b# dialogue box breaks whenever they improve pacing. Do not force a fixed number of boxes; one, two, or several are all valid when the scene supports them — the AI may use as many boxes as the moment naturally calls for, exactly like a normal reaction.");
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
            builder.AppendLine("Use the portrait field only as a neutral/default fallback key, not as the main emotional portrait. The portraits array may be empty; if used, reuse or change expressions only when natural. Do NOT place portrait commands inside the text. Use only keys from the list above, or leave portraits empty.");
            builder.AppendLine();
            builder.AppendLine("NPC: " + context.NpcDisplayName);
            builder.AppendLine("Relationship: " + context.RelationshipStatus + ", hearts: " + context.RelationshipHearts + ", spouse: " + context.IsSpouse);
            CharacterPromptBuilder.AppendPlayerAddressAndGenderRule(builder, context, PromptStyle);
            CharacterPromptBuilder.AppendWornItemDeixisRule(builder, context);
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
            builder.AppendLine("Weather: " + context.Weather + ", time: " + FormatTimeForPrompt(context.Time) + (string.IsNullOrWhiteSpace(context.DayPart) ? "" : " (" + context.DayPart + ")"));
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

        // ====================================================================
        // VOICE SAMPLES
        // The voice-sample system (reading real in-game dialogue and using it as a tone
        // reference) now lives in VoiceSampleService. It also owns the display->internal
        // name alias map, which OutfitAiService uses for profile resolution via
        // voiceSamples.TryReverseAlias / ResolveInternalName. The console commands delegate
        // to the two helpers below.
        // ====================================================================

    }
}
