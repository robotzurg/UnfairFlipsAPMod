using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace UnfairFlipsAPMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class UnfairFlipsAPMod : BaseUnityPlugin
    {
        private const string PluginGuid = "UnfairFlipsAPMod";
        private const string PluginName = "Unfair Flips Archipelago Mod";
        private const string PluginVersion = "1.4.1";
        public static string PluginDir;
        private readonly Harmony harmony = new(PluginGuid);
        public static ConfigEntry<bool>? FilterLog;
        public static ConfigEntry<float>? MessageInTime;
        public static ConfigEntry<float>? MessageHoldTime;
        public static ConfigEntry<float>? MessageOutTime;
        public static SlotData SlotData;
        public static ArchipelagoHandler ArchipelagoHandler { get; private set; }
        private static LoginHandler LoginHandler { get; set; }
        public static GameHandler GameHandler { get; private set; }
        public static ItemHandler ItemHandler { get; private set; }
        public static SaveDataHandler SaveDataHandler { get; private set; }

        public void Awake()
        {
            PluginDir = Path.GetDirectoryName(Info.Location);
            Application.runInBackground = true;
            harmony.PatchAll();
            Log.Init(Logger);
            var originalSaveManager = FindObjectOfType<MetaManager>();
            Destroy(originalSaveManager);
            gameObject.AddComponent<FileWriter>();
            ArchipelagoHandler = gameObject.AddComponent<ArchipelagoHandler>();
            LoginHandler = gameObject.AddComponent<LoginHandler>();
            LoginHandler.CreateUI(ArchipelagoHandler);
            GameHandler = gameObject.AddComponent<GameHandler>();
            ItemHandler = gameObject.AddComponent<ItemHandler>();
            SaveDataHandler = new SaveDataHandler();
            gameObject.AddComponent<CounterPatches>();
            
            ArchipelagoHandler.OnConnected += () =>
            {
                Log.Message("Connected to Archipelago - loading items");
                LoginHandler.ToggleUI();
            };
            
            ArchipelagoHandler.OnDisconnected += () =>
            {
                Log.Message("Disconnected from Archipelago");
                LoginHandler.ToggleUI();
            };
            
            FilterLog = Config.Bind(
                "Logging",
                "FilterLog",
                false,
                "Filter the archipelago log to only show messages relevant to you."
            );
        }
        
        public void OnDestroy()
        {
            ArchipelagoHandler?.Disconnect();
            LoginHandler?.DestroyMenu();
            harmony?.UnpatchSelf();
        }
    }
}