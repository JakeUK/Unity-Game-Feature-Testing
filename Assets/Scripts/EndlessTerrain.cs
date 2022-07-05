using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    public const float viewerMoveThresholdForChunkUpdate = 10f;
    public const float sqrViewerMoveThresholdForChunkupdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public const float horizontalViewDistance = 75f;
    public const float verticalViewDistance = 40f;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector3 viewerPosition;
    Vector3 viewerPositionOld;
    static MapGenerator mapGenerator;


    int chunkSize;
    int chunksVisibleInHorizontalViewDist;
    int chunksVisibleInVerticalViewDist;

    Dictionary<Vector3, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector3, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start(){
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 2;
        chunksVisibleInHorizontalViewDist = Mathf.RoundToInt(horizontalViewDistance / (chunkSize * mapGenerator.voxelScale));
        chunksVisibleInVerticalViewDist = Mathf.RoundToInt(verticalViewDistance / (chunkSize * mapGenerator.voxelScale));
        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector3(viewer.position.x, viewer.position.y, viewer.position.z);

        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkupdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    public void RegenerateAllVisibleChunks() {
        foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate) {
            chunk.RegenerateChunk();
        }
    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++){
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize * mapGenerator.voxelScale);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize * mapGenerator.voxelScale);
        int currentCHunkCoordZ = Mathf.RoundToInt(viewerPosition.z / chunkSize * mapGenerator.voxelScale);

        for(int zOffset = -chunksVisibleInHorizontalViewDist; zOffset <= chunksVisibleInHorizontalViewDist; zOffset++){
            for(int yOffset = -chunksVisibleInVerticalViewDist; yOffset <= chunksVisibleInVerticalViewDist; yOffset++){
                for(int xOffset = -chunksVisibleInHorizontalViewDist; xOffset <= chunksVisibleInHorizontalViewDist; xOffset++){

                    Vector3 viewedChunkedCoord = new Vector3(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset, currentCHunkCoordZ + zOffset);
                    if (terrainChunkDictionary.ContainsKey(viewedChunkedCoord)){
                        terrainChunkDictionary[viewedChunkedCoord].UpdateTerrainChunk();
                    }else {
                        terrainChunkDictionary.Add(viewedChunkedCoord, new TerrainChunk(viewedChunkedCoord, chunkSize, transform, mapMaterial));
                    }

                }
            }
        }

    }

    public class TerrainChunk {

        GameObject meshObject;
        Vector3 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        public TerrainChunk(Vector3 coord, int size, Transform parent, Material material) {
            position = coord * size * mapGenerator.voxelScale;

            bounds = new Bounds(position, Vector3.one * size * mapGenerator.voxelScale);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = position;
            meshObject.transform.SetParent(parent);
            SetVisible(false);

            mapGenerator.RequestMapData(position / mapGenerator.voxelScale - Vector3.one * 2, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
            UpdateTerrainChunk();
        }

        void OnMeshDataRecieved(MeshData meshData) {
            Mesh createdMesh = meshData.CreateMesh();
            if(createdMesh.vertexCount > 6) meshFilter.mesh = createdMesh;
            meshCollider.sharedMesh = createdMesh;
            UpdateTerrainChunk();

        }

        public void RegenerateChunk() {
            mapGenerator.RequestMapData(position / mapGenerator.voxelScale - Vector3.one * 2, OnMapDataRecieved);
        }

        public void UpdateTerrainChunk() {
            float viewDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            float topY = Mathf.Abs(viewerPosition.y - bounds.max.y);
            float bottomY = Mathf.Abs(viewerPosition.y - bounds.min.y);
            float vertDistance = Mathf.Min(topY, bottomY);

            bool visible = viewDistanceFromNearestEdge <= horizontalViewDistance;
            if(vertDistance > verticalViewDistance) {
                visible = false;
            }

            SetVisible(visible);

            if (visible) terrainChunksVisibleLastUpdate.Add(this);

        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }

    }

}
