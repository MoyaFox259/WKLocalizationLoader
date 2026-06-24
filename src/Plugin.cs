using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using WKLocalizationLoader.Config;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader
{
    [BepInPlugin(
        "mimimi-turret.wk-localization-loader",
        "WKLocalizationLoader",
        "0.3.0")
    ]
    [BepInProcess("White Knuckle.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> _languageFolder;
        private ConfigEntry<int> _maxScanDepth;

        private string _languageFolderDescription =>
            "Specifies the path to a Language Folder.\n"
            + "A relative path is resolved from \"BepInEx\\plugins\\\".\n"
            + "\n"
            + "A Language Folder may contain any of the following files:\n"
            + "* Texts.json\n"
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
            + "when it contains any of the following files:\n"
            + "* .wklocalization\n"
            + "* HawktuahLoadThis\n"
            + "Note: These files are for auto-detection purpose only.\n"
            + "They do not store any actual information or data.\n"
            + "\n"
            + "Scanning will start from \"BepInEx\\plugins\\\" "
            + "where the directory depth is 0.";

        public string LanguageFolder => _languageFolder?.Value;
        public int MaxScanDepth => _maxScanDepth?.Value ?? 5;
        public static new ManualLogSource Logger;

        private void Awake()
        {
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
            LoadAllModules();
        }

        private void Initialize()
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
            Logger = base.Logger;
            if (!string.IsNullOrWhiteSpace(LanguageFolder)) return;
            var languageFolders = LanguageScanner.Scan(MaxScanDepth, Logger);
            if (languageFolders.Count == 0) return;
            Logger.LogInfo(
                "Loading the first valid Language Folder detected by default."
            );
            _languageFolder.Value = languageFolders.FirstOrDefault();
        }

        private void LoadAllModules()
        {
            ModuleManager.LoadAllModules();
            var modules = ModuleManager.FilterModuleClassesByModuleStatus(
                ModuleStatus.OK
            );
            ModuleManager.PrintModuleInfoMessageBySeverity(ModuleStatus.OK);
            ApplyPatches(modules);
        }

        private void ApplyPatches(List<Type> moduleClasses)
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
                    Logger.LogInfo($"\"{moduleClass.Name}\" is patched.");
                }
            }
        }
    }
}

