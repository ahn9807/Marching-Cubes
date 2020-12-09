using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCube))]
public class MarchingCubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MarchingCube marchingCube = (MarchingCube)target;

        if(GUILayout.Button("Generate"))
        {
            marchingCube.GenerateMesh();
        }
    }
}
