using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class GamemodePatch
        : TextTranslator<GamemodePatch>, IScriptableObjectPatch
    {
        [JsonProperty]
        public static Dictionary<string, string> CapsuleNames;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeUnlockHints;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeNewGameTexts;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeMedalTitles;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeTextPrefixes;
        [JsonProperty]
        public static Dictionary<string, string> ModifierTitles;
        [JsonProperty]
        public static Dictionary<string, string> ModifierDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> ModifierAppends;
        [JsonProperty]
        public static bool KeepWhiteSpaceInCapsuleName;
        [JsonProperty]
        public static string GamemodeTextTemplate;
        [JsonProperty]
        public static string ModifierConflictedDescription;
        [JsonProperty]
        public static string ModifierLockedDescriptionTemplate;
        [JsonProperty]
        public static string ModifierUnlockProgressTemplate;

        [JsonIgnore]
        public static Regex WhiteSpaceRegex = CacheManager.GetOrCreateRegex(
            @"\s+|<br\s*>",
            RegexOptions.IgnoreCase
        );
        [JsonIgnore]
        public static GamemodePatchSettings ModuleSettings;

        public static void PatchScriptableObjects()
        {
            if (!IsEnabled) return;
            var database = CL_AssetManager.GetFullCombinedAssetDatabase();
            database?.gamemodeAssets?.ForEach(i => PatchGamemode(i));
        }

        public static void PatchGamemode(M_Gamemode gamemode)
        {
            if (gamemode is null) return;
            gamemode.unlockHint = GetTextTranslation(
                GamemodeUnlockHints,
                gamemode.unlockHint
            );
            gamemode.modeDescription = GetTextTranslation(
                GamemodeDescriptions,
                gamemode.modeDescription
            );
            gamemode.newGameText = GetTextTranslation(
                GamemodeNewGameTexts,
                gamemode.newGameText
            );
            gamemode.introText = GetTextTranslation(
                GamemodeIntroTexts,
                gamemode.introText
            );
            gamemode.gamemodeSettings?.ForEach(i => PatchGamemodeSetting(i));
        }

        public static void PatchGamemodeSetting(
            GamemodeSetting gamemodeSetting
        )
        {
            if (gamemodeSetting is null) return;
            gamemodeSetting.title = GetTextTranslation(
                ModifierTitles,
                gamemodeSetting.title
            );
            gamemodeSetting.description = GetTextTranslation(
                ModifierDescriptions,
                gamemodeSetting.description
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_Gamemode_Button),
            nameof(UI_Gamemode_Button.Initialize)
        )]
        public static void Postfix_GamemodeButton_Initialize(
            UI_Gamemode_Button __instance
        )
        {
            if (!IsEnabled || __instance.gamemode is null) return;
            if (
                CapsuleNames != null
                && CapsuleNames.TryGetValue(
                    __instance.gamemode.gamemodeName,
                    out string capsuleName
                )
                && capsuleName != null
            )
            {
                __instance.title.text = capsuleName;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_GamemodeText),
            nameof(UI_GamemodeText.Refresh)
        )]
        public static void Postfix_GamemodeText_Refresh(
            UI_GamemodeText __instance
        )
        {
            if (!IsEnabled || CL_GameManager.gamemode is null) return;
            __instance.text.text = GetTranslatedGamemodeText(
                CL_GameManager.gamemode,
                __instance.text.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_LeaderboardEntryDetailWindow),
            nameof(UI_LeaderboardEntryDetailWindow.ShowDetails)
        )]
        public static void Postfix_LeaderboardEntryWindow_ShowDetails(
            UI_LeaderboardEntryDetailWindow __instance
        )
        {
            if (!IsEnabled) return;
            __instance.gamemodeText.text = GetTranslatedGamemodeText(
                CL_GameManager.GetBaseGamemode(),
                __instance.gamemodeText.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_LeaderboardEntryDetailWindow),
            nameof(UI_LeaderboardEntryDetailWindow.ShowNoEntryDetails)
        )]
        public static void Postfix_LeaderboardEntryWindow_ShowNoEntryDetails(
            UI_LeaderboardEntryDetailWindow __instance
        )
        {
            if (!IsEnabled) return;
            __instance.gamemodeText.text = GetTranslatedGamemodeText(
                CL_GameManager.GetBaseGamemode(),
                __instance.gamemodeText.text
            );
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
                GamemodeMedalTitles,
                __instance.scoreRankText.text
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_GamemodeSetting),
            nameof(UI_GamemodeSetting.UpdateColor)
        )]
        public static void Postfix_GamemodeSettingUI_UpdateColor(
            UI_GamemodeSetting __instance
        )
        {
            if (!IsEnabled) return;
            var descriptionText = __instance.descriptionText.text;
            if (descriptionText == "LOCKED")
            {
                __instance.descriptionText.text =
                    ModifierConflictedDescription ?? "LOCKED";
                return;
            }
            var unlock = __instance.gamemodeSetting.unlock;
            if (unlock.CheckUnlock()) return;
            var progress = unlock.showProgression
                ? GetTranslatedProgress(unlock)
                : "";
            var descriptionTemplate = ModifierLockedDescriptionTemplate
                ?? "{unlockHint}{progress}";
            __instance.descriptionText.text = descriptionTemplate
                .Replace("{unlockHint}", unlock.unlockHint)
                .Replace("{progress}", progress);
        }

        public static string GetTranslatedGamemodeText(
            M_Gamemode gamemode,
            string gamemodeText
        )
        {
            var gamemodeName = gamemode.gamemodeName;
            var gamemodeTextSegments = gamemodeText.Split(
                new string[] { gamemodeName },
                2,
                StringSplitOptions.None
            );
            if (gamemodeTextSegments.Length != 2) return gamemodeText;
            var textPrefix = gamemodeTextSegments[0];
            var modifierAppends = gamemodeTextSegments[1];
            textPrefix = GetTextTranslation(GamemodeTextPrefixes, textPrefix);
            if (
                CapsuleNames != null
                && CapsuleNames.TryGetValue(
                    gamemodeName,
                    out string capsuleName
                )
                && capsuleName != null
            )
            {
                gamemodeName = WhiteSpaceRegex.Replace(
                    capsuleName,
                    KeepWhiteSpaceInCapsuleName ? " " : ""
                );
            }
            if (ModifierAppends != null)
            {
                foreach (var translation in ModifierAppends)
                {
                    if (translation.Value is null) continue;
                    modifierAppends = modifierAppends.Replace(
                        translation.Key,
                        translation.Value
                    );
                }
            }
            var gamemodeTextTemplate = GamemodeTextTemplate
                ?? "{textPrefix}{capsuleName}{modifierAppends}";
            return gamemodeTextTemplate
                .Replace("{textPrefix}", textPrefix)
                .Replace("{capsuleName}", gamemodeName)
                .Replace("{modifierAppends}", modifierAppends);
        }

        public static string GetTranslatedProgress(ProgressionUnlock unlock)
        {
            var progress = unlock.GetProgressString();
            if (ModifierUnlockProgressTemplate != null)
            {
                var progressSegments = progress.Split(new char[] { '/' }, 2);
                if (progressSegments.Length == 2)
                {
                    var current = progressSegments[0];
                    var required = progressSegments[1];
                    progress = ModifierUnlockProgressTemplate
                        .Replace("{current}", current)
                        .Replace("{required}", required);
                }
            }
            return progress;
        }
    }
}

