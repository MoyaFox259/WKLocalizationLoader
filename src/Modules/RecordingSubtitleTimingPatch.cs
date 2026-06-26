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
        public static string[] LinebreakPattern = new string[] { @"<br>" };
        [JsonIgnore]
        public static string DelayTagPattern =
            @"<delay\s*=\s*([+-]?\d*(?:\.\d+)?|\d+)>";
        [JsonIgnore]
        public static Regex DelayRegex = new Regex(
            DelayTagPattern,
            RegexOptions.IgnoreCase
        );

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
                var line = subtitleLines[lineIndex].TrimStart();
                line = RemoveDelayTag(line);
                var currentLineDuration =
                    line.Length * ModuleSettings.CharacterInterval
                    + ModuleSettings.BaseDuration;
                var targetLineDuration = subtitleTimings[lineIndex];
                if (lineIndex > 0)
                {
                    targetLineDuration -= subtitleTimings[lineIndex-1];
                }
                else if (lineIndex == count - 1)
                {
                    targetLineDuration += ModuleSettings.EndDelay;
                }
                var lineDelay = targetLineDuration - currentLineDuration;
                line = InsertDelayTag(line, lineDelay);
                subtitleLines[lineIndex] = line;
            }
            return string.Join(LinebreakPattern[0], subtitleLines);
        }

        public static string RemoveDelayTag(string subtitleLine)
        {
            var line = subtitleLine;
            var match = DelayRegex.Match(line);
            if (match.Success)
            {
                line = line.Remove(match.Index, match.Length);
            }
            return line;
        }

        public static string InsertDelayTag(
            string subtitleLine,
            float lineDelay
        )
        {
            if (lineDelay == 0f) return subtitleLine;
            var line = subtitleLine;
            var delay = lineDelay;
            var delayTag =
                "<delay="
                + delay.ToString("G", CultureInfo.InvariantCulture)
                + ">";
            var match = DelayRegex.Match(line);
            if (match.Success)
            {
                line = line.Insert(match.Index, delayTag);
            }
            else
            {
                line += delayTag;
            }
            return line;
        }
    }
}

