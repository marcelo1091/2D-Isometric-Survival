using System;
using System.Collections.Generic;

using Mirror;

using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Utp;
using Network;

public class RelayNetworkManager : NetworkManager
{
	private UtpTransport utpTransport;
	private UnityEngine.GameObject worldGeneratorGO;

	/// <summary>
	/// Server's join code if using Relay.
	/// </summary>
	public string relayJoinCode = "";

	/// <summary>
	/// Player prefab for single-player mode.
	/// </summary>
	//[SerializeField] private new GameObject playerPrefab;

	/// <summary>
	/// Spawn point for single-player mode (optional).
	/// </summary>
	[SerializeField] private Transform spawnPoint;

	public override void Awake()
	{
		DontDestroyOnLoad(gameObject);

		base.Awake();

		utpTransport = GetComponent<UtpTransport>();
		
		// Find worldGenerator by searching through all GameObjects for IsoWorldGenerator component
		FindWorldGenerator();

		string[] args = System.Environment.GetCommandLineArgs();
		for (int key = 0; key < args.Length; key++)
		{
			if (args[key] == "-port")
			{
				if (key + 1 < args.Length)
				{
					string value = args[key + 1];

					try
					{
						utpTransport.Port = ushort.Parse(value);
					}
					catch
					{
						UtpLog.Warning($"Unable to parse {value} into transport Port");
					}
				}
			}
		}
	}

	/// <summary>
	/// Get the port the server is listening on.
	/// </summary>
	/// <returns>The port.</returns>
	public ushort GetPort()
	{
		return utpTransport.Port;
	}

	/// <summary>
	/// Get whether Relay is enabled or not.
	/// </summary>
	/// <returns>True if enabled, false otherwise.</returns>
	public bool IsRelayEnabled()
	{
		return utpTransport.useRelay;
	}

	/// <summary>
	/// Ensures Relay is disabled. Starts the server, listening for incoming connections.
	/// </summary>
	public void StartStandardServer()
	{
		utpTransport.useRelay = false;
		StartServer();
	}

	/// <summary>
	/// Ensures Relay is disabled. Starts a network "host" - a server and client in the same application
	/// </summary>
	public void StartStandardHost()
	{
		utpTransport.useRelay = false;
		StartHost();
	}

	/// <summary>
	/// Gets available Relay regions.
	/// </summary>
	/// 
	public void GetRelayRegions(Action<List<Region>> onSuccess, Action onFailure)
	{
		utpTransport.GetRelayRegions(onSuccess, onFailure);
	}

	/// <summary>
	/// Ensures Relay is enabled. Starts a network "host" - a server and client in the same application
	/// </summary>
	public void StartRelayHost(int maxPlayers, Action onSuccess, Action onFailure, string regionId = null)
	{
		utpTransport.useRelay = true;
		utpTransport.AllocateRelayServer(maxPlayers, regionId,
		(string joinCode) =>
		{
			relayJoinCode = joinCode;
			Debug.LogError(joinCode);

			StartHost();
			onSuccess?.Invoke();
			Debug.Log("Relay host started with join code: " + joinCode);
		},
		() =>
		{
			onFailure?.Invoke();
			UtpLog.Error($"Failed to start a Relay host.");
		});

		if (worldGeneratorGO == null)
		{
			// Try finding it again in case it wasn't found during Awake
			FindWorldGenerator();
		}

		if (worldGeneratorGO == null)
		{
			Debug.LogError("IsoWorldGenerator not found in scene!");
			return;
		}

		if (worldGeneratorGO)
		{
			var worldGenerator = worldGeneratorGO.GetComponent("IsoWorldGenerator");
			worldGenerator.GetType().GetMethod("StartServer")?.Invoke(worldGenerator, null);
		}

	}

	/// <summary>
	/// Ensures Relay is disabled. Starts the client, connects it to the server with networkAddress.
	/// </summary>
	public void JoinStandardServer()
	{
		utpTransport.useRelay = false;
		StartClient();
	}

