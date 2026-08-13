using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class DocumentPatch: TextTranslator<DocumentPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> DocumentTexts;

        [JsonIgnore]
        public static DocumentPatchSettings ModuleSettings;

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(App_DocumentReader),
            nameof(App_DocumentReader.Start)
        )]
        public static void Prefix_DocumentReader_Start(
            App_DocumentReader __instance
        )
        {
            if (!IsEnabled) return;
            var translatedText = "";
            var window = __instance.GetComponent<OS_Window>();
            var fileInfo = window.file.fileInfo;
            if (fileInfo.textAssetData is null)
            {
                var data = fileInfo.data;
                var originalText = DarkMachineFunctions.ProcessText(data);
                translatedText = GetTextTranslation(
                    DocumentTexts,
                    originalText
                );
                fileInfo.data = data.Replace(originalText, translatedText);
                return;
            }
            translatedText = GetTextTranslation(
                DocumentTexts,
                fileInfo.textAssetData.text
            );
            fileInfo.textAssetData = CacheManager
                .GetOrCreateTextAsset(translatedText);
        }
    }
}

