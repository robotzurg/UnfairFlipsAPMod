using System.Collections.Generic;

namespace UnfairFlipsAPMod;

public class SlotData
{
    public readonly int RequiredHeads;
    public readonly int StartingHeadsChance;
    public readonly bool DeathLink;
    // public readonly bool EnergyLink;
    public readonly int DeathLinkChance;
    public readonly int DeathLinkMinStreak;
    public readonly int HeadsUpgradeCount;
    public readonly int FlipTimeUpgradeCount;
    public readonly int ComboUpgradeCount;
    public readonly int AutoFlipUpgradeCount;
    public readonly int FlipDifficulty;
    public readonly bool AutoFlipEnabled;

    public SlotData(Dictionary<string, object> slotDict)
    {
        if (slotDict.TryGetValue("RequiredHeads", out var requiredHeads))
            RequiredHeads = (int)(long)requiredHeads;
        if (slotDict.TryGetValue("StartingHeadsChance", out var startingHeadsChance))
            StartingHeadsChance = (int)(long)startingHeadsChance;
        if (slotDict.TryGetValue("DeathLink", out var deathLink))
            DeathLink = (int)(long)deathLink == 1;
        // if (slotDict.TryGetValue("EnergyLink", out var energyLink))
        //     EnergyLink = (int)(long)energyLink == 1;
        if (slotDict.TryGetValue("DeathLinkChance", out var deathLinkChance))
            DeathLinkChance = (int)(long)deathLinkChance;
        if (slotDict.TryGetValue("DeathLinkMinStreak", out var deathLinkMinStreak))
            DeathLinkMinStreak = (int)(long)deathLinkMinStreak;
        if (slotDict.TryGetValue("HeadsUpgradeCount", out var headsUpgradeCount))
            HeadsUpgradeCount = (int)(long)headsUpgradeCount;
        if (slotDict.TryGetValue("FlipSpeedUpgradeCount", out var flipTimeUpgradeCount))
            FlipTimeUpgradeCount = (int)(long)flipTimeUpgradeCount;
        if (slotDict.TryGetValue("ComboUpgradeCount", out var comboUpgradeCount))
            ComboUpgradeCount = (int)(long)comboUpgradeCount;
        if (slotDict.TryGetValue("AutoFlipUpgradeCount", out var autoFlipUpgradeCount))
            AutoFlipUpgradeCount = (int)(long)autoFlipUpgradeCount;
        if (slotDict.TryGetValue("FlipDifficulty", out var flipDifficulty))
            FlipDifficulty = (int)(long)flipDifficulty;
        if (slotDict.TryGetValue("AutoFlipEnabled", out var autoFlipEnabled))
            AutoFlipEnabled = (int)(long)autoFlipEnabled == 1;
        if (DeathLink)
            UnfairFlipsAPMod.ArchipelagoHandler.UpdateTags(["DeathLink"]);
        UnfairFlipsAPMod.ItemHandler.FlushQueue();
    }
}