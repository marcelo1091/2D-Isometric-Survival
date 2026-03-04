using UnityEngine;
using Mirror;
using Utp;

public class Connect : MonoBehaviour
{
    private RelayNetworkManager relayNetworkManager;

    private void Awake()
    {
        relayNetworkManager = FindFirstObjectByType<RelayNetworkManager>();
    }

    public void Host()
    {
        if (relayNetworkManager == null)
        {
            Debug.LogError("RelayNetworkManager not found!");
            return;
        }

        relayNetworkManager.StartRelayHost(2, null);
    }

    public void Client()
    {
        if (relayNetworkManager == null)
        {
            Debug.LogError("RelayNetworkManager not found!");
            return;
        }

        relayNetworkManager.JoinRelayServer();
    }
}