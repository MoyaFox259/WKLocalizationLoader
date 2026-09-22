using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace WKLocalizationLoader
{
    [BepInPlugin(
        "mimimi-turret.wk-localization-loader",
        "WKLocalizationLoader",
        "0.6.4"
    )]
    [BepInProcess("White Knuckle.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> _languageFolder;
        private ConfigEntry<int> _maxScanDepth;

        public static new ManualLogSource Logger;

        public string SupportedGameVersion => "0.60";
        public string LanguageFolder => _languageFolder?.Value;
        public int MaxScanDepth => _maxScanDepth?.Value ?? 5;

        public bool AllowAllModulesOnUnsupportedGameVersion
        {
            get
            {
                var configDefinition = new ConfigDefinition(
                    "Experimental",
                    "AllowAllModulesOnUnsupportedGameVersion"
                );
                if (CheckSupportedGameVersion())
                {
                    if (
                        Config.TryGetEntry<bool>(
                            configDefinition,
                            out ConfigEntry<bool> configEntry
                        )
                    )
                    {
                        configEntry.Value = false;
                        Config.Remove(configDefinition);
                        Config.Save();
                    }
                    return false;
                }
                var configDescription = new ConfigDescription(
                    "Incompatibility Warning:\n"
                    + $"Supported game version: {SupportedGameVersion}\n"
                    + $"Current game version: {Application.version}\n"
                    + _allowAllModulesOnUnsupportedGameVersionDescription
                );
                var allowUnstableModules = Config.Bind<bool>(
                    configDefinition,
                    false,
                    configDescription
                );
                return allowUnstableModules.Value;
            }
        }

        private string _unsupportedGameVersionWarning =>
            "Incompatibility Warning:\n"
            + "You are using the mod on a version of the game "
            + "that the mod does not yet fully support.\n"
            + "\n"
            + "Most modules will be disabled except the following ones:\n"
            + "* StaticTextPatch\n"
            + "* FontPatch\n"
            + "* FontAssetPatch\n"
            + "\n"
            + "If you wish to allow for all modules to load, "
            + "set \"AllowAllModulesOnUnsupportedGameVersion\" "
            + "in mod config to \"true\".\n"
            + "(VERY NOT RECOMMENDED / HIGHLY RISKY!!!)";

        private string _allowAllModulesOnUnsupportedGameVersionWarning =>
            "Incompatibility Warning:\n"
            + "You are forcing all modules to load on a version of the game "
            + "that the mod does not yet fully support.\n"
            + "\n"
            + "To disable this experimental feature, "
            + "set \"AllowAllModulesOnUnsupportedGameVersion\" "
            + "in mod config to \"false\" and restart your game.\n"
            + "\n"
            + "It is VERY RECOMMENDED to keep backups of your save.\n"
            + "Proceed at your own risk. You have been warned!";

        private string _languageFolderDescription =>
            "Specifies the path to a Language Folder.\n"
            + "A relative path is resolved from \"BepInEx\\plugins\\\".\n"
            + "\n"
            + "A Language Folder may contain any of the following files:\n"
            + "* StaticTexts.json\n"
            + "* FontAssets.json\n"
            + "* Fonts\\\n"
            + "* Licenses\\ (licenses of the fonts, etc.)\n"
            + "\n"
            + "Leave this field empty to auto-detect a \n"
            + "Language Folder installed in \"BepInEx\\plugins\\\".\n"
            + "By default, the plugin will load the "
            + "first valid Language Folder it detects.\n"
            + "For further info, see \"MaxScanDepth\" below or plugin wiki.";

        private string _maxScanDepthDescription =>
            "Limits the directory depth "
            + "when auto-detecting Language Folders.\n"
            + "\n"
            + "A Language Folder is detected "
            + "when it contains the following file:\n"
            + "* .wklocalization\n"
            + "Note: This file is for auto-detection purpose only.\n"
            + "It does not store any actual information or data.\n"
            + "\n"
            + "Scanning will start from \"BepInEx\\plugins\\\" "
            + "where the directory depth is 0.";

        private string _allowAllModulesOnUnsupportedGameVersionDescription =>
            "This mod is not tested on current version of the game.\n"
            + "Most modules are likely incompatible with newer or older\n"
            + "versions of the game than the mod supports.\n"
            + "\n"
            + "To prevent game from being unstable "
            + "or data from being corrupted,\n"
            + "most modules will be disabled except the following ones:\n"
            + "* StaticTextPatch\n"
            + "* FontPatch\n"
            + "* FontAssetPatch\n"
            + "\n"
            + "It is VERY RECOMMENDED to:\n"
            + "* Wait for newer updates of this mod.\n"
            + "* Keep backups of your save.\n"
            + "* Keep away from this setting "
            + "if you don't know what it means.\n"
            + "\n"
            + "This is a HIGHLY EXPERIMENTAL / RISKY feature.\n"
            + "Keep backups of your save before proceeding.\n"
            + "\n"
            + "Set this field to \"true\" if you still wish to allow for\n"
            + "all modules to load regardless the incompatibility warning.\n"
            + "Enable this at your own risk. You have been warned!";

        private void Awake()
        {
            Logger = base.Logger;
            if (!CheckSupportedGameVersion())
            {
                var warning = AllowAllModulesOnUnsupportedGameVersion
                    ? _allowAllModulesOnUnsupportedGameVersionWarning
                    : _unsupportedGameVersionWarning;
                Logger.LogWarning(warning);
            }
            Initialize();
            if (string.IsNullOrWhiteSpace(LanguageFolder))
            {
                Logger.LogFatal("Failed to auto-detect Language Folders.");
                return;
            }
            FileManager.Initialize(this);
            ConfigManager.Initialize(this);
            ModuleManager.Initialize(this);
            ResourceLoader.Initialize(this);
            CacheManager.Initialize(this);
            var moduleClasses = LoadAllModules();
            ApplyHarmonyPatches(moduleClasses);
            ApplyScriptableObjectPatches(moduleClasses);
        }

        public void Initialize()
        {
            _languageFolder = Config.Bind<string>(
                "General",
                "LanguageFolder",
                "",
                _languageFolderDescription
            );
            _maxScanDepth = Config.Bind<int>(
                "General",
                "MaxScanDepth",
                5,
                _maxScanDepthDescription
            );
            if (!string.IsNullOrWhiteSpace(LanguageFolder)) return;
            var languageFolders = LanguageScanner.Scan(MaxScanDepth, Logger);
            if (languageFolders.Count == 0) return;
            Logger.LogInfo(
                "Loading the first valid Language Folder detected by default."
            );
            _languageFolder.Value = languageFolders.FirstOrDefault();
        }

        public bool CheckSupportedGameVersion()
        => Application.version.Contains(SupportedGameVersion);

        public List<Type> LoadAllModules()
        {
            ModuleManager.LoadAllModules();
            var modulesClasses = ModuleManager.ModuleInfos
                .Where(m => m.Status == ModuleStatus.OK)
                .Select(m => m.ModuleClass)
                .ToList();
            return modulesClasses;
        }

        public void ApplyHarmonyPatches(List<Type> moduleClasses)
        {
            var harmony = new Harmony(Info.Metadata.GUID);
            foreach (var moduleClass in moduleClasses)
            {
                if (moduleClass.GetCustomAttribute<HarmonyPatch>() != null)
                {
                    var patchClassProcessor = harmony.CreateClassProcessor(
                        moduleClass
                    );
                    patchClassProcessor.Patch();
                }
            }
        }

        public void ApplyScriptableObjectPatches(List<Type> moduleClasses)
        {
            ScriptableObjectPatcher.Initialize(moduleClasses);
        }
    }
}

