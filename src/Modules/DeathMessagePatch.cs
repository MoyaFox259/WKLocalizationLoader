using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch(
        typeof(CL_LocalizationManager.Localization),
        nameof(CL_LocalizationManager.Localization.GetLine)
    )]
    public class DeathMessagePatch
        : ModuleBase<DeathMessagePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> DeathMessages;

        [JsonIgnore]
        public static DeathMessagePatchSettings ModuleSettings;

        public static string Postfix(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "deathmessages"
                || DeathMessages is null
                || !DeathMessages.ContainsKey(key)
            )
            {
                return __result;
            }
            return DeathMessages[key];
        }
    }
}

