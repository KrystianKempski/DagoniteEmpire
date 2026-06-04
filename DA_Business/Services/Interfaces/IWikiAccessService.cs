using DA_Business.Services.Wiki;

namespace DA_Business.Services.Interfaces;

/// <summary>
/// Phase 2: per-slug wiki visibility from character ownership and party rules.
/// Phase 1: <see cref="CanAccessAllWiki"/> mirrors authenticated access.
/// </summary>
public interface IWikiAccessService
{
    Task<bool> CanAccessAllWiki(string? userName, bool isAdminOrMg);

    Task<bool> CanAccessSlug(string? userName, bool isAdminOrMg, string slug);

    Task<string?> FilterContentIndexAsync(string? userName, bool isAdminOrMg, string json);

    /// <summary>Shadow index for encrypted/unlisted pages — opaque blobs; non-MG get empty entries.</summary>
    Task<string> FilterEncryptedContentIndexAsync(string? userName, bool isAdminOrMg, string json);

    Task<string?> FilterSitemapAsync(string? userName, bool isAdminOrMg, string xml);

    bool IsAnonymousPublicPath(string slug);

    /// <summary>First iframe URL: campaign hub when allowed, otherwise public lore root.</summary>
    Task<string> GetDefaultWikiIframePathAsync(string? userName, bool isAdminOrMg);

    /// <summary>MG/Admin (or CharacterMG): no tag/slug checks in app or iframe JS.</summary>
    Task<bool> ShouldBypassAccessChecksAsync(string? userName, bool isAdminOrMg);

    /// <summary>Development troubleshooting — who the wiki ACL layer thinks you are.</summary>
    Task<WikiAccessDiagnostics> GetAccessDiagnosticsAsync(string? userName, bool isAdminOrMg);
}
