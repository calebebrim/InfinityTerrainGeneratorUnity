using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public static float[, ] GeneratedNoiseMap (int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[, ] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random (seed);
        Vector2[] octavesOffsets = new Vector2[octaves];

        float halfWidht = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next (-100000, 100000) + offset.x;
            float offsetY = prng.Next (-100000, 100000) - offset.y;
            octavesOffsets[i] = new Vector2 (offsetX, offsetY);

        }
        if (scale <= 0) scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidht + octavesOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octavesOffsets[i].y) / scale * frequency;

                    float perlinVale = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVale * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (maxNoiseHeight < noiseHeight) {
                    maxNoiseHeight = noiseHeight;
                    // Debug.Log("Max value changed to: "+maxNoiseHeight);
                } else if (minNoiseHeight > noiseHeight) {
                    minNoiseHeight = noiseHeight;
                    // Debug.Log("Min value changed to: " + maxNoiseHeight);
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}