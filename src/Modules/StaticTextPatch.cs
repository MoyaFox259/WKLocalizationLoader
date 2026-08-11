using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine.UI;
using TMPro;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class StaticTextPatch : TextTranslator<StaticTextPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static StaticTextPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Text),
            nameof(Text.OnEnable)
        )]
        public static void Postfix_Text_OnEnable(Text __instance)
        {
            if (!IsEnabled) return;
            __instance.text = GetTextTranslation(
                TextTranslations,
                __instance.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(TextMeshPro),
            nameof(TextMeshPro.Awake)
        )]
        public static void Postfix_TextMeshPro_Awake(TextMeshPro __instance)
        {
            if (!IsEnabled) return;
            __instance.text = GetTextTranslation(
                TextTranslations,
                __instance.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(TextMeshProUGUI),
            nameof(TextMeshProUGUI.Awake)
        )]
        public static void Postfix_TextMeshProUGUI_Awake(
            TextMeshProUGUI __instance
        )
        {
            if (!IsEnabled) return;
            __instance.text = GetTextTranslation(
                TextTranslations,
                __instance.text
            );
        }
    }
}

