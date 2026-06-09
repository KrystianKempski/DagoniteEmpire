using AutoMapper;
using DA_Business.Repository.CharacterReps;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models;
using DA_Models.CharacterModels;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Tests.Helpers;

/// <summary>
/// Mirrors the CharacterUpsert create path at repository level so we can verify persistence end-to-end.
/// </summary>
public sealed class CharacterCreationTestHelper
{
    private readonly IDbContextFactory<ApplicationDbContext> _db;
    private readonly IMapper _mapper;

    public RaceRepository RaceRepository { get; }
    public ProfessionRepository ProfessionRepository { get; }
    public ITraitRepository<TraitProfessionDTO> TraitProfessionRepository { get; }
    public CharacterRepository CharacterRepository { get; }
    public AttributeRepository AttributeRepository { get; }
    public BaseSkillRepository BaseSkillRepository { get; }
    public SpecialSkillRepository SpecialSkillRepository { get; }
    public ITraitRepository<TraitCharacterDTO> TraitCharacterRepository { get; }
    public EquipmentRepository EquipmentRepository { get; }
    public EquipmentSlotRepository EquipmentSlotRepository { get; }

    public CharacterCreationTestHelper(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
        RaceRepository = new RaceRepository(db, mapper);
        ProfessionRepository = new ProfessionRepository(db, mapper);
        TraitProfessionRepository = new TraitProfessionRepository(db, mapper);
        CharacterRepository = new CharacterRepository(db, mapper);
        AttributeRepository = new AttributeRepository(db, mapper);
        BaseSkillRepository = new BaseSkillRepository(db, mapper);
        SpecialSkillRepository = new SpecialSkillRepository(db, mapper);
        TraitCharacterRepository = new TraitCharacterRepository(db, mapper);
        EquipmentRepository = new EquipmentRepository(db, mapper);
        EquipmentSlotRepository = new EquipmentSlotRepository(db, mapper);
    }

    public static void SeedFistsEquipment(ApplicationDbContext context)
    {
        if (context.Equipment.Any(e => e.Name == SD.BasicWeaponsMelee.Fists))
            return;

        context.Equipment.Add(new Equipment
        {
            Name = SD.BasicWeaponsMelee.Fists,
            Description = "Unarmed",
            ShortDescr = "Fists",
            IsApproved = true,
            EquipmentType = "melee",
        });
        context.SaveChanges();
    }

    public async Task<int> CreateCharacterAsync(
        string userName = "hero-player",
        string npcName = "Test Hero",
        int attributePoints = 100,
        int currentExpPoints = 250)
    {
        var race = await RaceRepository.Create(new RaceDTO
        {
            Name = "Test Race",
            Description = "Race for integration test",
            RaceApproved = false,
        });

        var profession = new ProfessionDTO
        {
            Name = "Test Class",
            Description = "Class for integration test",
            RelatedAttributeName = SD.Attributes.Strength,
            IsApproved = false,
            ClassLevel = 2,
        };

        var professionDto = await ProfessionRepository.Create(profession);
        if (profession.Traits is not null)
        {
            foreach (var skill in profession.Traits)
            {
                skill.ProfessionId = professionDto.Id;
                await TraitProfessionRepository.Create(skill);
            }
        }

        var fists = await EquipmentRepository.GetByName(SD.BasicWeaponsMelee.Fists);
        Assert.True(fists.Id > 0, "Fists equipment must be seeded before character creation tests.");

        var equipmentSlots = new List<EquipmentSlotDTO> { new(fists) };

        var characterDto = new CharacterDTO
        {
            UserName = userName,
            NPCName = npcName,
            Description = "Integration test character",
            RaceId = race.Id,
            ProfessionId = professionDto.Id,
            Age = 30,
            IsApproved = false,
            NPCType = SD.NPCType.Hero,
            AttributePoints = attributePoints,
            CurrentExpPoints = currentExpPoints,
            EquipmentSlots = equipmentSlots,
        };

        var character = await CharacterRepository.Create(characterDto);

        foreach (var attr in CharacterSeeder.GetAttributes().Values)
        {
            attr.CharacterId = character.Id;
            await AttributeRepository.Create(attr);
        }

        foreach (var skill in CharacterSeeder.GetBaseSkills())
        {
            skill.CharacterId = character.Id;
            await BaseSkillRepository.Create(skill);
        }

        foreach (var skill in CharacterSeeder.GetSpecialSkills())
        {
            skill.CharacterId = character.Id;
            await SpecialSkillRepository.Create(skill);
        }

        return character.Id;
    }

    public async Task<LoadedCharacterSnapshot> LoadCharacterSnapshotAsync(int characterId)
    {
        var character = await CharacterRepository.GetById(characterId);
        var attributes = await AttributeRepository.GetAll(characterId);
        var baseSkills = (await BaseSkillRepository.GetAll(characterId)).ToList();
        var specialSkills = await SpecialSkillRepository.GetAll(characterId);
        var traits = (await TraitCharacterRepository.GetAll(characterId)).ToList();
        var profession = await ProfessionRepository.GetById(character.ProfessionId);
        var professionTraits = (await TraitProfessionRepository.GetAll(character.ProfessionId)).ToList();
        var equipmentSlots = (await EquipmentSlotRepository.GetAll(characterId)).ToList();

        return new LoadedCharacterSnapshot(
            character,
            attributes,
            baseSkills,
            specialSkills,
            traits,
            profession,
            professionTraits,
            equipmentSlots);
    }

    public async Task UpdateAttributeAsync(int characterId, string attributeName, int newBaseBonus)
    {
        var attributes = await AttributeRepository.GetAll(characterId);
        var attr = attributes[attributeName];
        attr.BaseBonus = newBaseBonus;
        await AttributeRepository.Update(attr);
    }
}

public sealed record LoadedCharacterSnapshot(
    CharacterDTO Character,
    IDictionary<string, AttributeDTO> Attributes,
    IReadOnlyList<BaseSkillDTO> BaseSkills,
    IDictionary<string, SpecialSkillDTO> SpecialSkills,
    IReadOnlyList<TraitCharacterDTO> Traits,
    ProfessionDTO Profession,
    IReadOnlyList<TraitProfessionDTO> ProfessionTraits,
    IReadOnlyList<EquipmentSlotDTO> EquipmentSlots);
