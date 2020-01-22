/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

[CreateAssetMenu(fileName = "New Rabbit",menuName = "Animal/Rabbit Male")]
public class RabbitData : ScriptableObject
{
	[MinMaxSlider(0,100)] public Vector2 foodSaturationRNG;
	public float foodSaturation;

	private void OnEnable()
	{
		foodSaturation = Random.Range(foodSaturationRNG.x, foodSaturationRNG.y);
	}
}
