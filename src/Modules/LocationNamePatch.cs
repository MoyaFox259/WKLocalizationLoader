using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class LocationNamePatch
        : TextTranslator<LocationNamePatch>, IScriptableObjectPatch
    {
        [JsonProperty]
        public static Dictionary<string, string> RegionIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> SubregionIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> LevelIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> LevelSaveNames;
        [JsonProperty]
        public static string ContinueTextTemplate;

        [JsonIgnore]
        public static LocationNamePatchSettings ModuleSettings;

        public static void PatchScriptableObjects()
        {
            PatchRegions();
            PatchSubregions();
        }

        public static void PatchRegions()
        {
            if (!IsEnabled) return;
            var regions = CacheManager.EnumerateScriptableObjects<M_Region>();
            foreach (var region in regions)
            {
                region.introText = GetTextTranslation(
                    RegionIntroTexts,
                    region.introText
                );
            }
        }

        public static void PatchSubregions()
        {
            if (!IsEnabled) return;
            var subregions = CacheManager
                .EnumerateScriptableObjects<M_Subregion>();
            foreach (var subregion in subregions)
            {
                subregion.introText = GetTextTranslation(
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
            __instance.region = GetTextTranslation(
                RegionIntroTexts,
                __instance.region
            );
            __instance.subRegion = GetTextTranslation(
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
            __instance.introText = GetTextTranslation(
                LevelIntroTexts,
                __instance.introText
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_GamemodeScreen),
            nameof(UI_GamemodeScreen.RefreshCurrentGamemode)
        )]
        public static void Postfix_GamemodeScreen_RefreshCurrentGamemode(
            UI_GamemodeScreen __instance
        )
        {
            if (!IsEnabled) return;
            var continueButtonText =
                __instance.currentPanel.continueButtonText.text;
            if (
                !CL_SaveManager.SessionFileExists(
                    __instance.baseGamemode.gamemodeName,
                    CL_GameManager.IsHardmode()
                )
                || CL_GameManager.gamemode.IsCompetitive()
                || CL_GameManager.GetBaseGamemode().IsCompetitive()
                || !continueButtonText.StartsWith("Continue: ")
            )
            {
                return;
            }
            var saveName = continueButtonText.Substring(10);
            saveName = GetTextTranslation(LevelSaveNames, saveName);
            var continueTextTemplate = ContinueTextTemplate
                ?? "Continue: {saveName}";
            __instance.currentPanel.continueButtonText.text =
                continueTextTemplate.Replace("{saveName}", saveName);
        }
    }
}

