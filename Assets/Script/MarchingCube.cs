using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCube : MonoBehaviour
{
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    public float ignoreVertexValue;
    public bool interpolate;
    public float noiseScale;
    public Material meshMaterial;
    public Vector3 centerOfMesh;

    int totalVertexNumber;
    Cube[] cubes;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;


    public void Awake()
    {

    }

    public void Start()
    {

    }

    public void InitVertices()
    {
        totalVertexNumber = numberOfVerticesPerLine * numberOfVerticesPerLine * numberOfVerticesPerLine;
        cubes = new Cube[totalVertexNumber];
        for (int x = 0; x < numberOfVerticesPerLine; x++)
        {
            for (int y = 0; y < numberOfVerticesPerLine; y++)
            {
                for (int z = 0; z < numberOfVerticesPerLine; z++)
                {
                    int cubeIndex = x + y * numberOfVerticesPerLine + z * numberOfVerticesPerLine * numberOfVerticesPerLine;
                    cubes[cubeIndex] = new Cube(new Vector3(x, y, z) * distanceBetweenVertex + centerOfMesh, distanceBetweenVertex);
                    for (int i = 0; i < 8; i++)
                    {
                        cubes[cubeIndex].vertexSelected[i] = CalculateVertexSelction(cubes[cubeIndex].origin + cubes[cubeIndex].offset[i]);
                    }
                }
            }
        }
    }

    public void GenerateMesh()
    {
        InitVertices();

        GameObject meshObject = new GameObject("mesh object");
        meshObject.transform.parent = this.transform;
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CalculateMesh();
        meshRenderer.material = meshMaterial;
    }

    public Mesh CalculateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        int[] triangles;

        foreach(Cube cube in cubes)
        {
            Vector3[] verticesAtCube = cube.GetMarchingCubeVertices();
            vertices.AddRange(verticesAtCube);
        }

        triangles = new int[vertices.Count];
        for(int i=0;i<triangles.Length;i++)
        {
            triangles[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public bool CalculateVertexSelction(Vector3 vertexPosition)
    {
        bool selected = PerlinNoise.GenerateTerrainNoise(vertexPosition, noiseScale) < ignoreVertexValue;
        return selected;
    }
}

public struct Cube
{
    public Vector3 origin;
    public Vector3[] offset;
    public bool[] vertexSelected;
    public float[] vertexWeight;
    public float offsetDistance;

    public Cube(Vector3 origin, float offsetDistance)
    {
        this.origin = origin;
        this.offset = new Vector3[8];
        this.vertexSelected = new bool[8];
        this.vertexWeight = new float[8];
        this.offsetDistance = offsetDistance;

        //initialize cube
        for (int i = 0; i < 8; i++)
        {
            offset[0] = new Vector3(0, 0, 0) * offsetDistance;
            offset[1] = new Vector3(0, 0, 1) * offsetDistance;
            offset[2] = new Vector3(1, 0, 1) * offsetDistance;
            offset[3] = new Vector3(1, 0, 0) * offsetDistance;
            offset[4] = new Vector3(0, 1, 0) * offsetDistance;
            offset[5] = new Vector3(0, 1, 1) * offsetDistance;
            offset[6] = new Vector3(1, 1, 1) * offsetDistance;
            offset[7] = new Vector3(1, 1, 0) * offsetDistance;

            vertexSelected[i] = false;
        }
    }

    public Vector3[] GetMarchingCubeVertices()
    {
        List<Vector3> returnList = new List<Vector3>();

        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (vertexSelected[i])
            {
                index |= 1 << i;
            }
        }

        int[] triangulation = MarchingCubeTable.triangulation[index];

        foreach (int edgeIndex in triangulation)
        {
            if (edgeIndex == -1)
            {
                break;
            }

            int indexA = MarchingCubeTable.cornerIndexAFromEdge[edgeIndex];
            int indexB = MarchingCubeTable.cornerIndexBFromEdge[edgeIndex];

            Vector3 vertexPosition = (offset[indexA] + offset[indexB]) / 2.0f + origin;

            returnList.Add(vertexPosition);
        }

        return returnList.ToArray();
    }
}
