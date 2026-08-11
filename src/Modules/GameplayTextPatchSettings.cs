using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.GameplayTextPatch",
        "This module replaces texts of\n"
        + "roach counters, vendors and stat trackers."
    )]
    public class GameplayTextPatchSettings : ModuleSettingsBase
    {
    }
}

