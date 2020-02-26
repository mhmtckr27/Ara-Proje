/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	public const int numFoodSlots = 2;

	[System.Serializable]
	public struct FoodStruct
	{
		public Image foodImage;
		[HideInInspector]public Food food;
	}
	public FoodStruct[] foods = new FoodStruct[numFoodSlots];

	[HideInInspector] public int itemCount = 0;
	public void AddItem(Food foodToAdd)
	{
		for (int i = 0; i < foods.Length; i++)
		{
			if(foods[i].food == null)
			{
				foods[i].food = foodToAdd;
				if(foodToAdd.item.sprite == null)
				{
					Debug.Log("nedenn");
				}
				foods[i].foodImage.sprite = foodToAdd.item.sprite;
				foods[i].foodImage.enabled = true;
				itemCount++;
				return;
			}
		}
	}

	public void RemoveItem(Food foodToRemove)
	{
		for (int i = 0; i < foods.Length; i++)
		{
			if (foods[i].food == foodToRemove)
			{
				foods[i].food = null;
				foods[i].foodImage.sprite = null;
				foods[i].foodImage.enabled = false;
				itemCount--;
				return;
			}
		}
	}

	public Food GetItem()
	{
		for (int i = 0; i < foods.Length; i++)
		{
			if(foods[i].food != null)
			{
				return foods[i].food;
			}
		}
		return null;
	}
}
