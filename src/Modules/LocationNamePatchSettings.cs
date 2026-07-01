using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.LocationNamePatch",
        "This module replaces location intro texts and level save names."
    )]
    public class LocationNamePatchSettings : ModuleSettingsBase
    {
    }
}

