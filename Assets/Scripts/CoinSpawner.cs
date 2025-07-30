using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject coinPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCoin(new Vector3(0, 0, 0));
            SpawnCoin(new Vector3(2, 0, 0));
            SpawnCoin(new Vector3(-2, 0, 0));
        }
    }

    private void SpawnCoin(Vector3 position)
    {
        var coin = Instantiate(coinPrefab, position, Quaternion.identity);
        coin.GetComponent<NetworkObject>().Spawn();
    }
}
