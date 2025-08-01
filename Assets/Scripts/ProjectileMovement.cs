using UnityEngine;
using Unity.Netcode;

public class ProjectileMovement : NetworkBehaviour
{

    public float projectileSpeed = 100f;
    public float projectileLife = 10f;  // seconds
    [SerializeField] private int projectileDamage = 50;
    public GameObject owner;

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

    void OnTriggerEnter(Collider other){
        Debug.Log("projectile trigger enter: " + other.name);
        if(other.gameObject==owner) return;
        if(other.TryGetComponent<Stats>(out var damageable)){
            damageable.TakeDamage(projectileDamage);
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
