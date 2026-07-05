using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DA_Business.Repository.ChatRepos
{
    public class BattleMapRepository : IBattleMapRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BattleMapRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        public async Task<BattleMapDTO> GetOrCreateForChapter(int chapterId, int campaignId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.BattleMaps.AsNoTracking().FirstOrDefaultAsync(u => u.ChapterId == chapterId);
                if (obj is not null)
                    return ToDTO(obj);

                var entity = new BattleMap
                {
                    ChapterId = chapterId,
                    CampaignId = campaignId,
                    Width = 10,
                    Height = 10,
                    CellsJson = "[]",
                    TokensJson = "[]",
                };
                var added = await contex.BattleMaps.AddAsync(entity);
                await contex.SaveChangesAsync();
                return ToDTO(added.Entity);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<BattleMapDTO> Update(BattleMapDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.BattleMaps.FirstOrDefaultAsync(u => u.Id == objDTO.Id)
                          ?? await contex.BattleMaps.FirstOrDefaultAsync(u => u.ChapterId == objDTO.ChapterId);

                if (obj is null)
                {
                    obj = ToEntity(objDTO);
                    var added = contex.BattleMaps.Add(obj);
                    await contex.SaveChangesAsync();
                    return ToDTO(added.Entity);
                }

                obj.CampaignId = objDTO.CampaignId;
                obj.Width = objDTO.Width;
                obj.Height = objDTO.Height;
                obj.CellsJson = JsonSerializer.Serialize(objDTO.Cells, JsonOptions);
                obj.TokensJson = JsonSerializer.Serialize(objDTO.Tokens, JsonOptions);
                await contex.SaveChangesAsync();
                return ToDTO(obj);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message);
            }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.BattleMaps.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    contex.BattleMaps.Remove(obj);
                    return await contex.SaveChangesAsync();
                }
            }
            catch (System.Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex); }
            return 0;
        }

        private static BattleMapDTO ToDTO(BattleMap entity) => new()
        {
            Id = entity.Id,
            ChapterId = entity.ChapterId,
            CampaignId = entity.CampaignId,
            Width = entity.Width,
            Height = entity.Height,
            Cells = Deserialize<List<BattleMapCellDTO>>(entity.CellsJson) ?? new(),
            Tokens = Deserialize<List<BattleMapTokenDTO>>(entity.TokensJson) ?? new(),
        };

        private static BattleMap ToEntity(BattleMapDTO dto) => new()
        {
            Id = dto.Id,
            ChapterId = dto.ChapterId,
            CampaignId = dto.CampaignId,
            Width = dto.Width,
            Height = dto.Height,
            CellsJson = JsonSerializer.Serialize(dto.Cells, JsonOptions),
            TokensJson = JsonSerializer.Serialize(dto.Tokens, JsonOptions),
        };

        private static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;
            try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch { return default; }
        }
    }
}
