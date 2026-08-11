using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Common.Barony;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DA_Business.Repository.ChatRepos
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public ChapterRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private static IQueryable<ChapterDTO> ProjectChapterList(IQueryable<Chapter> query) =>
            query.AsNoTracking()
                .OrderBy(c => c.CreatedDate)
                .Select(c => new ChapterDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DateNumber = c.DateNumber,
                    DayTime = c.DayTime,
                    Place = c.Place,
                    CreatedDate = c.CreatedDate,
                    IsFinished = c.IsFinished,
                    CampaignId = c.CampaignId,
                    Posts = new List<PostDTO>(),
                    Characters = c.Characters
                        .Select(ch => new CharacterDTO
                        {
                            Id = ch.Id,
                            NPCName = ch.NPCName,
                            ImageUrl = ch.ImageUrl,
                        })
                        .ToList(),
                });

        public async Task<ChapterDTO> Create(ChapterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<ChapterDTO, Chapter>(objDTO);

                var characterIds = obj.Characters.Select(c => c.Id).ToList();
                var trackedCharacters = await contex.Characters
                    .Where(c => characterIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id);

                var charactersToReplace = obj.Characters.Where(c => trackedCharacters.ContainsKey(c.Id)).ToList();
                foreach (var untracked in charactersToReplace)
                {
                    obj.Characters.Remove(untracked);
                    obj.Characters.Add(trackedCharacters[untracked.Id]);
                }

                var postIds = obj.Posts.Select(p => p.Id).ToList();
                var trackedPosts = await contex.Posts
                    .Where(p => postIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var postsToReplace = obj.Posts.Where(p => trackedPosts.ContainsKey(p.Id)).ToList();
                foreach (var untracked in postsToReplace)
                {
                    obj.Posts.Remove(untracked);
                    obj.Posts.Add(trackedPosts[untracked.Id]);
                }

                var addedObj = await contex.Chapters.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Chapter, ChapterDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<bool> CheckIfChapterBelongToUser(string userName, int chapterId)
        {
            using var contex = await _db.CreateDbContextAsync();
            return await contex.Chapters
                .AsNoTracking()
                .AnyAsync(c => c.Id == chapterId && c.Characters.Any(ch => ch.UserName == userName));
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters.Include(a => a.Posts).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    if (obj.Posts != null && obj.Posts.Any())
                    {
                        foreach (var post in obj.Posts)
                        {
                            contex.Posts.Remove(post);
                        }
                    }
                    contex.Chapters.Remove(obj);
                    return contex.SaveChanges();
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
            return 0;
        }

        public async Task<IEnumerable<ChapterDTO>> GetAll(int? campaignId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var query = contex.Chapters.AsQueryable();
                if (campaignId is > 0)
                {
                    query = query.Where(u => u.CampaignId == campaignId);
                }

                var chapters = await ProjectChapterList(query).ToListAsync();
                await PopulateBaronyTotalsAsync(contex, chapters);
                return chapters;
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
        }

        public async Task<IEnumerable<ChapterDTO>> GetAllForUser(int characterId, int? campaignId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var query = contex.Chapters.AsQueryable();

                if (campaignId is null or < 1)
                {
                    var chapters = await query
                        .AsNoTracking()
                        .OrderBy(c => c.CreatedDate)
                        .Select(c => new ChapterDTO
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            DateNumber = c.DateNumber,
                            DayTime = c.DayTime,
                            Place = c.Place,
                            CreatedDate = c.CreatedDate,
                            IsFinished = c.IsFinished,
                            CampaignId = c.CampaignId,
                            Posts = new List<PostDTO>(),
                            Characters = c.Characters
                                .Where(ch => ch.Id == characterId)
                                .Select(ch => new CharacterDTO
                                {
                                    Id = ch.Id,
                                    NPCName = ch.NPCName,
                                    ImageUrl = ch.ImageUrl,
                                })
                                .ToList(),
                        })
                        .ToListAsync();

                    await PopulateBaronyTotalsAsync(contex, chapters);
                    return chapters;
                }

                query = query.Where(u => u.CampaignId == campaignId && u.Characters.Any(c => c.Id == characterId));
                var campaignChapters = await ProjectChapterList(query).ToListAsync();
                await PopulateBaronyTotalsAsync(contex, campaignChapters);
                return campaignChapters;
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
        }

        public async Task<ChapterDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters
                    .AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(c => new ChapterDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        DateNumber = c.DateNumber,
                        DayTime = c.DayTime,
                        Place = c.Place,
                        CreatedDate = c.CreatedDate,
                        IsFinished = c.IsFinished,
                        CampaignId = c.CampaignId,
                        Characters = c.Characters
                            .Select(ch => new CharacterDTO
                            {
                                Id = ch.Id,
                                NPCName = ch.NPCName,
                                UserName = ch.UserName,
                                ImageUrl = ch.ImageUrl,
                            })
                            .ToList(),
                        Posts = new List<PostDTO>()
                    })
                    .FirstOrDefaultAsync();
                if (obj != null)
                {
                    await PopulateBaronyTotalsAsync(contex, new List<ChapterDTO> { obj });
                    return obj;
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
            return new ChapterDTO();
        }

        public async Task<ChapterDTO> Update(ChapterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters.Include(a => a.Characters).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updatedChapter = _mapper.Map<ChapterDTO, Chapter>(objDTO);

                    contex.Entry(obj).CurrentValues.SetValues(updatedChapter);

                    if (obj.Characters is not null)
                    {
                        foreach (var existingChild in obj.Characters)
                        {
                            if (!updatedChapter.Characters.Any(c => c.Id == existingChild.Id))
                            {
                                obj.Characters.Remove(existingChild);
                            }
                        }
                    }

                    if (updatedChapter.Characters is not null)
                    {
                        foreach (var childChar in updatedChapter.Characters)
                        {
                            if (!obj.Characters.Any(c => c.Id == childChar.Id && c.Id != default(int)))
                            {
                                var existingChar = contex.Characters.Include(c => c.Chapters).FirstOrDefault(c => c.Id == childChar.Id);
                                existingChar!.Chapters.Add(obj);
                                contex.Characters.Update(existingChar);
                            }
                        }
                    }
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Chapter, ChapterDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<ChapterDTO, Chapter>(objDTO);

                    var characterIds = obj.Characters.Select(c => c.Id).ToList();
                    var trackedCharacters = await contex.Characters
                        .Where(c => characterIds.Contains(c.Id))
                        .ToDictionaryAsync(c => c.Id);

                    var charactersToReplace = obj.Characters.Where(c => trackedCharacters.ContainsKey(c.Id)).ToList();
                    foreach (var untracked in charactersToReplace)
                    {
                        obj.Characters.Remove(untracked);
                        obj.Characters.Add(trackedCharacters[untracked.Id]);
                    }

                    var addedObj = contex.Chapters.Add(obj);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<Chapter, ChapterDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        private static async Task PopulateBaronyTotalsAsync(ApplicationDbContext context, IList<ChapterDTO> chapters)
        {
            if (chapters.Count == 0)
                return;

            var chapterIds = chapters.Select(c => c.Id).ToList();
            var resourcePosts = await context.Posts
                .AsNoTracking()
                .Where(p => chapterIds.Contains(p.ChapterId) && p.AlternativeName == "Barony resources")
                .Select(p => new { p.ChapterId, p.Content })
                .ToListAsync();

            var totalsByChapter = resourcePosts
                .GroupBy(p => p.ChapterId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var ppb = new PpbVector();
                        var prestige = 0;
                        var honor = 0;
                        var fear = 0;
                        foreach (var post in g)
                        {
                            var delta = ExtractBaronyDelta(post.Content);
                            ppb.AddInPlace(delta.PpbDelta);
                            prestige += delta.PrestigeDelta;
                            honor += delta.HonorDelta;
                            fear += delta.FearDelta;
                        }
                        return (ppb, prestige, honor, fear);
                    });

            foreach (var chapter in chapters)
            {
                if (totalsByChapter.TryGetValue(chapter.Id, out var totals))
                {
                    chapter.BaronyPpbTotal = totals.ppb;
                    chapter.BaronyPrestigeTotal = totals.prestige;
                    chapter.BaronyHonorTotal = totals.honor;
                    chapter.BaronyFearTotal = totals.fear;
                }
                else
                {
                    chapter.BaronyPpbTotal = new PpbVector();
                    chapter.BaronyPrestigeTotal = 0;
                    chapter.BaronyHonorTotal = 0;
                    chapter.BaronyFearTotal = 0;
                }
            }
        }

        private static BaronyResourceDelta ExtractBaronyDelta(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new BaronyResourceDelta();

            // New format: hidden JSON payload embedded in the post.
            var payloadMatch = Regex.Match(
                content,
                @"<!--\s*BARONY_RESOURCE:(\{.*?\})\s*-->",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (payloadMatch.Success)
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<BaronyResourcePayload>(payloadMatch.Groups[1].Value);
                    if (payload is not null)
                    {
                        return new BaronyResourceDelta
                        {
                            PpbDelta = payload.PpbDelta ?? new PpbVector(),
                            PrestigeDelta = payload.PrestigeDelta,
                            HonorDelta = payload.HonorDelta,
                            FearDelta = payload.FearDelta
                        };
                    }
                }
                catch
                {
                    // Fall back to legacy parsing below.
                }
            }

            // Legacy fallback: "PPB: X" and "Prestige: Y" from first implementation.
            var legacyPpb = ExtractLegacyInt(content, "PPB");
            var legacyPrestige = ExtractLegacyInt(content, "Prestige");
            var ppbVector = new PpbVector();
            ppbVector[Ppb.Treasury] = legacyPpb;
            return new BaronyResourceDelta
            {
                PpbDelta = ppbVector,
                PrestigeDelta = legacyPrestige
            };
        }

        private static int ExtractLegacyInt(string content, string label)
        {
            var match = Regex.Match(content, $"{Regex.Escape(label)}:\\s*([+-]?\\d+)", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : 0;
        }

        private sealed class BaronyResourcePayload
        {
            public PpbVector? PpbDelta { get; set; }
            public int PrestigeDelta { get; set; }
            public int HonorDelta { get; set; }
            public int FearDelta { get; set; }
        }

        private sealed class BaronyResourceDelta
        {
            public PpbVector PpbDelta { get; set; } = new();
            public int PrestigeDelta { get; set; }
            public int HonorDelta { get; set; }
            public int FearDelta { get; set; }
        }
    }
}
