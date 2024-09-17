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
        private int AllDays  { get=> GetDaysFromDate(Day,Month,Year);   }
        
        public DateModel(int day, int month, int year = SD.Calendar.StartYear)
        {
            Day = day;
            Month = month;
            Year = year;
        }
        public DateModel(DateModel newDate)
        {
            Day = newDate.Day;
            Month = newDate.Month;
            Year = newDate.Year;
        }
        public static int GetDaysFromDate(int day, int month, int year)
        {
            if (day < 1 || month < 1 || year < SD.Calendar.StartYear)
                return 0;
            int days = day;

            foreach (var m in SD.Calendar.Months)
            {
                if (m.Number >= month)
                    break;
                days += m.Days;

            }

            return ((year - SD.Calendar.StartYear) * 365) + days;
        }
        public static DateModel? GetDateFromDays(int days)
        {
            if (days<1)
                return null;

            int year = days / 365;
            int month = 1;
            int day = 0;
            days = days % 365;

            foreach (var m in SD.Calendar.Months)
            {
                if (days <= m.Days)
                    break;
                days -= m.Days;
                month++;
            }
            day = days;

            return new DateModel(day,month,year);
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
        public static bool operator >(DateModel a, DateModel b)
        {
            if (a.AllDays > b.AllDays)
                return true;
            else
                return false;
        }
       
        public static bool operator <(DateModel a, DateModel b)
        {
            if (a.AllDays < b.AllDays)
                return true;
            else
                return false;
        }
        public static bool operator >=(DateModel a, DateModel b)
        {
            if (a.AllDays >= b.AllDays)
                return true;
            else
                return false;
        }
        public static bool operator <=(DateModel a, DateModel b)
        {
            if (a.AllDays <= b.AllDays)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return SD.Calendar.GetDate(Day, Month, Year);
        }
    }
}
