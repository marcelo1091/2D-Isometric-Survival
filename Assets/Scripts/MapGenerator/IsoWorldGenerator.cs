using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class IsoWorldGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 32;
    public int viewDistance = 2;
    public int chunksPerFrame = 2;

    [Header("References")]
    public Transform player;
    public Grid grid;

    public IsoTilemapRenderer renderer;
    public IsoTreeGenerator treeGenerator;
    public IsoNoise noise;

    public float waterThreshold = 0.45f;

    public int seed;

    Dictionary<Vector2Int, IsoChunk> chunks = new();
    Queue<Vector2Int> chunkQueue = new();

    Vector2Int currentPlayerChunk;

    bool initialized = false;
/*
    // SERVER generuje seed
    public override void OnStartServer()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
    }

    // CLIENT czeka aż seed przyjdzie
    public override void OnStartClient()
    {
        StartCoroutine(InitWhenReady());
    }*/

    public void StartServer()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        StartCoroutine(InitWhenReady());
    }

    /// <summary>
    /// Initialize world for single-player mode
    /// </summary>
    public void InitializeSingleplayer()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        noise.SetNoiseSeed(seed);
        treeGenerator.Init(seed, chunkSize);
    }

    /// <summary>
    /// Set player transform for single-player mode
    /// </summary>
    public void SetPlayerTransform(Transform playerTransform)
    {
        player = playerTransform;
        
        if (player != null && !initialized)
        {
            currentPlayerChunk = GetChunkCoord(player.position);
            StartCoroutine(ProcessChunkQueue());
            UpdateWorld();
            initialized = true;
        }
    }

    IEnumerator InitWhenReady()
    {
        while (NetworkClient.localPlayer == null)
            yield return null;

        player = NetworkClient.localPlayer.transform;

        noise.SetNoiseSeed(seed);

        treeGenerator.Init(seed, chunkSize);

        currentPlayerChunk = GetChunkCoord(player.position);

        StartCoroutine(ProcessChunkQueue());
        UpdateWorld();

        initialized = true;
    }

    void Update()
    {
        if (!initialized || player == null)
            return;

        Vector2Int newChunk = GetChunkCoord(player.position);

        if (newChunk != currentPlayerChunk)
        {
            currentPlayerChunk = newChunk;
            UpdateWorld();
        }
    }

    void UpdateWorld()
    {
        HashSet<Vector2Int> needed = new();

        for (int x = -viewDistance; x <= viewDistance; x++)
        for (int y = -viewDistance; y <= viewDistance; y++)
        {
            Vector2Int coord = currentPlayerChunk + new Vector2Int(x, y);
            needed.Add(coord);

            if (!chunks.ContainsKey(coord))
            {
                chunks.Add(coord, new IsoChunk(coord));
                chunkQueue.Enqueue(coord);
            }
        }

        List<Vector2Int> toRemove = new();

        foreach (var kvp in chunks)
        {
            if (!needed.Contains(kvp.Key))
            {
                renderer.ClearChunk(kvp.Key, chunkSize);
                treeGenerator.ClearChunkTrees(kvp.Key, chunkSize);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var coord in toRemove)
        {
            chunks.Remove(coord);
        }
    }

    IEnumerator ProcessChunkQueue()
    {
        while (true)
        {
            int count = 0;

            while (chunkQueue.Count > 0 && count < chunksPerFrame)
            {
                Vector2Int coord = chunkQueue.Dequeue();

                if (chunks.TryGetValue(coord, out IsoChunk chunk))
                {
                    if (!chunk.generated)
                    {
                        renderer.DrawChunk(coord, chunkSize, noise);
                        treeGenerator.GenerateChunkTrees(coord, chunkSize, noise, waterThreshold);

                        chunk.generated = true;
                    }
                }

                count++;
            }

            yield return null;
        }
    }

    Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);

        int cx = Mathf.FloorToInt((float)cell.x / chunkSize);
        int cy = Mathf.FloorToInt((float)cell.y / chunkSize);

        return new Vector2Int(cx, cy);
    }
}