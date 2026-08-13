using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch]
    public class FontPatch : ModuleBase<FontPatch>
    {
        [JsonProperty]
        public static FontPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, FontProperties> CustomFonts;
        [JsonProperty]
        public static string CharactersToRender;

        [JsonIgnore]
        public static Dictionary<string, Font> SubstituteFonts =
            new Dictionary<string, Font>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            if (!IsEnabled) return;
            foreach (var item in CustomFonts)
            {
                var targetFontName = item.Key;
                var customFontProperties = item.Value;
                CreateAndRegisterSubstituteFont(
                    targetFontName,
                    customFontProperties
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Text),
            nameof(Text.OnEnable)
        )]
        public static void Postfix_Text_OnEnable(Text __instance)
        {
            if (!IsEnabled) return;
            ReplaceFont(__instance);
        }

        public static void ReplaceFont(Text __instance)
        {
            var targetFontName = __instance.font?.name;
            if (TryGetSubstituteFont(targetFontName, out Font substituteFont))
            {
                __instance.font = substituteFont;
            }
        }

        public static void CreateAndRegisterSubstituteFont(
            string targetFontName,
            FontProperties substituteFontProperties
        )
        {
            if (
                ResourceLoader.TryGetOrCreateFont(
                    CharactersToRender,
                    substituteFontProperties,
                    out Font substituteFont,
                    ModuleSettings.SaveFontCacheOnDisk
                )
            )
            {
                RegisterSubstituteFont(targetFontName, substituteFont);
            }
        }

        public static void RegisterSubstituteFont(
            string targetFontName,
            Font substituteFont
        )
        {
            SubstituteFonts ??= new Dictionary<string, Font>();
            if (targetFontName is null || substituteFont is null) return;
            SubstituteFonts[targetFontName] = substituteFont;
        }

        public static bool TryGetSubstituteFont(
            string targetFontName,
            out Font substituteFont
        )
        {
            if (
                targetFontName is null
                || SubstituteFonts is null
                || !SubstituteFonts.TryGetValue(
                    targetFontName,
                    out substituteFont
                )
                || substituteFont is null
            )
            {
                substituteFont = null;
                return false;
            }
            return true;
        }
    }
}

