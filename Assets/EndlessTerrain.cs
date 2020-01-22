/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
	public const float maxViewDistance = 450;
	public Transform viewer;
	public Material mapMaterial;
	
	public static Vector2 viewerPosition;
	private static MapGenerator mapGenerator;
	int chunkSize;
	int chunksVisibleInViewDistance;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleInLastUpdate = new List<TerrainChunk>();
	private void Start()
	{
		mapGenerator = FindObjectOfType<MapGenerator>();
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
	}

	private void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
	}

	void UpdateVisibleChunks()
	{
		for(int i = 0; i < terrainChunksVisibleInLastUpdate.Count; i++)
		{
			terrainChunksVisibleInLastUpdate[i].SetVisible(false);
		}
		terrainChunksVisibleInLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
					{
						terrainChunksVisibleInLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
					}
				}
				else
				{
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
				}
			}

		}
	}
	public class TerrainChunk
	{
		GameObject meshObj;
		Vector2 position;
		Bounds bounds;

		MapData mapData;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
		{
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObj = new GameObject("Terrain Chunk");
			meshRenderer = meshObj.AddComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshFilter = meshObj.AddComponent<MeshFilter>();

			meshObj.transform.position = positionV3;
			meshObj.transform.parent = parent;
			SetVisible(false);

			mapGenerator.RequestMapData(OnMapDataReceived);
		}

		private void OnMapDataReceived(MapData mapData)
		{
			mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
		}
		private void OnMeshDataReceived(MeshData meshData)
		{
			meshFilter.mesh = meshData.CreateMesh();
		}

		public void UpdateTerrainChunk()
		{
			float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
			SetVisible(visible);
		}

		public void SetVisible(bool visible)
		{
			meshObj.SetActive(visible);
		}
		public bool IsVisible()
		{
			return meshObj.activeSelf;
		}
	}
}
