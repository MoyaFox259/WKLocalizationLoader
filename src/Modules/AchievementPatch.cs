using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class AchievementPatch : TextTranslator<AchievementPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> AchievementTitles;
        [JsonProperty]
        public static Dictionary<string, string> AchievementDescriptions;

        [JsonIgnore]
        public static AchievementPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CL_AchievementManager),
            nameof(CL_AchievementManager.Awake)
        )]
        public static void Postfix_AchievementManager_Awake(
            CL_AchievementManager __instance
        )
        {
            if (!IsEnabled) return;
            foreach (var achievement in __instance.achievements)
            {
                if (!achievement.announce) continue;
                achievement.name = GetTextTranslation(
                    AchievementTitles,
                    achievement.name
                );
                achievement.announceText = GetTextTranslation(
                    AchievementDescriptions,
                    achievement.announceText
                );
            }
        }
    }
}

