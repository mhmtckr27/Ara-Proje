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
using TMPro;
using com.hayricakir;
using System.Threading;

public partial class Animal : MonoBehaviour, IReproducible
{
	#region Constant Variable Declarations
	private const float GRAMSPERKG = 1000f;
	private const float MAXLifeTimeYears = 20f;
	private const float HUNDRED = 100f;
	private const float MAXEnergyConsumeSpeed = 20;
	private const float MAXRunSpeed = 100000;
	private const float MAXTrotSpeed = 2;
	private const float MAXAngularSpeed = 360;
	private const float MAXAcceleration = 1000000;
	private const float restLimit = 90;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
	protected const float changePriorityLimit = 60;
	protected const float criticalLimit = 25f;
	private const float forgetDangerTime = 10f;

	private const float MAXExploreTimer = 10f;
	private const float MAXExploreRadius = 20f;
	private const float MAXEscapeTimer = 10f;
	private const float MAXEscapeRadius = 20f;
	
	private const float MAXWeight = 100;
	private const string waterTag = "Water";
	#endregion

	#region Basic Animal Attributes
	public float _foodSaturation;//tokluk
	public float _waterSaturation;//suya doygunluk
	private float _reproductiveUrge;//suya doygunluk
	public float _energySaturation;
	private TimeSpan _lifeSpan;
	private float _runSpeed;
	private float _trotSpeed;
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
	protected float currentAge;
	private DateTime bornDate;
	public float currentFoodIntake;
	public float currentWaterIntake;
	private float currentEnergy;
	#endregion

	#region Inspector Fields
	[EnumFlags] [SerializeField] private Diet diet;
	[EnumFlags] [SerializeField] private LivingEntity dangers;
	[Tooltip("Basic attributes which are randomly generated from range(RNG stands for range)")]
	[SerializeField] private BasicAttributes basicAttributes;
	[SerializeField] protected AnimalStatsUI animalStatsUI;
	[SerializeField] protected NavMeshAgent navMeshAgent;
	[SerializeField] private GameObject canvas;
	[SerializeField] private GameObject child;
	[SerializeField] private float targetNearThreshold;
	[SerializeField] private FieldOfView fieldOfView;
	[SerializeField] private Food asFood;
	[SerializeField] public float bitePeriod;
	[SerializeField] public float biteWeight;
	[Header("Daily Needs")]
	//per day based. percentage of body weight.
	[SerializeField] [Range(0, 1)] private float foodIntakeNeedPercentage;
	[SerializeField] [Range(0, 1)] private float waterIntakeNeedPercentage;
	[SerializeField] private float energyKcalNeedPerKg;

	public float foodIntakeNeed;
	public float waterIntakeNeed;
	public float energyIntakeNeed;
	#endregion

	public Priority currentPriority;
	public State currentState;
	public Dictionary<string, GameObject> objectsDictionary = new Dictionary<string, GameObject>();
	private List<string> dietList = new List<string>();
	private List<string> dangerList = new List<string>();

	[HideInInspector] public Food currentFood;
	protected float timeLeftToForgetDanger;
	Vector3 firstPos;
	Vector3 lastPos;
	private UnityAction ready;
	protected bool isReady = true;
	private float readyTime1 = 5f;
	private float readyTime2;
	private float timeLeftToExplore;

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

