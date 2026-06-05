using DA_Models.ChatModels;

namespace DA_Business.Services.Interfaces
{
    public interface ICampaignSummaryService
    {
        Task<CampaignSummaryResult?> GenerateChapterSummaryAsync(int chapterId);
        Task<CampaignSummaryResult?> GenerateCampaignSummaryAsync(int campaignId);
    }
}
