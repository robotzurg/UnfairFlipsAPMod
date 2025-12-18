using HarmonyLib;
using UnityEngine;

namespace UnfairFlipsAPMod;

public class LoginHandler : MonoBehaviour
{
    private bool isMenuCreated;
    private GameObject menuObject;
    private ConnectionUI connectionUI;
    
    public void CreateUI(ArchipelagoHandler archipelagoHandler)
    {
        if (isMenuCreated) return;

        Log.Message("Creating Archipelago UI...");
        menuObject = new GameObject("ArchipelagoUI");
        DontDestroyOnLoad(menuObject);

        connectionUI = menuObject.AddComponent<ConnectionUI>();
        connectionUI.Initialize(archipelagoHandler);

        archipelagoHandler.OnConnected += () => SetCoinEnabled(true);
        archipelagoHandler.OnDisconnected += () => SetCoinEnabled(false);

        isMenuCreated = true;
    }

    public void ToggleUI()
    {
        if (!isMenuCreated || connectionUI == null) 
            return;
        connectionUI.ToggleUI();
    }

    public void DestroyMenu()
    {
        SetCoinEnabled(false);
        if (menuObject != null)
            Destroy(menuObject);
    }

    public void SetCoinEnabled(bool enabled)
    {
        var flipButton = GameObject.Find("Canvas/RATIO CANVAS/Coin Panel/FlipButton");
        if (flipButton != null)
            flipButton.SetActive(enabled);
    }
    
    [HarmonyPatch(typeof(PanelManager))]
    private class PanelManager_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start_Postfix(PanelManager __instance)
        {
            __instance.coinPanel.Find("FlipButton").gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(LogoFade))]
    private class LogoFade_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(LogoFade __instance)
        {
            if (!UnfairFlipsAPMod.ArchipelagoHandler.IsConnected)
                return false;
            __instance.img.color = Mathy.Decay(__instance.img.color, new Color(__instance.img.color.r, __instance.img.color.g, __instance.img.color.b, 0.0f), 8f, Time.deltaTime);
            if (__instance.img.color.a >= 0.05f)
                return false;
            Destroy(__instance.gameObject, 0.2f);
            return false;
        }
    }
}