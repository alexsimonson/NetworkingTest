using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Stats : NetworkBehaviour
{
    public GameObject deathScreenPrefab;
    private GameObject deathUIInstance;

    public int maxHealth = 100;
    public int maxMana = 100;
    public int maxStamina = 100;

    private int curHealth;

    [SerializeField]
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        curHealth = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            isDead.OnValueChanged += OnDeathStateChanged;
        }
    }

    private void OnDeathStateChanged(bool oldVal, bool newVal)
    {
        if (!IsOwner) return;

        if (newVal)
            ShowDeathUI();
        else
            HideDeathUI();
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return; // Only server modifies health
        curHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining: {curHealth}");

        if (curHealth <= 0 && !isDead.Value)
        {
            isDead.Value = true;
        }
    }

    [ContextMenu("Simulate Death")]
    public void Kill()
    {
        Debug.Log("KILL SIMULATION");
        if (IsServer)
        {
            isDead.Value = true;
        }
        else
        {
            SubmitDeathServerRpc();
        }
    }

    [ServerRpc]
    void SubmitDeathServerRpc()
    {
        isDead.Value = true;
    }

    void ShowDeathUI()
    {
        if (!IsOwner || deathUIInstance != null || deathScreenPrefab == null)
            return;

        Debug.Log("SHOW DEATH UI");
        deathUIInstance = Instantiate(deathScreenPrefab, GameObject.Find("Canvas")?.transform);
        deathUIInstance.SetActive(true);

        Button respawnBtn = deathUIInstance.transform.Find("Panel/RespawnButton")?.GetComponent<Button>();
        Button quitBtn = deathUIInstance.transform.Find("Panel/QuitButton")?.GetComponent<Button>();

        if (respawnBtn != null)
        {
            respawnBtn.onClick.AddListener(() =>
            {
                RequestRespawnServerRpc();
            });
        }

        if (quitBtn != null)
        {
            quitBtn.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }

    void HideDeathUI()
    {
        if (!IsOwner) return;

        if (deathUIInstance != null)
        {
            Destroy(deathUIInstance);
            deathUIInstance = null;
        }
    }

    [ServerRpc]
    void RequestRespawnServerRpc()
    {
        isDead.Value = false;
        curHealth = maxHealth;

        // Example respawn point
        transform.position = Vector3.zero;

        HideDeathUIClientRpc(OwnerClientId); // Ensure only the owner hides their UI
    }

    [ClientRpc]
    void HideDeathUIClientRpc(ulong clientId)
    {
        if (!IsOwner || NetworkManager.Singleton.LocalClientId != clientId) return;
        HideDeathUI();
    }
}
