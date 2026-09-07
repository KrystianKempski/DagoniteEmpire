using DA_Common;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Services
{
    /// <summary>
    /// Translates game-world ids into ASP.NET Identity user ids, which is what push subscriptions
    /// are keyed by. Characters reference their owner by user *name*, not id, so most lookups end
    /// with a hop through Identity. Hidden demo accounts are never returned — they are shared
    /// throwaway logins, so a push would reach whoever happens to be trying the demo.
    /// </summary>
    public class NotificationRecipientLookup
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public NotificationRecipientLookup(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        /// <summary>Identity ids for the given character owner names, demo accounts skipped.</summary>
        public async Task<List<string>> UserIdsForUserNames(
            IEnumerable<string?> userNames,
            CancellationToken cancellationToken = default)
        {
            // Identity stores names upper-cased in NormalizedUserName; matching that column keeps
            // the lookup case-insensitive without a client-side scan.
            var normalized = (userNames ?? Enumerable.Empty<string?>())
                .Where(n => !string.IsNullOrWhiteSpace(n) && !SD.IsDemoUserName(n))
                .Select(n => n!.Trim().ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (normalized.Count == 0)
                return new List<string>();

            using var ctx = await _db.CreateDbContextAsync(cancellationToken);
            return await ctx.Users
                .AsNoTracking()
                .Where(u => u.NormalizedUserName != null && normalized.Contains(u.NormalizedUserName))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        /// <summary>Identity id of the player who owns this barony, via its baron character.</summary>
        public async Task<string?> UserIdForBarony(
            int baronyId,
            CancellationToken cancellationToken = default)
        {
            if (baronyId <= 0)
                return null;

            string? ownerName;
            using (var ctx = await _db.CreateDbContextAsync(cancellationToken))
            {
                ownerName = await (
                    from b in ctx.Baronies.AsNoTracking()
                    join c in ctx.Characters.AsNoTracking() on b.CharacterId equals c.Id
                    where b.Id == baronyId
                    select c.UserName
                ).FirstOrDefaultAsync(cancellationToken);
            }

            var ids = await UserIdsForUserNames(new[] { ownerName }, cancellationToken);
            return ids.FirstOrDefault();
        }

        /// <summary>Identity ids of the real Game Masters, excluding the public demo GM.</summary>
        public async Task<List<string>> GameMasterUserIds(CancellationToken cancellationToken = default)
        {
            using var ctx = await _db.CreateDbContextAsync(cancellationToken);
            var candidates = await (
                from ur in ctx.UserRoles
                join r in ctx.Roles on ur.RoleId equals r.Id
                join u in ctx.Users on ur.UserId equals u.Id
                where r.Name == SD.Role_GameMaster
                select new { u.Id, u.UserName }
            ).ToListAsync(cancellationToken);

            return candidates
                .Where(u => !SD.IsDemoUserName(u.UserName))
                .Select(u => u.Id)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
