using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using HarmonyLib;
using WKLocalizationLoader.Config;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader
{
    public static class ModuleManager
    {
        private static Plugin _plugin;
        private static ManualLogSource _logger;
        private static JsonSerializerSettings _jsonSerializerSettings;
        private static ModuleLoadResult _moduleLoadResult;
        private static ValueCollection<Type, string> _conflictedModsInfo =
            new ValueCollection<Type, string>();

        public static void Initialize(Plugin plugin)
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new ModuleContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            _conflictedModsInfo.Add(
                typeof(AnnouncementSubtitleTimingsPatch),
                "mimimi-turret.wk-sync-subtitles"
            );
            if (plugin is null) return;
            _plugin = plugin;
            var loggerName = _plugin.Info.Metadata.Name + "/ModuleManager";
            _logger = Logger.CreateLogSource(loggerName);
            _moduleLoadResult = new ModuleLoadResult(_logger);
        }

        public static void LoadAllModules()
        {
            //LoadModule<ExampleModule>("test.json");
            LoadModule<AnnouncementSubtitlePatch>(
                "AnnouncementSubtitles.json"
            );
            LoadModule<AnnouncementSubtitleTimingsPatch>(
                "AnnouncementSubtitleTimings.json"
            );
            LoadModule<FontPatch>("Fonts.json");
            LoadModule<FontAssetPatch>("FontAssets.json");
            LoadModule<TextPatch>("Texts.json");
            LoadModule<TMPPatch>("TMPTexts.json");
            LoadModule<TMPUIPatch>("TMPUITexts.json");
            //LoadModule<>();
            //LoadModule<>();
            //LoadModule<>();
            //LoadModule<>();
            //LoadModule<>();
            //LoadModule<>();
        }

        public static void LoadModule<TModule>(string fileName)
            where TModule : ModuleBase<TModule>
        {
            var moduleClass = typeof(TModule);
            if (
                DetectConflictedMods(
                    moduleClass,
                    out List<string> conflictedModGUIDs
                )
            )
            {
                _moduleLoadResult.AddConflictedModule(
                    moduleClass,
                    conflictedModGUIDs
                );
                return;
            }
            if (
                !FileManager.TryGetModuleFilePath(
                    fileName,
                    out string filePath
                )
            )
            {
                _moduleLoadResult.AddFileMissingModule(moduleClass);
                return;
            }
            try
            {
                var jsonText = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    throw new InvalidDataException(
                        "File content is empty or whitespace."
                    );
                }
                JsonConvert.DeserializeObject<TModule>(
                    jsonText,
                    _jsonSerializerSettings
                );
            }
            catch (Exception e)
            {
                _moduleLoadResult.AddDeserializationFailedModule(
                    moduleClass,
                    fileName,
                    e
                );
            }
            if (ModuleBase<TModule>.IsEnabled)
            {
                _moduleLoadResult.AddOKModule(moduleClass);
            }
            else
            {
                _moduleLoadResult.AddDisabledModule(moduleClass);
            }
        }

        public static bool DetectConflictedMods(
            Type moduleClass,
            out List<string> conflictedModGUIDs
        )
        {
            conflictedModGUIDs = null;
            if (
                _conflictedModsInfo != null
                && _conflictedModsInfo.TryGetValues(
                    moduleClass,
                    out conflictedModGUIDs
                )
            )
            {
                return conflictedModGUIDs.Any(
                    g => Chainloader.PluginInfos.ContainsKey(g)
                );
            }
            return false;
        }

        public static List<Type> FilterModuleClassesByModuleStatus(
            ModuleStatus status
        )
        => _moduleLoadResult?.FilterModuleClassesByModuleStatus(status);

        public static void PrintModuleInfoMessageBySeverity(
            ModuleStatus minSeverity
        )
        => _moduleLoadResult?.PrintModuleInfoMessageBySeverity(minSeverity);
    }
}

