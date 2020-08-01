using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public enum NormalizedMode { Local, Global }
    public static float[, ] GeneratedNoiseMap (int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizedMode normalizedMode) {
        float[, ] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random (seed);
        Vector2[] octavesOffsets = new Vector2[octaves];

        float halfWidht = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        float maxPossibleHeight = 0f;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next (-100000, 100000) + offset.x;
            float offsetY = prng.Next (-100000, 100000) - offset.y;
            octavesOffsets[i] = new Vector2 (offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        if (scale <= 0) scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidht + octavesOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octavesOffsets[i].y) / scale * frequency;

                    float perlinVale = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVale * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (maxLocalNoiseHeight < noiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
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
                if (normalizedMode == NormalizedMode.Local) {
                    noiseMap[x, y] = Mathf.InverseLerp (minNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);
                    noiseMap[x, y] = Mathf.Clamp (normalizedHeight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}