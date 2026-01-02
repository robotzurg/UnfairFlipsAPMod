using BepInEx;
using HarmonyLib;

namespace UnfairFlipsAPMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class UnfairFlipsAPMod : BaseUnityPlugin
    {
        private const string PluginGuid = "UnfairFlipsAPMod";
        private const string PluginName = "Unfair Flips Archipelago Mod";
        private const string PluginVersion = "1.0.3";
        private readonly Harmony harmony = new(PluginGuid);
        public static SlotData SlotData;
        public static ArchipelagoHandler ArchipelagoHandler { get; private set; }
        private static LoginHandler LoginHandler { get; set; }
        public static GameHandler GameHandler { get; private set; }
        public static ItemHandler ItemHandler { get; private set; }
        public static SaveDataHandler SaveDataHandler { get; private set; }

        public void Awake()
        {
            harmony.PatchAll();
            Log.Init(Logger);

            var originalSaveManager = FindObjectOfType<MetaManager>();
            Destroy(originalSaveManager);
            gameObject.AddComponent<FileWriter>();
            ArchipelagoHandler = gameObject.AddComponent<ArchipelagoHandler>();
            LoginHandler = gameObject.AddComponent<LoginHandler>();
            LoginHandler.CreateUI(ArchipelagoHandler);
            GameHandler = gameObject.AddComponent<GameHandler>();
            ItemHandler = new ItemHandler();
            SaveDataHandler = new SaveDataHandler();
            
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
        }
        
        public void OnDestroy()
        {
            ArchipelagoHandler?.Disconnect();
            LoginHandler?.DestroyMenu();
            harmony?.UnpatchSelf();
        }
    }
}