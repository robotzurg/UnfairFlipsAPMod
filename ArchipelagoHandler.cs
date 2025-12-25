using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Archipelago.MultiClient.Net.Colors;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = System.Random;

namespace UnfairFlipsAPMod 
{
    public class ArchipelagoHandler : MonoBehaviour
    {
        private ArchipelagoSession Session { get; set; }
        private string Server { get; set; }
        private int Port { get; set; }
        private string Slot { get; set; }
        private string Password { get; set; }
        private string seed;
        public bool IsConnected => Session?.Socket.Connected ?? false;
        public event Action OnConnected;
        public event Action<string> OnConnectionFailed;
        public event Action OnDisconnected;
        private readonly ConcurrentQueue<long> locationsToCheck = new();
        private string lastDeath;
        private DateTime lastDeathLinkTime = DateTime.Now;
        private readonly Random random = new();
        
        private readonly string[] deathMessages =
        [
            "had a skill issue (died)",
            "RNGesus has smitten you on this day (died)",
        ];
        
        private static string GetColorHex(PaletteColor? color)
        {
            return color switch
            {
                PaletteColor.Red => "#EE0000",
                PaletteColor.Green => "#00FF7F",
                PaletteColor.Yellow => "#FAFAD2",
                PaletteColor.Blue => "#6495ED",
                PaletteColor.Magenta => "#EE00EE",
                PaletteColor.Cyan => "#00EEEE",
                PaletteColor.Black => "#000000",
                PaletteColor.White => "#FFFFFF",
                PaletteColor.SlateBlue => "#6D8BE8",
                PaletteColor.Salmon => "#FA8072",
                PaletteColor.Plum => "#AF99EF",
                _ => "#FFFFFF" // Default to white
            };
        }
        
        public void CreateSession(string server, int port, string slot, string password)
        {
            Server = server;
            Port = port;
            Slot = slot;
            Password = password;
            Session = ArchipelagoSessionFactory.CreateSession(Server, Port);
            Session.MessageLog.OnMessageReceived += OnMessageReceived;
            Session.Socket.ErrorReceived += OnError;
            Session.Socket.SocketClosed += OnSocketClosed;
            Session.Socket.PacketReceived += PacketReceived;
        }
        
        public void Connect()
        {
            Log.Message($"Logging in to {Server}:{Port} as {Slot}...");
            seed = Session.ConnectAsync()?.Result?.SeedName;
            
            var result = Session.LoginAsync(
                "Unfair Flips",
                Slot,
                ItemsHandlingFlags.AllItems,
                new Version(0, 6, 5),
                [],
                password: Password
            ).Result;

            if (result.Successful)
            {
                Log.Message($"Success! Connected to {Server}:{Port}");
                var successResult = (LoginSuccessful)result;
                UnfairFlipsAPMod.SlotData = new SlotData(successResult.SlotData);
                
                if (seed != null)
                    UnfairFlipsAPMod.SaveDataHandler!.GetSaveGame(seed, Slot);
                
                Log.Message("Subscribing to item events...");
                Session.Items.ItemReceived += ItemReceived;
            
                FindObjectOfType<PanelManager>().SetPanelArrangement(2);
                UnfairFlipsAPMod.GameHandler.InitOnConnect();
                StartCoroutine(RunCheckQueue());
                OnConnected?.Invoke();
                return;
            }

            var failure = (LoginFailure)result;
            var errorMessage = $"Failed to Connect to {Server}:{Port} as {Slot}:";
            errorMessage = failure.Errors.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            errorMessage = failure.ErrorCodes.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            OnConnectionFailed?.Invoke(errorMessage);
            Log.Error(errorMessage);
            Log.Info("Attempting reconnect...");
        }

        public void Disconnect()
        {
            if (Session == null)
                return;
            StopAllCoroutines();
            Session.Socket.DisconnectAsync();
            Session = null;
            Log.Message("Disconnected from Archipelago");
        }
        
        private void OnError(Exception ex, string message)
        {
            Log.Error($"Socket error: {message} - {ex.Message}");
        }

        private void OnSocketClosed(string reason)
        {
            StopAllCoroutines();
            Log.Warning($"Socket closed: {reason}");
            OnDisconnected?.Invoke();
        }

