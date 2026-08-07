using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
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
                NullValueHandling = NullValueHandling.Ignore
            };
            // _conflictedModsInfo.Add(
            //     typeof(AnnouncementSubtitleTimingPatch),
            //     "mimimi-turret.wk-sync-subtitles"
            // );
            if (plugin is null) return;
            _plugin = plugin;
            var loggerName = _plugin.Info.Metadata.Name + "/ModuleManager";
            _logger = Logger.CreateLogSource(loggerName);
            _moduleLoadResult = new ModuleLoadResult(_logger);
        }

        public static void LoadAllModules()
        {
            //LoadModule<ExampleModule>("test.json");
            LoadModule<AchievementPatch>("Achievements.json");
            LoadModule<AnnouncementSubtitlePatch>(
                "AnnouncementSubtitles.json"
            );
            LoadModule<AnnouncementSubtitleTimingPatch>(
                "AnnouncementSubtitleTimings.json"
            );
            LoadModule<CosmeticPatch>("Cosmetics.json");
            LoadModule<DeathTextPatch>("DeathTexts.json");
            LoadModule<DocumentPatch>("Documents.json");
            LoadModule<FacilityUpgradePatch>("FacilityUpgrades.json");
            LoadModule<FontPatch>("Fonts.json");
            LoadModule<FontAssetPatch>("FontAssets.json");
            LoadModule<GamemodePatch>("Gamemodes.json");
            LoadModule<HardcodedStringPatch>("HardcodedStrings.json");
            LoadModule<ItemDescriptionPatch>("ItemDescriptions.json");
            LoadModule<LocationNamePatch>("LocationNames.json");
            LoadModule<MotherSubtitlePatch>("MotherSubtitles.json");
            LoadModule<NotePatch>("Notes.json");
            LoadModule<ObjectivePatch>("Objectives.json");
            LoadModule<PerkPatch>("Perks.json");
            LoadModule<ProgressionUnlockPatch>("ProgressionUnlocks.json");
            LoadModule<QuietOSPatch>("QuietOS.json");
            LoadModule<RecordingSubtitlePatch>("RecordingSubtitles.json");
            LoadModule<RecordingSubtitleTimingPatch>(
                "RecordingSubtitleTimings.json"
            );
            LoadModule<RoachTraderSubtitlePatch>("RoachTraderSubtitles.json");
            LoadModule<StaticTextPatch>("StaticTexts.json");
            LoadModule<TextScrawlPatch>("TextScrawls.json");
            LoadModule<TrinketPatch>("Trinkets.json");
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
                return;
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

