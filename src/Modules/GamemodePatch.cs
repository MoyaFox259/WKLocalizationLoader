using System;
using System.Linq;
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
        [JsonProperty("GamemodeNames")]
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
        [JsonProperty]
        public static Dictionary<string, string> Medals;
        [JsonProperty]
        public static Dictionary<string, string> ModifierTitles;
        [JsonProperty]
        public static Dictionary<string, string> ModifierDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> ModifierAppends;
        [JsonProperty]
        public static string GamemodeTextTemplate;
        [JsonProperty]
        public static bool KeepWhiteSpaceInGamemodeName;

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
                TryGetTranslatedCapsuleName(
                    __instance.gamemode.gamemodeName,
                    out string capsuleName
                )
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
            if (gamemodeTextSegments.Length < 2) return;
            var prefix = gamemodeTextSegments[0];
            var modifiers = gamemodeTextSegments[1];
            if (GamemodeTextPrefixes != null)
            {
                prefix = GetTextTranslation(GamemodeTextPrefixes, prefix);
            }
            if (
                TryGetTranslatedCapsuleName(
                    gamemodeName,
                    out string capsuleName
                )
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

        public static bool TryGetTranslatedCapsuleName(
            string gamemodeName,
            out string capsuleName
        )
        {
            if (
                CapsuleNames != null
                && CapsuleNames.TryGetValue(
                    gamemodeName,
                    out capsuleName
                )
                && capsuleName != null
            )
            {
                return true;
            }
            capsuleName = gamemodeName;
            return false;
        }
    }
}

