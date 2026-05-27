using DA_Business.Repository.ChatRepos;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;

namespace DA_Business.Tests.Repositories;

public class BattlePhaseRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly BattlePhaseRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();
    private static int _testCampaignId;
    private static int _testChapterId;

    public BattlePhaseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new BattlePhaseRepository(_fixture.DbContextFactory, _fixture.Mapper);

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

        // Seed Campaign
        if (!context.Campaigns.Any(c => c.Name == "BattleTestCampaign"))
        {
            var campaign = new Campaign
            {
                Name = "BattleTestCampaign",
                Description = "Test campaign for battle phases",
                GameMaster = "TestGM",
                CreatedDate = DateTime.UtcNow
            };
            context.Campaigns.Add(campaign);
            context.SaveChanges();
            _testCampaignId = campaign.Id;
        }
        else
        {
            _testCampaignId = context.Campaigns.First(c => c.Name == "BattleTestCampaign").Id;
        }

        // Seed Chapter
        if (!context.Chapters.Any(c => c.Name == "BattleTestChapter"))
        {
            var chapter = new Chapter
            {
                Name = "BattleTestChapter",
                Description = "Test chapter for battle phases",
                DateNumber = 1,
                CampaignId = _testCampaignId,
                CreatedDate = DateTime.UtcNow
            };
            context.Chapters.Add(chapter);
            context.SaveChanges();
            _testChapterId = chapter.Id;
        }
        else
        {
            _testChapterId = context.Chapters.First(c => c.Name == "BattleTestChapter").Id;
        }
    }

    [Fact]
    public async Task Create_ShouldAddBattlePhase_AndReturnWithId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create unique chapter for this test
        var chapter = new Chapter
        {
            Name = "CreateBattleChapter",
            DateNumber = 50,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battlePhaseDto = new BattlePhaseDTO
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 1,
            BattleOngoing = true
        };

        // Act
        var result = await _repository.Create(battlePhaseDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(chapter.Id, result.ChapterId);
        Assert.True(result.BattleOngoing);
    }

    [Fact]
    public async Task GetById_ShouldReturnBattlePhase_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "GetByIdBattleChapter",
            DateNumber = 51,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battlePhase = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 3,
            BattleOngoing = true
        };
        context.BattlePhases.Add(battlePhase);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(battlePhase.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CurrentTurn);
        Assert.True(result.BattleOngoing);
    }

    [Fact]
    public async Task GetCurrentForChapter_ShouldReturnOngoingBattle()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "OngoingBattleChapter",
            DateNumber = 52,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battlePhase = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 5,
            BattleOngoing = true
        };
        context.BattlePhases.Add(battlePhase);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentForChapter(chapter.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.CurrentTurn);
        Assert.True(result.BattleOngoing);
    }

    [Fact]
    public async Task GetCurrentForChapter_ShouldReturnNull_WhenNoBattleOngoing()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "NoBattleChapter",
            DateNumber = 53,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Add a battle phase that is NOT ongoing
        var battlePhase = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 10,
            BattleOngoing = false
        };
        context.BattlePhases.Add(battlePhase);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentForChapter(chapter.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForChapter_ShouldReturnAllBattlePhasesInChapter()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "GetAllBattleChapter",
            DateNumber = 54,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battle1 = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            BattleOngoing = false
        };
        var battle2 = new BattlePhase
        {
            Name = 2,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            BattleOngoing = true
        };
        context.BattlePhases.AddRange(battle1, battle2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForChapter(chapter.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Delete_ShouldRemoveBattlePhase_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "DeleteBattleChapter",
            DateNumber = 55,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battlePhase = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId
        };
        context.BattlePhases.Add(battlePhase);
        await context.SaveChangesAsync();
        var battleId = battlePhase.Id;

        // Act
        await _repository.Delete(battleId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedBattle = await verifyContext.BattlePhases.FindAsync(battleId);
        Assert.Null(deletedBattle);
    }

    [Fact]
    public async Task Update_ShouldModifyBattlePhase()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var chapter = new Chapter
        {
            Name = "UpdateBattleChapter",
            DateNumber = 56,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var battlePhase = new BattlePhase
        {
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 1,
            BattleOngoing = true
        };
        context.BattlePhases.Add(battlePhase);
        await context.SaveChangesAsync();

        var updateDto = new BattlePhaseDTO
        {
            Id = battlePhase.Id,
            Name = 1,
            ChapterId = chapter.Id,
            CampaignId = _testCampaignId,
            CurrentTurn = 7,
            BattleOngoing = false
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.CurrentTurn);
        Assert.False(result.BattleOngoing);
    }
}
