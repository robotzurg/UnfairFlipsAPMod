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
        RequiredHeads =        (int)slotDict["RequiredHeads"];
        StartingHeadsChance =  (int)slotDict["StartingHeadsChance"];
        DeathLink =            (int)slotDict["DeathLink"] == 1;
        DeathLinkChance =      (int)slotDict["DeathLinkChance"];
        DeathLinkMinStreak =   (int)slotDict["DeathLinkMinStreak"];
        HeadsUpgradeCount =    (int)slotDict["HeadsUpgradeCount"];
        FlipTimeUpgradeCount = (int)slotDict["FlipSpeedUpgradeCount"];
        ComboUpgradeCount =    (int)slotDict["ComboUpgradeCount"];
        ValueUpgradeCount =    (int)slotDict["ValueUpgradeCount"];
        if (DeathLink)
            UnfairFlipsAPMod.ArchipelagoHandler.UpdateTags(["DeathLink"]);
    }
}