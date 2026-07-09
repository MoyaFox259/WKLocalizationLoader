using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Modules.QuietOSPatch",
        "This module replaces UI texts of QuietOS.\n"
        + "Attention: This module uses GetComponentsInChildren method\n"
        + "to translate Text/TMPText components\n"
        + "which is easy to slow down the game and cause lags!\n"
        + "For translating static ui texts, it is recommened to\n"
        + "use StaticTextPatch instead."
    )]
    public class QuietOSPatchSettings : ModuleSettingsBase
    {
    }
}

