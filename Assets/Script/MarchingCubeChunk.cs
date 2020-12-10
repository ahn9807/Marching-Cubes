using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubeChunk : MonoBehaviour
{
    public Vector3 chunkRenderNumber;

    public NoiseSetting noiseSetting;
    public int numberOfVerticesPerLine;
    public float distanceBetweenVertex;
    [Range(0,1)]
    public float ignoreVertexLevel;
    public Material terrainMaterial;

    Dictionary<Vector3, GameObject> marchingCubeChunkDictionary;

    private void Awake()
    {
        marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
    }

    private void Start()
    {

    }

    public void GenerateChunks()
    {
        if(marchingCubeChunkDictionary == null)
        {
            marchingCubeChunkDictionary = new Dictionary<Vector3, GameObject>();
        }

        DeleteMarchingCubeObject();

        for (int x = -(int)chunkRenderNumber.x; x <= (int)chunkRenderNumber.x; x++)
        {
            for (int y = -(int)chunkRenderNumber.y; y <= (int)chunkRenderNumber.y; y++)
            {
                for (int z = -(int)chunkRenderNumber.z; z <= (int)chunkRenderNumber.z; z++)
                {
                    Vector3 centerOfMarchingCube = new Vector3(x,y,z) * distanceBetweenVertex * numberOfVerticesPerLine;
                    marchingCubeChunkDictionary.Add(centerOfMarchingCube, GenerateChunk(centerOfMarchingCube));
                }
            }
        }
    }

    public void DeleteMarchingCubeObject()
    {
        if (marchingCubeChunkDictionary.Count != 0)
        {
            foreach (KeyValuePair<Vector3, GameObject> go in marchingCubeChunkDictionary)
            {
                DestroyImmediate(go.Value);
            }

            marchingCubeChunkDictionary.Clear();
        }
    }

    public GameObject GenerateChunk(Vector3 center)
    {
        if (marchingCubeChunkDictionary.ContainsKey(center))
        {
            //do nothing
            return null;
        }

        MarchingCube marchingCube = new MarchingCube();
        marchingCube.InitVertices(center, numberOfVerticesPerLine, distanceBetweenVertex, ignoreVertexLevel, noiseSetting);
        GameObject marchingCubeParentObject = new GameObject();
        marchingCubeParentObject.name = "marching cube parent " + center;
        marchingCubeParentObject.transform.parent = this.transform;
        MeshFilter meshFilterOfParent = marchingCubeParentObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderOfParent = marchingCubeParentObject.AddComponent<MeshRenderer>();

        meshFilterOfParent.mesh = marchingCube.CalculateMesh();
        meshRenderOfParent.material = terrainMaterial;

        return marchingCubeParentObject;
    }
}
