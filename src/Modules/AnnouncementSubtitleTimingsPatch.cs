using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(CL_LocalizationManager), "Awake")]
    public class AnnouncementSubtitleTimingsPatch
        : ModuleBase<AnnouncementSubtitleTimingsPatch>
    {
        [JsonProperty]
        public static AnnouncementSubtitleTimingsPatchSettings ModuleSettings;
        [JsonProperty]
        public static Dictionary<string, List<float>>
            AnnouncementSubtitleTimings;

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

        public static void Postfix(CL_LocalizationManager __instance)
        {
            if (
                !IsEnabled
                || Harmony.HasAnyPatches("mimimi-turret.wk-sync-subtitles")
            )
            {
                return;
            }
            ApplyTimingOverrides(
                CL_LocalizationManager.currentLocalization.announcements
            );
        }

        public static void ApplyTimingOverrides(
            Dictionary<string, string> subtitles
        )
        {
            if (AnnouncementSubtitleTimings is null) return;
            foreach (var subtitle in subtitles)
            {
                var audioID = subtitle.Key;
                var subtitleText = subtitle.Value;
                if (
                    !(
                        ModuleSettings.UseOriginalDelay
                        && DelayRegex.Match(subtitleText).Success
                    )
                    && AnnouncementSubtitleTimings.TryGetValue(
                        audioID,
                        out List<float> subtitleTimings
                    )
                )
                {
                    subtitles[audioID] = RebuildSubtitleTextWithTimings(
                        subtitleText,
                        subtitleTimings
                    );
                }
            }
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

