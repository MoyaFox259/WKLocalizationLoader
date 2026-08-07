using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class DeathTextPatch : TextTranslator<DeathTextPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> DeathMessages;
        [JsonProperty]
        public static Dictionary<string, string> DeathTips;

        [JsonIgnore]
        public static DeathTextPatchSettings ModuleSettings;

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
                || group != "deathmessages"
                || DeathMessages is null
                || !DeathMessages.ContainsKey(key)
            )
            {
                return __result;
            }
            return DeathMessages[key] ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_ScoreScreen),
            nameof(UI_ScoreScreen.SetTip)
        )]
        public static void Postfix_ScoreScreen_SetTip(
            UI_ScoreScreen __instance
        )
        {
            if (
                !IsEnabled
                || !__instance.useDeathText
                || __instance.tipText is null
            )
            {
                return;
            }
            __instance.tipText.text = GetTextTranslation(
                DeathTips,
                __instance.tipText.text
            );
        }
    }
}

