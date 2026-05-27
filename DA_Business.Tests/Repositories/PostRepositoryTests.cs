using DA_Business.Repository.ChatRepos;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;

namespace DA_Business.Tests.Repositories;

public class PostRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly PostRepository _repository;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();
    private static int _testChapterId;
    private static int _testCharacterId;

    public PostRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new PostRepository(_fixture.DbContextFactory, _fixture.Mapper);

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
        if (!context.Races.Any(r => r.Name == "PostTestRace"))
        {
            context.Races.Add(new Race
            {
                Name = "PostTestRace",
                Description = "Test race for posts",
                RaceApproved = true
            });
        }

        // Seed Profession
        if (!context.Professions.Any(p => p.Name == "PostTestProfession"))
        {
            context.Professions.Add(new Profession
            {
                Name = "PostTestProfession",
                Description = "Test profession for posts",
                RelatedAttributeName = "Strength",
                IsApproved = true
            });
        }

        context.SaveChanges();

        var race = context.Races.First(r => r.Name == "PostTestRace");
        var profession = context.Professions.First(p => p.Name == "PostTestProfession");

        // Seed Character
        if (!context.Characters.Any(c => c.NPCName == "PostTestCharacter"))
        {
            var character = new Character
            {
                NPCName = "PostTestCharacter",
                UserName = "postuser",
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
            _testCharacterId = context.Characters.First(c => c.NPCName == "PostTestCharacter").Id;
        }

        // Seed Campaign
        if (!context.Campaigns.Any(c => c.Name == "PostTestCampaign"))
        {
            context.Campaigns.Add(new Campaign
            {
                Name = "PostTestCampaign",
                Description = "Test campaign for posts",
                GameMaster = "TestGM",
                CreatedDate = DateTime.UtcNow
            });
        }

        context.SaveChanges();

        var campaign = context.Campaigns.First(c => c.Name == "PostTestCampaign");

        // Seed Chapter
        if (!context.Chapters.Any(c => c.Name == "PostTestChapter"))
        {
            var chapter = new Chapter
            {
                Name = "PostTestChapter",
                Description = "Test chapter for posts",
                DateNumber = 1,
                CampaignId = campaign.Id,
                CreatedDate = DateTime.UtcNow
            };
            context.Chapters.Add(chapter);
            context.SaveChanges();
            _testChapterId = chapter.Id;
        }
        else
        {
            _testChapterId = context.Chapters.First(c => c.Name == "PostTestChapter").Id;
        }
    }

    [Fact]
    public async Task Create_ShouldAddPost_AndReturnWithId()
    {
        // Arrange
        var postDto = new PostDTO
        {
            Content = "<p>This is a test post</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var result = await _repository.Create(postDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("<p>This is a test post</p>", result.Content);
        Assert.Equal(_testChapterId, result.ChapterId);
    }

    [Fact]
    public async Task GetById_ShouldReturnPost_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var post = new Post
        {
            Content = "<p>GetById test post</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("<p>GetById test post</p>", result.Content);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPostsForChapter()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var post1 = new Post
        {
            Content = "<p>GetAll post 1</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow
        };
        var post2 = new Post
        {
            Content = "<p>GetAll post 2</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow.AddMinutes(1)
        };
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll(_testChapterId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task Delete_ShouldRemovePost_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var post = new Post
        {
            Content = "<p>Post to delete</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        var postId = post.Id;

        // Act
        await _repository.Delete(postId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedPost = await verifyContext.Posts.FindAsync(postId);
        Assert.Null(deletedPost);
    }

    [Fact]
    public async Task Update_ShouldModifyPost()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var post = new Post
        {
            Content = "<p>Original content</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId,
            CreatedDate = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var updateDto = new PostDTO
        {
            Id = post.Id,
            Content = "<p>Updated content</p>",
            ChapterId = _testChapterId,
            CharacterId = _testCharacterId
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("<p>Updated content</p>", result.Content);
    }

    [Fact]
    public async Task GetPostCount_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create a unique chapter for this test
        var campaign = context.Campaigns.First(c => c.Name == "PostTestCampaign");
        var chapter = new Chapter
        {
            Name = "CountTestChapter",
            DateNumber = 100,
            CampaignId = campaign.Id,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Add exactly 3 posts
        for (int i = 0; i < 3; i++)
        {
            context.Posts.Add(new Post
            {
                Content = $"<p>Count test post {i}</p>",
                ChapterId = chapter.Id,
                CharacterId = _testCharacterId,
                CreatedDate = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPostCount(chapter.Id);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetPage_ShouldReturnCorrectPageOfPosts()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create a unique chapter for pagination test
        var campaign = context.Campaigns.First(c => c.Name == "PostTestCampaign");
        var chapter = new Chapter
        {
            Name = "PaginationTestChapter",
            DateNumber = 200,
            CampaignId = campaign.Id,
            CreatedDate = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        // Add 15 posts
        for (int i = 0; i < 15; i++)
        {
            context.Posts.Add(new Post
            {
                Content = $"<p>Pagination post {i}</p>",
                ChapterId = chapter.Id,
                CharacterId = _testCharacterId,
                CreatedDate = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        // Act - get page 1 with 10 posts per page
        var page1 = await _repository.GetPage(chapter.Id, 10, 1);
        var page2 = await _repository.GetPage(chapter.Id, 10, 2);

        // Assert
        Assert.Equal(10, page1.Count());
        Assert.Equal(5, page2.Count());
    }

    [Fact]
    public async Task GetCharacterPostCount_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        // Create a unique character for this test
        var race = context.Races.First(r => r.Name == "PostTestRace");
        var profession = context.Professions.First(p => p.Name == "PostTestProfession");
        var character = new Character
        {
            NPCName = "CharCountTest",
            UserName = "countuser",
            RaceId = race.Id,
            ProfessionId = profession.Id,
            IsApproved = true
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Add 5 posts for this character
        for (int i = 0; i < 5; i++)
        {
            context.Posts.Add(new Post
            {
                Content = $"<p>Character count post {i}</p>",
                ChapterId = _testChapterId,
                CharacterId = character.Id,
                CreatedDate = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCharacterPostCount(character.Id);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetCharacterLastPostDate_ShouldReturnMostRecentDate()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var race = context.Races.First(r => r.Name == "PostTestRace");
        var profession = context.Professions.First(p => p.Name == "PostTestProfession");
        var character = new Character
        {
            NPCName = "LastPostDateTest",
            UserName = "dateuser",
            RaceId = race.Id,
            ProfessionId = profession.Id,
            IsApproved = true
        };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        var oldDate = DateTime.UtcNow.AddDays(-5);
        var recentDate = DateTime.UtcNow;

        context.Posts.Add(new Post
        {
            Content = "<p>Old post</p>",
            ChapterId = _testChapterId,
            CharacterId = character.Id,
            CreatedDate = oldDate
        });
        context.Posts.Add(new Post
        {
            Content = "<p>Recent post</p>",
            ChapterId = _testChapterId,
            CharacterId = character.Id,
            CreatedDate = recentDate
        });
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCharacterLastPostDate(character.Id);

        // Assert
        Assert.True(result >= recentDate.AddSeconds(-1) && result <= recentDate.AddSeconds(1));
    }
}
