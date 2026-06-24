using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace WKLocalizationLoader.Config
{
    public class ConfigEntryValueProvider : IValueProvider
    {
        private IValueProvider _originalProvider;
        private ConfigSectionAttribute _configSectionAttribute;
        private ConfigEntryAttribute _configEntryAttribute;

        public ConfigEntryValueProvider(
            IValueProvider originalProvider,
            ConfigSectionAttribute configSectionAttribute,
            ConfigEntryAttribute configEntryAttribute
        )
        {
            _originalProvider = originalProvider;
            _configSectionAttribute = configSectionAttribute;
            _configEntryAttribute = configEntryAttribute;
        }

        public object GetValue(object target)
        {
            return _originalProvider?.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            object configEntryValue = null;
            if (
                _configSectionAttribute != null
                && _configEntryAttribute != null
            )
            {
                var (
                    section,
                    moduleDescription
                )
                = _configSectionAttribute;
                var (
                    key,
                    defaultValue,
                    entryDescription
                )
                = _configEntryAttribute;
                configEntryValue = ConfigManager.GetConfigEntryValue(
                    section,
                    moduleDescription,
                    key,
                    defaultValue,
                    entryDescription
                );
            }
            var finalValue = configEntryValue ?? value;
            _originalProvider?.SetValue(target, finalValue);
        }
    }
}

