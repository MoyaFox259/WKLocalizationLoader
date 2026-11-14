> **Notice:**  
> If you're looking for a ready-to-use White Knuckle translation mod, [WKTranslator](https://thunderstore.io/c/white-knuckle/p/galfarious/WKTranslator/) can be your best bet.
> Go check it out! It's pretty easy to use and WK Modding Server's dedicated translation channel is a very good place to start your translation and collab with others.

## About

This project is an alternative translation library mod for loading custom localizations for White Knuckle.  

Current supported game version: **0.62**  

You can obtain a copy of translation templates available at the [example translation](/examples/ExampleTranslation) page in the mod's repo.  

### Features 

**Font replacement/fallback**  
✅ : Supported  
❌ : Not supported as of writing this  
| Functionality | Support |
| :- | :- |
| Loading font from .ttf file | Static fonts only |
| Loading font from bundle | ❌ |
| Font replacement | ✅ |
| TMP\_FontAsset replacement | ❌ |
| TMP\_FontAsset fallback | ✅ |

**Text translation**  
✅ : Implemented translation support  
❌ : Not supported as of writing this
| Texts | Support |
| :- | :- |
| Vanilla Localization replacement (including Subtitles) | ✅ |
| Gamemodes | ✅ |
| Levels | ✅ |
| Cosmetics | ✅ |
| Trinkets/Bindings | ✅ |
| Perks | ✅ |
| Facility Upgrades | ✅ |
| Progression Unlocks | ✅ |
| Achievements | ✅ |
| Objectives | ✅ |
| Paper Notes | ✅ |
| OS Documents | ✅ |
| UI | ✅ |
| QuietOS UI | ✅ |
| Command Console | ❌ |

**Audio replacement**  
❌ No plans for audio replacement support as of writing this. There're already plenty of examples of audio replacement mods.  

**Texture replacement**  
❌ No plans for texture replacement support as of writing this. Replacing decal textures is difficult and beyond my ability.  

---

## How to Use

This mod requires BepInEx to be installed in order to function.  

To use this mod, specify the path to the folder where your translation .json files are located in the mod's config.  
If a .json file is missing, its associated patch module will simply be disabled and ignored while patching.  

A detailed documentaion of this mod is currently WIP.  

---

## Language Folder Structure

### Example Language Folder

```
YourLanguageFolder\
├── .wklocalization
├── Fonts.json
├── FontAssets.json
├── StaticTexts.json
├── ... (all other JSONs)
├── Fonts\
│   ├── FontA.ttf
│   └── FontB.ttf
└── Licenses\
    ├── FontA\
    │   └── OFL.txt
    └── FontB\
        └── OFL.txt
```

### Packaging for Thunderstore

```
YourModPackage\
├── manifest.json
├── icon.png
├── README.md
└── BepInEx\plugins\ (or plugins\)
    ├── .wklocalization
    ├── Fonts.json
    ├── FontAssets.json
    ├── StaticTexts.json
    ├── ... (all other JSONs)
    ├── Fonts\
    │   ├── FontA.ttf
    │   └── FontB.ttf
    └── Licenses\
        ├── FontA\
        │   └── OFL.txt
        └── FontB\
            └── OFL.txt
```

---

## JSON File Formats

See [translation templates](/examples/ExampleTranslation) for more information.  

A detailed documentaion of this mod is currently WIP.  

## Notes

This mod is an **unofficial** translation tool. It is not affiliated with Dark Machine.  

It patches the game's code heavily and is likely to break between game updates. I'll try to update it if breaks occur.  

None of the code or asset of this mod is ai-generated.  
