using System.Collections.Concurrent;
using DA_Business.Services.Interfaces;
using DA_Models.ChatModels;

namespace DA_Business.Services
{
    public sealed class CampaignChatBroadcaster : ICampaignChatBroadcaster
    {
        private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();

        public void Publish(CampaignChatMessageDTO message)
        {
            foreach (var sub in _subscriptions.Values)
            {
                if (sub.CampaignId != message.CampaignId)
                    continue;

                var visible = message.RecipientCharacterId is null
                    || message.RecipientCharacterId == sub.CharacterId
                    || message.SenderCharacterId == sub.CharacterId;

                if (!visible)
                    continue;

                _ = SafeInvoke(sub.Handler, message);
            }
        }

        public IDisposable Subscribe(int campaignId, int characterId, Func<CampaignChatMessageDTO, Task> handler)
        {
            var id = Guid.NewGuid();
            var sub = new Subscription(id, campaignId, characterId, handler, this);
            _subscriptions[id] = sub;
            return sub;
        }

        private void Unsubscribe(Guid id) => _subscriptions.TryRemove(id, out _);

        private static async Task SafeInvoke(Func<CampaignChatMessageDTO, Task> handler, CampaignChatMessageDTO message)
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
            private readonly CampaignChatBroadcaster _owner;
            private int _disposed;

            public Subscription(
                Guid id,
                int campaignId,
                int characterId,
                Func<CampaignChatMessageDTO, Task> handler,
                CampaignChatBroadcaster owner)
            {
                Id = id;
                CampaignId = campaignId;
                CharacterId = characterId;
                Handler = handler;
                _owner = owner;
            }

            public Guid Id { get; }
            public int CampaignId { get; }
            public int CharacterId { get; }
            public Func<CampaignChatMessageDTO, Task> Handler { get; }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    _owner.Unsubscribe(Id);
            }
        }
    }
}
