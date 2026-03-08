using Mirror;
using UnityEngine;

public class PlayerCameraSetter : NetworkBehaviour
{
    private bool isSinglePlayer = false;

    void Start()
    {
        if (isSinglePlayer)
        {
            SetCamera();
        }
    }

    public override void OnStartLocalPlayer()
    {
        SetCamera();
    }

    private void SetCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("Camera.main not found!");
            return;
        }

        CameraFollow cam = mainCam.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.SetTarget(transform);
        }
        else
        {
            Debug.LogWarning("CameraFollow component not found on Camera.main!");
        }
    }

    /// <summary>
    /// Mark this player as single-player
    /// </summary>
    public void SetAsSinglePlayer()
    {
        isSinglePlayer = true;
    }
}