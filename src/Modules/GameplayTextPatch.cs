using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class GameplayTextPatch : TextTranslator<GameplayTextPatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> RoachCounterTemplates;
        [JsonProperty]
        public static string ScoreTrackerTemplate;
        [JsonProperty]
        public static string DistanceTrackerTemplate;
        [JsonProperty]
        public static string SpeedTrackerTemplate;
        [JsonProperty]
        public static string HighScoreTrackerTemplate;
        [JsonProperty]
        public static string VendorUnavailableText;
        [JsonProperty]
        public static string VendorCostTemplate;
        [JsonProperty]
        public static string VendorPurchasedText;

        [JsonIgnore]
        public static GameplayTextPatchSettings ModuleSettings;

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(CL_GameManager),
            nameof(CL_GameManager.Update)
        )]
        public static void Postfix_GameManager_Update(
            CL_GameManager __instance
        )
        {
            if (!IsEnabled || CL_GameManager.runHasEnded) return;
            var uiManager = __instance.uiMan;
            if (uiManager is null || uiManager.scoreTracker is null) return;
            var scoreText = uiManager.scoreTracker.text;
            if (
                scoreText.StartsWith("Score: ")
                && ScoreTrackerTemplate != null
            )
            {
                var score = scoreText.Substring(7);
                uiManager.scoreTracker.text = ScoreTrackerTemplate
                    .Replace("{score}", score);
            }
            var distanceText = uiManager.ascentTracker.text;
            if (
                distanceText.StartsWith("Climb Distance: ")
                && DistanceTrackerTemplate != null
            )
            {
                var distance = distanceText.Substring(16);
                uiManager.ascentTracker.text = DistanceTrackerTemplate
                    .Replace("{distance}", distance);
            }
            var speedText = uiManager.ascentRateTracker.text;
            if (
                speedText.StartsWith("Climb Speed: ")
                && SpeedTrackerTemplate != null
            )
            {
                var speed = speedText.Substring(13);
                uiManager.ascentRateTracker.text = SpeedTrackerTemplate
                    .Replace("{speed}", speed);
            }
            var highScoreText = uiManager.highScoreTracker.text;
            if (
                highScoreText.StartsWith("High Score: ")
                && HighScoreTrackerTemplate != null
            )
            {
                var highScore = highScoreText.Substring(12);
                uiManager.highScoreTracker.text = HighScoreTrackerTemplate
                    .Replace("{highScore}", highScore);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UT_RoachTextCounter),
            nameof(UT_RoachTextCounter.UpdateText)
        )]
        public static void Prefix_RoachTextCounter_UpdateText(
            UT_RoachTextCounter __instance
        )
        {
            if (!IsEnabled) return;
            __instance.textFormat = GetTextTranslation(
                RoachCounterTemplates,
                __instance.textFormat
            );
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Disk),
            nameof(ENV_Vendor_Disk.CheckBlock)
        )]
        public static void Prefix_DiskVendor_CheckBlock(
            ENV_Vendor_Disk __instance
        )
        {
            if (
                !IsEnabled
                || VendorUnavailableText is null
                || __instance.isBlocked
                || __instance.hasBeenBought
                || !CL_GameManager.HasActiveFlag("blockshops")
            )
            {
                return;
            }
            __instance.isBlocked = true;
            __instance.purchaseButton.SetInteractable(false);
            __instance.costText.text = VendorUnavailableText;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Disk),
            nameof(ENV_Vendor_Disk.CheckRoaches)
        )]
        public static void Prefix_DiskVendor_CheckRoaches(
            ENV_Vendor_Disk __instance
        )
        {
            if (
                !IsEnabled
                || VendorPurchasedText is null
                || !__instance.allowPurchases
                || __instance.isBlocked
                || __instance.hasBeenBought
                || __instance.id == ""
            )
            {
                return;
            }
            var gameFlag = CL_GameManager
                .GetGameFlag("boughtdisk-" + __instance.id + "-station");
            if (gameFlag != null && gameFlag.state)
            {
                __instance.hasBeenBought = true;
                __instance.costText.text = VendorPurchasedText;
                __instance.purchaseSprite.gameObject.SetActive(false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Disk),
            nameof(ENV_Vendor_Disk.CheckRoaches)
        )]
        public static void Postfix_DiskVendor_CheckRoaches(
            ENV_Vendor_Disk __instance
        )
        {
            if (
                !IsEnabled
                || VendorCostTemplate is null
                || !__instance.allowPurchases
                || __instance.isBlocked
                || __instance.hasBeenBought
            )
            {
                return;
            }
            var cost = __instance.cost;
            var balance = CL_GameManager.GetRoaches();
            __instance.costText.text = VendorCostTemplate
                .Replace("{cost}", cost.ToString())
                .Replace("{balance}", balance.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Disk),
            nameof(ENV_Vendor_Disk.Purchase)
        )]
        public static void Postfix_DiskVendor_Purchase(
            ENV_Vendor_Disk __instance
        )
        {
            if (
                !IsEnabled
                || __instance.isBlocked
                || __instance.purchaseSprite.gameObject.activeInHierarchy
                || VendorPurchasedText is null
            )
            {
                return;
            }
            __instance.costText.text = VendorPurchasedText;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Event),
            nameof(ENV_Vendor_Event.CheckRoaches)
        )]
        public static void Prefix_EventVendor_CheckRoaches(
            ENV_Vendor_Event __instance
        )
        {
            if (
                !IsEnabled
                || VendorPurchasedText is null
                || !__instance.allowPurchases
                || __instance.hasBeenBought
                || __instance.id == ""
            )
            {
                return;
            }
            var gameFlag = CL_GameManager
                .GetGameFlag("boughtdisk-" + __instance.id + "-station");
            if (gameFlag != null && gameFlag.state)
            {
                __instance.hasBeenBought = true;
                __instance.costText.text = VendorPurchasedText;
                __instance.purchaseSprite.gameObject.SetActive(false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Event),
            nameof(ENV_Vendor_Event.CheckRoaches)
        )]
        public static void Postfix_EventVendor_CheckRoaches(
            ENV_Vendor_Event __instance
        )
        {
            if (
                !IsEnabled
                || VendorCostTemplate is null
                || !__instance.allowPurchases
                || __instance.hasBeenBought
            )
            {
                return;
            }
            var cost = __instance.cost;
            var balance = CL_GameManager.GetRoaches();
            __instance.costText.text = VendorCostTemplate
                .Replace("{cost}", cost.ToString())
                .Replace("{balance}", balance.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ENV_Vendor_Event),
            nameof(ENV_Vendor_Event.Purchase)
        )]
        public static void Postfix_EventVendor_Purchase(
            ENV_Vendor_Event __instance
        )
        {
            if (
                !IsEnabled
                || __instance.purchaseSprite.gameObject.activeInHierarchy
                || VendorPurchasedText is null
            )
            {
                return;
            }
            __instance.costText.text = VendorPurchasedText;
        }
    }
}

