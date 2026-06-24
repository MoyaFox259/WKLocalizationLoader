using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WKLocalizationLoader.Config;

namespace WKLocalizationLoader.Modules
{
    public abstract class ModuleBase<TModule>
        : IModule where TModule : ModuleBase<TModule>
    {
        [JsonIgnore]
        public static bool IsEnabled = false;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            var moduleSettings = this.GetType().GetField("ModuleSettings");
            if (moduleSettings is null) return;
            var configSectionAttribute = moduleSettings.FieldType
                .GetCustomAttribute<ConfigSectionAttribute>();
            if (configSectionAttribute is null) return;
            var (section, moduleDescription) = configSectionAttribute;
            IsEnabled = ConfigManager.IsModuleEnabled(
                section,
                moduleDescription
            );
        }
    }
}

