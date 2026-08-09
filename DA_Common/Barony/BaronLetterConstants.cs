namespace DA_Common.Barony
{
    public readonly struct BaronLetterStatus
    {
        public const string Draft = "Draft";
        public const string Sent = "Sent";

        public static readonly string[] All = { Draft, Sent };
    }

    /// <summary>Where the correspondent lives (flavor / travel context).</summary>
    public readonly struct BaronLetterReplyRegion
    {
        public const string EasternMarch = "Eastern March";
        public const string Empire = "Empire";
        public const string Other = "Other";

        public static readonly string[] All = { EasternMarch, Empire, Other };
    }

    public static class BaronLetterRules
    {
        /// <summary>
        /// Default max inbound letters from the same correspondent in one turn
        /// (Eastern March). Empire / Other use <see cref="MaxInboundFromCorrespondent"/>.
        /// </summary>
        public const int MaxLettersFromSameCorrespondentPerTurn = 3;

        public const int MaxInboundEasternMarchPerTurn = 3;
        public const int MaxInboundEmpirePerTurn = 1;
        public const int MaxInboundOtherPerTurn = 1;

        /// <summary>
        /// Max letters the baron may receive from one correspondent this turn,
        /// by where they live.
        /// </summary>
        public static int MaxInboundFromCorrespondent(string? replyRegion) =>
            (replyRegion ?? "").Trim() switch
            {
                BaronLetterReplyRegion.EasternMarch => MaxInboundEasternMarchPerTurn,
                BaronLetterReplyRegion.Empire => MaxInboundEmpirePerTurn,
                _ => MaxInboundOtherPerTurn,
            };

        /// <summary>
        /// A delivered inbound message counts as one received letter from that correspondent this turn.
        /// Quotas reset automatically when <paramref name="currentTurn"/> advances (Resolve Turn).
        /// </summary>
        public static bool CountsAsReceivedThisTurn(
            bool isDraft,
            bool isInbound,
            int turnNumber,
            int currentTurn)
        {
            if (isDraft || currentTurn <= 0 || !isInbound)
                return false;

            return turnNumber == currentTurn;
        }

        /// <summary>
        /// Baron is blocked from another outbound only while awaiting a reply
        /// to a letter sent <em>this</em> turn. A new quarter unlocks communication again.
        /// </summary>
        public static bool BaronAwaitingReplyThisTurn(
            bool lastIsInbound,
            int lastTurnNumber,
            int currentTurn)
        {
            if (lastIsInbound || currentTurn <= 0)
                return false;
            return lastTurnNumber == currentTurn;
        }

        public static bool SameCorrespondent(
            int? relationIdA,
            string? nameA,
            int? relationIdB,
            string? nameB)
        {
            if (relationIdA is int a && relationIdB is int b && a == b)
                return true;

            var na = (nameA ?? "").Trim();
            var nb = (nameB ?? "").Trim();
            return !string.IsNullOrEmpty(na)
                && string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// In-world letter dates within a season/turn. Simulates courier travel so replies
    /// spread across the quarter instead of all sharing the barony's "current month".
    /// </summary>
    public static class BaronLetterCalendar
    {
        public readonly record struct InWorldDate(int Day, int Month, int Year) : IComparable<InWorldDate>
        {
            public int CompareTo(InWorldDate other)
            {
                var c = Year.CompareTo(other.Year);
                if (c != 0) return c;
                c = Month.CompareTo(other.Month);
                if (c != 0) return c;
                return Day.CompareTo(other.Day);
            }

            public override string ToString() => Format(this);
        }

        public readonly record struct PriorLetter(int Day, int Month, int Year, bool IsInbound);

        /// <summary>Pathfinder-style months that belong to a season/turn.</summary>
        public static int[] MonthsForSeason(string? season) => season?.Trim() switch
        {
            "Spring" => new[] { 3, 4, 5 },
            "Summer" => new[] { 6, 7, 8 },
            "Fall" or "Autumn" => new[] { 9, 10, 11 },
            // Winter turn sits at the end of the year (Kuthona–Calistril).
            _ => new[] { 12, 1, 2 },
        };

        public static string MonthName(int month)
        {
            if (month is >= 1 and <= 12)
                return DA_Common.SD.Calendar.Months[month - 1].Name;
            return month.ToString();
        }

        public static string Format(int day, int month, int year)
        {
            if (day > 0 && month is >= 1 and <= 12)
                return $"{day} {MonthName(month)} {year}";
            if (month is >= 1 and <= 12)
                return $"{MonthName(month)} {year}";
            return year > 0 ? year.ToString() : "";
        }

        public static string Format(InWorldDate d) => Format(d.Day, d.Month, d.Year);

        public static int DaysInMonth(int month)
        {
            if (month is >= 1 and <= 12)
                return DA_Common.SD.Calendar.Months[month - 1].Days;
            return 30;
        }

        /// <summary>
        /// Pick the next letter's in-world date inside the current season, after prior letters,
        /// with courier delays (longer for Empire / Other than Eastern March).
        /// </summary>
        public static InWorldDate AssignNext(
            string season,
            int year,
            string? replyRegion,
            IEnumerable<PriorLetter> priorSent,
            bool nextIsInbound,
            Random? rng = null)
        {
            rng ??= Random.Shared;
            var months = MonthsForSeason(season);
            var seasonStart = new InWorldDate(1, months[0], year);
            var seasonEnd = new InWorldDate(DaysInMonth(months[^1]), months[^1], year);

            var priors = priorSent
                .Where(p => p.Year > 0 && p.Month is >= 1 and <= 12)
                .Select(p => (
                    Date: new InWorldDate(p.Day > 0 ? p.Day : MidDay(p.Month), p.Month, p.Year),
                    p.IsInbound))
                .OrderBy(p => p.Date)
                .ToList();

            InWorldDate next;
            if (priors.Count == 0)
            {
                // First letter of the exchange this season — early in the quarter.
                next = RandomDateInRange(seasonStart, AddDays(seasonStart, 35), rng);
            }
            else
            {
                var last = priors[^1];
                var delay = NextDelayDays(last.IsInbound, nextIsInbound, replyRegion, rng);
                next = AddDays(last.Date, delay);
            }

            if (next.CompareTo(seasonEnd) > 0)
                next = seasonEnd;
            if (next.CompareTo(seasonStart) < 0)
                next = seasonStart;

            return next;
        }

        private static int MidDay(int month) => Math.Max(1, DaysInMonth(month) / 2);

        private static int NextDelayDays(bool lastWasInbound, bool nextIsInbound, string? region, Random rng)
        {
            // Courier travel vs short writing/turnaround time.
            var travel = TravelDays(region, rng);
            if (!lastWasInbound && nextIsInbound)
                return travel; // baron→ then reply← : letter was on the road
            if (lastWasInbound && !nextIsInbound)
                return rng.Next(2, 9); // received mail, write a reply a few days later
            if (!lastWasInbound && !nextIsInbound)
                return rng.Next(3, 12); // another outbound without waiting for reply
            // inbound after inbound — another courier run
            return travel + rng.Next(0, 8);
        }

        private static int TravelDays(string? region, Random rng) => region?.Trim() switch
        {
            BaronLetterReplyRegion.EasternMarch => rng.Next(10, 19), // ~1.5–2.5 weeks
            BaronLetterReplyRegion.Empire => rng.Next(18, 29),       // ~3–4 weeks
            _ => rng.Next(14, 25),                                  // Other / unknown
        };

        private static InWorldDate RandomDateInRange(InWorldDate start, InWorldDate end, Random rng)
        {
            if (end.CompareTo(start) <= 0)
                return start;
            var span = DiffDays(start, end);
            return AddDays(start, rng.Next(0, Math.Max(1, span + 1)));
        }

        public static InWorldDate AddDays(InWorldDate date, int days)
        {
            var day = date.Day;
            var month = date.Month;
            var year = date.Year;
            day += days;
            while (true)
            {
                var dim = DaysInMonth(month);
                if (day <= dim) break;
                day -= dim;
                month++;
                if (month > 12)
                {
                    month = 1;
                    year++;
                }
            }

            while (day < 1)
            {
                month--;
                if (month < 1)
                {
                    month = 12;
                    year--;
                }
                day += DaysInMonth(month);
            }

            return new InWorldDate(day, month, year);
        }

        public static int DiffDays(InWorldDate from, InWorldDate to)
        {
            // Small ranges (within a season) — linear scan is fine.
            if (to.CompareTo(from) <= 0) return 0;
            var n = 0;
            var cur = from;
            while (cur.CompareTo(to) < 0 && n < 400)
            {
                cur = AddDays(cur, 1);
                n++;
            }
            return n;
        }
    }
}
