using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class ClosedCaptionSubtitlePatch
        : ModuleBase<ClosedCaptionSubtitlePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> ClosedCaptionSubtitles;

        [JsonIgnore]
        public static ClosedCaptionSubtitlePatchSettings ModuleSettings;

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
                || group != "sound"
                || ClosedCaptionSubtitles is null
                || !ClosedCaptionSubtitles.ContainsKey(key)
            )
            {
                return __result;
            }
            return ClosedCaptionSubtitles[key] ?? __result;
        }
    }
}

