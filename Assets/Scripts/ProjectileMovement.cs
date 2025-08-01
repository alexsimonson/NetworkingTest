using UnityEngine;
using Unity.Netcode;

public class ProjectileMovement : NetworkBehaviour
{

    public float projectileSpeed = 100f;
    public float projectileLife = 10f;  // seconds

    void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroySelf), projectileLife);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * projectileSpeed;
    }

    void DestroySelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
