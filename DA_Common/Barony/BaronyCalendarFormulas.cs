namespace DA_Common.Barony
{
    /// <summary>Advances barony calendar: one turn = one season.</summary>
    public static class BaronyCalendarFormulas
    {
        public static readonly string[] SeasonOrder = { "Winter", "Spring", "Summer", "Fall" };

        public static string NormalizeSeason(string? season)
        {
            var s = season?.Trim() ?? "";
            if (string.Equals(s, "Autumn", StringComparison.OrdinalIgnoreCase))
                return "Fall";
            foreach (var known in SeasonOrder)
            {
                if (string.Equals(s, known, StringComparison.OrdinalIgnoreCase))
                    return known;
            }
            return "Winter";
        }

        public static (int Year, int Month, int TurnNumber, string Season) AdvanceOneTurn(
            int year, int month, int turnNumber, string? season)
        {
            var current = NormalizeSeason(season);
            var idx = Array.FindIndex(SeasonOrder, s => s == current);
            if (idx < 0)
                idx = 0;

            var nextIdx = (idx + 1) % SeasonOrder.Length;
            var nextSeason = SeasonOrder[nextIdx];
            var nextYear = year;
            if (nextIdx == 0)
                nextYear = year + 1;

            var months = BaronLetterCalendar.MonthsForSeason(nextSeason);
            var nextMonth = months.Length > 0 ? months[0] : 1;

            return (nextYear, nextMonth, turnNumber + 1, nextSeason);
        }
    }
}
