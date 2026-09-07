using DA_Models.ChatModels;

namespace DA_Business.Repository.ChatRepos.IRepository
{
    public interface ICampaignChatRepository
    {
        Task<IReadOnlyList<CampaignChatContactDTO>> GetContactsAsync(
            int campaignId,
            int myCharacterId,
            string userName,
            bool isAdminOrMg);

        Task<IReadOnlyList<CampaignChatMessageDTO>> GetThreadAsync(
            int campaignId,
            int myCharacterId,
            int? peerCharacterId,
            string userName,
            bool isAdminOrMg,
            int take = 50,
            long? beforeId = null);

        Task<CampaignChatMessageDTO> SendAsync(
            int campaignId,
            int senderCharacterId,
            int? recipientCharacterId,
            string content,
            string userName,
            bool isAdminOrMg);

        Task MarkReadAsync(
            int campaignId,
            int myCharacterId,
            int? peerCharacterId,
            string userName,
            bool isAdminOrMg);

        Task<int> GetUnreadSummaryAsync(
            int myCharacterId,
            string userName,
            bool isAdminOrMg);
    }
}
