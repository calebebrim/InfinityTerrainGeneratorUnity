using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    const float scale = 5f;
    const float viewerMoveThresholdChunkUpdate = 25f;
    const float sqrtViewerMoveThresholdChunkUpdate = viewerMoveThresholdChunkUpdate * viewerMoveThresholdChunkUpdate;
    public LODInfo[] detailLevels;
    public static int maxViewDist;
    public Material mapMaterial;
    public Transform viewer;
    static MapGenerator mapGenerator;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    int chunksSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk> ();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk> ();
    void Start () {
        maxViewDist = Mathf.Max (detailLevels.Select (x => x.visibleDstThreshold).ToArray ());
        Debug.Log ("MaxViewDist" + maxViewDist);
        mapGenerator = FindObjectOfType<MapGenerator> ();
        chunksSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt (maxViewDist / chunksSize);
        UpdateVisibleChunks ();
    }

    void Update () {
        viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveThresholdChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks ();
        }
    }

    void UpdateVisibleChunks () {

        foreach (var chunk in terrainChunksVisibleLastUpdate) {
            chunk.setVisible (false);
        }

        terrainChunksVisibleLastUpdate.Clear ();

        int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunksSize);
        int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunksSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].updateTerrainChunk ();
                    // if (terrainChunkDictionary[viewedChunkCoord].isVisible ()) {
                    //     terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary[viewedChunkCoord]);
                    // }
                } else {
                    terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunksSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }
    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        public TerrainChunk (Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            position = coord * size;
            Vector3 positionV3 = new Vector3 (position.x, 0, position.y);
            this.detailLevels = detailLevels;
            bounds = new Bounds (position, Vector2.one * size);
            // meshObject = GameObject.CreatePrimitive (PrimitiveType.Plane);
            meshObject = new GameObject ("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer> ();
            meshFilter = meshObject.AddComponent<MeshFilter> ();

            meshObject.transform.parent = parent;
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.localScale = Vector3.one * scale;

            meshRenderer.material = material;
            setVisible (false);
            lodMeshes = detailLevels.Select (x => new LODMesh (x.lod, updateTerrainChunk)).ToArray ();
            mapGenerator.RequestMapData (position, OnMapDataReceived);
        }

        void OnMapDataReceived (MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap (mapData.colourMap, mapGenerator.mapChunkSize, mapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            updateTerrainChunk ();
        }

        public void updateTerrainChunk () {
            if (mapDataReceived) {
                float viewerDstFromNearestEdgePosition = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
                bool visible = viewerDstFromNearestEdgePosition < maxViewDist;
                if (visible) {
                    var lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++) {
                        if (viewerDstFromNearestEdgePosition > detailLevels[i].visibleDstThreshold) {
                            lodIndex = i + 1;
                        } else {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex) {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh) {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        } else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh (mapData);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add (this);
                }
                setVisible (visible);
            }
        }

        public void setVisible (bool visible) {
            meshObject.SetActive (visible);
        }
        public bool isVisible () {
            return meshObject.activeSelf;
        }
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;
        public LODMesh (int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void onMeshDataReceived (MeshData meshdata) {
            mesh = meshdata.createMesh ();
            hasMesh = true;

            updateCallback ();
        }
        public void RequestMesh (MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData (mapData, lod, onMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public int visibleDstThreshold;
    }
}