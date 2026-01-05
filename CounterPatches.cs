using HarmonyLib;
using TMPro;
using UnityEngine;

namespace UnfairFlipsAPMod;

public class CounterPatches : MonoBehaviour
{
    private GameObject moneyCapObject;
    private static TextMeshProUGUI moneyCapText;
    
    public void Start()
    {
        var moneyCounter = FindObjectOfType<MoneyCounter>();
        var source = moneyCounter.GetComponent<TMP_Text>();
        var parent = moneyCounter.transform.parent;
        var actualRes = Screen.height;
        var refRes = 1080;
        moneyCapObject = new GameObject("MoneyCap");
        moneyCapObject.transform.SetParent(parent.transform, false);
        moneyCapObject.transform.position = moneyCounter.gameObject.transform.position;
        moneyCapObject.transform.localPosition = moneyCounter.gameObject.transform.localPosition;
        moneyCapText = moneyCapObject.AddComponent<TextMeshProUGUI>();
        moneyCapText.font = source.font;
        moneyCapText.fontSize = source.fontSize;
        moneyCapText.alignment = TextAlignmentOptions.Center;
        moneyCapText.enableAutoSizing = true;
        moneyCapText.richText = true;
        moneyCapText.color = Color.white;
        moneyCapObject.transform.Translate(0, -50f * actualRes / refRes, 0);
    }
    
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
            {
                __instance.text.text = Mathy.CentsToDollarString(UnfairFlipsAPMod.SaveDataHandler.SaveData.PlayerMoney);
                moneyCapText.text = "Max Money: " + Mathy.CentsToDollarString(UnfairFlipsAPMod.SaveDataHandler.SaveData.MaxMoney);
            }
            return false;
        }
    }
}
