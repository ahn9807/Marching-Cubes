using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MarchingCubeGenerator : MonoBehaviour
{
    public NoiseSetting noiseSetting;
    public MarchingCubeChunkSetting chunkSetting;
    public MarchingCubeBiome biomeSetting;

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

        marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        marchingCubeChunkDictionary.Clear();

        chunkSetting.parent = this.transform;
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

        for (int x = -(int)chunkSetting.chunkRenderNumber.x; x <= (int)chunkSetting.chunkRenderNumber.x; x++)
        {
            for (int y = -(int)chunkSetting.chunkRenderNumber.y; y <= (int)chunkSetting.chunkRenderNumber.y; y++)
            {
                for (int z = -(int)chunkSetting.chunkRenderNumber.z; z <= (int)chunkSetting.chunkRenderNumber.z; z++)
                {
                    float meshDistance = chunkSetting.distanceBetweenVertex * (chunkSetting.numberOfVerticesPerLine);
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

                        meshData.cubes = MarchingCubeChunk.InitVertices(centerOfMarchingCube, chunkSetting, noiseSetting);
                        meshData.vertices = MarchingCubeChunk.GenerateVertices(meshData.cubes);
                        meshData.triangles = MarchingCubeChunk.GenerateTriangles(meshData.vertices);
                        Mesh mesh = new Mesh
                        {
                            vertices = meshData.vertices,
                            triangles = meshData.triangles
                        };
                        mesh.SetUVs(2, biomeSetting.GenerateUVS(chunkSetting, meshData.vertices));
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

        for (int x = -(int)chunkSetting.chunkRenderNumber.x; x <= (int)chunkSetting.chunkRenderNumber.x; x++)
        {
            for (int y = -(int)chunkSetting.chunkRenderNumber.y; y <= (int)chunkSetting.chunkRenderNumber.y; y++)
            {
                for (int z = -(int)chunkSetting.chunkRenderNumber.z; z <= (int)chunkSetting.chunkRenderNumber.z; z++)
                {
                    float meshDistance = chunkSetting.distanceBetweenVertex * (chunkSetting.numberOfVerticesPerLine);
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

                        RequestMeshData(meshData, centerOfMarchingCube, chunkSetting, noiseSetting, MarchingCubeCallback);
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
        meshRenderer.material = chunkSetting.terrainMaterial;

        return marchingCubeParentObject;
    }

    public void MarchingCubeCallback(MeshData meshData)
    {
        meshData.SetMesh();
    }

    public void RequestMeshData(MeshData meshData, Vector3 center, MarchingCubeChunkSetting chunkSettings, NoiseSetting noiseSettings, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            ThreadMeshData(meshData, center, chunkSettings, noiseSettings, callback);
	    };

        new Thread(threadStart).Start();
    }

    void ThreadMeshData(MeshData meshData, Vector3 center, MarchingCubeChunkSetting chunkSetting, NoiseSetting noiseSetting, Action<MeshData> callback)
    {
        List<MarchingCube> cubes = MarchingCubeChunk.InitVertices(center, chunkSetting, noiseSetting);
        Vector3[] vertices = MarchingCubeChunk.GenerateVertices(cubes);
        meshData.cubes = cubes;
        meshData.vertices = vertices;
        meshData.triangles = MarchingCubeChunk.GenerateTriangles(vertices);
        meshData.uvs = biomeSetting.GenerateUVS(this.chunkSetting, vertices);

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
        public Vector2[] uvs;

        public void SetMesh()
        {
            var mesh = terrainObject.GetComponent<MeshFilter>();
            var meshCollider = terrainObject.GetComponent<MeshCollider>();
            mesh.mesh.vertices = this.vertices;
            mesh.mesh.triangles = this.triangles;
            mesh.mesh.SetUVs(2, uvs);
            meshCollider.sharedMesh = mesh.mesh;
            mesh.mesh.RecalculateNormals();
        }
    }
}
