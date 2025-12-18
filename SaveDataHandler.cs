using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UnfairFlipsAPMod;

public class CustomSaveData
{
    public int ItemIndex { get; set; }
    public int Fairness { get; set; }
    public float HeadsChance { get; set; }
    public float FlipTime { get; set; }
    public float ComboMult { get; set; }
    public int CoinValue { get; set; }
    public int HeadsStreak { get; set; }
    public int FlipCount { get; set; }
    public long PlayerMoney { get; set; }
    public int QueuedTailsTraps { get; set; }
    public int QueuedPennyTraps { get; set; }
    public int QueuedTaxTraps { get; set; }
    public int QueuedSlowTraps { get; set; }
}

public class SaveDataHandler
{
    private string folderName;
    private string fileName;
    public CustomSaveData SaveData;
    public ShopButton[] ShopButtons;
    private bool gameWiped;
    
    public void GetSaveGame(string seed, string slot)
    {
        if (SaveData != null)
            return;
        UnfairFlipsAPMod.ArchipelagoHandler.OnDisconnected += () =>
        {
            SaveGame();
            SaveData = null;
        };
        folderName = "./ArchipelagoSaves";
        fileName = folderName + $"/{slot}{seed}.coin";
        if (File.Exists(fileName))
            LoadGame();
        else 
            CreateNewGame();
    }
    
    private void LoadGame()
    {
        Log.Debug("Loading game...");
        var json = File.ReadAllText(fileName);
        try
        {
            SaveData = JsonUtility.FromJson<CustomSaveData>(json);
        }
        catch
        {
            Log.Debug("First save file is corrupted!! Starting new game...");
            CreateNewGame();
        }
        var objectOfType = Object.FindObjectOfType<CoinFlip>();
        objectOfType.SetHeadsStreak(SaveData.HeadsStreak);
        objectOfType.SetNumFlips(SaveData.FlipCount);
        Object.FindObjectOfType<PlayerMoney>().moneyInCents = SaveData.PlayerMoney;
        // TODO Add handling for archipelago data
        Log.Debug("Game loaded!");
    }

    private void CreateNewGame()
    {
        Log.Debug("Creating new game...");
        SaveData = new CustomSaveData();
        SaveGame();
    }
    
    public void WipeGame()
    {
        Log.Debug("WIPING GAME");
        gameWiped = true;
        SaveData.HeadsStreak = 0;
        SaveData.FlipCount = 0;
        SaveData.PlayerMoney = 0L;
        FinishSavingGame();
    }

    public void ResetGame() => SceneManager.LoadScene("Game");

    public void SaveGame()
    {
        Log.Debug("Starting save...");
        if (gameWiped)
            return;
        var objectOfType = Object.FindObjectOfType<CoinFlip>();
        SaveData.HeadsStreak = objectOfType.GetHeadsStreak();
        SaveData.FlipCount = objectOfType.GetNumFlips();
        SaveData.PlayerMoney = Object.FindObjectOfType<PlayerMoney>().moneyInCents;
        FinishSavingGame();
    }

    private void FinishSavingGame()
    {
        Log.Debug("Saving game...");
        Directory.CreateDirectory(folderName);
        Log.Debug("Creating save file...");
        using var text = File.CreateText(fileName);
        text.Write(JsonUtility.ToJson(SaveData));
        text.Close();
    }
    
    [HarmonyPatch(typeof(QuitButton))]
    public class QuitButton_Patch
    {
        [HarmonyPatch("Click")]
        [HarmonyPrefix]
        public static bool Click_Prefix(QuitButton __instance)
        {
            if (Object.FindObjectOfType<PanelManager>() != null && Object.FindObjectOfType<PanelManager>().GetCurrentArrangement() == 0)
                Object.FindObjectOfType<PanelManager>().SetPanelArrangement(1);
            if (!__instance.quitPrimed)
            {
                Object.FindObjectOfType<MessageManager>().ShowMessage("<color=#ffbbbb>CLICK AGAIN TO QUIT GAME. PROGRESS IS SAVED AUTOMATICALLY</color>");
                __instance.quitPrimed = true;
                UnfairFlipsAPMod.SaveDataHandler.SaveGame();
            }
            else
            {
                UnfairFlipsAPMod.SaveDataHandler.SaveGame();
                Application.Quit();
            }
            return false;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_Prefix(QuitButton __instance) { return false; }
    }
}
