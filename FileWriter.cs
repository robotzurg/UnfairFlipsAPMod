using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace UnfairFlipsAPMod
{
    public class FileWriter : MonoBehaviour
    {
        private const string LastConnectionFileName = "last_connection.txt";

        // Save the last used connection info to disk. Overwrites each time so it's the default on next run.
        public void WriteLastConnection(string host, int port, string slotName, string password)
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                var lines = new List<string>
                {
                    host ?? "",
                    port.ToString(),
                    slotName ?? "",
                    password ?? ""
                };
                File.WriteAllLines(path, lines);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write last connection info: {ex.Message}");
            }
        }

        public static (string host, string port, string slotName, string password) ReadLastConnection()
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                if (!File.Exists(path))
                    return (null, null, null, null);

                var lines = File.ReadAllLines(path);
                string host = lines.Length > 0 ? lines[0] : null;
                string port = lines.Length > 1 ? lines[1] : null;
                string slot = lines.Length > 2 ? lines[2] : null;
                string pass = lines.Length > 3 ? lines[3] : null;
                return (host, port, slot, pass);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read last connection info: {ex.Message}");
                return (null, null, null, null);
            }
        }
    }
}