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
using UnityEngine.AI;

public partial class Animal : MonoBehaviour
{
	#region Constant Variable Declarations
	private const float maxLifeTime = 120f; //1 year = 60 sec.
	private const float maxFoodSaturation = 100f;
	private const float maxWaterSaturation = 100f;
	private const float maxReproductiveUrge = 100f;
	private const float maxViewingDistance = 20f;
	private const float maxEnergy = 100f;
	private const float maxEnergyConsumeSpeed = 20;
	private const float maxMoveSpeed = 20;
	private const float restLimit = 90;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
	private const float criticalLimit = 25f;
	private const float forgetDangerTime = 10f;
	private const float targetNearThreshold = 1f;
	private float stamina;
	private float charisma;
	#endregion

	#region Basic Animal Attributes
	private float foodSaturation;//tokluk
	private float waterSaturation;//suya doygunluk
	private float reproductiveUrge;//suya doygunluk
	private float energy;
	private float viewingDistance;
	private float remainingLifeTime;
	private float energyConsumeSpeed;
	private float moveSpeed;
	[Range(0,maxViewingDistance)] public float viewRadiusFront;
	[Range(0, 360)] public float viewAngleFront;
	[Range(0, maxViewingDistance)] public float viewRadiusBack;
	[Range(0, 360)] public float viewAngleBack;
	#endregion

	#region Inspector Fields
	[EnumFlags] [SerializeField] private Diet diet;
	[EnumFlags] [SerializeField] private LivingEntity dangers;
	[Tooltip("Basic attributes which are randomly generated from range")]
	[SerializeField] private BasicAttributes basicAttributes;
	[Tooltip("Field of view related attributes")]
	[SerializeField] private FieldOfView fieldOfView;
	[SerializeField] private new Collider collider;
	[SerializeField] private NavMeshAgent navMeshAgent;
	#endregion


	Vector3 moveTo;
	Priority currentPriority;
	public Dictionary<string, GameObject> objectsDictionary = new Dictionary<string, GameObject>();
	
	Vector3 firstPos;
	Vector3 lastPos;
	float timeToChangeDirection;
	float timeLeftToForgetDanger;
	private List<string> dietList = new List<string>();
	private List<string> dangerList = new List<string>();

	#if UNITY_EDITOR
	private void OnValidate()
	{
		viewAngleBack = 360 - viewAngleFront;
		FillList(diet, dietList);
		FillList(dangers, dangerList);

	}
	#endif
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
	private void Start()
	{
		//TODO: call initialize from here.
		Initialize();
		firstPos = transform.position;
		StartCoroutine("FindTargetsWithDelay", .2f);
		navMeshAgent.updateRotation = false;
	}

	public float wanderTimer;
	public float wanderRadius;
	private float timer;

	private void Update()
	{
		DeterminePriority();
		ChooseAction();
		Consume();
		//GetComponent<NavMeshAgent>().SetDestination(Vector3.zero);
	}

	#region Find targets within field of view related stuff

