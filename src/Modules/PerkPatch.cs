using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine.UI;
using TMPro;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class PerkPatch : TextTranslator<PerkPatch>, IScriptableObjectPatch
    {
        [JsonProperty]
        public static Dictionary<string, string> PerkTitles;
        [JsonProperty]
        public static Dictionary<string, string> PerkDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> PerkFlavorTexts;
        [JsonProperty]
        public static Dictionary<string, string> DurationRoughTexts;
        [JsonProperty]
        public static string DurationSecondTemplate;
        [JsonProperty]
        public static string DurationSecondsTemplate;
        [JsonProperty]
        public static string AppPerkHoverTextTemplate;
        [JsonProperty]
        public static string AppPerkAmountTemplate;
        [JsonProperty]
        public static string AppRefreshPurchasedText;

        [JsonIgnore]
        public readonly static Regex SecondsFormatRegex = CacheManager
            .GetOrCreateRegex(
                @"\{.*?\^s.*?\}",
                RegexOptions.Compiled
            );
        [JsonIgnore]
        public readonly static Regex SecondsRegex = CacheManager
            .GetOrCreateRegex(
                @"([+-]?\d+(?:\.\d+)?) Seconds?",
                RegexOptions.Compiled
            );
        [JsonIgnore]
        public static PerkPatchSettings ModuleSettings;

        public static void PatchScriptableObjects()
        {
            if (!IsEnabled) return;
            PatchPerks();
        }

        public static void PatchPerks()
        {
            var perks = CacheManager.EnumerateScriptableObjects<Perk>();
            foreach (var perk in perks)
            {
                perk.title = GetTextTranslation(
                    PerkTitles,
                    perk.title
                );
                perk.description = GetTextTranslation(
                    PerkDescriptions,
                    perk.description
                );
                perk.flavorText = GetTextTranslation(
                    PerkFlavorTexts,
                    perk.flavorText
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Perk),
            nameof(Perk.GetTitle)
        )]
        public static string Postfix_Perk_GetTitle(
            string __result,
            Perk __instance
        )
        {
            if (
                !IsEnabled
                || (
                    __instance.perkType != Perk.PerkType.trinket
                    && __instance.perkType != Perk.PerkType.binding
                )
            )
            {
                return __result;
            }
            var trinketType = GetTranslatedTrinketType(__instance);
            var title = GetTrinketPerkTitle(__instance);
            return trinketType
                + "\n<shimmer s=0.1>"
                + title
                + "</shimmer></color>";
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Perk),
            nameof(Perk.GetDescription)
        )]
        public static string Postfix_Perk_GetDescription(
            string __result,
            Perk __instance
        )
        {
            if (
                !IsEnabled
                || !SecondsFormatRegex.IsMatch(__instance.description)
            )
            {
                return __result;
            }
            return SecondsRegex.Replace(
                __result,
                m => {
                    var time = m.Groups[1].Value;
                    var timeTemplate = time == "1"
                        ? DurationSecondTemplate ?? "{time} Second"
                        : DurationSecondsTemplate ?? "{time} Seconds";
                    return timeTemplate.Replace("{time}", time);
                }
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(PerkModule_RemovalTimer),
            nameof(PerkModule_RemovalTimer.GetCounterString)
        )]
        public static string Postfix_RemovalTimerModule_GetCounterString(
            string __result,
            PerkModule_RemovalTimer __instance
        )
        {
            if (
                !IsEnabled
                || __instance.removalTimerDisplayType
                != PerkModule_RemovalTimer.RemovalTimerDisplayType.rough
            )
            {
                return __result;
            }
            var stylePrefix = __instance.removalTimerPrefix;
            var removeTime = __result.Substring(stylePrefix.Length);
            removeTime = GetTextTranslation(DurationRoughTexts, removeTime);
            return stylePrefix + removeTime;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_PerkPage),
            nameof(App_PerkPage.GenerateIcons)
        )]
        public static void Postfix_PerkPage_GenerateIcons(
            App_PerkPage __instance
        )
        {
            if (!IsEnabled) return;
            var icons = __instance.iconParent.GetComponentsInChildren<Image>();
            if (icons is null || icons.Length == 0) return;
            var perks = CL_GameManager.gMan.localPlayer.perks;
            if (perks is null || perks.Count == 0) return;
            var iconMapping = perks.ToDictionary(p => p.icon, p => p);
            for (var iconIndex = 0; iconIndex < icons.Length; iconIndex++)
            {
                var icon = icons[iconIndex];
                var tooltip = icon.GetComponent<OS_Tooltip>();
                if (
                    tooltip != null
                    && iconMapping.TryGetValue(icon.sprite, out Perk perk)
                )
                {
                    var title = perk.GetTitle(includeColor: true);
                    var amount = "<color=#C7C7C7>"
                        + GetTranslatedAppPerkAmount(perk, isPreview: false);
                    var description = "<color=\"grey\">"
                        + perk.GetDescription(adjusted: true, total: true)
                        + "<color>";
                    var descriptionTemplate = AppPerkHoverTextTemplate
                        ?? "{title}{amount}\n{description}";
                    tooltip.tip = descriptionTemplate
                        .Replace("{title}", title)
                        .Replace("{amount}", amount)
                        .Replace("{description}", description);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_PerkPage_Card),
            nameof(App_PerkPage_Card.Initialize)
        )]
        public static void Postfix_PerkPageCard_Initialize(
            App_PerkPage_Card __instance,
            App_PerkPage page,
            Perk p
        )
        {
            if (!IsEnabled) return;
            var title = p.GetTitle(includeColor: true);
            var amount = "<color=\"grey\">"
                + GetTranslatedAppPerkAmount(p, isPreview: true);
            var description = "<color=#C7C7C7>"
                + p.GetDescription(adjusted: true)
                + "</color>";
            var descriptionTemplate = AppPerkHoverTextTemplate
                ?? "{title}{amount}\n{description}";
            __instance.tooltip.tip = descriptionTemplate
                .Replace("{title}", title)
                .Replace("{amount}", amount)
                .Replace("{description}", description);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_PerkPage),
            nameof(App_PerkPage.PurchaseRefresh)
        )]
        public static void Postfix_PerkPage_PurchaseRefresh(
            App_PerkPage __instance
        )
        {
            if (!IsEnabled || AppRefreshPurchasedText is null) return;
            var refreshUI = __instance.reloadSettingsRoot;
            var tmpTexts = refreshUI.GetComponentsInChildren<TMP_Text>();
            if (tmpTexts is null || tmpTexts.Length == 0) return;
            for (var tmpIndex = 0; tmpIndex < tmpTexts.Length; tmpIndex++)
            {
                var tmpText = tmpTexts[tmpIndex];
                if (tmpText.text == "<color=\"red>PURCHASED</color>")
                {
                    tmpText.text = AppRefreshPurchasedText;
                    return;
                }
            }
        }

        public static string GetTranslatedTrinketType(Perk perk)
        {
            var isBinding = perk.perkType == Perk.PerkType.binding;
            var trinketType = GetTextTranslation(
                TrinketPatch.TrinketTypes,
                isBinding ? "Binding" : "Trinket"
            );
            return isBinding
                ? "<color=#808080>" + trinketType
                : "<color=#FF8AFF>" + trinketType;
        }

        public static string GetTrinketPerkTitle(Perk perk)
        => perk.perkType == Perk.PerkType.binding
            ? "<shake a=0.01>" + perk.title + "</shake>"
            : perk.title;

        public static string GetTranslatedAppPerkAmount(
            Perk perk,
            bool isPreview
        )
        {
            if (perk is null) return "";
            var amount = isPreview
                ? $"{perk.stackAmount + 1}"
                : $"{perk.stackAmount}";
            var amountTemplate = AppPerkAmountTemplate ?? " ({amount}x)";
            return amountTemplate.Replace("{amount}", amount);
        }
    }
}

