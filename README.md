# Outfit Reactions

A **Stardew Valley** mod that makes NPCs dynamically comment on your character's clothes whenever you change your look, designed exclusively for [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969).

Created by **NatrollEXE**.

> **Outfit Reactions does not replace or remove dialogue from the base game or other mods.** It only adds an extra outfit reaction when the mod's conditions are met; any dialogue the NPC would normally have remains available.

---

## ✨ Features

* **AI-powered reactions**: spouses and nearby NPCs react to your current outfit using AI, generating unique lines whenever you change clothes.
* **Vanilla hat and pants support**: reacts to Fashion Sense items as well as some common vanilla hats.
* **Special items with secrets**: a special-item system (`assets/special-reactions`) for creating unique reactions to memorable clothing pieces. Currently includes the Lucky Purple Shorts.
* **Outfit memory**: NPCs remember outfits and special items they have seen before, reacting with familiarity instead of repeating a first-time reaction.
* **Weather and location awareness**: reactions can consider whether the NPC is indoors or outdoors, whether it is sunny or raining, the time of day, and festival dates.
* **Custom content packs** can expand or customize NPC characteristics (`assets/npc-characteristics`) without requiring Content Patcher.
* **Configurable through Generic Mod Config Menu** (optional), with combined or special-item-focused reaction modes, multiple AI profiles, and more.

## 📋 Requirements

* [SMAPI](https://smapi.io/) 4.0.0+
* [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969) 7.5.0+ (required)
* [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional, but recommended for easy configuration)
* An API key from a compatible AI provider, such as [OpenRouter](https://openrouter.ai/) or Google Gemini, to generate reactions.

## 🔧 Installation

1. Install [SMAPI](https://smapi.io/).
2. Download and install Fashion Sense.
3. Download the latest Outfit Reactions release and extract its folder into your `Mods` directory.
4. Configure your AI API key through Generic Mod Config Menu, or by editing the `config.json` generated after the first launch.

## ⚙️ Configuration

The mod can be configured through Generic Mod Config Menu, including:

* AI profiles with provider, model, API key, and custom endpoint settings.
* Reaction modes for vanilla hats and special items, either combined with the full outfit or focused only on the special item.
* Visual analysis (vision) for models that support it.
* Additional reaction frequency and behavior settings.

## 🧩 Creating Custom Content

Outfit Reactions can be extended through its own content packs:

* `assets/npc-characteristics/*.json` — define an NPC's personality and speaking style to shape their reactions.
* `assets/special-reactions/*.json` — define special clothing items, hats, or pants with custom reactions and secrets.
* `assets/prompts/prompts.json` — customize the rules and instructions sent to the AI model.

## 📜 License

This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute it as long as the original credits are preserved.
