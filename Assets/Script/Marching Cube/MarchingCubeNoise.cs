//
// Perlin noise generator for Unity
// Keijiro Takahashi, 2013, 2015
// https://github.com/keijiro/PerlinNoise
//
// Based on the original implementation by Ken Perlin
// http://mrl.nyu.edu/~perlin/noise/
//
using UnityEngine;

public static class MarchingCubeNoise
{
    public static float GenerateTerrainNoise(Vector3 point, NoiseSetting setting)
    {
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        float returnNoise = 0;



        for(int i=0;i< setting.octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= setting.persistance;
        }

        if(setting.noiseScale <= 0)
        {
            setting.noiseScale = 0.0001f;
        }

        amplitude = 1;

        for(int i=0;i<setting.octaves;i++)
        {
            Vector3 sample = (point / setting.noiseScale) * frequency;
            float perlinValue = Perlin3D(sample);

            returnNoise += perlinValue * amplitude;

            amplitude *= setting.persistance;
            frequency *= setting.lacunarity;
        }

        return returnNoise / maxPossibleHeight;
    }

    public static float Perlin3D(float x, float y, float z)
    {
        return (PerlinNoise.Noise(x, y, z) + 1) / 2;
    }

    public static float Perlin3D(Vector3 xyz)
    {
        return Perlin3D(xyz.x, xyz.y, xyz.z);
    }
}

[System.Serializable]
public struct NoiseSetting
{
    public int seed;
    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
}