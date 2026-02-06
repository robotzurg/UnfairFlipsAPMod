using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using BreakInfinity;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace UnfairFlipsAPMod;

public class ShopHandler
{
    /*
     * Shop Design Notes:
     * Shop costs should be set based on fairness
     * Can display item names on shop items
     * Must have bought previous shop item for next to show up
     * Check location checked for the shop level in update based on fairness
     * If location is not checked, try scouting, else display the next valid level
     * Control which buttons can be clicked through the money cap alone to avoid saving extra data
     */

    private static readonly Dictionary<long, ScoutedItemInfo> scoutedLocations = new();
    private static readonly Dictionary<ShopButton, long> currentLocationForButton = new();
    
    [HarmonyPatch(typeof(ShopButton))]
    public class ShopButton_Patch
    {
        public static Dictionary<ShopButton, BigDouble> Costs = new();

        private static Dictionary<ShopButton.UpgradeType, string> PurchaseNames = new()
        {
            { ShopButton.UpgradeType.HeadsChance, "Heads Chance Purchase" },
            { ShopButton.UpgradeType.FlipBaseWorth, "Coin Value Purchase" },
            { ShopButton.UpgradeType.FlipMultiplier, "Combo Mult Purchase" },
            { ShopButton.UpgradeType.FlipTime, "Flip Time Purchase"}
        };
        private static List<int> _valueUpgradeGates = null;
        
        
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(ShopButton __instance)
        {
            __instance.gameObject.AddComponent<ShopButtonHoverHandler>();
        }

        private static List<int> InitializeValueUpgradeGates(int gateCount)
        {
            var gates = new List<int>();
            for (int i = 0; i < 4; i++)
                gates.Add(Mathf.RoundToInt((i + 1) * (gateCount - 1) / 4f));
            return gates;
        }

        public static BigDouble GetCost(int gateIndex)
        {
            var gateCount = (int)Math.Ceiling((double)(UnfairFlipsAPMod.SlotData.RequiredHeads + 1) / 2);
            _valueUpgradeGates ??= InitializeValueUpgradeGates(gateCount);
            var expectedCombo = ArchipelagoConstants.MinComboMultiplier + (ArchipelagoConstants.MaxComboMultiplier - ArchipelagoConstants.MinComboMultiplier) / gateCount * gateIndex;
            var expectedChance = UnfairFlipsAPMod.SlotData.StartingHeadsChance / 100f + (ArchipelagoConstants.MaxHeadsChance - UnfairFlipsAPMod.SlotData.StartingHeadsChance / 100f) / gateCount * gateIndex;
            var numValueUpgrades = _valueUpgradeGates.Count(x => x <= gateIndex);
            var expectedValue = ArchipelagoConstants.CoinValues[numValueUpgrades];
            var maxFlipLength = 1 + gateIndex * 2;
            var baseMult = UnfairFlipsAPMod.SlotData.FlipDifficulty * expectedValue;
            BigDouble expectedMoney = 0;
            for (int i = 0; i < maxFlipLength; i++)
                expectedMoney += BigDouble.Pow(expectedCombo, i) * BigDouble.Pow(expectedChance, i + 1);
            expectedMoney *= baseMult;
            var costMax = expectedMoney;
            var reducer = UnityEngine.Random.Range(0.8f, 0.95f);
            return BigDouble.Ceiling(costMax * reducer);
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(ShopButton __instance)
        {
            if (!UnfairFlipsAPMod.ArchipelagoHandler.IsConnected ||
                UnfairFlipsAPMod.SlotData == null)
                return false;

            var gateCount = (int)Math.Ceiling(((float)UnfairFlipsAPMod.SlotData.RequiredHeads + 1) / 2);

            for (var gateIndex = 0; gateIndex < gateCount; gateIndex++)
            {
                for (var layer = 0; layer < ArchipelagoConstants.ShopLayers; layer++)
                {
                    var shopIndex = gateIndex * ArchipelagoConstants.ShopLayers + layer;
                    long locationId = 0x200 + shopIndex * 4 + (int)__instance.upgradeType;

                    if (UnfairFlipsAPMod.ArchipelagoHandler.IsLocationChecked(locationId))
                        continue;

                    var hoverHandler = __instance.GetComponent<ShopButtonHoverHandler>();

                    if (!scoutedLocations.ContainsKey(locationId))
                    {
                        var shouldHint = !UnfairFlipsAPMod.SaveDataHandler.SaveData.HintedLocationIds.Contains(locationId);
                        var info = UnfairFlipsAPMod.ArchipelagoHandler.TryScoutLocation(locationId, shouldHint);
                        scoutedLocations[locationId] = info;

                        if (shouldHint)
                        {
                            UnfairFlipsAPMod.SaveDataHandler.SaveData.HintedLocationIds.Add(locationId);
                            UnfairFlipsAPMod.SaveDataHandler.SaveGame();
                        }
                        
                        if (Costs.ContainsKey(__instance))
                            Costs[__instance] = GetCost(gateIndex);
                        else
                            Costs.TryAdd(__instance, GetCost(gateIndex));

                        var itemDisplayName = info.ItemDisplayName;
                        var itemNameLength = itemDisplayName.Length;
                        if (itemNameLength > 23)
                            itemDisplayName = itemDisplayName[..23] + "...";
                        __instance.text.text =
                            $"{itemDisplayName}\n{Mathy.CentsToDollarString(Costs[__instance])}";
                    }

                    if (hoverHandler != null && scoutedLocations.TryGetValue(locationId, out var scoutInfo))
                    {
                        var ap = UnfairFlipsAPMod.ArchipelagoHandler;
                        var itemName = scoutInfo.ItemDisplayName;
                        var playerName = ap.GetPlayerName(scoutInfo.Player);
                        var gameName = scoutInfo.ItemGame;
                        var (rarityName, rarityColor) = GetRarityInfo(scoutInfo.Flags);
                        
                        hoverHandler.tooltipText = $"{PurchaseNames[__instance.upgradeType]} {shopIndex + 1}\nItem: {itemName}\nfor {playerName} in {gameName}\nTier: <color={rarityColor}>{rarityName}</color>";
                    }
                    
                    __instance.text.overflowMode = TMPro.TextOverflowModes.Truncate;

                    __instance.button.interactable =
                        UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney >= Costs[__instance];

                    currentLocationForButton[__instance] = locationId;
                    return false;
                }
            }

            currentLocationForButton.Remove(__instance);
            __instance.gameObject.SetActive(false);
            return false;
        }

        private static (string name, string color) GetRarityInfo(ItemFlags flags)
        {
            if (flags.HasFlag(ItemFlags.Advancement))
                return ("Progression", "#AF99EF"); // Plum/Purple
            if (flags.HasFlag(ItemFlags.Trap))
                return ("Trap", "#EE0000"); // Red
            if (flags.HasFlag(ItemFlags.NeverExclude))
                return ("Useful", "#6495ED"); // Blue
            
            return ("Filler", "#FFFFFF"); // White
        }
        
        [HarmonyPatch("Buy")]
        [HarmonyPrefix]
        public static bool Buy_Prefix(ShopButton __instance)
        {
            if (!currentLocationForButton.TryGetValue(__instance, out var locationId))
                return false;
            UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney -= Costs[__instance];
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