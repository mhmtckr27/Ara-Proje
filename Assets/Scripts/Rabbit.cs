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
	private const Diet diet = Diet.Plant;
	private const float maxLifeTime = 120f; //1 year = 60 sec.
	private const float maxFoodSaturation = 100f;
	private const float maxWaterSaturation = 100f;
	private const float maxReproductiveUrge = 100f;
	private const float restLimit = 75f;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
	private float lifeTime;
	private float reproductiveUrge;

	[SerializeField] private float foodSaturation;//tokluk
	[SerializeField] private float waterSaturation;//suya doygunluk
	[SerializeField] private float maxViewingDistance;
	[SerializeField] public float energy;
	[SerializeField] private float energyConsumeSpeed;
	[SerializeField] private LayerMask notGroundLayers;
	[SerializeField] private Collider col;

	#endregion

	Vector3 moveTo;
	Priority currentPriority;
	Dictionary<string,GameObject> objectsDictionary = new Dictionary<string, GameObject>();

	Vector3 firstPos;
	Vector3 lastPos;
	private void Start()
	{
		firstPos = transform.position;
	}

	private void Update()
	{
		CreateDictionary();//Scan and create scanned objects' dictionary
		DeterminePriority();
		ChooseAction();
		ConsumeEnergy();
	}

	private void ConsumeEnergy()
	{
		//TODO: bunu da bişeylere bağla işte bu kadar sade olmasın xd
		lastPos = transform.position;
		energy -= Vector3.Distance(lastPos, firstPos) * energyConsumeSpeed * Time.deltaTime;
		firstPos = lastPos;
	}

	private void Rest()
	{
		//TODO: enerji kazancını tokluk ve susamışlığa bağla, "wellness" veya "wellbeingness" fieldı koy.
		energy += Time.deltaTime * Time.deltaTime;
	}

	private void ChooseAction()
	{
		if(currentPriority == Priority.Rest)
		{
			Rest();
			return;
		}
		GameObject priorityObj;
		objectsDictionary.TryGetValue(currentPriority.ToString(), out priorityObj);
		if(currentPriority == Priority.Fox)
		{
			EscapeFox(priorityObj);
		}
		else
		{
			TakeAction(priorityObj);
		}
	}


	private void TakeAction(GameObject priorityObj)
	{
		Quaternion lookAt = Quaternion.LookRotation(priorityObj.transform.position - transform.position, transform.up);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, 0.1f);
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f);
	}

	private void EscapeFox(GameObject closestFox)
	{
		//TODO: move to fox script
		closestFox.transform.LookAt(gameObject.transform);
		transform.rotation = Quaternion.Slerp(transform.rotation,closestFox.transform.rotation, 0.1f);
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
		hits.Remove(col);
		
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
			currentPriority = Priority.Fox;
			return;
		}

		float maxPriority = Mathf.Min(foodSaturation, waterSaturation, maxReproductiveUrge - reproductiveUrge);

		if(maxPriority > restLimit)
		{
			currentPriority = Priority.Rest;
		}
		else if(maxPriority == foodSaturation)
		{
			currentPriority = Priority.Food;
		}
		else if(maxPriority == waterSaturation)
		{
			currentPriority = Priority.Water;
		}
		else
		{
			currentPriority = Priority.Rabbit;
		}
	}

	private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, maxViewingDistance);
	private Collider[] Scan => Physics.OverlapSphere(transform.position, maxViewingDistance, notGroundLayers);

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
