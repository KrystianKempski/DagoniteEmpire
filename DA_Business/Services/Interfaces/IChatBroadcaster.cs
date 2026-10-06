using DA_Models.ChatModels;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// In-process pub/sub for chat. One process today; swap for SignalR + a backplane if scaled out.
    /// Subscriptions are per account, so one subscription covers every thread the account takes part in.
    /// </summary>
    public interface IChatBroadcaster
    {
        /// <summary>
        /// Hand <paramref name="message"/> to every subscriber in <paramref name="recipientUserIds"/>.
        /// The caller resolves the recipients (see <see cref="ChatAccess.RecipientsAsync"/>) rather than
        /// the bus guessing them, so the in-app update and the push reach the same people.
        /// </summary>
        void Publish(ChatMessageDTO message, IEnumerable<string> recipientUserIds);

        /// <summary>Dispose the returned handle to unsubscribe.</summary>
        IDisposable Subscribe(string userId, Func<ChatMessageDTO, Task> handler);
    }
}
