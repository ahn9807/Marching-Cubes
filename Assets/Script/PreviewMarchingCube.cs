using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PreviewMarchingCube : MonoBehaviour
{
    public bool[] selectedCubeIndex;
    public MeshFilter mesh;

    private void OnDrawGizmos()
    {
        Cube cube = new Cube(Vector3.zero, 1);
       
        for(int i =0; i< 8;i++)
        {
            cube.vertexSelected[i] = selectedCubeIndex[i];
        }

        Vector3[] vertices = cube.GetMarchingCubeVertices();
        int[] triangle = new int[vertices.Length];
        for(int i =0; i<triangle.Length;i++)
        {
            triangle[i] = i;
        }

        mesh.sharedMesh = new Mesh();
        mesh.sharedMesh.vertices = vertices;
        mesh.sharedMesh.triangles = triangle;
        mesh.sharedMesh.RecalculateNormals();

        DrawMarchingCube(cube);
    }

    void DrawMarchingCube(Cube cube)
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < 8; i++)
        {
            if (cube.vertexSelected[i])
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(cube.origin + cube.offset[i], 0.05f);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(cube.origin + cube.offset[i], 0.05f);
            }
        }
    }
}