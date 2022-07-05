using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Blocks
{
    static Dictionary<int, Block> blockDictionary = new Dictionary<int, Block> {
        {0, new Block(false, true, -1, -1, -1, -1, -1, -1) },
        {1, new Block(true, false, 0, 0, 0, 0, 0, 0) },
        {2, new Block(true, false, 1, 2, 3, 3, 3, 3) },
        {3, new Block(true, false, 2, 2, 2, 2, 2, 2) }
    };


    public static Block getBlockInfo(int id) {
        return blockDictionary[id];
    }

    public static bool isBlockTransparent(int id) {
        return blockDictionary[id].transparent;
    }
}

public struct Block {
    public bool visible;
    public bool transparent;
    public Texture texture;

    public Block(bool visible, bool transparent, int top, int bottom, int left, int right, int front, int back) {
        this.visible = visible;
        this.transparent = transparent;
        this.texture.top = top;
        this.texture.bottom = bottom;
        this.texture.left = left;
        this.texture.right = right;
        this.texture.front = front;
        this.texture.back = back;
    }

    /*
     * Return top left location of block texture
    */
    public static Vector2 getVectorUV(int gridSize, int texturePos) {
        float increment = 1f / gridSize;
        float x = (texturePos % gridSize) * increment;
        float y = 1f - ((texturePos - (texturePos % gridSize)) / gridSize * increment);
        return new Vector2(x, y);
    }

    public struct Texture {
        public int top;
        public int bottom;
        public int left;
        public int right;
        public int front;
        public int back;
    }

}


