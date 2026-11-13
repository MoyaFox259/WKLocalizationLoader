using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.SceneManagement;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader
{
    public class ScriptableObjectPatcher
    {
        public static List<Type> ModuleClasses;

        public static void Initialize(List<Type> moduleClasses)
        {
            ModuleClasses = moduleClasses
                ?.Where(m => IsScriptableObjectPatchClass(m))
                .ToList();
            if (ModuleClasses is null || ModuleClasses.Count == 0) return;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void OnSceneLoaded(
            Scene scene,
            LoadSceneMode loadSceneMode
        )
        {
            if (scene.name == "Main-Menu")
            {
                ApplyScriptableObjectPatches();
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        public static void ApplyScriptableObjectPatches()
        {
            if (ModuleClasses is null || ModuleClasses.Count == 0) return;
            foreach (var moduleClass in ModuleClasses)
            {
                AccessTools
                    .Method(moduleClass, "PatchScriptableObjects")
                    ?.Invoke(null, null);
                AccessTools
                    .Field(moduleClass, "IsPatched")
                    ?.SetValue(null, true);
            }
        }

        public static bool IsScriptableObjectPatchClass(Type moduleClass) => (
            moduleClass != null
            && typeof(IScriptableObjectPatch).IsAssignableFrom(moduleClass)
            && AccessTools.Method(
                moduleClass,
                "PatchScriptableObjects"
            ) != null
        );
    }
}

