/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class Rabbit : MonoBehaviour
{
	#region Variable Declarations
	private const float maxLifeTime = 120f; //1 year = 60 sec.
	private const float maxFoodSaturation = 100f;
	private const float maxWaterSaturation = 100f;
	private const float maxReproductionUrge = 100f;
	private const float restLimit = 75f;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
	private float lifeTime;
	private float reproductionUrge;

	[SerializeField] private float foodSaturation;//tokluk
	[SerializeField] private float waterSaturation;//suya doygunluk
	[SerializeField] private float maxViewingDistance;
	#endregion

	Vector3 moveTo;
	Priority currentPriority;
	Dictionary<string,GameObject> objectsDictionary = new Dictionary<string, GameObject>();


	private void Start()
	{
		//InvokeRepeating("CalculateMovement", 0f, 1f);
	}
	private void Update()
	{
		CreateDictionary();//Scan and create scanned objects' dictionary
		DeterminePriority();
		ChooseAction();
		//Move();
	}

	private void ChooseAction()
	{
		switch (currentPriority)
		{
			case Priority.EscapeFox:
				EscapeFox();
				break;
			case Priority.FindFood:
				FindFood();
				break;
			case Priority.FindWater:
				FindWater();
				break;
			case Priority.Reproduce:
				FindMate();
				break;
			case Priority.Rest:
				Rest();
				break;
		}
	}

	private void Rest()
	{
	}

	private void FindMate()
	{
		throw new NotImplementedException();
	}

	private void FindWater()
	{
		throw new NotImplementedException();
	}

	private void FindFood()
	{
		throw new NotImplementedException();
	}

	private void EscapeFox()
	{
		GameObject closestFox;
		objectsDictionary.TryGetValue("Fox", out closestFox);
		transform.forward = transform.position - closestFox.transform.position;
		//TODO: smooth out the movement.
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f);
	}
	private bool ShouldReplace(Vector3 obj1, Vector3 obj2)
	{
		float distToObj1 = Vector3.Distance(transform.position, obj1);
		float distToObj2 = Vector3.Distance(transform.position, obj2);
		return distToObj1 > distToObj2 ? true : false;
	}

	private void CreateDictionary()
	{
		//clear previous values;
		objectsDictionary.Clear();
		List<Collider> hits = Scan.ToList();
		Debug.Log(hits.Remove(gameObject.GetComponent<Collider>()));
		
		foreach(Collider collider in hits)
		{
			if (!objectsDictionary.ContainsKey(collider.tag))
			{
				//aynı tipte varlık onceden eklenmemisse key, tagından olusturulur.
				objectsDictionary.Add(collider.tag, collider.gameObject);
			}
			else
			{
				//aynı tipte varlık onceden eklenmisse, hangisi yakınsa o bulunarak guncellenir.
				GameObject objectInDictionary;
				objectsDictionary.TryGetValue(collider.tag, out objectInDictionary);
				bool shouldReplace = ShouldReplace(objectInDictionary.transform.position, collider.gameObject.transform.position);
				if (shouldReplace)
				{
					objectsDictionary.Remove(objectInDictionary.tag);
					objectsDictionary.Add(collider.tag, collider.gameObject);
				}
			}
		}
	}
	bool IsNotSame()
	{
		return false;
	}
	private void DeterminePriority()
	{
		if (objectsDictionary.ContainsKey("Fox"))
		{
			currentPriority = Priority.EscapeFox;
			return;
		}

		float maxPriority = Mathf.Max(foodSaturation, waterSaturation, maxReproductionUrge - reproductionUrge);

		if(maxPriority > restLimit)
		{
			currentPriority = Priority.Rest;
		}
		else if(maxPriority == foodSaturation)
		{
			currentPriority = Priority.FindFood;
		}
		else if(maxPriority == waterSaturation)
		{
			currentPriority = Priority.FindWater;
		}
		else
		{
			currentPriority = Priority.Reproduce;
		}
	}

	private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, maxViewingDistance);
	private Collider[] Scan => Physics.OverlapSphere(transform.position, maxViewingDistance);

	private void CalculateMovement()
	{
		int randX = UnityEngine.Random.Range(-1, 2);
		int randZ = UnityEngine.Random.Range(-1, 2);
		moveTo = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
	}

	private void Move()
	{
		transform.forward = moveTo-transform.position;
		transform.position = Vector3.Lerp(transform.position, moveTo, .1f);
	}
}
