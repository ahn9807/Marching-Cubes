using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public static class MarchingCubeChunk
{
    public static List<MarchingCube> InitVertices(Vector3 center, MarchingCubeChunkSetting marchingCubeChunksettings, NoiseSetting noiseSetting)
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

    static void CalculateVertexWeight(MarchingCube cube, int index, MarchingCubeChunkSetting marchingCubeSetting, NoiseSetting noiseSetting)
    {
        //Cube neigborCube;
        //check there is exsiting noise value at neighbor
        //Skip this process because we use perlin noise
        //if(cubeDictionary.TryGetValue(new Vector3(vertexPosition.x - someOffset?, vertexPosition.y, vertexPosition.z), out neigborCube) {
        //    ...
        //} else if ...

        //For threading, we have to dupulicate Animation Curve
        AnimationCurve weightCurve = new AnimationCurve(marchingCubeSetting.weightCurve.keys);

        float height = cube.origin.y + cube.offset[index].y;
        float height01 = Mathf.Lerp(1, 0, (marchingCubeSetting.mapMaxHeight - height) / (marchingCubeSetting.mapMaxHeight - marchingCubeSetting.mapMinHeight));
        float weight = MarchingCubeNoise.GenerateTerrainNoise(cube.origin + cube.offset[index], noiseSetting) * weightCurve.Evaluate(height01);

        if (height < marchingCubeSetting.mapMinHeight + cube.offsetDistance)
        {
            weight = 1;
        } else if(height > marchingCubeSetting.mapMaxHeight - cube.offsetDistance)
        {
            weight = 0;
        }
        cube.vertexSelected[index] = weight < marchingCubeSetting.ignoreVertexLevel;
        cube.vertexWeight[index] = weight;
    }
}

[System.Serializable]
public class MarchingCubeChunkSetting
{
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    [Range(0,1)]
    public float ignoreVertexLevel;
    public Vector3 chunkRenderNumber;
    public AnimationCurve weightCurve;
    [System.NonSerialized]
    public Transform parent;
    public Material terrainMaterial;

    public float mapMinHeight
    {
        get
        {
            return -chunkRenderNumber.y * numberOfVerticesPerLine * distanceBetweenVertex;
        }
    }
    public float mapMaxHeight
    {
        get
        {
            return chunkRenderNumber.y * numberOfVerticesPerLine * distanceBetweenVertex;
        }
    }
}
