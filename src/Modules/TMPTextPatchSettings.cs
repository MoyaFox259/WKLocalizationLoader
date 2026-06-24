using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.TMPPatch",
        "This module replaces text content of TextMeshPro class instances."
    )]
    public class TMPPatchSettings : ModuleSettingsBase
    {
    }
}

