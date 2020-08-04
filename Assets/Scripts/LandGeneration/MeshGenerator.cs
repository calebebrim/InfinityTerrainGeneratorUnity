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

  Vector3[] CalculateNormals () {
    Vector3[] vertexNormals = new Vector3[vertices.Length];
    int triangleCount = triangles.Length / 3;
    for (int i = 0; i < triangleCount; i++) {
      int normalTriangleIndex = i * 3;
      int vertexIndexA = triangles[normalTriangleIndex];
      int vertexIndexB = triangles[normalTriangleIndex + 1];
      int vertexIndexC = triangles[normalTriangleIndex + 2];

      Vector3 triangleNormal = SurfaceNormalFromIndex (vertexIndexA, vertexIndexB, vertexIndexC);
      vertexNormals[vertexIndexA] += triangleNormal;
      vertexNormals[vertexIndexB] += triangleNormal;
      vertexNormals[vertexIndexC] += triangleNormal;

    }

    for (int i = 0; i < vertexNormals.Length; i++) {
      vertexNormals[i].Normalize ();
    }
    return vertexNormals;
  }

  Vector3 SurfaceNormalFromIndex (int indexA, int indexB, int indexC) {
    Vector3 pointA = vertices[indexA];
    Vector3 pointB = vertices[indexB];
    Vector3 pointC = vertices[indexC];

    Vector3 sideAB = pointB - pointA;
    Vector3 sideAC = pointC - pointA;
    return Vector3.Cross (sideAB, sideAC).normalized;
  }
  public Mesh createMesh () {
    Mesh mesh = new Mesh ();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.normals = CalculateNormals ();
    return mesh;
  }
}