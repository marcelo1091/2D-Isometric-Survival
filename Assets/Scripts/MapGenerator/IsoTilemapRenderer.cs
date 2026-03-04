// IsoTilemapRenderer.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class IsoTilemapRenderer : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;

    [Header("Tiles")]
    public TileBase landTile;
    public TileBase waterTile;

    TileBase[] groundBuffer;
    TileBase[] waterBuffer;
    TileBase[] clearBuffer;

    int cachedChunkSize = -1;

    void EnsureBuffers(int chunkSize)
    {
        if (cachedChunkSize == chunkSize && groundBuffer != null)
            return;

        cachedChunkSize = chunkSize;

        int area = chunkSize * chunkSize;

        groundBuffer = new TileBase[area];
        waterBuffer = new TileBase[area];
        clearBuffer = new TileBase[area];
    }

    public void DrawChunk(Vector2Int coord, int chunkSize, IsoNoise noise)
    {
        EnsureBuffers(chunkSize);

        int size = chunkSize;

        Vector3Int startPos = new Vector3Int(
            coord.x * size,
            coord.y * size,
            0
        );

        BoundsInt bounds = new BoundsInt(
            startPos.x,
            startPos.y,
            0,
            size,
            size,
            1
        );

        int i = 0;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int worldX = coord.x * size + x;
                int worldY = coord.y * size + y;

                float n = noise.Get(worldX, worldY);

                if (n < 0.45f)
                {
                    waterBuffer[i] = waterTile;
                    groundBuffer[i] = null;
                }
                else
                {
                    groundBuffer[i] = landTile;
                    waterBuffer[i] = null;
                }

                i++;
            }

        groundTilemap.SetTilesBlock(bounds, groundBuffer);
        waterTilemap.SetTilesBlock(bounds, waterBuffer);
    }

    public void ClearChunk(Vector2Int coord, int chunkSize)
    {
        EnsureBuffers(chunkSize);

        int size = chunkSize;

        Vector3Int startPos = new Vector3Int(
            coord.x * size,
            coord.y * size,
            0
        );

        BoundsInt bounds = new BoundsInt(
            startPos.x,
            startPos.y,
            0,
            size,
            size,
            1
        );

        groundTilemap.SetTilesBlock(bounds, clearBuffer);
        waterTilemap.SetTilesBlock(bounds, clearBuffer);
    }
}