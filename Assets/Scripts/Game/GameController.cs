/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameTime gameTime;

    void Start()
    {
        SpawnObjects();
        gameTime.Init();
        //Time.timeScale = 50;
        StartCoroutine(gameTime.UpdateTimeCoroutine());
    }

	// Update is called once per frame
	void Update()
    {
        Debug.Log(gameTime.ToString());
    }

    [System.Serializable]
    private class GameTime
    {
        #region DayTime Constants
        private const int EarlyMorning = 4;
        private const int Sunrise = 6;
        private const int Noon = 12;
        private const int Sunset = 18;
        private const int Evening = 21;
        private const int LateEvening = 23;
        private const int Midnight = 0;
		#endregion

		#region Season Constants
		private const int Spring = 3;
        private const int Summer = 6;
        private const int Fall = 9;
        private const int Winter = 12;
		#endregion

		#region DaysInMonth Constants
        //TODO: may be implement 29 day feb and leap year layer.
		private const int DaysInLongerMonths = 31;
        private const int DaysInShorterMonths = 30;
        private const int DaysInFebruary = 28;

        private readonly int[] DaysIntMonths = { DaysInJanuary, DaysInFebruary, DaysInMarch, DaysInApril, DaysInMay, DaysInJune, DaysInJuly, DaysInAugust, DaysInSeptember, DaysInOctober, DaysInNovember, DaysInDecember };
        private const int DaysInJanuary = DaysInLongerMonths;
        private const int DaysInMarch = DaysInLongerMonths;
        private const int DaysInMay = DaysInLongerMonths;
        private const int DaysInJuly = DaysInLongerMonths;
        private const int DaysInAugust = DaysInLongerMonths;
        private const int DaysInOctober = DaysInLongerMonths;
        private const int DaysInDecember = DaysInLongerMonths;

        private const int DaysInApril = DaysInShorterMonths;
        private const int DaysInJune = DaysInShorterMonths;
        private const int DaysInSeptember = DaysInShorterMonths;
        private const int DaysInNovember = DaysInShorterMonths;
		#endregion
		public enum Month
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
        public enum DayTime
        {
            //Early Morning = 4,
            //Sunrise = 6,
            //Noon = 12,
            //Sunset = 18,
            //LateEvening = 21,
            //Midnight = 0,
            EarlyMorning,
            Morning,
            Afternoon,
            Evening,
            LateEvening,
            Night
        }
        public enum Season
        {
            Spring,
            Summer,
            Fall,
            Winter
        }


        private int second;
        [SerializeField][Range(0,59)] private int minute;
        [SerializeField][Range(0,23)] private int hour;
        [SerializeField][Range(1,12)] private int month;
        [SerializeField] private int year;
        [SerializeField][Range(1, 31)] private int dayOfMonth;
        private int dayOfYear;
        private DayTime currentDayTime;
        private Month currentMonth;
        private Season currentSeason;

        public int p_Month 
        { 
            get => month;
            set
            {
                month = value;
                currentMonth = (Month)p_Month;
            } 
        }

        public void Init()
        {
            second = 1; //to get rid of extra checks in currentdaytime control

            for (int i = 0; i < p_Month; i++)
            {
                dayOfYear += DaysIntMonths[i];
            }

            currentMonth = (Month)p_Month;

            #region Daytime Init
            if (hour >= Midnight && hour < EarlyMorning)
            {
                currentDayTime = DayTime.Night;
            }
            else if (hour >= EarlyMorning && hour < Sunrise)
            {
                currentDayTime = DayTime.EarlyMorning;
            }
            else if (hour >= Sunrise && hour < Noon)
            {
                currentDayTime = DayTime.Morning;
            }
            else if (hour >= Noon && hour < Sunset)
            {
                currentDayTime = DayTime.Afternoon;
            }
            else if (hour >= Sunset && hour < Evening)
            {
                currentDayTime = DayTime.Evening;
            }
            else if (hour >= Evening && hour < LateEvening)
            {
                currentDayTime = DayTime.LateEvening;
            }
            else if (hour >= LateEvening || hour < EarlyMorning)
            {
                currentDayTime = DayTime.Night;
            }
            #endregion

            #region Season Init
            if (p_Month == Winter || p_Month < Spring)
            {
                currentSeason = Season.Winter;
            }
            else if (p_Month >= Spring && p_Month < Summer)
            {
                currentSeason = Season.Spring;
            }
            else if (p_Month >= Summer && p_Month < Fall)
            {
                currentSeason = Season.Summer;
            }
            else if (p_Month >= Fall && p_Month < Winter)
            {
                currentSeason = Season.Fall;
            }
            #endregion
        }
        public IEnumerator UpdateTimeCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                UpdateTime();
            }
        }

        private void UpdateTime()
        {
            second++;
            if (second == 60)
            {
                second = 0;
                minute++;
                if(minute == 60)
                {
                    minute = 0;
                    hour++;
                    if(hour == 24)
                    {
                        hour = 0;
                        dayOfMonth++;
                        if(dayOfMonth == DaysIntMonths[(int) currentMonth - 1] + 1)
                        {
                            dayOfMonth = 1;
                            p_Month++;
                            if(p_Month == 13)
                            {
                                p_Month = 1;
                                year++;
                            }
                        }
                    }
                }
            }
        }
        public override string ToString()
        {
            //"DayTime:\t" + currentDayTime + "\n" +
            //          "Month:\t\t" + currentMonth + "\n" +
            //          "Season:\t\t" + currentSeason + "\n" +
            //          "DayOfYear:\t" + dayOfYear + "\n" +
            return 
                   (hour < 10 ? ("0" + hour) : "" + hour) + ":" + (minute < 10 ? ("0" + minute) : "" + minute) + ":" + (second < 10 ? ("0" + second) : "" + second) + "\t" +
                   (dayOfMonth < 10 ? ("0" + dayOfMonth) : "" + dayOfMonth) + "." + (p_Month < 10 ? ("0" + p_Month) : "" + p_Month) + "." + year;
        }
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
