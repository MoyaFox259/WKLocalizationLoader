using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TMPro;
using HarmonyLib;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch(typeof(TMP_FontAsset), "Awake")]
    public class FontAssetPatch : ModuleBase<FontAssetPatch>
    {
        [JsonProperty]
        public static FontAssetPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, List<FontAssetProperties>>
            FontAssetInfos;
        [JsonProperty]
        public static string CharactersToRender;

        [JsonIgnore]
        public static ValueCollection<string, TMP_FontAsset>
            FallbackFontAssets = new ValueCollection<string, TMP_FontAsset>();

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            foreach (var fontAssetInfo in FontAssetInfos)
            {
                var targetFontAssetName = fontAssetInfo.Key;
                var fontAssetPropertiesList = fontAssetInfo.Value;
                foreach (var fontAssetProperties in fontAssetPropertiesList)
                if (
                    ResourceLoader.TryGetOrCreateFontAsset(
                        CharactersToRender,
                        fontAssetProperties,
                        out TMP_FontAsset fontAsset,
                        ModuleSettings.SaveFontAssetCacheOnDisk
                    )
                )
                {
                    FallbackFontAssets?.Add(targetFontAssetName, fontAsset);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(TMP_FontAsset __instance)
        {
            if (!IsEnabled) return;
            AddFallbackFontAssets(__instance);
        }

        public static void AddFallbackFontAssets(TMP_FontAsset __instance)
        {
            if (
                FallbackFontAssets != null
                && FallbackFontAssets.TryGetValues(
                    __instance?.name,
                    out List<TMP_FontAsset> fallbackFontAssets
                )
            )
            {
                __instance.fallbackFontAssetTable = __instance
                    .fallbackFontAssetTable
                    .Union(fallbackFontAssets)
                    .ToList();
            }
        }
    }
}

