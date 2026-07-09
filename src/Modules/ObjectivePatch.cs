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
        public static Dictionary<string, string> ObjectiveViewerTitles;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveTitles;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveProgressHeaders;
        [JsonProperty]
        public static Dictionary<string, string> ObjectiveSuccessHeaders;

        [JsonIgnore]
        public static TemplateTranslations ObjectiveViewerTitleTemplates;
        [JsonIgnore]
        public static TemplateTranslations ObjectiveTitleTemplates;
        [JsonIgnore]
        public static TemplateTranslations ObjectiveDescriptionTemplates;
        [JsonIgnore]
        public static ObjectivePatchSettings ModuleSettings;

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (!IsEnabled) return;
            ObjectiveViewerTitleTemplates =
                new TemplateTranslations(ObjectiveViewerTitles);
            ObjectiveTitleTemplates =
                new TemplateTranslations(ObjectiveTitles);
            ObjectiveDescriptionTemplates =
                new TemplateTranslations(ObjectiveDescriptions);
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
                ObjectiveViewerTitleTemplates,
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
            s = GetTemplateTranslation(ObjectiveViewerTitleTemplates, s);
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
            title = GetTemplateTranslation(ObjectiveTitleTemplates, title);
            desc = GetTemplateTranslation(ObjectiveDescriptionTemplates, desc);
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
                    ObjectiveTitleTemplates,
                    objectiveCounter.objectiveTitle
                );
                objectiveCounter.objectiveDesc = GetTemplateTranslation(
                    ObjectiveDescriptionTemplates,
                    objectiveCounter.objectiveDesc
                );
                objectiveCounter.progressHeaderDesc = GetTextTranslation(
                    ObjectiveProgressHeaders,
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

