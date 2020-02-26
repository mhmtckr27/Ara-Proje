/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System.Collections;
using UnityEngine;

public partial class Animal
{
	public Coroutine eatFoodProcess;
	public Coroutine swallowBiteProcess;
	protected virtual bool PriorityFood()
	{
		currentFood = FindFood();
		if (currentFood == null)
		{
			return false;
		}
		GoToFood(currentFood);
		if (TargetNear(currentFood.gameObject) && isReady)
		{
			EatFood(currentFood);
		}
		return true;
	}
	protected Food FindFood()
	{
		if (objectsDictionary.Count == 0)
		{
			return null;
		}
		foreach (string food in dietList)
		{
			GameObject foodFound;
			if (objectsDictionary.TryGetValue(food, out foodFound))
			{
				//TODO: kontrole gerek kalmamali
				if(foodFound!=null)
				return foodFound.GetComponent<Food>();
			}
		}
		return null;
	}
	protected void GoToFood(Food currentFood)
	{
		//TODO: excuse me, WTF? :d
		currentFood.onDestroyEvent.RemoveListener(OnPreyEaten);
		currentFood.onDestroyEvent.AddListener(OnPreyEaten);
		if (!navMeshAgent.isOnNavMesh) return;
		navMeshAgent.SetDestination(currentFood.transform.position);
		currentState = State.GoingToFood;
	}
	protected virtual void EatFood(Food foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water.
		readyTime2 = readyTime1;
		isReady = false;
		CurrentFoodIntake += foodFound.nutritionValue;
		CurrentEnergy += foodFound.energyValue;
		CurrentWaterIntake += foodFound.moistureValue;
		objectsDictionary.Remove(foodFound.tag);
		//GameObject carcass = GameObject.CreatePrimitive(PrimitiveType.Cube);
		foodFound.GetEaten();
	}
	protected IEnumerator EatFoodProcess(GameObject carcass, Food carcassFood)
	{
		int bitesToFinish = (int)(carcassFood.edibleWeight / biteWeight);
		while (bitesToFinish > 0)
		{
			float oldIntake = CurrentFoodIntake + CurrentWaterIntake;
			swallowBiteProcess = StartCoroutine(SwallowBiteProcess(carcass, carcassFood));
			//son lokmayi yedikten sonra bitePeriod beklemesine gerek yok.
			yield return new WaitForSeconds(bitePeriod * (bitesToFinish != 1 ? 1 : 0));
			bitesToFinish--;
		}
		//son lokmayi yerken destroy edilirse nullref hatasi alirsin. son lokmayi yutana kadar daha bekle ve destroy et.
		float lastBiteDelay = biteWeight / carcassFood.edibleWeight + carcassFood.carcassDestroyWaitThreshold;
		yield return new WaitForSeconds(lastBiteDelay);
		currentState = State.EatingDone;
		Destroy(carcass);
	}

	//TODO: performans kaybi yasarsan sil. status barlarin smooth olarak artmasi icin tamamen gorsel amacli bir fonksiyon.
	protected IEnumerator SwallowBiteProcess(GameObject carcass, Food carcassFood)
	{
		float remainingBiteToSwallow = biteWeight;
		float biteDelay = Time.fixedDeltaTime;
		while (remainingBiteToSwallow > 0)
		{
			yield return new WaitForSeconds(biteDelay);

			CurrentFoodIntake += carcassFood.nutritionValue * biteDelay;
			CurrentEnergy += carcassFood.energyValue * biteDelay;
			CurrentWaterIntake += carcassFood.moistureValue * biteDelay;

			carcass.transform.localScale -= carcass.transform.localScale * biteDelay;
			remainingBiteToSwallow -= carcassFood.edibleWeight * biteDelay;

			asFood.UpdateNutritionValues(carcassFood.carbPercentage, carcassFood.proteinPercentage, carcassFood.fatPercentage, carcassFood.edibleWeight * biteDelay);
		}
	}
	//if prey is eaten by either this animal or another, all animals going for that prey should stop and recalculate where to go.
	private void OnPreyEaten()
	{
		if (navMeshAgent.hasPath)
		{
			navMeshAgent.ResetPath();
		}
	}
}
