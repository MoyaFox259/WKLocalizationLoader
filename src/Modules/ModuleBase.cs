using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    public abstract class ModuleBase<TModule>
    {
        [JsonIgnore]
        public static bool IsEnabled = false;

        public ModuleBase()
        {
            var moduleSettingsField = this
                .GetType()
                .GetField(
                    "ModuleSettings",
                    BindingFlags.Public | BindingFlags.Static
                );
            if (moduleSettingsField is null) return;
            var configSectionAttribute = moduleSettingsField.FieldType
                .GetCustomAttribute<ConfigSectionAttribute>();
            if (configSectionAttribute != null)
            {
                var (section, moduleDescription) = configSectionAttribute;
                IsEnabled = ConfigManager.IsModuleEnabled(
                    section,
                    moduleDescription
                );
            }
            if (IsEnabled && moduleSettingsField.GetValue(this) is null)
            {
                var moduleSettings =
                    Activator.CreateInstance(moduleSettingsField.FieldType);
                moduleSettingsField.SetValue(this, moduleSettings);
            }
        }
    }
}

