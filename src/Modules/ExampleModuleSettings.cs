#if false

using System;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    [ConfigSection(
        "Module.Example",
        "This is an example configuration for demonstration purposes."
    )]
    public class ExampleModuleSettings : ModuleSettingsBase
    {
        [ConfigEntry(
            "Paragraphs",
            67,
            "Number of Lorem Ipsum paragraphs to insert elsewhere."
        )]
        public int Paragraphs;

        [ConfigEntry(
            "PlaceholderText",
            "PlaceholdarText",
            "PlaceholderText"
        )]
        public string PlaceholderText;
    }
}

#endif

