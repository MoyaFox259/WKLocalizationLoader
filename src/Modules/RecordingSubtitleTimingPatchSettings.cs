using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.RecordingSubtitleTimingPatch",
        "This module adjusts display timings of recording subtitles."
    )]
    public class RecordingSubtitleTimingPatchSettings : ModuleSettingsBase
    {
        [ConfigEntry(
            "BaseDuration",
            2.5f,
            "Base duration (in seconds) for displaying a subtitle."
        )]
        public float BaseDuration;

        [ConfigEntry(
            "CharacterInterval",
            0.1f,
            "Additional duration (in seconds) added "
            + "per character in the subtitle text."
        )]
        public float CharacterInterval;

        [ConfigEntry(
            "EndDelay",
            0.5f,
            "Extra duration (in seconds) added at the end of a subtitle."
        )]
        public float EndDelay;

        [ConfigEntry(
            "UseOriginalDelay",
            false,
            "Set this field to \"true\" to retain original timings of\n"
            + "subtitles that contain \"<delay>\" tag(s)."
        )]
        public bool UseOriginalDelay;
    }
}

