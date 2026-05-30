using DA_Business.Tests.Fixtures;
using DA_DataAccess.Scribe;
using DA_Scribe.Configuration;
using DA_Scribe.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DA_Business.Tests.Scribe;

/// <summary>
/// Verifies the post-lifecycle helpers added so that editing or deleting a chat
/// post keeps the Scribe chunk index consistent. We exercise the removal path
/// directly (ingest itself is integration-tested via Ollama and out of scope here).
/// </summary>
public class ScribePostLifecycleTests : IClassFixture<ScribeInMemoryDatabaseFixture>
{
    private readonly ScribeInMemoryDatabaseFixture _fixture;
    private readonly ScribeService _service;

    public ScribePostLifecycleTests(ScribeInMemoryDatabaseFixture fixture)
    {
        _fixture = fixture;
        _service = new ScribeService(
            _fixture.DbContextFactory,
            new NoopEmbeddingService(),
            new NoopHttpClientFactory(),
            new NoopChunkService(),
            new NoopDocumentParserService(),
            NullLogger<ScribeService>.Instance,
            Options.Create(new ScribeOptions()));
    }

    [Fact]
    public async Task RemovePostAsync_IsNoOp_WhenPostNotIndexed()
    {
        await _service.RemovePostAsync(postId: 999_999);

        await using var ctx = _fixture.CreateContext();
        Assert.False(ctx.ScribeMemories.Any(m => m.SourcePostId == 999_999));
    }

    [Fact]
    public async Task RemovePostAsync_DeletesMemoryAndChunks_WhenPostIndexed()
    {
        const int postId = 4242;

        await using (var seed = _fixture.CreateContext())
        {
            var memory = new ScribeMemory
            {
                Title = "post #4242",
                Content = "snippet",
                Type = MemoryType.Post,
                SourcePostId = postId,
                CreatedAt = DateTime.UtcNow,
            };
            memory.Chunks.Add(new ScribeChunk
            {
                Content = "chunk text",
                ChunkIndex = 0,
                TokenCount = 2,
            });
            seed.ScribeMemories.Add(memory);
            await seed.SaveChangesAsync();
        }

        Assert.True(await _service.IsPostIndexedAsync(postId));

        await _service.RemovePostAsync(postId);

        await using var verify = _fixture.CreateContext();
        Assert.False(verify.ScribeMemories.Any(m => m.SourcePostId == postId));
        // ScribeMemory -> ScribeChunk uses cascade delete via the EF relationship
        Assert.False(verify.ScribeChunks.Any(c => c.Content == "chunk text"));
    }
}
