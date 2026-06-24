using System;

namespace WKLocalizationLoader.Config
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false
    )]
    public class ConfigSectionAttribute : Attribute
    {
        public string Section { get; }
        public string ModuleDescription { get; }

        public ConfigSectionAttribute(
            string section,
            string moduleDescription = null
        )
        {
            Section = section;
            ModuleDescription = moduleDescription;
        }

        public void Deconstruct(
            out string section,
            out string moduleDescription
        )
        {
            section = Section;
            moduleDescription = ModuleDescription;
        }
    }
}

