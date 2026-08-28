
namespace DA_Common
{
    public class DateModel
    {
        public int Day { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int Year { get; set; } = SD.Calendar.StartYear;
        public int AllDays  { get => GetDaysFromDate(this);  }

        public DateModel(int Date)
        {
            if (Date == 0)
                return;

            int yearDelta;
            int dayOfYear;

            if (Date > 0)
            {
                // Epoch: DateNumber 1 = StartYear, day 1 of month 1.
                // (Date-1) avoids Dec 31 mapping to day 0 of the next year.
                yearDelta = (Date - 1) / 365;
                dayOfYear = ((Date - 1) % 365) + 1;
            }
            else
            {
                // Negative offsets = years before StartYear.
                // Note: AllDays == 0 is reserved for "unset" (defaults to StartYear);
                // Dec 31 of StartYear-1 also computes to 0 and cannot round-trip.
                yearDelta = (int)Math.Floor((Date - 1) / 365.0);
                dayOfYear = Date - yearDelta * 365;
            }

            Year = SD.Calendar.StartYear + yearDelta;
            Month = 1;
            Day = 0;
            var remaining = dayOfYear;

            foreach (var m in SD.Calendar.Months)
            {
                if (remaining <= m.Days)
                    break;
                remaining -= m.Days;
                Month++;
            }
            Day = remaining;
        }

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
        public static int GetDaysFromDate(DateModel date)
        {
            if (date.Day < 1 || date.Month < 1)
                return 0;

            int days = date.Day;

            foreach (var m in SD.Calendar.Months)
            {
                if (m.Number >= date.Month)
                    break;
                days += m.Days;
            }

            return ((date.Year - SD.Calendar.StartYear) * 365) + days;
        }
        public static DateModel? GetDateFromDays(int days)
        {
            if (days == 0)
                return null;

            // Same epoch as DateModel(int) / GetDaysFromDate.
            return new DateModel(days);
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
