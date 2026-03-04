using UnityEngine;

public class IsoNoise : MonoBehaviour
{
    public float baseFrequency = 0.02f;
    public int octaves = 4;
    public int seed = 1234;

    float offsetX;
    float offsetY;

    public void SetNoiseSeed(int newSeed)
    {
        seed = newSeed;
        var prng = new System.Random(seed);
        offsetX = prng.Next(-100000, 100000);
        offsetY = prng.Next(-100000, 100000);
    }

    public float Get(float x, float y)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = baseFrequency;
        float max = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(
                x * frequency + offsetX,
                y * frequency + offsetY
            ) * amplitude;

            max += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }

        return total / max;
    }
}