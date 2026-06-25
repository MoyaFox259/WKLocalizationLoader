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
    public class AnnouncementSubtitlePatch
        : ModuleBase<AnnouncementSubtitlePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> AnnouncementSubtitles;

        [JsonIgnore]
        public static AnnouncementSubtitlePatchSettings ModuleSettings;

        public static string Postfix(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "announcements"
                || AnnouncementSubtitles is null
                || !AnnouncementSubtitles.ContainsKey(key)
            )
            {
                return __result;
            }
            return AnnouncementSubtitles[key];
        }
    }
}

