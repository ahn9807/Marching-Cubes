﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MarchingCubeGenerator : MonoBehaviour
{
    public Vector3 chunkRenderNumber;

    public NoiseSetting noiseSetting;
    public MarchinCubeChunkSettings marchinCubeChunkSettings;

    public Transform playerTransform;

    Dictionary<Vector3, GameObject> marchingCubeChunkDictionary;

    Queue<GeneratorThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<GeneratorThreadInfo<MeshData>>();

    private void Start()
    {
        Initialize();
        GenerateChunksAtThread(playerTransform.position);
    }

    private void Update()
    {
        GenerateChunksAtThread(playerTransform.position);

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for(int i=0;i<meshDataThreadInfoQueue.Count;i++)
            {
                GeneratorThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public void Initialize()
    {
        DeleteMarchingCubeObject();

        float mapMinHeight = -chunkRenderNumber.y * marchinCubeChunkSettings.numberOfVerticesPerLine * marchinCubeChunkSettings.distanceBetweenVertex;
        float mapMaxHeight = chunkRenderNumber.y * marchinCubeChunkSettings.numberOfVerticesPerLine * marchinCubeChunkSettings.distanceBetweenVertex;

        marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        marchingCubeChunkDictionary.Clear();

        marchinCubeChunkSettings.parent = this.transform;
        marchinCubeChunkSettings.mapMaxHeight = mapMaxHeight;
        marchinCubeChunkSettings.mapMinHeight = mapMinHeight;
    }

    //This method is INTENDED TO MAKE CHUNK AT EDITOR, EXTERAMLY SLOW AT IN GAME
    public void GenerateChunksAtMain(Vector3 center)
    {
        if (marchingCubeChunkDictionary == null)
        {
            marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        }

        int xOffset = (int)center.x;
        int zOffset = (int)center.z;

        for (int x = -(int)chunkRenderNumber.x; x <= (int)chunkRenderNumber.x; x++)
        {
            for (int y = -(int)chunkRenderNumber.y; y <= (int)chunkRenderNumber.y; y++)
            {
                for (int z = -(int)chunkRenderNumber.z; z <= (int)chunkRenderNumber.z; z++)
                {
                    float meshDistance = marchinCubeChunkSettings.distanceBetweenVertex * (marchinCubeChunkSettings.numberOfVerticesPerLine);
                    Vector3 offset = new Vector3(0, 0, 0);
                    offset.x = Mathf.RoundToInt(center.x / meshDistance) * meshDistance;
                    offset.y = 0;
                    offset.z = Mathf.RoundToInt(center.z / meshDistance) * meshDistance;
                    Vector3 centerOfMarchingCube = new Vector3(x, y, z) * meshDistance + offset;
                    if (!marchingCubeChunkDictionary.ContainsKey(centerOfMarchingCube))
                    {
                        GameObject marchingCubeParentObject = GenerateChunkObject(centerOfMarchingCube);
                        marchingCubeChunkDictionary.Add(centerOfMarchingCube, marchingCubeParentObject);

                        //Make mesh without threading
                        MeshData meshData = new MeshData();
                        meshData.terrainObject = marchingCubeParentObject;

                        meshData.cubes = MarchingCubeChunk.InitVertices(centerOfMarchingCube, marchinCubeChunkSettings, noiseSetting);
                        meshData.vertices = MarchingCubeChunk.GenerateVertices(meshData.cubes);
                        meshData.triangles = MarchingCubeChunk.GenerateTriangles(meshData.vertices);
                        Mesh mesh = new Mesh();
                        mesh.vertices = meshData.vertices;
                        mesh.triangles = meshData.triangles;
                        mesh.RecalculateNormals();
                        meshData.terrainObject.GetComponent<MeshCollider>().sharedMesh = mesh;
                        meshData.terrainObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                    }
                }
            }
        }
    }

    public void GenerateChunksAtThread(Vector3 center)
    {
        if (marchingCubeChunkDictionary == null)
        {
            marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        }

        int xOffset = (int)center.x;
        int zOffset = (int)center.z;

        for (int x = -(int)chunkRenderNumber.x; x <= (int)chunkRenderNumber.x; x++)
        {
            for (int y = -(int)chunkRenderNumber.y; y <= (int)chunkRenderNumber.y; y++)
            {
                for (int z = -(int)chunkRenderNumber.z; z <= (int)chunkRenderNumber.z; z++)
                {
                    float meshDistance = marchinCubeChunkSettings.distanceBetweenVertex * (marchinCubeChunkSettings.numberOfVerticesPerLine);
                    Vector3 offset = new Vector3(0, 0, 0);
                    offset.x = Mathf.RoundToInt(center.x / meshDistance) * meshDistance;
                    offset.y = 0;
                    offset.z = Mathf.RoundToInt(center.z / meshDistance) * meshDistance;
                    Vector3 centerOfMarchingCube = new Vector3(x, y, z) * meshDistance + offset;
                    if (!marchingCubeChunkDictionary.ContainsKey(centerOfMarchingCube)) {
                        GameObject marchingCubeParentObject = GenerateChunkObject(centerOfMarchingCube);
                        marchingCubeChunkDictionary.Add(centerOfMarchingCube, marchingCubeParentObject);

                        MeshData meshData = new MeshData();
                        meshData.terrainObject = marchingCubeParentObject;

                        RequestMeshData(meshData, centerOfMarchingCube, marchinCubeChunkSettings, noiseSetting, MarchingCubeCallback);
                    }
                }
            }
        }
    }

    public GameObject GenerateChunkObject(Vector3 center)
    {
        GameObject marchingCubeParentObject = new GameObject();
        marchingCubeParentObject.name = "marching cube parent " + center;
        marchingCubeParentObject.transform.parent = this.transform;
        marchingCubeParentObject.AddComponent<MeshFilter>();
        marchingCubeParentObject.AddComponent<MeshCollider>();

        MeshRenderer meshRenderer = marchingCubeParentObject.AddComponent<MeshRenderer>();
        meshRenderer.material = marchinCubeChunkSettings.terrainMaterial;

        return marchingCubeParentObject;
    }

    public void MarchingCubeCallback(MeshData meshData)
    {
        Mesh mesh = meshData.terrainObject.GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = meshData.terrainObject.GetComponent<MeshCollider>();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    public void RequestMeshData(MeshData meshData, Vector3 center, MarchinCubeChunkSettings chunkSettings, NoiseSetting noiseSettings, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            ThreadMeshData(meshData, center, chunkSettings, noiseSettings, callback);
	    };

        new Thread(threadStart).Start();
    }

    void ThreadMeshData(MeshData meshData, Vector3 center, MarchinCubeChunkSettings chunkSettings, NoiseSetting noiseSettings, Action<MeshData> callback)
    {
        List<MarchingCube> cubes = MarchingCubeChunk.InitVertices(center, chunkSettings, noiseSettings);
        Vector3[] vertices = MarchingCubeChunk.GenerateVertices(cubes);
        meshData.cubes = cubes;
        meshData.vertices = vertices;
        meshData.triangles = MarchingCubeChunk.GenerateTriangles(vertices);

        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new GeneratorThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void DeleteMarchingCubeObject()
    {
        Transform[] cubes = transform.GetComponentsInChildren<Transform>();

        //Start from 1 to avoid self
        for(int i=1;i<cubes.Length;i++)
        {
            DestroyImmediate(cubes[i].transform.gameObject);
        }
    }

    struct GeneratorThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public GeneratorThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public class MeshData
    {
        public GameObject terrainObject;
        public List<MarchingCube> cubes;
        public Vector3[] vertices;
        public int[] triangles;
    }
}