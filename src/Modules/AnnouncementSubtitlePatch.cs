using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch(typeof(CL_LocalizationManager), "Awake")]
    public class AnnouncementSubtitlePatch
        : ModuleBase<AnnouncementSubtitlePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> AnnouncementSubtitles;

        [JsonIgnore]
        public static AnnouncementSubtitlePatchSettings ModuleSettings;

        public static void Postfix(CL_LocalizationManager __instance)
        {
            if (!IsEnabled || AnnouncementSubtitles is null) return;
            CL_LocalizationManager.currentLocalization.announcements =
                AnnouncementSubtitles;
        }
    }
}

