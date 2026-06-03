using System.Text.Json;
using System.Text.RegularExpressions;
using DA_Business.Services.Interfaces;
using DA_Business.Services.Wiki;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace DA_Business.Services;

public class WikiLinkService : IWikiLinkService
{
    private const string LinksCacheKey = "wiki-links-v1";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _cache;

    public WikiLinkService(IWebHostEnvironment environment, IMemoryCache cache)
    {
        _environment = environment;
        _cache = cache;
    }

    public bool IsWikiDeployed()
    {
        var root = Path.Combine(_environment.WebRootPath ?? "", "wiki");
        return File.Exists(Path.Combine(root, "index.html"));
    }

    public string? GetCharacterPagePath(string? npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
        {
            return null;
        }

        var links = LoadLinks();
        if (links?.Characters.TryGetValue(npcName.Trim(), out var slug) == true)
        {
            return ToWikiPath(slug);
        }

        return null;
    }

    public string? GetCampaignPagePath(string? campaignName)
    {
        if (string.IsNullOrWhiteSpace(campaignName))
        {
            return null;
        }

        var links = LoadLinks();
        if (links is null)
        {
            return null;
        }

        var key = campaignName.Trim();
        if (links.Campaigns.TryGetValue(key, out var slug))
        {
            return ToWikiPath(slug);
        }

        foreach (var (name, path) in links.Campaigns)
        {
            if (key.Contains(name, StringComparison.OrdinalIgnoreCase)
                || name.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return ToWikiPath(path);
            }
        }

        return null;
    }

    public string? GetChapterArchivePath(string? chapterName)
    {
        if (string.IsNullOrWhiteSpace(chapterName))
        {
            return null;
        }

        var links = LoadLinks();
        if (links is null)
        {
            return null;
        }

        var normChapter = NormalizeMatchKey(chapterName);
        string? bestSlug = null;
        var bestScore = 0;

        foreach (var (title, slug) in links.Chapters)
        {
            var normTitle = NormalizeMatchKey(title);
            if (normChapter == normTitle)
            {
                return ToWikiPath(slug);
            }

            if (normChapter.Contains(normTitle, StringComparison.OrdinalIgnoreCase)
                || normTitle.Contains(normChapter, StringComparison.OrdinalIgnoreCase))
            {
                var score = Math.Min(normChapter.Length, normTitle.Length);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSlug = slug;
                }
            }
        }

        return bestSlug is null ? null : ToWikiPath(bestSlug);
    }

    private WikiLinksFile? LoadLinks()
    {
        return _cache.GetOrCreate(LinksCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var path = Path.Combine(_environment.WebRootPath ?? "", "wiki", "static", "wiki-links.json");
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<WikiLinksFile>(File.ReadAllText(path), JsonOptions);
            }
            catch
            {
                return null;
            }
        });
    }

    private static string ToWikiPath(string slug)
    {
        slug = slug.Trim().TrimStart('/');
        if (slug.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            return $"/wiki/{slug}";
        }

        return $"/wiki/{slug}.html";
    }

    private static string NormalizeMatchKey(string value)
    {
        value = value.ToLowerInvariant();
        value = Regex.Replace(value, @"[^a-z0-9ąćęłńóśźż]+", "", RegexOptions.IgnoreCase);
        return value;
    }
}
