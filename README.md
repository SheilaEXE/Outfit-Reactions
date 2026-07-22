# Outfit Reactions

A **Stardew Valley** mod that makes NPCs dynamically comment on your character's clothes whenever you change your look, designed exclusively for [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969).

Created by **NatrollEXE**.

> **Outfit Reactions does not replace or remove dialogue from the base game or other mods.** It only adds an extra outfit reaction when the mod's conditions are met; any dialogue the NPC would normally have remains available.

---

## ✨ Features

* **AI-powered reactions**: spouses and nearby NPCs react to your current outfit using AI, generating unique lines whenever you change clothes.
* **Vanilla and modded clothing support**: content packs can add focused reactions for hats, shirts, pants, and shoes from the base game, or other mods.
* **Outfit memory**: NPCs remember outfits and special items they have seen before, reacting with familiarity instead of repeating a first-time reaction.
* **Custom content packs** can customize characteristics for supported NPCs (`assets/npc-characteristics`) without requiring Content Patcher.
* **Configurable through Generic Mod Config Menu** (optional), with combined or special-item-focused reaction modes, multiple AI profiles, and more.

## 📋 Requirements

* [SMAPI](https://smapi.io/) 4.0.0+
* [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969) 7.5.0+ (required)
* [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional, but recommended for easy configuration)
* Tile Marker 1.1.0+ (optional, for choosing furniture and map tiles that should not block an NPC's view)
* A personal API key from a compatible AI provider. Profile 1 is enabled by default with Google Gemini and the `gemini-3.1-flash-lite` model, so new players only need to add their own Gemini API key.

## 🔧 Installation

1. Install [SMAPI](https://smapi.io/).
2. Download and install Fashion Sense.
3. Download the latest Outfit Reactions release and extract its folder into your `Mods` directory.
4. Open Generic Mod Config Menu and paste your Gemini API key into Profile 1. The provider and recommended low-cost model are already selected. Advanced users can choose a different provider or model instead.

## ⚙️ Configuration

The mod can be configured through Generic Mod Config Menu, including:

* AI profiles with provider, model, API key, and custom endpoint settings.
* Reaction modes for vanilla hats and special items, either combined with the full outfit or focused only on the special item.
* Visual analysis (vision) for models that support it.
* Additional reaction frequency and behavior settings.

> **Dialogue quality depends on the AI model you choose.** Outfit Reactions provides detailed character profiles, visual context, and writing rules, but the model is ultimately responsible for following them. More capable models generally produce more natural, varied, context-aware, and character-accurate reactions, while smaller or cheaper models may be more repetitive or miss subtle details. Results and API costs vary by provider and model.

## Ignoring transparent vision obstacles

With the optional Tile Marker framework installed, open its editor and choose **Outfit Reactions:
transparent vision obstacles**. Mark counters, tables, chairs, decorative objects, or custom-map
tiles that block movement but should still allow an NPC to see the farmer's appearance. These
marks affect only Outfit Reactions' line-of-sight check; they do not change collision or maps.

Tile Marker stores this category separately from other mods. If Lots of Kisses is also installed,
enable **Merge compatible categories** in Tile Marker's GMCM to let both mods use the union of
their vision markings. Turning the option off restores the separate selections without deleting
them. Outfit Reactions continues working normally when Tile Marker is not installed.

## 🚧 Development Status and Custom NPC Compatibility

Outfit Reactions is still being actively improved, but it has already reached a stable and enjoyable enough stage to be used normally. New improvements, refinements, and fixes may continue to be added as the mod develops.

The mod supports its included base-game NPC profiles, while compatibility with custom NPCs is currently limited to **Stardew Valley Expanded** through the official Outfit Reactions SVE profile pack.

Other custom NPC mods are intentionally not supported. Many mod authors do not want their characters, writing, or other creative work involved with AI-generated content. To respect their wishes, Outfit Reactions does not automatically generate reactions for NPCs from other mods. This is an intentional restriction, not a compatibility bug. Support for another custom NPC mod would only be considered with clear permission from its author.

## 🧩 Creating Custom Content

Outfit Reactions can be extended through its own content packs:

* `assets/npc-characteristics/*.json` — define an NPC's personality and speaking style to shape their reactions.
* `assets/special-reactions/*.json` — define focused reactions for hats, shirts, pants, and shoes. The secret system remains exclusive to Mayor Lewis's purple shorts.
* `assets/prompts/prompts.json` — customize the rules and instructions sent to the AI model.

### Adding clothing reactions with a content pack

Authors can add reactions without editing or replacing any Outfit Reactions files. Create a normal SMAPI content pack with this structure:

```text
[OR] My Clothing Reactions/
├── manifest.json
└── assets/
    └── special-reactions/
        └── items.json
```

The content pack's `manifest.json` should point to Outfit Reactions:

```json
{
  "Name": "My Clothing Reactions",
  "Author": "YourName",
  "Version": "1.0.0",
  "Description": "Adds custom clothing reactions to Outfit Reactions.",
  "UniqueID": "YourName.MyClothingReactions",
  "ContentPackFor": {
    "UniqueID": "NatrollEXE.OutfitReactions"
  }
}
```

Then add one or more `.json` files inside `assets/special-reactions`. File names are flexible, so a pack may keep hats, dresses, and shoes in separate files:

```json
{
  "FormatVersion": 1,
  "GlobalRules": [
    "React to this item naturally in the NPC's own voice."
  ],
  "Items": {
    "YourName.RubyBoots": {
      "DisplayName": "Ruby Boots",
      "MatchNames": [
        "Ruby Boots"
      ],
      "MatchIds": [
        "YourMod_RubyBoots",
        "(B)YourMod_RubyBoots"
      ],
      "ItemType": "Shoes",
      "ReactionPriority": "High",
      "CoreDescription": "Bright red boots that look unusually dramatic and expensive.",
      "ReactionHint": "The NPC may find the boots flashy, impressive, excessive, or impractical depending on their personality.",
      "NpcOverrides": {
        "Emily": {
          "ReactionHint": "Emily is especially interested in their bold color and unusual style."
        }
      }
    }
  }
}
```

Supported `ItemType` values are:

* `Hat`
* `Shirt`
* `Pants`
* `Shoes` (`Boots` is also accepted as an alias)

`MatchIds` is the most reliable option for modded items. It may contain an `ItemId`, a `QualifiedItemId`, or an exact Fashion Sense appearance ID. When a stable ID is available, authors do not need to list the item's translated names for every language. `MatchNames` remains available as an optional fallback and may contain display names, internal names, or localized alternatives.

Content-pack definitions take priority over Outfit Reactions' built-in visual reactions. To intentionally replace an existing definition, use the same entry ID or match the same equipped item. If two content packs define the same entry ID, Outfit Reactions logs which pack won.

The secret and reveal-choice system is reserved for Mayor Lewis's purple shorts. Secret-related fields in third-party content packs are ignored; packs can still customize the shorts' visible reaction while the original Lewis and Marnie secret behavior remains protected.

## 📜 License

This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute it as long as the original credits are preserved.
