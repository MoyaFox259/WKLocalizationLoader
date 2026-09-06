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
        private static List<ModuleInfo> _moduleInfos;
        private static ValueCollection<Type, string> _conflictedModGUIDs;

        public static List<ModuleInfo> ModuleInfos
        {
            get
            {
                if (_moduleInfos is null)
                {
                    _logger?.LogWarning("ModuleInfos is null.");
                    return new List<ModuleInfo>();
                }
                return _moduleInfos;
            }
        }

        public static void Initialize(Plugin plugin)
        {
            if (plugin != null)
            {
                _plugin = plugin;
                var loggerName = _plugin.Info.Metadata.Name + "/ModuleManager";
                _logger = Logger.CreateLogSource(loggerName);
            }
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new ModuleContractResolver()
            };
            _moduleInfos = new List<ModuleInfo>();
            _conflictedModGUIDs = new ValueCollection<Type, string>();
            // _conflictedModGUIDs.Add(
            //     typeof(AnnouncementSubtitleTimingPatch),
            //     "mimimi-turret.wk-sync-subtitles"
            // );
        }

        public static void LoadAllModules()
        {
            //LoadModule<ExampleModule>("Example.json");
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
            LoadModule<GameplayTextPatch>("GameplayTexts.json");
            LoadModule<ItemDescriptionPatch>("ItemDescriptions.json");
            LoadModule<LocationNamePatch>("LocationNames.json");
            LoadModule<MainMenuPatch>("MainMenu.json");
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
            LoadModule<ScoreScreenPatch>("ScoreScreen.json");
            LoadModule<StaticTextPatch>("StaticTexts.json");
            LoadModule<TextScrawlPatch>("TextScrawls.json");
            LoadModule<TrinketPatch>("Trinkets.json");
        }

        public static void LoadModule<TModule>(string fileName)
            where TModule : ModuleBase<TModule>
        {
            var moduleClass = typeof(TModule);
            if (
                CheckConflictedMods(
                    moduleClass,
                    out List<string> conflictedModGUIDs
                )
            )
            {
                RegisterConflictedModule(moduleClass, conflictedModGUIDs);
                return;
            }
            if (
                !FileManager.TryGetModuleFilePath(
                    fileName,
                    out string filePath
                )
            )
            {
                RegisterFileMissingModule(moduleClass);
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
                RegisterDeserializationFailedModule(moduleClass, fileName, e);
                return;
            }
            if (!ModuleBase<TModule>.IsEnabled)
            {
                RegisterDisabledModule(moduleClass);
                return;
            }
            RegisterOKModule(moduleClass);
        }

        public static void RegisterOKModule(Type moduleClass)
        {
            var message = $"Loaded \"{moduleClass.Name}\" successfully.";
            RegisterModule(moduleClass, ModuleStatus.OK, message);
        }

        public static void RegisterDisabledModule(Type moduleClass)
        {
            var message =
                $"\"{moduleClass.Name}\" "
                + "is loaded but manually disabled in config.";
            RegisterModule(moduleClass, ModuleStatus.Disabled, message);
        }

        public static void RegisterFileMissingModule(Type moduleClass)
        {
            var message =
                $"\"{moduleClass.Name}\" is missing its "
                + "associated .json file and disabled by default.";
            RegisterModule(moduleClass, ModuleStatus.Disabled, message);
        }

        public static void RegisterConflictedModule(
            Type moduleClass,
            List<string> conflictedModGUIDs
        )
        {
            string message = null;
            if (conflictedModGUIDs is null || conflictedModGUIDs.Count == 0)
            {
                message =
                    $"\"{moduleClass.Name}\" is disabled to avoid conflicts.";
            }
            else
            {
                message =
                    $"\"{moduleClass.Name}\" is disabled "
                    + "to avoid conflicts with the following mod(s):\n"
                    + string.Join("\n", conflictedModGUIDs);
            }
            RegisterModule(moduleClass, ModuleStatus.Conflicted, message);
        }

        public static void RegisterDeserializationFailedModule(
            Type moduleClass,
            string filePath,
            Exception e
        )
        {
            var message = "An error occurred while deserializing "
                + $"\"{moduleClass.Name}\" from \"{filePath}\".\n"
                + e.Message;
            RegisterModule(moduleClass, ModuleStatus.Failed, message);
        }

        public static void RegisterModule(
            Type moduleClass,
            ModuleStatus status,
            string message
        )
        {
            _moduleInfos ??= new List<ModuleInfo>();
            var moduleInfo = _moduleInfos
                .FirstOrDefault(m => m.ModuleClass == moduleClass);
            if (moduleInfo is null)
            {
                moduleInfo = new ModuleInfo(moduleClass, status, message);
                _moduleInfos.Add(moduleInfo);
                return;
            }
            moduleInfo.Status = status;
            moduleInfo.Message = message;
        }

        public static bool CheckConflictedMods(
            Type moduleClass,
            out List<string> conflictedModGUIDs
        )
        {
            conflictedModGUIDs = null;
            if (
                _conflictedModGUIDs != null
                && _conflictedModGUIDs.TryGetValues(
                    moduleClass,
                    out conflictedModGUIDs
                )
            )
            {
                return conflictedModGUIDs.Any(
                    g => (
                        Chainloader.PluginInfos.ContainsKey(g)
                        || Harmony.HasAnyPatches(g)
                    )
                );
            }
            return false;
        }
    }
}

