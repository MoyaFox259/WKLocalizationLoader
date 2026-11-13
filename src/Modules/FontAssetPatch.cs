using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine;
using TMPro;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader.Modules
{
    [CrossGameVersionCompatible]
    // [HarmonyPriority(Priority.HigherThanNormal)]
    // [HarmonyPatch]
    public class FontAssetPatch
        : ModuleBase<FontAssetPatch>, IScriptableObjectPatch
    {
        [JsonProperty]
        public static FontAssetPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, List<FontAssetProperties>>
            CustomFontAssets;
        [JsonProperty]
        public static string CharactersToRender;

        [JsonIgnore]
        public static ValueCollection<string, TMP_FontAsset>
            FallbackFontAssets = new ValueCollection<string, TMP_FontAsset>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            if (!IsEnabled) return;
            foreach (var item in CustomFontAssets)
            {
                var targetFontAssetName = item.Key;
                var customFontAssetPropertiesList = item.Value;
                foreach (var customFontAssetProperties in customFontAssetPropertiesList)
                {
                    CreateAndRegisterFallbackFontAsset(
                        targetFontAssetName,
                        customFontAssetProperties
                    );
                }
            }
        }

        public static void PatchScriptableObjects()
        {
            if (!IsEnabled) return;
            var fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (fontAssets is null || fontAssets.Length == 0) return;
            Array.ForEach(fontAssets, i => PatchFontAsset(i));
        }

        public static void PatchFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset is null) return;
            AddFallbackFontAssets(fontAsset);
        }

        public static void AddFallbackFontAssets(TMP_FontAsset fontAsset)
        {
            if (
                TryGetFallbackFontAssets(
                    fontAsset?.name,
                    out List<TMP_FontAsset> fallbackFontAssets
                )
            )
            {
                if (ModuleSettings.HighFallbackPriority)
                {
                    fontAsset.fallbackFontAssetTable = fallbackFontAssets
                        .Union(fontAsset.fallbackFontAssetTable)
                        .ToList();
                    return;
                }
                fontAsset.fallbackFontAssetTable = fontAsset
                    .fallbackFontAssetTable
                    .Union(fallbackFontAssets)
                    .ToList();
            }
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(
        //     typeof(TextMeshPro),
        //     nameof(TextMeshPro.Awake)
        // )]
        // public static void Postfix_TextMeshPro_Awake(TextMeshPro __instance)
        // {
        //     if (!IsEnabled) return;
        //     AddFallbackFontAssets(__instance);
        // }
        //
        // [HarmonyPostfix]
        // [HarmonyPatch(
        //     typeof(TextMeshProUGUI),
        //     nameof(TextMeshProUGUI.Awake)
        // )]
        // public static void Postfix_TextMeshProUGUI_Awake(
        //     TextMeshPro __instance
        // )
        // {
        //     if (!IsEnabled) return;
        //     AddFallbackFontAssets(__instance);
        // }
        //
        // public static void AddFallbackFontAssets(TMP_Text tmpText)
        // {
        // }

        public static void CreateAndRegisterFallbackFontAsset(
            string targetFontAssetName,
            FontAssetProperties fallbackFontAssetProperties
        )
        {
            if (
                ResourceLoader.TryGetOrCreateFontAsset(
                    CharactersToRender,
                    fallbackFontAssetProperties,
                    out TMP_FontAsset fallbackFontAsset,
                    ModuleSettings.SaveFontAssetCacheOnDisk
                )
            )
            {
                RegisterFallbackFontAsset(
                    targetFontAssetName,
                    fallbackFontAsset
                );
            }
        }

        public static void RegisterFallbackFontAsset(
            string targetFontAssetName,
            TMP_FontAsset fallbackFontAsset
        )
        => FallbackFontAssets?.Add(targetFontAssetName, fallbackFontAsset);

        public static bool TryGetFallbackFontAssets(
            string targetFontAssetName,
            out List<TMP_FontAsset> fallbackFontAssets
        )
        {
            if (
                FallbackFontAssets != null
                && FallbackFontAssets.TryGetValues(
                    targetFontAssetName,
                    out fallbackFontAssets
                )
            )
            {
                return true;
            }
            fallbackFontAssets = null;
            return false;
        }
    }
}

