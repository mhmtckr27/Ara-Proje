﻿/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdatableData
{
	public Noise.NormalizeMode normalizeMode;
	public float noiseScale;
	public int octaves;
	[Range(0, 1)] public float persistence;
	public float lacunarity;
	public int seed;
	public Vector2 offset;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 1)
		{
			octaves = 1;
		}
		base.OnValidate();
	}
	#endif
}
