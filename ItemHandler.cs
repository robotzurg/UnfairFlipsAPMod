using System;
using Archipelago.MultiClient.Net.Models;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace UnfairFlipsAPMod;

public enum UFItem
{
    ProgressiveFairness = 0x1,
    HeadsUp = 0x2,
    FlipUp = 0x3,
    ComboUp = 0x4,
    CoinUp = 0x5,
    AutoFlipUp = 0x6,
    
    TailsTrap = 0x10,
    PennyTrap = 0x11,
    TaxTrap = 0x12,
    SlowTrap = 0x13,
    
    Money = 0x20,
    MoreMoney = 0x21,
    BigMoney = 0x22,
}


public class ItemHandler
{
    public void HandleItem(int index, ItemInfo item)
    {
        try
        {
            var saveData = UnfairFlipsAPMod.SaveDataHandler.SaveData;
            var slotData = UnfairFlipsAPMod.SlotData;
            if (index < saveData.ItemIndex)
                return;
            saveData.ItemIndex++;
            switch ((UFItem)item.ItemId)
            {
                case UFItem.ProgressiveFairness:
                    saveData.Fairness++;
                    break;
                case UFItem.HeadsUp:
                    var startingHeadsChance = slotData.StartingHeadsChance / 100f;
                    var maxTotalIncrease = ArchipelagoConstants.MaxHeadsChance - startingHeadsChance;
                    var increasePerUpgrade = maxTotalIncrease / slotData.HeadsUpgradeCount;
                    var newValue = saveData.HeadsChance + increasePerUpgrade;
                    saveData.HeadsChance = Math.Min(ArchipelagoConstants.MaxHeadsChance, newValue); 
                    break;
                case UFItem.FlipUp:
                    var maxTotalDecrease = ArchipelagoConstants.MaxFlipTime - ArchipelagoConstants.MinFlipTime;
                    var decreasePerUpgrade = maxTotalDecrease / slotData.FlipTimeUpgradeCount;
                    newValue = saveData.FlipTime - decreasePerUpgrade;
                    saveData.FlipTime = Math.Max(ArchipelagoConstants.MinFlipTime, newValue); 
                    break;
                case UFItem.ComboUp:
                    maxTotalIncrease = ArchipelagoConstants.MaxComboMultiplier - ArchipelagoConstants.MinComboMultiplier;
                    increasePerUpgrade = maxTotalIncrease / slotData.ComboUpgradeCount;
                    newValue = saveData.ComboMult + increasePerUpgrade;
                    saveData.ComboMult = Math.Min(ArchipelagoConstants.MaxComboMultiplier, newValue); 
                    break;
                case UFItem.CoinUp:
                    UnfairFlipsAPMod.SaveDataHandler.SaveData.CoinUpgradeLevel++;
                    UnfairFlipsAPMod.GameHandler.UpdateCoinValue();
                    break;
                case UFItem.AutoFlipUp:
                    saveData.HasAutoFlip = true;
                    maxTotalDecrease = ArchipelagoConstants.MaxAutoFlipAddition - ArchipelagoConstants.MinAutoFlipAddition;
                    decreasePerUpgrade = maxTotalDecrease / slotData.AutoFlipUpgradeCount;
                    newValue = saveData.AutoFlipAddition - decreasePerUpgrade;
                    saveData.AutoFlipAddition = Math.Max(ArchipelagoConstants.MinAutoFlipAddition, newValue);
                    break;
                case UFItem.TailsTrap:
                    saveData.QueuedTailsTraps++;
                    break;
                case UFItem.PennyTrap:
                    saveData.QueuedPennyTraps++;
                    break;
                case UFItem.TaxTrap:
                    saveData.QueuedTaxTraps++;
                    break;
                case UFItem.SlowTrap:
                    saveData.QueuedSlowTraps++;
                    break;
                case UFItem.Money:
                    saveData.PlayerMoney += 1 * saveData.CoinValue;
                    break;
                case UFItem.MoreMoney:
                    saveData.PlayerMoney += 10 * saveData.CoinValue;
                    break;
                case UFItem.BigMoney:
                    saveData.PlayerMoney += 100 * saveData.CoinValue;
                    break;
                default:
                    break;
            }
            
            UnfairFlipsAPMod.SaveDataHandler.SaveGame();
        }
        catch (Exception ex)
        {
            Log.Error($"Handle Item Error: {ex}");
            throw;
        }
    }
}