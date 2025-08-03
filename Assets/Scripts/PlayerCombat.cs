using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerCombat : NetworkBehaviour
{
    public GameObject slashVFXPrefab;
    public GameObject fireballPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;

    private Transform projectileSpawnPoint;
    private string[] debugWeaponsArray = new string[10];
    private int weaponIndex;

    public float meleeRange = 2f;
    public float meleeRadius = 1f;
    public int meleeDamage = 10;

    public float gunRange = 50f;
    public int gunDamage = 35;
    public LayerMask hitLayers;

    public GameObject playerCam;

    void Awake()
    {
        hitLayers = LayerMask.GetMask("Attackable");
        projectileSpawnPoint = transform.Find("ProjectileSpawn");
        playerCam = transform.Find("Camera").gameObject;
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) weaponIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) weaponIndex = 3;

        if (Input.GetMouseButtonDown(0))
        {
            switch (debugWeaponsArray[weaponIndex])
            {
                case "melee":
                    PerformMeleeAttackServerRpc();
                    break;
                case "range":
                    PerformGunAttackServerRpc(playerCam.transform.forward.normalized);
                    break;
                case "magic":
                    PerformMagicAttackServerRpc(Quaternion.LookRotation(playerCam.transform.forward.normalized));
                    break;
            }
        }
    }

    [ServerRpc]
    void PerformMagicAttackServerRpc(Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        var fireball = Instantiate(fireballPrefab, projectileSpawnPoint.position, rotation);
        fireball.GetComponent<ProjectileMovement>().owner = gameObject;
        fireball.GetComponent<NetworkObject>().Spawn();
    }


    [ServerRpc]
    void PerformGunAttackServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        Vector3 origin = projectileSpawnPoint.position;

        if (muzzleFlashPrefab)
            SpawnEffectClientRpc(origin, direction, muzzleFlashPrefab.name);

        ShowRayClientRpc(origin, direction, gunRange);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, gunRange, hitLayers))
        {
            if (hitEffectPrefab)
                SpawnEffectClientRpc(hit.point, hit.normal, hitEffectPrefab.name);

            if (hit.collider.TryGetComponent<Stats>(out var damageable))
            {
                damageable.TakeDamage(gunDamage);
            }
        }
    }


    [ClientRpc]
    void ShowRayClientRpc(Vector3 origin, Vector3 direction, float distance)
    {
        GameObject lineObj = new GameObject("DebugRay");
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + direction * distance);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = Color.red;
        lr.startWidth = lr.endWidth = 0.05f;
        Destroy(lineObj, 1f);
    }


    [ClientRpc]
    void SpawnEffectClientRpc(Vector3 pos, Vector3 normal, string effectName)
    {
        var prefab = Resources.Load<GameObject>(effectName);
        if (!prefab) return;

        var effect = Instantiate(prefab, pos, Quaternion.LookRotation(normal));
        Destroy(effect, 2f);
    }

    [ServerRpc]
    void PerformMeleeAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        StartCoroutine(SlashArcCoroutine());
        PerformSlashVisualClientRpc();
    }

    [ClientRpc]
    void PerformSlashVisualClientRpc()
    {
        if (slashVFXPrefab)
        {
            GameObject vfx = Instantiate(slashVFXPrefab, transform.position, transform.rotation);
            if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
            Destroy(vfx, 2f);
        }
    }

    IEnumerator SlashArcCoroutine()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        int rayCount = 15;
        float slashArc = 120f;
        float delayBetweenRays = 0.01f;
        float maxDistance = meleeRange;

        Quaternion baseRotation = Quaternion.LookRotation(forward);
        bool alreadyDealtDamage = false;

        for (int i = 0; i < rayCount; i++)
        {
            float lerp = (float)i / (rayCount - 1);
            float angle = Mathf.Lerp(slashArc / 2f, -slashArc / 2f, lerp);
            Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
            Vector3 rayDirection = rotation * Vector3.forward;

            Debug.DrawRay(origin, rayDirection * maxDistance, Color.red, 1f);

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, maxDistance, hitLayers))
            {
                if (hit.collider.TryGetComponent<Stats>(out var damageable))
                {
                    if (!alreadyDealtDamage)
                    {
                        damageable.TakeDamage(meleeDamage);
                        alreadyDealtDamage = true;
                    }
                }
            }

            yield return new WaitForSeconds(delayBetweenRays);
        }
    }
}
