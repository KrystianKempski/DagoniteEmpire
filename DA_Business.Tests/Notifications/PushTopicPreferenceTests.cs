using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Common.Notifications;
using DA_Models.NotificationModels;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DA_Business.Tests.Notifications;

public class PushTopicPreferenceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly PushNotificationService _service;

    public PushTopicPreferenceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _service = new PushNotificationService(
            _fixture.DbContextFactory,
            Options.Create(new WebPushOptions
            {
                PublicKey = "test-public",
                PrivateKey = "test-private",
                Subject = "mailto:test@example.com",
            }),
            NullLogger<PushNotificationService>.Instance);
    }

    [Fact]
    public async Task SaveAndGetTopics_RoundTrip()
    {
        await _service.SaveSubscription("user-1", SampleSubscription("https://push/a"), "test");

        Assert.True(await _service.SaveTopics("user-1", "https://push/a", new[]
        {
            NotificationTopic.Chat,
            NotificationTopic.BaronLetter,
            "garbage",
        }));

        var topics = await _service.GetTopics("user-1", "https://push/a");
        Assert.Equal(new[] { NotificationTopic.Chat, NotificationTopic.BaronLetter }, topics);
    }

    [Fact]
    public async Task GetTopics_UnknownEndpoint_ReturnsNull()
    {
        Assert.Null(await _service.GetTopics("user-1", "https://push/missing"));
    }

    [Fact]
    public async Task SaveTopics_RejectsSomebodyElsesDevice()
    {
        await _service.SaveSubscription("owner", SampleSubscription("https://push/shared"), null);

        Assert.False(await _service.SaveTopics("intruder", "https://push/shared", new[] { NotificationTopic.Chat }));
        var topics = await _service.GetTopics("owner", "https://push/shared");
        Assert.Equal(NotificationTopic.All, topics);
    }

    [Fact]
    public async Task FreshSubscription_WithoutTopics_ReceivesEverything()
    {
        await _service.SaveSubscription("user-1", SampleSubscription("https://push/fresh"), null);

        using var ctx = _fixture.CreateContext();
        var row = Assert.Single(ctx.WebPushSubscriptions);
        Assert.Null(row.TopicsJson);
        Assert.Equal(NotificationTopic.All, await _service.GetTopics("user-1", "https://push/fresh"));
    }

    [Fact]
    public async Task Subscribe_WithExplicitTopics_PersistsThem()
    {
        var dto = SampleSubscription("https://push/chosen");
        dto.Topics = new List<string> { NotificationTopic.Posts, NotificationTopic.TurnResolved };

        await _service.SaveSubscription("user-1", dto, null);

        Assert.Equal(
            new[] { NotificationTopic.Posts, NotificationTopic.TurnResolved },
            await _service.GetTopics("user-1", "https://push/chosen"));
    }

    [Fact]
    public async Task EmptyTopicList_MeansMuteEverything()
    {
        await _service.SaveSubscription("user-1", SampleSubscription("https://push/mute"), null);
        Assert.True(await _service.SaveTopics("user-1", "https://push/mute", Array.Empty<string>()));

        Assert.Empty(await _service.GetTopics("user-1", "https://push/mute")!);
    }

    private static WebPushSubscriptionDTO SampleSubscription(string endpoint) => new()
    {
        Endpoint = endpoint,
        P256dh = "p256dh-key",
        Auth = "auth-secret",
    };
}
