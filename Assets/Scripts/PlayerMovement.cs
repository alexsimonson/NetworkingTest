using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2f;

    public float speed = 5f;
    public float rotationSpeed = 150f;
    public float verticalClampAngle = 85f;

    private Vector3 movementInput;
    private float yawInput;
    private float pitchInput;
    private float cameraPitch = 0f;

    private Transform playerCamera;

    void Start()
    {
        if (IsOwner)
        {
            playerCamera = transform.Find("Camera");
            if (playerCamera == null)
                Debug.LogError("Player Camera child not found! Make sure the Main Camera is a child of the player.");
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!IsOwner || playerCamera == null) return;

        // Get movement input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        movementInput = new Vector3(h, 0, v);

        // Get mouse input
        yawInput = Input.GetAxis("Mouse X") * mouseSensitivity;
        pitchInput = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Apply pitch locally for camera
        cameraPitch += pitchInput;
        cameraPitch = Mathf.Clamp(cameraPitch, -verticalClampAngle, verticalClampAngle);
        playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        // Send movement and yaw to server
        if (movementInput.sqrMagnitude > 0 || Mathf.Abs(yawInput) > 0f)
        {
            SendInputToServerServerRpc(movementInput, yawInput);
        }
    }

    [ServerRpc]
    void SendInputToServerServerRpc(Vector3 moveInput, float yawDelta, ServerRpcParams rpcParams = default)
    {
        // Move the player on the server
        Vector3 move = moveInput * speed * Time.deltaTime;
        transform.Translate(move, Space.Self);

        // Rotate the player (yaw) on the server
        float rotationAmount = yawDelta * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount);
    }
}