	private Collider[] ScanFieldOfView => Physics.OverlapSphere(transform.position, viewRadiusFront, fieldOfView.targetMask);
	private bool IsDuplicate(Collider collider) => objectsDictionary.ContainsKey(collider.tag);
	IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}

	private void FindVisibleTargets()
	{
		objectsDictionary.Clear();
		List<Collider> targetsInViewRadius = ScanFieldOfView.ToList();
		targetsInViewRadius.Remove(collider);

		for (int i = 0; i < targetsInViewRadius.Count; i++)
		{
			Collider target = targetsInViewRadius[i];
			Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
			//ön görüş açısında neler var onu bulur
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngleFront / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, fieldOfView.obstacleMask))
				{
					if (IsDuplicate(target))
					{
						AddCloserOneToDictionary(target);
					}
					else
					{
						objectsDictionary.Add(target.tag, target.gameObject);
					}
				}
			}
			//Arka görüş açısında neler var onu bulur
			else if(Vector3.Angle(-transform.forward, dirToTarget) < viewAngleBack / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				if(viewRadiusBack > dstToTarget)
				{
					if(!Physics.Raycast(transform.position, dirToTarget, dstToTarget, fieldOfView.obstacleMask))
					{
						if (IsDuplicate(target))
						{
							AddCloserOneToDictionary(target);
						}
						else
						{
							objectsDictionary.Add(target.tag, target.gameObject);
						}
					}
				}
			}
		}
	}

	private void AddCloserOneToDictionary(Collider collider)
	{           
		//aynı tipte varlık onceden eklenmisse, hangisi yakınsa o bulunarak guncellenir.
		GameObject objectInDictionary;
		objectsDictionary.TryGetValue(collider.tag, out objectInDictionary);
		bool shouldReplace = ShouldReplace(objectInDictionary.transform.position, collider.gameObject.transform.position);
		if (shouldReplace)
		{
			objectsDictionary[objectInDictionary.tag] = collider.gameObject;
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
	private void CheckDuplicate(Collider collider)
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
				objectsDictionary[objectInDictionary.tag] = collider.gameObject;
			}
		}
	}

	private bool ShouldReplace(Vector3 obj1, Vector3 obj2)
	{
		float distToObj1 = Vector3.Distance(transform.position, obj1);
		float distToObj2 = Vector3.Distance(transform.position, obj2);
		return distToObj1 > distToObj2 ? true : false;
	}

	#endregion
	public void Initialize()
	{
		foodSaturation = UnityEngine.Random.Range(basicAttributes.foodSaturationRNG.x, basicAttributes.foodSaturationRNG.y);
		waterSaturation = UnityEngine.Random.Range(basicAttributes.waterSaturationRNG.x, basicAttributes.waterSaturationRNG.y);
		reproductiveUrge = UnityEngine.Random.Range(basicAttributes.reproductiveUrgeRNG.x, basicAttributes.reproductiveUrgeRNG.y);
		energy = UnityEngine.Random.Range(basicAttributes.energyRNG.x, basicAttributes.energyRNG.y);
		//viewingDistance = Random.Range(baseAttributes.viewingDistanceRNG.x, baseAttributes.viewingDistanceRNG.y);
		remainingLifeTime = UnityEngine.Random.Range(basicAttributes.remainingLifeTimeRNG.x, basicAttributes.remainingLifeTimeRNG.y);
		energyConsumeSpeed = UnityEngine.Random.Range(basicAttributes.energyConsumeSpeedRNG.x, basicAttributes.energyConsumeSpeedRNG.y);
		moveSpeed = UnityEngine.Random.Range(basicAttributes.moveSpeedRNG.x, basicAttributes.moveSpeedRNG.y);

		viewAngleFront = UnityEngine.Random.Range(fieldOfView.viewAngleFrontRNG.x, fieldOfView.viewAngleFrontRNG.y);
		viewAngleBack = 360-viewAngleFront;	
		
		viewRadiusFront = UnityEngine.Random.Range(fieldOfView.viewRadiusFrontRNG.x, fieldOfView.viewRadiusFrontRNG.y);
		viewRadiusBack = UnityEngine.Random.Range(fieldOfView.viewRadiusBackRNG.x, viewRadiusFront);

		FillList(diet, dietList);
		FillList(dangers, dangerList);
	}

	private void Consume()
	{
		//TODO: bunu da bişeylere bağla işte bu kadar sade olmasın xd
		lastPos = transform.position;
		//energy -= Vector3.Distance(lastPos, firstPos) * energyConsumeSpeed * Time.deltaTime;
		firstPos = lastPos;
		remainingLifeTime -= Time.deltaTime;
		if (remainingLifeTime <= 0)
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
		energy += Time.deltaTime * Time.deltaTime;
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

		float maxPriority = Mathf.Min(foodSaturation, waterSaturation, maxReproductiveUrge - reproductiveUrge);
		if(maxPriority > restLimit)
		{
			currentPriority = Priority.Rest;
		}
		else if(maxPriority < criticalLimit)
		{
			if (maxPriority == foodSaturation)
			{
				currentPriority = Priority.Food;
			}
			else if (maxPriority == waterSaturation)
			{
				currentPriority = Priority.Water;
			}
			else if (maxPriority == reproductiveUrge)
			{
				currentPriority = Priority.Mate;
			}
		}
		else
		{
			currentPriority = Priority.Explore;
		}
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


	private Vector3 RandomNavSphere(Vector3 origin, float dist, LayerMask layerMask)
	{
		Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;

		randDirection += origin;

		NavMeshHit navHit;

		NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);

		return navHit.position;
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
		else if(currentPriority == Priority.Food)
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
		else //currentPriority == Priority.Water
		{
			//Consume water
		}
	}

	Quaternion quat;
	private void ChangeDirection()
	{
		float angle = UnityEngine.Random.Range(-90f, 90f);
		//Vector3 newUp = quat * Vector3.forward;
		quat = Quaternion.AngleAxis(angle, Vector3.up);
		//newUp.z = 0;
		//newUp.Normalize();
		timeToChangeDirection = UnityEngine.Random.Range(.25f, 1.5f);
	}

	//private void Explore()
	//{
	//	timeToChangeDirection -= Time.deltaTime;

	//	if (timeToChangeDirection <= 0)
	//	{
	//		ChangeDirection();
	//	}
	//	transform.rotation = Quaternion.Slerp(transform.rotation, quat, 0.1f);
	//	transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f * moveSpeed);

	//}

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
		if (TargetNear())
		{
			objectsDictionary.Remove(foodFound.tag);
			Destroy(foodFound);
			foodSaturation += 20;
			energy += 20;
			waterSaturation -= 5;
		}
		return true;
	}

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

	private bool TargetNear() => navMeshAgent.hasPath && navMeshAgent.remainingDistance < targetNearThreshold;

	//private void Escape()
	//{
	//	//GetComponent<Animator>().speed = moveSpeed;
	//	//GetComponent<Animator>().SetTrigger("jump");
	//	GameObject closestDanger = FindClosestDanger();
	//	//TODO: move to danger's script's hunt/findfood funtion
	//	closestDanger.transform.forward = new Vector3((transform.position - closestDanger.transform.position).x, 0, (transform.position - closestDanger.transform.position).z);
	//	transform.rotation = Quaternion.Slerp(transform.rotation, closestDanger.transform.rotation, 0.1f);
	//	//transform.forward = closestDanger.transform.forward;
	//	Escape1();
	//}

	//private void Escape1()
	//{
	//	transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f * moveSpeed);
	//}

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

	private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, viewingDistance);

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

	}
	[System.Serializable]
	private class FieldOfView
	{
		public LayerMask targetMask;
		public LayerMask obstacleMask;
		[Header("Front View")]
		[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusFrontRNG;
		[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleFrontRNG;

		//TODO: isi bitince sil. arka gorus on gorusun tumleyeni olacak.
		[Header("Back View")]
		[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusBackRNG;
		[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleBackRNG;
	}
}
