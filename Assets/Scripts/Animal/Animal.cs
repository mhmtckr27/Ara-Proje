/*
Code by Hayri Cakir
www.hayricakir.com
*/
//using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

public partial class Animal : MonoBehaviour
{
	#region Constant Variable Declarations
	private const float maxLifeTime = 120f; //1 year = 60 sec.
	private const float maxFoodSaturation = 100f;
	private const float maxWaterSaturation = 100f;
	private const float maxReproductiveUrge = 100f;
	private const float maxEnergy = 100f;
	private const float maxEnergyConsumeSpeed = 20;
	private const float maxMoveSpeed = 20;
	private const float maxAngularSpeed = 360;
	private const float maxAcceleration = 20;
	private const float restLimit = 90;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
	private const float criticalLimit = 25f;
	private const float forgetDangerTime = 10f;
	private const float targetNearThreshold = 1f;
	private const string waterTag = "Water";
	private float stamina;
	private float charisma;
	#endregion

	#region Basic Animal Attributes
	private float _foodSaturation;//tokluk
	private float _waterSaturation;//suya doygunluk
	private float _reproductiveUrge;//suya doygunluk
	private float _energy;
	private float _remainingLifeTime;
	private float _moveSpeed;
	private float _angularSpeed;
	private float _acceleration;
	private float _energyConsumeSpeed;
	#endregion

	#region Inspector Fields
	[EnumFlags] [SerializeField] private Diet diet;
	[EnumFlags] [SerializeField] private LivingEntity dangers;
	[Tooltip("Basic attributes which are randomly generated from range(RNG stands for range)")]
	[SerializeField] private BasicAttributes basicAttributes;
	[SerializeField] private AnimalStatsUI animalStatsUI;
	[SerializeField] private FieldOfView fieldOfView;
	[SerializeField] private new Collider collider;
	[SerializeField] private NavMeshAgent navMeshAgent;
	#endregion

	public Priority currentPriority;
	public Dictionary<string, GameObject> objectsDictionary = new Dictionary<string, GameObject>();
	private List<string> dietList = new List<string>();
	private List<string> dangerList = new List<string>();
	
	float timeLeftToForgetDanger;
	Vector3 firstPos;
	Vector3 lastPos;
	private UnityAction ready;
	private bool isReady = true;
	private float readyTime1 = 5f;
	private float readyTime2;

	public float wanderTimer;
	public float wanderRadius;
	private float timer;

	private void IsReady()
	{
		isReady = true;
	}
	#if UNITY_EDITOR
	private void OnValidate()
	{
		FillList(diet, dietList);
		FillList(dangers, dangerList);
	}
	#endif

	#region Filling diet and danger lists
	//---------------------------------------------------------------------------------------------
	/// <summary>
	/// Fills a list from enum flags.
	/// </summary>
	///---------------------------------------------------------------------------------------------
	private void FillList(Enum type, List<string> list)
	{
		int[] values = (int[])Enum.GetValues(type.GetType());
		for (int i = 0; i < values.Length; i++)
		{
			int layer = 1 << i;
			if ((Convert.ToInt32(type) & layer) != 0)
			{
				string value = Enum.GetName(type.GetType(), values[i]);
				list.Add(value);
			}
		}
	}
	#endregion
	private void Start()
	{
		//TODO: call initialize from here.
		Initialize();
		firstPos = transform.position;
		navMeshAgent.updateRotation = false;
		ready += IsReady;
	}

	private void Update()
	{
		//TODO: duzenle burayi
		if(readyTime2 <= 0f)
		{
			isReady = true;
		}
		DeterminePriority();
		ChooseAction();
		Consume();
		readyTime2 -= Time.deltaTime;
	}

