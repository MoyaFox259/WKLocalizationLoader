using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class ItemDescriptionPatch : ModuleBase<ItemDescriptionPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> ItemDescriptions;

        [JsonIgnore]
        public static ItemDescriptionPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CL_LocalizationManager.Localization),
            nameof(CL_LocalizationManager.Localization.GetLine)
        )]
        public static string Postfix_Localization_GetLine(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "items"
                || ItemDescriptions is null
                || !ItemDescriptions.ContainsKey(key)
            )
            {
                return __result;
            }
            return ItemDescriptions[key] ?? __result;
        }
    }
}

