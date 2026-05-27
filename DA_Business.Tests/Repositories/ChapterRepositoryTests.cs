using DA_Business.Repository.ChatRepos;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;

namespace DA_Business.Tests.Repositories;

public class ChapterRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ChapterRepository _repository;
    private readonly CampaignRepository _campaignRepository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();

    public ChapterRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ChapterRepository(_fixture.DbContextFactory, _fixture.Mapper);
        _campaignRepository = new CampaignRepository(_fixture.DbContextFactory, _fixture.Mapper);

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

        // Seed Race and Profession for Characters
        if (!context.Races.Any(r => r.Name == "ChapterTestRace"))
        {
            context.Races.Add(new Race
            {
                Name = "ChapterTestRace",
                Description = "Test race for chapters",
                RaceApproved = true
            });
        }

        if (!context.Professions.Any(p => p.Name == "ChapterTestProfession"))
        {
            context.Professions.Add(new Profession
            {
                Name = "ChapterTestProfession",
                Description = "Test profession for chapters",
                RelatedAttributeName = "Strength",
                IsApproved = true
            });
        }

        // Seed a Campaign for chapter tests
        if (!context.Campaigns.Any(c => c.Name == "ChapterTestCampaign"))
        {
            context.Campaigns.Add(new Campaign
            {
                Name = "ChapterTestCampaign",
                Description = "Test campaign for chapters",
                GameMaster = "TestGM",
                CreatedDate = DateTime.UtcNow,
                IsFinished = false
            });
        }

        context.SaveChanges();
    }

    private int GetTestCampaignId()
    {
        using var context = _fixture.CreateContext();
        return context.Campaigns.First(c => c.Name == "ChapterTestCampaign").Id;
    }

    private (int raceId, int professionId) GetTestIds()
    {
        using var context = _fixture.CreateContext();
        var race = context.Races.First(r => r.Name == "ChapterTestRace");
        var profession = context.Professions.First(p => p.Name == "ChapterTestProfession");
        return (race.Id, profession.Id);
    }

    [Fact]
    public async Task Create_ShouldAddChapter_AndReturnWithId()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        var chapterDto = new ChapterDTO
        {
            Name = "The Beginning",
            Description = "First chapter of the adventure",
            DateNumber = 1,
            DayTime = "Morning",
            Place = "Tavern",
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow,
            IsFinished = false
        };

        // Act
        var result = await _repository.Create(chapterDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("The Beginning", result.Name);
        Assert.Equal(campaignId, result.CampaignId);
    }

    [Fact]
    public async Task GetById_ShouldReturnChapter_WhenExists()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        using var context = _fixture.CreateContext();
        var chapter = new Chapter
        {
            Name = "GetById Test Chapter",
            Description = "Test",
            DateNumber = 2,
            DayTime = "Afternoon",
            Place = "Forest",
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(chapter.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetById Test Chapter", result.Name);
        Assert.Equal(campaignId, result.CampaignId);
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
    public async Task GetAll_ShouldReturnAllChapters()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        using var context = _fixture.CreateContext();
        var chapter1 = new Chapter
        {
            Name = "GetAll Chapter 1",
            Description = "Test 1",
            DateNumber = 3,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        var chapter2 = new Chapter
        {
            Name = "GetAll Chapter 2",
            Description = "Test 2",
            DateNumber = 4,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.AddRange(chapter1, chapter2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll(campaignId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task Delete_ShouldRemoveChapter_WhenExists()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        using var context = _fixture.CreateContext();
        var chapter = new Chapter
        {
            Name = "Chapter To Delete",
            Description = "Will be deleted",
            DateNumber = 5,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();
        var chapterId = chapter.Id;

        // Act
        await _repository.Delete(chapterId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedChapter = await verifyContext.Chapters.FindAsync(chapterId);
        Assert.Null(deletedChapter);
    }

    [Fact]
    public async Task Update_ShouldModifyChapter()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        using var context = _fixture.CreateContext();
        var chapter = new Chapter
        {
            Name = "Original Name",
            Description = "Original",
            DateNumber = 6,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var updateDto = new ChapterDTO
        {
            Id = chapter.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            DateNumber = 6,
            CampaignId = campaignId
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task CheckIfChapterBelongToUser_ShouldReturnTrue_WhenUserHasCharacterInChapter()
    {
        // Arrange
        var (raceId, professionId) = GetTestIds();
        var campaignId = GetTestCampaignId();

        using var context = _fixture.CreateContext();

        var character = new Character
        {
            NPCName = "ChapterTestHero",
            UserName = "chapteruser",
            RaceId = raceId,
            ProfessionId = professionId,
            IsApproved = true
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        var chapter = new Chapter
        {
            Name = "User Belongs Chapter",
            Description = "Test",
            DateNumber = 7,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow,
            Characters = new List<Character> { character }
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfChapterBelongToUser("chapteruser", chapter.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckIfChapterBelongToUser_ShouldReturnFalse_WhenUserHasNoCharacterInChapter()
    {
        // Arrange
        var campaignId = GetTestCampaignId();
        using var context = _fixture.CreateContext();

        var chapter = new Chapter
        {
            Name = "No User Chapter",
            Description = "Test",
            DateNumber = 8,
            CampaignId = campaignId,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfChapterBelongToUser("nonexistentuser", chapter.Id);

        // Assert
        Assert.False(result);
    }
}
