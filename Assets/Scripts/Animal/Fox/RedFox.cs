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
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Color targetRed;
	public Item rabbitItem;
	public const float turnRedUpperLimit = 1 / 12f;

	public override void PriorityExplore()
	{
		Food foodFound = FindFood();
		if (foodFound != null)
		{
			navMeshAgent.SetDestination(foodFound.transform.position);

			if (TargetNear(foodFound.gameObject))
			{
				StockFood(foodFound);
			}
		}
		else if (foodFound == null)
		{
			navMeshAgent.speed = TrotSpeed;
			Explore();
		}
	}

	//public override void FixedUpdate()
	//{
	//	base.FixedUpdate();
	//	//StartCoroutine(TurnRedDelayed());
	//}

	private IEnumerator TurnRedDelayed()
	{ 
		while(meshRenderer.material.color != targetRed)
		{
			yield return new WaitForSeconds(1);
			TurnRed();
		}
		Debug.LogError("bitti");
	}

	private void TurnRed()
	{
		meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, targetRed, 0.0006f);
	}

	public void StockFood(Food foodFound)
	{
		inventory.AddItem(foodFound);
		foodFound.GetEaten();
	}


	//if we have a food in our stock/inventory, eat it. Otherwise call the base script's
	//PriorityFood function to find another food around us.
	protected override bool PriorityFood()
	{
		Food currentFood = inventory.GetItem();
		if (currentFood != null)
		{
			inventory.RemoveItem(currentFood);
			EatFood(currentFood);
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

	protected override void DetermineState()
	{
		if (currentState == State.Eating)
		{
			if (FoodSaturation >= changePriorityLimit)
			{
				if(swallowBiteProcess != null)
				{
					StopCoroutine(swallowBiteProcess);
					StopCoroutine(eatFoodProcess);
				}
				inventory.AddItem(carcassFood);
				Destroy(carcass);
				currentState = State.EatingDone;
			}
			return;
		}
		else if (currentState == State.ExploringStarted)
		{
			navMeshAgent.speed = TrotSpeed;
			currentState = State.Exploring;
		}
		else if (currentState == State.GoingToFood)
		{
			navMeshAgent.speed = RunSpeed;
		}
		else if (currentState == State.EatingDone)
		{
		}
	}
	Food carcassFood;
	GameObject carcass;

	#region Food Stuff
	protected override void EatFood(Food foodFound)
	{
		//TODO: properly implement timer to eat or drink next for preventing overeat/drink and rushing food or water. OR do we really need a timer? becuz hayvan zaten yavas yavas yiyor yemegini amk.
		currentState = State.Eating;
		objectsDictionary.Remove(foodFound.tag);
		//TODO: replace placeholder with fancy effects and proper carcass model.
		carcass = GameObject.CreatePrimitive(PrimitiveType.Cube);
		carcass.transform.position = foodFound.transform.position;
		carcass.transform.localScale = Vector3.one * .2f;
		carcassFood = foodFound;
		foodFound.GetEaten();
		eatFoodProcess = StartCoroutine(EatFoodProcess(carcass, carcassFood));
	}
	#endregion
}
