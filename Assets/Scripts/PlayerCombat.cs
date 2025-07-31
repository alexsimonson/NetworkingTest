using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerCombat : NetworkBehaviour
{
    public GameObject slashVFXPrefab;  // Assign in the inspector
    
    private string[] debugWeaponsArray = new string[10];
    private int weaponIndex;

    public float meleeRange = 2f;
    public float meleeRadius = 1f;
    public int meleeDamage = 10;
    public LayerMask hitLayers;

    void Awake(){
        hitLayers = LayerMask.GetMask("Attackable");
    }

    void Start()
    {
        debugWeaponsArray[1] = "melee";
        debugWeaponsArray[2] = "range";
        debugWeaponsArray[3] = "magic";
    }

    void Update()
    {
        if (!IsOwner) return;

        // Weapon selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) weaponIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) weaponIndex = 3;

        // Left-click attack (only if melee weapon is selected)
        if (Input.GetMouseButtonDown(0) && debugWeaponsArray[weaponIndex] == "melee")
        {
            Debug.Log("Should perform melee attack");
            PerformMeleeAttackServerRpc();
        }
    }

    [ServerRpc]
    void PerformMeleeAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        StartCoroutine(SlashArcCoroutine());
        PerformSlashVisualClientRpc();  // show effect on all clients
    }

    [ClientRpc]
    void PerformSlashVisualClientRpc()
    {
        if (slashVFXPrefab)
        {
            Debug.Log("Instantiating VFX");
            GameObject vfx = Instantiate(slashVFXPrefab, transform.position, transform.rotation);
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            else
            {
                Debug.LogWarning("No ParticleSystem found on VFX prefab.");
            }
            // vfx.transform.localScale = Vector3.one * 10000;
            // Debug.DrawRay(transform.position, Vector3.forward * 0.5f, Color.green, 5f);

            Destroy(vfx, 2f);  // Destroy after duration
        }
    }

    IEnumerator SlashArcCoroutine()
    {
        // Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        int rayCount = 15;
        float slashArc = 120f;
        float delayBetweenRays = 0.01f; // seconds
        float maxDistance = meleeRange;

        Quaternion baseRotation = Quaternion.LookRotation(forward);

        bool alreadyDealtDamage = false;

        for (int i = 0; i < rayCount; i++)
        {
            float lerp = (float)i / (rayCount - 1);
            float angle = Mathf.Lerp(slashArc / 2f, -slashArc / 2f, lerp); // right to left
            Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
            Vector3 rayDirection = rotation * Vector3.forward;

            // Debug draw
            Debug.DrawRay(origin, rayDirection * maxDistance, Color.red, 1f);

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, maxDistance, hitLayers))
            {
                if (hit.collider.TryGetComponent<Stats>(out var damageable))
                {
                    if(!alreadyDealtDamage){
                        damageable.TakeDamage(meleeDamage);
                        alreadyDealtDamage = true;
                    }
                }
            }

            yield return new WaitForSeconds(delayBetweenRays);
        }
    }
}