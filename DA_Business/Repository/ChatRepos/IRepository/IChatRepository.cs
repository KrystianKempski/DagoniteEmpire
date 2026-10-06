using DA_Models.ChatModels;

namespace DA_Business.Repository.ChatRepos.IRepository
{
    /// <summary>
    /// Account-to-account chat. Every method identifies the caller by Identity id only — chat has no
    /// notion of characters or of a "currently selected" anything, which is what keeps the unread badge
    /// and push notifications working for players who hold heroes in several campaigns.
    /// </summary>
    public interface IChatRepository
    {
        /// <summary>
        /// Everything the chat list shows: party channels for the caller's campaigns, direct threads,
        /// and peers that are reachable but have no thread yet.
        /// </summary>
        Task<IReadOnlyList<ChatContactDTO>> GetContactsAsync(string userId);

        Task<IReadOnlyList<ChatMessageDTO>> GetThreadAsync(
            long conversationId,
            string userId,
            int take = 50,
            long? beforeId = null);

        Task<ChatConversationInfoDTO?> GetConversationInfoAsync(long conversationId, string userId);

        /// <summary>
        /// Thread with another account, or with the Game Master role when <paramref name="peerIsGameMaster"/>
        /// is set. Created on first use; calling it twice returns the same thread.
        /// </summary>
        Task<long> GetOrCreateDirectAsync(string userId, string? peerUserId, bool peerIsGameMaster);

        Task<long> GetOrCreatePartyAsync(string userId, int campaignId);

        Task<ChatMessageDTO> SendAsync(long conversationId, string senderUserId, string content);

        Task MarkReadAsync(long conversationId, string userId);

        /// <summary>Unread messages across every thread this account takes part in.</summary>
        Task<int> GetUnreadTotalAsync(string userId);
    }
}
