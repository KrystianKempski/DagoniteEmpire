using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;

namespace DA_Business.Tests.Helpers;

/// <summary>
/// Regression tests for async repository reads used by EquipmentComponent (Gold display).
/// </summary>
public class WealthRecordRepositoryAsyncTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly WealthRecordRepository _repository;

    public WealthRecordRepositoryAsyncTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new WealthRecordRepository(_fixture.DbContextFactory, _fixture.Mapper);
        SeedLookups();
    }

    private void SeedLookups()
    {
        using var context = _fixture.CreateContext();
        if (!context.Races.Any())
        {
            context.Races.Add(new Race { Id = 1, Name = "Human", Description = "Test", RaceApproved = true });
        }
        if (!context.Professions.Any())
        {
            context.Professions.Add(new Profession
            {
                Id = 1,
                Name = "Warrior",
                Description = "Test",
                RelatedAttributeName = "Strength",
                IsApproved = true,
            });
        }
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ForCharacter_ReturnsOnlyMatchingRecords()
    {
        using (var context = _fixture.CreateContext())
        {
            context.Characters.AddRange(
                new Character { Id = 10, UserName = "u1", NPCName = "A", RaceId = 1, ProfessionId = 1 },
                new Character { Id = 11, UserName = "u2", NPCName = "B", RaceId = 1, ProfessionId = 1 });
            context.WealthRecords.AddRange(
                new WealthRecord { CharacterId = 10, Value = 1.5m, Description = "loot" },
                new WealthRecord { CharacterId = 10, Value = 2.5m, Description = "pay" },
                new WealthRecord { CharacterId = 11, Value = 9m, Description = "other" });
            await context.SaveChangesAsync();
        }

        var records = (await _repository.GetAll(10)).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal(4.0m, records.Sum(r => r.Value));
    }

    [Fact]
    public async Task GetAll_CompletesWithoutBlocking_WhenCalledWithAwait()
    {
        var records = await _repository.GetAll(999);

        Assert.NotNull(records);
        Assert.Empty(records);
    }
}
