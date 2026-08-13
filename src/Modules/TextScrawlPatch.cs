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
        public static Dictionary<string, string> ScrawlTextTemplates;

        [JsonIgnore]
        public static TemplateTranslations ScrawlTemplates;
        [JsonIgnore]
        public static TextScrawlPatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            if (!IsEnabled) return;
            ScrawlTemplates = new TemplateTranslations(ScrawlTextTemplates);
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

