using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WKLocalizationLoader
{
    public class TemplateTranslations
    {
        private Dictionary<string, string> _textTranslations;
        private Dictionary<Regex, string> _templateMappings =
            new Dictionary<Regex, string>();
        private string _templateGroupPattern = @"\{(\d+)\}";
        private Regex _templateGroupRegex;
        private string _escapedTemplateGroupPattern = @"\\\{\d+\}";
        private Regex _escapedTemplateGroupRegex;

        public TemplateTranslations(
            Dictionary<string, string> textTranslations
        )
        {
            _textTranslations = textTranslations;
            _templateGroupRegex = new Regex(
                _templateGroupPattern,
                RegexOptions.Compiled
            );
            _escapedTemplateGroupRegex = new Regex(
                _escapedTemplateGroupPattern,
                RegexOptions.Compiled
            );
            RegisterTemplateMappings(_textTranslations);
        }

        public void RegisterTemplateMappings(
            Dictionary<string, string> textTranslations
        )
        {
            foreach (var textTranslation in textTranslations)
            {
                var originalText = textTranslation.Key;
                var groupMatch = _templateGroupRegex.Match(originalText);
                if (groupMatch.Success)
                {
                    var translatedText = textTranslation.Value;
                    RegisterTemplateMapping(originalText, translatedText);
                }
            }
        }

        public string GetTemplateTranslation(string originalText)
        {
            if (string.IsNullOrWhiteSpace(originalText))
            {
                return originalText;
            }
            if (
                _textTranslations != null
                && _textTranslations.TryGetValue(
                    originalText,
                    out string translatedText
                )
                && translatedText != null
            )
            {
                return translatedText;
            }
            foreach (var templateMapping in _templateMappings)
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

        public string BuildStringFromTemplate(
            string templateString,
            Match templateMatch
        )
        {
            var resultString = templateString;
            var groupMatch = _templateGroupRegex.Match(resultString);
            while (groupMatch.Success)
            {
                var groupIndex =
                    Convert.ToInt32(groupMatch.Groups[1].Value) + 1;
                var insertString =
                    groupIndex > templateMatch.Groups.Count
                    ? ""
                    : templateMatch.Groups[groupIndex].Value;
                resultString = resultString
                    .Remove(groupMatch.Index, groupMatch.Length)
                    .Insert(groupMatch.Index, insertString);
                groupMatch = _templateGroupRegex.Match(resultString);
            }
            return resultString;
        }

        public void RegisterTemplateMapping(
            string originalTemplateString,
            string translatedTemplateString
        )
        {
            _templateMappings ??= new Dictionary<Regex, string>();
            var originalTemplateRegex =
                CreateTemplateRegex(originalTemplateString);
            _templateMappings[originalTemplateRegex] =
                translatedTemplateString;
        }

        public Regex CreateTemplateRegex(string templateString)
        {
            var escapedTemplateString = Regex.Escape(templateString);
            var templatePattern = _escapedTemplateGroupRegex.Replace(
                escapedTemplateString,
                @"(.*)"
            );
            return new Regex("^" + templatePattern + "$");
        }
    }
}

