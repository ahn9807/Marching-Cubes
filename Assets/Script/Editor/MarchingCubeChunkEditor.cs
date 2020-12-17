using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubeGenerator))]
public class MarchingCubeChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MarchingCubeGenerator marchingCubeChunk = (MarchingCubeGenerator)target;

        if(GUILayout.Button("Generate"))
        {
            marchingCubeChunk.Initialize();
            marchingCubeChunk.GenerateChunks(Vector3.zero);
        }
        if(GUILayout.Button("Delete Mesh"))
        {
            marchingCubeChunk.DeleteMarchingCubeObject();
        }
    }
}
