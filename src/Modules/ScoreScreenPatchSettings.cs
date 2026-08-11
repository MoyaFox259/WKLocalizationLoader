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
    }
}

