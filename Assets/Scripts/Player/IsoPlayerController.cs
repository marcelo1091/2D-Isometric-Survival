using Mirror;
using UnityEngine;

public class IsoPlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 9f;

    private bool isSinglePlayer = false;

    void Update()
    {
        bool canControl = isSinglePlayer || isLocalPlayer;
        
        if (!canControl) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Mark this player as the local player in single-player mode
    /// </summary>
    public void SetLocalPlayer()
    {
        isSinglePlayer = true;
    }
}