/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
	[SerializeField] private Renderer textureRenderer;
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;
	public void DrawTexture(Texture2D texture)
	{
		textureRenderer.sharedMaterial.mainTexture = texture;
		textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
	}

	public void DrawMesh(MeshData meshData, Texture2D texture2D)
	{
		meshFilter.sharedMesh = meshData.CreateMesh();
		meshRenderer.sharedMaterial.mainTexture = texture2D;
	}
}
