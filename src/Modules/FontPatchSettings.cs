using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.FontPatch",
        "This module replaces font of Text class instances.\n"
        + "Note: This module requires TextPatch module to be enabled."
    )]
    public class FontPatchSettings : ModuleSettingsBase
    {
        [ConfigEntry(
            "SaveFontCacheOnDisk",
            false,
            "Set this field to \"true\" to cache generated Font\n"
            + "on disk to reduce load times on subsequent game launches.\n"
            + "Warning: Cache size may grow significantly --\n"
            + "A 4096×4096 atlas alone is about 16MB.\n"
            + "Enable this only if you have spare disk space.\n"
            + "Cache files are stored in the same directory as plugin .dll."
        )]
        public bool SaveFontCacheOnDisk;
    }
}

