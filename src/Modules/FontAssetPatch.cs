using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;
using TMPro;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class FontAssetPatch : ModuleBase<FontAssetPatch>
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

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(TMP_FontAsset),
            nameof(TMP_FontAsset.Awake)
        )]
        public static void Postfix_FontAsset_Awake(TMP_FontAsset __instance)
        {
            if (!IsEnabled) return;
            AddFallbackFontAssets(__instance);
        }

        public static void AddFallbackFontAssets(TMP_FontAsset __instance)
        {
            if (
                TryGetFallbackFontAssets(
                    __instance.name,
                    out List<TMP_FontAsset> fallbackFontAssets
                )
            )
            {
                if (ModuleSettings.HighFallbackPriority)
                {
                    __instance.fallbackFontAssetTable = fallbackFontAssets
                        .Union(__instance.fallbackFontAssetTable)
                        .ToList();
                    return;
                }
                __instance.fallbackFontAssetTable = __instance
                    .fallbackFontAssetTable
                    .Union(fallbackFontAssets)
                    .ToList();
            }
        }

        public static void CreateAndRegisterFallbackFontAsset(
            string targetFontName,
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
                RegisterFallbackFontAsset(targetFontName, fallbackFontAsset);
            }
        }

        public static void RegisterFallbackFontAsset(
            string targetFontName,
            TMP_FontAsset fallbackFontAsset
        )
        => FallbackFontAssets?.Add(targetFontName, fallbackFontAsset);

        public static bool TryGetFallbackFontAssets(
            string targetFontName,
            out List<TMP_FontAsset> fallbackFontAssets
        )
        {
            if (
                FallbackFontAssets != null
                && FallbackFontAssets.TryGetValues(
                    targetFontName,
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

