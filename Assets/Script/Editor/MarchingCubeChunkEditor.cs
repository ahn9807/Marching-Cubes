using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubeChunk))]
public class MarchingCubeChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MarchingCubeChunk marchingCubeChunk = (MarchingCubeChunk)target;

        if(GUILayout.Button("Generate"))
        {
            marchingCubeChunk.GenerateChunks();
        }
        if(GUILayout.Button("Delete Mesh"))
        {
            marchingCubeChunk.DeleteMarchingCubeObject();
        }
    }
}
