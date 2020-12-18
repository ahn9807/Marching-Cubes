using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubeGenerator))]
public class MarchingCubeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MarchingCubeGenerator marchingCubeChunk = (MarchingCubeGenerator)target;

        if(GUILayout.Button("Generate"))
        {
            marchingCubeChunk.Initialize();
            marchingCubeChunk.GenerateChunksAtMain(Vector3.zero);
        }
        if(GUILayout.Button("Delete Mesh"))
        {
            marchingCubeChunk.DeleteMarchingCubeObject();
        }
    }
}
