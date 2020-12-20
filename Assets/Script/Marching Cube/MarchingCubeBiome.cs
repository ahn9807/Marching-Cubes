using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class MarchingCubeBiome : ScriptableObject
{
    public Biome[] biomes;

    public Vector2[] GenerateUVS(MarchingCubeChunkSetting settings, Vector3[] vertices)
    {
        Vector2[] returnUVS = new Vector2[vertices.Length];

        for(int v=0;v<vertices.Length;v++)
        {
            float height = vertices[v].y;
            float height01 = Mathf.Lerp(1, 0, (settings.mapMaxHeight - height) / (settings.mapMaxHeight - settings.mapMinHeight));
            int textureIndex = 0;
            for (int b = 0; b < biomes.Length; b++)
            {
                if(biomes[b].startHeight <= height01)
                {
                    textureIndex = b;
                }
            }
            returnUVS[v] = new Vector2(textureIndex, textureIndex);
        }

        return returnUVS;
    }
}

[System.Serializable]
public class Biome
{
    public Texture2D texture;
    [Range(0,1)]
    public float startHeight;
    public float textureScale;
}
