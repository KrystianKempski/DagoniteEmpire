using DA_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;

namespace DA_Models.ComponentModels
{
    public class DateModel
    {
        public int Day { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int Year { get; set; } = SD.Calendar.StartYear;
        private int AllDays 
        { get
            {
                int days = Day;

                foreach(var month in SD.Calendar.Months)
                {
                    if (month.Number >= Month)
                        break;
                    days += month.Days;
                    
                }

                return ((Year - SD.Calendar.StartYear) * 365) + days;
            } 
        }
        public string DateString { get => SD.Calendar.GetDate(Day, Month, Year); }
        public DateModel(int day, int month, int year = SD.Calendar.StartYear)
        {
            Day = day;
            Month = month;
            Year = year;
        }
        public static int operator -(DateModel a, DateModel b)
        {
            if (a.AllDays > b.AllDays)
            {
                return a.AllDays-b.AllDays;
            }
            else
            {
                return b.AllDays - a.AllDays;
            }
        }
        public static int operator -(DateModel a, int b)
        {
            if (a.AllDays > b)
            {
                return a.AllDays - b;
            }
            else
            {
                return 0;
            }
        }
        public static DateModel operator +(DateModel a, int day)
        {
            while (day + a.Day > SD.Calendar.Months[a.Month-1].Days)
            {
                day = day - (SD.Calendar.Months[a.Month - 1].Days);
                a.Month++;
            }
            a.Day += day;
            return a;
        }
    }
}
