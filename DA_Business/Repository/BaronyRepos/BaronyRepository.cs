using System.Text.Json;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Repozytorium warstwy baronii. Wzorem BattleMapRepository operuje na własnych kontekstach
    /// z IDbContextFactory i ręcznie mapuje encje ↔ DTO (wektory PPB trzymane jako JSON).
    /// </summary>
    public class BaronyRepository : IBaronyRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BaronyRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        // ---------------- JSON helpers ----------------
        private static string Ser(PpbVector? v)
        {
            (v ?? new PpbVector()).EnsureSize();
            return JsonSerializer.Serialize(v ?? new PpbVector(), JsonOptions);
        }

        private static PpbVector De(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PpbVector();
            try
            {
                var v = JsonSerializer.Deserialize<PpbVector>(json, JsonOptions) ?? new PpbVector();
                v.EnsureSize();
                return v;
            }
            catch
            {
                return new PpbVector();
            }
        }

        private static RepositoryErrorException Err(System.Exception ex, string method) =>
            new RepositoryErrorException("Error in " + method + ": " + ex.Message, ex);

        // ---------------- Barony ----------------
        public async Task<BaronyDTO?> GetByCharacterId(int characterId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.CharacterId == characterId);
                return e is null ? null : ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetByCharacterId)); }
        }

        public async Task<BaronyDTO?> GetById(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                return e is null ? null : ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetById)); }
        }

        public async Task<BaronyDTO> CreateForCharacter(int characterId, string name)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var existing = await ctx.Baronies.FirstOrDefaultAsync(b => b.CharacterId == characterId);
                if (existing is not null)
                    return ToDTO(existing);

                var entity = new Barony
                {
                    CharacterId = characterId,
                    Name = string.IsNullOrWhiteSpace(name) ? "Nowa Baronia" : name,
                    BaseParametersJson = Ser(new PpbVector()),
                };
                var added = await ctx.Baronies.AddAsync(entity);
                await ctx.SaveChangesAsync();
                return ToDTO(added.Entity);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(CreateForCharacter)); }
        }

        public async Task<BaronyDTO> UpdateBarony(BaronyDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == dto.Id);
                if (e is null)
                {
                    var created = ToEntity(dto);
                    var added = ctx.Baronies.Add(created);
                    await ctx.SaveChangesAsync();
                    return ToDTO(added.Entity);
                }
                ApplyBarony(e, dto);
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(UpdateBarony)); }
        }

        public async Task<BaronyOverviewDTO?> GetOverview(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId);
                if (barony is null)
                    return null;

                return new BaronyOverviewDTO
                {
                    Barony = ToDTO(barony),
                    Advisors = (await ctx.Advisors.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Buildings = (await ctx.BaronyBuildings.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    SocialRelations = (await ctx.SocialGroupRelations.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Improvements = (await ctx.TerrainImprovements.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Decrees = (await ctx.Decrees.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Events = (await ctx.BaronyEvents.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    CommunityModifiers = (await ctx.CommunityModifiers.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Fiefs = (await ctx.Fiefs.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Tiles = (await ctx.TerrainTiles.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Projects = (await ctx.BaronyProjects.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                };
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetOverview)); }
        }

        // ---------------- Advisors ----------------
        public async Task<List<AdvisorDTO>> GetAdvisors(int baronyId) =>
            await GetList(ctx => ctx.Advisors, baronyId, ToDTO, nameof(GetAdvisors));

        public async Task<AdvisorDTO> SaveAdvisor(AdvisorDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Advisors.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null)
                {
                    e = ToEntity(dto);
                    ctx.Advisors.Add(e);
                }
                else
                {
                    ApplyAdvisor(e, dto);
                }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveAdvisor)); }
        }

        public Task<int> DeleteAdvisor(int id) => Delete(ctx => ctx.Advisors, id, nameof(DeleteAdvisor));

        // ---------------- Buildings ----------------
        public async Task<List<BaronyBuildingDTO>> GetBuildings(int baronyId) =>
            await GetList(ctx => ctx.BaronyBuildings, baronyId, ToDTO, nameof(GetBuildings));

        public async Task<BaronyBuildingDTO> SaveBuilding(BaronyBuildingDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyBuildings.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyBuildings.Add(e); }
                else { ApplyBuilding(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBuilding)); }
        }

        public Task<int> DeleteBuilding(int id) => Delete(ctx => ctx.BaronyBuildings, id, nameof(DeleteBuilding));

        // ---------------- Social relations ----------------
        public async Task<List<SocialGroupRelationDTO>> GetSocialRelations(int baronyId) =>
            await GetList(ctx => ctx.SocialGroupRelations, baronyId, ToDTO, nameof(GetSocialRelations));

        public async Task<SocialGroupRelationDTO> SaveSocialRelation(SocialGroupRelationDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.SocialGroupRelations.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.SocialGroupRelations.Add(e); }
                else { ApplySocial(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveSocialRelation)); }
        }

        public Task<int> DeleteSocialRelation(int id) => Delete(ctx => ctx.SocialGroupRelations, id, nameof(DeleteSocialRelation));

        // ---------------- Decrees ----------------
        public async Task<List<DecreeDTO>> GetDecrees(int baronyId) =>
            await GetList(ctx => ctx.Decrees, baronyId, ToDTO, nameof(GetDecrees));

        public async Task<DecreeDTO> SaveDecree(DecreeDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Decrees.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.Decrees.Add(e); }
                else { ApplyDecree(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveDecree)); }
        }

        public Task<int> DeleteDecree(int id) => Delete(ctx => ctx.Decrees, id, nameof(DeleteDecree));

        // ---------------- Events ----------------
        public async Task<List<BaronyEventDTO>> GetEvents(int baronyId) =>
            await GetList(ctx => ctx.BaronyEvents, baronyId, ToDTO, nameof(GetEvents));

        public async Task<BaronyEventDTO> SaveEvent(BaronyEventDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyEvents.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyEvents.Add(e); }
                else { ApplyEvent(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveEvent)); }
        }

        public Task<int> DeleteEvent(int id) => Delete(ctx => ctx.BaronyEvents, id, nameof(DeleteEvent));

        // ---------------- Community modifiers ----------------
        public async Task<List<CommunityModifierDTO>> GetCommunityModifiers(int baronyId) =>
            await GetList(ctx => ctx.CommunityModifiers, baronyId, ToDTO, nameof(GetCommunityModifiers));

        public async Task<CommunityModifierDTO> SaveCommunityModifier(CommunityModifierDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.CommunityModifiers.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.CommunityModifiers.Add(e); }
                else { ApplyCommunity(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveCommunityModifier)); }
        }

        public Task<int> DeleteCommunityModifier(int id) => Delete(ctx => ctx.CommunityModifiers, id, nameof(DeleteCommunityModifier));

        // ---------------- Fiefs ----------------
        public async Task<List<FiefDTO>> GetFiefs(int baronyId) =>
            await GetList(ctx => ctx.Fiefs, baronyId, ToDTO, nameof(GetFiefs));

        public async Task<FiefDTO> SaveFief(FiefDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Fiefs.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.Fiefs.Add(e); }
                else { ApplyFief(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveFief)); }
        }

        public Task<int> DeleteFief(int id) => Delete(ctx => ctx.Fiefs, id, nameof(DeleteFief));

        // ---------------- Tiles ----------------
        public async Task<List<TerrainTileDTO>> GetTiles(int baronyId) =>
            await GetList(ctx => ctx.TerrainTiles, baronyId, ToDTO, nameof(GetTiles));

        public async Task<TerrainTileDTO> SaveTile(TerrainTileDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.TerrainTiles.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.TerrainTiles.Add(e); }
                else { ApplyTile(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveTile)); }
        }

        public Task<int> DeleteTile(int id) => Delete(ctx => ctx.TerrainTiles, id, nameof(DeleteTile));

        // ---------------- Improvements ----------------
        public async Task<List<TerrainImprovementDTO>> GetImprovements(int baronyId) =>
            await GetList(ctx => ctx.TerrainImprovements, baronyId, ToDTO, nameof(GetImprovements));

        public async Task<TerrainImprovementDTO> SaveImprovement(TerrainImprovementDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.TerrainImprovements.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.TerrainImprovements.Add(e); }
                else { ApplyImprovement(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveImprovement)); }
        }

        public Task<int> DeleteImprovement(int id) => Delete(ctx => ctx.TerrainImprovements, id, nameof(DeleteImprovement));

        // ---------------- Projects ----------------
        public async Task<List<BaronyProjectDTO>> GetProjects(int baronyId) =>
            await GetList(ctx => ctx.BaronyProjects, baronyId, ToDTO, nameof(GetProjects));

        public async Task<BaronyProjectDTO> SaveProject(BaronyProjectDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyProjects.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyProjects.Add(e); }
                else { ApplyProject(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveProject)); }
        }

        public Task<int> DeleteProject(int id) => Delete(ctx => ctx.BaronyProjects, id, nameof(DeleteProject));

        // ---------------- Building templates (global) ----------------
        public async Task<List<BuildingTemplateDTO>> GetBuildingTemplates()
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var list = await ctx.BuildingTemplates.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
                return list.Select(ToDTO).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetBuildingTemplates)); }
        }

        public async Task<BuildingTemplateDTO> SaveBuildingTemplate(BuildingTemplateDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BuildingTemplates.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BuildingTemplates.Add(e); }
                else { ApplyTemplate(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBuildingTemplate)); }
        }

        public Task<int> DeleteBuildingTemplate(int id) => Delete(ctx => ctx.BuildingTemplates, id, nameof(DeleteBuildingTemplate));

        // ---------------- Generic list/delete helpers ----------------
        private async Task<List<TDto>> GetList<TEntity, TDto>(
            Func<ApplicationDbContext, DbSet<TEntity>> set,
            int baronyId,
            Func<TEntity, TDto> map,
            string method) where TEntity : class
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var query = set(ctx).AsNoTracking();
                var filtered = query.Where(BuildBaronyPredicate<TEntity>(baronyId));
                var list = await filtered.ToListAsync();
                return list.Select(map).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, method); }
        }

        private static System.Linq.Expressions.Expression<Func<TEntity, bool>> BuildBaronyPredicate<TEntity>(int baronyId)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "x");
            var prop = System.Linq.Expressions.Expression.Property(param, "BaronyId");
            var constant = System.Linq.Expressions.Expression.Constant(baronyId);
            var body = System.Linq.Expressions.Expression.Equal(prop, constant);
            return System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        private async Task<int> Delete<TEntity>(
            Func<ApplicationDbContext, DbSet<TEntity>> set,
            int id,
            string method) where TEntity : class
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var dbSet = set(ctx);
                var e = await dbSet.FindAsync(id);
                if (e is null)
                    return 0;
                dbSet.Remove(e);
                return await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, method); }
        }

        // ---------------- Mapping: Barony ----------------
        private static BaronyDTO ToDTO(Barony e) => new()
        {
            Id = e.Id,
            CharacterId = e.CharacterId,
            Name = e.Name,
            Size = e.Size,
            Year = e.Year,
            Month = e.Month,
            TurnNumber = e.TurnNumber,
            Season = e.Season,
            TreasuryGold = e.TreasuryGold,
            BaronPurseGold = e.BaronPurseGold,
            FoodInGranaries = e.FoodInGranaries,
            Unrest = e.Unrest,
            BaseParameters = De(e.BaseParametersJson),
            Notes = e.Notes,
        };

        private static Barony ToEntity(BaronyDTO d)
        {
            var e = new Barony();
            ApplyBarony(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBarony(Barony e, BaronyDTO d)
        {
            e.CharacterId = d.CharacterId;
            e.Name = d.Name;
            e.Size = d.Size;
            e.Year = d.Year;
            e.Month = d.Month;
            e.TurnNumber = d.TurnNumber;
            e.Season = d.Season;
            e.TreasuryGold = d.TreasuryGold;
            e.BaronPurseGold = d.BaronPurseGold;
            e.FoodInGranaries = d.FoodInGranaries;
            e.Unrest = d.Unrest;
            e.BaseParametersJson = Ser(d.BaseParameters);
            e.Notes = d.Notes;
        }

        // ---------------- Mapping: Advisor ----------------
        private static AdvisorDTO ToDTO(Advisor e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, OfficeType = e.OfficeType, Title = e.Title,
            PersonName = e.PersonName, IsBaron = e.IsBaron, HasAssistant = e.HasAssistant,
            AssistantBonus = e.AssistantBonus, Skills = De(e.SkillsJson), Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), FormulaText = e.FormulaText, Description = e.Description, UpkeepGold = e.UpkeepGold,
        };

        private static Advisor ToEntity(AdvisorDTO d) { var e = new Advisor(); ApplyAdvisor(e, d); e.Id = d.Id; return e; }

        private static void ApplyAdvisor(Advisor e, AdvisorDTO d)
        {
            e.BaronyId = d.BaronyId; e.OfficeType = d.OfficeType; e.Title = d.Title; e.PersonName = d.PersonName;
            e.IsBaron = d.IsBaron; e.HasAssistant = d.HasAssistant; e.AssistantBonus = d.AssistantBonus;
            e.SkillsJson = Ser(d.Skills); e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent);
            e.FormulaText = d.FormulaText; e.Description = d.Description; e.UpkeepGold = d.UpkeepGold;
        }

        // ---------------- Mapping: Building ----------------
        private static BaronyBuildingDTO ToDTO(BaronyBuilding e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, TemplateId = e.TemplateId, Name = e.Name, Kind = e.Kind,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description,
        };

        private static BaronyBuilding ToEntity(BaronyBuildingDTO d) { var e = new BaronyBuilding(); ApplyBuilding(e, d); e.Id = d.Id; return e; }

        private static void ApplyBuilding(BaronyBuilding e, BaronyBuildingDTO d)
        {
            e.BaronyId = d.BaronyId; e.TemplateId = d.TemplateId; e.Name = d.Name; e.Kind = d.Kind;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description;
        }

        // ---------------- Mapping: Social ----------------
        private static SocialGroupRelationDTO ToDTO(SocialGroupRelation e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Group = e.Group, RelationLevel = e.RelationLevel,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), FormulaText = e.FormulaText,
        };

        private static SocialGroupRelation ToEntity(SocialGroupRelationDTO d) { var e = new SocialGroupRelation(); ApplySocial(e, d); e.Id = d.Id; return e; }

        private static void ApplySocial(SocialGroupRelation e, SocialGroupRelationDTO d)
        {
            e.BaronyId = d.BaronyId; e.Group = d.Group; e.RelationLevel = d.RelationLevel;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Decree ----------------
        private static DecreeDTO ToDTO(Decree e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), Description = e.Description, FormulaText = e.FormulaText,
        };

        private static Decree ToEntity(DecreeDTO d) { var e = new Decree(); ApplyDecree(e, d); e.Id = d.Id; return e; }

        private static void ApplyDecree(Decree e, DecreeDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent); e.Description = d.Description; e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Event ----------------
        private static BaronyEventDTO ToDTO(BaronyEvent e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, TurnNumber = e.TurnNumber, IsActive = e.IsActive,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description,
        };

        private static BaronyEvent ToEntity(BaronyEventDTO d) { var e = new BaronyEvent(); ApplyEvent(e, d); e.Id = d.Id; return e; }

        private static void ApplyEvent(BaronyEvent e, BaronyEventDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.TurnNumber = d.TurnNumber; e.IsActive = d.IsActive;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description;
        }

        // ---------------- Mapping: Community ----------------
        private static CommunityModifierDTO ToDTO(CommunityModifier e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Source = e.Source, Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), FormulaText = e.FormulaText,
        };

        private static CommunityModifier ToEntity(CommunityModifierDTO d) { var e = new CommunityModifier(); ApplyCommunity(e, d); e.Id = d.Id; return e; }

        private static void ApplyCommunity(CommunityModifier e, CommunityModifierDTO d)
        {
            e.BaronyId = d.BaronyId; e.Source = d.Source; e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent); e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Fief ----------------
        private static FiefDTO ToDTO(Fief e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, LiegeName = e.LiegeName,
            IsBaronDemesne = e.IsBaronDemesne, BonusMultiplier = e.BonusMultiplier,
        };

        private static Fief ToEntity(FiefDTO d) { var e = new Fief(); ApplyFief(e, d); e.Id = d.Id; return e; }

        private static void ApplyFief(Fief e, FiefDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.LiegeName = d.LiegeName;
            e.IsBaronDemesne = d.IsBaronDemesne; e.BonusMultiplier = d.BonusMultiplier;
        }

        // ---------------- Mapping: Tile ----------------
        private static TerrainTileDTO ToDTO(TerrainTile e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, X = e.X, Y = e.Y, BaseType = e.BaseType,
            FeaturesCsv = e.FeaturesCsv, Fertility = e.Fertility, Resource = e.Resource,
            FiefId = e.FiefId, Comment = e.Comment,
        };

        private static TerrainTile ToEntity(TerrainTileDTO d) { var e = new TerrainTile(); ApplyTile(e, d); e.Id = d.Id; return e; }

        private static void ApplyTile(TerrainTile e, TerrainTileDTO d)
        {
            e.BaronyId = d.BaronyId; e.X = d.X; e.Y = d.Y; e.BaseType = d.BaseType;
            e.FeaturesCsv = d.FeaturesCsv; e.Fertility = d.Fertility; e.Resource = d.Resource;
            e.FiefId = d.FiefId; e.Comment = d.Comment;
        }

        // ---------------- Mapping: Improvement ----------------
        private static TerrainImprovementDTO ToDTO(TerrainImprovement e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, TileId = e.TileId, TemplateId = e.TemplateId, Name = e.Name,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description, FormulaText = e.FormulaText,
        };

        private static TerrainImprovement ToEntity(TerrainImprovementDTO d) { var e = new TerrainImprovement(); ApplyImprovement(e, d); e.Id = d.Id; return e; }

        private static void ApplyImprovement(TerrainImprovement e, TerrainImprovementDTO d)
        {
            e.BaronyId = d.BaronyId; e.TileId = d.TileId; e.TemplateId = d.TemplateId; e.Name = d.Name;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description; e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Project ----------------
        private static BaronyProjectDTO ToDTO(BaronyProject e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, Cost = De(e.CostJson), Result = De(e.ResultJson),
            Allocated = De(e.AllocatedJson), ResultDescription = e.ResultDescription, Status = e.Status,
            TurnsRemaining = e.TurnsRemaining, Notes = e.Notes,
        };

        private static BaronyProject ToEntity(BaronyProjectDTO d) { var e = new BaronyProject(); ApplyProject(e, d); e.Id = d.Id; return e; }

        private static void ApplyProject(BaronyProject e, BaronyProjectDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.CostJson = Ser(d.Cost); e.ResultJson = Ser(d.Result);
            e.AllocatedJson = Ser(d.Allocated); e.ResultDescription = d.ResultDescription; e.Status = d.Status;
            e.TurnsRemaining = d.TurnsRemaining; e.Notes = d.Notes;
        }

        // ---------------- Mapping: Template ----------------
        private static BuildingTemplateDTO ToDTO(BuildingTemplate e) => new()
        {
            Id = e.Id, Name = e.Name, Kind = e.Kind, GoldCost = e.GoldCost, ProductionCost = e.ProductionCost,
            EffectAdditive = De(e.EffectAdditiveJson), EffectPercent = De(e.EffectPercentJson),
            Description = e.Description, TerrainRequirement = e.TerrainRequirement,
        };

        private static BuildingTemplate ToEntity(BuildingTemplateDTO d) { var e = new BuildingTemplate(); ApplyTemplate(e, d); e.Id = d.Id; return e; }

        private static void ApplyTemplate(BuildingTemplate e, BuildingTemplateDTO d)
        {
            e.Name = d.Name; e.Kind = d.Kind; e.GoldCost = d.GoldCost; e.ProductionCost = d.ProductionCost;
            e.EffectAdditiveJson = Ser(d.EffectAdditive); e.EffectPercentJson = Ser(d.EffectPercent);
            e.Description = d.Description; e.TerrainRequirement = d.TerrainRequirement;
        }
    }
}
