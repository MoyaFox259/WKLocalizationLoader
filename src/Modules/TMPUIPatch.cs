using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    public class TMPUIPatch : ModuleBase<TMPUIPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static TMPUIPatchSettings ModuleSettings;

        [HarmonyPostfix]
        public static void Postfix(TextMeshProUGUI __instance)
        {
            if (!IsEnabled) return;
            TranslateText(__instance);
        }

        public static void TranslateText(TextMeshProUGUI __instance)
        {
            var originalText = __instance.text;
            if (TryGetTextTranslation(originalText, out string translatedText))
            {
                __instance.text = translatedText;
            }
        }

        public static bool TryGetTextTranslation(
            string originalText,
            out string translatedText
        )
        {
            if (
                string.IsNullOrWhiteSpace(originalText)
                || TextTranslations is null
                || !TextTranslations.TryGetValue(
                    originalText,
                    out translatedText
                )
                || translatedText is null
            )
            {
                translatedText = originalText;
                return false;
            }
            return true;
        }

        public static void AddTextTranslation(
            string originalText,
            string translatedText
        )
        {
            TextTranslations ??= new Dictionary<string, string>();
            if (
                string.IsNullOrWhiteSpace(originalText)
                || translatedText is null
            )
            {
                return;
            }
            TextTranslations[originalText] = translatedText;
        }
    }
}

