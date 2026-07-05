using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class TextScrawlPatch : TemplateTranslator<TextScrawlPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static TemplateTranslations ScrawlTemplates;
        [JsonIgnore]
        public static TextScrawlPatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            ScrawlTemplates = new TemplateTranslations(TextTranslations);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UT_TextScrawl),
            nameof(UT_TextScrawl.ShowText)
        )]
        public static void Prefix_TextScrawl_ShowText(ref string s)
        {
            if (!IsEnabled) return;
            s = GetTemplateTranslation(ScrawlTemplates, s);
        }
    }
}

