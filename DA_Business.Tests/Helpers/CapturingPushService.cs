using DA_Business.Services.Interfaces;
using DA_Common.Notifications;
using DA_Models.NotificationModels;

namespace DA_Business.Tests.Helpers;

/// <summary>Stands in for the real push service and records what would have gone out.</summary>
public sealed class CapturingPushService : IPushNotificationService
{
    public record Send(List<string> UserIds, PushNotificationDTO Payload, string? Topic);

    public List<Send> Sent { get; } = new();
    public bool Configured { get; set; } = true;

    public bool IsConfigured => Configured;
    public string PublicKey => "test-key";

    public Task SaveSubscription(string userId, WebPushSubscriptionDTO subscription, string? userAgent) =>
        Task.CompletedTask;

    public Task RemoveSubscription(string endpoint) => Task.CompletedTask;

    public Task<IReadOnlyList<string>?> GetTopics(string userId, string endpoint) =>
        Task.FromResult<IReadOnlyList<string>?>(NotificationTopic.All);

    public Task<bool> SaveTopics(string userId, string endpoint, IEnumerable<string>? topics) =>
        Task.FromResult(true);

    public Task<int> CountSubscriptions(string userId) => Task.FromResult(0);

    public Task<int> SendToUser(string userId, PushNotificationDTO payload, string? topic = null) =>
        SendToUsers(new[] { userId }, payload, topic);

    public Task<int> SendToUsers(
        IEnumerable<string> userIds,
        PushNotificationDTO payload,
        string? topic = null)
    {
        var ids = userIds.ToList();
        Sent.Add(new Send(ids, payload, topic));
        return Task.FromResult(ids.Count);
    }
}
