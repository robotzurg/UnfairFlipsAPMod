using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;

namespace UnfairFlipsAPMod
{
    public class ArchipelagoClient
    {
        private ArchipelagoSession session;
        
        public bool IsConnected => session?.Socket.Connected ?? false;

        public event Action OnConnected;
        public event Action<string> OnConnectionFailed;
        public event Action OnDisconnected;

        public void Connect(string hostname, int port, string slotName, string password = "")
        {
            try
            {
                Log.Message($"Attempting to connect to {hostname}:{port} as {slotName}");

                session = ArchipelagoSessionFactory.CreateSession(hostname, port);

                session.Socket.ErrorReceived += OnError;
                session.Socket.SocketClosed += OnSocketClosed;

                LoginResult result = session.TryConnectAndLogin(
                    "Unfair Flips",
                    slotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(0, 6, 4),
                    password: string.IsNullOrEmpty(password) ? null : password
                );

                if (result.Successful)
                {
                    LoginSuccessful loginSuccess = (LoginSuccessful)result;
                    UnfairFlipsAPMod.sessionSlotData = loginSuccess.SlotData;
                    Log.Message($"Connected successfully! Slot: {loginSuccess.Slot}");

                    // Load items we already have
                    ArchipelagoItemTracker.LoadFromServer();

                    OnConnected?.Invoke();

                    ArchipelagoItemTracker.LogAllReceivedItems();
                    ArchipelagoItemTracker.LogAllCheckedLocations();
                }
                else
                {
                    LoginFailure failure = (LoginFailure)result;
                    string errorMsg = string.Join(", ", failure.Errors);
                    Log.Error($"Connection failed: {errorMsg}");
                    OnConnectionFailed?.Invoke(errorMsg);
                    session = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Connection exception: {ex.ToString()}");
                OnConnectionFailed?.Invoke(ex.Message);
                session = null;
            }
        }

        public ArchipelagoSession GetSession()
        {
            return session;
        }

        public void Disconnect()
        {
            if (session != null)
            {
                session.Socket.DisconnectAsync();
                session = null;
                Log.Message("Disconnected from Archipelago");
            }
        }

        //private void OnItemReceived(ReceivedItemsHelper helper)
        //{
        //    var item = helper.PeekItem();

        //    string itemName = session.Items.GetItemName(item.ItemId);
        //    string playerName = session.Players.GetPlayerName(item.Player);

        //    Log.Message($"Item Received: {itemName} from {playerName}");

        //    ArchipelagoItemTracker.AddReceivedItem(item.ItemId);

        //    pendingNotifications.Enqueue((item.ItemId, item.Player, itemName, playerName));

        //    helper.DequeueItem();
        //}

        private void OnError(Exception ex, string message)
        {
            Log.Error($"Socket error: {message} - {ex.Message}");
        }

        private void OnSocketClosed(string reason)
        {
            Log.Warning($"Socket closed: {reason}");
            OnDisconnected?.Invoke();
        }

        public void SendLocation(long locationId)
        {
            if (IsConnected)
            {
                ArchipelagoItemTracker.AddCheckedLocation(locationId);
                session.Locations.CompleteLocationChecks(locationId);
                Log.Message($"Sent location check: {locationId}");
            }
        }

        public string GetSlotDataValue(string key)
        {
            Dictionary<string, object> defaultSlotData = new() { };
            if (session != null && UnfairFlipsAPMod.sessionSlotData != null)
            {
                if (UnfairFlipsAPMod.sessionSlotData.ContainsKey(key))
            {
                    return UnfairFlipsAPMod.sessionSlotData[key].ToString();
                }
                else if (defaultSlotData.ContainsKey(key)) 
                {
                    return defaultSlotData[key].ToString();
                }
                else
                {
                    throw new SlotDataException($"Invalid option requested from apworld: {key}. Did you generate on the wrong version?");
            }
            }

            // Client is not connected
            return null;
        }
    }
}