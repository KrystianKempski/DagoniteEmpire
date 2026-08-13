using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Seeds vassal family members (Relations → Vassals) for the baron's direct noble houses.
    /// Idempotent: skips entries whose Name already exists in Vassals for that barony.
    /// </summary>
    public static class VassalFamilySeeder
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
            // --- House Bullewyn ---
            new(
                GroupName: "House Bullewyn",
                Name: "Jochim Bullewyn",
                Title: "Baronet",
                Age: 53,
                Description: "Head of House Bullewyn. Direct vassal of the baron.",
                SortOrder: 100),
            new(
                GroupName: "House Bullewyn",
                Name: "Jora Bullewyn",
                Title: "Heir",
                Age: 21,
                Description: "Heir and son of Jochim Bullewyn.",
                SortOrder: 101),
            new(
                GroupName: "House Bullewyn",
                Name: "Umbra Bullewyn",
                Title: "Lady",
                Age: 45,
                Description: "Mother of the house. Wife of Jochim.",
                SortOrder: 102),
            new(
                GroupName: "House Bullewyn",
                Name: "Callor Bullewyn",
                Title: "Son",
                Age: 18,
                Description: "Son of Jochim and Umbra.",
                SortOrder: 103),
            new(
                GroupName: "House Bullewyn",
                Name: "Ranel Bullewyn",
                Title: "Daughter",
                Age: 18,
                Description: "Daughter of Jochim and Umbra.",
                SortOrder: 104),
            new(
                GroupName: "House Bullewyn",
                Name: "Mereya Bullewyn",
                Title: "Daughter",
                Age: 14,
                Description: "Youngest daughter of Jochim and Umbra.",
                SortOrder: 105),
            new(
                GroupName: "House Bullewyn",
                Name: "Terren Wynch",
                Title: "Family friend",
                Age: 45,
                Description: "Friend of the Bullewyn family.",
                SortOrder: 106),

            // --- House Canterill Brązowi ---
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Millena Canterill",
                Title: "Baronet",
                Age: 56,
                Description: "Head of House Canterill Brązowi. Direct vassal of the baron.",
                SortOrder: 110),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Ellya Canterill",
                Title: "Heiress",
                Age: 25,
                Description: "Eldest daughter and heiress of Millena Canterill.",
                SortOrder: 111),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Laurane Canterill",
                Title: "Daughter",
                Age: 23,
                Description: "Daughter of Millena Canterill.",
                SortOrder: 112),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Dyanna Canterill",
                Title: "Daughter",
                Age: 18,
                Description: "Daughter of Millena Canterill.",
                SortOrder: 113),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Nysah Canterill",
                Title: "Daughter",
                Age: 16,
                Description: "Youngest daughter of Millena Canterill.",
                SortOrder: 114),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Dorran Carner",
                Title: "Suitor",
                Age: 26,
                Description: "Suitor seeking the hand of one of the Canterill daughters.",
                SortOrder: 115),
            new(
                GroupName: "House Canterill Brązowi",
                Name: "Lanard Apperford",
                Title: "Suitor",
                Age: 28,
                Description: "Suitor seeking the hand of one of the Canterill daughters.",
                SortOrder: 116),
        ];

        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existingNames = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Vassals)
                .Select(r => r.Name)
                .ToList();

            var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

            foreach (var entry in Defaults)
            {
                if (existingSet.Contains(entry.Name))
                    continue;

                ctx.BaronyRelations.Add(new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.Vassals,
                    GroupName = entry.GroupName,
                    Name = entry.Name,
                    Title = entry.Title,
                    Age = entry.Age,
                    Description = entry.Description,
                    TroopCount = 0,
                    RelationDescription = string.Empty,
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
