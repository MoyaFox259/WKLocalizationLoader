using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class ProgressionUnlockPatch
        : TextTranslator<ProgressionUnlockPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> LogbookUnlockTitles;
        [JsonProperty]
        public static Dictionary<string, string> UnlockTitles;
        [JsonProperty]
        public static Dictionary<string, string> UnlockDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> UnlockHints;
        [JsonProperty]
        public static Dictionary<string, string> UnlockProgressTextTemplates;
        [JsonProperty]
        public static string LogbookUnlockedDescription;
        [JsonProperty]
        public static string LogbookRedactedTitle;

        [JsonIgnore]
        public static ProgressionUnlockPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ProgressionUnlock),
            nameof(ProgressionUnlock.CheckUnlock)
        )]
        public static void Postfix_ProgressionUnlock_CheckUnlock(
            ProgressionUnlock __instance
        )
        {
            if (!IsEnabled) return;
            __instance.unlockLogDescription = GetTextTranslation(
                LogbookUnlockTitles,
                __instance.unlockLogDescription
            );
            __instance.unlockTitle = GetTextTranslation(
                UnlockTitles,
                __instance.unlockTitle
            );
            __instance.unlockDescription = GetTextTranslation(
                UnlockDescriptions,
                __instance.unlockDescription
            );
            __instance.unlockHint = GetTextTranslation(
                UnlockHints,
                __instance.unlockHint
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_ProgressionLog),
            nameof(UI_ProgressionLog.Start)
        )]
        public static void Postfix_ProgressionLog_Start(
            UI_ProgressionLog __instance
        )
        {
            if (!IsEnabled) return;
            var unlock = __instance.unlock;
            var prerequisiteUnlock = __instance.prerequisite;
            var popup = __instance.GetComponent<UI_ProgressionPopup>();
            if (
                unlock.CheckUnlock()
                && (
                    prerequisiteUnlock is null
                    || prerequisiteUnlock.CheckUnlock()
                )
            )
            {
                popup.UpdateInformation(
                    unlock.unlockIcon,
                    unlock.unlockLogDescription,
                    LogbookUnlockedDescription ?? "Unlocked"
                );
                return;
            }
            var lockedDescription = unlock.unlockHint;
            if (unlock.showProgression && unlock.GetProgress() > 0f)
            {
                lockedDescription = GetTranslatedProgressText(unlock);
            }
            popup.UpdateInformation(
                __instance.unknownIcon,
                LogbookRedactedTitle ?? "REDACTED",
                lockedDescription
            );
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(CL_ProgressionManager),
            nameof(CL_ProgressionManager.UpdateUnlocks)
        )]
        public static IEnumerable<CodeInstruction>
        Transpiler_ProgressionManager_UpdateUnlocks(
            IEnumerable<CodeInstruction> codeInstructions
        )
        {
            var codeMatcher = new CodeMatcher(codeInstructions);
            codeMatcher.MatchForward(
                false,
                new CodeMatch(OpCodes.Ldstr, "Progress: ")
            );
            if (!codeMatcher.IsValid) return codeInstructions;
            var startInstructionIndex = codeMatcher.Pos;
            codeMatcher.MatchForward(
                false,
                new CodeMatch(
                    OpCodes.Call,
                    typeof(string).GetMethod(
                        "Concat",
                        new Type[] {
                            typeof(string),
                            typeof(string),
                            typeof(string),
                            typeof(string)
                        }
                    )
                )
            );
            if (!codeMatcher.IsValid) return codeInstructions;
            var endInstructionIndex = codeMatcher.Pos;
            var getTranslatedProgressText = typeof(ProgressionUnlockPatch)
                .GetMethod("GetTranslatedProgressText");
            codeMatcher.RemoveInstructionsInRange(
                startInstructionIndex,
                endInstructionIndex
            );
            codeMatcher.Start().Advance(startInstructionIndex);
            codeMatcher.Insert(
                new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(
                        OpCodes.Call,
                        getTranslatedProgressText
                    )
                }
            );
            return codeMatcher.InstructionEnumeration();
        }

        public static string GetTranslatedProgressText(
            ProgressionUnlock unlock
        )
        {
            var progress = unlock.GetProgressString();
            if (progress != "N/A")
            {
                var progressSegments = progress.Split(new char[] { '/' }, 2);
                if (
                    progressSegments.Length == 2
                    && UnlockProgressTextTemplates != null
                    && UnlockProgressTextTemplates.TryGetValue(
                        unlock.id,
                        out string progressTextTemplate
                    )
                    && progressTextTemplate != null
                )
                {
                    var current = progressSegments[0];
                    var required = progressSegments[1];
                    return progressTextTemplate
                        .Replace("{current}", current)
                        .Replace("{required}", required);
                }
            }
            return "Progress: " + progress + " " + unlock.progressionString;
        }
    }
}

