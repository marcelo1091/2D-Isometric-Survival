using UnityEngine;
using UnityEngine.Tilemaps;

public class IsoTreeGenerator : MonoBehaviour
{
    [Header("References")]
    public Tilemap treeTilemap;

    [Header("Tree Tiles")]
    public TileBase[] treeTiles;

    [Header("Forest Noise")]
    public float baseFrequency = 0.05f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float forestThreshold = 0.55f;

    [Range(0f, 1f)]
    public float whiteNoiseDensity = 0.85f;

    public int seedOffset = 9999;

    int worldSeed;
    TileBase[] treeBuffer;
    TileBase[] clearBuffer;

    public void Init(int seed, int chunkSize)
    {
        worldSeed = seed;

        int area = chunkSize * chunkSize;

        treeBuffer = new TileBase[area];
        clearBuffer = new TileBase[area];
    }

    public void GenerateChunkTrees(Vector2Int coord, int chunkSize, IsoNoise noise, float waterThreshold)
    {
        if (treeTiles == null || treeTiles.Length == 0)
            return;

        int size = chunkSize;

        Vector3Int startPos = new Vector3Int(
            coord.x * size,
            coord.y * size,
            1
        );

        BoundsInt bounds = new BoundsInt(
            startPos.x,
            startPos.y,
            1,
            size,
            size,
            1
        );

        System.Random prng = new System.Random(
            worldSeed + seedOffset +
            coord.x * 73856093 ^
            coord.y * 19349663
        );

        float offsetX = prng.Next(-10000, 10000);
        float offsetY = prng.Next(-10000, 10000);

        int i = 0;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int worldX = coord.x * size + x;
                int worldY = coord.y * size + y;

                float terrainNoise = noise.Get(worldX, worldY);

                if (terrainNoise < waterThreshold)
                {
                    treeBuffer[i] = null;
                    i++;
                    continue;
                }

                float forestNoise = FractalNoise(worldX, worldY, offsetX, offsetY);
                float white = (float)prng.NextDouble();

                if (forestNoise > forestThreshold &&
                    white < whiteNoiseDensity)
                {
                    treeBuffer[i] = treeTiles[prng.Next(treeTiles.Length)];
                }
                else
                {
                    treeBuffer[i] = null;
                }

                i++;
            }

        treeTilemap.SetTilesBlock(bounds, treeBuffer);
    }

    public void ClearChunkTrees(Vector2Int coord, int chunkSize)
    {
        int size = chunkSize;

        Vector3Int startPos = new Vector3Int(
            coord.x * size,
            coord.y * size,
            1
        );

        BoundsInt bounds = new BoundsInt(
            startPos.x,
            startPos.y,
            1,
            size,
            size,
            1
        );

        treeTilemap.SetTilesBlock(bounds, clearBuffer);
    }

    float FractalNoise(int x, int y, float offsetX, float offsetY)
    {
        float amplitude = 1f;
        float frequency = baseFrequency;
        float total = 0f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float noise = Mathf.PerlinNoise(
                x * frequency + offsetX,
                y * frequency + offsetY
            );

            total += noise * amplitude;
            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }
}