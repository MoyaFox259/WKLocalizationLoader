using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.RecordingSubtitlePatch",
        "This module replaces recording subtitle texts."
    )]
    public class RecordingSubtitlePatchSettings : ModuleSettingsBase
    {
    }
}

