/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	public const int numItemSlots = 2;

	[System.Serializable]
	public struct ItemStruct
	{
		public Image itemImage;
		[HideInInspector]public Item item;
	}
	public ItemStruct[] items = new ItemStruct[numItemSlots];

	[HideInInspector] public int itemCount = 0;
	public void AddItem(Item itemToAdd)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if(items[i].item == null)
			{
				items[i].item = itemToAdd;
				items[i].itemImage.sprite = itemToAdd.sprite;
				items[i].itemImage.enabled = true;
				itemCount++;
				return;
			}
		}
	}

	public void RemoveItem(Item itemToRemove)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].item == itemToRemove)
			{
				items[i].item = null;
				items[i].itemImage.sprite = null;
				items[i].itemImage.enabled = false;
				itemCount--;
				return;
			}
		}
	}

	public Item GetItem()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if(items[i].item != null)
			{
				return items[i].item;
			}
		}
		return null;
	}
}
