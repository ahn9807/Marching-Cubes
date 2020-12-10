using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCube
{
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    public float ignoreVertexValue;
    public bool interpolate;
    public float noiseScale;
    public Material meshMaterial;
    public Vector3 centerOfMesh;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    int totalVertexNumber;
    Dictionary<Vector3, Cube> cubeDictionary;
    Cube[] cubes;


    public void InitVertices()
    {
        totalVertexNumber = numberOfVerticesPerLine * numberOfVerticesPerLine * numberOfVerticesPerLine;
        cubeDictionary = new Dictionary<Vector3, Cube>();
        for (int x = 0; x < numberOfVerticesPerLine; x++)
        {
            for (int y = 0; y < numberOfVerticesPerLine; y++)
            {
                for (int z = 0; z < numberOfVerticesPerLine; z++)
                {
                    int cubeIndex = x + y * numberOfVerticesPerLine + z * numberOfVerticesPerLine * numberOfVerticesPerLine;
                    Cube cube = new Cube(new Vector3(x, y, z) * distanceBetweenVertex + centerOfMesh, distanceBetweenVertex);
                    cubeDictionary.Add(cube.origin, cube);
                    for (int i = 0; i < 8; i++)
                    {
                        cube.vertexSelected[i] = CalculateVertexSelction(cube.origin + cube.offset[i]);
                    }
                }
            }
        }
    }

    public void GenerateMesh(Transform parent)
    {
        InitVertices();

        GameObject meshObject = new GameObject("mesh object");
        meshObject.transform.parent = parent;
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

    public Vector3 GetVertexPosition(int index)
    {
        return origin + offset[index] * offsetDistance;
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
