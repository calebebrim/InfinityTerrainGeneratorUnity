using System.Collections;
using UnityEngine;
public static class MeshGenerator {
  public static MeshData GenerateTerrainMesh (float[, ] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail) {
    AnimationCurve _heightCurve = new AnimationCurve (heightCurve.keys);
    var width = heightMap.GetLength (0);
    var height = heightMap.GetLength (1);
    float topLeftX = (width - 1) / -2f;
    float topLeftZ = (height - 1) / 2f;

    int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
    int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;
    MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
    int vertexIndex = 0;

    for (int y = 0; y < height; y += meshSimplificationIncrement) {
      for (int x = 0; x < width; x += meshSimplificationIncrement) {
        meshData.vertices[vertexIndex] = new Vector3 (topLeftX + x, _heightCurve.Evaluate (heightMap[x, y]) * heightMultiplier, topLeftZ - y);
        meshData.uvs[vertexIndex] = new Vector2 (x / (float) width, y / (float) height);
        if (x < width - 1 && y < height - 1) {
          meshData.addTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
          meshData.addTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
        }

        vertexIndex += 1;

      }
    }
    return meshData;
  }

}
public class MeshData {
  public Vector3[] vertices;
  public int[] triangles;
  int triangleIndex = 0;

  public Vector2[] uvs;
  public MeshData (int meshWidth, int meshHeigh) {
    vertices = new Vector3[meshWidth * meshHeigh];
    uvs = new Vector2[meshWidth * meshHeigh];
    triangles = new int[(meshWidth - 1) * (meshHeigh - 1) * 6];
  }

  public void addTriangle (int a, int b, int c) {
    triangles[triangleIndex] = a;
    triangles[triangleIndex + 1] = b;
    triangles[triangleIndex + 2] = c;
    triangleIndex += 3;
  }

  public Mesh createMesh () {
    Mesh mesh = new Mesh ();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals ();
    return mesh;
  }
}