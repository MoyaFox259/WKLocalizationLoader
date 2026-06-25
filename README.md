## Overview

WKLocalizationLoader is a BepInEx plugin that loads language-specific 
resource files and provides localization support for White Knuckle.  

The plugin's functionality is divided into several independent modules, 
each driven by its corresponding JSON file. 
If the plugin fails to load a specific json file in language folder 
(whether due to absence or invaild format), 
its associated module will be disabled by default.  

The table below summarizes all available modules: 

| JSON file | Module Class | Description |
|---|---|---|
| `AnnouncementSubtitles.json` | AnnouncementSubtitlePatch | Replaces announcer subtitle texts. |
| `AnnouncementSubtitleTimings.json` | AnnouncementSubtitleTimingPatch | Adjusts display timings of announcer subtitles. |
| `Fonts.json` | FontPatch | Loads `.ttf` fonts from `Fonts\` folder and replaces font of `Text` class instances. |
| `FontAssets.json` | FontAssetPatch | Loads `.ttf` fonts from `Fonts\` folder and adds them as fallback fonts to `TMPro.TMP_FontAsset` class instances. |
| `Texts.json` | TextPatch | Replaces text content of `UnityEngine.UI.Text` class instances. |
| `TMPTexts.json` | TMPPatch | Replaces text content of `TMPro.TextMeshPro` class instances. |
| `TMPUITexts.json` | TMPUIPatch | Replaces text content of `TMPro.TextMeshProUGUI` class instances. |

## Language Folder Auto-Detection

The plugin will automatically detect Language Folders only if the 
`LanguageFolder` setting in plugin configuration is left empty.  

Scanning will begin from BepInEx plugins folder 
and traverse subdirectories recursively. 
The Scan will terminate if the search depth 
exceeds the `MaxScanDepth` setting \(default: 5\).  

A Folder is immediately recognized as a Language Folder 
if it contains a `.wklocalization` file.  

* This file act as a marker file used by the plugin solely for detection purposes. 
* It does not store any actual information or translation data.  

The plugin will load the first valid Language Folder it detects 
and stores its relative path to the `LanguageFolder` setting. 
This ensures the same folder is loaded 
on subsequent game launches without re-scanning.  

> The auto-detection mechanism is intentionally designed to avoid two common pitfalls: 
> * Reading and parsing file content, which is inefficient and prone to misuse. 
> * Matching similar file or folder names, which can be inaccurate at times.  

The plugin also recognizes `HawktuahLoadThis` as an 
alternative marker file in addition to `.wklocalization` for fun.  

> * `Hawktuah` is an acronym of the plugin's joke name `Here's Another White Knuckle Translation Utility And Helper`.  

---

## Language Folder Structure

### Basic Language Folder structure

A basic Language Folder layout should be as follows: 

```
YourLanguageFolder\
├── .wklocalization
├── AnnouncementSubtitles.json
├── AnnouncementSubtitleTimings.json
├── Fonts.json
├── FontAssets.json
├── Texts.json
├── TMPTexts.json
├── TMPUITexts.json
├── Fonts\
│   ├── FontA.ttf
│   └── FontB.ttf
└── Licenses\
    ├── FontA\
    │   └── OFL.txt
    └── FontB\
        └── OFL.txt
```

The `Fonts\` subdirectory is used 
**only when** you need to load external `.ttf` fonts.  

* If you include custom fonts, remember to place their license files in the `Licenses\` subdirectory as required. This is especially important if you plan to distribute your language pack.  

### Distribution via Thunderstore

**Important**: 
If you plan to distribute your language pack as a mod on Thunderstore, 
you will need to place all language pack contents inside a subdirectory named 
`BepInEx\plugins\` or `plugins\` at the root of your mod package.  

* This is to ensure that the intened folder hierarchy is preserved during installation via mod managers such as r2modman.  

Your mod package should resemble the following structure: 

```
YourModPackage\
├── manifest.json
├── icon.png
├── README.md
└── BepInEx\plugins\ (or plugins\)
    ├── .wklocalization
    ├── AnnouncementSubtitles.json
    ├── AnnouncementSubtitleTimings.json
    ├── Fonts.json
    ├── FontAssets.json
    ├── Texts.json
    ├── TMPTexts.json
    ├── TMPUITexts.json
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

### `Texts.json` / `TMPTexts.json` / `TMPUITexts.json`

All three use the same JSON structure: 

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