	public void Initialize()
	{
		FoodSaturation = UnityEngine.Random.Range(basicAttributes.foodSaturationRNG.x, basicAttributes.foodSaturationRNG.y);
		WaterSaturation = UnityEngine.Random.Range(basicAttributes.waterSaturationRNG.x, basicAttributes.waterSaturationRNG.y);
		ReproductiveUrge = UnityEngine.Random.Range(basicAttributes.reproductiveUrgeRNG.x, basicAttributes.reproductiveUrgeRNG.y);
		Energy = UnityEngine.Random.Range(basicAttributes.energyRNG.x, basicAttributes.energyRNG.y);
		RemainingLifeTime = UnityEngine.Random.Range(basicAttributes.remainingLifeTimeRNG.x, basicAttributes.remainingLifeTimeRNG.y);
		MoveSpeed = UnityEngine.Random.Range(basicAttributes.moveSpeedRNG.x, basicAttributes.moveSpeedRNG.y);
		AngularSpeed = UnityEngine.Random.Range(basicAttributes.angularSpeedRNG.x, basicAttributes.angularSpeedRNG.y);
		Acceleration = UnityEngine.Random.Range(basicAttributes.accelerationRNG.x, basicAttributes.accelerationRNG.y);
		_energyConsumeSpeed = UnityEngine.Random.Range(basicAttributes.energyConsumeSpeedRNG.x, basicAttributes.energyConsumeSpeedRNG.y);

		FillList(diet, dietList);
		FillList(dangers, dangerList);
	}
	private void Consume()
	{
		//TODO: bunu da bişeylere bağla işte bu kadar sade olmasın xd
		lastPos = transform.position;
		//energy -= Vector3.Distance(lastPos, firstPos) * energyConsumeSpeed * Time.deltaTime;
		firstPos = lastPos;
		RemainingLifeTime -= Time.deltaTime;
		if (RemainingLifeTime <= 0)
		{
			Die(CauseOfDeath.OldAge);
		}
	}
	private void Die(CauseOfDeath causeOfDeath)
	{
		Debug.Log(gameObject.name + " died of " + causeOfDeath);
		//TODO: ölüm olayları ekle,animasyon vs, yan yatıp gözleri x_x(çarpı yap) bikaç saniye sonra yok olsun.
		Destroy(gameObject);
	}
	private void Rest()
	{
		//TODO: enerji kazancını tokluk ve susamışlığa bağla, "wellness" veya "wellbeingness" fieldı koy.
		Energy += Time.deltaTime * Time.deltaTime;
	}
	private void DeterminePriority()
	{
		//TODO: burası da cumbersome, belki düzenleyebilirsin.
		foreach (string danger in dangerList)
		{
			if (objectsDictionary.ContainsKey(danger))
			{
				currentPriority = Priority.Danger;
				return;
			}
		}

		float maxPriority = Mathf.Min(FoodSaturation, WaterSaturation, maxReproductiveUrge - ReproductiveUrge);
		if(maxPriority > restLimit)
		{
			currentPriority = Priority.Rest;
		}
		else if(maxPriority < criticalLimit)
		{
			if (maxPriority == FoodSaturation)
			{
				currentPriority = Priority.Food;
			}
			else if (maxPriority == WaterSaturation)
			{
				currentPriority = Priority.Water;
			}
			else if (maxPriority == ReproductiveUrge)
			{
				currentPriority = Priority.Mate;
			}
		}
		else
		{
			currentPriority = Priority.Explore;
		}
	}
	private void ChooseAction()
	{
		//TODO: burası karışıyor vaziyet al!! bunu daha düzenli yapabilirsin bence?
		timeLeftToForgetDanger -= Time.deltaTime;
		if (currentPriority == Priority.Danger)
		{
			//timeLeftToForgetDanger = forgetDangerTime;
			//Escape();
		}
		else if (timeLeftToForgetDanger > 0f)
		{
			//Escape1();
		}
		else if (currentPriority == Priority.Food)
		{
			if (!EatFood())
			{
				Explore();
			}
		}
		else if (currentPriority == Priority.Rest)
		{
			Rest();
			return;
		}
		else if (currentPriority == Priority.Mate)
		{
			//Go to mate if you like him/her.
		}
		else if (currentPriority == Priority.Water)
		{
			if (!DrinkWater())
			{
				Explore();
			}
		}
		else //currentpriority == explore
		{
			Explore();
		}
	}

