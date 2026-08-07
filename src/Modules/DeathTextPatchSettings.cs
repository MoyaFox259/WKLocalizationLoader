using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.DeathTextPatch",
        "This module replaces death messages and death tips."
    )]
    public class DeathTextPatchSettings : ModuleSettingsBase
    {
    }
}

