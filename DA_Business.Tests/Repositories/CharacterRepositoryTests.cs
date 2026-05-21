using AutoMapper;
using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Tests.Repositories;

public class CharacterRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CharacterRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();

    public CharacterRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CharacterRepository(_fixture.DbContextFactory, _fixture.Mapper);
        
        // Only seed once across all tests
        lock (_lock)
        {
            if (!_isInitialized)
            {
                SeedRequiredData();
                _isInitialized = true;
            }
        }
    }

    /// <summary>
    /// Seeds required lookup data (Race, Profession) for Character foreign keys
    /// </summary>
    private void SeedRequiredData()
    {
        using var context = _fixture.CreateContext();
        
        // Add a race
        var race = new Race
        {
            Id = 1,
            Name = "Human",
            Description = "Test race",
            RaceApproved = true
        };
        context.Races.Add(race);

        // Add a profession (set IsApproved=true to prevent cascade delete)
        var profession = new Profession
        {
            Id = 1,
            Name = "Warrior",
            Description = "Test profession",
            RelatedAttributeName = "Strength",
            IsApproved = true
        };
        context.Professions.Add(profession);

        context.SaveChanges();
    }

    [Fact]
    public async Task Create_ShouldAddCharacter_AndReturnWithId()
    {
        // Arrange
        var characterDto = new CharacterDTO
        {
            UserName = "TestUser",
            NPCName = "Test Character",
            Description = "A test character",
            RaceId = 1,
            ProfessionId = 1,
            Age = 25,
            IsApproved = false
        };

        // Act
        var result = await _repository.Create(characterDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("TestUser", result.UserName);
        Assert.Equal("Test Character", result.NPCName);
    }

    [Fact]
    public async Task GetById_ShouldReturnCharacter_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            UserName = "GetByIdUser",
            NPCName = "GetById Character",
            Description = "Test",
            RaceId = 1,
            ProfessionId = 1
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(character.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetByIdUser", result.UserName);
        Assert.Equal("GetById Character", result.NPCName);
    }

    [Fact]
    public async Task GetById_ShouldReturnEmptyDTO_WhenNotExists()
    {
        // Act
        var result = await _repository.GetById(99999);

        // Assert - repository returns empty CharacterDTO, not null
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task GetAllForUser_ShouldReturnOnlyUserCharacters()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        context.Characters.AddRange(
            new Character { UserName = "User1", NPCName = "Char1", RaceId = 1, ProfessionId = 1 },
            new Character { UserName = "User1", NPCName = "Char2", RaceId = 1, ProfessionId = 1 },
            new Character { UserName = "User2", NPCName = "Char3", RaceId = 1, ProfessionId = 1 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForUser("User1");

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.Equal("User1", c.UserName));
    }

    [Fact]
    public async Task Delete_ShouldRemoveCharacterAndRelatedData()
    {
        // The repository Delete method cleans up related data (traits, equipment, etc.)
        // and removes the Character entity itself.
        
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            UserName = "DeleteUser",
            NPCName = "ToBeDeleted",
            RaceId = 1,
            ProfessionId = 1
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();
        var id = character.Id;

        // Act - should delete character and return count of changes
        var result = await _repository.Delete(id);

        // Assert - returns 1 when character is deleted (profession is approved so not deleted)
        Assert.True(result >= 1);
        
        // Verify the character was actually deleted
        using var verifyContext = _fixture.CreateContext();
        var deleted = await verifyContext.Characters.FindAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CheckIfCharacterBelongToUser_ShouldReturnTrue_WhenBelongs()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            UserName = "OwnerUser",
            NPCName = "OwnedChar",
            RaceId = 1,
            ProfessionId = 1
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfCharacterBelongToUser("OwnerUser", character.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckIfCharacterBelongToUser_ShouldReturnFalse_WhenNotBelongs()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            UserName = "ActualOwner",
            NPCName = "SomeChar",
            RaceId = 1,
            ProfessionId = 1
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfCharacterBelongToUser("DifferentUser", character.Id);

        // Assert
        Assert.False(result);
    }
}
