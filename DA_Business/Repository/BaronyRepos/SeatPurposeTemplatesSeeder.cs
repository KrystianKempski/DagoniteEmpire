using System.Text.Json;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Lord's Seat purpose templates (universal, all baronies).</summary>
    public static class SeatPurposeTemplatesSeeder
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public sealed record SeedEntry(
            string Name,
            string Description,
            string MinSizeCategory,
            string WhoOccupies,
            int SleepCapacity,
            decimal AdditivePrestige,
            decimal UpkeepGold,
            decimal Production,
            decimal Economy,
            decimal Defense,
            int SortOrder,
            decimal AdditiveHonor = 0m,
            decimal AdditiveFear = 0m);

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                Name: "Throne Room",
                Description:
                    "The baron's seat of judgment and ceremony. Grants +20 prestige, +2 Honor, and +0 Fear (honor/fear when Baron Card tracks them).",
                MinSizeCategory: SeatRoomSizeCategory.Large,
                WhoOccupies: "The baron and the court at audience",
                SleepCapacity: 0,
                AdditivePrestige: 20,
                UpkeepGold: 4,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 0),
            new(
                Name: "Council Hall",
                Description: "Chamber for council meetings, petitions, and baronial administration. +10 prestige.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Baron, advisors, and petitioners",
                SleepCapacity: 0,
                AdditivePrestige: 10,
                UpkeepGold: 3,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 1),
            new(
                Name: "Ballroom",
                Description: "Grand hall for feasts, dances, and diplomatic receptions. +30 prestige.",
                MinSizeCategory: SeatRoomSizeCategory.Large,
                WhoOccupies: "Guests and household at court events",
                SleepCapacity: 0,
                AdditivePrestige: 30,
                UpkeepGold: 4,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 2),
            new(
                Name: "Baron Chambers",
                Description: "Private sleeping apartments of the baron and close family.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "The baron",
                SleepCapacity: 2,
                AdditivePrestige: 0,
                UpkeepGold: 4,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 3),
            new(
                Name: "Baron Study",
                Description: "Private office for correspondence, maps, and baronial business.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "The baron",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 3,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 4),
            new(
                Name: "Advisor Chambers",
                Description: "Lodgings for a resident court advisor.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "A court advisor",
                SleepCapacity: 1,
                AdditivePrestige: 0,
                UpkeepGold: 2,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 5),
            new(
                Name: "Guest Chambers",
                Description: "Comfortable rooms for visiting nobles and envoys.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "Honored guests",
                SleepCapacity: 2,
                AdditivePrestige: 0,
                UpkeepGold: 1,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 6),
            new(
                Name: "Kitchen",
                Description: "Hearths, ovens, and sculleries feeding the household.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Cooks and kitchen staff",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 5,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 7),
            new(
                Name: "Pantry",
                Description: "Cool storage for provisions close to the kitchens.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "Stewards and kitchen servants",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 1,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 8),
            new(
                Name: "Supply Warehouse",
                Description: "Dry storage for grain, salt meat, ale, and other household stores.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "Stewards and porters",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 1,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 9),
            new(
                Name: "Treasury",
                Description: "Vaults and counting rooms for the baron's coin and plate.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Treasurer and trusted guards",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 2,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 10),
            new(
                Name: "Guard Barracks",
                Description:
                    "Barracks for the lord's guard. Upkeep covers a garrison of 10; effective capacity scales with chamber size.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Household guard (10 soldiers)",
                SleepCapacity: 10,
                AdditivePrestige: 0,
                UpkeepGold: 5,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 11),
            new(
                Name: "Armory",
                Description: "Racks and chests for arms, armor, and militia equipment.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Armorer and guards",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 3,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 12),
            new(
                Name: "Smithy",
                Description: "Forge for repairs, horseshoes, and household metalwork.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Smith and apprentices",
                SleepCapacity: 0,
                AdditivePrestige: 0,
                UpkeepGold: 4,
                Production: 1,
                Economy: 1,
                Defense: 0,
                SortOrder: 13),
            new(
                Name: "Stables",
                Description: "Stalls and tack rooms for warhorses and riding stock. +10 prestige.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Grooms and stablehands",
                SleepCapacity: 0,
                AdditivePrestige: 10,
                UpkeepGold: 3,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 14),
            new(
                Name: "Servants Quarters",
                Description: "Shared lodgings for domestic staff and day laborers.",
                MinSizeCategory: SeatRoomSizeCategory.Small,
                WhoOccupies: "Household servants",
                SleepCapacity: 6,
                AdditivePrestige: 0,
                UpkeepGold: 2,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 15),
            new(
                Name: "Watch Tower",
                Description: "Observation post and strongpoint overlooking approaches to the seat.",
                MinSizeCategory: SeatRoomSizeCategory.Medium,
                WhoOccupies: "Sentries and archers",
                SleepCapacity: 4,
                AdditivePrestige: 0,
                UpkeepGold: 2,
                Production: 0,
                Economy: 0,
                Defense: 0,
                SortOrder: 16),
        ];

        public static IEnumerable<string> LocalizationKeys()
        {
            foreach (var entry in Defaults)
            {
                yield return entry.Name;
                if (!string.IsNullOrWhiteSpace(entry.Description))
                    yield return entry.Description;
                if (!string.IsNullOrWhiteSpace(entry.WhoOccupies))
                    yield return entry.WhoOccupies;
            }
        }

        public static void EnsureDefaults(ApplicationDbContext ctx)
        {
            var existing = ctx.SeatPurposeTemplates
                .Where(t => t.IsUniversal)
                .Select(t => t.Name)
                .ToList();

            foreach (var entry in Defaults)
            {
                if (existing.Any(n => string.Equals(n, entry.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                ctx.SeatPurposeTemplates.Add(ToEntity(entry));
            }
        }

        public static async Task EnsureDefaultsAsync(ApplicationDbContext ctx)
        {
            EnsureDefaults(ctx);
            await ctx.SaveChangesAsync();
        }

        private static SeatPurposeTemplate ToEntity(SeedEntry entry)
        {
            var additive = new PpbVector();
            if (entry.UpkeepGold > 0)
                additive[Ppb.Treasury] = -entry.UpkeepGold;
            if (entry.Production != 0)
                additive[Ppb.Production] = entry.Production;
            if (entry.Economy != 0)
                additive[Ppb.Economy] = entry.Economy;
            if (entry.Defense != 0)
                additive[Ppb.Defense] = entry.Defense;

            return new SeatPurposeTemplate
            {
                Name = entry.Name,
                Description = entry.Description,
                MinSizeCategory = entry.MinSizeCategory,
                WhoOccupies = entry.WhoOccupies,
                SleepCapacity = entry.SleepCapacity,
                AdditivePrestige = entry.AdditivePrestige,
                AdditiveHonor = entry.AdditiveHonor,
                AdditiveFear = entry.AdditiveFear,
                AdditiveJson = JsonSerializer.Serialize(additive, JsonOptions),
                PercentJson = JsonSerializer.Serialize(new PpbVector(), JsonOptions),
                IsUniversal = true,
                BaronyId = null,
                SortOrder = entry.SortOrder,
            };
        }
    }
}
