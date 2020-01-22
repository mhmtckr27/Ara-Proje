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
		ColorMap,
		Mesh,
		FalloffMap
	}

	public Noise.NormalizeMode normalizeMode;

	public const int mapChunkSize = 239;

	[SerializeField] private DrawMode drawMode;
	[Range(0, 6)] [SerializeField] private int editorPreviewLOD;
	[SerializeField] private float scale;
	[SerializeField] private int octaves;
	[Range(0, 1)] [SerializeField] private float persistence;
	[SerializeField] private float lacunarity;
	[SerializeField] private int seed;
	[SerializeField] private Vector2 offset;
	[SerializeField] private bool applyFalloff;
	[SerializeField] private float meshHeightMultiplier;
	[SerializeField] private AnimationCurve meshHeightCurve;
	[SerializeField] private TerrainType[] regions;

	float[,] falloffMap;

	public bool autoUpdate;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
	private void Awake()
	{
		falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
	}
	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData(Vector2.zero);
		MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			mapDisplay.DrawTexture(TextureGenerator.GenerateTextureFromHeightMap(mapData.heightMap));
		}
		else if (drawMode == DrawMode.ColorMap)
		{
			mapDisplay.DrawTexture(TextureGenerator.GenerateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
		}
		else if(drawMode == DrawMode.Mesh)
		{
			mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.GenerateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
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
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
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
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, scale, octaves, persistence, lacunarity, center + offset, normalizeMode);
		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for(int y = 0; y < mapChunkSize; y++)
		{
			for(int x = 0; x < mapChunkSize; x++)
			{
				if (applyFalloff)
				{
					noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
				}
				float currentHeight = noiseMap[x, y];
				for(int i = 0; i < regions.Length; i++)
				{
					if (currentHeight >= regions[i].height)
					{
						colorMap[y * mapChunkSize + x] = regions[i].color;
					}
					else
					{
						break;
					}
				}
			}
		}

		return new MapData(noiseMap, colorMap);

	}

	private void OnValidate()
	{
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 1)
		{
			octaves = 1;
		}
		falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
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
[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}

public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;

	public MapData(float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}