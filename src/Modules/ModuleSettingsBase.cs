using System;
using System.Reflection;
using System.Runtime.Serialization;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    public class ModuleSettingsBase
    {
        public ModuleSettingsBase()
        {
            var moduleSettingsClass = this.GetType();
            var configSectionAttribute = moduleSettingsClass
                .GetCustomAttribute<ConfigSectionAttribute>();
            var fields = moduleSettingsClass.GetFields();
            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var field = fields[fieldIndex];
                var configEntryAttribute =
                    field.GetCustomAttribute<ConfigEntryAttribute>();
                PopulateModuleSettingsField(
                    this,
                    field,
                    configSectionAttribute,
                    configEntryAttribute
                );
            }
        }

        private void PopulateModuleSettingsField(
            ModuleSettingsBase moduleSettings,
            FieldInfo field,
            ConfigSectionAttribute configSectionAttribute,
            ConfigEntryAttribute configEntryAttribute
        )
        {
            if (
                field.GetValue(moduleSettings) != null
                && configEntryAttribute is null
            )
            {
                return;
            }
            field.SetValue(moduleSettings, configEntryAttribute.DefaultValue);
            if (configSectionAttribute is null) return;
            var (
                section,
                moduleDescription
            )
            = configSectionAttribute;
            var (
                key,
                defaultValue,
                entryDescription
            )
            = configEntryAttribute;
            var configEntryValue = ConfigManager.GetConfigEntryValue(
                section,
                moduleDescription,
                key,
                defaultValue,
                entryDescription
            );
            field.SetValue(moduleSettings, configEntryValue);
        }
    }
}

