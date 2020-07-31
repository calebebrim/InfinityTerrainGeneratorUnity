using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
  public Renderer textureRender;
  public MeshFilter meshFilter;
  public MeshRenderer meshRenderer;

  public void DrawnTexture (Texture2D texture) {

    textureRender.sharedMaterial.mainTexture = texture;

    textureRender.transform.localScale = Vector3.one * 25 /*new Vector3 (texture.width, 1, texture.height)*/ ;

  }
  public void DrawnMesh (MeshData meshData, Texture2D texture) {
    meshFilter.sharedMesh = meshData.createMesh ();
    meshRenderer.sharedMaterial.mainTexture = texture;
  }

}