/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using System.Collections;
using UnityEngine;

public class RedFox : Animal, ICanStockFood
{
	[SerializeField] public Inventory inventory;
	public Item rabbitItem;
	[SerializeField] public float bitePeriod;
	[SerializeField] public float biteWeight;
	public override void PriorityExplore()
	{
		Food foodFound = FindFood();
		if(foodFound != null)
		{
			navMeshAgent.SetDestination(foodFound.transform.position);

			if (TargetNear(foodFound.gameObject))
			{
				StockFood(foodFound);
			}
		}
		else if (foodFound == null)
		{
			Explore();
		}
	}

	public void StockFood(Food foodFound)
	{
		inventory.AddItem(foodFound.item);
		foodFound.GetEaten();
	}

	
	//if we have a food in our stock/inventory, eat it. Otherwise call the base script's
	//PriorityFood function to find another food around us.
	protected override bool PriorityFood()
	{
		Item currentFoodItem = inventory.GetItem();
		if(currentFoodItem != null)
		{
			inventory.RemoveItem(currentFoodItem);
			EatFood(currentFoodItem);
			return true;
		}
		return base.PriorityFood();
	}

	public void EatFood(Item foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water.
		//readyTime2 = readyTime1;
		isReady = false;
		CurrentFoodIntake += foodFound.nutritionValue;
		EnergySaturation += foodFound.energyValue;
		CurrentWaterIntake += foodFound.moistureValue;
	}


	protected override void EatFood(Food foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water.
		//readyTime2 = readyTime1;
		//isReady = false;
		currentState = State.Eating;
		objectsDictionary.Remove(foodFound.tag);
		//TODO: replace placeholder with fancy effects and proper carcass model.
		GameObject carcass = GameObject.CreatePrimitive(PrimitiveType.Cube);
		carcass.transform.position = foodFound.transform.position;
		carcass.transform.localScale = Vector3.one * .2f;

		Item carcassItem = foodFound.item;
		foodFound.GetEaten();
		StartCoroutine(EatFoodProcess(carcass, carcassItem));
	}

	IEnumerator EatFoodProcess(GameObject carcass, Item carcassItem)
	{
		int bitesToFinish = (int) (carcassItem.edibleWeight / biteWeight);
		while (bitesToFinish > 0)
		{
			float oldIntake = CurrentFoodIntake + CurrentWaterIntake;
			StartCoroutine(SwallowBiteProcess(carcass, carcassItem));
			//son lokmayi yedikten sonra beklemesine gerek yok.
			yield return new WaitForSeconds(bitePeriod * (bitesToFinish != 1 ? 1 : 0));
			bitesToFinish--;
		}
		currentState = State.EatingDone;

	}

	IEnumerator SwallowBiteProcess(GameObject carcass, Item carcassItem)
	{
		float remainingBiteToSwallow = biteWeight;
		float biteDelay = Time.fixedDeltaTime;
		while (remainingBiteToSwallow > 0)
		{
			yield return new WaitForSeconds(biteDelay);

			CurrentFoodIntake += carcassItem.nutritionValue * biteDelay;
			CurrentEnergy += carcassItem.energyValue * biteDelay;
			CurrentWaterIntake += carcassItem.moistureValue * biteDelay;

			carcass.transform.localScale -= carcass.transform.localScale * biteDelay;
			remainingBiteToSwallow -= carcassItem.edibleWeight * biteDelay;
		}
	}
}
