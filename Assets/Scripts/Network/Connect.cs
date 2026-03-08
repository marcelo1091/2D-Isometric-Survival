using UnityEngine;
using Mirror;
using Utp;
using UnityEngine.SceneManagement;
using TMPro;

namespace Network
{   
    public enum ConnectionType
    {
        Host,
        Client,
        SinglePlayer
    }

    public class Connect : MonoBehaviour
    {
        public static Connect Instance { get; private set; }

        public string sceneName;
        public TMP_InputField joinCodeInput;
        public TMP_Text wrongJoinCodeText;
        private RelayNetworkManager relayNetworkManager;
        private ConnectionType connectionType;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnEmptyJoinCode;
        public UnityEngine.Events.UnityEvent OnWrongJoinCode;

        private bool showWrongCode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Ensure the correct UI is shown based on the current scene
            SwitchConnectUI();

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            relayNetworkManager = FindFirstObjectByType<RelayNetworkManager>();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            if(scene.name == "Loading")
            {
                switch (connectionType)
                {
                    case ConnectionType.Host:
                        relayNetworkManager.StartRelayHost(2, () =>
                        {
                            Debug.Log("Relay host started successfully.");
                            SceneManager.LoadScene(sceneName);
                        }, () =>
                        {
                            Debug.LogError("Failed to start Relay host.");
                        }, 
                        null);
                        break;
                    case ConnectionType.Client:
                        relayNetworkManager.JoinRelayServer(() =>
                        {
                            Debug.Log("Joined Relay server successfully.");
                            SceneManager.LoadScene(sceneName);
                        }, () =>
                        {
                            Debug.LogError("Failed to join Relay server.");
                            WrongCodeEntered();
                        });
                        break;
                    case ConnectionType.SinglePlayer:
                        relayNetworkManager.StartSingleplayer();
                        break;
                }
            }

            if(scene.name == sceneName)
            {
                switch (connectionType)
                {
                    case ConnectionType.Host:
                        SetupNewNetworkScene();
                        break;
                    case ConnectionType.Client:
                        SetupNewNetworkScene();
                        break;
                    case ConnectionType.SinglePlayer:
                        relayNetworkManager.StartSingleplayer();
                        break;
                }
            }
            else if(scene.name == "MainMenu")
            {
                if(showWrongCode)
                {
                    showWrongCode = false;
                    OnWrongJoinCode?.Invoke();
                }
            }

            // Ensure the correct UI is shown based on the current scene
            SwitchConnectUI();
            
        }

        public void Host()
        {
            if (relayNetworkManager == null)
            {
                Debug.LogError("RelayNetworkManager not found!");
                return;
            }

            connectionType = ConnectionType.Host;
            SceneManager.LoadScene("Loading");
        }

        public void Client()
        {
            if (relayNetworkManager == null)
            {
                Debug.LogError("RelayNetworkManager not found!");
                return;
            }

            if(joinCodeInput == null || string.IsNullOrEmpty(joinCodeInput.text))
            {
                Debug.LogError("Join code input is empty!");
                OnEmptyJoinCode?.Invoke();
                return;
            }

            relayNetworkManager.relayJoinCode = joinCodeInput.text;

            connectionType = ConnectionType.Client;
            SceneManager.LoadScene("Loading");
        }

        public void Singleplayer()
        {
            if (relayNetworkManager == null)
            {
                Debug.LogError("RelayNetworkManager not found!");
                return;
            }

            connectionType = ConnectionType.SinglePlayer;
            SceneManager.LoadScene(sceneName);
        }

        public void WrongCodeEntered()
        {
            print("Wrong code entered!");
            showWrongCode = true;
            SceneManager.LoadScene("MainMenu");
        }

        public void SwitchConnectUI()
        {
            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                gameObject.SetActive(true);
            } else {
                gameObject.SetActive(false);
            }
        }

        public void SetupNewNetworkScene()
        {
            IsoWorldGenerator worldGenerator = FindFirstObjectByType<IsoWorldGenerator>();
            worldGenerator.StartServer();
        }

        public void SetupNewSinglePlayerScene()
        {
            IsoWorldGenerator worldGenerator = FindFirstObjectByType<IsoWorldGenerator>();
            worldGenerator.InitializeSingleplayer();
        }

        public void SetupNewClientScene()
        {
            IsoWorldGenerator worldGenerator = FindFirstObjectByType<IsoWorldGenerator>();
            worldGenerator.StartServer();
        }
    }
}