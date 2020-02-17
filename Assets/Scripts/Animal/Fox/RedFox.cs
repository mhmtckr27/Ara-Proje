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

	//Eat from stock
	//TODO: replace with EatFood(Food)
	public void EatFood(Item foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water.
		//readyTime2 = readyTime1;
		isReady = false;
		//CurrentFoodIntake += foodFound.nutritionValue;
		//EnergySaturation += foodFound.energyValue;
		//CurrentWaterIntake += foodFound.moistureValue;
	}

	#region Food Stuff
	protected override void EatFood(Food foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water. OR do we really need a timer? becuz hayvan zaten yavas yavas yiyor yemegini amk.
		currentState = State.Eating;
		objectsDictionary.Remove(foodFound.tag);
		//TODO: replace placeholder with fancy effects and proper carcass model.
		GameObject carcass = GameObject.CreatePrimitive(PrimitiveType.Cube);
		carcass.transform.position = foodFound.transform.position;
		carcass.transform.localScale = Vector3.one * .2f;
		Food carcassFood = foodFound;
		foodFound.GetEaten();
		StartCoroutine(EatFoodProcess(carcass, carcassFood));
	}
	#endregion
}
