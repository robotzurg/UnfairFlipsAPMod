using HarmonyLib;

namespace UnfairFlipsAPMod;

public class CounterPatches
{
    [HarmonyPatch(typeof(HeadsChanceCounter))]
    public class HeadsChanceCounter_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(HeadsChanceCounter __instance)
        {
            if (UnfairFlipsAPMod.SaveDataHandler?.SaveData != null)
                __instance.text.text = $"HEADS CHANCE: {((int) (UnfairFlipsAPMod.SaveDataHandler.SaveData.HeadsChance * 100.0)).ToString()}%";
            return false;
        }
    }
    
    [HarmonyPatch(typeof(MoneyCounter))]
    public class MoneyCounter_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(MoneyCounter __instance)
        {
            if (UnfairFlipsAPMod.SaveDataHandler?.SaveData != null)
                __instance.text.text = Mathy.CentsToDollarString(UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney);
            return false;
        }
    }
}