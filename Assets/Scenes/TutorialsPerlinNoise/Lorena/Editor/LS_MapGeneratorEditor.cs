using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof(LS_MapGenerator))]
public class LS_MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LS_MapGenerator mapGen = (LS_MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateMap();

            }
        }



        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
    
}
