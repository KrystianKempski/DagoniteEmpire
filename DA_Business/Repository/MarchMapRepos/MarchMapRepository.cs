using System.Text.Json;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.MarchMapRepos
{
    public interface IMarchMapRepository
    {
        Task<MarchMapDocument> GetDocumentAsync();
        Task SaveDocumentAsync(MarchMapDocument document);
        Task ResetToSeedAsync();
    }

    public sealed class MarchMapRepository : IMarchMapRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public MarchMapRepository(IDbContextFactory<ApplicationDbContext> db) => _db = db;

        public async Task<MarchMapDocument> GetDocumentAsync()
        {
            await using var ctx = await _db.CreateDbContextAsync();
            var row = await ctx.MarchMapStates.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == MarchMapState.GlobalId);

            MarchMapDocument doc;
            if (row is null || string.IsNullOrWhiteSpace(row.PayloadJson))
            {
                doc = EasternMarchMapDefaults.CreateSeedDocument();
            }
            else
            {
                try
                {
                    doc = JsonSerializer.Deserialize<MarchMapDocument>(row.PayloadJson, JsonOptions)
                          ?? EasternMarchMapDefaults.CreateSeedDocument();
                }
                catch
                {
                    doc = EasternMarchMapDefaults.CreateSeedDocument();
                }
            }

            var before = JsonSerializer.Serialize(doc, JsonOptions);
            doc = Normalize(doc);
            var after = JsonSerializer.Serialize(doc, JsonOptions);
            if (!string.Equals(before, after, StringComparison.Ordinal))
                await SaveDocumentAsync(doc);

            return doc;
        }

        public async Task SaveDocumentAsync(MarchMapDocument document)
        {
            var normalized = Normalize(document);
            await using var ctx = await _db.CreateDbContextAsync();
            var row = await ctx.MarchMapStates.FirstOrDefaultAsync(x => x.Id == MarchMapState.GlobalId);
            if (row is null)
            {
                row = new MarchMapState { Id = MarchMapState.GlobalId };
                ctx.MarchMapStates.Add(row);
            }

            row.PayloadJson = JsonSerializer.Serialize(normalized, JsonOptions);
            await ctx.SaveChangesAsync();
        }

        public async Task ResetToSeedAsync()
        {
            await SaveDocumentAsync(EasternMarchMapDefaults.CreateSeedDocument());
        }

        private static MarchMapDocument Normalize(MarchMapDocument doc)
        {
            doc.ImageUrl = string.IsNullOrWhiteSpace(doc.ImageUrl)
                ? MarchMapDocument.DefaultImageUrl
                : doc.ImageUrl.Trim();

            doc.Nodes ??= new List<MarchMapNode>();
            doc.Routes ??= new List<MarchMapRoute>();

            foreach (var node in doc.Nodes)
            {
                if (string.IsNullOrWhiteSpace(node.Id))
                    node.Id = Guid.NewGuid().ToString("N");
                node.Label = node.Label?.Trim() ?? string.Empty;
                node.Kind = MarchMapNodeKind.Normalize(node.Kind);
                if (node.DefaultCustomsGoldPerTurn is < 0m)
                    node.DefaultCustomsGoldPerTurn = 0m;
                node.X = Math.Clamp(node.X, 0, 1000);
                node.Y = Math.Clamp(node.Y, 0, 1000);
            }

            foreach (var route in doc.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.Id))
                    route.Id = Guid.NewGuid().ToString("N");
                route.Kind = MarchRouteKind.Normalize(route.Kind);
            }

            var nodeIds = doc.Nodes.Select(n => n.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            doc.Routes = doc.Routes
                .Where(r => nodeIds.Contains(r.FromNodeId) && nodeIds.Contains(r.ToNodeId))
                .Where(r => !string.Equals(r.FromNodeId, r.ToNodeId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            KnownLordsCatalog.ApplyKnownLordLinks(doc.Nodes);

            // Drop orphan places (no lord) and their links — trade map requires a lord on every node.
            var orphanIds = doc.Nodes
                .Where(n => string.IsNullOrWhiteSpace(n.LordKey))
                .Select(n => n.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (orphanIds.Count > 0)
            {
                doc.Nodes.RemoveAll(n => orphanIds.Contains(n.Id));
                doc.Routes.RemoveAll(r =>
                    orphanIds.Contains(r.FromNodeId) || orphanIds.Contains(r.ToNodeId));
            }

            // Re-add known seats that were dropped before their catalog entry existed (e.g. Dawntree).
            EnsureCatalogSeat(doc, holdingsLabel: "Dawntree", nearLabel: "Forestedge");

            return doc;
        }

        private static void EnsureCatalogSeat(MarchMapDocument doc, string holdingsLabel, string? nearLabel)
        {
            var lord = KnownLordsCatalog.FindByPlaceLabel(holdingsLabel);
            if (lord is null)
                return;

            var lordKey = KnownLordsCatalog.LordKey(lord);
            var existing = doc.Nodes.FirstOrDefault(n =>
                string.Equals(n.LordKey, lordKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.Label, holdingsLabel, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.LordKey = lordKey;
                if (string.IsNullOrWhiteSpace(existing.Label))
                    existing.Label = holdingsLabel;
                return;
            }

            double x = 420;
            double y = 380;
            if (!string.IsNullOrWhiteSpace(nearLabel))
            {
                var near = doc.Nodes.FirstOrDefault(n =>
                    string.Equals(n.Label, nearLabel, StringComparison.OrdinalIgnoreCase));
                if (near is not null)
                {
                    x = Math.Clamp(near.X - 28, 20, 980);
                    y = Math.Clamp(near.Y - 18, 20, 980);
                }
            }

            var node = new MarchMapNode
            {
                Id = Guid.NewGuid().ToString("N"),
                Label = holdingsLabel,
                Kind = lord.Wealth >= 6 ? MarchMapNodeKind.LargeCity : MarchMapNodeKind.City,
                LordKey = lordKey,
                X = x,
                Y = y,
            };
            doc.Nodes.Add(node);

            if (!string.IsNullOrWhiteSpace(nearLabel))
            {
                var near = doc.Nodes.FirstOrDefault(n =>
                    string.Equals(n.Label, nearLabel, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(n.Id, node.Id, StringComparison.OrdinalIgnoreCase));
                if (near is not null &&
                    !doc.Routes.Any(r =>
                        (string.Equals(r.FromNodeId, near.Id, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(r.ToNodeId, node.Id, StringComparison.OrdinalIgnoreCase)) ||
                        (string.Equals(r.FromNodeId, node.Id, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(r.ToNodeId, near.Id, StringComparison.OrdinalIgnoreCase))))
                {
                    doc.Routes.Add(new MarchMapRoute
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        FromNodeId = near.Id,
                        ToNodeId = node.Id,
                        Kind = MarchRouteKind.Road,
                    });
                }
            }
        }
    }
}
