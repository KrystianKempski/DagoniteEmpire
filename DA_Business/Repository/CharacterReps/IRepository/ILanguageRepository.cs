using DA_Models.CharacterModels;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ILanguageRepository
    {
        Task<IEnumerable<LanguageDTO>> GetAllApproved();
        Task<IEnumerable<LanguageDTO>> GetForCharacter(int characterId);
        Task SetCharacterLanguages(int characterId, IEnumerable<int> languageIds, int maxSlots);
    }
}
