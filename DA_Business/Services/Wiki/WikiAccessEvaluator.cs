namespace DA_Business.Services.Wiki;

/// <summary>
/// Pure, side-effect-free wiki access decisions. All I/O (manifest loading, character lookups,
/// HttpContext) lives in <see cref="WikiAccessService"/>; this type only reasons over an already
/// built <see cref="WikiAccessContext"/> and <see cref="WikiAccessManifest"/> so it can be unit tested.
/// </summary>
public static class WikiAccessEvaluator
{
    /// <summary>
    /// Decides whether the given user (with the supplied resolved context) may open <paramref name="slug"/>.
    /// </summary>
    public static bool CanAccessSlug(
        string? userName,
        WikiAccessContext context,
        string slug,
        WikiAccessManifest manifest)
    {
        if (context.TreatAsAdmin)
        {
            return true;
        }

        slug = NormalizeSlug(slug);

        if (IsPrefixMatch(slug, manifest.AnonymousPrefixes))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        // Index, maps, etc. — only for characters tied to a campaign party (explorer + pages).
        if (context.IsCampaignParticipant && IsPrefixMatch(slug, manifest.LoggedInPublicPrefixes))
        {
            return true;
        }

        if (context.IsDukeLoreOnly)
        {
            return AllowsDukePlayerSlug(slug, manifest);
        }

        if (IsUnderCampaign(slug, manifest) && !context.IsCampaignParticipant)
        {
            return false;
        }

        if (!TryGetManifestEntry(slug, manifest, out var entry))
        {
            return false;
        }

        return EvaluateEntry(entry, userName, context, slug, manifest);
    }

    public static bool IsAnonymousPublicPath(WikiAccessManifest? manifest, string slug)
    {
        slug = NormalizeSlug(slug);

        if (manifest is null)
        {
            return slug.StartsWith("świat-i-zasady", StringComparison.OrdinalIgnoreCase);
        }

        if (manifest.Slugs.TryGetValue(slug, out var entry) && entry.Mode == WikiAccessMode.Anonymous)
        {
            return true;
        }

        return IsPrefixMatch(slug, manifest.AnonymousPrefixes);
    }

    public static bool IsCampaignParticipant(IReadOnlyCollection<string> userCharacters, WikiAccessManifest? manifest)
    {
        if (manifest is null || manifest.AllPartyCharacters.Count == 0)
        {
            return false;
        }

        return userCharacters.Any(c => manifest.AllPartyCharacters.Contains(c, StringComparer.OrdinalIgnoreCase));
    }

    public static bool IsUnderCampaign(string slug, WikiAccessManifest manifest) =>
        manifest.CampaignIds.Any(id =>
            slug.Equals(id, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(id + "/", StringComparison.OrdinalIgnoreCase));

    public static bool IsPrefixMatch(string slug, IEnumerable<string> prefixes) =>
        prefixes.Any(p =>
            slug.Equals(p, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));

    /// <summary>Maps raw character names (e.g. from the selected hero) to their canonical form.</summary>
    public static HashSet<string> Canonicalize(IEnumerable<string> names, WikiAccessManifest? manifest)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (manifest is not null
                && manifest.CharacterCanonical.TryGetValue(NormalizeCharacter(name), out var canonical))
            {
                result.Add(canonical);
            }
            else
            {
                result.Add(name);
            }
        }

        return result;
    }

    public static string NormalizeSlug(string slug)
    {
        // Folder links in Quartz arrive with a trailing slash (".../postacie/"); the manifest keys are
        // stored without it, so collapse the trailing slash before matching or the folder reads as deny.
        slug = slug.Trim().TrimStart('/').Replace('\\', '/').TrimEnd('/');
        if (slug.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[..^5];
        }

        if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[..^6].TrimEnd('/');
        }

        return slug;
    }

    public static string NormalizeCharacter(string value) =>
        new string((value ?? string.Empty)
            .Where(ch => ch is not (' ' or '-' or '_' or '.'))
            .ToArray())
            .ToLowerInvariant();

    private static bool EvaluateEntry(
        WikiAccessEntry entry,
        string? userName,
        WikiAccessContext context,
        string slug,
        WikiAccessManifest manifest)
    {
        return entry.Mode switch
        {
            WikiAccessMode.Anonymous => true,
            WikiAccessMode.Deny => false,
            WikiAccessMode.Authenticated => IsPrefixMatch(slug, manifest.LoggedInPublicPrefixes)
                    || IsUnderCampaign(slug, manifest)
                ? context.IsCampaignParticipant
                : !string.IsNullOrWhiteSpace(userName),
            WikiAccessMode.Characters or WikiAccessMode.Party =>
                HasCharacterAccess(entry, context, manifest),
            _ => false,
        };
    }

    /// <summary>
    /// Allowed characters = explicitly listed characters ∪ members of referenced parties
    /// (expanded from the manifest at runtime, so party membership lives in one place).
    /// </summary>
    private static bool HasCharacterAccess(
        WikiAccessEntry entry,
        WikiAccessContext context,
        WikiAccessManifest manifest)
    {
        if (context.UserCharacters.Count == 0)
        {
            return false;
        }

        if (entry.Characters.Any(c =>
                context.UserCharacters.Contains(c, StringComparer.OrdinalIgnoreCase)))
        {
            return true;
        }

        foreach (var partyId in entry.Parties)
        {
            if (manifest.Parties.TryGetValue(partyId, out var members)
                && members.Any(m => context.UserCharacters.Contains(m, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetManifestEntry(string slug, WikiAccessManifest manifest, out WikiAccessEntry entry)
    {
        if (manifest.Slugs.TryGetValue(slug, out entry!))
        {
            return true;
        }

        if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            var parent = slug[..^6].TrimEnd('/');
            if (manifest.Slugs.TryGetValue(parent, out entry!))
            {
                return true;
            }
        }

        entry = null!;
        return false;
    }

    private static bool AllowsDukePlayerSlug(string slug, WikiAccessManifest manifest)
    {
        if (IsPrefixMatch(slug, manifest.AnonymousPrefixes))
        {
            return true;
        }

        return manifest.DukeAccessibleCampaignIds.Any(id =>
            slug.Equals(id, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(id + "/", StringComparison.OrdinalIgnoreCase));
    }
}
