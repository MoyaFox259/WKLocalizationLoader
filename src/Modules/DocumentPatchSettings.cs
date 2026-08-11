using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.DocumentPatch",
        "This module replaces texts for QuietOS document files."
    )]
    public class DocumentPatchSettings : ModuleSettingsBase
    {
    }
}

