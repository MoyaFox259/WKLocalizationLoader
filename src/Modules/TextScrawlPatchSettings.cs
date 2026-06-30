using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.TextScrawlPatch",
        "This module replaces text content of UT_TextScrawl class instances."
    )]
    public class TextScrawlPatchSettings : ModuleSettingsBase
    {
    }
}

