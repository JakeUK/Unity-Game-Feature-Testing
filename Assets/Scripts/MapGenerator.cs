using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {Spheres, IndividualCubes, Mesh};
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 26;
    private Vector3Int chunkDimensions = new Vector3Int(mapChunkSize, mapChunkSize, mapChunkSize);

    public Vector3 globalNoiseScale;
    public Vector3 noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;

    public Vector3 manualOffset;
    public AnimationCurve floorBias;

    [Range(0, 1)]
    public float threshold;
    public bool autoUpdate;
    public float voxelScale = 1;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector3.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.ClearChildren(false);

        if (drawMode == DrawMode.Spheres) {
            //display.DrawNoiseMap(mapData.noiseMap, voxelScale, threshold, true);
        } else if (drawMode == DrawMode.IndividualCubes) {
            //display.DrawNoiseMap(mapData.noiseMap, voxelScale, threshold, false);
        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.blockMap, voxelScale, threshold));
        }
    }

    public void RequestMapData(Vector3 offset, Action<MapData> callback) {
        //Sets the threadstart to what we want to do inside the thread
        ThreadStart threadStart = delegate {
            MapDataThread(offset, callback);
        };
        
        //Starts the thread
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector3 offset, Action<MapData> callback) {
        //Generates the mapData
        MapData mapData = GenerateMapData(offset);

        //Adds the mapData to a queue to be processed outside of the thread
        lock (mapDataThreadInfoQueue) { 
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
        
        //Sets the threadstart to what we want to do inside the thread
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, callback);
        };

        //Starts the thread
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.blockMap, voxelScale, threshold);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    //Clears the queue of generated terrain and sends it to run
    private void Update() {
        int maxMapsCreatedPerFrame = 2;
        int counter1 = 0;
        if (mapDataThreadInfoQueue.Count > 0) {
            for(int i = 0; i < mapDataThreadInfoQueue.Count && counter1 < maxMapsCreatedPerFrame; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
                counter1++;
            }
        }

        int maxMeshsCreatedPerFrame = 2;
        int counter2 = 0;
        if(meshDataThreadInfoQueue.Count > 0) {
            for(int i = 0; i < meshDataThreadInfoQueue.Count && counter2 < maxMeshsCreatedPerFrame; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
                counter2++;
            }
        }

    }

    MapData GenerateMapData(Vector3 centre)
    {
        //3D Noise Example
        NoiseSettings editorSettings = new NoiseSettings();
        editorSettings.noiseType = Noise.NoiseType.D3;
        editorSettings.mapDimensions = chunkDimensions;
        editorSettings.noiseScale = new Vector3(70, 35, 50);
        editorSettings.octaves = octaves;
        editorSettings.persistance = persistance;
        editorSettings.lacunarity = lacunarity;
        editorSettings.seed = seed;
        float[,,] noiseMap = editorSettings.getNoise(centre + manualOffset);
        

        AnimationCurve acCopy = new AnimationCurve(floorBias.keys);
        NoiseSettings floor2D = new NoiseSettings();
        floor2D.noiseType = Noise.NoiseType.D2;
        floor2D.mapDimensions = chunkDimensions;
        floor2D.noiseScale = noiseScale + globalNoiseScale;
        floor2D.octaves = octaves;
        floor2D.persistance = persistance;
        floor2D.lacunarity = lacunarity;
        floor2D.seed = seed;
        float[,,] noise2D = floor2D.getNoise(centre + manualOffset);

        //set
        float bottomOfChunk = centre.y - (chunkDimensions.y / 2f);
        int heightRange = 100;
        int[,,] blockMap = new int[chunkDimensions.x, chunkDimensions.y, chunkDimensions.z];

        for(int z = 0; z < chunkDimensions.z; z++) {
            for(int y = 0; y < chunkDimensions.y; y++) {
                for(int x = 0; x < chunkDimensions.x; x++) {
                    float biasedNoise = acCopy.Evaluate(noise2D[x, 0, z]);
                    float currentYInRange = Mathf.Lerp(0, heightRange, biasedNoise);
                    float currentY = bottomOfChunk + y;

                    //Sets 2D height map
                    if(currentY < currentYInRange) {
                        blockMap[x, y, z] = 1;

                        //Add dirt
                        if(currentY >= currentYInRange - 3) {
                            blockMap[x, y, z] = 3;
                        }
                        //Add grass
                        if(currentY >= currentYInRange - 1) {
                            blockMap[x, y, z] = 2;
                        }

                    } else {
                        blockMap[x, y, z] = 0;
                    }


                    //Carves out caves if block is not transparent
                    if (!Blocks.isBlockTransparent(blockMap[x, y, z]) && noiseMap[x, y, z] < 0.18 && currentY > -15) {
                        blockMap[x, y, z] = 0;
                    }

                }
            }
        }

        return new MapData(blockMap);
    }

    //Called when the editor is updated
    private void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

//Structs
public struct MapData {
    public readonly int[,,] blockMap;

    public MapData(int[,,] blockMap) {
        this.blockMap = blockMap;
    }

}
