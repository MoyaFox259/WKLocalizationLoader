using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.HardcodedStringPatch",
        "This module replaces hardcoded texts."
    )]
    public class HardcodedStringPatchSettings : ModuleSettingsBase
    {
    }
}

