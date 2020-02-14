﻿/*
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
using TMPro;
using com.hayricakir;

public partial class Animal : MonoBehaviour, IReproducible
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
	private const float maxExploreTimer = 10f;
	private const float maxEscapeTimer = 10f;
	private const float maxExploreRadius = 20f;
	private const float maxEscapeRadius = 20f;
	private const string waterTag = "Water";
	#endregion

	#region Basic Animal Attributes
	private float _foodSaturation;//tokluk
	private float _waterSaturation;//suya doygunluk
	private float _reproductiveUrge;//suya doygunluk
	private float _energy;
	private float _lifeTime;
	private float _moveSpeed;
	private float _angularSpeed;
	private float _acceleration;
	private float _energyConsumeSpeed;
	private float _exploreTimer;
	private float _exploreRadius;
	private float _escapeTimer;
	private float _escapeRadius;
	private float _charisma;
	private float stamina;
	private DateTime currentLifeTimeDateTime;
	private float currentAge;
	private DateTime bornDate;
	#endregion

	#region Inspector Fields
	[EnumFlags] [SerializeField] private Diet diet;
	[EnumFlags] [SerializeField] private LivingEntity dangers;
	[Tooltip("Basic attributes which are randomly generated from range(RNG stands for range)")]
	[SerializeField] private BasicAttributes basicAttributes;
	[SerializeField] private AnimalStatsUI animalStatsUI;
	[SerializeField] private NavMeshAgent navMeshAgent;
	[SerializeField] private GameObject canvas;
	[SerializeField] private GameObject child;
	[SerializeField] private float targetNearThreshold;
	[SerializeField] private FieldOfView fieldOfView;
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

	public struct LifeTime
	{
		public int second;
		public int minute;
		public int hour;
		public int day;
		public int month;
		public int year;
	}

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
		StartCoroutine(UpdateLifeTimeCoroutine());
		firstPos = transform.position;
		navMeshAgent.updateRotation = false;
		ready += IsReady;
		//TODO: koddan silip editorden yap.
		canvas.SetActive(false);
	}
	private void OnMouseDown()
	{
		if (canvas.activeInHierarchy)
		{
			canvas.SetActive(false);
		}
		else
		{
			canvas.SetActive(true);
		}
	}
	private void Update()
	{
		//TODO: delete before final build
		if(Selection.activeGameObject == gameObject || Selection.activeGameObject == child)
		{
			canvas.SetActive(true);
		}
		//TODO: duzenle burayi
		if(readyTime2 <= 0f)
		{
			isReady = true;
		}
		DeterminePriority();
		ChooseAction();
		Consume();
		readyTime2 -= Time.deltaTime;
		Debug.Log((float)currentAge);
	}


	public void Initialize()
	{
		bornDate = Calendar.CurrentDateTime;

		FoodSaturation = UnityEngine.Random.Range(basicAttributes.foodSaturationRNG.x, basicAttributes.foodSaturationRNG.y);
		WaterSaturation = UnityEngine.Random.Range(basicAttributes.waterSaturationRNG.x, basicAttributes.waterSaturationRNG.y);
		ReproductiveUrge = UnityEngine.Random.Range(basicAttributes.reproductiveUrgeRNG.x, basicAttributes.reproductiveUrgeRNG.y);
		
		Energy = UnityEngine.Random.Range(basicAttributes.energyRNG.x, basicAttributes.energyRNG.y);
		_energyConsumeSpeed = UnityEngine.Random.Range(basicAttributes.energyConsumeSpeedRNG.x, basicAttributes.energyConsumeSpeedRNG.y);
		_lifeTime = UnityEngine.Random.Range(basicAttributes.lifeTimeRNG.x, basicAttributes.lifeTimeRNG.y);

		Charisma = basicAttributes.baseCharisma;

		MoveSpeed = UnityEngine.Random.Range(basicAttributes.moveSpeedRNG.x, basicAttributes.moveSpeedRNG.y);
		AngularSpeed = UnityEngine.Random.Range(basicAttributes.angularSpeedRNG.x, basicAttributes.angularSpeedRNG.y);
		Acceleration = UnityEngine.Random.Range(basicAttributes.accelerationRNG.x, basicAttributes.accelerationRNG.y);

		ExploreTimer = UnityEngine.Random.Range(basicAttributes.exploreTimerRNG.x, basicAttributes.exploreTimerRNG.y);
		ExploreRadius = UnityEngine.Random.Range(basicAttributes.exploreRadiusRNG.x, basicAttributes.exploreRadiusRNG.y);
		
		EscapeRadius = UnityEngine.Random.Range(basicAttributes.escapeRadiusRNG.x, basicAttributes.escapeRadiusRNG.y);
		EscapeTimer = UnityEngine.Random.Range(basicAttributes.escapeTimerRNG.x, basicAttributes.escapeTimerRNG.y);

		FillList(diet, dietList);
		FillList(dangers, dangerList);
	}

	private void Consume()
	{
		//TODO: bunu da bişeylere bağla işte bu kadar sade olmasın xd
		lastPos = transform.position;
		//energy -= Vector3.Distance(lastPos, firstPos) * energyConsumeSpeed * Time.deltaTime;
		firstPos = lastPos;
		//TODO: eceli geldiyse ölsün.
		//if (CurrentLifeTime > LifeTime)
		//{
		//	Die(CauseOfDeath.OldAge);
		//}
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
			//TODO: timer koy.
			GameObject closestDanger = FindClosestDanger();
			if(closestDanger != null)
			{
				Escape(closestDanger);
			}
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

	#region Escape Stuff
	private Vector3 CalculateEscapeRoute(GameObject closestDanger)
	{
		transform.forward = transform.position - closestDanger.transform.position;
		Vector3 escapePosition = UnityEngine.Random.insideUnitSphere * (EscapeRadius * Mathf.Tan(fieldOfView.viewAngleDanger));

		escapePosition += transform.position;
		escapePosition += transform.forward * EscapeRadius;

		NavMeshHit navHit;

		NavMesh.SamplePosition(escapePosition, out navHit, EscapeRadius, -1);

		Debug.DrawLine(transform.position, escapePosition);
		return navHit.position;
	}

	private float timeLeftToEscape;
	private void Escape(GameObject closestDanger)
	{
		timeLeftToEscape += Time.deltaTime;
		//Debug.Log(timeLeftToEscape + "   " + EscapeTimer);
		if(timeLeftToEscape >= EscapeTimer)
		{
			Vector3 escapePosition = CalculateEscapeRoute(closestDanger);
			navMeshAgent.SetDestination(escapePosition);
			timeLeftToEscape = 0;
		}
	}
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
	#endregion

	#region Explore stuff
	private Vector3 RandomNavSphere(Vector3 origin, float dist, LayerMask layerMask)
	{
		Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;

		randDirection += origin;

		NavMeshHit navHit;

		NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);

		return navHit.position;
	}

	private float timeLeftToExplore;


	private void Explore()
	{
		timeLeftToExplore += Time.deltaTime;
		animalStatsUI.exploreTimerText.text = "Explore timer: " +((ExploreTimer - timeLeftToExplore).ToString("F1") + "/" + maxExploreTimer);
		if (timeLeftToExplore >= ExploreTimer)
		{
			Vector3 newPos = RandomNavSphere(transform.position, ExploreRadius, -1);
			navMeshAgent.SetDestination(newPos);
			timeLeftToExplore = 0;
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
		//Debug.Log("first: " + ((foodFound.transform.position - transform.position).sqrMagnitude < targetNearThreshold) + "second:" + (navMeshAgent.remainingDistance < targetNearThreshold) + "third:" + isReady);
		//TODO: buraları düzenle hardcodingi düzelt
		if (TargetNear(foodFound) && isReady)
		{
			//Debug.LogError("HAHA");
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

	#region Mating Stuff
	public virtual GameObject FindPotentialMate()
	{
		throw new NotImplementedException();
	}

	public void RequestMating()
	{
		throw new NotImplementedException();
	}

	public bool IsAttracted()
	{
		throw new NotImplementedException();
	}

	public void Mate()
	{
		throw new NotImplementedException();
	}
	#endregion

	//TODO: calismiyor, daha iyi bir yol bul 
	private bool TargetNear(GameObject target) => (target.transform.position - transform.position).sqrMagnitude < targetNearThreshold && navMeshAgent.remainingDistance < targetNearThreshold * 1.5f;

	public IEnumerator UpdateLifeTimeCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(GameController.Instance.animalAgeUpdatePeriod);
			UpdateLifeTime(GameController.Instance.timeFlowRate.timeFlowRate);
		}
	}

	private void UpdateLifeTime(float amountToAdd)
	{
		switch (GameController.Instance.timeFlowRate.timeType)
		{
			case GameController.TimeType.Second:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddSeconds(amountToAdd);
				break;
			case GameController.TimeType.Minute:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddMinutes(amountToAdd);
				break;
			case GameController.TimeType.Hour:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddHours(amountToAdd);
				break;
			case GameController.TimeType.Day:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddDays(amountToAdd);
				break;
			case GameController.TimeType.Month:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddMonths((int)amountToAdd);
				break;
			case GameController.TimeType.Year:
				currentLifeTimeDateTime = currentLifeTimeDateTime.AddYears((int)amountToAdd);
				break;
		}

		TimeSpan temp = Calendar.GetDifference(bornDate, Calendar.CurrentDateTime);
		currentAge = (float) temp.TotalDays / 365;
	}

	//private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, fieldOfView.viewRadiusFront);

	[System.Serializable]
	private class AnimalStatsUI
	{
		[Header("Main Stats")]
		[SerializeField] public Image foodSaturationBar;
		[SerializeField] public Image waterSaturationBar;
		[SerializeField] public Image reproductiveUrgeBar;
		[SerializeField] public Image remainingLifeTimeBar;
		[SerializeField] public Image energyBar;

		[Header("Info Stats")]
		[SerializeField] public TextMeshProUGUI moveSpeedText;
		[SerializeField] public TextMeshProUGUI angularSpeedText;
		[SerializeField] public TextMeshProUGUI accelerationText;
		[SerializeField] public TextMeshProUGUI exploreTimerText;
		[SerializeField] public TextMeshProUGUI exploreRadiusText;
	}
	[System.Serializable]
	private class BasicAttributes
	{
		[SerializeField] [MinMaxSlider(0, maxFoodSaturation)] public Vector2 foodSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxWaterSaturation)] public Vector2 waterSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxReproductiveUrge)] public Vector2 reproductiveUrgeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergy)] public Vector2 energyRNG;
		[SerializeField] [MinMaxSlider(1, maxLifeTime)] public Vector2 lifeTimeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergyConsumeSpeed)] public Vector2 energyConsumeSpeedRNG;
		[SerializeField] public float baseCharisma;
		[Header("Movement Stuff")]
		[SerializeField] [MinMaxSlider(1, maxMoveSpeed)] public Vector2 moveSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxAngularSpeed)] public Vector2 angularSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxAcceleration)] public Vector2 accelerationRNG;
		[Header("Randomly Explore Stuff")]
		[SerializeField] [MinMaxSlider(1, maxExploreTimer)] public Vector2 exploreTimerRNG;
		[SerializeField] [MinMaxSlider(1, maxExploreRadius)] public Vector2 exploreRadiusRNG;
		[Header("Danger Stuff")]
		[SerializeField] [MinMaxSlider(0, maxEscapeTimer)] public Vector2 escapeTimerRNG;
		[SerializeField] [MinMaxSlider(1, maxEscapeRadius)] public Vector2 escapeRadiusRNG;

	}
}