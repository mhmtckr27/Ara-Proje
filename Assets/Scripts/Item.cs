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
	private const float gramsPerKilogram = 1000f;
	//ekosistemi dengelemek icin gerekli.
	private const float energyBalanceMultiplier = .5f;
	private const float moistureBalanceMultiplier = .15f;
	[HideInInspector] public float energyValue;
	[HideInInspector] public float nutritionValue;
	[HideInInspector] public float moistureValue;
	public Sprite sprite;
	[Range(0, 1)] public float fatPercentage;
	[Range(0, 1)] public float proteinPercentage;
	[Range(0, 1)] public float carbohydratePercentage;
	[Range(0, 1)] public float moisturePercentage;
	public float weightInKg;
	public float reproductiveUrgeIncrementValue;
	[Range(0, 1)] public float ediblePercentage;
	[HideInInspector] public float edibleWeight;

	private void OnEnable()
	{
		float totalKcalFromFats = fatPercentage * kcalPerFatGram;
		float totalKcalFromProteins = proteinPercentage * kcalPerProteinGram;
		float totalKcalFromCarbohydrates = carbohydratePercentage * kcalPerCarbohydrateGram;
		energyValue = (totalKcalFromCarbohydrates + totalKcalFromFats + totalKcalFromProteins) * ediblePercentage * weightInKg * gramsPerKilogram * energyBalanceMultiplier;
		nutritionValue = weightInKg * ediblePercentage * (fatPercentage + proteinPercentage + carbohydratePercentage) * gramsPerKilogram;
		moistureValue = weightInKg * ediblePercentage * moisturePercentage * gramsPerKilogram * moistureBalanceMultiplier;
		edibleWeight = ediblePercentage * weightInKg * gramsPerKilogram;
	}
}
