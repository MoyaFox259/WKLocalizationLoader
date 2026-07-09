using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.ItemDescriptionPatch",
        "This module replaces item description texts."
    )]
    public class ItemDescriptionPatchSettings : ModuleSettingsBase
    {
    }
}

