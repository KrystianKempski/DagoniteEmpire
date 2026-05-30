using DA_Business.Tests.Fixtures;
using DA_DataAccess.Scribe;
using DA_Scribe.Configuration;
using DA_Scribe.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DA_Business.Tests.Scribe;

/// <summary>
/// Verifies the per-user privacy guarantees on Scribe conversation APIs.
/// User decision: "GM nie powinien widzieć rozmów" - even GMs cannot read
/// or delete conversations owned by other users.
/// </summary>
public class ScribeConversationOwnershipTests : IClassFixture<ScribeInMemoryDatabaseFixture>
{
    private readonly ScribeInMemoryDatabaseFixture _fixture;
    private readonly ScribeService _service;

    public ScribeConversationOwnershipTests(ScribeInMemoryDatabaseFixture fixture)
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
    public async Task GetConversationAsync_ReturnsConversation_ForOwner()
    {
        var conv = await _service.CreateConversationAsync(userId: "user-owner-1", title: "mine");

        var loaded = await _service.GetConversationAsync(conv.Id, userId: "user-owner-1");

        Assert.NotNull(loaded);
        Assert.Equal(conv.Id, loaded!.Id);
        Assert.Equal("user-owner-1", loaded.UserId);
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsNull_ForDifferentUser()
    {
        var conv = await _service.CreateConversationAsync(userId: "user-owner-2", title: "private");

        var asOtherPlayer = await _service.GetConversationAsync(conv.Id, userId: "user-intruder");
        var asGameMaster  = await _service.GetConversationAsync(conv.Id, userId: "gm-account");

        Assert.Null(asOtherPlayer);
        Assert.Null(asGameMaster); // GM has no special read access
    }

    [Fact]
    public async Task DeleteConversationAsync_DoesNothing_ForNonOwner()
    {
        var conv = await _service.CreateConversationAsync(userId: "user-owner-3");

        await _service.DeleteConversationAsync(conv.Id, userId: "someone-else");

        await using var ctx = _fixture.CreateContext();
        Assert.NotNull(await ctx.ScribeConversations.FindAsync(conv.Id));
    }

    [Fact]
    public async Task DeleteConversationAsync_RemovesConversation_AndCascadesMessages_ForOwner()
    {
        const string user = "user-owner-4";
        var conv = await _service.CreateConversationAsync(userId: user);

        await using (var ctx = _fixture.CreateContext())
        {
            ctx.ScribeMessages.Add(new ScribeMessage
            {
                ConversationId = conv.Id,
                Role = "user",
                Content = "test",
                Timestamp = DateTime.UtcNow,
            });
            await ctx.SaveChangesAsync();
        }

        await _service.DeleteConversationAsync(conv.Id, userId: user);

        await using var verify = _fixture.CreateContext();
        Assert.Null(await verify.ScribeConversations.FindAsync(conv.Id));
        Assert.False(verify.ScribeMessages.Any(m => m.ConversationId == conv.Id));
    }

    [Fact]
    public async Task GetConversationsAsync_OnlyReturnsConversationsOwnedByCaller()
    {
        await _service.CreateConversationAsync(userId: "alice", title: "a1");
        await _service.CreateConversationAsync(userId: "alice", title: "a2");
        await _service.CreateConversationAsync(userId: "bob",   title: "b1");

        var aliceConvs = await _service.GetConversationsAsync("alice");
        var bobConvs   = await _service.GetConversationsAsync("bob");

        Assert.All(aliceConvs, c => Assert.Equal("alice", c.UserId));
        Assert.All(bobConvs,   c => Assert.Equal("bob",   c.UserId));
    }
}
