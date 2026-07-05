using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class QuietOSPatch : TemplateTranslator<QuietOSPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> FolderNames;
        [JsonProperty]
        public static Dictionary<string, string> FileNames;
        [JsonProperty]
        public static Dictionary<string, string> UITexts;
        [JsonProperty]
        public static Dictionary<string, string> MessageTexts;
        [JsonProperty]
        public static Dictionary<string, string> MessageOptions;
        [JsonProperty]
        public static Dictionary<string, string> HoverTexts;
        [JsonProperty]
        public static Dictionary<string, string> ContextMenuOptions;
        [JsonProperty]
        public static Dictionary<string, string> StationIDs;
        [JsonProperty]
        public static string DiskCardTemplate;
        [JsonProperty]
        public static string SaveTextTemplate;
        [JsonProperty]
        public static string SaveTemplate;
        [JsonProperty]
        public static string SolarKnightTimeTemplate;

        [JsonIgnore]
        public static TemplateTranslations UITextTemplates;
        [JsonIgnore]
        public static TemplateTranslations MessageTemplates;
        [JsonIgnore]
        public static TemplateTranslations HoverTextTemplates;
        [JsonIgnore]
        public static QuietOSPatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            UITextTemplates = new TemplateTranslations(UITexts);
            MessageTemplates = new TemplateTranslations(MessageTexts);
            HoverTextTemplates = new TemplateTranslations(HoverTexts);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(OS_Manager),
            nameof(OS_Manager.Awake)
        )]
        public static void Postfix_OS_Awake(OS_Manager __instance)
        {
            if (!IsEnabled) return;
            TranslateTextComponents(UITextTemplates, __instance);
            TranslateTMPTextComponents(UITextTemplates, __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(OS_Window),
            nameof(OS_Window.Start)
        )]
        public static void Postfix_Window_Start(OS_Window __instance)
        {
            if (!IsEnabled) return;
            TranslateTextComponents(UITextTemplates, __instance);
            TranslateTMPTextComponents(UITextTemplates, __instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(OS_File),
            nameof(OS_File.Initialize)
        )]
        public static void Prefix_File_Initialize(
            OS_File __instance,
            ref OS_Filesystem.FileInfo info
        )
        {
            if (!IsEnabled) return;
            if (info.type == OS_Filesystem.FileInfo.fileType.folder)
            {
                info.name = GetTextTranslation(FolderNames, info.name);
            }
            else
            {
                info.name = GetTextTranslation(FileNames, info.name);
            }
            var fileNameText = __instance.nameText;
            fileNameText.characterLimit = Math.Max(
                info.name.Length,
                fileNameText.characterLimit
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(OS_File),
            nameof(OS_File.Initialize)
        )]
        public static void Postfix_File_Initialize(OS_File __instance)
        {
            if (!IsEnabled) return;
            var textComponent = __instance.nameText.textComponent;
            var textRect = textComponent.GetComponent<RectTransform>();
            var textArea = textComponent.transform.parent;
            var textAreaRect = textArea.GetComponent<RectTransform>();
            if (
                textRect != null
                && textAreaRect != null
                && textRect.rect.width < textAreaRect.rect.width
            )
            {
                textRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    textAreaRect.rect.width
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(OS_Folder),
            nameof(OS_Folder.UpdateInfoText)
        )]
        public static void Postfix_Folder_UpdateInfoText(OS_Folder __instance)
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, __instance.infoText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_DocumentReader),
            nameof(App_DocumentReader.UpdateButtons)
        )]
        public static void Postfix_DocumentReader_UpdateButtons(
            App_DocumentReader __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, __instance.pageCounter);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_PerkPage),
            nameof(App_PerkPage.PurchaseRefresh)
        )]
        public static void Postfix_PerkPage_PurchaseRefresh(
            App_PerkPage __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPTextComponents(UITextTemplates, __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SavePage),
            nameof(App_SavePage.UpdateSaveText)
        )]
        public static void Postfix_SavePage_UpdateSaveText(
            App_SavePage __instance
        )
        {
            if (!IsEnabled) return;
            var saveText = __instance.floppyText.text;
            if (
                CL_SaveManager.GetNumberOfDiskLives() == 0
                || !saveText.StartsWith("SAVES | <mspace=5>")
                || SaveTextTemplate is null
                || SaveTemplate is null
            )
            {
                __instance.floppyText.text = GetTemplateTranslation(
                    UITextTemplates,
                    saveText
                );
                return;
            }
            var saves = saveText.Trim().Substring(18).Split(' ');
            var translatedSaves = new List<string>();
            for (int saveIndex = 0; saveIndex < saves.Length; saveIndex++)
            {
                var saveInfo = saves[saveIndex].Split(':');
                var stationID = saveInfo[0];
                stationID = GetTextTranslation(StationIDs, stationID);
                var saveCount = saveInfo[1];
                var translatedSave = SaveTemplate
                    .Replace("{stationID}", stationID)
                    .Replace("{saveCount}", saveCount);
                translatedSaves.Add(translatedSave);
            }
            var translatedSavesText = string.Join(" ", translatedSaves);
            __instance.floppyText.text = SaveTextTemplate
                .Replace("{saves}", translatedSavesText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SavePage_DiskCard),
            nameof(App_SavePage_DiskCard.Initialize)
        )]
        public static void Postfix_DiskCard_Initialize(
            App_SavePage_DiskCard __instance,
            ref string diskName,
            ref int capacity
        )
        {
            if (!IsEnabled || DiskCardTemplate is null) return;
            diskName = GetTextTranslation(FolderNames, diskName);
            __instance.text.text = DiskCardTemplate
                .Replace("{diskName}", diskName)
                .Replace("{capacity}", capacity.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_FacilitySlotHolder),
            nameof(App_FacilitySlotHolder.Awake)
        )]
        public static void Postfix_FacilitySlotHolder_Awake(
            App_FacilitySlotHolder __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPTextComponents(
                UITextTemplates,
                __instance.facilityCardAsset
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_FacilitySlotHolder),
            nameof(App_FacilitySlotHolder.SetPage)
        )]
        public static void Postfix_FacilitySlotHolder_SetPage(
            List<App_FacilitySlotHolder.UpgradePage> pageList,
            ref int pageNumber,
            TMP_Text titleObject
        )
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, titleObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_Unlocker),
            nameof(App_Unlocker.CheckAuthorize)
        )]
        public static void Postfix_Unlocker_CheckAuthorize(
            App_Unlocker __instance
        )
        {
            if (!IsEnabled) return;
            __instance.authorizationTitle = GetTemplateTranslation(
                UITextTemplates,
                __instance.authorizationTitle
            );
            TranslateTMPText(
                UITextTemplates,
                __instance.authorizationCostTextObject
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SolarKnight),
            nameof(App_SolarKnight.AddScore)
        )]
        public static void Postfix_SolarKnight_AddScore(
            App_SolarKnight __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, __instance.scoreText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SolarKnight),
            nameof(App_SolarKnight.AddLife)
        )]
        public static void Postfix_SolarKnight_AddLife(
            App_SolarKnight __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, __instance.livesText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SolarKnight),
            nameof(App_SolarKnight.LoseLife)
        )]
        public static void Postfix_SolarKnight_LoseLife(
            App_SolarKnight __instance
        )
        {
            if (!IsEnabled) return;
            __instance.StartCoroutine(TranslateLiveText());
            IEnumerator TranslateLiveText()
            {
                yield return new WaitForSeconds(1.001f);
                TranslateTMPText(UITextTemplates, __instance.livesText);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SolarKnight),
            nameof(App_SolarKnight.Reset)
        )]
        public static void Postfix_SolarKnight_Reset(
            App_SolarKnight __instance
        )
        {
            if (!IsEnabled) return;
            TranslateTMPText(UITextTemplates, __instance.scoreText);
            TranslateTMPText(UITextTemplates, __instance.livesText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_SolarKnight),
            nameof(App_SolarKnight.Update)
        )]
        public static void Postfix_SolarKnight_Update(
            App_SolarKnight __instance
        )
        {
            if (!IsEnabled) return;
            var timeText = __instance.timeText.text;
            if (
                !timeText.StartsWith("Time: ")
                || SolarKnightTimeTemplate is null
            )
            {
                return;
            }
            var time = timeText.Substring(6);
            __instance.timeText.text = SolarKnightTimeTemplate
                .Replace("{time}", time);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Message_Manager),
            nameof(Message_Manager.CreateMessage)
        )]
        public static void Prefix_MessageManager_CreateMessage(
            ref Message_Manager.Message_Packet packet
        )
        {
            if (!IsEnabled) return;
            packet.message = GetTemplateTranslation(
                MessageTemplates,
                packet.message
            );
            packet.closeText = GetTextTranslation(
                MessageOptions,
                packet.closeText
            );
            packet.aText = GetTextTranslation(MessageOptions, packet.aText);
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(OS_Tooltip),
            nameof(OS_Tooltip.ShowTooltip)
        )]
        public static void Prefix_Tooltip_ShowTooltip(OS_Tooltip __instance)
        {
            if (!IsEnabled) return;
            __instance.tip = GetTemplateTranslation(
                HoverTextTemplates,
                __instance.tip
            );
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(ContextMenu),
            nameof(ContextMenu.ShowMessage)
        )]
        public static void Prefix_ContextMenu_ShowMessage(
            ContextMenu __instance
        )
        {
            if (!IsEnabled) return;
            foreach (var option in __instance.options)
            {
                option.text = GetTextTranslation(
                    ContextMenuOptions,
                    option.text
                );
            }
        }

        public static void TranslateTextComponents(
            TemplateTranslations templateTranslations,
            Component component,
            bool includeDisabledComponents = true
        )
        {
            if (component is null) return;
            var texts = component.transform
                .GetComponentsInChildren<Text>(includeDisabledComponents);
            for (int textIndex = 0; textIndex < texts.Length; textIndex++)
            {
                TranslateText(UITextTemplates, texts[textIndex]);
            }
        }

        public static void TranslateTMPTextComponents(
            TemplateTranslations templateTranslations,
            Component component,
            bool includeDisabledComponents = true
        )
        {
            if (component is null) return;
            var tmpTexts = component.transform
                .GetComponentsInChildren<TMP_Text>(includeDisabledComponents);
            for (int tmpIndex = 0; tmpIndex < tmpTexts.Length; tmpIndex++)
            {
                TranslateTMPText(UITextTemplates, tmpTexts[tmpIndex]);
            }
        }

        public static void TranslateText(
            TemplateTranslations templateTranslations,
            Text text
        )
        {
            if (text is null) return;
            text.text = GetTemplateTranslation(
                templateTranslations,
                text.text
            );
        }

        public static void TranslateTMPText(
            TemplateTranslations templateTranslations,
            TMP_Text textComponent
        )
        {
            if (textComponent is null) return;
            textComponent.text = GetTemplateTranslation(
                templateTranslations,
                textComponent.text
            );
        }
    }
}

