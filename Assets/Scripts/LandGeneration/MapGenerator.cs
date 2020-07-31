using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawnMode { NoiseMap, ColourMap, Mesh }

    public DrawnMode drawnMode;
    public int mapChunkSize = 241;
    [Range (0, 6)]
    public int editorPreviewLOD;
    public float noiseScale;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public int octaves;
    [Range (0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public bool autoUpdate;
    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQ = new Queue<MapThreadInfo<MapData>> ();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQ = new Queue<MapThreadInfo<MeshData>> ();
    public void DrawnMapInEditor () {
        MapDisplay display = FindObjectOfType<MapDisplay> ();
        MapData mapdata = GenerateMapData (Vector2.zero);
        if (drawnMode == DrawnMode.NoiseMap) {
            display.DrawnTexture (TextureGenerator.TextureFromHeightMap (mapdata.heightMap));
        } else
        if (drawnMode == DrawnMode.ColourMap) {
            display.DrawnTexture (TextureGenerator.TextureFromColourMap (mapdata.colourMap, mapChunkSize, mapChunkSize));
        } else if (drawnMode == DrawnMode.Mesh) {
            display.DrawnMesh (MeshGenerator.GenerateTerrainMesh (mapdata.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap (mapdata.colourMap, mapChunkSize, mapChunkSize));
        }
    }
    public void RequestMapData (Vector2 center, Action<MapData> callback) {
        ThreadStart thread = delegate {
            MapDataThread (center, callback);
        };
        new Thread (thread).Start ();
    }
    void MapDataThread (Vector2 center, Action<MapData> callback) {
        MapData mapData = GenerateMapData (center);
        lock (mapDataThreadInfoQ) {
            mapDataThreadInfoQ.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
        }
    }
    public void RequestMeshData (MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart thread = delegate {
            MeshDataThread (mapData, lod, callback);
        };
        new Thread (thread).Start ();
    }

    void MeshDataThread (MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQ) {
            meshDataThreadInfoQ.Enqueue (new MapThreadInfo<MeshData> (callback, meshData));
        }
    }

    void call<T> (MapThreadInfo<T> threadInfo) {
        threadInfo.callback (threadInfo.parameter);
    }
    void Update () {
        var queueSize = mapDataThreadInfoQ.Count;
        for (int i = 0; i < queueSize; i++) {
            call (mapDataThreadInfoQ.Dequeue ());

        }

        queueSize = meshDataThreadInfoQ.Count;
        for (int i = 0; i < queueSize; i++) {
            call (meshDataThreadInfoQ.Dequeue ());

        }
    }

    MapData GenerateMapData (Vector2 center) {
        float[, ] noiseMap = Noise.GeneratedNoiseMap (mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistance, lacunarity, center + offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        return new MapData (noiseMap, colourMap);
    }

    void onValidate () {

        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter) {
            this.parameter = parameter;
            this.callback = callback;
        }

    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
    public readonly float[, ] heightMap;
    public readonly Color[] colourMap;
    public MapData (float[, ] heightMap, Color[] colourMap) {
        this.colourMap = colourMap;
        this.heightMap = heightMap;
    }
}