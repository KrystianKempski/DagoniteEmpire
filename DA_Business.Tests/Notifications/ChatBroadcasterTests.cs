using DA_Business.Services;
using DA_Models.ChatModels;

namespace DA_Business.Tests.Notifications;

public class ChatBroadcasterTests
{
    [Fact]
    public void Publish_ReachesOnlyTheListedRecipients()
    {
        var bus = new ChatBroadcaster();
        ChatMessageDTO? forBob = null;
        ChatMessageDTO? forCarol = null;

        using var bobsSub = bus.Subscribe("id-bob", msg => { forBob = msg; return Task.CompletedTask; });
        using var carolsSub = bus.Subscribe("id-carol", msg => { forCarol = msg; return Task.CompletedTask; });

        bus.Publish(
            new ChatMessageDTO { Id = 1, ConversationId = 7, SenderUserId = "id-alice" },
            new[] { "id-bob" });

        Assert.NotNull(forBob);
        Assert.Equal(7, forBob!.ConversationId);
        Assert.Null(forCarol);
    }

    [Fact]
    public void Publish_AlwaysReachesTheAuthorsOwnCircuits()
    {
        var bus = new ChatBroadcaster();
        ChatMessageDTO? mirrored = null;

        // A second tab of the author's account must show the message it did not type.
        using var sub = bus.Subscribe("id-alice", msg => { mirrored = msg; return Task.CompletedTask; });

        bus.Publish(
            new ChatMessageDTO { Id = 2, ConversationId = 7, SenderUserId = "id-alice" },
            new[] { "id-bob" });

        Assert.NotNull(mirrored);
    }

    [Fact]
    public void Dispose_StopsDelivery()
    {
        var bus = new ChatBroadcaster();
        var received = 0;

        var sub = bus.Subscribe("id-bob", _ => { received++; return Task.CompletedTask; });
        sub.Dispose();

        bus.Publish(
            new ChatMessageDTO { Id = 3, ConversationId = 7, SenderUserId = "id-alice" },
            new[] { "id-bob" });

        Assert.Equal(0, received);
    }
}
