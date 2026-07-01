using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using HarmonyLib;
using WKLocalizationLoader.Modules;

namespace WKLocalizationLoader
{
    [HarmonyPatch(
        typeof(UT_Intro),
        nameof(UT_Intro.Start)
    )]
    public class GameResourcePatcher
    {
        public static List<Type> ModuleClasses;

        public static void Initialize(List<Type> moduleClasses)
        {
            ModuleClasses = moduleClasses;
            FilterResourcePatchClasses();
        }

        public static void Postfix(UT_Intro __instance)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void OnSceneLoaded(
            Scene scene,
            LoadSceneMode loadSceneMode
        )
        {
            ApplyResourcePatches();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public static void ApplyResourcePatches()
        {
            FilterResourcePatchClasses();
            foreach (var moduleClass in ModuleClasses)
            {
                var patchMethod = moduleClass.GetMethod(
                    "PatchResources",
                    (
                        BindingFlags.Public
                        | BindingFlags.Static
                    )
                );
                patchMethod?.Invoke(null, null);
            }
        }

        public static void FilterResourcePatchClasses()
        {
            if (ModuleClasses is null || ModuleClasses.Count == 0) return;
            ModuleClasses = ModuleClasses
                .Where(
                    m => (
                        typeof(IResourcePatch).IsAssignableFrom(m)
                        && m.GetMethod(
                            "PatchResources",
                            (
                                BindingFlags.Public
                                | BindingFlags.Static
                            )
                        ) != null
                    )
                )
                .ToList();
        }
    }
}

