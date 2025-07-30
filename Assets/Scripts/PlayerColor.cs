using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    // Use Vector4 because Color isn’t supported natively
    public NetworkVariable<Vector4> playerColor = new NetworkVariable<Vector4>(
        new Vector4(1, 1, 1, 1), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private Renderer rend;

    private void Awake()
    {
        // Get Renderer from self or children
        rend = GetComponentInChildren<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to changes to update material color on all clients
        playerColor.OnValueChanged += OnColorChanged;

        // Apply current color on spawn immediately
        OnColorChanged(default, playerColor.Value);
    }

    private void OnColorChanged(Vector4 oldValue, Vector4 newValue)
    {
        if (rend != null)
        {
            rend.material.color = new Color(newValue.x, newValue.y, newValue.z, newValue.w);
        }
    }

    // Client calls this to request a color change on their player
    public void RequestColorChange(Color newColor)
    {
        if (IsOwner)
        {
            ChangeColorServerRpc(new Vector4(newColor.r, newColor.g, newColor.b, newColor.a));
        }
    }

    // Server receives client request and updates the NetworkVariable
    [ServerRpc]
    private void ChangeColorServerRpc(Vector4 newColor, ServerRpcParams rpcParams = default)
    {
        playerColor.Value = newColor;
    }
}
