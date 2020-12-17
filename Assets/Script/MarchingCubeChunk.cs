using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public static class MarchingCubeChunk
{
    public static List<MarchingCube> InitVertices(Vector3 center, MarchinCubeChunkSettings marchingCubeChunksettings, NoiseSetting noiseSetting)
    {
        List<MarchingCube> cubeList = new List<MarchingCube>();
        for (int x = 0; x < marchingCubeChunksettings.numberOfVerticesPerLine; x++)
        {
            for (int y = 0; y < marchingCubeChunksettings.numberOfVerticesPerLine; y++)
            {
                for (int z = 0; z < marchingCubeChunksettings.numberOfVerticesPerLine; z++)
                {
                    MarchingCube cube = new MarchingCube(new Vector3(x, y, z) * marchingCubeChunksettings.distanceBetweenVertex + center, marchingCubeChunksettings.distanceBetweenVertex);
                    cubeList.Add(cube);
                    for (int i = 0; i < 8; i++)
                    {
                        CalculateVertexWeight(cube, i, marchingCubeChunksettings, noiseSetting);
                    }
                }
            }
        }

        return cubeList;
    }

    public static Vector3[] GenerateVertices(List<MarchingCube> cubes)
    {
        List<Vector3> vertices = new List<Vector3>();

        foreach (MarchingCube cube in cubes)
        {
            Vector3[] verticesAtCube = cube.GetMarchingCubeVertices();
            vertices.AddRange(verticesAtCube);
        }

        return vertices.ToArray();
    }

    public static int[] GenerateTriangles(Vector3[] vertices)
    {
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        return triangles;
    }

    //public GameObject GenerateChunk(Transform parent, Vector3 center, MarchinCubeChunkSettings settings, NoiseSetting noiseSetting)
    //{
    //    //ThreadStart threadStart = delegate
    //    //{
    //    //    ThreadInitVertices(center, numberOfVerticesPerLine, distanceBetweenVertex, ignoreVertexLevel, mapMinHeight, mapMaxHeight, noiseSetting);
    //    //};

    //    //new Thread(threadStart).Start();

    //    this.InitVertices(center, settings, noiseSetting);
    //    GameObject marchingCubeParentObject = new GameObject();
    //    marchingCubeParentObject.name = "marching cube parent " + center;
    //    marchingCubeParentObject.transform.parent = parent;
    //    MeshFilter meshFilter = marchingCubeParentObject.AddComponent<MeshFilter>();
    //    MeshRenderer meshRenderer = marchingCubeParentObject.AddComponent<MeshRenderer>();
    //    MeshCollider meshCollider = marchingCubeParentObject.AddComponent<MeshCollider>();

    //    meshRenderer.material = settings.terrainMaterial;
    //    Mesh mesh = GenerateMesh();
    //    meshFilter.mesh = mesh;
    //    meshCollider.sharedMesh = mesh;

    //    return marchingCubeParentObject;
    //}

    static void CalculateVertexWeight(MarchingCube cube, int index, MarchinCubeChunkSettings marchinCubeChunkSettings, NoiseSetting noiseSetting)
    {
        //Cube neigborCube;
        //check there is exsiting noise value at neighbor
        //Skip this process because we use perlin noise
        //if(cubeDictionary.TryGetValue(new Vector3(vertexPosition.x - someOffset?, vertexPosition.y, vertexPosition.z), out neigborCube) {
        //    ...
        //} else if ...

        AnimationCurve heightCurve = new AnimationCurve(marchinCubeChunkSettings.heightCurve.keys);
        float height = cube.origin.y + cube.offset[index].y;
        float height01 = Mathf.Lerp(1, 0, (marchinCubeChunkSettings.mapMaxHeight - height) / (marchinCubeChunkSettings.mapMaxHeight - marchinCubeChunkSettings.mapMinHeight));
        
        float weight = Noise.GenerateTerrainNoise(cube.origin + cube.offset[index], noiseSetting) * heightCurve.Evaluate(height01);
        //Debug.Log(height01);
        if (height < marchinCubeChunkSettings.mapMinHeight + cube.offsetDistance)
        {
            weight = 1;
        } else if(height > marchinCubeChunkSettings.mapMaxHeight - cube.offsetDistance)
        {
            weight = 0;
        }
        cube.vertexSelected[index] = weight < marchinCubeChunkSettings.ignoreVertexLevel;
        cube.vertexWeight[index] = weight;
    }
}

[System.Serializable]
public class MarchinCubeChunkSettings
{
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    public float ignoreVertexLevel;
    [System.NonSerialized]
    public float mapMinHeight;
    [System.NonSerialized]
    public float mapMaxHeight;
    public AnimationCurve heightCurve;
    [System.NonSerialized]
    public Transform parent;
    public Material terrainMaterial;
}


