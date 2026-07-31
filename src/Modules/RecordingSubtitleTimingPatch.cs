using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(
        typeof(CL_LocalizationManager.Localization),
        nameof(CL_LocalizationManager.Localization.GetLine)
    )]
    public class RecordingSubtitleTimingPatch
        : ModuleBase<RecordingSubtitleTimingPatch>
    {
        [JsonProperty]
        public static RecordingSubtitleTimingPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, List<float>>
            RecordingSubtitleTimings;

        [JsonIgnore]
        public readonly static string[] LinebreakPattern =
            new string[] { @"<br>" };
        [JsonIgnore]
        public readonly static Regex DelayRegex = CacheManager
            .GetOrCreateRegex(
                @"<delay\s*=\s*([+-]?\d*(?:\.\d+)?|\d+)>",
                (
                    RegexOptions.IgnoreCase
                    | RegexOptions.Compiled
                )
            );

        [HarmonyPostfix]
        public static string Postfix(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "recordings"
                || RecordingSubtitleTimings is null
                || !RecordingSubtitleTimings.ContainsKey(key)
                || (
                    ModuleSettings.UseOriginalDelay
                    && DelayRegex.IsMatch(__result)
                )
            )
            {
                return __result;
            }
            return RebuildSubtitleTextWithTimings(
                __result,
                RecordingSubtitleTimings[key]
            );
        }

        public static string RebuildSubtitleTextWithTimings(
            string subtitleText,
            List<float> subtitleTimings
        )
        {
            if (subtitleTimings is null || subtitleTimings.Count == 0)
            {
                return subtitleText;
            }
            var subtitleLines = subtitleText.Split(
                LinebreakPattern,
                StringSplitOptions.None
            );
            var count = Math.Min(subtitleLines.Length, subtitleTimings.Count);
            for (int lineIndex = 0; lineIndex < count; lineIndex++)
            {
                var subtitleLine = subtitleLines[lineIndex];
                subtitleLine = RemoveDelayTag(subtitleLine);
                var currentLineDuration =
                    subtitleLine.Length * ModuleSettings.CharacterInterval
                    + ModuleSettings.BaseDuration;
                var targetLineDuration = subtitleTimings[lineIndex];
                if (lineIndex > 0)
                {
                    targetLineDuration -= subtitleTimings[lineIndex - 1];
                }
                if (lineIndex == count - 1)
                {
                    targetLineDuration += ModuleSettings.EndDelay;
                }
                var lineDelay = targetLineDuration - currentLineDuration;
                var delayTag = lineDelay < 0
                    ? $"<delay={lineDelay:F3}>"
                    : $"<delay={lineDelay:F4}>";
                subtitleLine = InsertDelayTag(subtitleLine, delayTag);
                subtitleLines[lineIndex] = subtitleLine;
            }
            return string.Join(LinebreakPattern[0], subtitleLines);
        }

        public static string RemoveDelayTag(string subtitleLine)
        {
            var delayMatch = DelayRegex.Match(subtitleLine);
            return delayMatch.Success
                ? subtitleLine.Remove(delayMatch.Index, delayMatch.Length)
                : subtitleLine;
        }

        public static string InsertDelayTag(
            string subtitleLine,
            string delayTag
        )
        {
            if (string.IsNullOrEmpty(delayTag)) return subtitleLine;
            var delayMatch = DelayRegex.Match(subtitleLine);
            return delayMatch.Success
                ? subtitleLine.Insert(delayMatch.Index, delayTag)
                : subtitleLine + delayTag;
        }
    }
}

