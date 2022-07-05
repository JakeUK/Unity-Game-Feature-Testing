using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator) target;

        //Change in value
        if (DrawDefaultInspector()){
            if (mapGen.autoUpdate){
                if (!Application.isPlaying) mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Re-Generate"))
        {
            if (!Application.isPlaying) mapGen.DrawMapInEditor();
            else FindObjectOfType<EndlessTerrain>().RegenerateAllVisibleChunks();
        }
    }
}
