using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.DeathMessagePatch",
        "This module replaces death message texts."
    )]
    public class DeathMessagePatchSettings : ModuleSettingsBase
    {
    }
}

