using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class TrinketPatch : TextTranslator<TrinketPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TrinketTypes;
        [JsonProperty]
        public static Dictionary<string, string> TrinketTitles;
        [JsonProperty]
        public static Dictionary<string, string> TrinketDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> TrinketFlavorTexts;
        [JsonProperty]
        public static string TrinketDescriptionTemplate;
        [JsonProperty]
        public static string TrinketLockedDescriptionTemplate;
        [JsonProperty]
        public static string TrinketUnlockProgressTemplate;
        [JsonProperty]
        public static string TooExpensiveText;
        [JsonProperty]
        public static string NoTrinketsAvailableDescription;
        [JsonProperty]
        public static string NoTrinketsInIronKnuckleDescription;

        [JsonIgnore]
        public static TrinketPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Trinket),
            nameof(Trinket.IsUnlocked)
        )]
        public static void Postfix_Trinket_IsUnlocked(Trinket __instance)
        {
            if (!IsEnabled) return;
            __instance.title = GetTextTranslation(
                TrinketTitles,
                __instance.title
            );
            __instance.description = GetTextTranslation(
                TrinketDescriptions,
                __instance.description
            );
            __instance.flavorText = GetTextTranslation(
                TrinketFlavorTexts,
                __instance.flavorText
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Trinket),
            nameof(Trinket.GetDescription)
        )]
        public static string Postfix_Trinket_GetDescription(
            string __result,
            Trinket __instance
        )
        {
            if (!IsEnabled) return __result;
            var trinketType = GetTranslatedTrinketType(__instance);
            var title = GetTrinketTitle(__instance);
            var description = GetTrinketDescription(__instance);
            var descriptionTemplate = TrinketDescriptionTemplate
                ?? "<color=grey>{type}:</color> {title}. {description}\n"
                    + "<color=grey>{flavorText}</color>";
            return descriptionTemplate
                .Replace("{type}", trinketType)
                .Replace("{title}", title)
                .Replace("{description}", description)
                .Replace("{flavorText}", __instance.flavorText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Trinket),
            nameof(Trinket.GetLockedDescription)
        )]
        public static string Postfix_Trinket_GetLockedDescription(
            string __result,
            Trinket __instance
        )
        {
            if (!IsEnabled) return __result;
            var trinketType = GetTranslatedTrinketType(__instance);
            var title = GetTrinketTitle(__instance);
            var unlock = __instance.progressionUnlock;
            var progress = unlock.showProgression
                ? GetTranslatedProgress(unlock)
                : "";
            var descriptionTemplate = TrinketLockedDescriptionTemplate
                ?? "<color=grey>Locked {type}: </color>{title}\n"
                    + "<color=grey>Unlock Requirement: {unlockHint}"
                    + "{progress}</color>";
            return descriptionTemplate
                .Replace("{type}", trinketType)
                .Replace("{title}", title)
                .Replace("{unlockHint}", unlock.unlockHint)
                .Replace("{progress}", progress);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_TrinketPicker),
            nameof(UI_TrinketPicker.ReloadTrinkets)
        )]
        public static void Postfix_TrinketPicker_ReloadTrinkets(
            UI_TrinketPicker __instance
        )
        {
            if (!IsEnabled) return;
            var descriptionText = __instance.descriptionText;
            var currentGamemode = __instance.currentGamemode;
            if (currentGamemode.availableTrinkets is null)
            {
                descriptionText.text = NoTrinketsAvailableDescription
                    ?? "No trinkets available for this gamemode.";
            }
            if (currentGamemode.IsIronKnuckle())
            {
                descriptionText.text = NoTrinketsInIronKnuckleDescription
                    ?? "Trinkets are not available in Iron Knuckle";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_TrinketPicker),
            nameof(UI_TrinketPicker.UpdateTrinketActivation)
        )]
        public static void Postfix_TrinketPicker_UpdateTrinketActivation(
            UI_TrinketPicker __instance
        )
        {
            if (!IsEnabled) return;
            if (__instance.costText.text == "<color=\"red\">Too expensive!")
            {
                __instance.costText.text = TooExpensiveText
                    ?? "<color=\"red\">Too expensive!";
            }
        }

        public static string GetTranslatedTrinketType(Trinket trinket)
        => GetTextTranslation(
            TrinketTypes,
            trinket.isBinding ? "Binding" : "Trinket"
        );

        public static string GetTrinketTitle(Trinket trinket)
        {
            var title = trinket.isBinding ? "<color=red>" : "<color=#FF8AFF>";
            title += "<shimmer s=0.1>" + trinket.title + "</shimmer></color>";
            return title;
        }

        public static string GetTrinketDescription(Trinket trinket)
        => trinket.isBinding
            ? "<shake a=0.01>" + trinket.description + "</shake>"
            : trinket.description;

        public static string GetTranslatedProgress(ProgressionUnlock unlock)
        {
            var progress = unlock.GetProgressString();
            if (progress == "N/A" && TrinketUnlockProgressTemplate != null)
            {
                var progressSegments = progress.Split(new char[] { '/' }, 2);
                if (progressSegments.Length == 2)
                {
                    var current = progressSegments[0];
                    var required = progressSegments[1];
                    progress = TrinketUnlockProgressTemplate
                        .Replace("{current}", current)
                        .Replace("{required}", required);
                }
            }
            return progress;
        }
    }
}

