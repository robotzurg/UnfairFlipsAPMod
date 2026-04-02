using HarmonyLib;
using TMPro;
using UnityEngine;

namespace UnfairFlipsAPMod;

public class CounterPatches : MonoBehaviour
{
    private static TextMeshProUGUI headsChanceText;
    private static TextMeshProUGUI fairnessAmtText;

    public void Start()
    {
        var moneyCounter = FindObjectOfType<MoneyCounter>();
        var refText = moneyCounter.GetComponent<TMP_Text>();
        var coinPanel = FindObjectOfType<PanelManager>().coinPanel;
        var actualRes = Screen.height;
        var refRes = 1080;

        var headsChanceObject = new GameObject("HeadsChanceAmt");
        headsChanceObject.transform.SetParent(coinPanel, false);
        headsChanceObject.transform.localPosition = moneyCounter.transform.localPosition - new Vector3(0, actualRes / refRes, 0);
        headsChanceText = headsChanceObject.AddComponent<TextMeshProUGUI>();
        headsChanceText.font = refText.font;
        headsChanceText.fontSize = refText.fontSize * 1.5f;
        headsChanceText.alignment = TextAlignmentOptions.Center;
        headsChanceText.enableAutoSizing = true;
        headsChanceText.fontSizeMax = refText.fontSize;
        headsChanceText.overflowMode = TextOverflowModes.Overflow;
        headsChanceText.richText = true;
        headsChanceText.color = Color.white;
        headsChanceText.rectTransform.anchorMin = new Vector2(0, headsChanceText.rectTransform.anchorMin.y);
        headsChanceText.rectTransform.anchorMax = new Vector2(1, headsChanceText.rectTransform.anchorMax.y);

        var fairnessAmtObject = new GameObject("FairnessAmt");
        fairnessAmtObject.transform.SetParent(coinPanel, false);
        fairnessAmtObject.transform.localPosition = headsChanceObject.transform.localPosition - new Vector3(0, actualRes / refRes * 25, 0);
        fairnessAmtText = fairnessAmtObject.AddComponent<TextMeshProUGUI>();
        fairnessAmtText.font = refText.font;
        fairnessAmtText.fontSize = refText.fontSize;
        fairnessAmtText.alignment = TextAlignmentOptions.Top;
        fairnessAmtText.enableAutoSizing = true;
        fairnessAmtText.fontSizeMax = 48;
        fairnessAmtText.overflowMode = TextOverflowModes.Overflow;
        fairnessAmtText.richText = true;
        fairnessAmtText.color = Color.white;
        fairnessAmtText.rectTransform.anchorMin = new Vector2(0, fairnessAmtText.rectTransform.anchorMin.y);
        fairnessAmtText.rectTransform.anchorMax = new Vector2(1, fairnessAmtText.rectTransform.anchorMax.y);
    }

    public void Update()
    {
        if (UnfairFlipsAPMod.SaveDataHandler?.SaveData == null) return;
        var headsChance = 1 + (UnfairFlipsAPMod.SaveDataHandler.SaveData.Fairness * 2);
        headsChanceText.text = $"HEADS CHANCE: {((int)(UnfairFlipsAPMod.SaveDataHandler.SaveData.HeadsChance * 100.0))}%";
        fairnessAmtText.text = $"Max Heads In A Row: {headsChance:D0}";
        if (Mathf.Approximately(headsChance, UnfairFlipsAPMod.SaveDataHandler.SaveData.HeadsChance))
            fairnessAmtText.color = new Color(0.56f, 0.93f, 0.56f);
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
            }
            return false;
        }
    }
}
