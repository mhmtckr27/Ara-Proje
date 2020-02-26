/*
Code by Hayri Cakir
www.hayricakir.com
*/
using com.hayricakir;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    public static List<FieldOfView> animalFOVs = new List<FieldOfView>();
    public static List<Rotator> animalRotators = new List<Rotator>();
    public static List<Animal> animals = new List<Animal>();
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
        SpawnObjects();        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        currentGameTime.Init(1f);
        //Time.timeScale = 100;
        
    }
    private void FixedUpdate()
    {
        //foreach (FieldOfView fov in animalFOVs)
        //{
        //    if(fov == null)
        //    {
        //        continue;
        //    }
        //    ThreadStart thread = delegate
        //    {
        //        fov.FindVisibleTargets();
        //    };
        //    thread.Invoke();
        //}
        //foreach (Rotator rotator in animalRotators)
        //{
        //    if (rotator == null)
        //    {
        //        continue;
        //    }
        //    ThreadStart thread = delegate
        //    {
        //        rotator.AlignToTerrain();
        //    };
        //    thread.Invoke();
        //}
        //foreach (Animal animal in animals)
        //{
        //    if (animal == null)
        //    {
        //        continue;
        //    }
        //    ThreadStart thread = delegate
        //    {
        //        animal.DeterminePriority();
        //        animal.ChooseAction();
        //    };
        //    thread.Invoke();
        //}
    }
    #region spawning stuff
    private void SpawnObjects()
    {
        //SpawnWater();
        SpawnBunny();
        SpawnFox();
        SpawnGrass();
    }
    private void SpawnGrass()
    {
        for (int i = 0; i < 500; i++)
        {
            float x = UnityEngine.Random.Range(-100, 100);
            float z = UnityEngine.Random.Range(-100, 100);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 92)
            {
                Instantiate(grass, hit.point + Vector3.down * .05f, Quaternion.identity).transform.up = hit.normal;
            }
        }
    }

    private void SpawnFox()
    {
        int foxCount = 0;
        for (int i = 0; i < 500; i++)
        {
            float x = UnityEngine.Random.Range(-100, 100);
            float z = UnityEngine.Random.Range(-100, 100);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 92)
            {
                Instantiate(fox, hit.point + Vector3.up * .5f, Quaternion.identity);
                foxCount++;
            }
        }
        Debug.Log("total fox:" + foxCount);
    }

    private void SpawnBunny()
    {
        int bunnyCount = 0;
        for (int i = 0; i < 500; i++)
        {
            float x = UnityEngine.Random.Range(-100, 100);
            float z = UnityEngine.Random.Range(-100, 100);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 92)
            {
                Instantiate(bunny, hit.point + Vector3.up * 0.25f, Quaternion.identity);
                rabbits.Add(bunny);
                bunnyCount++;
            }
        }
        Debug.Log("total bunny:" + bunnyCount);

    }

    private void SpawnWater()
    {
        for (int i = 150; i< 350; i++)
        {
            for (int j = 150; j< 350; j++)
            {
                RaycastHit hit;

                if (Physics.Raycast(new Vector3(i, -1, j), Vector3.down, out hit, Mathf.Infinity, waterLayer) && hit.distance > 18.6 && hit.distance< 18.9)
                {
                    //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = new Vector3(i, hit.point.y, j);
                    Instantiate(emptyObject).transform.position = new Vector3(i, hit.point.y, j);
}
            }
        }
    }
    #endregion
}
