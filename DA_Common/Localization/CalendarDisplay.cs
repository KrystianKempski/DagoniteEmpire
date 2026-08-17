using Microsoft.Extensions.Localization;

namespace DA_Common.Localization;

/// <summary>Display-time localization for in-world calendar dates.</summary>
public static class CalendarDisplay
{
    public static string Format(DateModel date)
        => Format(date, localizer: null);

    public static string Format(DateModel date, IStringLocalizer? localizer)
    {
        if (date.Day == 0 || date.Month == 0)
            return string.Empty;

        var weekday = SD.Calendar.GetDayOfWeek(date.Day, date.Month);
        var monthName = SD.Calendar.Months[date.Month - 1].Name;
        var locWeekday = localizer is null ? LocCatalog.Name(weekday) : LocCatalog.Name(weekday, localizer);
        var locMonth = localizer is null ? LocCatalog.Name(monthName) : LocCatalog.Name(monthName, localizer);

        if (date.Year > 0)
        {
            return localizer is null
                ? Loc.T("{0}, {1}. {2}, year {3}", locWeekday, date.Day, locMonth, date.Year)
                : localizer["{0}, {1}. {2}, year {3}", locWeekday, date.Day, locMonth, date.Year].Value;
        }

        return localizer is null
            ? Loc.T("{0}, {1}. {2}", locWeekday, date.Day, locMonth)
            : localizer["{0}, {1}. {2}", locWeekday, date.Day, locMonth].Value;
    }
}
