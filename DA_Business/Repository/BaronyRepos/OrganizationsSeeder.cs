using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Organizations contacts seeded for every barony.</summary>
    public static class OrganizationsSeeder
    {
        public sealed record SeedEntry(
            string GroupName,
            string Name,
            string Title,
            int? Age,
            string Description,
            int SortOrder);

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                GroupName: "The Inquisition",
                Name: "Gideon Sloane",
                Title: "Senior Inquisitor of the Eastern March",
                Age: null,
                Description:
                    "First Inquisitor of the March and senior over three regional inquisitors. Mild-mannered and soft-spoken in well-cut burgundy and black, with three old scars across his face—yet his office is feared: dagonite dust, heresy, forbidden cults, and investigations into the mighty all fall under the Inquisition’s eye. He answers ultimately to the Emperor; locally, a single word from him can freeze a city or a noble house.",
                SortOrder: 0),
            new(
                GroupName: "Order of Thunder",
                Name: "Jorenth von Egilburry",
                Title: "Komtur of the Eastern March",
                Age: null,
                Description:
                    "Highest commander of the Most Holy Order of Thyrus Thunderwielder in the Eastern March—chaplain-knights, hospitallers, and heavy cavalry under the cry *Let the Thunder Fall!* Grave and measured in duty, he answers for the Order’s honor in the region and can place even a Trial Knight on a path that binds faith, Empire, and secrets best kept behind cloister walls.",
                SortOrder: 1),
            new(
                GroupName: "Imperial Administration",
                Name: "Ernhold Tourraine",
                Title: "Emperor of the Thyrotan Empire",
                Age: null,
                Description:
                    "Reigning Tourraine and heir of Jergon’s line, seated in Dagareth amid the High Council of electors, officers of treasury, war, and spies, the Chancellor, Arcymagister, Grand Inquisitor, and the Archpriest of Thyrus. His word is final, yet he seldom overrides a united Council. From the capital he sets taxes, law, and the Empire’s recent appetite for conquest—most recently the reduction of Kildrad to a march shared among his generals.",
                SortOrder: 2),
            new(
                GroupName: "Mage Guild",
                Name: "Cassian Orthas",
                Title: "Arcymagister",
                Age: 163,
                Description:
                    "Head of Magna Gilda Arcana Magistanem—the Empire’s arcane authority, a campus-city of seven colleges plus the Eighth that hunts forbidden arts. Bound to the Emperor and free of the Inquisition’s yoke, the Guild controls who may wield high magic and how dagonite dust is allotted to its ranks. On the High Council the Arcymagister speaks for learning; in the marches, a guild seal can open doors—or close them forever.",
                SortOrder: 3),
        ];

        /// <summary>
        /// Adds any missing default organization relations for <paramref name="baronyId"/>.
        /// Idempotent: skips entries whose Name already exists in Organizations for that barony.
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existingNames = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Organizations)
                .Select(r => r.Name)
                .ToList();

            foreach (var entry in Defaults)
            {
                if (existingNames.Any(n => string.Equals(n, entry.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                ctx.BaronyRelations.Add(new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.Organizations,
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
