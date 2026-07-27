using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WKLocalizationLoader
{
    public class TemplateTranslations
    {
        private Dictionary<string, string> _textTranslations;
        private Dictionary<Regex, string> _templateMappings;
        private readonly Regex _templateGroupRegex;
        private readonly Regex _escapedTemplateGroupRegex;

        public TemplateTranslations(
            Dictionary<string, string> textTranslations
        )
        {
            _templateGroupRegex = CacheManager.GetOrCreateRegex(
                @"\{(\d+)\}",
                RegexOptions.Compiled
            );
            _escapedTemplateGroupRegex = CacheManager.GetOrCreateRegex(
                @"\\\{\d+\}",
                RegexOptions.Compiled
            );
            AddTemplateTranslations(textTranslations);
        }

        public void AddTemplateTranslations(
            Dictionary<string, string> textTranslations
        )
        {
            foreach (var textTranslation in textTranslations)
            {
                AddTemplateTranslation(
                    textTranslation.Key,
                    textTranslation.Value
                );
            }
        }

        public void AddTemplateTranslation(
            string originalTemplateString,
            string translatedTemplateString
        )
        {
            _textTranslations ??= new Dictionary<string, string>();
            _templateMappings ??= new Dictionary<Regex, string>();
            if (
                originalTemplateString is null
                || translatedTemplateString is null
            )
            {
                return;
            }
            _textTranslations[originalTemplateString] =
                translatedTemplateString;
            if (_templateGroupRegex.IsMatch(originalTemplateString))
            {
                var originalTemplateRegex =
                    CreateTemplateRegex(originalTemplateString);
                _templateMappings[originalTemplateRegex] =
                    translatedTemplateString;
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
            if (_templateMappings != null)
            {
                foreach (var templateMapping in _templateMappings)
                {
                    var originalTemplateRegex = templateMapping.Key;
                    if (originalTemplateRegex is null) continue;
                    var originalTemplateMatch =
                        originalTemplateRegex.Match(originalText);
                    if (originalTemplateMatch.Success)
                    {
                        var translatedTemplateString = templateMapping.Value;
                        return translatedTemplateString is null
                            ? originalText
                            : BuildStringFromTemplate(
                                translatedTemplateString,
                                originalTemplateMatch
                            );
                    }
                }
            }
            return originalText;
        }

        public string BuildStringFromTemplate(
            string templateString,
            Match templateMatch
        )
        => _templateGroupRegex.Replace(
            templateString,
            m => {
                var groupIndex = Convert.ToInt32(m.Groups[1].Value) + 1;
                return groupIndex > templateMatch.Groups.Count
                    ? ""
                    : templateMatch.Groups[groupIndex].Value;
            }
        );

        public Regex CreateTemplateRegex(string templateString)
        {
            var escapedTemplateString = Regex.Escape(templateString);
            var templatePattern = _escapedTemplateGroupRegex.Replace(
                escapedTemplateString,
                @"(.*)"
            );
            templatePattern = "^" + templatePattern + "$";
            return CacheManager.GetOrCreateRegex(
                templatePattern,
                RegexOptions.Singleline
            );
        }
    }
}

