using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.ScorePanelPatch",
        "This module replaces texts of\n"
        + "stats, scores, medals, popups and leaderboards."
    )]
    public class ScorePanelPatchSettings : ModuleSettingsBase
    {
    }
}