	#region Explore stuff
	private Vector3 RandomNavSphere(Vector3 origin, float dist, LayerMask layerMask)
	{
		Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;

		randDirection += origin;

		NavMeshHit navHit;

		NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);

		return navHit.position;
	}
	private void Explore()
	{
		timer += Time.deltaTime;

		if (timer >= wanderTimer)
		{
			Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
			navMeshAgent.SetDestination(newPos);
			timer = 0;
		}
	}
	#endregion

	#region Water stuff
	private GameObject FindWater()
	{
		GameObject waterFound;
		objectsDictionary.TryGetValue(waterTag, out waterFound);
		return waterFound;
	}
	private bool DrinkWater()
	{
		GameObject waterFound = FindWater();
		if(waterFound == null)
		{
			return false;
		}
		navMeshAgent.SetDestination(waterFound.transform.position);
		if (TargetNear(waterFound) && isReady)
		{
			isReady = false;
			readyTime2 = readyTime1;
			WaterSaturation += 20;
		}
		return true;
	}
	#endregion

	#region Food stuff
	private GameObject FindFood()
	{
		GameObject foodFound = null;
		foreach (string food in dietList)
		{
			if (objectsDictionary.TryGetValue(food, out foodFound))
			{
				break;
			}
		}
		
		return foodFound;
	}
	private bool EatFood()
	{
		//find food
		GameObject foodFound = FindFood();
		if (foodFound == null)
		{
			return false;
		}
		navMeshAgent.SetDestination(foodFound.transform.position);

		//TODO: buraları düzenle hardcodingi düzelt
		if (TargetNear(foodFound) && isReady)
		{
			readyTime2 = readyTime1;
			isReady = false;
			objectsDictionary.Remove(foodFound.tag);
			Destroy(foodFound);
			FoodSaturation += 20;
			Energy += 20;
			WaterSaturation -= 5;
		}
		return true;
	}
	#endregion

	private GameObject FindClosestDanger()
	{
		float closestDistance = Mathf.Infinity;
		float currentDistance;
		GameObject closestDanger = null;
		GameObject currentDanger;
		//TODO: IJobParallelFor kullanabilirsen kullan bu ve diğer for'lar için.
		foreach (string danger in dangerList)
		{
			if (objectsDictionary.TryGetValue(danger, out currentDanger))
			{
				currentDistance = Vector3.Distance(gameObject.transform.position, currentDanger.transform.position);
				if (currentDistance < closestDistance)
				{
					closestDistance = currentDistance;
					closestDanger = currentDanger;
				}
			}
		}
		return closestDanger;
	}
	//TODO: calismiyor, daha iyi bir yol bul 
	private bool TargetNear(GameObject target) => (target.transform.position - transform.position).sqrMagnitude < targetNearThreshold && navMeshAgent.remainingDistance < targetNearThreshold;
	//private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, fieldOfView.viewRadiusFront);

	[System.Serializable]
	private class AnimalStatsUI
	{
		[SerializeField] public Image foodSaturationBar;
		[SerializeField] public Image waterSaturationBar;
		[SerializeField] public Image reproductionUrgeBar;
		[SerializeField] public Image remainingLifeTimeBar;
		[SerializeField] public Image energyBar;
	}
	[System.Serializable]
	private class BasicAttributes
	{
		[SerializeField] [MinMaxSlider(0, maxFoodSaturation)] public Vector2 foodSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxWaterSaturation)] public Vector2 waterSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxReproductiveUrge)] public Vector2 reproductiveUrgeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergy)] public Vector2 energyRNG;
		[SerializeField] [MinMaxSlider(1, maxLifeTime)] public Vector2 remainingLifeTimeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergyConsumeSpeed)] public Vector2 energyConsumeSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxMoveSpeed)] public Vector2 moveSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxAngularSpeed)] public Vector2 angularSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxAcceleration)] public Vector2 accelerationRNG;

	}
}