/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	public const int numItemSlots = 2;

	public Image[] itemImages = new Image[numItemSlots];
	public Item[] items = new Item[numItemSlots];
	public int itemCount = 0;
	public void AddItem(Item itemToAdd)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if(items[i] == null)
			{
				items[i] = itemToAdd;
				itemImages[i].sprite = itemToAdd.sprite;
				itemImages[i].enabled = true;
				itemCount++;
				return;
			}
		}
	}

	public void RemoveItem(Item itemToRemove)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] == itemToRemove)
			{
				items[i] = null;
				itemImages[i].sprite = null;
				itemImages[i].enabled = false;
				itemCount--;
				return;
			}
		}
	}

	public Item GetItem()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if(items[i] != null)
			{
				return items[i];
			}
		}
		return null;
	}
}
