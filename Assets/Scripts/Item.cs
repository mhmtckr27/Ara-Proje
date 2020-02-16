/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
	//unit is kcal
	private const float kcalPerFatGram = 9;
	private const float kcalPerProteinGram = 4;
	private const float kcalPerCarbohydrateGram = 3.75f;
	[HideInInspector] public float energyValue;
	[HideInInspector] public float nutritionValue;
	public Sprite sprite;
	[Range(0, 1)] public float fatPercentage;
	[Range(0, 1)] public float proteinPercentage;
	[Range(0, 1)] public float carbohydratePercentage;
	public float weightInKg;
	public float baseThirstinessValue;
	public float thirstinessValue;
	public float reproductiveUrgeIncrementValue;
	[Range(0, 1)] public float eatablePercentage;

	private void OnEnable()
	{
		float totalKcalFromFats = fatPercentage * weightInKg * kcalPerFatGram;
		float totalKcalFromProteins = proteinPercentage * weightInKg * kcalPerProteinGram;
		float totalKcalFromCarbohydrates = carbohydratePercentage * weightInKg * kcalPerCarbohydrateGram;
		energyValue = totalKcalFromCarbohydrates + totalKcalFromFats + totalKcalFromProteins;
		thirstinessValue = baseThirstinessValue + ((proteinPercentage * 1.5f) + fatPercentage + (carbohydratePercentage * 1.25f)) * weightInKg;
		nutritionValue = weightInKg * eatablePercentage;
	}
}
