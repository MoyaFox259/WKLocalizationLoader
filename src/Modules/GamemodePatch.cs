using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
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
        public static Dictionary<string, string> NewGameTexts;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeIntroTexts;
        [JsonProperty]
        public static Dictionary<string, string> GamemodeTextPrefixes;
        // [JsonProperty]
        // public static Dictionary<string, string> Medals;
        [JsonProperty]
        public static Dictionary<string, string> ModifierTitles;
        [JsonProperty]
        public static Dictionary<string, string> ModifierDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> ModifierAppends;
        [JsonProperty]
        public static bool KeepWhiteSpaceInGamemodeName;
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
        public GamemodePatchSettings ModuleSettings;

        public static void PatchScriptableObjects()
        {
            PatchGamemodes();
            PatchGamemodeSettings();
        }

        public static void PatchGamemodes()
        {
            if (!IsEnabled) return;
            var gamemodes = CacheManager
                .EnumerateScriptableObjects<M_Gamemode>();
            foreach (var gamemode in gamemodes)
            {
                gamemode.unlockHint = GetTextTranslation(
                    GamemodeUnlockHints,
                    gamemode.unlockHint
                );
                gamemode.modeDescription = GetTextTranslation(
                    GamemodeDescriptions,
                    gamemode.modeDescription
                );
                gamemode.newGameText = GetTextTranslation(
                    NewGameTexts,
                    gamemode.newGameText
                );
                gamemode.introText = GetTextTranslation(
                    GamemodeIntroTexts,
                    gamemode.introText
                );
            }
        }

        public static void PatchGamemodeSettings()
        {
            if (!IsEnabled) return;
            var gamemodeSettings = CacheManager
                .EnumerateScriptableObjects<GamemodeSetting>();
            foreach (var gamemodeSetting in gamemodeSettings)
            {
                gamemodeSetting.title = GetTextTranslation(
                    ModifierTitles,
                    gamemodeSetting.title
                );
                gamemodeSetting.description = GetTextTranslation(
                    ModifierDescriptions,
                    gamemodeSetting.description
                );
            }
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
            var gamemodeText = __instance.text.text;
            var gamemode = CL_GameManager.gamemode;
            var gamemodeName = gamemode.gamemodeName;
            var gamemodeTextSegments = gamemodeText.Split(
                new string[] { gamemodeName },
                2,
                StringSplitOptions.None
            );
            if (gamemodeTextSegments.Length != 2) return;
            var prefix = gamemodeTextSegments[0];
            var modifiers = gamemodeTextSegments[1];
            if (GamemodeTextPrefixes != null)
            {
                prefix = GetTextTranslation(GamemodeTextPrefixes, prefix);
            }
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
                    KeepWhiteSpaceInGamemodeName ? " " : ""
                );
            }
            if (__instance.includeGamemodeOptions && ModifierAppends != null)
            {
                foreach (var modifierAppend in ModifierAppends)
                {
                    var originalAppend = modifierAppend.Key;
                    var translatedAppend = modifierAppend.Value;
                    if (originalAppend is null || translatedAppend is null)
                    {
                        continue;
                    }
                    modifiers = modifiers.Replace(
                        originalAppend,
                        translatedAppend
                    );
                }
            }
            var gamemodeTextTemplate = GamemodeTextTemplate
                ?? "{prefix}{gamemodeName}{modifierAppends}";
            __instance.text.text = gamemodeTextTemplate
                .Replace("{prefix}", prefix)
                .Replace("{gamemodeName}", gamemodeName)
                .Replace("{modifierAppends}", modifiers);
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

        public static string GetTranslatedProgress(ProgressionUnlock unlock)
        {
            var progress = unlock.GetProgressString();
            if (progress != "N/A" && ModifierUnlockProgressTemplate != null)
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

