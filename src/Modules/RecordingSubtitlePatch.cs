using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch(
        typeof(CL_LocalizationManager.Localization),
        nameof(CL_LocalizationManager.Localization.GetLine)
    )]
    public class RecordingSubtitlePatch
        : ModuleBase<RecordingSubtitlePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> RecordingSubtitles;

        [JsonIgnore]
        public static RecordingSubtitlePatchSettings ModuleSettings;

        public static string Postfix(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "recordings"
                || RecordingSubtitles is null
                || !RecordingSubtitles.ContainsKey(key)
            )
            {
                return __result;
            }
            return RecordingSubtitles[key];
        }
    }
}

