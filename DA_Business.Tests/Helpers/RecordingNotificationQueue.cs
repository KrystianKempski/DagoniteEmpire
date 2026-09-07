using DA_Business.Services.Interfaces;
using DA_Common.Notifications;

namespace DA_Business.Tests.Helpers;

/// <summary>
/// Captures the events gameplay code raises instead of delivering them, so tests can assert
/// what would have been pushed without touching a push service.
/// </summary>
public class RecordingNotificationQueue : IGameNotificationQueue
{
    public List<GameNotification> Raised { get; } = new();

    public void Enqueue(GameNotification notification) => Raised.Add(notification);

    public async IAsyncEnumerable<GameNotification> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var notification in Raised)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return notification;
        }

        await Task.CompletedTask;
    }
}
