using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common;
using DA_Models;
using DA_Models.CharacterModels;
using DA_Models.ComponentModels;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Tests.Repositories;

public class CharacterCreationFlowTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CharacterCreationTestHelper _helper;

    public CharacterCreationFlowTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _helper = new CharacterCreationTestHelper(_fixture.DbContextFactory, _fixture.Mapper);

        using var context = _fixture.CreateContext();
        CharacterCreationTestHelper.SeedUnarmedEquipment(context);
    }

    [Fact]
    public async Task CreateCharacter_PersistsCharacterAndForeignKeys()
    {
        var characterId = await _helper.CreateCharacterAsync();

        var snapshot = await _helper.LoadCharacterSnapshotAsync(characterId);

        Assert.True(snapshot.Character.Id > 0);
        Assert.Equal("Test Hero", snapshot.Character.NPCName);
        Assert.Equal("hero-player", snapshot.Character.UserName);
        Assert.True(snapshot.Character.RaceId > 0);
        Assert.True(snapshot.Character.ProfessionId > 0);
        Assert.Equal(snapshot.Character.ProfessionId, snapshot.Profession.Id);
    }

    [Fact]
    public async Task CreateCharacter_PersistsSeededAttributesBaseSkillsAndSpecialSkills()
    {
        var characterId = await _helper.CreateCharacterAsync();

        var snapshot = await _helper.LoadCharacterSnapshotAsync(characterId);

        Assert.Equal(CharacterSeeder.GetAttributes().Count, snapshot.Attributes.Count);
        Assert.Equal(CharacterSeeder.GetBaseSkills().Count, snapshot.BaseSkills.Count);
        Assert.Equal(CharacterSeeder.GetSpecialSkills().Count, snapshot.SpecialSkills.Count);

        Assert.True(snapshot.Attributes.ContainsKey(SD.Attributes.Strength));
        Assert.Equal(6, snapshot.Attributes[SD.Attributes.Strength].BaseBonus);

        var melee = snapshot.BaseSkills.First(s => s.Name == SD.BaseSkills.Melee);
        Assert.Equal(SD.Attributes.Strength, melee.RelatedAttribute1);
        Assert.Equal(characterId, melee.CharacterId);

        Assert.True(snapshot.SpecialSkills.ContainsKey("Heavy weapons"));
        Assert.Equal(characterId, snapshot.SpecialSkills["Heavy weapons"].CharacterId);
    }

    [Fact]
    public async Task CreateCharacter_PersistsProfessionTraitsAndUnarmedEquipmentSlot()
    {
        var characterId = await _helper.CreateCharacterAsync();

        var snapshot = await _helper.LoadCharacterSnapshotAsync(characterId);

        Assert.NotEmpty(snapshot.ProfessionTraits);
        Assert.Contains(snapshot.EquipmentSlots, s => s.Equipment.Name == SD.BasicWeaponsMelee.Unarmed);

        using var context = _fixture.CreateContext();
        var slotCount = await context.EquipmentSlots.CountAsync(s => s.CharacterID == characterId);
        Assert.True(slotCount >= 1);
    }

    [Fact]
    public async Task UpdateCharacter_AttributeChange_IsPersistedOnReload()
    {
        var characterId = await _helper.CreateCharacterAsync();

        await _helper.UpdateAttributeAsync(characterId, SD.Attributes.Strength, 8);

        var snapshot = await _helper.LoadCharacterSnapshotAsync(characterId);

        Assert.Equal(8, snapshot.Attributes[SD.Attributes.Strength].BaseBonus);
    }

    [Fact]
    public void AttributesModel_IncrAttr_DecrementsPointsAndIncrementsBonus()
    {
        var allParams = new AllParamsModel
        {
            Character = new()
            {
                IsApproved = false,
                AttributePoints = 100,
            },
        };
        allParams.Attributes.FillPropertiesContainer(CharacterSeeder.GetAttributes());

        var strength = allParams.Attributes.Get(SD.Attributes.Strength)!;
        var result = allParams.Attributes.IncrAttr(strength);

        Assert.Equal(string.Empty, result);
        Assert.Equal(7, strength.BaseBonus);
        Assert.Equal(99, allParams.Character.AttributePoints);
    }

    [Fact]
    public async Task RaceRepository_GetAllApproved_ReturnsMaterializedList_AfterAsyncRefactor()
    {
        await _helper.RaceRepository.Create(new RaceDTO
        {
            Name = "Approved Elf",
            Description = "Approved test race",
            RaceApproved = true,
        });

        var races = (await _helper.RaceRepository.GetAllApproved()).ToList();

        Assert.Contains(races, r => r.Name == "Approved Elf");
    }
}
