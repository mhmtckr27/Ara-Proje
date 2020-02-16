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
	private void OnDestroy()
	{
		onDestroyEvent.Invoke();
	}

	public void GetEaten()
	{
		Destroy(gameObject);
	}
}
