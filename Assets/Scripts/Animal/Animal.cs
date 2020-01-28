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
	private const float restLimit = 75f;//eğer tokluk ve suya doygunluk bu sınırın üstündeyse ve üreme dürtüsü de bunun altındaysa rest durumuna geçer.
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
	[System.Serializable]
	private class BasicAttributes
	{
		[Tooltip("Rastgele deger uretilecek min max araligini seciniz..")]
		[SerializeField] [MinMaxSlider(0, maxFoodSaturation)] public Vector2 foodSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxWaterSaturation)] public Vector2 waterSaturationRNG;
		[SerializeField] [MinMaxSlider(0, maxReproductiveUrge)] public Vector2 reproductiveUrgeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergy)] public Vector2 energyRNG;
		//[SerializeField] [MinMaxSlider(1, maxViewingDistance)] public Vector2 viewingDistanceRNG;
		[SerializeField] [MinMaxSlider(1, maxLifeTime)] public Vector2 remainingLifeTimeRNG;
		[SerializeField] [MinMaxSlider(1, maxEnergyConsumeSpeed)] public Vector2 energyConsumeSpeedRNG;
		[SerializeField] [MinMaxSlider(1, maxMoveSpeed)] public Vector2 moveSpeedRNG;

		public LayerMask targetMask;
		public LayerMask obstacleMask;

		[Header("Front View")]
		[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusFrontRNG;
		[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleFrontRNG;

		[Header("Back View")]
		[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusBackRNG;
		[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleBackRNG;
	}

	[SerializeField] private BasicAttributes basicAttributes;
	[SerializeField] private LayerMask notGroundLayers;
	[SerializeField] private Collider col;
	[SerializeField] private List<string> dangers;
	[SerializeField] private List<string> foods;
	#endregion

	[EnumFlags] [SerializeField] private Diet diet;

	Vector3 moveTo;
	Priority currentPriority;
	Dictionary<string, GameObject> objectsDictionary = new Dictionary<string, GameObject>();
	
	Vector3 firstPos;
	Vector3 lastPos;
	float timeToChangeDirection;
	[HideInInspector]
	public List<GameObject> visibleTargets = new List<GameObject>();
	private List<int> dietList = new List<int>();

	private void OnValidate()
	{
		if(viewAngleFront + viewAngleBack > 360)
		{
			viewAngleBack = 360 - viewAngleFront;
		}
		CreateDietList();
	}
	private void CreateDietList()
	{

		for (int i = 0; i < System.Enum.GetValues(typeof(Diet)).Length; i++)
		{
			int layer = 1 << i;
			if (((int)diet & layer) != 0)
			{
				dietList.Add(i);
			}
		}

	}
	private void Start()
	{
		//TODO: call initialize from here.
		Initialize();
		firstPos = transform.position;
		StartCoroutine("FindTargetsWithDelay", .2f);
	}

	#region Find targets within field of view related stuff
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
		visibleTargets.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadiusFront, basicAttributes.targetMask);
		Debug.Log(targetsInViewRadius.Length);
		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			GameObject target = targetsInViewRadius[i].gameObject;
			Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
			//ön görüş açısında neler var onu bulur
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngleFront / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, basicAttributes.obstacleMask))
				{
					visibleTargets.Add(target);
					objectsDictionary.Add(target.tag, target);
				}
			}
			//Arka görüş açısında(6.his veya ses duyma) neler var onu bulur
			else if(Vector3.Angle(-transform.forward, dirToTarget) < viewAngleBack / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				if(viewRadiusBack > dstToTarget)
				{
					if(!Physics.Raycast(transform.position, dirToTarget, dstToTarget, basicAttributes.obstacleMask))
					{
						visibleTargets.Add(target);
						objectsDictionary.Add(target.tag, target);
					}
				}
			}
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
	#endregion
	public void Initialize()
	{
		foodSaturation = Random.Range(basicAttributes.foodSaturationRNG.x, basicAttributes.foodSaturationRNG.y);
		waterSaturation = Random.Range(basicAttributes.waterSaturationRNG.x, basicAttributes.waterSaturationRNG.y);
		reproductiveUrge = Random.Range(basicAttributes.reproductiveUrgeRNG.x, basicAttributes.reproductiveUrgeRNG.y);
		energy = Random.Range(basicAttributes.energyRNG.x, basicAttributes.energyRNG.y);
		//viewingDistance = Random.Range(baseAttributes.viewingDistanceRNG.x, baseAttributes.viewingDistanceRNG.y);
		remainingLifeTime = Random.Range(basicAttributes.remainingLifeTimeRNG.x, basicAttributes.remainingLifeTimeRNG.y);
		energyConsumeSpeed = Random.Range(basicAttributes.energyConsumeSpeedRNG.x, basicAttributes.energyConsumeSpeedRNG.y);
		moveSpeed = Random.Range(basicAttributes.moveSpeedRNG.x, basicAttributes.moveSpeedRNG.y);

		viewAngleFront = Random.Range(basicAttributes.viewAngleFrontRNG.x, basicAttributes.viewAngleFrontRNG.y);
		viewAngleBack = 360-viewAngleFront;	
		
		viewRadiusFront = Random.Range(basicAttributes.viewRadiusFrontRNG.x, basicAttributes.viewRadiusFrontRNG.y);
		viewRadiusBack = Random.Range(basicAttributes.viewRadiusBackRNG.x, basicAttributes.viewRadiusBackRNG.y);
	}

	private void Update()
	{
		CreateDictionary();//Scan and create scanned objects' dictionary
		DeterminePriority();
		ChooseAction();
		Consume();
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

	private void ChooseAction()
	{
		//TODO: burası karışıyor vaziyet al!! bunu daha düzenli yapabilirsin bence?
		if (currentPriority == Priority.Rest)
		{
			Rest();
			return;
		}
		if (currentPriority == Priority.Danger)
		{
			Escape();
		}
		else if (currentPriority == Priority.Mate)
		{
			//Go to mate if you like him/her.
		}
		else if(currentPriority == Priority.Food)
		{
			if (!FindAndConsumeFood())
			{
				Explore();
			}
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

	private void Explore()
	{
		timeToChangeDirection -= Time.deltaTime;

		if (timeToChangeDirection <= 0)
		{
			ChangeDirection();
		}
		transform.rotation = Quaternion.Slerp(transform.rotation, quat, 0.1f);
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f * moveSpeed);

	}

	//find ve consume u seperate et
	private bool FindAndConsumeFood()
	{
		//find food
		GameObject foodFound = null;
		foreach(string food in foods)
		{
			if(objectsDictionary.TryGetValue(food, out foodFound))
			{
				break;
			}
		}
		if (foodFound == null)
		{
			return false;
		}
		//consume food
		Quaternion lookAt = Quaternion.LookRotation(foodFound.transform.position - transform.position, transform.up);
		//TODO: buraları düzenle hardcodingi düzelt.
		if (PriorityNear(foodFound))
		{
			if (Consume(foodFound))
			{
				foodSaturation += 20;
				energy += 20;
				waterSaturation -= 5;
			}
		}
		transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, 0.1f);
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f * moveSpeed);
		return true;
	}

	private bool Consume(GameObject priorityObj)
	{
		//TODO: null kontrolü gerekli mi karar ver, çok yük bindirebilir.
		if (priorityObj != null)
		{
			Destroy(priorityObj);
			return true;
		}
		return false;
	}

	private bool PriorityNear(GameObject priority) => Vector3.Distance(priority.transform.position, transform.position) <= 1f;

	private void Escape()
	{
		GameObject closestDanger = FindClosestDanger();
		//TODO: move to danger's script's hunt/findfood funtion
		closestDanger.transform.LookAt(gameObject.transform);
		transform.rotation = Quaternion.Slerp(transform.rotation, closestDanger.transform.rotation, 0.1f);
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, 0.1f * moveSpeed);
	}

	private GameObject FindClosestDanger()
	{
		float closestDistance = Mathf.Infinity;
		float currentDistance;
		GameObject closestDanger = null;
		GameObject currentDanger;
		//TODO: IJobParallelFor kullanabilirsen kullan bu ve diğer for'lar için.
		foreach (string danger in dangers)
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

	private bool ShouldReplace(Vector3 obj1, Vector3 obj2)
	{
		float distToObj1 = Vector3.Distance(transform.position, obj1);
		float distToObj2 = Vector3.Distance(transform.position, obj2);
		return distToObj1 > distToObj2 ? true : false;
	}
	private void CreateDictionary()
	{
		//TODO: bu fonksiyon da fazla karmaşık düzeltebilirsen düzelt.
		//clear previous values;
		objectsDictionary.Clear();
		List<Collider> hits = Scan.ToList();
		hits.Remove(col);

		
		//TODO: IJobParallelFor kullanabilirsen kullan = job system yani safe-multithreading gibi. Performansı müthiş artırır.
		foreach (Collider collider in hits)
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
	}

	private void DeterminePriority()
	{
		//TODO: burası da cumbersome, belki düzenleyebilirsin.
		foreach(string danger in dangers)
		{
			if (objectsDictionary.ContainsKey(danger))
			{
				currentPriority = Priority.Danger;
				return;
			}
		}

		float maxPriority = Mathf.Min(foodSaturation, waterSaturation, maxReproductiveUrge - reproductiveUrge);

		if (maxPriority > restLimit)
		{
			currentPriority = Priority.Rest;
		}
		else if (maxPriority == foodSaturation)
		{
			currentPriority = Priority.Food;
		}
		else if (maxPriority == waterSaturation)
		{
			currentPriority = Priority.Water;
		}
		else if (reproductiveUrge >= 75)
		{
			currentPriority = Priority.Mate;
		}
	}

	private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, viewingDistance);
	private Collider[] Scan => Physics.OverlapSphere(transform.position, viewingDistance, notGroundLayers);

	//TODO: random hareket için geçici kod güzelini yazınca bunu sil.
	private void CalculateMovement()
	{
		int randX = UnityEngine.Random.Range(-1, 2);
		int randZ = UnityEngine.Random.Range(-1, 2);
		moveTo = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
	}

	//TODO: random hareket için geçici kod güzelini yazınca bunu sil.
	private void Move()
	{
		transform.forward = moveTo - transform.position;
		transform.position = Vector3.Lerp(transform.position, moveTo, .1f);
	}

}
