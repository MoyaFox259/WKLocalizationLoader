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
    public class HardcodedStringPatch
        : ModuleBase<HardcodedStringPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> TextTranslations;

        [JsonIgnore]
        public static HardcodedStringPatchSettings ModuleSettings;
        [JsonIgnore]
        public static List<MethodBase> TargetMethods = new List<MethodBase>();

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> GetTargetMethods()
        {
            CollectAllTargetMethods();
            foreach (var targetMethod in TargetMethods)
            {
                yield return targetMethod;
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction>
        Transpiler_LocalizeHardcodedStrings(
            IEnumerable<CodeInstruction> codeInstructions
        )
        {
            return LocalizeHardcodedStrings(codeInstructions);
        }

        public static void CollectAllTargetMethods()
        {
            CollectTargetNestedClassMethod(
                typeof(UT_Intro),
                "MoveNext"
            );
            CollectTargetMethod(
                typeof(UT_Intro),
                nameof(UT_Intro.EndIntro)
            );
            CollectTargetMethod(
                typeof(UI_GamemodeScreen),
                nameof(UI_GamemodeScreen.RefreshCurrentGamemode)
            );
            CollectTargetMethod(
                typeof(UI_GamemodeSetting),
                nameof(UI_GamemodeSetting.UpdateColor)
            );
            CollectTargetMethod(
                typeof(UI_TrinketPicker),
                nameof(UI_TrinketPicker.ReloadTrinkets)
            );
            CollectTargetMethod(
                typeof(UI_TrinketPicker),
                nameof(UI_TrinketPicker.UpdateTrinketActivation)
            );
            CollectTargetMethod(
                typeof(Leaderboard_Panel),
                nameof(Leaderboard_Panel.Refresh)
            );
            CollectTargetMethod(
                typeof(Leaderboard_Panel),
                nameof(Leaderboard_Panel.SwitchScore)
            );
            CollectTargetMethod(
                typeof(Leaderboard_Panel),
                nameof(Leaderboard_Panel.SwitchType)
            );
            CollectTargetMethod(
                typeof(UI_LeaderboardEntryDetailWindow),
                nameof(UI_LeaderboardEntryDetailWindow.ShowDetails)
            );
            CollectTargetMethod(
                typeof(UI_LeaderboardEntryDetailWindow),
                nameof(UI_LeaderboardEntryDetailWindow.ShowNoEntryDetails)
            );
            CollectTargetMethod(
                typeof(CL_GameManager),
                nameof(CL_GameManager.Update)
            );
            CollectTargetNestedClassMethod(
                typeof(UI_EndScreenScoreWindow),
                "MoveNext"
            );
            CollectTargetMethod(
                typeof(ENV_Vendor_Disk),
                nameof(ENV_Vendor_Disk.CheckBlock)
            );
            CollectTargetMethod(
                typeof(ENV_Vendor_Disk),
                nameof(ENV_Vendor_Disk.CheckRoaches)
            );
            CollectTargetMethod(
                typeof(ENV_Vendor_Disk),
                nameof(ENV_Vendor_Disk.Purchase)
            );
            CollectTargetMethod(
                typeof(ENV_Vendor_Event),
                nameof(ENV_Vendor_Disk.CheckRoaches)
            );
            CollectTargetMethod(
                typeof(ENV_Vendor_Event),
                nameof(ENV_Vendor_Disk.Purchase)
            );
            CollectTargetMethod(
                typeof(App_Unlocker),
                nameof(App_Unlocker.CheckAuthorize)
            );
            CollectTargetMethod(
                typeof(App_DocumentReader),
                nameof(App_DocumentReader.UpdateButtons)
            );
            CollectTargetMethod(
                typeof(OS_Folder),
                nameof(OS_Folder.UpdateInfoText)
            );
            CollectTargetMethod(
                typeof(App_SavePage),
                nameof(App_SavePage.UpdateSaveText)
            );
            CollectTargetMethod(
                typeof(App_SolarKnight),
                nameof(App_SolarKnight.Update)
            );
            CollectTargetMethod(
                typeof(App_SolarKnight),
                nameof(App_SolarKnight.AddScore)
            );
            CollectTargetMethod(
                typeof(App_SolarKnight),
                nameof(App_SolarKnight.AddLife)
            );
            CollectTargetNestedClassMethod(
                typeof(App_SolarKnight),
                "MoveNext"
            );
            CollectTargetMethod(
                typeof(App_SolarKnight),
                nameof(App_SolarKnight.Reset)
            );
        }

        public static void CollectTargetMethod(
            Type targetClass,
            string methodName
        )
        {
            TargetMethods ??= new List<MethodBase>();
            var method = GetMethod(targetClass, methodName);
            if (method is null || TargetMethods.Contains(method)) return;
            TargetMethods.Add(method);
        }

        public static void CollectTargetNestedClassMethod(
            Type targetClass,
            string methodName
        )
        {
            TargetMethods ??= new List<MethodBase>();
            var method = GetNestedClassMethod(targetClass, methodName);
            if (method is null || TargetMethods.Contains(method)) return;
            TargetMethods.Add(method);
        }

        public static MethodBase GetMethod(
            Type targetClass,
            string methodName
        )
        => targetClass
            .GetMethod(
                methodName,
                (
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly
                )
            );

        public static MethodBase GetNestedClassMethod(
            Type targetClass,
            string methodName
        )
        => targetClass
            .GetNestedTypes(
                BindingFlags.Public
                | BindingFlags.NonPublic
            )
            .SelectMany(
                t => t.GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly
                )
            )
            .FirstOrDefault(m => m.Name == methodName);

        public static IEnumerable<CodeInstruction> LocalizeHardcodedStrings(
            IEnumerable<CodeInstruction> codeInstructions
        )
        {
            var getTextTranslation =
                typeof(HardcodedStringPatch).GetMethod("GetTextTranslation");
            foreach (var codeInstruction in codeInstructions)
            {
                if (
                    codeInstruction.opcode == OpCodes.Ldstr
                    && codeInstruction.operand is string s
                    && TextTranslations != null
                    && TextTranslations.ContainsKey(s)
                )
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, s);
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        getTextTranslation
                    );
                }
                else
                {
                    yield return codeInstruction;
                }
            }
        }

        public static string GetTextTranslation(string originalText)
        {
            if (
                IsEnabled
                && TextTranslations != null
                && TextTranslations.TryGetValue(
                    originalText,
                    out string translatedText
                )
                && translatedText != null
            )
            {
                return translatedText;
            }
            return originalText;
        }
    }
}

