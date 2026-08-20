using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using System.Text.Json;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Built-in decrees that exist for every barony (work calendar policies).
    /// Seeded on create and backfilled on Domain Panel load.
    /// </summary>
    public static class PermanentDecreesSeeder
    {
        public const string FewFreeDaysName = "Few free days";
        public const string ManyFreeDaysName = "Many free days";

        public sealed record SeedEntry(
            string Name,
            string Description,
            bool DefaultActive,
            PpbVector Additive,
            PpbVector Percent);

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                Name: FewFreeDaysName,
                Description: "Strict work calendar — fewer holidays. Boosts the economy at the cost of loyalty and stability.",
                DefaultActive: false,
                Additive: Additive(loyalty: -5m, stability: -5m),
                Percent: Percent(economy: 10m)),
            new(
                Name: ManyFreeDaysName,
                Description: "Generous work calendar — many holidays. Softens the economy, raises loyalty and stability.",
                DefaultActive: false,
                Additive: Additive(loyalty: 3m, stability: 3m),
                Percent: Percent(economy: -10m)),
        ];

        public static bool IsPermanent(string? name) =>
            !string.IsNullOrWhiteSpace(name)
            && Defaults.Any(d => string.Equals(d.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

        public static bool IsHolidayPolicy(string? name) => IsPermanent(name);

        /// <summary>
        /// Adds any missing permanent decrees. Syncs PPB + description for existing ones
        /// (keeps each barony’s IsActive choice). Removes accidental duplicates
        /// (e.g. from concurrent GetOverview backfills).
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existing = ctx.Decrees
                .Where(d => d.BaronyId == baronyId)
                .ToList();

            foreach (var entry in Defaults)
            {
                var matches = existing
                    .Where(d => string.Equals(d.Name, entry.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(d => d.IsActive)
                    .ThenBy(d => d.Id)
                    .ToList();

                if (matches.Count == 0)
                {
                    var added = new Decree
                    {
                        BaronyId = baronyId,
                        Name = entry.Name,
                        Description = entry.Description,
                        AdditiveJson = Ser(entry.Additive),
                        PercentJson = Ser(entry.Percent),
                        IsActive = entry.DefaultActive,
                    };
                    ctx.Decrees.Add(added);
                    existing.Add(added);
                    continue;
                }

                var keep = matches[0];
                keep.Name = entry.Name;
                keep.Description = entry.Description;
                keep.AdditiveJson = Ser(entry.Additive);
                keep.PercentJson = Ser(entry.Percent);

                foreach (var dup in matches.Skip(1))
                {
                    // Prefer keeping an active copy; fold active flag onto survivor.
                    if (dup.IsActive)
                        keep.IsActive = true;
                    ctx.Decrees.Remove(dup);
                    existing.Remove(dup);
                }
            }
        }

        /// <summary>
        /// Holiday policies are mutually exclusive — activating one turns the other off.
        /// </summary>
        public static void ApplyMutualExclusivity(ApplicationDbContext ctx, Decree changed)
        {
            if (!IsHolidayPolicy(changed.Name) || !changed.IsActive)
                return;

            var siblings = ctx.Decrees
                .Where(d => d.BaronyId == changed.BaronyId && d.Id != changed.Id)
                .ToList();

            foreach (var sibling in siblings)
            {
                if (!IsHolidayPolicy(sibling.Name) || !sibling.IsActive)
                    continue;
                sibling.IsActive = false;
            }
        }

        private static PpbVector Additive(decimal loyalty = 0m, decimal stability = 0m)
        {
            var v = new PpbVector();
            if (loyalty != 0m) v[Ppb.Loyalty] = loyalty;
            if (stability != 0m) v[Ppb.Stability] = stability;
            return v;
        }

        private static PpbVector Percent(decimal economy = 0m)
        {
            var v = new PpbVector();
            if (economy != 0m) v[Ppb.Economy] = economy;
            return v;
        }

        private static string Ser(PpbVector v)
        {
            var vector = v ?? new PpbVector();
            vector.EnsureSize();
            return JsonSerializer.Serialize(vector, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
