using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MarchingCubeGenerator : MonoBehaviour
{
    public Vector3 chunkRenderNumber;

    public NoiseSetting noiseSetting;
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    [Range(0, 1)]
    public float ignoreVertexLevel;
    public AnimationCurve heightCurve;
    public Material terrainMaterial;

    public Transform playerTransform;

    Dictionary<Vector3, GameObject> marchingCubeChunkDictionary;

    private void Awake()
    {
        marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
    }

    private void Start()
    {
        //GenerateChunks(Vector3.zero);
    }

    private void Update()
    {
        GenerateChunks(playerTransform.position);
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
                    float meshDistance = distanceBetweenVertex * (numberOfVerticesPerLine);
                    Vector3 offset = new Vector3(0,0,0);
                    offset.x = Mathf.RoundToInt(center.x / meshDistance) * meshDistance;
                    offset.y = 0;
                    offset.z = Mathf.RoundToInt(center.z / meshDistance) * meshDistance;
                    Vector3 centerOfMarchingCube = new Vector3(x, y, z) * meshDistance + offset;
                    if (!marchingCubeChunkDictionary.ContainsKey(centerOfMarchingCube))
                    {
                        StartCoroutine(IEGenerateChunk(centerOfMarchingCube));
                    }
                }
            }
        }
    }

    public void DeleteMarchingCubeObject()
    {
        if (marchingCubeChunkDictionary == null)
        {
            marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        }

        if (marchingCubeChunkDictionary.Count != 0)
        {
            foreach (KeyValuePair<Vector3, GameObject> go in marchingCubeChunkDictionary)
            {
                DestroyImmediate(go.Value);
            }

            marchingCubeChunkDictionary.Clear();
        }
    }

    struct MapDataThreadInfo
    {
        public readonly Action<MeshData> callback;
        public readonly MeshData parameter;

        public MapDataThreadInfo(Action<MeshData> callback, MeshData parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public struct MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;

        public MeshData(Vector3[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }

    IEnumerator IEGenerateChunk(Vector3 center)
    {
        MarchingCubeChunk marchingCubeChunk = new MarchingCubeChunk();
        float mapMinHeight = -chunkRenderNumber.y * numberOfVerticesPerLine * distanceBetweenVertex;
        float mapMaxHeight = chunkRenderNumber.y * numberOfVerticesPerLine * distanceBetweenVertex;
        marchingCubeChunkDictionary.Add(center, marchingCubeChunk.GenerateChunk(this.transform, terrainMaterial, center, numberOfVerticesPerLine, distanceBetweenVertex, ignoreVertexLevel, heightCurve, mapMinHeight, mapMaxHeight, noiseSetting));
        yield return null;
    }
}
