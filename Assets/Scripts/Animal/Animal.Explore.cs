/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.AI;

public partial class Animal
{
	public virtual void PriorityExplore()
	{
		Explore();
	}
	private Vector3 RandomNavSphere(Vector3 origin, float dist, LayerMask layerMask)
	{
		Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;

		randDirection += origin;

		NavMeshHit navHit;

		NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);

		return navHit.position;
	}
	protected void Explore()
	{
		timeLeftToExplore += Time.deltaTime;
		animalStatsUI.exploreTimerText.text = "Explore timer: " + ((ExploreTimer - timeLeftToExplore).ToString("F1") + "/" + MAXExploreTimer);
		if (timeLeftToExplore >= ExploreTimer)
		{
			Vector3 newPos = RandomNavSphere(transform.position, ExploreRadius, -1);
			navMeshAgent.SetDestination(newPos);
			timeLeftToExplore = 0;
		}
	}
}
