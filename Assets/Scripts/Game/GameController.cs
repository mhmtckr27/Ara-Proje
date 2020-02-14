/*
Code by Hayri Cakir
www.hayricakir.com
*/
using com.hayricakir;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject grass;
    [SerializeField] private GameObject bunny;
    [SerializeField] private GameObject fox;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private GameObject emptyObject;
    List<GameObject> rabbits = new List<GameObject>();
    [SerializeField] public Calendar currentGameTime;
    [SerializeField] public TextMeshProUGUI currentGameTimeText;
    [SerializeField] [Range(1, 10)] public float animalAgeUpdatePeriod;
    [SerializeField] public TimeFlowRate timeFlowRate;

	public static GameController Instance { get; private set; }

	[System.Serializable]
    public struct TimeFlowRate
    {
        [SerializeField] [Range(0,10)] public float timeFlowRate;
        [SerializeField] public TimeType timeType;
    }
    public enum TimeType
    {
        Second,
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    void Awake()
    {
        //SpawnObjects();        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        currentGameTime.Init(1f);
        //Time.timeScale = 50;
    }

    #region spawning stuff
    private void SpawnObjects()
    {
        SpawnWater();
        SpawnBunny();
        SpawnFox();
        SpawnGrass();
    }

    private void SpawnGrass()
    {
        for (int i = 0; i < 125; i++)
        {
            float x = UnityEngine.Random.Range(150, 350);
            float z = UnityEngine.Random.Range(150, 350);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 19)
            {
                Instantiate(grass, hit.point + Vector3.down * .05f, Quaternion.identity).transform.up = hit.normal;
            }
        }
    }

    private void SpawnFox()
    {
        for (int i = 0; i < 100; i++)
        {
            float x = UnityEngine.Random.Range(200, 320);
            float z = UnityEngine.Random.Range(200, 320);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, groundLayer))
            {
                Instantiate(fox, hit.point + Vector3.up * .5f, Quaternion.identity);
            }
        }
    }

    private void SpawnBunny()
    {
        for (int i = 0; i < 250; i++)
        {
            float x = UnityEngine.Random.Range(150, 350);
            float z = UnityEngine.Random.Range(150, 350);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 19)
            {
                Instantiate(bunny, hit.point + Vector3.up * 0.25f, Quaternion.identity);
                rabbits.Add(bunny);
            }
        }
    }

    private void SpawnWater()
    {
        for (int i = 150; i < 350; i++)
        {
            for (int j = 150; j < 350; j++)
            {
                RaycastHit hit;

                if (Physics.Raycast(new Vector3(i, -1, j), Vector3.down, out hit, Mathf.Infinity, waterLayer) && hit.distance > 18.6 && hit.distance < 18.9)
                {
                    //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = new Vector3(i, hit.point.y, j);
                    Instantiate(emptyObject).transform.position = new Vector3(i, hit.point.y, j);
                }
            }
        }
    }
    #endregion
}
