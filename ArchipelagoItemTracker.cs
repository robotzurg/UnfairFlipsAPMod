using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnfairFlipsAPMod.Helpers;

namespace UnfairFlipsAPMod
{
    public class ArchipelagoItemTracker
    {
        // Thread-safe collections so background socket callbacks can't corrupt state
        private static ConcurrentDictionary<long, int> receivedItems = new ConcurrentDictionary<long, int>();
        private static ConcurrentDictionary<long, byte> checkedLocations = new ConcurrentDictionary<long, byte>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        // ========== ITEM METHODS ==========

        public static void AddReceivedItem(long itemId)
        {
            receivedItems.AddOrUpdate(itemId, 1, (_, existing) => existing + 1);
        }

        public static bool HasItem(long itemId)
        {
            return receivedItems.ContainsKey(itemId);
        }

        public static int AmountOfItem(long itemId)
        {
            return receivedItems.TryGetValue(itemId, out var count) ? count : 0;
        }

        // ========== LOCATION METHODS ==========

        public static void AddCheckedLocation(long locationId)
        {
            checkedLocations.TryAdd(locationId, 0);
        }

        public static bool HasLocation(long locationId)
        {
            return checkedLocations.ContainsKey(locationId);
        }

        public static int GetCheckedLocationCount()
        {
            return checkedLocations.Count;
        }

        // ========== LOAD/CLEAR METHODS ==========

        public static void Clear()
        {
            receivedItems.Clear();
            checkedLocations.Clear();
            Log.Message("[AP] Cleared all received items and checked locations");
        }

        // Make LoadFromServer authoritative: clear local state then populate with server state.
        // Safe to call from background threads because collections are concurrent.
        public static void LoadFromServer()
        {
            try
            {
                var session = UnfairFlipsAPMod.APClient.GetSession();
                if (session == null)
                    return;

                Clear();

                // Load items
                var itemsList = session.Items.AllItemsReceived?.ToList();
                if (itemsList != null)
                {
                    Log.Message($"[AP] Loading {itemsList.Count} items from server");
                    foreach (var item in itemsList)
                    {
                        Log.Message($"[AP] Item: {item.ItemName} (ID: {item.ItemId})");
                        receivedItems.AddOrUpdate(item.ItemId, 1, (_, existing) => existing + 1);
                    }
                }

                // Load locations
                List<long> locationsList = session.Locations.AllLocationsChecked?.ToList();
                if (locationsList != null)
                {
                    Log.Message($"[AP] Loading {locationsList.Count} checked locations from server");
                    foreach (var locationId in locationsList)
                    {
                        Log.Message($"[AP] Location checked: {locationId}");
                        checkedLocations.TryAdd(locationId, 0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AP] LoadFromServer exception: {ex.ToString()}");
            }
        }

        // Convenience: force a resync from the current session (call on reconnect)
        public static void ResyncFromServer()
        {
            Log.Message("[AP] Resyncing Archipelago state from server");
            LoadFromServer();
        }

        // ========== HELPER METHODS ==========
        


        // ========== DEBUG METHODS ==========

        public static void LogAllReceivedItems()
        {
            var total = receivedItems.Sum(kv => kv.Value);
            Log.Message($"[AP Debug] === All Received Items ({total} total entries) ===");
            foreach (var kv in receivedItems.OrderBy(kv => kv.Key))
            {
                Log.Message($"[AP Debug] Item ID: {kv.Key} Count: {kv.Value}");
            }
        }

        public static void LogAllCheckedLocations()
        {
            Log.Message($"[AP Debug] === All Checked Locations ({checkedLocations.Count} total) ===");
            foreach (var locationId in checkedLocations.Keys.OrderBy(x => x))
            {
                Log.Message($"[AP Debug] Location ID: {locationId}");
            }
        }
    }
}