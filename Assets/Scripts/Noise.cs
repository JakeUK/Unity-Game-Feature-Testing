using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public enum NormalizeMode {Local, Global};
    public enum NoiseType {D2, D3};

    public static float[,,] GenerateNoiseMap(NoiseType noiseType, int seed, Vector3Int mapDimensions, Vector3 scale, int octaves, float persistance, float lacunarity, Vector3 offset, NormalizeMode normalizeMode){
        float[,,] noiseMap = new float[mapDimensions.x, mapDimensions.y, mapDimensions.z];
        System.Random prng = new System.Random(seed);

        Vector3[] octaveOffsets = new Vector3[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            float offsetZ = prng.Next(-100000, 100000) + offset.z;

            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;

        }

        //Clamp for cases of zero
        if (scale.x <= 0) scale.x = 0.00001f;
        if (scale.y <= 0) scale.y = 0.00001f;
        if (scale.z <= 0) scale.z = 0.00001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfX = mapDimensions.x / 2f;
        float halfY = mapDimensions.y / 2f;
        float halfZ = mapDimensions.z / 2f;

        if (noiseType == NoiseType.D2) mapDimensions.y = 1;

        for (int z = 0; z < mapDimensions.z; z++){
            for(int y = 0; y < mapDimensions.y; y++){
                for(int x = 0; x < mapDimensions.x; x++){

                    amplitude = 1;
                    frequency = 1;
                    float localNoiseHeight = 0;

                    for (int i = 0; i < octaves; i++) {
                        float rawNoise;

                        float sampleX = (x - halfX + octaveOffsets[i].x) / scale.x * frequency;
                        float sampleY = (y - halfY + octaveOffsets[i].y) / scale.y * frequency;
                        float sampleZ = (z - halfZ + octaveOffsets[i].z) / scale.z * frequency;

                        //Equation for 3D Noise
                        if (noiseType == NoiseType.D3) {
                            float ab = Mathf.PerlinNoise(sampleX, sampleY);
                            float bc = Mathf.PerlinNoise(sampleY, sampleZ);
                            float ac = Mathf.PerlinNoise(sampleX, sampleZ);

                            float ba = Mathf.PerlinNoise(sampleY, sampleX);
                            float cb = Mathf.PerlinNoise(sampleZ, sampleY);
                            float ca = Mathf.PerlinNoise(sampleZ, sampleX);

                            float abc = ab + bc + ac + ba + cb + ca;
                            rawNoise = abc / 6f;
                        } else {
                            rawNoise = Mathf.PerlinNoise(sampleX, sampleZ);
                        }

                        float perlinValue = rawNoise * 2 - 1;
                        localNoiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    if (localNoiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = localNoiseHeight;
                    else if (localNoiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = localNoiseHeight;
                    noiseMap[x, y, z] = localNoiseHeight;
                }
            }
        }

        //Normalises noise between 0-1
        for (int z = 0; z < mapDimensions.z; z++){
            for (int y = 0; y < mapDimensions.y; y++){
                for (int x = 0; x < mapDimensions.x; x++){
                    if(normalizeMode == NormalizeMode.Local) {
                        noiseMap[x, y, z] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y, z]);
                    } else if (normalizeMode == NormalizeMode.Global) {
                        float normalizedHeight = (noiseMap[x, y, z] + 1) / (2f * maxPossibleHeight / 1.5f);
                        noiseMap[x, y, z] = normalizedHeight;
                    }
                }
            }
        }

        return noiseMap;
    }
}
