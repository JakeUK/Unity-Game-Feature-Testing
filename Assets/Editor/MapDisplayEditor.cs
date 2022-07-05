using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDisplay))]
public class MapDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapDisplay mapDis = (MapDisplay)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Clear"))
        {
            mapDis.ClearChildren(true);
        }
    }
}
