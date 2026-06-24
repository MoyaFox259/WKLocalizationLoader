using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.TextPatch",
        "This module replaces text content of Text class instances."
    )]
    public class TextPatchSettings : ModuleSettingsBase
    {
    }
}

