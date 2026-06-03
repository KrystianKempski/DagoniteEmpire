namespace DA_Business.Services.Interfaces;

public interface IWikiLinkService
{
    bool IsWikiDeployed();

    string? GetCharacterPagePath(string? npcName);

    string? GetCampaignPagePath(string? campaignName);

    string? GetChapterArchivePath(string? chapterName);

    IReadOnlyList<string> GetAllPlayerNamesForPreview();
}
