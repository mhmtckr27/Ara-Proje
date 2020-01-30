/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RabbitRotator : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private LayerMask mask;
    Vector3 oldPosition;
    Vector3 newPosition;
    private void Start()
    {
        StartCoroutine("AlignToTerrainWithDelay", 0.25f);
        oldPosition = transform.position;
    }

    IEnumerator AlignToTerrainWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            newPosition = transform.position;
            AlignToTerrain();
            oldPosition = newPosition;
        }
    }
    private void Update()
    {

    }
    void AlignToTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), -Vector3.up, out hit, 3, mask))
        {
            Vector3 newForward = newPosition - oldPosition;
            transform.rotation = Quaternion.LookRotation(newForward != Vector3.zero ? newForward : transform.forward, hit.normal);
        }

    }
}
