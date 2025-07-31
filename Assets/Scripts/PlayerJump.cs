using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : NetworkBehaviour
{
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundMask;
    private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = transform.Find("GroundCheck");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            // Local jump, and send to server for consistency
            PerformJumpServerRpc();
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    [ServerRpc]
    private void PerformJumpServerRpc(ServerRpcParams rpcParams = default)
    {
        // Zero vertical velocity first for consistent jump height
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
