using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    public abstract class ModuleBase<TModule>
    {
        [JsonIgnore]
        public static bool IsEnabled = false;
        [JsonIgnore]
        public static bool IsPatched = false;

        public ModuleBase()
        {
            var moduleSettingsField = AccessTools.Field(
                this.GetType(),
                "ModuleSettings"
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