	private void OnEnable()
	{
		Initialize();
	}
	private void Awake()
	{
		Debug.Log(GameController.animalFOVs.Count);
		GameController.animals.Add(this);
	}
	public virtual void Start()
	{
		StartCoroutine(UpdateLifeTimeCoroutine());
		StartCoroutine(FindTargetsWithDelay(0f));
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
	public virtual void FixedUpdate()
	{
		#region Debugging, delete before final build
		//if (Selection.activeGameObject == gameObject || Selection.activeGameObject == child)
		//{
		//	canvas.SetActive(true);
		//}
		#endregion

		//DeterminePriority();
		//ChooseAction();
	}
	private IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(Time.fixedDeltaTime);
			ThreadStart thread = delegate
			{
				DeterminePriority();
				ChooseAction();
			};
			thread.Invoke();
			//FindVisibleTargets();
		}
	}
	#region Initialization
	public void Initialize()
	{
		bornDate = Calendar.CurrentDateTime;

		FoodSaturation = UnityEngine.Random.Range(basicAttributes.foodSaturationRNG.x, basicAttributes.foodSaturationRNG.y);
		WaterSaturation = UnityEngine.Random.Range(basicAttributes.waterSaturationRNG.x, basicAttributes.waterSaturationRNG.y);
		ReproductiveUrge = UnityEngine.Random.Range(basicAttributes.reproductiveUrgeRNG.x, basicAttributes.reproductiveUrgeRNG.y);
		
		EnergySaturation = UnityEngine.Random.Range(basicAttributes.energyRNG.x, basicAttributes.energyRNG.y);
		_energyConsumeSpeed = UnityEngine.Random.Range(basicAttributes.energyConsumeSpeedRNG.x, basicAttributes.energyConsumeSpeedRNG.y);
		LifeSpan = LifeSpan.Add(new TimeSpan((int)(UnityEngine.Random.Range(basicAttributes.lifeTimeYearsRNG.x, basicAttributes.lifeTimeYearsRNG.y) * 365), 0, 0, 0));
	
		Charisma = basicAttributes.baseCharisma;

		RunSpeed = UnityEngine.Random.Range(basicAttributes.runSpeedRNG.x, basicAttributes.runSpeedRNG.y);
		TrotSpeed = UnityEngine.Random.Range(basicAttributes.trotSpeedRNG.x, basicAttributes.trotSpeedRNG.y);
		AngularSpeed = UnityEngine.Random.Range(basicAttributes.angularSpeedRNG.x, basicAttributes.angularSpeedRNG.y);
		Acceleration = UnityEngine.Random.Range(basicAttributes.accelerationRNG.x, basicAttributes.accelerationRNG.y);

		ExploreTimer = UnityEngine.Random.Range(basicAttributes.exploreTimerRNG.x, basicAttributes.exploreTimerRNG.y);
		ExploreRadius = UnityEngine.Random.Range(basicAttributes.exploreRadiusRNG.x, basicAttributes.exploreRadiusRNG.y);
		
		EscapeRadius = UnityEngine.Random.Range(basicAttributes.escapeRadiusRNG.x, basicAttributes.escapeRadiusRNG.y);
		EscapeTimer = UnityEngine.Random.Range(basicAttributes.escapeTimerRNG.x, basicAttributes.escapeTimerRNG.y);
		asFood.weightInKg = UnityEngine.Random.Range(basicAttributes.weightRNG.x, basicAttributes.weightRNG.y);

		//TODO: hareket miktarini da iceren bir formul gelistir.
		foodIntakeNeed = asFood.weightInKg * GRAMSPERKG * foodIntakeNeedPercentage;
		waterIntakeNeed = asFood.weightInKg * GRAMSPERKG * waterIntakeNeedPercentage;
		energyIntakeNeed = asFood.weightInKg * energyKcalNeedPerKg;

		FillList(diet, dietList);
		FillList(dangers, dangerList);

		firstPos = transform.position;
		navMeshAgent.updateRotation = false;
		ready += IsReady;
		//TODO: koddan silip editorden yap.
		canvas.SetActive(false);
	}
	#endregion

	#region further implementation
	//private void Consume()
	//{
	//	//ODO: bunu da bişeylere bağla işte bu kadar sade olmasın xd
	//	lastPos = transform.position;
	//	//energy -= Vector3.Distance(lastPos, firstPos) * energyConsumeSpeed * Time.deltaTime;
	//	firstPos = lastPos;
	//	//ODO: eceli geldiyse ölsün.
	//	//if (CurrentLifeTime > LifeTime)
	//	//{
	//	//	Die(CauseOfDeath.OldAge);
	//	//}
	//}
	//private void Die(CauseOfDeath causeOfDeath)
	//{
	//	Debug.Log(gameObject.name + " died of " + causeOfDeath);
	//	//ODO: ölüm olayları ekle,animasyon vs, yan yatıp gözleri x_x(çarpı yap) bikaç saniye sonra yok olsun.
	//	Destroy(gameObject);
	//}
	//protected void Rest()
	//{
	//	//ODO: enerji kazancını tokluk ve susamışlığa bağla, "wellness" veya "wellbeingness" fieldı koy.
	//	EnergySaturation += Time.deltaTime * Time.deltaTime;
	//}
	#endregion

	public void DeterminePriority()
	{
		if(InDanger())
		{
			currentPriority = Priority.Danger;
			return;
		}

		float maxPriority = Mathf.Min(FoodSaturation, WaterSaturation, HUNDRED - ReproductiveUrge);

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

		DetermineState();
	}


	protected virtual void DetermineState()
	{
		if (currentState == State.Eating)
		{
			if(FoodSaturation >= changePriorityLimit)
			{
				currentState = State.EatingDone;
			}
			return;
		}
		else if (currentState == State.ExploringStarted)
		{
			navMeshAgent.speed = TrotSpeed;
			currentState = State.Exploring;
		}
		else if(currentState == State.GoingToFood)
		{
			navMeshAgent.speed = RunSpeed;
		}
		else if(currentState == State.EatingDone)
		{
			//StopCoroutine(eatFoodProcess);
		}
	}

	public virtual void ChooseAction()
	{
		//TODO: burası karışıyor vaziyet al!! bunu daha düzenli yapabilirsin bence?
		animalStatsUI.remainingLifeTimeBar.text = currentPriority.ToString();
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
		else if (currentState == State.Eating)
		{
			return;
		}
		else if (currentPriority == Priority.Food)
		{
			if (!PriorityFood())
			{
				Explore();
			}
		}
		else if (currentPriority == Priority.Rest)
		{
			//Rest();
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
			PriorityExplore();
		}
	}


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
	protected bool TargetNear(GameObject target) => (target.transform.position - transform.position).sqrMagnitude < targetNearThreshold && navMeshAgent.remainingDistance < targetNearThreshold * 1.5f;

	#region Lifetime stuff
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
	#endregion

	//private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, fieldOfView.viewRadiusFront);

	[System.Serializable]
	protected class AnimalStatsUI
	{
		[Header("Main Stats")]
		[SerializeField] public Image foodSaturationBar;
		[SerializeField] public Image waterSaturationBar;
		[SerializeField] public Image reproductiveUrgeBar;
		[SerializeField] public TextMeshProUGUI remainingLifeTimeBar;
		[SerializeField] public Image energyBar;

		[Header("Info Stats")]
		[SerializeField] public TextMeshProUGUI runSpeed;
		[SerializeField] public TextMeshProUGUI angularSpeedText;
		[SerializeField] public TextMeshProUGUI accelerationText;
		[SerializeField] public TextMeshProUGUI exploreTimerText;
		[SerializeField] public TextMeshProUGUI exploreRadiusText;
	}
	[System.Serializable]
	private class BasicAttributes
	{
		[SerializeField] [MinMaxSlider(0, HUNDRED)] public Vector2 foodSaturationRNG;
		[SerializeField] [MinMaxSlider(0, HUNDRED)] public Vector2 waterSaturationRNG;
		[SerializeField] [MinMaxSlider(0, HUNDRED)] public Vector2 reproductiveUrgeRNG;
		[SerializeField] [MinMaxSlider(1, HUNDRED)] public Vector2 energyRNG;

		[SerializeField] [MinMaxSlider(0, MAXLifeTimeYears)] public Vector2 lifeTimeYearsRNG;
		[SerializeField] [MinMaxSlider(1, MAXEnergyConsumeSpeed)] public Vector2 energyConsumeSpeedRNG;
		[SerializeField] [MinMaxSlider(0, MAXWeight)] public Vector2 weightRNG;
		[SerializeField] public float baseCharisma;
		[Header("Movement Stuff")]
		[Tooltip("x10 kmph (5 means 50 kmph)")]
		[SerializeField] [MinMaxSlider(0, MAXRunSpeed)] public Vector2 runSpeedRNG;
		[SerializeField] [MinMaxSlider(0, MAXTrotSpeed)] public Vector2 trotSpeedRNG;
		[SerializeField] [MinMaxSlider(1, MAXAngularSpeed)] public Vector2 angularSpeedRNG;
		[SerializeField] [MinMaxSlider(1, MAXAcceleration)] public Vector2 accelerationRNG;
		[Header("Randomly Explore Stuff")]
		[SerializeField] [MinMaxSlider(1, MAXExploreTimer)] public Vector2 exploreTimerRNG;
		[SerializeField] [MinMaxSlider(1, MAXExploreRadius)] public Vector2 exploreRadiusRNG;
		[Header("Danger Stuff")]
		[SerializeField] [MinMaxSlider(0, MAXEscapeTimer)] public Vector2 escapeTimerRNG;
		[SerializeField] [MinMaxSlider(1, MAXEscapeRadius)] public Vector2 escapeRadiusRNG;
	}
}