using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{

    public static MeshData GenerateTerrainMesh(int[,,] landMap, float voxelScale, float threshold){
        int width = landMap.GetLength(0);
        int height = landMap.GetLength(1);
        int depth = landMap.GetLength(2);

        MeshData meshData = new MeshData(width, height, depth);
        int atlusGridSize = 10;
        float atlusIncrement = 1f / atlusGridSize;

        int vertexIndex = 0;

        //Buffer layer on each side so that with chunk implementations, cliffs at chunk borders are still rendered correctly
        for (int z = 1; z < depth - 1; z++){
            for(int y = 1; y < height - 1; y++){
                for(int x = 1; x < width - 1; x++){

                    Block block = Blocks.getBlockInfo(landMap[x, y, z]);

                    //Only create vertices if it is a visible block
                    if (block.visible == false) continue;

                    //Front
                    if (Blocks.isBlockTransparent(landMap[x ,y ,z - 1]) )
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.front);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex+1] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex+2] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex+3] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);

                        meshData.addTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 3);
                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 2, vertexIndex);
                        vertexIndex += 4;
                    }

                    //Back
                    if (Blocks.isBlockTransparent(landMap[x, y, z + 1]))
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.back);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 1] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 2] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 3] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);

                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 1, vertexIndex);
                        meshData.addTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
                        vertexIndex += 4;
                    }

                    //Top
                    if (Blocks.isBlockTransparent(landMap[x, y + 1, z]))
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.top);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 1] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 2] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 3] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);

                        meshData.addTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 3);
                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 2, vertexIndex);
                        vertexIndex += 4;
                    }

                    //Bottom
                    if (Blocks.isBlockTransparent(landMap[x, y - 1, z]))
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.bottom);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 1] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 2] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 3] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);

                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 1, vertexIndex);
                        meshData.addTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
                        vertexIndex += 4;
                    }

                    //Left
                    if (Blocks.isBlockTransparent(landMap[x - 1, y, z]))
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.left);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 1] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 2] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 3] = new Vector3((x * voxelScale) - voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);

                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 1, vertexIndex);
                        meshData.addTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
                        vertexIndex += 4;
                    }

                    //Right
                    if (Blocks.isBlockTransparent(landMap[x + 1, y, z]))
                    {
                        Vector2 texturePosition = Block.getVectorUV(atlusGridSize, block.texture.right);

                        meshData.uvs[vertexIndex] = texturePosition;
                        meshData.uvs[vertexIndex + 1] = texturePosition + new Vector2(atlusIncrement, 0f);
                        meshData.uvs[vertexIndex + 2] = texturePosition + new Vector2(0f, -atlusIncrement);
                        meshData.uvs[vertexIndex + 3] = texturePosition + new Vector2(atlusIncrement, -atlusIncrement);

                        meshData.vertices[vertexIndex] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 1] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) + voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);
                        meshData.vertices[vertexIndex + 2] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) - voxelScale / 2f);
                        meshData.vertices[vertexIndex + 3] = new Vector3((x * voxelScale) + voxelScale / 2f, (y * voxelScale) - voxelScale / 2f, (z * voxelScale) + voxelScale / 2f);

                        meshData.addTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 3);
                        meshData.addTriangle(vertexIndex + 3, vertexIndex + 2, vertexIndex);
                        vertexIndex += 4;
                    }
                }
            }
        }

        return meshData;
    }



}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight, int meshDepth)
    {
        vertices = new Vector3[(meshWidth + 1) * (meshHeight + 1) * (meshDepth + 1)];
        uvs = new Vector2[(meshWidth + 1) * (meshHeight + 1) * (meshDepth + 1)];
        //Max number of triangles possible in the chunk every other cube with max number of faces
        triangles = new int[(meshWidth / 2) * (meshHeight / 2) * (meshDepth / 2) * 3 * 12];
    }

    public void addTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

}