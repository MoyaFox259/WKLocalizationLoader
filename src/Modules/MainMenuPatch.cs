using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class MainMenuPatch : TextTranslator<MainMenuPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> PageTitles;
        [JsonProperty]
        public static string LoadingProgressTemplate;
        [JsonProperty]
        public static string PageCounterTemplate;

        [JsonIgnore]
        public static MainMenuPatchSettings ModuleSettings;

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UT_Intro),
            nameof(UT_Intro.EndIntro)
        )]
        public static bool Prefix_Intro_EndIntro(UT_Intro __instance)
        {
            if (!IsEnabled || LoadingProgressTemplate is null) return true;
            __instance.video.Stop();
            __instance.hasSkipped = true;
            var loadPercentageText = __instance.loadPercentageText;
            loadPercentageText.transform.parent.gameObject.SetActive(true);
            loadPercentageText.text = LoadingProgressTemplate
                .Replace("{progress}", "0");
            __instance.StartCoroutine(CustomLoadingIntro(__instance));
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_PageHolder),
            nameof(UI_PageHolder.UpdatePage)
        )]
        public static void Postfix_PageHolder_UpdatePage(
            UI_PageHolder __instance
        )
        {
            if (!IsEnabled) return;
            var pageTitle = __instance.pageTitle;
            if (pageTitle is null) return;
            var pages = __instance.pages;
            var total = pages.Count;
            if (total < 2) return;
            var current = __instance.currentPage + 1;
            var title = GetTextTranslation(PageTitles, pages[current].title);
            var counterTemplate = PageCounterTemplate
                ?? "{title} ({current}/{total})";
            pageTitle.text = counterTemplate
                .Replace("{title}", title)
                .Replace("{current}", current.ToString())
                .Replace("{total}", total.ToString());
        }

        public static IEnumerator CustomLoadingIntro(UT_Intro intro)
        {
            yield return new WaitForSeconds(0.1f);
            intro.onEnd?.Invoke();
            yield return new WaitForSeconds(0.1f);
            while (true)
            {
                UpdateLoadingProgressText();
                yield return null;
                yield return null;
                if (
                    intro.loadMenuOperation is null
                    || (double)intro.loadMenuOperation.progress < 0.89
                )
                {
                    continue;
                }
                intro.loadMenuOperation.allowSceneActivation = true;
                if (intro.loadMenuOperation.isDone) break;
            }
            UpdateLoadingProgressText();
            intro.loadMenuOperation.allowSceneActivation = true;
            void UpdateLoadingProgressText()
            {
                if (intro.loadMenuOperation is null) return;
                var progress = Mathf
                    .RoundToInt(intro.loadMenuOperation.progress * 100f)
                    .ToString();
                intro.loadPercentageText.text = LoadingProgressTemplate
                    .Replace("{progress}", progress);
            }
        }
    }
}

