using UnityEngine;

public class IsoChunk
{
    public Vector2Int coord;
    public bool generated;

    public IsoChunk(Vector2Int coord)
    {
        this.coord = coord;
        generated = false;
    }
}