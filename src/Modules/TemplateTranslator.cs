using System;

namespace WKLocalizationLoader.Modules
{
    public class TemplateTranslator<TModule> : TextTranslator<TModule>
    {
        public static string GetTemplateTranslation(
            TemplateTranslations templateTranslations,
            string originalText
        )
        {
            if (
                string.IsNullOrWhiteSpace(originalText)
                || templateTranslations is null
            )
            {
                return originalText;
            }
            return templateTranslations.GetTemplateTranslation(originalText)
                ?? originalText;
        }
    }
}

