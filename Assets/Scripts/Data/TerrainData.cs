/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData
{
	public float uniformScale = 1f;

	public bool applyFalloff;
	public bool useFlatShading;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public float minHeight
	{
		get
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
		}
	}

	public float maxHeight
	{
		get
		{
			return uniformScale * meshHeightCurve.Evaluate(1) * meshHeightMultiplier;
		}
	}
}
