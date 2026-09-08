using DA_Common.Notifications;
using WebPush;

namespace DA_Business.Tests.Notifications;

public class WebPushConfigTests
{
    [Theory]
    [InlineData("", "", "", false)]
    [InlineData("pub", "priv", "", false)]
    [InlineData("SET_IN_USER_SECRETS_OR_ENV", "priv", "mailto:a@b.c", false)]
    [InlineData("pub", "SET_IN_USER_SECRETS_OR_ENV", "mailto:a@b.c", false)]
    [InlineData("pub", "priv", "mailto:a@b.c", true)]
    public void IsConfigured_RejectsBlanksAndPlaceholders(
        string publicKey, string privateKey, string subject, bool expected)
    {
        var options = new WebPushOptions
        {
            PublicKey = publicKey,
            PrivateKey = privateKey,
            Subject = subject,
        };

        Assert.Equal(expected, options.IsConfigured);
    }

    [Fact]
    public void GeneratedVapidKeys_AreAcceptedByWebPush()
    {
        var keys = VapidHelper.GenerateVapidKeys();
        var options = new WebPushOptions
        {
            PublicKey = keys.PublicKey,
            PrivateKey = keys.PrivateKey,
            Subject = "mailto:admin@example.com",
        };

        Assert.True(options.IsConfigured);

        // Throws when the subject or either key has the wrong shape.
        VapidHelper.GetVapidHeaders(
            "https://push.example.com",
            options.Subject,
            options.PublicKey,
            options.PrivateKey);
    }

    [Theory]
    [InlineData("posts", NotificationTopic.Posts)]
    [InlineData("  TURN-RESOLVED  ", NotificationTopic.TurnResolved)]
    [InlineData("gm-question", NotificationTopic.GmQuestion)]
    [InlineData("nope", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Normalize_MapsKnownTopicsOnly(string? input, string? expected)
    {
        Assert.Equal(expected, NotificationTopic.Normalize(input));
    }

    [Fact]
    public void ForAccount_HidesBaronyTopicsForHeroes()
    {
        var hero = NotificationTopic.ForAccount(includeBarony: false);
        Assert.Equal(new[] { NotificationTopic.Posts, NotificationTopic.Chat }, hero);
        Assert.DoesNotContain(NotificationTopic.BaronLetter, hero);
        Assert.DoesNotContain(NotificationTopic.BattleTurn, hero);
    }

    [Fact]
    public void ForAccount_KeepsBaronyTopicsForDukesAndGm()
    {
        Assert.Equal(NotificationTopic.All, NotificationTopic.ForAccount(includeBarony: true));
    }
}
