using UnityEngine;
using Unity.Netcode;

public class ProjectileMovement : NetworkBehaviour
{
    LayerMask groundLayer;
    public float projectileSpeed = 100f;
    public float projectileLife = 10f;  // seconds
    [SerializeField] private int projectileDamage = 50;
    public GameObject owner;

    void Awake(){
        groundLayer = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * projectileSpeed;
    }

    void OnTriggerEnter(Collider other){
        Debug.Log("projectile trigger enter: " + other.name);
        if(other.gameObject==owner) return;
        if(other.TryGetComponent<Stats>(out var damageable)){
            damageable.TakeDamage(projectileDamage);
            RequestDespawnServerRpc();
        }
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("Hit Ground/Wall");
            RequestDespawnServerRpc();
        }
    }

    [ServerRpc]
    void RequestDespawnServerRpc(){
        GetComponent<NetworkObject>().Despawn();
    }
}
