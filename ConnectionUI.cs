using UnityEngine;

namespace UnfairFlipsAPMod
{
    public class ConnectionUI : MonoBehaviour
    {
        private bool showUI = true;
#if DEBUG
        private string hostname = "localhost";
        private string slotName = "Jeff-UF";
#elif RELEASE
        private string hostname = "archipelago.gg";
        private string slotName = "";
#endif
        private string port = "38281";
        private string password = "";
        private string statusMessage = "";
        private Rect windowRect = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 250, 800, 700);

        private ArchipelagoClient apClient;

        public void Initialize(ArchipelagoClient client)
        {
            apClient = client;
            apClient.OnConnected += () => statusMessage = "Connected successfully!";
            apClient.OnConnectionFailed += (error) => statusMessage = $"Failed: {error}";
            apClient.OnDisconnected += () => statusMessage = "Disconnected";

            // When connected, persist the connection info so it can be used as default next time
            apClient.OnConnected += () =>
            {
                var fw = GameObject.FindObjectOfType<FileWriter>();
                if (fw != null)
                {
                    int.TryParse(port, out int p);
                    fw.WriteLastConnection(hostname, p, slotName, password);
                }
            };

            // Try to prefill fields from last saved connection
            var last = FileWriter.ReadLastConnection();
            if (!string.IsNullOrEmpty(last.host))
            {
                hostname = last.host;
            }
            if (!string.IsNullOrEmpty(last.port))
            {
                port = last.port;
            }
            if (!string.IsNullOrEmpty(last.slotName))
            {
                slotName = last.slotName;
            }
            if (!string.IsNullOrEmpty(last.password))
            {
                password = last.password;
            }
        }

        public void ToggleUI()
        {
            showUI = !showUI;
        }

        private void Update()
        {
            // Toggle UI with F1 key
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleUI();
            }
        }

        private void OnGUI()
        {
            if (showUI)
            {
                // Scale up the GUI
                GUI.skin.label.fontSize = 24;
                GUI.skin.button.fontSize = 24;
                GUI.skin.textField.fontSize = 24;

                windowRect = GUI.Window(0, windowRect, DrawWindow, "Archipelago Connection");
            }
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Press F1 to toggle this menu");
            GUILayout.Space(15);

            GUILayout.Label("Hostname:");
            hostname = GUILayout.TextField(hostname, GUILayout.Height(40));
            GUILayout.Space(10);

            GUILayout.Label("Port:");
            port = GUILayout.TextField(port, GUILayout.Height(40));
            GUILayout.Space(10);

            GUILayout.Label("Slot Name:");
            slotName = GUILayout.TextField(slotName, GUILayout.Height(40));
            GUILayout.Space(10);

            GUILayout.Label("Password (optional):");
            password = GUILayout.PasswordField(password, '*', GUILayout.Height(40));
            GUILayout.Space(15);

            if (apClient != null && apClient.IsConnected)
            {
                if (GUILayout.Button("Disconnect", GUILayout.Height(40)))
                {
                    apClient.Disconnect();
                }
            }
            else
            {
                if (GUILayout.Button("Connect", GUILayout.Height(40)))
                {
                    if (string.IsNullOrEmpty(slotName))
                    {
                        statusMessage = "Please enter a slot name!";
                    }
                    else if (int.TryParse(port, out int portNum))
                    {
                        statusMessage = "Connecting...";
                        apClient.Connect(hostname, portNum, slotName, password);
                    }
                    else
                    {
                        statusMessage = "Invalid port number!";
                    }
                }
            }

            GUILayout.Space(15);

            if (!string.IsNullOrEmpty(statusMessage))
            {
                GUILayout.Label($"Status: {statusMessage}");
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}