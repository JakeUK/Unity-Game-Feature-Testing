using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    public GameObject voxelSphere;
    public GameObject voxelCube;
    public Transform voxelParent;
    public bool drawIntensity;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
      

    public void DrawNoiseMap(float[,,] noiseMap, float voxelSpacing, float threshold, bool isSphere){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        int depth = noiseMap.GetLength(2);

        for(int z = 0; z < depth; z++){
            for(int y = 0; y < height; y++){
                for(int x = 0; x < width; x++){
                    //Check for threshold
                    if (noiseMap[x,y,z] <= threshold)
                    {
                        Color voxelColor = Color.Lerp(Color.black, Color.white, noiseMap[x, y, z]);

                        GameObject inUse;
                        if (isSphere) inUse = voxelSphere;
                        else inUse = voxelCube;

                        GameObject newVoxel = Instantiate(inUse, new Vector3(x, y, z) * voxelSpacing, Quaternion.identity);

                        //Sets color of Voxel to intensity of that sample
                        if (drawIntensity) newVoxel.GetComponent<Renderer>().material.color = voxelColor;
                        newVoxel.transform.SetParent(voxelParent);
                    }
                }
            }
        }
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    public void ClearChildren(bool editor){
        foreach(Transform child in voxelParent)
        {
            if(editor)   GameObject.DestroyImmediate(child.gameObject);
            if (!editor) GameObject.Destroy(child.gameObject);
        }
    }
}
