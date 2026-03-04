using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    private async void Start()
    {
        await InitializeAuthentication();
    }

    private async Task InitializeAuthentication()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Authentication successful. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
            else
            {
                Debug.Log($"Already signed in. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Authentication error: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Request failed: {ex.Message}");
        }
    }
}