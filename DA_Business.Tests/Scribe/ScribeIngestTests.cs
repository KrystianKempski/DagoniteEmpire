using DA_Business.Tests.Fixtures;
using DA_DataAccess.Scribe;
using DA_Scribe.Configuration;
using DA_Scribe.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DA_Business.Tests.Scribe;

public class ScribeIngestTests : IClassFixture<ScribeInMemoryDatabaseFixture>
{
    private readonly ScribeInMemoryDatabaseFixture _fixture;
    private readonly ScribeService _service;

    public ScribeIngestTests(ScribeInMemoryDatabaseFixture fixture)
    {
        _fixture = fixture;
        _service = new ScribeService(
            _fixture.DbContextFactory,
            new NoopEmbeddingService(),
            new NoopLLMService(),
            new NoopChunkService(),
            new NoopDocumentParserService(),
            NullLogger<ScribeService>.Instance,
            Options.Create(new ScribeOptions()));
    }

    [Fact]
    public async Task IngestContentAsync_PersistsMemoryAndChunks()
    {
        const int campaignId = 7;

        var memoryId = await _service.IngestContentAsync(
            title: "World lore: the Northern Reach",
            content: "Snow-bitten ridges shelter the last free clans.",
            type: MemoryType.World,
            campaignId: campaignId,
            characterIds: new[] { 1, 2 },
            isPublic: true);

        Assert.True(memoryId > 0);

        await using var ctx = _fixture.CreateContext();
        var saved = await ctx.ScribeMemories
            .Include(m => m.Chunks)
            .SingleAsync(m => m.Id == memoryId);

        Assert.Equal("World lore: the Northern Reach", saved.Title);
        Assert.Equal(MemoryType.World, saved.Type);
        Assert.Equal(campaignId, saved.SourceCampaignId);
        Assert.True(saved.IsPublic);
        Assert.Single(saved.Chunks);
        Assert.Equal(campaignId, saved.Chunks.First().CampaignId);
        Assert.True(saved.Chunks.First().IsPublic);
    }
}
