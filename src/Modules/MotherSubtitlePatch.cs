using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class MotherSubtitlePatch : ModuleBase<MotherSubtitlePatch>
    {
        [JsonProperty]
        public static string RandomCharacters;
        [JsonProperty]
        public static string NonRandomCharacters;
        [JsonProperty]
        public static Dictionary<string, string> MotherSubtitles;

        [JsonIgnore]
        public static MotherSubtitlePatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CL_LocalizationManager.Localization),
            nameof(CL_LocalizationManager.Localization.GetLine)
        )]
        public static string Postfix_GetLine(
            string __result,
            string group,
            string key
        )
        {
            if (
                !IsEnabled
                || group != "mother"
                || MotherSubtitles is null
                || !MotherSubtitles.ContainsKey(key)
            )
            {
                return __result;
            }
            return MotherSubtitles[key] ?? __result;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(HUD_CustomElement_PsychicCommunication),
            nameof(HUD_CustomElement_PsychicCommunication.PlaySubtitle)
        )]
        public static IEnumerable<CodeInstruction> Transpiler_PlaySubtitle(
            IEnumerable<CodeInstruction> codeInstructions
        )
        {
            var codeMatcher = new CodeMatcher(codeInstructions);
            codeMatcher.MatchForward(
                false,
                new CodeMatch(
                    i => (
                        i.opcode == OpCodes.Stfld
                        && i.operand is FieldInfo f
                        && f.Name == "rand"
                    )
                )
            );
            if (!codeMatcher.IsValid) return codeInstructions;
            var randField = (FieldInfo)codeMatcher.Instruction.operand;
            var startStringField = randField.DeclaringType
                .GetField("startString");
            codeMatcher.MatchBack(
                false,
                new CodeMatch(OpCodes.Ldstr, "abcdefghijklmnopqrstuvwxyz")
            );
            if (!codeMatcher.IsValid) return codeInstructions;
            var getRandomCharacters =
                typeof(MotherSubtitlePatch).GetMethod("GetRandomCharacters");
            codeMatcher.RemoveInstruction();
            codeMatcher.Insert(
                new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldfld, startStringField),
                    new CodeInstruction(OpCodes.Call, getRandomCharacters)
                }
            );
            return codeMatcher.InstructionEnumeration();
        }

        public static string GetRandomCharacters(string startString)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(startString))
            {
                return "abcdefghijklmnopqrstuvwxyz";
            }
            var randomCharacters =
                string.IsNullOrEmpty(RandomCharacters)
                ? new string(startString.Distinct().ToArray())
                : RandomCharacters;
            randomCharacters = randomCharacters.ToLower();
            if (!string.IsNullOrEmpty(NonRandomCharacters))
            {
                NonRandomCharacters = NonRandomCharacters.ToLower();
                randomCharacters = new string(
                    randomCharacters
                        .Where(c => !NonRandomCharacters.Contains(c))
                        .ToArray()
                );
            }
            return randomCharacters;
        }
    }
}

