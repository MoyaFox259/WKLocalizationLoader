using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class CosmeticPatch: TextTranslator<CosmeticPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> CosmeticDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> PaletteNames;
        [JsonProperty]
        public static string PaletteTextTemplate;

        [JsonIgnore]
        public static CosmeticPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_CosmeticInfoPanel),
            nameof(UI_CosmeticInfoPanel.Open)
        )]
        public static void Postfix_CosmeticInfoPanel_Open(
            UI_CosmeticInfoPanel __instance
        )
        {
            if (!IsEnabled) return;
            __instance.descText.text = GetTextTranslation(
                CosmeticDescriptions,
                __instance.descText.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_CosmeticInfoPanel),
            nameof(UI_CosmeticInfoPanel.UpdateSprite)
        )]
        public static void Postfix_CosmeticInfoPanel_UpdateSprite(
            UI_CosmeticInfoPanel __instance
        )
        {
            if (!IsEnabled) return;
            TranslatePaletteText(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_CosmeticInfoPanel),
            nameof(UI_CosmeticInfoPanel.ChangePalette)
        )]
        public static void Postfix_CosmeticInfoPanel_ChangePalette(
            UI_CosmeticInfoPanel __instance
        )
        {
            if (!IsEnabled) return;
            TranslatePaletteText(__instance);
        }

        public static void TranslatePaletteText(UI_CosmeticInfoPanel infoPanel)
        {
            var cosmetic = infoPanel.selectedCosmetic;
            if (cosmetic.cosmeticInfo.tag != "hand") return;
            var handCosmetic = cosmetic as Cosmetic_HandItem;
            var palettes = handCosmetic.cosmeticData.palettes;
            if (palettes is null || palettes.Count == 0) return;
            var palette = palettes[handCosmetic.currentPaletteId];
            var paletteName = GetTextTranslation(PaletteNames, palette.title);
            var paletteIndex = handCosmetic.currentPaletteId + 1;
            var paletteCount = palettes.Count;
            var paletteTextTemplate = PaletteTextTemplate
                ?? "{name} ({current}/{count})";
            infoPanel.debugText.text = paletteTextTemplate
                .Replace("{name}", paletteName)
                .Replace("{current}", paletteIndex.ToString())
                .Replace("{count}", paletteCount.ToString());
        }
    }
}

