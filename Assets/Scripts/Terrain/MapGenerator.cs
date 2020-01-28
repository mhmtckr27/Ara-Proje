/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
	private enum DrawMode
	{
		NoiseMap,
		Mesh,
		FalloffMap
	}

	public NoiseData noiseData;
	public TerrainData terrainData;
	public TextureData textureData;

	public Material terrainMaterial;
	public GameObject water;

	[SerializeField] private DrawMode drawMode;
	[Range(0, 6)] [SerializeField] private int editorPreviewLOD;

	float[,] falloffMap;

	public bool autoUpdate;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
	private void Awake()
	{
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeight(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
	}
	void OnValuesUpdated()
	{
		if (!Application.isPlaying)
		{
			DrawMapInEditor();
		}
	}

	void OnTextureValuesUpdated()
	{
		textureData.ApplyToMaterial(terrainMaterial);
	}

	public int mapChunkSize
	{
		get
		{
			if (terrainData.useFlatShading)
			{
				return 95;
			}
			else
			{
				return 239;
			}
		}
	}

	public void DrawMapInEditor()
	{
		textureData.UpdateMeshHeight(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

		MapData mapData = GenerateMapData(Vector2.zero);
		MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			mapDisplay.DrawTexture(TextureGenerator.GenerateTextureFromHeightMap(mapData.heightMap));
		}
		else if(drawMode == DrawMode.Mesh)
		{
			mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading, water));
		}
		else if(drawMode == DrawMode.FalloffMap)
		{
			mapDisplay.DrawTexture(TextureGenerator.GenerateTextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
		}
	}

	public void RequestMapData(Vector2 center, Action<MapData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MapDataThread(center, callback);
		};

		new Thread(threadStart).Start();
	}

	private void MapDataThread(Vector2 center, Action<MapData> callback)
	{
		MapData mapData = GenerateMapData(center);
		//locks mapDataThreadInfoQueue untill one thread completes enqueue so 
		//prevents accessing to queue at same time which can lead to issues.
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}
	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate 
		{
			MeshDataThread(mapData, lod, callback);
		};
		new Thread(threadStart).Start();
	}
	
	private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading, water);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	private void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		if (meshDataThreadInfoQueue.Count > 0)
		{
			for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}


	private MapData GenerateMapData(Vector2 center)
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

		if (terrainData.applyFalloff)
		{
			if(falloffMap == null)
			{
				falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
			}

			for (int y = 0; y < mapChunkSize + 2; y++)
			{
				for (int x = 0; x < mapChunkSize + 2; x++)
				{
					if (terrainData.applyFalloff)
					{
						noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
					}
				}
			}
		}

		return new MapData(noiseMap);

	}

	private void OnValidate()
	{
		if(terrainData != null)
		{
			terrainData.OnValuesUpdated -= OnValuesUpdated;
			terrainData.OnValuesUpdated += OnValuesUpdated;
		}
		if(noiseData != null)
		{
			noiseData.OnValuesUpdated -= OnValuesUpdated;
			noiseData.OnValuesUpdated += OnValuesUpdated;
		}
		if(textureData != null)
		{
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
	}

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}

}

public struct MapData
{
	public readonly float[,] heightMap;

	public MapData(float[,] heightMap)
	{
		this.heightMap = heightMap;
	}
}