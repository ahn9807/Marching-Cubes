using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCube
{
    int totalVertexNumber;
    Dictionary<Vector3, Cube> cubeDictionary;

    public void InitVertices(Vector3 center, int numberOfVerticesPerLine, float distanceBetweenVertex, float ignoreVertexLevel, NoiseSetting noiseSetting)
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
                    Cube cube = new Cube(new Vector3(x, y, z) * distanceBetweenVertex + center, distanceBetweenVertex);
                    cubeDictionary.Add(cube.origin, cube);
                    for (int i = 0; i < 8; i++)
                    {
                        CalculateVertexWeight(cube, i, ignoreVertexLevel, noiseSetting);
                    }
                }
            }
        }
    }

    public Mesh CalculateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        int[] triangles;

        foreach(KeyValuePair<Vector3, Cube> cube in cubeDictionary)
        {
            Vector3[] verticesAtCube = cube.Value.GetMarchingCubeVertices();
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

    void CalculateVertexWeight(Cube cube, int index, float ignoreVertexValue,  NoiseSetting noiseSetting)
    {
        //Cube neigborCube;
        //check there is exsiting noise value at neighbor
        //Skip this process because we use perlin noise
        //if(cubeDictionary.TryGetValue(new Vector3(vertexPosition.x - someOffset?, vertexPosition.y, vertexPosition.z), out neigborCube) {
        //    ...
        //} else if ...

        float weight = Noise.GenerateTerrainNoise(cube.origin + cube.offset[index], noiseSetting);
        cube.vertexSelected[index] = weight > ignoreVertexValue;
        cube.vertexWeight[index] = weight;
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
