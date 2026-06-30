using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class TextScrawlPatch : ModuleBase<TextScrawlPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static Dictionary<Regex, string> TemplateMappings =
            new Dictionary<Regex, string>();
        [JsonIgnore]
        public static string EscapedTemplateGroupPattern =
            @"(?:\\)?\{\d+(?:\\)?\}";
        [JsonIgnore]
        public static Regex EscapedTemplateGroupRegex =
            new Regex(EscapedTemplateGroupPattern);
        [JsonIgnore]
        public static string TemplateGroupPattern = @"\{(\d+)\}";
        [JsonIgnore]
        public static Regex TemplateGroupRegex =
            new Regex(TemplateGroupPattern);
        [JsonIgnore]
        public static TextScrawlPatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            foreach (var textTranslation in TextTranslations)
            {
                var originalText = textTranslation.Key;
                var groupMatch = TemplateGroupRegex.Match(originalText);
                if (groupMatch.Success)
                {
                    var translatedText = textTranslation.Value;
                    RegisterTemplateMapping(originalText, translatedText);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UT_TextScrawl),
            nameof(UT_TextScrawl.ShowText)
        )]
        public static void Prefix_TextScrawl_ShowText(ref string s)
        {
            if (!IsEnabled) return;
            s = GetTextTranslation(s);
        }

        public static string GetTextTranslation(string originalText)
        {
            if (string.IsNullOrWhiteSpace(originalText))
            {
                return originalText;
            }
            if (
                TextTranslations != null
                && TextTranslations.TryGetValue(
                    originalText,
                    out string translatedText
                )
                && translatedText != null
            )
            {
                return translatedText;
            }
            foreach (var templateMapping in TemplateMappings)
            {
                var originalTemplateRegex = templateMapping.Key;
                var originalTemplateMatch =
                    originalTemplateRegex.Match(originalText);
                if (originalTemplateMatch.Success)
                {
                    var translatedTemplateString = templateMapping.Value;
                    return BuildStringFromTemplate(
                        translatedTemplateString,
                        originalTemplateMatch
                    );
                }
            }
            return originalText;
        }

        public static string BuildStringFromTemplate(
            string templateString,
            Match templateMatch
        )
        {
            var resultString = templateString;
            var groupMatch = TemplateGroupRegex.Match(resultString);
            while (groupMatch.Success)
            {
                var groupIndex =
                    Convert.ToInt32(groupMatch.Groups[1].Value) + 1;
                groupIndex = Math.Min(
                    groupIndex,
                    templateMatch.Groups.Count
                );
                resultString = resultString
                    .Remove(
                        groupMatch.Index,
                        groupMatch.Length
                    )
                    .Insert(
                        groupMatch.Index,
                        templateMatch.Groups[groupIndex].Value
                    );
                groupMatch = groupMatch.NextMatch();
            }
            return resultString;
        }

        public static Regex CreateTemplateRegex(string templateString)
        {
            var escapedTemplateString = Regex.Escape(templateString);
            var templatePattern = EscapedTemplateGroupRegex.Replace(
                escapedTemplateString,
                @"(.*?)"
            );
            return new Regex("^" + templatePattern + "$");
        }

        public static void RegisterTemplateMapping(
            string originalTemplateString,
            string translatedTemplateString
        )
        {
            TemplateMappings ??= new Dictionary<Regex, string>();
            var originalTemplateRegex =
                CreateTemplateRegex(originalTemplateString);
            TemplateMappings[originalTemplateRegex] =
                translatedTemplateString;
        }
    }
}

