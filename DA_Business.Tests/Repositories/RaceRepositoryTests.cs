using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;

namespace DA_Business.Tests.Repositories;

public class RaceRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly RaceRepository _repository;

    public RaceRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new RaceRepository(_fixture.DbContextFactory, _fixture.Mapper);
    }

    [Fact]
    public async Task Create_ShouldAddRace_AndReturnWithId()
    {
        // Arrange
        var raceDto = new RaceDTO
        {
            Name = "Elf",
            Description = "Graceful woodland race",
            RaceApproved = false,
            Index = 1
        };

        // Act
        var result = await _repository.Create(raceDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Elf", result.Name);
        Assert.Equal("Graceful woodland race", result.Description);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllRaces()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var race1 = new Race { Name = "Dwarf", Description = "Stout mountain folk", RaceApproved = true };
        var race2 = new Race { Name = "Halfling", Description = "Small but brave", RaceApproved = true };
        context.Races.AddRange(race1, race2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.True(list.Count >= 2);
        Assert.Contains(list, r => r.Name == "Dwarf");
        Assert.Contains(list, r => r.Name == "Halfling");
    }

    [Fact]
    public async Task GetById_ShouldReturnRace_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var race = new Race
        {
            Name = "Orc",
            Description = "Strong warriors",
            RaceApproved = true
        };
        context.Races.Add(race);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(race.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Orc", result.Name);
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
    public async Task Delete_ShouldRemoveRace_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var race = new Race
        {
            Name = "Goblin",
            Description = "Small green creatures",
            RaceApproved = false
        };
        context.Races.Add(race);
        await context.SaveChangesAsync();
        var id = race.Id;

        // Act
        await _repository.Delete(id);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deleted = await verifyContext.Races.FindAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetAllApproved_ShouldReturnOnlyApprovedRaces()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Races.AddRange(
            new Race { Name = "ApprovedRace1", RaceApproved = true },
            new Race { Name = "UnapprovedRace", RaceApproved = false },
            new Race { Name = "ApprovedRace2", RaceApproved = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllApproved();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.All(list, r => Assert.True(r.RaceApproved));
        Assert.DoesNotContain(list, r => r.Name == "UnapprovedRace");
    }
}
