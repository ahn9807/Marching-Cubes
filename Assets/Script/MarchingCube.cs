using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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