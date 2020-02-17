/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.AI;

public partial class Animal
{
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

	protected void Escape(GameObject closestDanger)
	{
		timeLeftToEscape += Time.deltaTime;
		//Debug.Log(timeLeftToEscape + "   " + EscapeTimer);
		if (timeLeftToEscape >= EscapeTimer)
		{
			Vector3 escapePosition = CalculateEscapeRoute(closestDanger);
			navMeshAgent.SetDestination(escapePosition);
			timeLeftToEscape = 0;
		}
	}
	protected GameObject FindClosestDanger()
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
}
