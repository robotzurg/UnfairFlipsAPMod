using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using UnityEngine;
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
    private Queue<(int, ItemInfo)> cachedItems = new Queue<(int, ItemInfo)>();

    private bool IsGameReady()
    {
        // Check if all required components are initialized
        if (UnfairFlipsAPMod.SaveDataHandler?.SaveData == null)
        {
            Log.Debug("SaveData not ready");
            return false;
        }
        
        if (UnfairFlipsAPMod.SlotData == null)
        {
            Log.Debug("SlotData not ready");
            return false;
        }
        
        var coinFlip = Object.FindObjectOfType<CoinFlip>();
        if (coinFlip == null)
        {
            Log.Debug("CoinFlip not found in scene");
            return false;
        }
        
        return true;
    }
    
    public void HandleItem(int index, ItemInfo item, bool save = true)
    {
        try
        {
            // If game isn't ready, cache the item for later
            if (!IsGameReady())
            {
                Log.Debug($"Game not ready, caching item: {item.ItemName} (index {index})");
                cachedItems.Enqueue((index, item));
                return;
            }
            
            // Process any cached items first
            if (cachedItems.Count > 0)
            {
                Log.Message($"Processing {cachedItems.Count} cached items...");
                FlushQueue();
            }
            
            // Now process the current item
            ProcessItem(index, item, save);
        }
        catch (Exception ex)
        {
            Log.Error($"HandleItem Error: {ex}");
            // Don't rethrow - prevents cascading errors
        }
    }
    
    public void FlushQueue()
    {
        if (!IsGameReady())
        {
            Log.Warning("Attempted to flush queue but game is not ready");
            return;
        }
        
        int processedCount = 0;
        while (cachedItems.Count > 0)
        {
            var (index, item) = cachedItems.Dequeue();
            ProcessItem(index, item, false);
            processedCount++;
        }
        
        if (processedCount > 0)
        {
            Log.Message($"Flushed {processedCount} cached items");
            UnfairFlipsAPMod.SaveDataHandler.SaveGame();
        }
    }
    
    private void ProcessItem(int index, ItemInfo item, bool save = true)
    {
        var saveData = UnfairFlipsAPMod.SaveDataHandler.SaveData;
        var slotData = UnfairFlipsAPMod.SlotData;
        
        if (index < saveData.ItemIndex)
        {
            Log.Debug($"Item {index} already processed (current: {saveData.ItemIndex})");
            return;
        }
        
        saveData.ItemIndex++;
        
        switch ((UFItem)item.ItemId)
        {
            case UFItem.ProgressiveFairness:
                saveData.Fairness++;
                Log.Debug($"Received Fairness upgrade! Level: {saveData.Fairness}");
                break;
                
            case UFItem.HeadsUp:
                var startingHeadsChance = slotData.StartingHeadsChance / 100f;
                var maxTotalIncrease = ArchipelagoConstants.MaxHeadsChance - startingHeadsChance;
                var increasePerUpgrade = maxTotalIncrease / slotData.HeadsUpgradeCount;
                var newValue = saveData.HeadsChance + increasePerUpgrade;
                saveData.HeadsChance = Math.Min(ArchipelagoConstants.MaxHeadsChance, newValue);
                Log.Debug($"Received Heads Up! Chance: {saveData.HeadsChance:P1}");
                break;
                
            case UFItem.FlipUp:
                var maxTotalDecrease = ArchipelagoConstants.MaxFlipTime - ArchipelagoConstants.MinFlipTime;
                var decreasePerUpgrade = maxTotalDecrease / slotData.FlipTimeUpgradeCount;
                newValue = saveData.FlipTime - decreasePerUpgrade;
                saveData.FlipTime = Math.Max(ArchipelagoConstants.MinFlipTime, newValue);
                Log.Debug($"Received Flip Up! Time: {saveData.FlipTime:F2}s");
                break;
                
            case UFItem.ComboUp:
                maxTotalIncrease = ArchipelagoConstants.MaxComboMultiplier - ArchipelagoConstants.MinComboMultiplier;
                increasePerUpgrade = maxTotalIncrease / slotData.ComboUpgradeCount;
                newValue = saveData.ComboMult + increasePerUpgrade;
                saveData.ComboMult = Math.Min(ArchipelagoConstants.MaxComboMultiplier, newValue);
                Log.Debug($"Received Combo Up! Multiplier: {saveData.ComboMult:F2}x");
                break;
                
            case UFItem.CoinUp:
                saveData.CoinUpgradeLevel++;
                UnfairFlipsAPMod.GameHandler?.UpdateCoinValue();
                Log.Debug($"Received Coin Up! Level: {saveData.CoinUpgradeLevel}");
                break;
                
            case UFItem.AutoFlipUp:
                saveData.HasAutoFlip = true;
                maxTotalDecrease = ArchipelagoConstants.MaxAutoFlipAddition - ArchipelagoConstants.MinAutoFlipAddition;
                decreasePerUpgrade = maxTotalDecrease / slotData.AutoFlipUpgradeCount;
                newValue = saveData.AutoFlipAddition - decreasePerUpgrade;
                saveData.AutoFlipAddition = Math.Max(ArchipelagoConstants.MinAutoFlipAddition, newValue);
                Log.Debug($"Received Auto Flip Up! Addition: {saveData.AutoFlipAddition:F2}");
                break;
                
            case UFItem.TailsTrap:
                saveData.QueuedTailsTraps++;
                Log.Debug($"Received Tails Trap! Queued: {saveData.QueuedTailsTraps}");
                break;
                
            case UFItem.PennyTrap:
                saveData.QueuedPennyTraps++;
                Log.Debug($"Received Penny Trap! Queued: {saveData.QueuedPennyTraps}");
                break;
                
            case UFItem.TaxTrap:
                saveData.QueuedTaxTraps++;
                Log.Debug($"Received Tax Trap! Queued: {saveData.QueuedTaxTraps}");
                break;
                
            case UFItem.SlowTrap:
                saveData.QueuedSlowTraps++;
                Log.Debug($"Received Slow Trap! Queued: {saveData.QueuedSlowTraps}");
                break;
                
            case UFItem.Money:
                saveData.PlayerMoney += 1 * saveData.CoinValue;
                Log.Debug($"Received Money! Total: {saveData.PlayerMoney}");
                break;
                
            case UFItem.MoreMoney:
                saveData.PlayerMoney += 10 * saveData.CoinValue;
                Log.Debug($"Received More Money! Total: {saveData.PlayerMoney}");
                break;
                
            case UFItem.BigMoney:
                saveData.PlayerMoney += 100 * saveData.CoinValue;
                Log.Debug($"Received Big Money! Total: {saveData.PlayerMoney}");
                break;
                
            default:
                Log.Warning($"Unknown item: {item.ItemId} ({item.ItemName})");
                break;
        }
        
        if (save)
            UnfairFlipsAPMod.SaveDataHandler.SaveGame();
    }
    
    
}