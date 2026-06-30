using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(Text), "OnEnable")]
    public class TextPatch : ModuleBase<TextPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static TextPatchSettings ModuleSettings;

        [HarmonyPostfix]
        public static void Postfix(Text __instance)
        {
            if (!IsEnabled) return;
            TranslateText(__instance);
        }

        public static void TranslateText(Text __instance)
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

