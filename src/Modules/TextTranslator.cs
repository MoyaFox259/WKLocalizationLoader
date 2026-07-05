using System;
using System.Collections.Generic;

namespace WKLocalizationLoader.Modules
{
    public class TextTranslator<TModule> : ModuleBase<TModule>
    {
        public static string GetTextTranslation(
            Dictionary<string, string> textTranslations,
            string originalText
        )
        {
            if (
                string.IsNullOrWhiteSpace(originalText)
                || textTranslations is null
                || !textTranslations.ContainsKey(originalText)
            )
            {
                return originalText;
            }
            return textTranslations[originalText] ?? originalText;
        }
    }
}

