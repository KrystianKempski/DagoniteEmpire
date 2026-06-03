namespace DA_Business.Services.Interfaces;

public interface IWikiViewAsService
{
    string? GetViewAsCharacterName();

    /// <summary>True when MG/Admin simulates a single character (not full GM access).</summary>
    bool IsPreviewActive();

    void SetViewAs(string? npcName);

    void ClearViewAs();
}
