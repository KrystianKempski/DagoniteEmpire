using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;

namespace DA_Business.Tests.Repositories;

public class ProfessionRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProfessionRepository _repository;

    public ProfessionRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProfessionRepository(_fixture.DbContextFactory, _fixture.Mapper);
    }

    [Fact]
    public async Task Create_ShouldAddProfession_AndReturnWithId()
    {
        // Arrange
        var professionDto = new ProfessionDTO
        {
            Name = "Mage",
            Description = "Master of arcane arts",
            RelatedAttributeName = "Intelligence",
            IsApproved = false,
            IsUniversal = false
        };

        // Act
        var result = await _repository.Create(professionDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Mage", result.Name);
        Assert.Equal("Master of arcane arts", result.Description);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllProfessions()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Professions.AddRange(
            new Profession { Name = "Ranger", Description = "Wilderness expert", RelatedAttributeName = "Dexterity", IsApproved = true },
            new Profession { Name = "Paladin", Description = "Holy warrior", RelatedAttributeName = "Charisma", IsApproved = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.True(list.Count >= 2);
        Assert.Contains(list, p => p.Name == "Ranger");
        Assert.Contains(list, p => p.Name == "Paladin");
    }

    [Fact]
    public async Task GetAllApproved_ShouldReturnOnlyApprovedProfessions()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Professions.AddRange(
            new Profession { Name = "ApprovedClass1", RelatedAttributeName = "Str", IsApproved = true },
            new Profession { Name = "UnapprovedClass", RelatedAttributeName = "Str", IsApproved = false },
            new Profession { Name = "ApprovedClass2", RelatedAttributeName = "Str", IsApproved = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllApproved();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.All(list, p => Assert.True(p.IsApproved));
        Assert.DoesNotContain(list, p => p.Name == "UnapprovedClass");
    }

    [Fact]
    public async Task GetById_ShouldReturnProfession_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var profession = new Profession
        {
            Name = "Cleric",
            Description = "Divine spellcaster",
            RelatedAttributeName = "Wisdom",
            IsApproved = true
        };
        context.Professions.Add(profession);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(profession.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cleric", result.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnEmptyDTO_WhenNotExists()
    {
        // Act
        var result = await _repository.GetById(99999);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProfession_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var profession = new Profession
        {
            Name = "Bard",
            Description = "Musical performer",
            RelatedAttributeName = "Charisma",
            IsApproved = false
        };
        context.Professions.Add(profession);
        await context.SaveChangesAsync();
        var id = profession.Id;

        // Act
        await _repository.Delete(id);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deleted = await verifyContext.Professions.FindAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_ShouldNotThrow_WhenProfessionNotExists()
    {
        // Act & Assert - should complete without exception
        var result = await _repository.Delete(99999);
        Assert.Equal(0, result);
    }
}
