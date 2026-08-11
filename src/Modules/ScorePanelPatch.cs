using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;
using Febucci.UI.Core;
using DG.Tweening;
using Steamworks.Data;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class ScorePanelPatch : TextTranslator<ScorePanelPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> StatDefaultTexts;
        [JsonProperty]
        public static Dictionary<string, string> StatTextTemplates;
        [JsonProperty]
        public static Dictionary<string, string> ScoreItemTitles;
        [JsonProperty]
        public static Dictionary<string, string> MedalTitles;
        [JsonProperty]
        public static Dictionary<string, string> LeaderboardRangeTypes;
        [JsonProperty]
        public static Dictionary<string, string> LeaderboardScoreTypes;
        [JsonProperty]
        public static string StatMetersTemplate;
        [JsonProperty]
        public static string StatMeterPerSecondTemplate;
        [JsonProperty]
        public static string StatTimeTemplate;
        [JsonProperty]
        public static string StatTimeDayTemplate;
        [JsonProperty]
        public static string StatTimeHourTemplate;
        [JsonProperty]
        public static string StatTimeMinuteTemplate;
        [JsonProperty]
        public static string StatTimeSecondTemplate;
        [JsonProperty]
        public static string EndScreenDistanceDefaultText;
        [JsonProperty]
        public static string EndScreenDistanceTemplate;
        [JsonProperty]
        public static string EndScreenSpeedDefaultText;
        [JsonProperty]
        public static string EndScreenSpeedTemplate;
        [JsonProperty]
        public static string EndScreenFillerText;
        [JsonProperty]
        public static string EndScreenBaseScoreTemplate;
        [JsonProperty]
        public static string EndScreenFinalScoreTemplate;
        [JsonProperty]
        public static string EndScreenScoreItemTitleTemplate;
        [JsonProperty]
        public static string LeaderboardDetailNameTemplate;
        [JsonProperty]
        public static string LeaderboardDetailNoSessionDataText;
        [JsonProperty]
        public static string LeaderboardDetailNoScoreDataText;
        [JsonProperty]
        public static string LeaderboardSessionScoreTemplate;
        [JsonProperty]
        public static string LeaderboardSessionDistanceTemplate;
        [JsonProperty]
        public static string LeaderboardSessionTimeTemplate;
        [JsonProperty]
        public static string LeaderboardSessionSpeedTemplate;
        [JsonProperty]
        public static string LeaderboardSessionDateTemplate;
        [JsonProperty]
        public static string LeaderboardScoreBonusTemplate;
        [JsonProperty]
        public static string LeaderboardScoreMultiplierTemplate;
        [JsonProperty]
        public static string PopupRoachBankedTitle;
        [JsonProperty]
        public static string PopupRoachBankedDescriptionTemplate;
        [JsonProperty]
        public static string PopupCreditWonTitle;
        [JsonProperty]
        public static string PopupCreditWonDescriptionTemplate;

        [JsonIgnore]
        public static ScorePanelPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UT_StatText),
            nameof(UT_StatText.RefreshText)
        )]
        public static void Postfix_StatText_RefreshText(
            UT_StatText __instance
        )
        {
            if (!IsEnabled) return;
            var statText = __instance.text.text;
            if (statText == __instance.defaultText)
            {
                __instance.text.text = GetTextTranslation(
                    StatDefaultTexts,
                    statText
                );
                return;
            }
            var textPrefix = __instance.textPrefix;
            if (
                StatTextTemplates != null
                && StatTextTemplates.TryGetValue(
                    textPrefix + "{value}",
                    out string statTextTemplate
                )
                && statTextTemplate != null
            )
            {
                var statValue = statText.Substring(textPrefix.Length);
                __instance.text.text = statTextTemplate
                    .Replace("{value}", statValue);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatManager.Statistic),
            nameof(StatManager.Statistic.GetString)
        )]
        public static string Postfix_Statistic_GetString(
            string __result,
            StatManager.Statistic __instance
        )
        {
            if (!IsEnabled) return __result;
            var displayType = __instance.displayType;
            if (displayType == StatManager.Statistic.DisplayType.Default)
            {
                return __result;
            }
            var statValue = Math.Round((float)__instance.GetValue(), 2);
            if (displayType == StatManager.Statistic.DisplayType.Meters)
            {
                var meters = Math
                    .Round(statValue, 2)
                    .ToString(CultureInfo.InvariantCulture);
                var metersTemplate = StatMetersTemplate
                    ?? "{meter} Meters";
                return metersTemplate.Replace("{meter}", meters);
            }
            if (displayType == StatManager.Statistic.DisplayType.Speed)
            {
                var mps = Math
                    .Round(statValue, 2)
                    .ToString(CultureInfo.InvariantCulture);
                var mpsTemplate = StatMeterPerSecondTemplate
                    ?? "{mps} m/s";
                return mpsTemplate.Replace("{mps}", mps);
            }
            if (displayType == StatManager.Statistic.DisplayType.Time)
            {
                var timeSpan = TimeSpan.FromSeconds(statValue);
                return timeSpan.TotalHours < 1.0
                    ? timeSpan.ToString(@"mm\:ss\:ff")
                    : DarkMachineFunctions.SecondsToTimeLeaderboardString(
                        (float)__instance.GetValue()
                    );
            }
            return __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(DarkMachineFunctions),
            nameof(DarkMachineFunctions.SecondsToTimeLeaderboardString)
        )]
        public static string Postfix_SecondsToString(
            string __result,
            float seconds
        )
        {
            if (!IsEnabled) return __result;
            var timeSpan = TimeSpan.FromSeconds(seconds);
            var day = "";
            if (timeSpan.TotalDays >= 1.0)
            {
                var dayTemplate = StatTimeDayTemplate ?? "D:{day} ";
                day = dayTemplate.Replace("{day}", timeSpan.Days.ToString());
            }
            var hour = "";
            if (timeSpan.TotalHours >= 1.0)
            {
                var hourTemplate = StatTimeHourTemplate ?? "H:{hour} ";
                hour = hourTemplate
                    .Replace("{hour}", timeSpan.Hours.ToString());
            }
            var minuteTemplate = StatTimeMinuteTemplate ?? "M:{minute} ";
            var minute = minuteTemplate
                .Replace("{minute}", timeSpan.Minutes.ToString());
            var secondTemplate = StatTimeSecondTemplate
                ?? "S:{second}.{millisecond}";
            var second = secondTemplate
                .Replace("{second}", timeSpan.Seconds.ToString())
                .Replace("{millisecond}", timeSpan.Milliseconds.ToString());
            var timeTemplate = StatTimeTemplate
                ?? "{day}{hour}{minute}{second}";
            return timeTemplate
                .Replace("{day}", day)
                .Replace("{hour}", hour)
                .Replace("{minute}", minute)
                .Replace("{second}", second);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UI_EndScreenScoreWindow),
            nameof(UI_EndScreenScoreWindow.StartAnimation)
        )]
        public static bool Prefix_EndScreenScoreWindow_StartAnimation(
            UI_EndScreenScoreWindow __instance
        )
        {
            if (!IsEnabled) return true;
            foreach (Transform item in __instance.scoreItemRoot)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
            __instance.StartCoroutine(CustomAnimateEndScreen(__instance));
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_EndScreenScoreWindow_ScoreItem),
            nameof(UI_EndScreenScoreWindow_ScoreItem.Initialize)
        )]
        public static void Postfix_EndScreenScoreItem_Initialize(
            UI_EndScreenScoreWindow_ScoreItem __instance,
            ref string title,
            float bonus,
            float multiplier,
            in int count
        )
        {
            if (!IsEnabled) return;
            title = GetTextTranslation(ScoreItemTitles, title);
            if (count == 0)
            {
                __instance.titleText.text = title;
                return;
            }
            var titleTemplate = EndScreenScoreItemTitleTemplate
                ?? "{title} <color=grey>({count})";
            __instance.titleText.text = titleTemplate
                .Replace("{title}", title)
                .Replace("{count}", count.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_ScoreScreen),
            nameof(UI_ScoreScreen.SetMedalInfo)
        )]
        public static void Postfix_ScoreScreen_SetMedalInfo(
            UI_ScoreScreen __instance
        )
        {
            if (!IsEnabled) return;
            __instance.scoreRankText.text = GetTextTranslation(
                MedalTitles,
                __instance.scoreRankText.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Leaderboard_Panel),
            nameof(Leaderboard_Panel.StartUpdateScore)
        )]
        public static void Postfix_LeaderboardPanel_StartUpdateScore(
            Leaderboard_Panel __instance
        )
        {
            if (!IsEnabled) return;
            __instance.title.text = GetTextTranslation(
                LeaderboardRangeTypes,
                __instance.title.text
            );
            __instance.scoreTypeText.text = GetTextTranslation(
                LeaderboardScoreTypes,
                __instance.scoreTypeText.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_LeaderboardEntryDetailWindow),
            nameof(UI_LeaderboardEntryDetailWindow.ShowDetails)
        )]
        public static void Postfix_LeaderboardEntryWindow_ShowDetails(
            UI_LeaderboardEntryDetailWindow __instance,
            in LeaderboardEntry scoreInfo,
            CL_Leaderboard.WK_Leaderboard_UserData data
        )
        {
            if (!IsEnabled) return;
            if (LeaderboardDetailNameTemplate != null)
            {
                var player = scoreInfo.User.Name;
                var rank = scoreInfo.GlobalRank;
                __instance.nameText.text = LeaderboardDetailNameTemplate
                    .Replace("{player}", player)
                    .Replace("{rank}", rank.ToString());
            }
            var statText = __instance.statText.text;
            var statStrings = statText.Split('\n');
            for (
                var statIndex = 0;
                statIndex < statStrings.Length;
                statIndex++
            )
            {
                var statString = statStrings[statIndex];
                if (
                    statString.StartsWith("<color=red><size=24>Score: ")
                    && statString.EndsWith("</size></color>")
                )
                {
                    var score =
                        statString.Substring(27, statString.Length - 42);
                    var scoreTemplate = LeaderboardSessionScoreTemplate
                        ?? "<color=red><size=24>Score: {score}</size></color>";
                    statString = scoreTemplate.Replace("{score}", score);
                }
                else if (
                    statString.StartsWith("<color=yellow>Distance: ")
                    && statString.EndsWith("m</color>")
                )
                {
                    var meter =
                        statString.Substring(24, statString.Length - 33);
                    var distanceTemplate = LeaderboardSessionDistanceTemplate
                        ?? "<color=yellow>Distance: {meter}m</color>";
                    statString = distanceTemplate.Replace("{meter}", meter);
                }
                else if (statString.StartsWith("Run Playtime: "))
                {
                    var time = statString.Substring(14);
                    var timeTemplate = LeaderboardSessionTimeTemplate
                        ?? "Run Playtime: {time}";
                    statString = timeTemplate.Replace("{time}", time);
                }
                else if (
                    statString.StartsWith("Speed: ")
                    && statString.EndsWith("m/s")
                )
                {
                    var mps =
                        statString.Substring(7, statString.Length - 10);
                    var speedTemplate = LeaderboardSessionSpeedTemplate
                        ?? "Speed: {mps}m/s";
                    statString = speedTemplate.Replace("{mps}", mps);
                }
                else if (statString.StartsWith("Date: "))
                {
                    var date = statString.Substring(6);
                    var dateTemplate = LeaderboardSessionDateTemplate
                        ?? "Date: {date}";
                    statString = dateTemplate.Replace("{date}", date);
                }
                statStrings[statIndex] = statString;
            }
            statText = string.Join("\n", statStrings);
            __instance.statText.text = statText;
            var scoreStrings = new List<string>();
            if (data.scoreData is null || data.scoreData.Count == 0) return;
            foreach (var scoreDataText in data.scoreData)
            {
                var scoreDataStrings = scoreDataText.Split(':');
                if (scoreDataStrings.Length < 5) continue;
                var title = GetTextTranslation(
                    ScoreItemTitles,
                    scoreDataStrings[1]
                );
                int.TryParse(scoreDataStrings[2], out int count);
                count = Mathf.Max(count, 1);
                float.TryParse(scoreDataStrings[3], out float bonus);
                bonus = (float)Math.Round(bonus, 2);
                float.TryParse(scoreDataStrings[4], out float multiplier);
                multiplier = (float)Math.Round(multiplier, 2);
                var scoreTemplate = "";
                if (multiplier == 1f)
                {
                    scoreTemplate = LeaderboardScoreBonusTemplate
                        ?? "{title} <color=grey>({count})</color>: {bonus}";
                }
                else
                {
                    scoreTemplate = LeaderboardScoreMultiplierTemplate
                        ?? "{title} <color=grey>({count})</color>: "
                            + "{multiplier}x";
                }
                var scoreString = scoreTemplate
                    .Replace("{title}", title)
                    .Replace("{count}", count.ToString())
                    .Replace("{bonus}", bonus.ToString())
                    .Replace("{multiplier}", multiplier.ToString());
                scoreStrings.Add(scoreString);
            }
            var scoreText = string.Join("\n", scoreStrings);
            __instance.scoreText.text = scoreText;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_LeaderboardEntryDetailWindow),
            nameof(UI_LeaderboardEntryDetailWindow.ShowNoEntryDetails)
        )]
        public static void Postfix_LeaderboardEntryWindow_ShowNoEntryDetails(
            UI_LeaderboardEntryDetailWindow __instance,
            in LeaderboardEntry scoreInfo
        )
        {
            if (!IsEnabled) return;
            if (LeaderboardDetailNameTemplate != null)
            {
                var player = scoreInfo.User.Name;
                var rank = scoreInfo.GlobalRank;
                __instance.nameText.text = LeaderboardDetailNameTemplate
                    .Replace("{player}", player)
                    .Replace("{rank}", rank.ToString());
            }
            __instance.statText.text = LeaderboardDetailNoSessionDataText
                ?? "No Run Data Found.";
            __instance.scoreText.text = LeaderboardDetailNoScoreDataText
                ?? "No Score Data Found.";
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(CL_ProgressionManager),
            nameof(CL_ProgressionManager.ShowUnlockProgress)
        )]
        public static void Prefix_ProgressionManager_ShowUnlockProgress(
            Sprite icon,
            ref string title,
            ref string desc
        )
        {
            if (!IsEnabled || title != "ROACHES BANKED")
            {
                return;
            }
            title = PopupRoachBankedTitle ?? "ROACHES BANKED";
            if (
                desc.StartsWith("Banked ")
                && desc.EndsWith(" from your inventory.")
                && PopupRoachBankedDescriptionTemplate != null
            )
            {
                var count = desc.Substring(7, desc.Length - 28);
                desc = PopupRoachBankedDescriptionTemplate
                    .Replace("{count}", count);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(CL_ProgressionManager),
            nameof(CL_ProgressionManager.ShowUnlockPopup),
            new Type[] {
                typeof(Sprite),
                typeof(string),
                typeof(string),
                typeof(UnityEngine.Color),
                typeof(AudioClip),
                typeof(bool)
            }
        )]
        public static void Prefix_ProgressionManager_ShowUnlockPopup(
            Sprite icon,
            ref string title,
            ref string desc
        )
        {
            if (!IsEnabled || title != "WIN BONUS")
            {
                return;
            }
            title = PopupCreditWonTitle ?? "WIN BONUS";
            if (
                desc.EndsWith(" Facility Credits Added")
                && PopupCreditWonDescriptionTemplate != null
            )
            {
                var count = desc.Substring(0, desc.Length - 23);
                desc = PopupCreditWonDescriptionTemplate
                    .Replace("{count}", count);
            }
        }

        public static IEnumerator CustomAnimateEndScreen(
            UI_EndScreenScoreWindow scoreWindow
        )
        {
            var scoreTitle = scoreWindow.scoreTitle;
            var scoreTitleColor = scoreWindow.titleColor;
            var loadingText = scoreWindow.loadingText;
            var loadingTextAnimator = scoreWindow.loadingTextAnimator;
            var loadingTextTypewriter = loadingTextAnimator
                .GetComponent<TypewriterCore>();
            var distance = CL_GameManager.gMan.GetPlayerBestTravelDistance();
            var distanceText = scoreWindow.distanceScoreText;
            var speed = CL_GameManager.gMan.GetPlayerTravelSpeed();
            var speedText = scoreWindow.speedScoreText;
            var modifierTitle = scoreWindow.modifierTitleText;
            var currentScore = Mathf.Round(distance * speed);
            var currentMultiplier = 1f;
            var baseScoreText = scoreWindow.baseScoreText;
            var finalScore = CL_GameManager
                .GetCurrentGamemode()
                .GetPlayerScore();
            var finalScoreText = scoreWindow.totalScoreText;
            var finalScoreTextAnimator = finalScoreText
                .GetComponent<TextAnimator_TMP>();
            var highScoreText = scoreWindow.highScoreText;
            var scoreItemAsset = scoreWindow.scoreItemAsset;
            var scoreItemRoot = scoreWindow.scoreItemRoot;
            var tickSound = scoreWindow.tickSound;
            UI_EndScreenScoreWindow.FinishSound finishSound;
            var finishSounds = scoreWindow.finishSounds;
            var distanceDefaultText = EndScreenDistanceDefaultText
                ?? "DISTANCE: .............................";
            var distanceTemplate = EndScreenDistanceTemplate
                ?? "DISTANCE: ...................{filler}{meter}M";
            var speedDefaultText = EndScreenSpeedDefaultText
                ?? "SPEED: ..........................";
            var speedTemplate = EndScreenSpeedTemplate
                ?? "SPEED: ................{filler}{mps}m/s";
            var fillerText = EndScreenFillerText ?? "..........";
            var baseScoreTemplate = EndScreenBaseScoreTemplate
                ?? "SCORE: {score}";
            var finalScoreTemplate = EndScreenFinalScoreTemplate
                ?? "FINAL: {score}";
            scoreTitle.color = UnityEngine.Color.clear;
            loadingTextAnimator.TMProComponent.color = UnityEngine.Color.clear;
            distanceText.text = "";
            speedText.text = "";
            modifierTitle.gameObject.SetActive(false);
            baseScoreText.text = "";
            finalScoreText.text = finalScoreTemplate.Replace("{score}", "0");
            if (!scoreWindow.skip)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                AudioManager.PlayUISound(scoreWindow.startSound, 0.7f);
            }
            scoreTitle.transform
                .DOPunchScale(Vector3.one * -0.05f, 0.5f)
                .SetUpdate(isIndependentUpdate: true);
            scoreTitle.transform
                .DOPunchRotation(Vector3.forward * 5f, 0.5f, 16)
                .SetUpdate(isIndependentUpdate: true);
            scoreTitle
                .DOColor(scoreTitleColor, 0.5f)
                .SetUpdate(isIndependentUpdate: true);
            loadingTextAnimator.SetText(loadingText);
            loadingTextAnimator.TMProComponent.color = scoreTitleColor;
            loadingTextTypewriter.StartShowingText(restart: true);
            if (!scoreWindow.skip)
            {
                yield return new WaitForSecondsRealtime(0.8f);
            }
            loadingTextTypewriter.StartDisappearingText();
            distanceText.text = distanceDefaultText;
            speedText.text = speedDefaultText;
            baseScoreText.text = baseScoreTemplate.Replace("{score}", "0");
            if (currentScore > 10f)
            {
                scoreWindow.PlayTickSound();
                if (!scoreWindow.skip)
                {
                    yield return new WaitForSecondsRealtime(0.5f);
                }
                tickSound.Play();
                var tickTime = 0f;
                while (tickTime < 1f && !scoreWindow.skip)
                {
                    tickTime += Time.unscaledDeltaTime;
                    var lerpDistance = Mathf
                        .RoundToInt(Mathf.Lerp(0f, distance, tickTime))
                        .ToString();
                    UpdateDistanceText(lerpDistance);
                    var lerpSpeed = Math
                        .Round(Mathf.Lerp(0f, speed, tickTime), 2)
                        .ToString();
                    UpdateSpeedText(lerpSpeed);
                    var lerpBaseScore = Mathf
                        .Round(Mathf.Lerp(0f, currentScore, tickTime))
                        .ToString();
                    baseScoreText.text = baseScoreTemplate
                        .Replace("{score}", lerpBaseScore);
                    finalScoreTextAnimator.SetText(
                        finalScoreTemplate.Replace("{score}", lerpBaseScore)
                    );
                    yield return null;
                }
                tickSound.Stop();
            }
            UpdateDistanceText(Mathf.RoundToInt(distance).ToString());
            UpdateSpeedText(Math.Round(speed, 2).ToString());
            baseScoreText.text =
                "<color=grey>"
                + baseScoreTemplate.Replace(
                    "{score}",
                    currentScore.ToString()
                );
            baseScoreText.transform
                .DOPunchScale(Vector3.one * 0.02f, 0.5f)
                .SetUpdate(isIndependentUpdate: true);
            DOTween.Complete(finalScoreText.transform);
            finalScoreText.text =
                finalScoreTemplate.Replace("{score}", currentScore.ToString());
            finalScoreText.transform
                .DOPunchScale(Vector3.one * 0.02f, 0.5f)
                .SetUpdate(isIndependentUpdate: true);
            scoreWindow.PlayTickSound();
            if (!scoreWindow.skip)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }
            modifierTitle.gameObject.SetActive(true);
            var scores = CL_ScoreManager.sessionScore.scores;
            if (scores.Count > 0)
            {
                foreach (var score in scores)
                {
                    scoreWindow.PlayTickSound();
                    if (!scoreWindow.skip)
                    {
                        yield return new WaitForSecondsRealtime(0.08f);
                    }
                    var scoreItem = UnityEngine.Object
                        .Instantiate(scoreItemAsset, scoreItemRoot);
                    scoreItem.Initialize(
                        score.title,
                        score.bonus,
                        score.multiplier,
                        score.count
                    );
                    scoreItem.transform
                        .DOPunchScale(Vector3.one * 0.05f, 0.5f)
                        .SetUpdate(isIndependentUpdate: true);
                    currentScore += score.bonus;
                    currentMultiplier += score.multiplier;
                    var currentFinalScore = Mathf
                        .Round(currentScore * currentMultiplier)
                        .ToString();
                    if (!scoreWindow.skip)
                    {
                        yield return new WaitForSecondsRealtime(0.08f);
                    }
                    finalScoreText.text = finalScoreTemplate
                        .Replace("{score}", currentFinalScore);
                    DOTween.Complete(finalScoreText.transform);
                }
            }
            finalScoreText.transform
                .DOPunchScale(Vector3.one * 0.2f, 0.8f)
                .SetUpdate(isIndependentUpdate: true);
            finalScoreText
                .DOColor(UnityEngine.Color.white, 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .SetUpdate(isIndependentUpdate: true);
            var isHighScore = M_Gamemode.IsCurrentlyAHighScore();
            if (isHighScore)
            {
                finishSound = scoreWindow.personalBestFinishSound;
            }
            else
            {
                var soundIndex =
                    finishSounds.FindIndex(s => s.minimumScore < finalScore);
                finishSound = finishSounds[Math.Max(0, soundIndex)];
            }
            finalScoreText.text =
                finishSound.scoreTitlePrefix
                + finalScoreTemplate.Replace("{score}", finalScore.ToString())
                + finishSound.scoreTitleSuffix;
            if (!scoreWindow.skip)
            {
                AudioManager.PlayUISound(finishSound.clip, 0.7f);
                scoreWindow.transform
                    .DOPunchRotation(Vector3.forward * 0.5f, 0.5f)
                    .SetUpdate(isIndependentUpdate: true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            if (isHighScore)
            {
                highScoreText.gameObject.SetActive(true);
                highScoreText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }
            if (!scoreWindow.skip)
            {
                yield return new WaitForSecondsRealtime(3.5f);
            }
            CL_ProgressionManager.instance?.deathList?.Check();
            void UpdateDistanceText(string meter)
            {
                var distanceFillerLength =
                    Math.Min(fillerText.Length, meter.Length);
                var distanceFillerText = fillerText
                    .Substring(distanceFillerLength);
                distanceText.text = distanceTemplate
                    .Replace("{meter}", meter)
                    .Replace("{filler}", distanceFillerText);
            }
            void UpdateSpeedText(string mps)
            {
                var speedFillerLength =
                    Math.Min(fillerText.Length, mps.Length);
                var speedFillerText = fillerText
                    .Substring(speedFillerLength);
                speedText.text = speedTemplate
                    .Replace("{mps}", mps)
                    .Replace("{filler}", speedFillerText);
            }
        }
    }
}

