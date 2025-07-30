using Unity.Netcode;
using UnityEngine;

public class CoinPickup : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var player = other.GetComponent<PlayerCoins>();
        if (player != null)
        {
            player.IncreaseCoinsServerRpc();
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
