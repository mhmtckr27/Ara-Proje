/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{

	const float viewerMovementThresholdForChunkUpdate = 25f;
	const float sqrViewerMovementThresholdForChunkUpdate = viewerMovementThresholdForChunkUpdate * viewerMovementThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDistance;

	public Transform viewer;
	public Material mapMaterial;
	
	public static Vector2 viewerPosition;
	private Vector2 viewerPositionOld;
	private static MapGenerator mapGenerator;
	int chunkSize;
	int chunksVisibleInViewDistance;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunksVisibleInLastUpdate = new List<TerrainChunk>();
	private void Start()
	{
		mapGenerator = FindObjectOfType<MapGenerator>();
		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

		UpdateVisibleChunks();
	}

	private void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;
		if((viewerPositionOld- viewerPosition).sqrMagnitude > sqrViewerMovementThresholdForChunkUpdate)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
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
				}
				else
				{
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}
			}

		}
	}
	public class TerrainChunk
	{
		GameObject meshObj;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		MeshCollider meshCollider;

		LODInfo[] detailLevels;
		LevelOfDetailMesh[] lodMeshes;
		LevelOfDetailMesh collisionLODMesh;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
		{
			this.detailLevels = detailLevels;
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObj = new GameObject("Terrain Chunk");
			meshRenderer = meshObj.AddComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshFilter = meshObj.AddComponent<MeshFilter>();
			meshCollider = meshObj.AddComponent<MeshCollider>();

			meshObj.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
			meshObj.transform.parent = parent;
			meshObj.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
			SetVisible(false);

			lodMeshes = new LevelOfDetailMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LevelOfDetailMesh(detailLevels[i].lod, UpdateTerrainChunk);
				if (detailLevels[i].useForCollider)
				{
					collisionLODMesh = lodMeshes[i];
				}
			}

			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}

		private void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			mapDataReceived = true;

			Texture2D texture = TextureGenerator.GenerateTextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}
		public void UpdateTerrainChunk()
		{
			if (mapDataReceived)
			{
				float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

				if (visible)
				{
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++)
					{
						if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
						{
							lodIndex = i + 1;
						}
						else
						{
							break;
						}
					}
					if (lodIndex != previousLODIndex)
					{
						LevelOfDetailMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh)
						{
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh)
						{
							lodMesh.RequestMesh(mapData);
						}
					}
					if(lodIndex == 0)
					{
						if (collisionLODMesh.hasMesh)
						{
							meshCollider.sharedMesh = collisionLODMesh.mesh;
						}
						else if (!collisionLODMesh.hasRequestedMesh)
						{
							collisionLODMesh.RequestMesh(mapData);
						}
					}
					terrainChunksVisibleInLastUpdate.Add(this);
				}

				SetVisible(visible);
			}
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

	private class LevelOfDetailMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		private int levelOfDetail;
		System.Action updateCallback;

		public LevelOfDetailMesh(int levelOfDetail, System.Action updateCallback)
		{
			this.levelOfDetail = levelOfDetail;
			this.updateCallback = updateCallback;
		}

		private void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallback();
		}

		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);
		}
	}

	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDistanceThreshold;
		public bool useForCollider;
	}
}
