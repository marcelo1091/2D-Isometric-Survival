using Mirror;
using UnityEngine;

public class PlayerCameraSetter : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();

        if (cam != null)
        {
            cam.SetTarget(transform);
        }
    }
}