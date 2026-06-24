using System;

namespace WKLocalizationLoader.Config
{
    [AttributeUsage(
        AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = false
    )]
    public class ConfigEntryAttribute : Attribute
    {
        public string Key { get; }
        public object DefaultValue { get; }
        public string EntryDescription { get; }

        public ConfigEntryAttribute(
            string key,
            object defaultValue,
            string entryDescription
        )
        {
            Key = key;
            DefaultValue = defaultValue;
            EntryDescription = entryDescription;
        }

        public void Deconstruct(
            out string key,
            out object defaultValue,
            out string entryDescription
        )
        {
            key = Key;
            defaultValue = DefaultValue;
            entryDescription = EntryDescription;
        }
    }
}

