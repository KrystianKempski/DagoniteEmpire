using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.CharacterModels;

namespace DA_Business.Tests.Repositories;

public class MobRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly MobRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();
    private static int _testCampaignId;
    private static int _testChapterId;

    public MobRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new MobRepository(_fixture.DbContextFactory, _fixture.Mapper);

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
        if (!context.Campaigns.Any(c => c.Name == "MobTestCampaign"))
        {
            var campaign = new Campaign
            {
                Name = "MobTestCampaign",
                Description = "Test campaign for mobs",
                GameMaster = "TestGM",
                CreatedDate = DateTime.UtcNow
            };
            context.Campaigns.Add(campaign);
            context.SaveChanges();
            _testCampaignId = campaign.Id;
        }
        else
        {
            _testCampaignId = context.Campaigns.First(c => c.Name == "MobTestCampaign").Id;
        }

        // Seed Chapter
        if (!context.Chapters.Any(c => c.Name == "MobTestChapter"))
        {
            var chapter = new Chapter
            {
                Name = "MobTestChapter",
                Description = "Test chapter for mobs",
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
            _testChapterId = context.Chapters.First(c => c.Name == "MobTestChapter").Id;
        }
    }

    [Fact]
    public async Task Create_ShouldAddMob_AndReturnWithId()
    {
        // Arrange
        var mobDto = new MobDTO
        {
            Name = "Test Goblin",
            Description = "A small green creature",
            CampaignId = _testCampaignId,
            ChapterId = _testChapterId,
            AttackSkillValue = 10,
            DodgeSkillValue = 5,
            MaxWounds = 3,
            CurrentWounds = 3,
            IsApproved = true
        };

        // Act
        var result = await _repository.Create(mobDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Goblin", result.Name);
        Assert.Equal(_testCampaignId, result.CampaignId);
    }

    [Fact]
    public async Task GetById_ShouldReturnMob_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mob = new Mob
        {
            Name = "GetById Test Mob",
            Description = "Test mob",
            CampaignId = _testCampaignId,
            ChapterId = _testChapterId,
            MaxWounds = 5,
            CurrentWounds = 5
        };
        context.Mobs.Add(mob);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(mob.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetById Test Mob", result.Name);
    }

    [Fact]
    public async Task GetAllForChapter_ShouldReturnMobsInChapter()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Create unique chapter for this test
        var chapter = new Chapter
        {
            Name = "MobChapterTest",
            DateNumber = 10,
            CampaignId = _testCampaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var mob1 = new Mob { Name = "Chapter Mob 1", CampaignId = _testCampaignId, ChapterId = chapter.Id };
        var mob2 = new Mob { Name = "Chapter Mob 2", CampaignId = _testCampaignId, ChapterId = chapter.Id };
        context.Mobs.AddRange(mob1, mob2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForChapter(chapter.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllForCampaign_ShouldReturnMobsInCampaign()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Create unique campaign for this test
        var campaign = new Campaign
        {
            Name = "UniqueMobCampaign",
            GameMaster = "GM",
            CreatedDate = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var chapter = new Chapter
        {
            Name = "CampaignMobChapter",
            DateNumber = 20,
            CampaignId = campaign.Id,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var mob1 = new Mob { Name = "Campaign Mob 1", CampaignId = campaign.Id, ChapterId = chapter.Id };
        var mob2 = new Mob { Name = "Campaign Mob 2", CampaignId = campaign.Id, ChapterId = chapter.Id };
        context.Mobs.AddRange(mob1, mob2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForCampaing(campaign.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Delete_ShouldRemoveMob_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mob = new Mob
        {
            Name = "Mob To Delete",
            CampaignId = _testCampaignId,
            ChapterId = _testChapterId
        };
        context.Mobs.Add(mob);
        await context.SaveChangesAsync();
        var mobId = mob.Id;

        // Act
        await _repository.Delete(mobId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedMob = await verifyContext.Mobs.FindAsync(mobId);
        Assert.Null(deletedMob);
    }

    [Fact]
    public async Task Update_ShouldModifyMob()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mob = new Mob
        {
            Name = "Original Mob Name",
            Description = "Original",
            CampaignId = _testCampaignId,
            ChapterId = _testChapterId,
            MaxWounds = 3,
            CurrentWounds = 3
        };
        context.Mobs.Add(mob);
        await context.SaveChangesAsync();

        var updateDto = new MobDTO
        {
            Id = mob.Id,
            Name = "Updated Mob Name",
            Description = "Updated Description",
            CampaignId = _testCampaignId,
            ChapterId = _testChapterId,
            MaxWounds = 5,
            CurrentWounds = 2
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Mob Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(5, result.MaxWounds);
        Assert.Equal(2, result.CurrentWounds);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllMobs()
    {
        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Any());
    }
}
