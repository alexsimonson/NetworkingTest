using Unity.Netcode;
using UnityEngine;

public class PlayerCameraHandler : NetworkBehaviour
{
    private Camera playerCamera;
    private AudioListener audioListener;

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>(true);
        audioListener = playerCamera.GetComponent<AudioListener>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
            if (audioListener != null)
                audioListener.enabled = true;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            playerCamera.enabled = false;
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }
}
