using Unity.Netcode;
using UnityEngine;

public class PlayerCoins : NetworkBehaviour
{
    public NetworkVariable<int> coinCount = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            coinCount.OnValueChanged += (oldVal, newVal) =>
            {
                Debug.Log("Coins: " + newVal);
            };
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseCoinsServerRpc(ServerRpcParams rpcParams = default)
    {
        coinCount.Value++;
    }
}
