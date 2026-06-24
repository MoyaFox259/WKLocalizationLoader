using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Text), "OnEnable")]
    public class FontPatch : ModuleBase<FontPatch>
    {
        [JsonProperty]
        public static FontPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, FontProperties> FontInfos;
        [JsonProperty]
        public static string CharactersToRender;

        [JsonIgnore]
        public static Dictionary<string, Font> SubstituteFonts =
            new Dictionary<string, Font>();

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            foreach (var fontInfo in FontInfos)
            {
                if (
                    ResourceLoader.TryGetOrCreateFont(
                        CharactersToRender,
                        fontInfo.Value,
                        out Font font,
                        ModuleSettings.SaveFontCacheOnDisk
                    )
                )
                {
                    RegisterSubstituteFont(fontInfo.Key, font);
                }
            }
        }

        public static void Postfix(Text __instance)
        {
            if (!IsEnabled) return;
            ReplaceFont(__instance);
        }

        public static void ReplaceFont(Text __instance)
        {
            var originalFontName = __instance.font?.name;
            if (
                TryGetSubstituteFont(
                    originalFontName,
                    out Font substituteFont
                )
            )
            {
                Canvas.ForceUpdateCanvases();
                __instance.font = substituteFont;
                //__instance.material = null;
                __instance.SetAllDirty();
            }
        }

        public static bool TryGetSubstituteFont(
            string originalFontName,
            out Font substituteFont
        )
        {
            if (
                originalFontName is null
                || SubstituteFonts is null
                || !SubstituteFonts.TryGetValue(
                    originalFontName,
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

        public static void RegisterSubstituteFont(
            string originalFontName,
            Font substituteFont
        )
        {
            SubstituteFonts ??= new Dictionary<string, Font>();
            if (originalFontName is null || substituteFont is null) return;
            SubstituteFonts[originalFontName] = substituteFont;
        }
    }
}

