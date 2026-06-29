using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.CharacterReps
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public LanguageRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LanguageDTO>> GetAllApproved()
        {
            using var context = await _db.CreateDbContextAsync();
            var languages = await context.Languages
                .AsNoTracking()
                .Where(l => l.IsApproved)
                .OrderBy(l => l.Index)
                .ThenBy(l => l.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<Language>, IEnumerable<LanguageDTO>>(languages);
        }

        public async Task<IEnumerable<LanguageDTO>> GetForCharacter(int characterId)
        {
            using var context = await _db.CreateDbContextAsync();
            var character = await context.Characters
                .AsNoTracking()
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character?.Languages is null)
                return Enumerable.Empty<LanguageDTO>();

            return _mapper.Map<IEnumerable<Language>, IEnumerable<LanguageDTO>>(
                character.Languages.OrderBy(l => l.Index).ThenBy(l => l.Name));
        }

        public async Task SetCharacterLanguages(int characterId, IEnumerable<int> languageIds, int maxSlots)
        {
            var ids = languageIds.Distinct().ToList();
            if (ids.Count > maxSlots)
                throw new RepositoryErrorException($"Character can know at most {maxSlots} languages.");

            try
            {
                using var context = await _db.CreateDbContextAsync();
                var character = await context.Characters
                    .Include(c => c.Languages)
                    .FirstOrDefaultAsync(c => c.Id == characterId);

                if (character is null)
                    throw new RepositoryErrorException("Character not found.");

                var languages = await context.Languages
                    .Where(l => ids.Contains(l.Id) && l.IsApproved)
                    .ToListAsync();

                if (languages.Count != ids.Count)
                    throw new RepositoryErrorException("One or more selected languages are invalid.");

                character.Languages ??= new List<Language>();
                character.Languages.Clear();
                foreach (var language in languages.OrderBy(l => l.Index).ThenBy(l => l.Name))
                    character.Languages.Add(language);

                await context.SaveChangesAsync();
            }
            catch (RepositoryErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Language Repository SetCharacterLanguages: " + ex.Message);
            }
        }
    }
}
