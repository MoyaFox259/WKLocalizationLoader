> **Notice:**  
> If you're looking for a ready-to-use White Knuckle translation mod, [WKTranslator](https://thunderstore.io/c/white-knuckle/p/galfarious/WKTranslator/) can be your best bet.
> Go check it out! It's pretty easy to use and WK Modding Server's dedicated translation channel is a very good place to start your translation and collab with others.

## About this project...

> **Before continuing:**  
> AI assistance was used to help me with background knowledge of Unity.
> This project is still in a heavy WIP state as of writing this.
> **Please proceed with caution.**

### Project progress

**Font replacement/fallback**  
✅ : Supported  
❌ : Not supported as of writing this  
| Functionality | Support |
| :- | :- |
| Load font from .ttf file | Static fonts only |
| Load font from bundle | ❌ |
| Font replacement | ✅ |
| TMP\_FontAsset replacement | ❌ |
| TMP\_FontAsset fallback | ✅ |

**Text translation**  
✅ : Implemented translation support  
➖ : WIP  
| Texts | Support |
| :- | :- |
| Main Menu UI | ➖ |
| Score Panel UI | ➖ |
| Gameplay UI | ➖ |
| QuietOS UI | ✅ |
| Gamemodes | ✅ |
| Location Names | ✅ |
| Subtitles | ✅ |
| Cosmetics | ➖ |
| Trinkets/Bindings | ✅ |
| Perks | ✅ |
| Facility Upgrades | ✅ |
| Progression Unlocks | ✅ |
| Achievements | ✅ |
| Objectives | ✅ |
| Item Descriptions | ✅ |
| Death Texts | ✅ |
| Death Tips | ➖ |
| Paper Notes | ➖ |
| OS Documents | ➖ |
| Scrawl Headers | ✅ |
| In-world Texts | ➖ |

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

- TODO: Update and put these in an example folder.

### `AnnouncementSubtitles.json`

```json
{
    "AnnouncementSubtitles": {
        "ANN_ContainmentBreach": "警报：5号生物研究实验室出现收容失效。未经正式许可，请勿接近该实验室。",
        "ANN_CriticalPower": "注意：设施剩余电力严重不足。已检测到主反应堆崩溃。辅助电力系统正以85%的功率运行。<delay=3>",
        "ANN_MemoryCore": "注意：记忆核心加速恶化中，预测有84%的数据丢失。",
        "ANN_UnauthorizedAccess": "安全系统存在漏洞，检测到警戒区域内有未经授权的访问。",
        "ANN_AtmosphericBreach": "警告：第12区的大气荚发现裂隙，目前具有高危风险。",
        "ANN_AutomatedDefenses": "请注意：自动防御系统可能发生故障，在所有区域行动时应保持最高警惕。",
        "ANN_SecuritySystemsOffline": "监控系统仅有12%可正常运行，仍有多数区域未受监控。请谨慎前进。"
    }
}
```

### `AnnouncementSubtitleTimings.json`

```json
{
    "ModuleSettings": {
        "BaseDuration": 2.5,
        "CharacterInterval": 0.1,
        "EndDelay": 0.5,
        "UseOriginalDelay": false
    },
    "AnnouncementSubtitleTimings": {
        "ANN_ContainmentBreach": [8.86],
        "ANN_CriticalPower": [14.04],
        "ANN_MemoryCore": [10.71],
        "ANN_UnauthorizedAccess": [7.51],
        "ANN_AtmosphericBreach": [7.93],
        "ANN_AutomatedDefenses": [9.09],
        "ANN_SecuritySystemsOffline": [12.32],
        "ANN_OxygenLevels": [8.32],
        "ANN_StructuralIntegrity": [10.25],
        "ANN_UnstableEnergySignatures": [9.89],
        "ANN_TutorialStart_Filter": [10.54, 15.82, 21.67],
        "ANN_TutorialFinish_Filter": [8.92, 20.22, 27.27, 36.62, 40.31, 43.96, 48.00, 51.84, 60.42]
    }
}
```

### `Fonts.json`

```json
{
    "ModuleSettings": {
        "SaveFontCacheOnDisk": false
    },
    "FontInfos": {
        "ChicagoFLF": {
            "FileName": "fusion-pixel-12px.ttf",
            "FontName": "SubstituteFont - fusion-pixel-12px",
            "PointSize": 12,
            "VerticalOffset": 0,
            "AtlasWidth": 4096,
            "AtlasHeight": 4096,
            "AtlasPadding": 5,
            "ShaderName": "GUI/Text Shader",
            "DefaultOSFont": "Arial",
            "FontGlyphLoadFlags": "LOAD_DEFAULT",
            "AtlasTextureFilterMode": "Point",
            "AtlasGlyphPackingMode": "BestShortSideFit",
            "AtlasRenderMode": "RASTER_HINTED"
        },
        "monoclefixed": {
            "FileName": "Cubic_11.ttf",
            "PointSize": 12,
            "AtlasWidth": 2048,
            "AtlasHeight": 2048,
            "AtlasTextureFilterMode": "Point",
            "AtlasRenderMode": "RASTER_HINTED"
        }
    },
    "CharactersToRender": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZäöüßÄÖÜ0123456789.,!?…-–—()[]{}„«»/\\@#$%^&*+<=>:;'\"",
}
```

### `FontAssets.json`

```json
{
    "ModuleSettings": {
        "SaveFontAssetCacheOnDisk": false
    },
    "FontAssetInfos": {
        "Ticketing SDF Subtitle": [
            {
                "FileName": "fusion-pixel-12px.ttf",
                "FontName": "FallbackFontAsset - fusion-pixel-12px",
                "FontVersion": "1.1.0",
                "PointSize": 64,
                "AtlasWidth": 4096,
                "AtlasHeight": 4096,
                "AtlasPadding": 5,
                "SingleAtlas": false,
                "ShaderName": "TextMeshPro/Distance Field",
                "FontGlyphLoadFlags": "LOAD_DEFAULT",
                "AtlasTextureFilterMode": "Point",
                "AtlasGlyphPackingMode": "BestShortSideFit",
                "AtlasRenderMode": "SDFAA"
            },
            {
                "FileName": "NotoSansSC-VariableFont_wght.ttf",
                "PointSize": 64,
                "AtlasWidth": 2048,
                "AtlasHeight": 2048,
                "AtlasTextureFilterMode": "Point",
                "AtlasRenderMode": "SDFAA_HINTED"
            }
        ],
        "ChicagoFLF": [
            {
                "FileName": "NotoSansSC-VariableFont_wght.ttf",
                "FontName": "DefaultSettingTest - NotoSansSC-VariableFont_wght.ttf"
            }
        ]
    },
    "CharactersToRender": "一二三四五六七八九十百千万亿你我他她它我们你们他们"
}
```

### `MotherSubtitles.json`

```json
{
    "RandomCharacters": "abcdefghijklmnopqrstuvwxyz"
    "NonRandomCharacters": ",.!?' ",
    "MotherSubtitles": {
        "nest-hunter-intro-01": "LEAVING PROTECTION... SLIPPERY BEAST. BE ALERT",
        "nest-hunter-intro-02": "CLIMB, LITTLE ONE.. ESCAPE.."
    }
}
```

### `StaticTexts.json`

```json
{
    "TextTranslations": {
        "PLAY": "开始游戏",
        "LOGBOOK": "日志",
        "COSMETIC": "装扮",
        "SETTINGS": "设置",
        "QUIT": "退出"
    }
}
```

