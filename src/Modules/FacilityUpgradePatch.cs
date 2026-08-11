using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;
using TMPro;

namespace WKLocalizationLoader.Modules
{
    [HarmonyPatch]
    public class FacilityUpgradePatch : TextTranslator<FacilityUpgradePatch>
    {
        [JsonProperty]
        public static Dictionary<string, string> UpgradePageTitles;
        [JsonProperty]
        public static Dictionary<string, string> UpgradeTitles;
        [JsonProperty]
        public static Dictionary<string, string> UpgradeDescriptions;
        [JsonProperty]
        public static Dictionary<string, string> UpgradeUnlockDescriptions;
        [JsonProperty]
        public static string UpgradePageCounterTemplate;
        [JsonProperty]
        public static string UpgradeLockedHoverTextTemplate;
        [JsonProperty]
        public static string UpgradeCantAffordHoverTextTemplate;

        [JsonIgnore]
        public static FacilityUpgradePatchSettings ModuleSettings;

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UI_FacilityMenu_Button),
            nameof(UI_FacilityMenu_Button.Initialize)
        )]
        public static void Prefix_FacilityMenuButton_Initialize(
            ref FacilityUpgrade upg
        )
        {
            if (!IsEnabled) return;
            PatchFacilityUpgrade(upg);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UI_FacilityMenu_Button),
            nameof(UI_FacilityMenu_Button.Refresh)
        )]
        public static void Postfix_FacilityMenuButton_Refresh(
            UI_FacilityMenu_Button __instance
        )
        {
            if (!IsEnabled) return;
            var upgrade = __instance.upgrade;
            var facility = __instance.facility;
            if (upgrade is null || facility is null) return;
            if (upgrade.IsLocked(facility.id))
            {
                if (UpgradeLockedHoverTextTemplate != null)
                {
                    __instance.tooltip.tip = UpgradeLockedHoverTextTemplate
                        .Replace("{unlockDescription}", upgrade.unlockDesc)
                        .Replace("{description}", upgrade.description);
                }
                return;
            }
            var balance =
                StatManager.saveData.GetRoachBankByID("campaign").value;
            if (
                upgrade.IsOwned(facility.id)
                || upgrade.cost < balance
                || UpgradeCantAffordHoverTextTemplate is null
            )
            {
                return;
            }
            __instance.tooltip.tip = UpgradeCantAffordHoverTextTemplate
                .Replace("{description}", upgrade.description);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(App_Facility_Card),
            nameof(App_Facility_Card.Initialize)
        )]
        public static void Prefix_FacilityApp_Initialize(
            ref FacilityUpgrade up
        )
        {
            if (!IsEnabled) return;
            PatchFacilityUpgrade(up);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_Facility_Card),
            nameof(App_Facility_Card.CheckLock)
        )]
        public static void Postfix_FacilityAppCard_CheckLock(
            App_Facility_Card __instance
        )
        {
            if (!IsEnabled) return;
            var upgrade = __instance.upgrade;
            if (
                string.IsNullOrEmpty(upgrade.unlockFlag)
                || CL_GameManager.HasActiveFlag(upgrade.unlockFlag, true)
                || upgrade.prerequisiteUpgrade is null
                || __instance.facility.HasUpgrade(
                    upgrade.prerequisiteUpgrade.id
                )
                || UpgradeLockedHoverTextTemplate is null
            )
            {
                return;
            }
            __instance.tooltip.tip = UpgradeLockedHoverTextTemplate
                .Replace("{unlockDescription}", upgrade.unlockDesc)
                .Replace("{description}", upgrade.description);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(App_FacilitySlotHolder),
            nameof(App_FacilitySlotHolder.SetPage)
        )]
        public static void Postfix_FacilitySlotHolder_SetPage(
            List<App_FacilitySlotHolder.UpgradePage> pageList,
            ref int pageNumber,
            TMP_Text titleObject,
            string title
        )
        {
            if (!IsEnabled) return;
            var pageTitle = GetTextTranslation(UpgradePageTitles, title);
            var total = pageList.Count;
            if (total < 2)
            {
                titleObject.text = pageTitle;
                return;
            }
            var current = pageNumber + 1;
            var counterTemplate = UpgradePageCounterTemplate
                ?? "{title} ({current}/{total})";
            titleObject.text = counterTemplate
                .Replace("{title}", pageTitle)
                .Replace("{current}", current.ToString())
                .Replace("{total}", total.ToString());
        }

        public static void PatchFacilityUpgrade(FacilityUpgrade upgrade)
        {
            upgrade.cardName = GetTextTranslation(
                UpgradeTitles,
                upgrade.cardName
            );
            upgrade.description = GetTextTranslation(
                UpgradeDescriptions,
                upgrade.description
            );
            upgrade.unlockDesc = GetTextTranslation(
                UpgradeUnlockDescriptions,
                upgrade.unlockDesc
            );
        }
    }
}

