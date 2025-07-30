using UnityEngine;
using Unity.Netcode;

public class SimpleNetworkUI : MonoBehaviour
{
    private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan };

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }
        else
        {
            GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label(NetworkManager.Singleton.IsHost ? "Mode: Host" :
                            NetworkManager.Singleton.IsServer ? "Mode: Server" :
                            "Mode: Client");

            GUILayout.Label("Select Player Color:");

            for (int i = 0; i < colors.Length; i++)
            {
                GUI.backgroundColor = colors[i];
                if (GUILayout.Button(colors[i].ToString()))
                {
                    SetPlayerColor(colors[i]);
                }
            }

            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndArea();
    }

    void SetPlayerColor(Color newColor)
    {
        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            var colorScript = localPlayer.GetComponent<PlayerColor>();
            if (colorScript != null)
            {
                Debug.Log($"Requesting color change to {newColor} from client {NetworkManager.Singleton.LocalClientId}");
                colorScript.RequestColorChange(newColor);
            }
        }
    }

}
