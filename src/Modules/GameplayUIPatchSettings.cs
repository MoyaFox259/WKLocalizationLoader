using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.GameplayUIPatch",
        "This module replaces texts of\n"
        + "roach counters, vendors and stat trackers."
    )]
    public class GameplayUIPatchSettings : ModuleSettingsBase
    {
    }
}

