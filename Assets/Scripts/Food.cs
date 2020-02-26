/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using UnityEngine;
using UnityEngine.Events;

public class Food : MonoBehaviour
{
	[HideInInspector] public UnityEvent onDestroyEvent = new UnityEvent();
	[SerializeField] public Item item;
	[SerializeField] private Animal animal;

	//unit is kcal
	private const float kcalPerFatGram = 9;
	private const float kcalPerProteinGram = 4;
	private const float kcalPerCarbohydrateGram = 3.75f;
	private const float gramsPerKilogram = 1000f;
	//ekosistemi dengelemek icin gerekli.
	private const float energyBalanceMultiplier = .5f;
	private const float moistureBalanceMultiplier = .075f;
	[HideInInspector] public float energyValue;
	[HideInInspector] public float nutritionValue;
	[HideInInspector] public float moistureValue;
	[Range(0, 1)] public float fatPercentage;
	[Range(0, 1)] public float proteinPercentage;
	[Range(0, 1)] public float carbPercentage;
	public float proteinWeight;
	public float carbWeight;
	public float fatWeight;
	[Range(0, 1)] public float moisturePercentage;
	public float weightInKg;
	public float reproductiveUrgeIncrementValue;
	[Range(0, 1)] public float ediblePercentage;
	public float edibleWeight;
	public float carcassDestroyWaitThreshold;
	public float nutritionPercentage;

	//TODO: minimum costla hesapla.
	private void OnEnable()
	{
		float totalKcalFromFats = fatPercentage * kcalPerFatGram;
		float totalKcalFromProteins = proteinPercentage * kcalPerProteinGram;
		float totalKcalFromCarbohydrates = carbPercentage * kcalPerCarbohydrateGram;
		edibleWeight = ediblePercentage * weightInKg * gramsPerKilogram;
		energyValue = (totalKcalFromCarbohydrates + totalKcalFromFats + totalKcalFromProteins) * ediblePercentage * weightInKg * gramsPerKilogram * energyBalanceMultiplier;
		nutritionValue = weightInKg * ediblePercentage * (fatPercentage + proteinPercentage + carbPercentage) * gramsPerKilogram;
		moistureValue = weightInKg * ediblePercentage * moisturePercentage * gramsPerKilogram * moistureBalanceMultiplier;
		carbWeight = edibleWeight * carbPercentage;
		proteinWeight = edibleWeight * proteinPercentage;
		fatWeight = edibleWeight * fatPercentage;
		nutritionPercentage = fatPercentage + proteinPercentage + carbPercentage;
	}

	internal void UpdateNutritionValues(float carbPercentage, float proteinPercentage, float fatPercentage, float edibleWeight)
	{
		proteinWeight += edibleWeight * proteinPercentage;
		carbWeight += edibleWeight * carbPercentage;
		fatWeight += edibleWeight * fatPercentage;
	}

	private void OnDestroy()
	{
		onDestroyEvent.Invoke();
	}

	public void GetEaten()
	{
		Destroy(gameObject);
	}
}
