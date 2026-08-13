using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class RoachTraderSubtitlePatch
        : ModuleBase<RoachTraderSubtitlePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> RoachTraderSubtitles;

        [JsonIgnore]
        public static RoachTraderSubtitlePatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CL_LocalizationManager.Localization),
            nameof(CL_LocalizationManager.Localization.GetLine)
        )]
        public static string Postfix_Localization_GetLine(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "roachtrader"
                || RoachTraderSubtitles is null
                || !RoachTraderSubtitles.ContainsKey(key)
            )
            {
                return __result;
            }
            return RoachTraderSubtitles[key];
        }
    }
}

