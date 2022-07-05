using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSettings
{
    public Noise.NoiseType noiseType;
    public Vector3Int mapDimensions;
    public Vector3 noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;

    public float[,,] getNoise(Vector3 offset) {
        return Noise.GenerateNoiseMap(noiseType, seed, mapDimensions, noiseScale, octaves, persistance, lacunarity, offset, Noise.NormalizeMode.Global);
    }

}
