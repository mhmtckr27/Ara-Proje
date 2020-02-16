/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
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
}
