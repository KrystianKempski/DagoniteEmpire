using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;

namespace DA_Business.Tests.Repositories;

public class WoundRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly WoundRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();
    private static int _testCharacterId;

    public WoundRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new WoundRepository(_fixture.DbContextFactory, _fixture.Mapper);

        lock (_lock)
        {
            if (!_isInitialized)
            {
                SeedRequiredData();
                _isInitialized = true;
            }
        }
    }

    private void SeedRequiredData()
    {
        using var context = _fixture.CreateContext();

        // Seed Race
        if (!context.Races.Any(r => r.Name == "WoundTestRace"))
        {
            context.Races.Add(new Race
            {
                Name = "WoundTestRace",
                Description = "Test race for wounds",
                RaceApproved = true
            });
        }

        // Seed Profession
        if (!context.Professions.Any(p => p.Name == "WoundTestProfession"))
        {
            context.Professions.Add(new Profession
            {
                Name = "WoundTestProfession",
                Description = "Test profession for wounds",
                RelatedAttributeName = "Strength",
                IsApproved = true
            });
        }

        context.SaveChanges();

        var race = context.Races.First(r => r.Name == "WoundTestRace");
        var profession = context.Professions.First(p => p.Name == "WoundTestProfession");

        // Seed Character
        if (!context.Characters.Any(c => c.NPCName == "WoundTestCharacter"))
        {
            var character = new Character
            {
                NPCName = "WoundTestCharacter",
                UserName = "wounduser",
                RaceId = race.Id,
                ProfessionId = profession.Id,
                IsApproved = true
            };
            context.Characters.Add(character);
            context.SaveChanges();
            _testCharacterId = character.Id;
        }
        else
        {
            _testCharacterId = context.Characters.First(c => c.NPCName == "WoundTestCharacter").Id;
        }
    }

    [Fact]
    public async Task Create_ShouldAddWound_AndReturnWithId()
    {
        // Arrange
        var woundDto = new WoundDTO
        {
            Description = "Sword cut",
            Location = "Left Arm",
            Value = 2,
            IsIgnored = false,
            IsTended = false,
            IsMagicHealed = false,
            DateNumber = 1,
            IsCondition = false,
            CharacterId = _testCharacterId
        };

        // Act
        var result = await _repository.Create(woundDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Sword cut", result.Description);
        Assert.Equal("Left Arm", result.Location);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnWound_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var wound = new Wound
        {
            Description = "GetById Test Wound",
            Location = "Head",
            Value = 1,
            CharacterId = _testCharacterId,
            DateNumber = 5,
            HealTime = 7
        };
        context.Wounds.Add(wound);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(wound.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetById Test Wound", result.Description);
        Assert.Equal("Head", result.Location);
    }

    [Fact]
    public async Task GetAll_ShouldReturnWoundsForCharacter()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create unique character for this test
        var race = context.Races.First(r => r.Name == "WoundTestRace");
        var profession = context.Professions.First(p => p.Name == "WoundTestProfession");
        var character = new Character
        {
            NPCName = "WoundGetAllChar",
            UserName = "woundgetalluser",
            RaceId = race.Id,
            ProfessionId = profession.Id
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        var wound1 = new Wound
        {
            Description = "Wound 1",
            Location = "Chest",
            Value = 2,
            CharacterId = character.Id,
            IsCondition = false
        };
        var wound2 = new Wound
        {
            Description = "Wound 2",
            Location = "Right Leg",
            Value = 1,
            CharacterId = character.Id,
            IsCondition = false
        };
        context.Wounds.AddRange(wound1, wound2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll(character.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllCond_ShouldReturnOnlyConditions()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create unique character for this test
        var race = context.Races.First(r => r.Name == "WoundTestRace");
        var profession = context.Professions.First(p => p.Name == "WoundTestProfession");
        var character = new Character
        {
            NPCName = "ConditionTestChar",
            UserName = "conduser",
            RaceId = race.Id,
            ProfessionId = profession.Id
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Add wound (not condition)
        var wound = new Wound
        {
            Description = "Regular Wound",
            Location = "Back",
            Value = 2,
            CharacterId = character.Id,
            IsCondition = false
        };
        // Add condition
        var condition = new Wound
        {
            Description = "Fatigue",
            Location = "General",
            Value = 1,
            CharacterId = character.Id,
            IsCondition = true
        };
        context.Wounds.AddRange(wound, condition);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllCond(character.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Fatigue", result.First().Description);
    }

    [Fact]
    public async Task Delete_ShouldRemoveWound_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var wound = new Wound
        {
            Description = "Wound To Delete",
            Location = "Stomach",
            Value = 3,
            CharacterId = _testCharacterId
        };
        context.Wounds.Add(wound);
        await context.SaveChangesAsync();
        var woundId = wound.Id;

        // Act
        await _repository.Delete(woundId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedWound = await verifyContext.Wounds.FindAsync(woundId);
        Assert.Null(deletedWound);
    }

    [Fact]
    public async Task Update_ShouldModifyWound()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var wound = new Wound
        {
            Description = "Original Wound",
            Location = "Shoulder",
            Value = 1,
            IsTended = false,
            CharacterId = _testCharacterId
        };
        context.Wounds.Add(wound);
        await context.SaveChangesAsync();

        var updateDto = new WoundDTO
        {
            Id = wound.Id,
            Description = "Tended Wound",
            Location = "Shoulder",
            Value = 1,
            IsTended = true,
            CharacterId = _testCharacterId
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tended Wound", result.Description);
        Assert.True(result.IsTended);
    }
}
