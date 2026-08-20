using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Senior Houses contacts seeded for every barony (EN/PL at write time).</summary>
    public static class SeniorHousesSeeder
    {
        public const string AllyEmpireVassalModifierPl = "sojusznik, wasal Imperium";

        public sealed record SeedEntry(
            string GroupNameEn,
            string GroupNamePl,
            string Name,
            string TitleEn,
            string TitlePl,
            int Age,
            string DescriptionEn,
            string DescriptionPl,
            int SortOrder)
        {
            public string GroupName => BaronyCulture.IsPolish ? GroupNamePl : GroupNameEn;
            public string Title => BaronyCulture.IsPolish ? TitlePl : TitleEn;
            public string Description => BaronyCulture.IsPolish ? DescriptionPl : DescriptionEn;
        }

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                GroupNameEn: "House Greatwing",
                GroupNamePl: "Ród Greatwing",
                Name: "Hardwin Greatwing",
                TitleEn: "Margrave of the Eastern March",
                TitlePl: "Margrabia Marchii Wschodniej",
                Age: 39,
                DescriptionEn:
                    "Heir of the Eastern Greatwings and son of Hyrion. A gifted general, warrior, and diplomat—charismatic, ambitious, and keenly aware of his house’s power. In twenty years he pacified and enriched his share of the March, ruling from Warington; his gaze already turns to neighboring lands, within the March and beyond.",
                DescriptionPl:
                    "Dziedzic wschodnich Greatwingów i syn Hyriona. Utalentowany generał, wojownik i dyplomata — charyzmatyczny, ambitny i świadomy potęgi swego domu. W dwadzieścia lat spacyfikował i wzbogacił swoją część Marchii, rządząc z Warington; wzrok już kieruje ku sąsiednim ziemiom, w Marchii i poza nią.",
                SortOrder: 0),
            new(
                GroupNameEn: "House Canterill",
                GroupNamePl: "Ród Canterill",
                Name: "Argewald Canterill",
                TitleEn: "Marquis of Totham",
                TitlePl: "Markiz Totham",
                Age: 53,
                DescriptionEn:
                    "Head of the Canterills in the Eastern March and lord of wealthy Totham (nicknamed “Eight Chickens”). Like his kin he favors wit, trade, and gold over the sword—shrewd, cheerful in manner, and ruthless when crossed. He and his relatives steer the “Sheep Company,” one of the Empire’s great merchant houses.",
                DescriptionPl:
                    "Głowa Canterillów we Wschodniej Marchii i pan bogatego Totham (z przydomkiem „Osiem Kur”). Jak krewni ceni dowcip, handel i złoto ponad miecz — bystry, pogodny w manierach, bezlitosny gdy go obrazić. On i krewni sterują „Kompanią Owiec”, jednym z wielkich domów kupieckich Imperium.",
                SortOrder: 1),
            new(
                GroupNameEn: "House Greyward",
                GroupNamePl: "Ród Greyward",
                Name: "Myrton Greyward",
                TitleEn: "Lord of Durnwald",
                TitlePl: "Pan Durnwaldu",
                Age: 49,
                DescriptionEn:
                    "Third son of Myrweld who won glory in the Kildrad war by sealing the mountain pass and breaking Doratell’s army. Granted the high valley and its austere seat of Durnwald, he rules with Greyward honor and reserve—yet struggles to pacify wild borders of rebels, rival lords, and monsters, and seeks able banner-men to hold them.",
                DescriptionPl:
                    "Trzeci syn Myrwelda, który zdobył chwałę w wojnie kildradzkiej, zamykając górską przełęcz i łamiąc armię Doratella. Otrzymawszy wysoką dolinę i surowy tron Durnwaldu, rządzi z honorem Greywardów i powściągliwością — lecz zmaga się z ujarzmieniem dzikich granic zbuntowanych, rywali i potworów, i szuka zdolnych chorążych, by je utrzymać.",
                SortOrder: 2),
        ];

        private static string AllyModifierDescription =>
            BaronyCulture.IsPolish
                ? AllyEmpireVassalModifierPl
                : RelationSeniorDefaults.AllyEmpireVassalModifier;

        /// <summary>
        /// Adds any missing default senior-house relations for <paramref name="baronyId"/>
        /// and ensures every Senior Houses contact has +10 “ally, empire vassal”.
        /// When UI culture is Polish, also refreshes seeded titles/descriptions to PL.
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existing = ctx.BaronyRelations
                .Include(r => r.Modifiers)
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.SeniorHouses)
                .ToList();

            var byName = existing
                .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var entry in Defaults)
            {
                if (byName.TryGetValue(entry.Name, out var relation))
                {
                    ApplyLocalizedFields(relation, entry);
                }
                else
                {
                    relation = new BaronyRelation
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
                    };
                    ctx.BaronyRelations.Add(relation);
                    existing.Add(relation);
                    byName[entry.Name] = relation;
                }
            }

            foreach (var relation in existing)
                EnsureAllyEmpireVassalModifier(relation);
        }

        public static async Task EnsureForAllBaroniesAsync(ApplicationDbContext ctx)
        {
            var baronyIds = await ctx.Baronies.AsNoTracking().Select(b => b.Id).ToListAsync();
            foreach (var id in baronyIds)
                EnsureForBarony(ctx, id);
            await ctx.SaveChangesAsync();
        }

        private static void ApplyLocalizedFields(BaronyRelation relation, SeedEntry entry)
        {
            // Only rewrite catalog fields when Polish UI is active (backfill EN → PL).
            if (!BaronyCulture.IsPolish)
                return;

            relation.GroupName = entry.GroupNamePl;
            relation.Title = entry.TitlePl;
            relation.Description = entry.DescriptionPl;
            relation.SortOrder = entry.SortOrder;
        }

        private static void EnsureAllyEmpireVassalModifier(BaronyRelation relation)
        {
            relation.Modifiers ??= new List<BaronyRelationModifier>();
            var existing = relation.Modifiers.FirstOrDefault(m =>
                string.Equals(m.Description, RelationSeniorDefaults.AllyEmpireVassalModifier, StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.Description, AllyEmpireVassalModifierPl, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Description = AllyModifierDescription;
                existing.Value = RelationSeniorDefaults.AllyEmpireVassalAttitude;
                return;
            }

            relation.Modifiers.Add(new BaronyRelationModifier
            {
                Description = AllyModifierDescription,
                Value = RelationSeniorDefaults.AllyEmpireVassalAttitude,
                SortOrder = 0,
            });
        }
    }
}
