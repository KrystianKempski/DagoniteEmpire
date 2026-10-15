using DA_Business.Repository.CharacterReps.IRepository;
using DA_Models.BaronyModels;
using DA_Models.CharacterModels;
using DA_Models.ComponentModels;

namespace DagoniteEmpire.Pages.Barony;

/// <summary>
/// Loads full character sheets for barony skill rolls: the baron plus every
/// <see cref="CharacterDTO"/> on the barony's dedicated campaign.
/// </summary>
public sealed class BaronyRollRoster
{
    private readonly IBaronyRepository _baronyRepo;
    private readonly ICampaignRepository _campaignRepo;
    private readonly ICharacterRepository _characterRepo;
    private readonly IAttributeRepository _attributeRepo;
    private readonly IBaseSkillRepository _baseSkillRepo;
    private readonly ISpecialSkillRepository _specialSkillRepo;
    private readonly ITraitRepository<TraitCharacterDTO> _traitCharacterRepo;
    private readonly ITraitRepository<TraitProfessionDTO> _traitProfessionRepo;
    private readonly IEquipmentSlotRepository _equipmentSlotRepo;
    private readonly IWoundRepository _woundRepo;
    private readonly IRaceRepository _raceRepo;
    private readonly IProfessionRepository _professionRepo;

    public BaronyRollRoster(
        IBaronyRepository baronyRepo,
        ICampaignRepository campaignRepo,
        ICharacterRepository characterRepo,
        IAttributeRepository attributeRepo,
        IBaseSkillRepository baseSkillRepo,
        ISpecialSkillRepository specialSkillRepo,
        ITraitRepository<TraitCharacterDTO> traitCharacterRepo,
        ITraitRepository<TraitProfessionDTO> traitProfessionRepo,
        IEquipmentSlotRepository equipmentSlotRepo,
        IWoundRepository woundRepo,
        IRaceRepository raceRepo,
        IProfessionRepository professionRepo)
    {
        _baronyRepo = baronyRepo;
        _campaignRepo = campaignRepo;
        _characterRepo = characterRepo;
        _attributeRepo = attributeRepo;
        _baseSkillRepo = baseSkillRepo;
        _specialSkillRepo = specialSkillRepo;
        _traitCharacterRepo = traitCharacterRepo;
        _traitProfessionRepo = traitProfessionRepo;
        _equipmentSlotRepo = equipmentSlotRepo;
        _woundRepo = woundRepo;
        _raceRepo = raceRepo;
        _professionRepo = professionRepo;
    }

    public async Task<IReadOnlyList<AllParamsModel>> LoadAsync(BaronyDTO barony)
    {
        if (barony.CharacterId <= 0)
            return Array.Empty<AllParamsModel>();

        var ids = new HashSet<int> { barony.CharacterId };

        var campaigns = (await _campaignRepo.GetAll(barony.CharacterId)).ToList();
        var campaign = campaigns.FirstOrDefault(c =>
                             string.Equals(c.Name, barony.Name, StringComparison.OrdinalIgnoreCase))
                       ?? campaigns.OrderByDescending(c => c.CreatedDate).FirstOrDefault();

        if (campaign?.Characters is not null)
        {
            foreach (var character in campaign.Characters)
            {
                if (character.Id > 0)
                    ids.Add(character.Id);
            }
        }

        var list = new List<AllParamsModel>(ids.Count);
        foreach (var id in ids.OrderBy(x => x == barony.CharacterId ? 0 : 1).ThenBy(x => x))
        {
            var loaded = await LoadCharacterAsync(id);
            if (loaded?.Character is not null)
                list.Add(loaded);
        }

        return list;
    }

    private async Task<AllParamsModel?> LoadCharacterAsync(int id)
    {
        var characterTask = _characterRepo.GetById(id);
        var attributesTask = _attributeRepo.GetAll(id);
        var baseSkillsTask = _baseSkillRepo.GetAll(id);
        var specialSkillsTask = _specialSkillRepo.GetAll(id);
        var traitCharactersTask = _traitCharacterRepo.GetAll(id);
        var equipmentSlotsTask = _equipmentSlotRepo.GetAll(id);
        var woundsTask = _woundRepo.GetAll(id);
        var condsTask = _woundRepo.GetAllCond(id);

        await Task.WhenAll(
            characterTask,
            attributesTask,
            baseSkillsTask,
            specialSkillsTask,
            traitCharactersTask,
            equipmentSlotsTask,
            woundsTask,
            condsTask);

        var character = await characterTask;
        if (character is null || character.Id <= 0)
            return null;

        var allParams = new AllParamsModel
        {
            Character = character,
        };
        allParams.Attributes.FillPropertiesContainer(await attributesTask);
        allParams.BaseSkills = await baseSkillsTask;
        allParams.SpecialSkills.FillPropertiesContainer(await specialSkillsTask);

        var characterTraits = (await traitCharactersTask).ToList();
        allParams.TraitsCharacter = characterTraits.Where(u => u.IsTemporary == false).Cast<TraitDTO>().ToList();
        allParams.TraitsTemporary = characterTraits.Where(u => u.IsTemporary == true).Cast<TraitDTO>().ToList();

        var raceTask = _raceRepo.GetById(allParams.Character.RaceId);
        var professionTask = _professionRepo.GetById(allParams.Character.ProfessionId);
        var professionTraitsTask = _traitProfessionRepo.GetAll(allParams.Character.Id);

        await Task.WhenAll(raceTask, professionTask, professionTraitsTask);

        allParams.CurrentRace = await raceTask;
        allParams.Profession = await professionTask ?? new ProfessionDTO();
        allParams.Profession.Traits = (await professionTraitsTask).ToList();
        allParams.EquipmentSlots = (await equipmentSlotsTask).ToList();

        allParams.SpecialSkills.AddRelatedParametes();
        await BaronReputationCharacterOverlay.ApplyAsync(allParams, _baronyRepo, id);
        allParams.AllTraitsChange();
        allParams.Profession.CalculateClassParams(allParams.Attributes.GetAll());

        ICollection<WoundDTO> wounds = (await woundsTask).ToList();
        ICollection<WoundDTO> conds = (await condsTask).Cast<WoundDTO>().ToList();
        wounds = wounds.Concat(conds).ToList();
        allParams.Health.FillPropertiesContainer(wounds);
        allParams.Health.UpdateHealthBonusesInAttributes();

        return allParams;
    }
}
