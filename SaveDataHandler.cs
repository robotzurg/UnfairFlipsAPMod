using System;
using System.IO;
using System.Numerics;
using System.Runtime;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UnfairFlipsAPMod;

public class CustomSaveData
{
    public int ItemIndex;
    public int Fairness;
    public float HeadsChance;
    public float FlipTime;
    public float AutoFlipAddition;
    public float ComboMult;
    public int CoinValue;
    public int CoinUpgradeLevel;
    public int HeadsStreak;
    public int FlipCount;
    public bool HasAutoFlip;

    [SerializeField]
    private string playerMoney = "0";

    public BigInteger PlayerMoney
    {
        get => BigInteger.Parse(playerMoney);
        set
        {
            var maxMoney = new BigInteger(Math.Pow(10, Fairness));
            playerMoney = value > maxMoney ? maxMoney.ToString() : value.ToString();
        }
    }
    
    public int QueuedTailsTraps;
    public int QueuedPennyTraps;
    public int QueuedTaxTraps;
    public int QueuedSlowTraps;
}

public class SaveDataHandler
{
    private string folderName;
    private string fileName;
    public CustomSaveData SaveData;
    public ShopButton[] ShopButtons;
    
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
        Log.Debug("Game loaded!");
    }

    private void CreateNewGame()
    {
        Log.Debug("Creating new game...");
        SaveData = new CustomSaveData();
        SaveData.Fairness = 0;
        SaveData.HeadsChance = (float)UnfairFlipsAPMod.SlotData.StartingHeadsChance / 100;
        SaveData.FlipTime = ArchipelagoConstants.MaxFlipTime;
        SaveData.ComboMult = ArchipelagoConstants.MinComboMultiplier;
        SaveData.CoinValue = 1;
        SaveData.CoinUpgradeLevel = 0;
        SaveData.AutoFlipAddition = ArchipelagoConstants.MaxAutoFlipAddition;
        SaveGame();
    }

    public void ResetGame() => SceneManager.LoadScene("Game");

    public void SaveGame()
    {
        Log.Debug("Starting save...");
        var objectOfType = Object.FindObjectOfType<CoinFlip>();
        SaveData.HeadsStreak = objectOfType.GetHeadsStreak();
        SaveData.FlipCount = objectOfType.GetNumFlips();
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
