/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Rotator : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private NavMeshAgent parentAgent;
    private void Start()
    {
        StartCoroutine("AlignToTerrainWithDelay", .25f);
    }

    IEnumerator AlignToTerrainWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            AlignToTerrain();
        }
    }
    void AlignToTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), -Vector3.up, out hit, 3, mask))
        {
            Quaternion lookRotation = Quaternion.LookRotation(parentAgent.velocity != Vector3.zero ? parentAgent.velocity : transform.forward, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, .75f);
        }

    }
}
