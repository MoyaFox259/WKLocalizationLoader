using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.ScoreScreenPatch",
        "This module replaces texts of\n"
        + "stats, scores, medals, popups and leaderboards."
    )]
    public class ScoreScreenPatchSettings : ModuleSettingsBase
    {
        [ConfigEntry(
            "UseHighScoreFallbackFontAsset",
            true,
            "Set this field to \"false\" to use orignal font asset for\n"
            + "high score text of end screen score window.\n"
            + "Be careful, some characters may not be rendered properly."
        )]
        public bool UseHighScoreFallbackFontAsset;
    }
}

