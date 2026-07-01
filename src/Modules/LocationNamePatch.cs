using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class LocationNamePatch
        : ModuleBase<LocationNamePatch>, IResourcePatch
    {
        [JsonProperty]
        public static Dictionary<string, string> RegionIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> SubregionIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> LevelIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> LevelSaveNames;

        [JsonIgnore]
        public static LocationNamePatchSettings ModuleSettings;

        public static void PatchResources()
        {
            PatchRegionIntroText();
            PatchSubegionIntroText();
            Plugin.Logger?.LogWarning("PATCHING");
        }

        public static void PatchRegionIntroText()
        {
            if (!IsEnabled) return;
            var regions = Resources.FindObjectsOfTypeAll<M_Region>();
            Plugin.Logger?.LogWarning($"region count: {regions.Length}");
            for (
                int regionIndex = 0;
                regionIndex < regions.Length;
                regionIndex++
            )
            {
                var region = regions[regionIndex];
                region.introText = GetTranslationText(
                    RegionIntroTexts,
                    region.introText
                );
            }
        }

        public static void PatchSubegionIntroText()
        {
            if (!IsEnabled) return;
            var subregions = Resources.FindObjectsOfTypeAll<M_Subregion>();
            Plugin.Logger?.LogWarning($"subregion count: {subregions.Length}");
            for (
                int subregionIndex = 0;
                subregionIndex < subregions.Length;
                subregionIndex++
            )
            {
                var subregion = subregions[subregionIndex];
                subregion.introText = GetTranslationText(
                    SubregionIntroTexts,
                    subregion.introText
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UT_ZoneTitler),
            nameof(UT_ZoneTitler.Start)
        )]
        public static void Postfix_ZoneTitler_Start(UT_ZoneTitler __instance)
        {
            if (!IsEnabled) return;
            __instance.region = GetTranslationText(
                RegionIntroTexts,
                __instance.region
            );
            __instance.subRegion = GetTranslationText(
                SubregionIntroTexts,
                __instance.subRegion
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(M_Level),
            nameof(M_Level.Awake)
        )]
        public static void Postfix_Level_Awake(M_Level __instance)
        {
            if (!IsEnabled) return;
            __instance.introText = GetTranslationText(
                LevelIntroTexts,
                __instance.introText
            );
            __instance.saveName = GetTranslationText(
                LevelSaveNames,
                __instance.saveName
            );
        }

        public static string GetTranslationText(
            Dictionary<string, string> textTranslations,
            string originalText
        )
        {
            if (
                string.IsNullOrWhiteSpace(originalText)
                || textTranslations is null
                || !textTranslations.ContainsKey(originalText)
            )
            {
                return originalText;
            }
            return textTranslations[originalText] ?? originalText;
        }
    }
}

