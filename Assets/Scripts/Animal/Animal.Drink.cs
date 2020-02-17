/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

public partial class Animal
{
	private GameObject FindWater()
	{
		GameObject waterFound;
		objectsDictionary.TryGetValue(waterTag, out waterFound);
		return waterFound;
	}
	protected bool DrinkWater()
	{
		GameObject waterFound = FindWater();
		if (waterFound == null)
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
}
