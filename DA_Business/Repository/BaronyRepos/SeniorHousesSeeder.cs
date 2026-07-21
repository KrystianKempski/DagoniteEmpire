using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Senior Houses contacts seeded for every barony.</summary>
    public static class SeniorHousesSeeder
    {
        public sealed record SeedEntry(
            string GroupName,
            string Name,
            string Title,
            int Age,
            string Description,
            int SortOrder);

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                GroupName: "House Greatwing",
                Name: "Hardwin Greatwing",
                Title: "Margrave of the Eastern March",
                Age: 39,
                Description:
                    "Heir of the Eastern Greatwings and son of Hyrion. A gifted general, warrior, and diplomat—charismatic, ambitious, and keenly aware of his house’s power. In twenty years he pacified and enriched his share of the March, ruling from Warington; his gaze already turns to neighboring lands, within the March and beyond.",
                SortOrder: 0),
            new(
                GroupName: "House Canterill",
                Name: "Argeweld Canterill",
                Title: "Marquis of Totham",
                Age: 53,
                Description:
                    "Head of the Canterills in the Eastern March and lord of wealthy Totham (nicknamed “Eight Chickens”). Like his kin he favors wit, trade, and gold over the sword—shrewd, cheerful in manner, and ruthless when crossed. He and his relatives steer the “Sheep Company,” one of the Empire’s great merchant houses.",
                SortOrder: 1),
            new(
                GroupName: "House Greyward",
                Name: "Myrton Greyward",
                Title: "Lord of Durnwald",
                Age: 49,
                Description:
                    "Third son of Myrweld who won glory in the Kildrad war by sealing the mountain pass and breaking Doratell’s army. Granted the high valley and its austere seat of Durnwald, he rules with Greyward honor and reserve—yet struggles to pacify wild borders of rebels, rival lords, and monsters, and seeks able banner-men to hold them.",
                SortOrder: 2),
        ];

        /// <summary>
        /// Adds any missing default senior-house relations for <paramref name="baronyId"/>.
        /// Idempotent: skips entries whose Name already exists in Senior Houses for that barony.
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existingNames = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.SeniorHouses)
                .Select(r => r.Name)
                .ToList();

            foreach (var entry in Defaults)
            {
                if (existingNames.Any(n => string.Equals(n, entry.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                ctx.BaronyRelations.Add(new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.SeniorHouses,
                    GroupName = entry.GroupName,
                    Name = entry.Name,
                    Title = entry.Title,
                    Age = entry.Age,
                    Description = entry.Description,
                    TroopCount = 0,
                    RelationDescription = string.Empty,
                    Notes = null,
                    SortOrder = entry.SortOrder,
                });
            }
        }

        public static async Task EnsureForAllBaroniesAsync(ApplicationDbContext ctx)
        {
            var baronyIds = await ctx.Baronies.AsNoTracking().Select(b => b.Id).ToListAsync();
            foreach (var id in baronyIds)
                EnsureForBarony(ctx, id);
            await ctx.SaveChangesAsync();
        }
    }
}
