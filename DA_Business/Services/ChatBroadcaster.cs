using System.Collections.Concurrent;
using DA_Business.Services.Interfaces;
using DA_Models.ChatModels;

namespace DA_Business.Services
{
    /// <inheritdoc cref="IChatBroadcaster"/>
    public sealed class ChatBroadcaster : IChatBroadcaster
    {
        private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();

        public void Publish(ChatMessageDTO message, IEnumerable<string> recipientUserIds)
        {
            var recipients = new HashSet<string>(
                (recipientUserIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.Ordinal);

            // The author's own circuits need the message too, to keep several open tabs in step.
            if (!string.IsNullOrWhiteSpace(message.SenderUserId))
                recipients.Add(message.SenderUserId);

            if (recipients.Count == 0)
                return;

            foreach (var sub in _subscriptions.Values)
            {
                if (!recipients.Contains(sub.UserId))
                    continue;

                _ = SafeInvoke(sub.Handler, message);
            }
        }

        public IDisposable Subscribe(string userId, Func<ChatMessageDTO, Task> handler)
        {
            var id = Guid.NewGuid();
            var sub = new Subscription(id, userId ?? string.Empty, handler, this);
            _subscriptions[id] = sub;
            return sub;
        }

        private void Unsubscribe(Guid id) => _subscriptions.TryRemove(id, out _);

        private static async Task SafeInvoke(Func<ChatMessageDTO, Task> handler, ChatMessageDTO message)
        {
            try
            {
                await handler(message);
            }
            catch
            {
                // Circuit may be gone; ignore so one dead subscriber does not break others.
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly ChatBroadcaster _owner;
            private int _disposed;

            public Subscription(
                Guid id,
                string userId,
                Func<ChatMessageDTO, Task> handler,
                ChatBroadcaster owner)
            {
                Id = id;
                UserId = userId;
                Handler = handler;
                _owner = owner;
            }

            public Guid Id { get; }
            public string UserId { get; }
            public Func<ChatMessageDTO, Task> Handler { get; }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    _owner.Unsubscribe(Id);
            }
        }
    }
}
