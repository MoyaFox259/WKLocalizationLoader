using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.ClosedCaptionSubtitlePatch",
        "This module replaces closed caption subtitle texts."
    )]
    public class ClosedCaptionSubtitlePatchSettings : ModuleSettingsBase
    {
    }
}

