using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader
{
    public class ScriptableObjectPatcher
    {
        public static List<Type> ModuleClasses;

        public static void Initialize(List<Type> moduleClasses)
        {
            ModuleClasses = moduleClasses;
            FilterScriptableObjectPatchClasses();
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
                CacheManager.ScanScriptableObjects();
                ApplyScriptableObjectPatches();
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        public static void ApplyScriptableObjectPatches()
        {
            foreach (var moduleClass in ModuleClasses)
            {
                var patchMethod = moduleClass.GetMethod(
                    "PatchScriptableObjects",
                    BindingFlags.Public | BindingFlags.Static
                );
                patchMethod?.Invoke(null, null);
            }
        }

        public static void FilterScriptableObjectPatchClasses()
        {
            if (ModuleClasses is null || ModuleClasses.Count == 0) return;
            ModuleClasses = ModuleClasses
                .Where(
                    m => (
                        typeof(IScriptableObjectPatch).IsAssignableFrom(m)
                        && m.GetMethod(
                            "PatchScriptableObjects",
                            BindingFlags.Public | BindingFlags.Static
                        ) != null
                    )
                )
                .ToList();
        }
    }
}

