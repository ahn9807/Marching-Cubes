using System.Collections;
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
    }

    private void Update()
    {
        GenerateChunks(playerTransform.position);

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

    public void GenerateChunks(Vector3 center)
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
                        GameObject marchingCubeParentObject = new GameObject();
                        marchingCubeParentObject.name = "marching cube parent " + center;
                        marchingCubeParentObject.transform.parent = this.transform;
                        MeshFilter meshFilter = marchingCubeParentObject.AddComponent<MeshFilter>();
                        MeshRenderer meshRenderer = marchingCubeParentObject.AddComponent<MeshRenderer>();
                        MeshCollider meshCollider = marchingCubeParentObject.AddComponent<MeshCollider>();
                        meshRenderer.material = marchinCubeChunkSettings.terrainMaterial;

                        MeshData meshData = new MeshData();
                        meshData.meshFilter = meshFilter;
                        meshData.terrainObject = marchingCubeParentObject;

                        marchingCubeChunkDictionary.Add(centerOfMarchingCube, marchingCubeParentObject);
                        RequestMeshData(meshData, centerOfMarchingCube, marchinCubeChunkSettings, noiseSetting, MarchingCubeCallback);
                    }
                }
            }
        }
    }

    public void MarchingCubeCallback(MeshData meshData)
    {
        meshData.meshFilter.mesh.vertices = meshData.vertices;
        meshData.meshFilter.mesh.triangles = meshData.triangles;
        meshData.meshFilter.mesh.RecalculateNormals();
        meshData.terrainObject.GetComponent<MeshCollider>().sharedMesh = meshData.meshFilter.mesh;
    }

    public void VerticesCallback(Vector3[] vertices)
    {
        int[] triangles = MarchingCubeChunk.GenerateTriangles(vertices);
        marchingCubeChunkDictionary[Vector3.zero].GetComponent<MeshFilter>().mesh.vertices = vertices;
        marchingCubeChunkDictionary[Vector3.zero].GetComponent<MeshFilter>().mesh.triangles = triangles;
        marchingCubeChunkDictionary[Vector3.zero].GetComponent<MeshFilter>().mesh.RecalculateNormals();
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
        //if (marchingCubeChunkDictionary == null)
        //{
        //    return;
        //}

        //if (marchingCubeChunkDictionary.Count != 0)
        //{
        //    foreach (KeyValuePair<Vector3, GameObject> go in marchingCubeChunkDictionary)
        //    {
        //        DestroyImmediate(go.Value);
        //    }

        //    marchingCubeChunkDictionary.Clear();
        //}
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
        public MeshFilter meshFilter;
        public List<MarchingCube> cubes;
        public Vector3[] vertices;
        public int[] triangles;
    }
}
