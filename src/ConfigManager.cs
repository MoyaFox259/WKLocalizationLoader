using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace WKLocalizationLoader
{
    public static class ConfigManager
    {
        private static Plugin _plugin;
        private static ConfigFile _config;
        private static ManualLogSource _logger;

        public static void Initialize(Plugin plugin)
        {
            if (plugin is null) return;
            _plugin = plugin;
            _config = plugin.Config;
            var loggerName = _plugin.Info.Metadata.Name + "/ConfigManager";
            _logger = Logger.CreateLogSource(loggerName);
        }

        public static bool IsModuleEnabled(
            string section,
            string moduleDescription = null
        )
        {
            if (_config is null) return true;
            var configDefinition = new ConfigDefinition(section, "IsEnabled");
            var entryDescription =
                "Set this field to \"false\" to disable this module.";
            if (moduleDescription != null)
            {
                entryDescription = moduleDescription + "\n" + entryDescription;
            }
            else if (
                _config.TryGetEntry<bool>(
                    configDefinition,
                    out ConfigEntry<bool> configEntry
                )
            )
            {
                entryDescription = configEntry.Description.Description;
            }
            var configDescription = new ConfigDescription(entryDescription);
            ConfigEntry<bool> isModuleEnabled = _config.Bind<bool>(
                configDefinition,
                true,
                configDescription
            );
            return isModuleEnabled.Value;
        }

        public static bool IsModuleUserOverridesEnabled(string section)
        {
            if (_config is null) return false;
            ConfigEntry<bool> isModuleUserOverridesEnabled =
                _config.Bind<bool>(
                    section,
                    "EnableUserOverrides",
                    false,
                    "Set this field to \"true\" to "
                    + "apply the custom values below."
                );
            return isModuleUserOverridesEnabled.Value;
        }

        public static object GetConfigEntryValue(
            string section,
            string moduleDescription,
            string key,
            object defaultValue,
            string entryDescription
        )
        {
            if (_config is null) return null;
            var isModuleEnabled = IsModuleEnabled(section, moduleDescription);
            var isModuleUserOverridesEnabled =
                IsModuleUserOverridesEnabled(section);
            var configEntryValue = BindConfigEntryValue(
                section,
                key,
                defaultValue,
                entryDescription
            );
            return (isModuleEnabled && isModuleUserOverridesEnabled)
                ? configEntryValue
                : null;
        }

        public static object BindConfigEntryValue(
            string section,
            string key,
            object defaultValue,
            string entryDescription
        )
        {
            ConfigEntryBase configEntry = defaultValue switch
            {
                bool b => _config.Bind<bool>(
                    section,
                    key,
                    b,
                    entryDescription
                ),
                int i => _config.Bind<int>(
                    section,
                    key,
                    i,
                    entryDescription
                ),
                float f => _config.Bind<float>(
                    section,
                    key,
                    f,
                    entryDescription
                ),
                string s => _config.Bind<string>(
                    section,
                    key,
                    s,
                    entryDescription
                ),
                _ => throw new NotSupportedException(
                    $"\"{defaultValue?.GetType().Name ?? "Null"}\" "
                    + "entry type isn't currently supported."
                )
            };
            return configEntry.BoxedValue;
        }
    }
}

