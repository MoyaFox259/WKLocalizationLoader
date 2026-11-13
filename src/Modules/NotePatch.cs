using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class NotePatch: TextTranslator<NotePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> NoteTexts;
        [JsonProperty]
        public static Dictionary<string, string> RoachTexts;
        [JsonProperty]
        public static string RoachTextTemplate;

        [JsonIgnore]
        public static Dictionary<string, string> TrimmedNoteTexts;
        [JsonIgnore]
        public static NotePatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            if (NoteTexts != null && NoteTexts.Count > 0)
            {
                TrimmedNoteTexts = new Dictionary<string, string>();
                foreach (var translation in NoteTexts)
                {
                    var translatedText = translation.Value;
                    if (translatedText is null) continue;
                    var originalText = translation.Key.TrimStart();
                    TrimmedNoteTexts[originalText] = translatedText;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(HandItem_Note),
            nameof(HandItem_Note.Initialize)
        )]
        public static void Postfix_HandItemNote_Initialize(
            HandItem_Note __instance
        )
        {
            if (!IsEnabled) return;
            __instance.text.text = GetTextTranslation(
                TrimmedNoteTexts,
                __instance.text.text
            );
        }

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
                || group != "roachtext"
                || RoachTexts is null
                || !RoachTexts.ContainsKey(key)
            )
            {
                return __result;
            }
            return RoachTexts[key] ?? __result;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(CL_UIManager),
            nameof(CL_UIManager.SetDescription)
        )]
        public static void Prefix_UIManager_SetDescription(
            ref string t
        )
        {
            if (
                IsEnabled
                && t.StartsWith("<color=#89501A>[ROACH MODE] ")
                && t.EndsWith("</color>")
                && RoachTextTemplate != null
            )
            {
                var roachText = t.Substring(28, t.Length - 36);
                t = RoachTextTemplate.Replace("{text}", roachText);
            }
        }
    }
}