        private void ItemReceived(ReceivedItemsHelper helper)
        {
            try
            {
                while (helper.Any())
                {
                    var itemIndex = helper.Index;
                    var item = helper.DequeueItem();
                    UnfairFlipsAPMod.ItemHandler.HandleItem(itemIndex, item);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ItemReceived Error: {ex}");
                throw;
            }
        }

        public void ResyncItems()
        {
            if (Session == null || !IsConnected)
            {
                Log.Warning("Cannot resync items: Not connected to Archipelago");
                return;
            }

            Log.Message("Resyncing items from server...");
            var items = Session.Items.AllItemsReceived;
            for (int i = 0; i < items.Count; i++)
            {
                UnfairFlipsAPMod.ItemHandler.HandleItem(i, items[i], false);
            }
            UnfairFlipsAPMod.SaveDataHandler.SaveGame();
            Log.Message($"Resync complete. Processed up to item {items.Count}");
        }

        public void Release()
        {
            Session.SetGoalAchieved();
            Session.SetClientState(ArchipelagoClientState.ClientGoal);
        }
        
        public void CheckLocations(long[] ids)
        {
            ids.ToList().ForEach(id => locationsToCheck.Enqueue(id));
        }
        
        public void CheckLocation(long id)
        {
            locationsToCheck.Enqueue(id);
        }
        
        private IEnumerator RunCheckQueue()
        {
            while (true)
            {
                if (locationsToCheck.TryDequeue(out var locationId))
                {
                    Session.Locations.CompleteLocationChecks(locationId);
                    Log.Message($"Sent location check: {locationId}");
                } 
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        public bool IsLocationChecked(long id)
        {
            return Session.Locations.AllLocationsChecked.Contains(id);
        }

        public int CountLocationsCheckedInRange(long start, long end)
        {
            var startId = start;
            var endId = end;
            return Session.Locations.AllLocationsChecked.Count(loc => loc >= startId && loc < endId);
        }
        
        public void UpdateTags(List<string> tags)
        {
            var packet = new ConnectUpdatePacket
            {
                Tags = tags.ToArray(),
                ItemsHandling = ItemsHandlingFlags.AllItems
            };
            Session.Socket.SendPacket(packet);
        }
        
        private void OnMessageReceived(LogMessage message)
        {
            string messageStr;
            if (message.Parts.Length == 1)
            {
                messageStr = message.Parts[0].Text;
            }
            else
            {
                var builder = new StringBuilder();
                foreach (var part in message.Parts)
                {
                    string hexColor = GetColorHex(part.PaletteColor);
                    builder.Append($"<color={hexColor}>{part.Text}</color>");
                }
                messageStr = builder.ToString();
            }
            AddMessageToGameLog(messageStr);
        }

        private void AddMessageToGameLog(string message)
        {
            StartCoroutine(AddMessageToGameLogRoutine(message));
        }
        
        private MessageManager MessageManager => FindObjectOfType<MessageManager>();
        private IEnumerator AddMessageToGameLogRoutine(string message)
        {
            yield return null;
            MessageManager?.ShowMessage(message);
        }
        
        private void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case BouncePacket bouncePacket:
                    BouncePacketReceived(bouncePacket);
                    break;
            }
        }
        
        public void SendDeath()
        {
            var packet = new BouncePacket();
            var now = DateTime.Now;

            if (now - lastDeathLinkTime < TimeSpan.FromSeconds(2))
                return;
            
            packet.Tags = ["DeathLink"];
            packet.Data = new Dictionary<string, JToken>
            {
                { "time", now.ToUnixTimeStamp() },
                { "source", Slot },
                { "cause", $"{Slot} {deathMessages[random.Next(deathMessages.Length)]}" }
            };

            if (packet.Data.TryGetValue("source", out var sourceObj))
            {
                var source = sourceObj?.ToString() ?? "Unknown";
                if (packet.Data.TryGetValue("cause", out var causeObj))
                {
                    var cause = causeObj?.ToString() ?? "Unknown";
                    if (packet.Data.TryGetValue("time", out var timeObj))
                    {
                        var time = timeObj?.ToString() ?? "Unknown";
                    }
                }
            }
            
            lastDeathLinkTime = now;
            Session.Socket.SendPacket(packet);
        }

        private void BouncePacketReceived(BouncePacket packet)
        {
            if (UnfairFlipsAPMod.SlotData.DeathLink)
                ProcessBouncePacket(packet, "DeathLink", ref lastDeath, (source, data) =>
                    HandleDeathLink(source, data["cause"]?.ToString() ?? "Unknown"));
        }

        private static void ProcessBouncePacket(BouncePacket packet, string tag, ref string lastTime,
            Action<string, Dictionary<string, JToken>> handler)
        {
            if (!packet.Tags.Contains(tag)) return;
            if (!packet.Data.TryGetValue("time", out var timeObj))
                return;
            if (lastTime == timeObj.ToString())
                return;
            lastTime = timeObj.ToString();
            if (!packet.Data.TryGetValue("source", out var sourceObj))
                return;
            var source = sourceObj?.ToString() ?? "Unknown";
            if (packet.Data.TryGetValue("cause", out var causeObj))
            {
                var cause = causeObj?.ToString() ?? "Unknown";
                //Console.WriteLine($"Received Bounce Packet with Tag: {tag} :: {cause}");
            }

            handler(source, packet.Data);
        }
        
        private void HandleDeathLink(string source, string cause)
        {
            if (!UnfairFlipsAPMod.SlotData.DeathLink)
                return;
            AddMessageToGameLog(cause);
            if (source == Slot)
                return;
            UnfairFlipsAPMod.GameHandler.Kill();
        }

        public ScoutedItemInfo TryScoutLocation(long locationId)
        {
            return Session.Locations.ScoutLocationsAsync(locationId)?.Result?.Values.First();
        }
    }
}