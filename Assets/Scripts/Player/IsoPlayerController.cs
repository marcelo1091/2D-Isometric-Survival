using Mirror;
using UnityEngine;

public class IsoPlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 9f;

    void Update()
    {
        // tylko lokalny gracz steruje tą postacią
        if (!isLocalPlayer) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }
}