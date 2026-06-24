using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Translation.FontAssetPatch",
        "This module adds fallback font assets."
    )]
    public class FontAssetPatchSettings : ModuleSettingsBase
    {
        [ConfigEntry(
            "SaveFontAssetCacheOnDisk",
            false,
            "Set this field to \"true\" to cache generated TMP_FontAsset\n"
            + "on disk to reduce load times on subsequent game launches.\n"
            + "Warning: Cache size may grow significantly --\n"
            + "A 4096×4096 atlas alone is about 16MB.\n"
            + "Enable this only if you have spare disk space.\n"
            + "Cache files are stored in the same directory as plugin .dll."
        )]
        public bool SaveFontAssetCacheOnDisk;
    }
}

