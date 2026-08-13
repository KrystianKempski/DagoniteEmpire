using DA_Business.Repository.BaronyRepos;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Services
{
    /// <summary>
    /// Provisions isolated "Try baron" demo sessions. Each session gets a throwaway clone of the
    /// seeded baron and a freshly seeded Darkhold barony; everything is purged on exit / TTL so the
    /// public demo can never mutate shared/global records.
    /// </summary>
    public class DemoBaronyService : IDemoBaronyService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IBaronyRepository _baronyRepo;

        public DemoBaronyService(IDbContextFactory<ApplicationDbContext> db, IBaronyRepository baronyRepo)
        {
            _db = db;
            _baronyRepo = baronyRepo;
        }

        public async Task<DemoSessionInfo> CreateSessionAsync()
        {
            int characterId;
            await using (var ctx = await _db.CreateDbContextAsync())
            {
                var source = await ctx.Characters
                    .AsNoTracking()
                    .Include(c => c.Attributes)
                    .Include(c => c.BaseSkills)
                    .Include(c => c.SpecialSkills)
                    .Include(c => c.EquipmentSlots)
                    .Include(c => c.Languages)
                    .FirstOrDefaultAsync(c => c.NPCName == SD.DemoBaronSourceCharacterName)
                    ?? throw new InvalidOperationException(
                        $"Demo baron source character '{SD.DemoBaronSourceCharacterName}' not found.");

                var clone = new Character
                {
                    UserName = SD.DemoBaronUserName,
                    Relation = source.Relation,
                    NPCName = source.NPCName,
                    Description = source.Description,
                    Age = source.Age,
                    ImageUrl = source.ImageUrl,
                    IconUrl = source.IconUrl,
                    NPCType = SD.NPCType.Duke,
                    AttributePoints = source.AttributePoints,
                    CurrentExpPoints = source.CurrentExpPoints,
                    UsedExpPoints = source.UsedExpPoints,
                    TraitBalance = source.TraitBalance,
                    RaceId = source.RaceId,
                    IsApproved = true,
                    ProfessionId = source.ProfessionId,
                    WeaponSet = source.WeaponSet,
                    DateNumber = source.DateNumber,
                    Attributes = source.Attributes?
                        .OrderBy(a => a.Index)
                        .Select(a => new Attribute
                        {
                            Name = a.Name,
                            FeatureType = a.FeatureType,
                            Index = a.Index,
                            BaseBonus = a.BaseBonus,
                            RaceBonus = a.RaceBonus,
                            GearBonus = a.GearBonus,
                            TraitBonus = a.TraitBonus,
                            OtherBonuses = a.OtherBonuses,
                            TempBonuses = a.TempBonuses,
                            HealthBonus = a.HealthBonus,
                        })
                        .ToList(),
                    BaseSkills = source.BaseSkills?
                        .OrderBy(s => s.Index)
                        .Select(s => new BaseSkill
                        {
                            Name = s.Name,
                            FeatureType = s.FeatureType,
                            Index = s.Index,
                            BaseBonus = s.BaseBonus,
                            RaceBonus = s.RaceBonus,
                            GearBonus = s.GearBonus,
                            TraitBonus = s.TraitBonus,
                            OtherBonuses = s.OtherBonuses,
                            TempBonuses = s.TempBonuses,
                            HealthBonus = s.HealthBonus,
                            RelatedAttribute1 = s.RelatedAttribute1,
                            RelatedAttribute2 = s.RelatedAttribute2,
                        })
                        .ToList(),
                    SpecialSkills = source.SpecialSkills?
                        .OrderBy(s => s.RelatedBaseSkillName)
                        .ThenBy(s => s.Index)
                        .ThenBy(s => s.Name)
                        .Select(s => new SpecialSkill
                        {
                            Name = s.Name,
                            FeatureType = s.FeatureType,
                            Index = s.Index,
                            BaseBonus = s.BaseBonus,
                            RaceBonus = s.RaceBonus,
                            GearBonus = s.GearBonus,
                            TraitBonus = s.TraitBonus,
                            OtherBonuses = s.OtherBonuses,
                            TempBonuses = s.TempBonuses,
                            HealthBonus = s.HealthBonus,
                            RelatedAttribute1 = s.RelatedAttribute1,
                            RelatedAttribute2 = s.RelatedAttribute2,
                            RelatedBaseSkillName = s.RelatedBaseSkillName,
                            ChosenAttribute = s.ChosenAttribute,
                            Editable = s.Editable,
                        })
                        .ToList(),
                    EquipmentSlots = source.EquipmentSlots?
                        .OrderBy(s => s.Id)
                        .Select(s => new EquipmentSlot
                        {
                            Count = s.Count,
                            EquipmentID = s.EquipmentID,
                            IsEquipped = s.IsEquipped,
                            SlotType = s.SlotType,
                        })
                        .ToList(),
                };

                if (source.Languages is { Count: > 0 })
                {
                    var languageIds = source.Languages.Select(l => l.Id).ToList();
                    clone.Languages = await ctx.Languages
                        .Where(l => languageIds.Contains(l.Id))
                        .ToListAsync();
                }

                ctx.Characters.Add(clone);
                await ctx.SaveChangesAsync();
                characterId = clone.Id;
            }

            // Reuse the standard barony creation path so the demo gets the exact same seeded Darkhold state.
            var barony = await _baronyRepo.CreateForCharacter(characterId, DarkholdSeeder.BaronyName, "Demo barony", "darkhold");

            var token = Guid.NewGuid();
            await using (var ctx = await _db.CreateDbContextAsync())
            {
                var now = DateTime.UtcNow;
                ctx.DemoSessions.Add(new DemoSession
                {
                    Token = token,
                    CharacterId = characterId,
                    BaronyId = barony.Id,
                    CreatedUtc = now,
                    LastSeenUtc = now,
                });
                await ctx.SaveChangesAsync();
            }

            return new DemoSessionInfo(token, characterId, barony.Id);
        }

        public async Task TouchAsync(Guid token)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            await ctx.DemoSessions
                .Where(d => d.Token == token)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.LastSeenUtc, DateTime.UtcNow));
        }

        public async Task MarkLeavingAsync(Guid token)
        {
            // Push last-seen just past the TTL (minus a short grace) so a real "leave" is swept
            // within a sweeper cycle, while a page refresh re-touches the session before expiry.
            var staleAt = DateTime.UtcNow - SD.DemoSessionTtl + TimeSpan.FromSeconds(15);
            await using var ctx = await _db.CreateDbContextAsync();
            await ctx.DemoSessions
                .Where(d => d.Token == token)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.LastSeenUtc, staleAt));
        }

        public async Task<bool> IsDemoCharacterAsync(int characterId)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            return await ctx.DemoSessions.AnyAsync(d => d.CharacterId == characterId);
        }

        public async Task<bool> IsSessionActiveAsync(Guid token)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            return await ctx.DemoSessions.AnyAsync(d => d.Token == token);
        }

        public async Task EndSessionAsync(Guid token)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            var session = await ctx.DemoSessions.FirstOrDefaultAsync(d => d.Token == token);
            if (session is null)
                return;
            await PurgeSessionAsync(ctx, session);
        }

        public async Task<int> SweepExpiredAsync(TimeSpan ttl)
        {
            var cutoff = DateTime.UtcNow - ttl;
            await using var ctx = await _db.CreateDbContextAsync();
            var expired = await ctx.DemoSessions.Where(d => d.LastSeenUtc < cutoff).ToListAsync();
            foreach (var session in expired)
            {
                await PurgeSessionAsync(ctx, session);
            }
            return expired.Count;
        }

        /// <summary>Deletes the demo barony subgraph, the baron character and the tracking row.</summary>
        private static async Task PurgeSessionAsync(ApplicationDbContext ctx, DemoSession session)
        {
            await DeleteBaronySubgraphAsync(ctx, session.BaronyId);
            await DeleteCharacterAsync(ctx, session.CharacterId);
            ctx.DemoSessions.Remove(session);
            await ctx.SaveChangesAsync();
        }

        /// <summary>Removes every per-barony row (children before parents) for the given barony, then the barony itself.</summary>
        private static async Task DeleteBaronySubgraphAsync(ApplicationDbContext ctx, int baronyId)
        {
            // Lord's Seat
            await ctx.SeatRoomTraits
                .Where(t => ctx.SeatRooms.Any(r => r.Id == t.RoomId
                    && ctx.BaronySeats.Any(s => s.Id == r.SeatId && s.BaronyId == baronyId)))
                .ExecuteDeleteAsync();
            await ctx.BaronArtifacts.Where(a => a.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.SeatRooms
                .Where(r => ctx.BaronySeats.Any(s => s.Id == r.SeatId && s.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.SeatTiles
                .Where(t => ctx.BaronySeats.Any(s => s.Id == t.SeatId && s.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.BaronySeats.Where(s => s.BaronyId == baronyId).ExecuteDeleteAsync();

            // Correspondence
            await ctx.BaronLetterMessages
                .Where(m => ctx.BaronLetterThreads.Any(th => th.Id == m.ThreadId && th.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.BaronLetterThreads.Where(th => th.BaronyId == baronyId).ExecuteDeleteAsync();

            // Audiences
            await ctx.BaronAudienceExchanges
                .Where(e => ctx.BaronAudiences.Any(a => a.Id == e.AudienceId && a.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.BaronAudiences.Where(a => a.BaronyId == baronyId).ExecuteDeleteAsync();

            // Relations
            await ctx.BaronyRelationModifiers
                .Where(m => ctx.BaronyRelations.Any(r => r.Id == m.RelationId && r.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.BaronyRelations.Where(r => r.BaronyId == baronyId).ExecuteDeleteAsync();

            // Advisors / offices
            await ctx.AdvisorInfluenceModifiers
                .Where(m => ctx.Advisors.Any(a => a.Id == m.AdvisorId && a.BaronyId == baronyId))
                .ExecuteDeleteAsync();
            await ctx.Advisors.Where(a => a.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.AvailableAdvisors.Where(a => a.BaronyId == baronyId).ExecuteDeleteAsync();

            // Terrain
            await ctx.TerrainImprovements.Where(i => i.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.TerrainTiles.Where(t => t.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.Fiefs.Where(f => f.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.TerrainMapDomains.Where(d => d.BaronyId == baronyId).ExecuteDeleteAsync();

            // Flat per-barony rows
            await ctx.BaronPhpSources.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronPurseSources.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyResourceSources.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronTimeActions.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronTimeModifiers.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronInfluenceModifiers.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.CommunityModifiers.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyEvents.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.Decrees.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.SocialGroupRelations.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyBuildings.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyPlayerNotes.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyProjects.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyUnits.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.BaronyBattleMaps.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();
            await ctx.SeatPurposeTemplates.Where(x => x.BaronyId == baronyId).ExecuteDeleteAsync();

            await ctx.Baronies.Where(b => b.Id == baronyId).ExecuteDeleteAsync();
        }

        /// <summary>Removes the demo baron character, its features and its dedicated campaign.</summary>
        private static async Task DeleteCharacterAsync(ApplicationDbContext ctx, int characterId)
        {
            var character = await ctx.Characters
                .Include(c => c.Campaigns)
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.Id == characterId);
            if (character is null)
                return;

            var campaignIds = character.Campaigns?.Select(c => c.Id).ToList() ?? new List<int>();
            character.Campaigns?.Clear();
            character.Languages?.Clear();
            await ctx.SaveChangesAsync();

            if (campaignIds.Count > 0)
            {
                await ctx.Campaigns.Where(c => campaignIds.Contains(c.Id)).ExecuteDeleteAsync();
            }

            await ctx.Attributes.Where(a => a.CharacterId == characterId).ExecuteDeleteAsync();
            await ctx.BaseSkills.Where(s => s.CharacterId == characterId).ExecuteDeleteAsync();
            await ctx.SpecialSkills.Where(s => s.CharacterId == characterId).ExecuteDeleteAsync();
            await ctx.EquipmentSlots.Where(s => s.CharacterID == characterId).ExecuteDeleteAsync();
            await ctx.Wounds.Where(w => w.CharacterId == characterId).ExecuteDeleteAsync();

            await ctx.Characters.Where(c => c.Id == characterId).ExecuteDeleteAsync();
        }
    }
}
