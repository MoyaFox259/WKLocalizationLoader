using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader.Config
{
    public class ModuleContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo memberInfo,
            MemberSerialization memberSerialization
        )
        {
            var jsonProperty = base.CreateProperty(
                memberInfo,
                memberSerialization
            );
            if (
                typeof(ModuleSettingsBase).IsAssignableFrom(
                    memberInfo.DeclaringType
                )
            )
            {
                HandleModuleSettingsMember(jsonProperty, memberInfo);
            }
            return jsonProperty;
        }

        private void HandleModuleSettingsMember(
            JsonProperty jsonProperty,
            MemberInfo memberInfo
        )
        {
            var configSectionAttribute = memberInfo
                .DeclaringType
                .GetCustomAttribute<ConfigSectionAttribute>();
            var configEntryAttribute = memberInfo
                .GetCustomAttribute<ConfigEntryAttribute>();
            if (configSectionAttribute != null && configEntryAttribute != null)
            {
                jsonProperty.DefaultValue = configEntryAttribute.DefaultValue;
                var originalProvider = jsonProperty.ValueProvider;
                var valueProvider = new ConfigEntryValueProvider(
                    originalProvider,
                    configSectionAttribute,
                    configEntryAttribute
                );
                jsonProperty.ValueProvider = valueProvider;
            }
        }
    }
}

