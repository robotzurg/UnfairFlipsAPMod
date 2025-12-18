using System.Collections.Generic;

namespace UnfairFlipsAPMod;

public class SlotData
{
    public readonly int RequiredHeads;
    public readonly int StartingHeadsChance;
    public readonly bool DeathLink;
    public readonly int DeathLinkChance;
    public readonly int DeathLinkMinStreak;
    public readonly int HeadsUpgradeCount;
    public readonly int FlipTimeUpgradeCount;
    public readonly int ComboUpgradeCount;
    public readonly int ValueUpgradeCount;

    public SlotData(Dictionary<string, object> slotDict)
    {
        RequiredHeads =        (int)(long)slotDict["RequiredHeads"];
        StartingHeadsChance =  (int)(long)slotDict["StartingHeadsChance"];
        DeathLink =            (int)(long)slotDict["DeathLink"] == 1;
        DeathLinkChance =      (int)(long)slotDict["DeathLinkChance"];
        DeathLinkMinStreak =   (int)(long)slotDict["DeathLinkMinStreak"];
        HeadsUpgradeCount =    (int)(long)slotDict["HeadsUpgradeCount"];
        FlipTimeUpgradeCount = (int)(long)slotDict["FlipSpeedUpgradeCount"];
        ComboUpgradeCount =    (int)(long)slotDict["ComboUpgradeCount"];
        if (DeathLink)
            UnfairFlipsAPMod.ArchipelagoHandler.UpdateTags(["DeathLink"]);
    }
}