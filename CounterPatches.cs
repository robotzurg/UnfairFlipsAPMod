using Archipelago.MultiClient.Net.Colors;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace UnfairFlipsAPMod;

public class CounterPatches : MonoBehaviour
{
    private GameObject moneyCapObject;
    private static TextMeshProUGUI moneyCapText;
    private GameObject fairnessAmtObject;
    private static TextMeshProUGUI fairnessAmtText;
    
    public void Start()
    {
        var moneyCounter = FindObjectOfType<MoneyCounter>();
        var moneySource = moneyCounter.GetComponent<TMP_Text>();
        var moneyParent = moneyCounter.transform.parent;
        var headsChanceCounter = FindObjectOfType<HeadsChanceCounter>();
        var headsSource = headsChanceCounter.GetComponent<TMP_Text>();
        var headsParent = headsChanceCounter.transform.parent;
        var actualRes = Screen.height;
        var refRes = 1080;
        moneyCapObject = new GameObject("MoneyCap");
        moneyCapObject.transform.SetParent(moneyParent.transform, false);
        moneyCapObject.transform.position = moneyCounter.gameObject.transform.position;
        moneyCapObject.transform.localPosition = moneyCounter.gameObject.transform.localPosition;
        moneyCapText = moneyCapObject.AddComponent<TextMeshProUGUI>();
        moneyCapText.font = moneySource.font;
        moneyCapText.fontSize = moneySource.fontSize;
        moneyCapText.alignment = TextAlignmentOptions.Center;
        moneyCapText.enableAutoSizing = true;
        moneyCapText.richText = true;
        moneyCapText.color = Color.white;
        moneyCapObject.transform.Translate(0, -50f * actualRes / refRes, 0);
        
        fairnessAmtObject = new GameObject("FairnessAmt");
        fairnessAmtObject.transform.SetParent(headsParent.transform, false);
        fairnessAmtObject.transform.position = headsChanceCounter.gameObject.transform.position;
        fairnessAmtObject.transform.localPosition = headsChanceCounter.gameObject.transform.localPosition;
        fairnessAmtText = fairnessAmtObject.AddComponent<TextMeshProUGUI>();
        fairnessAmtText.font = headsSource.font;
        fairnessAmtText.fontSize = headsSource.fontSize;
        fairnessAmtText.alignment = TextAlignmentOptions.Center;
        fairnessAmtText.enableAutoSizing = true;
        fairnessAmtText.overflowMode = TextOverflowModes.Overflow;
        fairnessAmtText.richText = true;
        fairnessAmtText.color = Color.white;
        fairnessAmtObject.transform.Translate(0, -50f * actualRes / refRes, 0);
    }
    
    [HarmonyPatch(typeof(HeadsChanceCounter))]
    public class HeadsChanceCounter_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(HeadsChanceCounter __instance)
        {
            if (UnfairFlipsAPMod.SaveDataHandler?.SaveData != null)
            {
                var headsChance = 1 + (UnfairFlipsAPMod.SaveDataHandler.SaveData.Fairness * 2);
                __instance.text.text = $"HEADS CHANCE: {((int) (UnfairFlipsAPMod.SaveDataHandler.SaveData.HeadsChance * 100.0)).ToString()}%";
                fairnessAmtText.text = $"Max Heads Chain: {headsChance:D0}";
                if (Mathf.Approximately(headsChance, UnfairFlipsAPMod.SaveDataHandler.SaveData.HeadsChance))
                    fairnessAmtText.color = new Color(0.56f, 0.93f, 0.56f); // Light green
            }
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