	/// <summary>
	/// Find the IsoWorldGenerator component in the scene
	/// </summary>
	private void FindWorldGenerator()
	{
		worldGeneratorGO = FindFirstObjectByType<IsoWorldGenerator>()?.gameObject;
		
		Debug.LogWarning("IsoWorldGenerator not found in scene!");
	}

	/// <summary>
	/// Start single-player game mode without networking.
	/// </summary>
	public void StartSingleplayer()
	{
		utpTransport.useRelay = false;

		if (worldGeneratorGO == null)
		{
			// Try finding it again in case it wasn't found during Awake
			FindWorldGenerator();
		}

		if (worldGeneratorGO == null)
		{
			Debug.LogError("IsoWorldGenerator not found in scene!");
			return;
		}

		// Make sure the world generator GameObject is active FIRST
		if (!worldGeneratorGO.activeInHierarchy)
		{
			worldGeneratorGO.SetActive(true);
			Debug.Log("Activated IsoWorldGenerator GameObject");
		}

		var worldGenerator = worldGeneratorGO.GetComponent<IsoWorldGenerator>();
		if (worldGenerator == null)
		{
			Debug.LogError("IsoWorldGenerator component not found!");
			return;
		}

		// Generate world seed
		int worldSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

		// Spawn player FIRST (before initializing world)
		if (playerPrefab != null)
		{
			SpawnLocalPlayer(worldGenerator, worldSeed);
		}
		else
		{
			Debug.LogWarning("Player prefab not assigned to RelayNetworkManager. Player will not be spawned.");

			worldGenerator.InitializeSingleplayer();
		}

		Debug.Log("Single-player mode started with seed: " + worldSeed);
	}

	private void SpawnLocalPlayer(IsoWorldGenerator worldGenerator, int worldSeed)
	{
		Vector3 spawn = spawnPoint != null ? spawnPoint.position : Vector3.zero;

		GameObject localPlayer = Instantiate(playerPrefab, spawn, Quaternion.identity);

		// Initialize world BEFORE setting up player components

		worldGenerator.InitializeSingleplayer();

		// Setup player controller via reflection
		var controller = localPlayer.GetComponent<IsoPlayerController>();
		if (controller != null)
		{
			controller.SetLocalPlayer();
		}

		// Setup camera via reflection
		var cameraSetter = localPlayer.GetComponent<PlayerCameraSetter>();
		if (cameraSetter != null)
		{
			cameraSetter.SetAsSinglePlayer();
		}

		// Tell world generator where the player is via reflection
		worldGenerator.SetPlayerTransform(localPlayer.transform);

		Debug.Log("Single-player: Player spawned at " + spawn);
	}

	/// <summary>
	/// Ensures Relay is enabled. Starts the client, connects to the server with the relayJoinCode.
	/// </summary>
	public void JoinRelayServer( Action onSuccess, Action onFailure)
	{
		utpTransport.useRelay = true;
		try{
			var connect = FindFirstObjectByType<Connect>();
			utpTransport.ConfigureClientWithJoinCode(relayJoinCode,
			onSuccess: () =>
			{
				StartClient();
				onSuccess?.Invoke();
				Debug.Log("Joined Relay server with join code: " + relayJoinCode);
				/*if (worldGeneratorGO == null)
				{
					// Try finding it again in case it wasn't found during Awake
					FindWorldGenerator();
				}

				if (worldGeneratorGO == null)
				{
					Debug.LogError("IsoWorldGenerator not found in scene!");
					return;
				}

				if (worldGeneratorGO)
				{
					var connect = FindFirstObjectByType<Connect>();
					var worldGenerator = worldGeneratorGO.GetComponent<IsoWorldGenerator>();
					connect.wrongJoinCodeText.gameObject.SetActive(false);
					worldGenerator.StartServer();
				}*/
			},
			onFailure: () =>
			{
				onFailure?.Invoke();
				UtpLog.Error($"Failed to join Relay server.");
				
				// if(connect != null)
				// {
				// 	connect.WrongCodeEntered();
				// }
			}
			);
			Debug.Log("Attempting to join Relay server with join code: " + relayJoinCode);
		} catch (Exception e)
		{
			Debug.LogError("Error logging join code: " + e.Message);
		}
	}
}