> **Notice:**  
> If you're looking for a ready-to-use White Knuckle translation mod, [WKTranslator](https://thunderstore.io/c/white-knuckle/p/galfarious/WKTranslator/) can be your best bet.
> Go check it out! It's pretty easy to use and WK Modding Server's dedicated translation channel is a very good place to start your translation and collab with others.

## About this project...

> **Before continuing:**  
> AI assistance was used to help me with background knowledge of Unity.
> This project is still in a heavy WIP state as of writing this.
> **Please proceed with caution.**

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
| Main Menu UI | ✅ |
| Score Screen UI | ✅ |
| Gameplay UI | ✅ |
| QuietOS UI | ✅ |
| Gamemodes | ✅ |
| Location Names | ✅ |
| Subtitles | ✅ |
| Cosmetics | ✅ |
| Trinkets/Bindings | ✅ |
| Perks | ✅ |
| Facility Upgrades | ✅ |
| Progression Unlocks | ✅ |
| Achievements | ✅ |
| Objectives | ✅ |
| Item Descriptions | ✅ |
| Death Messages | ✅ |
| Death Tips | ✅ |
| Paper Notes | ✅ |
| OS Documents | ✅ |
| Scrawl Headers | ✅ |
| In-world Texts | ✅ |
| Command Console | ❌ |

**Audio replacement**  
❌ No plans for audio replacement support as of writing this.

**Texture replacement**  
❌ No plans for texture replacement support as of writing this.

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

