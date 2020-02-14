/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace com.hayricakir
{
    [System.Serializable]
    public class Calendar
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


        public static DateTime CurrentDateTime;

        [SerializeField] private int year;
        [SerializeField] [Range(1, 12)] private int month;
        [SerializeField] [Range(1, 31)] private int day;
        [SerializeField] [Range(0, 23)] private int hour;
        [SerializeField] [Range(0, 59)] private int minute;
        [SerializeField] [Range(0, 59)] private int second;
        [SerializeField] private GameController gameController;

        [HideInInspector] public Season currentSeason;
        [HideInInspector] public Month currentMonth;
        [HideInInspector] public DayTime currentDayTime;

        public void Init(float secondsToUpdate)
        {
            CurrentDateTime = new DateTime(year, month, day, hour, minute, second);
            currentMonth = (Month)CurrentDateTime.Month;

            #region Daytime Init
            if (CurrentDateTime.Hour >= Midnight && CurrentDateTime.Hour < EarlyMorning)
            {
                currentDayTime = DayTime.Night;
            }
            else if (CurrentDateTime.Hour >= EarlyMorning && CurrentDateTime.Hour < Sunrise)
            {
                currentDayTime = DayTime.EarlyMorning;
            }
            else if (CurrentDateTime.Hour >= Sunrise && CurrentDateTime.Hour < Noon)
            {
                currentDayTime = DayTime.Morning;
            }
            else if (CurrentDateTime.Hour >= Noon && CurrentDateTime.Hour < Sunset)
            {
                currentDayTime = DayTime.Afternoon;
            }
            else if (CurrentDateTime.Hour >= Sunset && CurrentDateTime.Hour < Evening)
            {
                currentDayTime = DayTime.Evening;
            }
            else if (CurrentDateTime.Hour >= Evening && CurrentDateTime.Hour < LateEvening)
            {
                currentDayTime = DayTime.LateEvening;
            }
            else if (CurrentDateTime.Hour >= LateEvening || CurrentDateTime.Hour < EarlyMorning)
            {
                currentDayTime = DayTime.Night;
            }
            #endregion

            #region Season Init
            if (CurrentDateTime.Month == Winter || CurrentDateTime.Month < Spring)
            {
                currentSeason = Season.Winter;
            }
            else if (CurrentDateTime.Month >= Spring && CurrentDateTime.Month < Summer)
            {
                currentSeason = Season.Spring;
            }
            else if (CurrentDateTime.Month >= Summer && CurrentDateTime.Month < Fall)
            {
                currentSeason = Season.Summer;
            }
            else if (CurrentDateTime.Month >= Fall && CurrentDateTime.Month < Winter)
            {
                currentSeason = Season.Fall;
            }
            #endregion
            gameController.StartCoroutine(UpdateTimeCoroutine(secondsToUpdate));
        }
        public IEnumerator UpdateTimeCoroutine(float secondsToUpdate)
        {
            while (true)
            {
                yield return new WaitForSeconds(secondsToUpdate);
                UpdateTime(GameController.Instance.timeFlowRate.timeFlowRate);
            }
        }

        private void UpdateTime(float amountToAdd)
        {
            switch(GameController.Instance.timeFlowRate.timeType)
            {
                case GameController.TimeType.Second:
                    CurrentDateTime = CurrentDateTime.AddSeconds(amountToAdd);
                    break;
                case GameController.TimeType.Minute:
                    CurrentDateTime = CurrentDateTime.AddMinutes(amountToAdd);
                    break;
                case GameController.TimeType.Hour:
                    CurrentDateTime = CurrentDateTime.AddHours(amountToAdd);
                    break;
                case GameController.TimeType.Day:
                    CurrentDateTime = CurrentDateTime.AddDays(amountToAdd);
                    break;
                case GameController.TimeType.Month:
                    CurrentDateTime = CurrentDateTime.AddMonths((int)amountToAdd);
                    break;
                case GameController.TimeType.Year:
                    CurrentDateTime = CurrentDateTime.AddYears((int)amountToAdd);
                    break;
            }
                    CurrentDateTime.AddSeconds(amountToAdd);
            gameController.currentGameTimeText.text = ToString();
        }

        public static TimeSpan GetDifference(DateTime fromDateTime, DateTime toDateTime)
        {
            return toDateTime > fromDateTime ? (toDateTime - fromDateTime) : fromDateTime - toDateTime;
        }

        public override string ToString()
        {
            return CurrentDateTime.ToString("HH:mm:ss dd/MM/yyyy") + "\n" + currentSeason + "/" + currentMonth + "\n" + currentDayTime;
        }

        //public override string ToString()
        //{
        //    //"DayTime:\t" + currentDayTime + "\n" +
        //    //          "Month:\t\t" + currentMonth + "\n" +
        //    //          "Season:\t\t" + currentSeason + "\n" +
        //    //          "DayOfYear:\t" + dayOfYear + "\n" +
        //    return
        //           (hour < 10 ? ("0" + hour) : "" + hour) + ":" + (minute < 10 ? ("0" + minute) : "" + minute) + ":" + (second < 10 ? ("0" + second) : "" + second) + "\t" +
        //           (dayOfMonth < 10 ? ("0" + dayOfMonth) : "" + dayOfMonth) + "." + (p_Month < 10 ? ("0" + p_Month) : "" + p_Month) + "." + year;
        //}
    }
}
