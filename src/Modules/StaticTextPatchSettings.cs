using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.StaticTextPatch",
        "This module replaces text content of Text/TMP_Text class instances."
    )]
    public class StaticTextPatchSettings : ModuleSettingsBase
    {
    }
}

