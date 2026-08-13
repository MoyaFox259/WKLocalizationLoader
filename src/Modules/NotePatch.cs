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

        [JsonIgnore]
        public static Dictionary<string, string> TrimmedNoteTexts;
        [JsonIgnore]
        public static NotePatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            TrimmedNoteTexts = NoteTexts
                .ToDictionary(t => t.Key.TrimStart(), t => t.Value);
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
    }
}

