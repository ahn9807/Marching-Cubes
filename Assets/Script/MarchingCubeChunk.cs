using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MarchingCubeChunk
{
    int totalVertexNumber;
    Dictionary<Vector3, MarchingCube> cubeDictionary;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Material terrainMaterial;

    void InitVertices(Vector3 center, int numberOfVerticesPerLine, float distanceBetweenVertex, float ignoreVertexLevel, float mapMinHeight, float mapMaxHeight, AnimationCurve heightCurve, NoiseSetting noiseSetting)
    {
        this.totalVertexNumber = numberOfVerticesPerLine * numberOfVerticesPerLine * numberOfVerticesPerLine;

        cubeDictionary = new Dictionary<Vector3, MarchingCube>();
        for (int x = 0; x < numberOfVerticesPerLine; x++)
        {
            for (int y = 0; y < numberOfVerticesPerLine; y++)
            {
                for (int z = 0; z < numberOfVerticesPerLine; z++)
                {
                    int cubeIndex = x + y * numberOfVerticesPerLine + z * numberOfVerticesPerLine * numberOfVerticesPerLine;
                    MarchingCube cube = new MarchingCube(new Vector3(x, y, z) * distanceBetweenVertex + center, distanceBetweenVertex);
                    cubeDictionary.Add(cube.origin, cube);
                    for (int i = 0; i < 8; i++)
                    {
                        CalculateVertexWeight(cube, i, ignoreVertexLevel, mapMinHeight, mapMaxHeight, heightCurve, noiseSetting);
                    }
                }
            }
        }
    }

    public GameObject GenerateChunk(Transform parent, Material terrainMaterial, Vector3 center, int numberOfVerticesPerLine, float distanceBetweenVertex, float ignoreVertexLevel, AnimationCurve heightCurve, float mapMinHeight, float mapMaxHeight, NoiseSetting noiseSetting)
    {
        //ThreadStart threadStart = delegate
        //{
        //    ThreadInitVertices(center, numberOfVerticesPerLine, distanceBetweenVertex, ignoreVertexLevel, mapMinHeight, mapMaxHeight, noiseSetting);
        //};

        //new Thread(threadStart).Start();

        this.InitVertices(center, numberOfVerticesPerLine, distanceBetweenVertex, ignoreVertexLevel, mapMinHeight, mapMaxHeight, heightCurve, noiseSetting);
        this.terrainMaterial = terrainMaterial;
        GameObject marchingCubeParentObject = new GameObject();
        marchingCubeParentObject.name = "marching cube parent " + center;
        marchingCubeParentObject.transform.parent = parent;
        meshFilter = marchingCubeParentObject.AddComponent<MeshFilter>();
        meshRenderer = marchingCubeParentObject.AddComponent<MeshRenderer>();
        meshCollider = marchingCubeParentObject.AddComponent<MeshCollider>();

        meshRenderer.material = terrainMaterial;
        Mesh mesh = GenerateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        return marchingCubeParentObject;
    }

    Mesh GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        int[] triangles;

        foreach (KeyValuePair<Vector3, MarchingCube> cube in cubeDictionary)
        {
            Vector3[] verticesAtCube = cube.Value.GetMarchingCubeVertices();
            vertices.AddRange(verticesAtCube);
        }

        triangles = new int[vertices.Count];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void CalculateVertexWeight(MarchingCube cube, int index, float ignoreVertexValue, float mapMinHeight, float mapMaxHeight, AnimationCurve heightCurve, NoiseSetting noiseSetting)
    {
        //Cube neigborCube;
        //check there is exsiting noise value at neighbor
        //Skip this process because we use perlin noise
        //if(cubeDictionary.TryGetValue(new Vector3(vertexPosition.x - someOffset?, vertexPosition.y, vertexPosition.z), out neigborCube) {
        //    ...
        //} else if ...

        float height = cube.origin.y + cube.offset[index].y;
        float height01 = Mathf.Lerp(1, 0, (mapMaxHeight - height) / (mapMaxHeight - mapMinHeight));
        float weight = Noise.GenerateTerrainNoise(cube.origin + cube.offset[index], noiseSetting) * heightCurve.Evaluate(height01);
        if (height < mapMinHeight + cube.offsetDistance)
        {
            weight = 1;
        } else if(height > mapMaxHeight - cube.offsetDistance)
        {
            weight = 0;
        }
        cube.vertexSelected[index] = weight < ignoreVertexValue;
        cube.vertexWeight[index] = weight;
    }
}

public struct MarchingCube
{
    public Vector3 origin;
    public Vector3[] offset;
    public bool[] vertexSelected;
    public float[] vertexWeight;
    public float offsetDistance;

    public MarchingCube(Vector3 origin, float offsetDistance)
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

            float indexAWeight = vertexWeight[indexA] + 0.01f;
            float indexBWeight = vertexWeight[indexB] + 0.01f;
            float weightOffset = 1 / (indexAWeight + indexBWeight);
            indexAWeight *= weightOffset;
            indexBWeight *= weightOffset;

            Vector3 vertexPosition = offset[indexA] * indexAWeight + offset[indexB] * indexBWeight + origin;
            //Debug.Log(weightOffset);
            //Vector3 vertexPosition = (offset[indexA] + offset[indexB]) / 2.0f + origin;

            returnList.Add(vertexPosition);
        }

        return returnList.ToArray();
    }
}
