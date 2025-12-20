using System;
using System.Collections.Generic;
using HarmonyLib;

namespace UnfairFlipsAPMod;

public class ShopHandler
{
    /*
     * Shop Design Notes:
     * Shop costs should be set based on fairness
     * Must also hook money to ensure there is a cap on the amount of money earned for each fairness level
     * Can display item names on shop items
     * Must have bought previous shop item for next to show up
     * Check location checked for the shop level in update based on fairness
     * If location is not checked, try scouting, else display the next valid level
     * Control which buttons can be clicked through the money cap alone to avoid saving extra data
     */

    private static List<long> scoutedLocations = [];
    private static readonly Dictionary<ShopButton, long> currentLocationForButton = new();
    
    [HarmonyPatch(typeof(ShopButton))]
    public class ShopButton_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(ShopButton __instance)
        {
            if (!UnfairFlipsAPMod.ArchipelagoHandler.IsConnected ||
                UnfairFlipsAPMod.SlotData == null)
                return false;

            var gateCount = UnfairFlipsAPMod.SlotData.RequiredHeads / 2;

            for (var gateIndex = 0; gateIndex < gateCount; gateIndex++)
            {
                for (var layer = 0; layer < ArchipelagoConstants.ShopLayers; layer++)
                {
                    var shopIndex = gateIndex * ArchipelagoConstants.ShopLayers + layer;
                    long locationId = 0x200 + shopIndex * 4 + (int)__instance.upgradeType;

                    if (UnfairFlipsAPMod.ArchipelagoHandler.IsLocationChecked(locationId))
                        continue;

                    if (!scoutedLocations.Contains(locationId))
                    {
                        var info = UnfairFlipsAPMod.ArchipelagoHandler.TryScoutLocation(locationId);
                        scoutedLocations.Add(locationId);

                        __instance.currentCost = (int)Math.Ceiling(
                            Math.Pow(10, gateIndex) *
                            UnityEngine.Random.Range(0.6f, 0.9f)
                        );

                        __instance.text.text =
                            $"{info.ItemDisplayName}\n{Mathy.CentsToDollarString(__instance.currentCost)}";
                    }

                    __instance.button.interactable =
                        UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney >= __instance.currentCost;

                    currentLocationForButton[__instance] = locationId;
                    return false;
                }
            }

            currentLocationForButton.Remove(__instance);
            __instance.gameObject.SetActive(false);
            return false;
        }
        
        [HarmonyPatch("Buy")]
        [HarmonyPrefix]
        public static bool Buy_Prefix(ShopButton __instance)
        {
            if (!currentLocationForButton.TryGetValue(__instance, out var locationId))
                return false;
            UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney -= __instance.currentCost;
            UnfairFlipsAPMod.ArchipelagoHandler.CheckLocation(locationId);
            return false;
        }

        [HarmonyPatch("SetShopLevel")]
        [HarmonyPrefix]
        public static bool SetShopLevel_Prefix(int level)
        {
            return false;
        }
        
        [HarmonyPatch("IncreaseHeadsChance")]
        [HarmonyPrefix]
        public static bool  IncreaseHeadsChance(float amount)
        {
          return false;
        }
        
        [HarmonyPatch("DecreaseFlipTime")]
        [HarmonyPrefix]
        public static bool DecreaseFlipTime(float amount)
        {
            return false;
        }
        
        [HarmonyPatch("IncreaseFlipMultiplier")]
        [HarmonyPrefix]
        public static bool IncreaseFlipMultiplier(float amount)
        {
            return false;
        }

                
        [HarmonyPatch("IncreaseFlipBaseWorth")]
        [HarmonyPrefix]
        public static bool IncreaseFlipBaseWorth()
        {
            return false;
        }
    }
}