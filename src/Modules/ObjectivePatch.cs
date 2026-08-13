using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class ObjectivePatch : TemplateTranslator<ObjectivePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveViewerTitleTemplates;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveTitleTemplates;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveDescriptionTemplates;
        [JsonProperty]
        public static Dictionary<string, string>
            ObjectiveProgressHeaderTemplates;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveSuccessHeaders;

        [JsonIgnore]
        public static TemplateTranslations ViewerTitleTemplates;
        [JsonIgnore]
        public static TemplateTranslations TitleTemplates;
        [JsonIgnore]
        public static TemplateTranslations DescriptionTemplates;
        [JsonIgnore]
        public static ObjectivePatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            if (!IsEnabled) return;
            ViewerTitleTemplates =
                new TemplateTranslations(ObjectiveViewerTitleTemplates);
            TitleTemplates = new TemplateTranslations(ObjectiveTitleTemplates);
            DescriptionTemplates =
                new TemplateTranslations(ObjectiveDescriptionTemplates);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_ObjectiveViewer),
            nameof(UI_ObjectiveViewer.Awake)
        )]
        public static void Postfix_ObjectiveViewer_Awake(
            UI_ObjectiveViewer __instance
        )
        {
            if (!IsEnabled) return;
            __instance.objectiveViewerTitle.text = GetTemplateTranslation(
                ViewerTitleTemplates,
                __instance.objectiveViewerTitle.text
            );
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UI_ObjectiveViewer),
            nameof(UI_ObjectiveViewer.SetTitle)
        )]
        public static void Prefix_ObjectiveViewer_SetTitle(ref string s)
        {
            if (!IsEnabled) return;
            s = GetTemplateTranslation(ViewerTitleTemplates, s);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UI_ObjectiveViewer),
            nameof(UI_ObjectiveViewer.CreateOrUpdateObjective)
        )]
        public static void Prefix_ObjectiveViewer_CreateOrUpdateObjective(
            string id,
            ref string title,
            ref string desc
        )
        {
            if (!IsEnabled) return;
            title = GetTemplateTranslation(TitleTemplates, title);
            desc = GetTemplateTranslation(DescriptionTemplates, desc);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(CH_ChallengeCounter),
            nameof(CH_ChallengeCounter.Start)
        )]
        public static void Prefix_ChallengeCounter_Start(
            CH_ChallengeCounter __instance
        )
        {
            if (!IsEnabled) return;
            foreach (var objectiveCounter in __instance.objectives)
            {
                objectiveCounter.objectiveTitle = GetTemplateTranslation(
                    TitleTemplates,
                    objectiveCounter.objectiveTitle
                );
                objectiveCounter.objectiveDesc = GetTemplateTranslation(
                    DescriptionTemplates,
                    objectiveCounter.objectiveDesc
                );
                objectiveCounter.progressHeaderDesc = GetTextTranslation(
                    ObjectiveProgressHeaderTemplates,
                    objectiveCounter.progressHeaderDesc
                );
                objectiveCounter.finishedHeaderDesc = GetTextTranslation(
                    ObjectiveSuccessHeaders,
                    objectiveCounter.finishedHeaderDesc
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CH_RoachCollector),
            nameof(CH_RoachCollector.Start)
        )]
        public static void Postfix_RoachCollector_Start(
            CH_RoachCollector __instance
        )
        {
            if (!IsEnabled) return;
            __instance.successText = GetTextTranslation(
                ObjectiveSuccessHeaders,
                __instance.successText
            );
        }
    }
}

