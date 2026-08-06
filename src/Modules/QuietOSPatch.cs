using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine;

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
        public static Dictionary<string, string> UnlockerAccessTitles;
        [JsonProperty]
        public static string FileCounterTemplate;
        [JsonProperty]
        public static string PageCounterTemplate;
        [JsonProperty]
        public static string DiskCardTemplate;
        [JsonProperty]
        public static string NoSaveText;
        [JsonProperty]
        public static string SaveTextTemplate;
        [JsonProperty]
        public static string SaveTemplate;
        [JsonProperty]
        public static string UnlockerCostTemplate;
        [JsonProperty]
        public static string SolarKnightScoreTemplate;
        [JsonProperty]
        public static string SolarKnightLivesTemplate;
        [JsonProperty]
        public static string SolarKnightTimeTemplate;

        [JsonIgnore]
        public static TemplateTranslations MessageTemplates;
        [JsonIgnore]
        public static QuietOSPatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            MessageTemplates = new TemplateTranslations(MessageTexts);
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
            var textTranslations = info.type switch
            {
                OS_Filesystem.FileInfo.fileType.folder => FolderNames,
                _ => FileNames
            };
            info.name = GetTextTranslation(textTranslations, info.name);
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
            if (
                !IsEnabled
                || __instance.infoText is null
                || FileCounterTemplate is null
            )
            {
                return;
            }
            var count = __instance.subFiles.Count;
            __instance.infoText.text = FileCounterTemplate
                .Replace("{count}", count.ToString());
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(OS_Tooltip_Manager),
            nameof(OS_Tooltip_Manager.ShowTip)
        )]
        public static void Prefix_TooltipManager_ShowTip(ref string tip)
        {
            if (!IsEnabled) return;
            tip = GetTextTranslation(HoverTexts, tip);
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

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_DocumentReader),
            nameof(App_DocumentReader.UpdateButtons)
        )]
        public static void Postfix_DocumentReader_UpdateButtons(
            App_DocumentReader __instance
        )
        {
            if (!IsEnabled || PageCounterTemplate is null) return;
            var current = __instance.curPage + 1;
            var total = __instance.pages.Count;
            if (total > 1)
            {
                __instance.pageCounter.text = PageCounterTemplate
                    .Replace("{current}", current.ToString())
                    .Replace("{total}", total.ToString());
            }
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
            if (CL_SaveManager.GetNumberOfDiskLives() == 0)
            {
                __instance.floppyText.text = NoSaveText
                    ?? "SAVES | <mspace=5>NO BACKUP DATA FOUND";
                return;
            }
            var saveText = __instance.floppyText.text;
            if (!saveText.StartsWith("SAVES | <mspace=5>")) return;
            var saveTextTemplate = SaveTextTemplate
                ?? "SAVES | <mspace=5>{saves} ";
            var saveTemplate = SaveTemplate ?? "{stationID}:{saveCount}";
            var saves = saveText.TrimEnd().Substring(18).Split(' ');
            var translatedSaves = new List<string>();
            for (int saveIndex = 0; saveIndex < saves.Length; saveIndex++)
            {
                var save = saves[saveIndex];
                var saveInfo = save.Split(new char[] { ':' }, 2);
                if (saveInfo.Length != 2)
                {
                    translatedSaves.Add(save);
                    continue;
                }
                var stationID = saveInfo[0];
                stationID = GetTextTranslation(StationIDs, stationID);
                var saveCount = saveInfo[1];
                var translatedSave = saveTemplate
                    .Replace("{stationID}", stationID)
                    .Replace("{saveCount}", saveCount);
                translatedSaves.Add(translatedSave);
            }
            var translatedSavesText = string.Join(" ", translatedSaves);
            __instance.floppyText.text = saveTextTemplate
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
            if (!IsEnabled) return;
            diskName = GetTextTranslation(FolderNames, diskName);
            var cardTemplate = DiskCardTemplate
                ?? "{diskName}\nCapacity: {capacity}x";
            __instance.text.text = cardTemplate
                .Replace("{diskName}", diskName)
                .Replace("{capacity}", capacity.ToString());
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
            if (
                !IsEnabled
                || !__instance.showAuthorizationMenu
                || UnlockerCostTemplate is null
            )
            {
                return;
            }
            __instance.authorizationTitleObject.text = GetTextTranslation(
                UnlockerAccessTitles,
                __instance.authorizationTitleObject.text
            );
            var cost = __instance.authorizationCost;
            if (cost <= 0) return;
            var costTemplate = CL_GameManager.GetRoaches() < cost
                ? "<color=red>" + UnlockerCostTemplate + "</color>"
                : UnlockerCostTemplate;
            __instance.authorizationCostTextObject.text = costTemplate
                .Replace("{cost}", cost.ToString());
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
            TranslateSolarKnightScoreText(__instance);
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
            TranslateSolarKnightLivesText(__instance);
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
            __instance.StartCoroutine(TranslateLivesText());
            IEnumerator TranslateLivesText()
            {
                yield return new WaitForSeconds(1.001f);
                TranslateSolarKnightLivesText(__instance);
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
            TranslateSolarKnightScoreText(__instance);
            TranslateSolarKnightLivesText(__instance);
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

        public static void TranslateSolarKnightLivesText(
            App_SolarKnight solarKnight
        )
        {
            var livesText = solarKnight.livesText.text;
            if (
                !livesText.StartsWith("Lives: ")
                || SolarKnightLivesTemplate is null
            )
            {
                return;
            }
            var lives = livesText.Substring(6);
            solarKnight.livesText.text = SolarKnightLivesTemplate
                .Replace("{lives}", lives);
        }

        public static void TranslateSolarKnightScoreText(
            App_SolarKnight solarKnight
        )
        {
            var scoreText = solarKnight.scoreText.text;
            if (
                !scoreText.StartsWith("Score: ")
                || SolarKnightScoreTemplate is null
            )
            {
                return;
            }
            var score = scoreText.Substring(7);
            solarKnight.scoreText.text = SolarKnightScoreTemplate
                .Replace("{score}", score);
        }
    }
}

