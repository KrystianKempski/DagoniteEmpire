using DA_Business.Repository.ChatRepos;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;

namespace DA_Business.Tests.Repositories;

public class CampaignRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CampaignRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();

    public CampaignRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CampaignRepository(_fixture.DbContextFactory, _fixture.Mapper);
        
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
    /// Seeds required lookup data (Race, Profession) for Character foreign keys used in campaign tests
    /// </summary>
    private void SeedRequiredData()
    {
        using var context = _fixture.CreateContext();
        
        // Check if we already have required data (from other test classes)
        if (!context.Races.Any(r => r.Name == "CampaignTestRace"))
        {
            var race = new Race
            {
                Name = "CampaignTestRace",
                Description = "Test race for campaigns",
                RaceApproved = true
            };
            context.Races.Add(race);
        }

        if (!context.Professions.Any(p => p.Name == "CampaignTestProfession"))
        {
            var profession = new Profession
            {
                Name = "CampaignTestProfession",
                Description = "Test profession for campaigns",
                RelatedAttributeName = "Strength",
                IsApproved = true
            };
            context.Professions.Add(profession);
        }

        context.SaveChanges();
    }

    private (int raceId, int professionId) GetTestIds()
    {
        using var context = _fixture.CreateContext();
        var race = context.Races.First(r => r.Name == "CampaignTestRace");
        var profession = context.Professions.First(p => p.Name == "CampaignTestProfession");
        return (race.Id, profession.Id);
    }

    [Fact]
    public async Task Create_ShouldAddCampaign_AndReturnWithId()
    {
        // Arrange
        var campaignDto = new CampaignDTO
        {
            Name = "The Lost Mines",
            Description = "An adventure into the depths",
            GameMaster = "TestGM",
            CreatedDate = DateTime.UtcNow,
            IsFinished = false
        };

        // Act
        var result = await _repository.Create(campaignDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("The Lost Mines", result.Name);
        Assert.Equal("TestGM", result.GameMaster);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllCampaigns()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        context.Campaigns.AddRange(
            new Campaign { Name = "Campaign1", Description = "Desc1", GameMaster = "GM1", CreatedDate = DateTime.UtcNow },
            new Campaign { Name = "Campaign2", Description = "Desc2", GameMaster = "GM2", CreatedDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.True(list.Count >= 2);
        Assert.Contains(list, c => c.Name == "Campaign1");
        Assert.Contains(list, c => c.Name == "Campaign2");
    }

    [Fact]
    public async Task GetById_ShouldReturnCampaign_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var campaign = new Campaign
        {
            Name = "GetByIdCampaign",
            Description = "Test campaign",
            GameMaster = "TestGM",
            CreatedDate = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(campaign.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetByIdCampaign", result.Name);
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
    public async Task Delete_ShouldRemoveCampaign_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var campaign = new Campaign
        {
            Name = "ToDeleteCampaign",
            Description = "Will be deleted",
            GameMaster = "GM",
            CreatedDate = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();
        var id = campaign.Id;

        // Act
        await _repository.Delete(id);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deleted = await verifyContext.Campaigns.FindAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CheckIfCampaignBelongToUser_ShouldReturnTrue_WhenUserHasCharacterInCampaign()
    {
        // Arrange
        var (raceId, professionId) = GetTestIds();
        
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            UserName = "CampaignOwner",
            NPCName = "CampaignChar",
            RaceId = raceId,
            ProfessionId = professionId
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        var campaign = new Campaign
        {
            Name = "UserCampaign",
            Description = "Test",
            GameMaster = "GM",
            CreatedDate = DateTime.UtcNow,
            Characters = new List<Character> { character }
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfCampaignBelongToUser("CampaignOwner", campaign.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckIfCampaignBelongToUser_ShouldReturnFalse_WhenUserHasNoCharacterInCampaign()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var campaign = new Campaign
        {
            Name = "OtherUserCampaign",
            Description = "Test",
            GameMaster = "GM",
            CreatedDate = DateTime.UtcNow,
            Characters = new List<Character>()
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfCampaignBelongToUser("RandomUser", campaign.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckIfCampaignBelongToUser_ShouldReturnFalse_WhenCampaignNotExists()
    {
        // Act
        var result = await _repository.CheckIfCampaignBelongToUser("AnyUser", 99999);

        // Assert
        Assert.False(result);
    }
}
