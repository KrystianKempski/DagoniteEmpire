using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Keeps Relations in sync with grantable terrain fiefs (!baron demesne, !domain default).
    /// Fiefs under the baron's primary domain → Vassals (Baronet + "direct vassal" +30).
    /// Fiefs under any other domain → Neighbors.
    /// </summary>
    public static class VassalsFromFiefsSeeder
    {
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var primaryDomainId = ctx.TerrainMapDomains
                .Where(d => d.BaronyId == baronyId && d.IsPrimary)
                .Select(d => (int?)d.Id)
                .FirstOrDefault();

            var allGrantableFiefs = ctx.Fiefs
                .Where(f => f.BaronyId == baronyId && !f.IsBaronDemesne && !f.IsDomainDefault)
                .OrderBy(f => f.LiegeName)
                .ThenBy(f => f.Name)
                .ToList();

            // Direct baron vassals: fiefs in primary domain (or unassigned when no primary domain yet).
            var vassalFiefs = allGrantableFiefs
                .Where(f => primaryDomainId == null
                         || f.SeniorDomainId == primaryDomainId
                         || f.SeniorDomainId == null)
                .ToList();

            // Fiefs under a foreign domain → show as Neighbors instead.
            var neighborFiefs = primaryDomainId.HasValue
                ? allGrantableFiefs
                    .Where(f => f.SeniorDomainId.HasValue && f.SeniorDomainId != primaryDomainId)
                    .ToList()
                : new List<Fief>();

            SyncVassals(ctx, baronyId, vassalFiefs);
            SyncNeighborsFromFiefs(ctx, baronyId, neighborFiefs);
        }

        private static void SyncVassals(ApplicationDbContext ctx, int baronyId, List<Fief> vassalFiefs)
        {
            var vassalRelations = ctx.BaronyRelations
                .Include(r => r.Modifiers)
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Vassals)
                .ToList();

            var linkedByFief = vassalRelations
                .Where(r => r.FiefId is int)
                .GroupBy(r => r.FiefId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var unlinked = vassalRelations.Where(r => r.FiefId is null).ToList();
            var fiefIds = vassalFiefs.Select(f => f.Id).ToHashSet();

            foreach (var orphan in linkedByFief.Where(kv => !fiefIds.Contains(kv.Key)).Select(kv => kv.Value).ToList())
            {
                ctx.BaronyRelations.Remove(orphan);
                linkedByFief.Remove(orphan.FiefId!.Value);
            }

            var nextSort = vassalRelations.Count == 0
                ? 0
                : vassalRelations.Max(r => r.SortOrder) + 1;

            foreach (var fief in vassalFiefs)
            {
                var personName = string.IsNullOrWhiteSpace(fief.LiegeName)
                    ? (string.IsNullOrWhiteSpace(fief.Name) ? "Unknown" : fief.Name.Trim())
                    : fief.LiegeName.Trim();
                var groupName = string.IsNullOrWhiteSpace(fief.Name) ? personName : fief.Name.Trim();

                if (!linkedByFief.TryGetValue(fief.Id, out var relation))
                {
                    relation = unlinked.FirstOrDefault(r =>
                        string.Equals(r.Name, personName, StringComparison.OrdinalIgnoreCase));
                    if (relation is not null)
                    {
                        unlinked.Remove(relation);
                        relation.FiefId = fief.Id;
                    }
                }

                if (relation is null)
                {
                    relation = new BaronyRelation
                    {
                        BaronyId = baronyId,
                        Category = RelationCategory.Vassals,
                        FiefId = fief.Id,
                        Name = personName,
                        Title = RelationVassalDefaults.BaronetTitle,
                        GroupName = groupName,
                        Description = string.Empty,
                        TroopCount = 0,
                        RelationDescription = string.Empty,
                        SortOrder = nextSort++,
                    };
                    ctx.BaronyRelations.Add(relation);
                    linkedByFief[fief.Id] = relation;
                }
                else
                {
                    relation.FiefId = fief.Id;
                    relation.Category = RelationCategory.Vassals;
                    relation.Name = personName;
                    relation.Title = RelationVassalDefaults.BaronetTitle;
                    if (string.IsNullOrWhiteSpace(relation.GroupName))
                        relation.GroupName = groupName;
                }

                EnsureDirectVassalModifier(relation);
            }
        }

        private static void SyncNeighborsFromFiefs(ApplicationDbContext ctx, int baronyId, List<Fief> neighborFiefs)
        {
            var neighborRelations = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Neighbors)
                .ToList();

            var linkedByFief = neighborRelations
                .Where(r => r.FiefId is int)
                .GroupBy(r => r.FiefId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var fiefIds = neighborFiefs.Select(f => f.Id).ToHashSet();

            // Remove stale Neighbor entries whose fief is gone or moved to primary domain.
            foreach (var orphan in linkedByFief.Where(kv => !fiefIds.Contains(kv.Key)).Select(kv => kv.Value).ToList())
            {
                ctx.BaronyRelations.Remove(orphan);
                linkedByFief.Remove(orphan.FiefId!.Value);
            }

            var nextSort = neighborRelations.Count == 0
                ? 0
                : neighborRelations.Max(r => r.SortOrder) + 1;

            foreach (var fief in neighborFiefs)
            {
                if (linkedByFief.ContainsKey(fief.Id))
                    continue; // already synced

                var personName = string.IsNullOrWhiteSpace(fief.LiegeName)
                    ? (string.IsNullOrWhiteSpace(fief.Name) ? "Unknown" : fief.Name.Trim())
                    : fief.LiegeName.Trim();
                var groupName = string.IsNullOrWhiteSpace(fief.Name) ? personName : fief.Name.Trim();

                // Don't create a duplicate if a Neighbor with the same name already exists (no FiefId link).
                if (neighborRelations.Any(r => string.Equals(r.Name, personName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var relation = new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.Neighbors,
                    FiefId = fief.Id,
                    Name = personName,
                    Title = string.Empty,
                    GroupName = groupName,
                    Description = string.Empty,
                    TroopCount = 0,
                    RelationDescription = string.Empty,
                    SortOrder = nextSort++,
                };
                ctx.BaronyRelations.Add(relation);
                linkedByFief[fief.Id] = relation;
            }
        }

        public static async Task EnsureForAllBaroniesAsync(ApplicationDbContext ctx)
        {
            var baronyIds = await ctx.Baronies.AsNoTracking().Select(b => b.Id).ToListAsync();
            foreach (var id in baronyIds)
                EnsureForBarony(ctx, id);
            await ctx.SaveChangesAsync();
        }

        private static void EnsureDirectVassalModifier(BaronyRelation relation)
        {
            relation.Modifiers ??= new List<BaronyRelationModifier>();
            var existing = relation.Modifiers.FirstOrDefault(m =>
                string.Equals(m.Description, RelationVassalDefaults.DirectVassalModifier, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Value = RelationVassalDefaults.DirectVassalAttitude;
                return;
            }

            relation.Modifiers.Add(new BaronyRelationModifier
            {
                Description = RelationVassalDefaults.DirectVassalModifier,
                Value = RelationVassalDefaults.DirectVassalAttitude,
                SortOrder = 0,
            });
        }
    }
}
