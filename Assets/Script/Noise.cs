//
// Perlin noise generator for Unity
// Keijiro Takahashi, 2013, 2015
// https://github.com/keijiro/PerlinNoise
//
// Based on the original implementation by Ken Perlin
// http://mrl.nyu.edu/~perlin/noise/
//
using UnityEngine;

public static class Noise
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
        x += 100;
        y += 100;
        z += 100;

        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
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