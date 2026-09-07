using DA_Models.ChatModels;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// In-process pub/sub for campaign chat. One process today; swap for SignalR + backplane if scaled out.
    /// </summary>
    public interface ICampaignChatBroadcaster
    {
        void Publish(CampaignChatMessageDTO message);

        /// <summary>
        /// Subscribe to messages visible to <paramref name="characterId"/> in <paramref name="campaignId"/>.
        /// Dispose the returned handle to unsubscribe.
        /// </summary>
        IDisposable Subscribe(int campaignId, int characterId, Func<CampaignChatMessageDTO, Task> handler);
    }
}
