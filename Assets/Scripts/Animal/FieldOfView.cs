﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Jobs;

public class FieldOfView : MonoBehaviour 
{
	private const float maxViewingDistance = 20f;

	[HideInInspector] public float viewAngleDanger;
	[HideInInspector] public float viewRadiusFront;
	[HideInInspector] public float viewAngleFront;
	[HideInInspector] public float viewRadiusBack;
	[HideInInspector] public float viewAngleBack;

	[Header("Parent Related Stuff")]
	[SerializeField] public Animal animalScript;
	[SerializeField] private new Collider collider;

	[Header("Danger View")]
	[Tooltip("Escape view, kind of a survival instinct(the more survival instinct, the lower angle. So it can escape without zigzags.)")]
	[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleDangerRNG;

	[Header("Front View")]
	[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusFrontRNG;
	[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleFrontRNG;

	//TODO: isi bitince sil. arka gorus on gorusun tumleyeni olacak.
	[Header("Back View")]
	[SerializeField] [MinMaxSlider(0, maxViewingDistance)] public Vector2 viewRadiusBackRNG;
	[SerializeField] [MinMaxSlider(0, 360)] public Vector2 viewAngleBackRNG;
	
	[Space][Space]
	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();

	#if UNITY_EDITOR
	private void OnValidate()
	{
		viewAngleBack = 360 - viewAngleFront;
		//Initialize();
	}
	#endif

	private void Awake() 
	{
		Initialize();
		StartCoroutine ("FindTargetsWithDelay", .2f);

		GameController.animalFOVs.Add(this);
	}

	private void Initialize()
	{
		viewAngleDanger = Random.Range(viewAngleDangerRNG.x, viewAngleDangerRNG.y);
		viewAngleFront = Random.Range(viewAngleFrontRNG.x, viewAngleFrontRNG.y);
		viewAngleBack = 360 - viewAngleFront;

		viewRadiusFront = Random.Range(viewRadiusFrontRNG.x, viewRadiusFrontRNG.y);
		viewRadiusBack = Random.Range(viewRadiusBackRNG.x, viewRadiusFront);
	}

	private IEnumerator FindTargetsWithDelay(float delay) 
	{
		while (true) 
		{
			yield return new WaitForSeconds (delay);
			//FindVisibleTargets();
		}
	}
	public struct FindVisibleTargetsJob : IJob
	{
		public void Execute()
		{
			FindVisibleTargets();
		}

	}

	private Collider[] ScanFieldOfView => Physics.OverlapSphere(transform.position, viewRadiusFront, targetMask, QueryTriggerInteraction.Collide);

	public void FindVisibleTargets()
	{
		animalScript.objectsDictionary.Clear();
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

				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
				{
					if (IsDuplicate(target))
					{
						AddCloserOneToDictionary(target);
					}
					else
					{
						animalScript.objectsDictionary.Add(target.tag, target.gameObject);
					}
				}
			}
			//Arka görüş açısında neler var onu bulur
			else if (Vector3.Angle(-transform.forward, dirToTarget) < viewAngleBack / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				if (viewRadiusBack > dstToTarget)
				{
					if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
					{
						if (IsDuplicate(target))
						{
							AddCloserOneToDictionary(target);
						}
						else
						{
							animalScript.objectsDictionary.Add(target.tag, target.gameObject);
						}
					}
				}
			}
		}
	}
	private bool IsDuplicate(Collider collider) => animalScript.objectsDictionary.ContainsKey(collider.tag);

	private void AddCloserOneToDictionary(Collider collider)
	{
		//aynı tipte varlık onceden eklenmisse, hangisi yakınsa o bulunarak guncellenir.
		GameObject objectInDictionary;
		animalScript.objectsDictionary.TryGetValue(collider.tag, out objectInDictionary);
		bool shouldReplace = ShouldReplace(objectInDictionary.transform.position, collider.gameObject.transform.position);
		if (shouldReplace)
		{
			animalScript.objectsDictionary[objectInDictionary.tag] = collider.gameObject;
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

	private bool ShouldReplace(Vector3 obj1, Vector3 obj2)
	{
		//float distToObj1 = (transform.position - obj1).sqrMagnitude;
		float distToObj1 = Vector3.Distance(transform.position, obj1);
		float distToObj2 = Vector3.Distance(transform.position, obj2);
		//float distToObj2 = (transform.position - obj2).sqrMagnitude;
		return distToObj1 > distToObj2 ? true : false;
	}
}
