using DA_Business.Services.Wiki;
using Xunit;

namespace DA_Business.Tests.Wiki;

/// <summary>
/// Golden-matrix coverage for the pure wiki access decision. Locks the current behavior so the
/// surrounding service/middleware can be refactored without silently changing who sees what.
/// </summary>
public class WikiAccessEvaluatorTests
{
    private const string Anon = "anon";
    private const string LoggedNoParty = "logged-no-party";
    private const string Bonefyre = "bonefyre-member";
    private const string Smok = "smok-member";
    private const string Duke = "duke-lore-only";
    private const string Mg = "mg";

    [Theory]
    // Public lore — visible to everyone, including anonymous.
    [InlineData("świat-i-zasady", Anon, true)]
    [InlineData("świat-i-zasady/organizacje/dom-bonefyre", Anon, true)]
    [InlineData("świat-i-zasady", LoggedNoParty, true)]
    [InlineData("świat-i-zasady", Duke, true)]
    // Logged-in hub (index/maps) — only campaign participants.
    [InlineData("index", Anon, false)]
    [InlineData("index", LoggedNoParty, false)]
    [InlineData("index", Bonefyre, true)]
    [InlineData("index", Smok, true)]
    [InlineData("index", Duke, false)]
    [InlineData("mapy/mapa-powiązań", LoggedNoParty, false)]
    [InlineData("mapy/mapa-powiązań", Bonefyre, true)]
    // Campaign root — participants and (by config) the duke; not outsiders.
    [InlineData("w-służbie-bonefyre", Anon, false)]
    [InlineData("w-służbie-bonefyre", LoggedNoParty, false)]
    [InlineData("w-służbie-bonefyre", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre", Smok, true)]
    [InlineData("w-służbie-bonefyre", Duke, true)]
    // Folder/section pages reached via Quartz links carry a trailing slash — must still resolve.
    [InlineData("w-służbie-bonefyre/", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre/postacie/", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre/postacie", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre/postacie/", LoggedNoParty, false)]
    // Character page (visibility: characters [Lawenda]) — only that hero's party owner.
    [InlineData("w-służbie-bonefyre/postacie/lawenda", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre/postacie/lawenda", Smok, false)]
    [InlineData("w-służbie-bonefyre/postacie/lawenda", LoggedNoParty, false)]
    [InlineData("w-służbie-bonefyre/postacie/lawenda", Duke, true)]
    // Party page (visibility: party [bonefyre]).
    [InlineData("w-służbie-bonefyre/wspolne/sekret", Bonefyre, true)]
    [InlineData("w-służbie-bonefyre/wspolne/sekret", Smok, false)]
    // Team-tagged page expanded to the pijany-smok party.
    [InlineData("w-służbie-bonefyre/postacie/udar", Bonefyre, false)]
    [InlineData("w-służbie-bonefyre/postacie/udar", Smok, true)]
    // GM-only / deny page outside the campaign tree.
    [InlineData("tajne/gm", Bonefyre, false)]
    [InlineData("tajne/gm", Duke, false)]
    // Unknown slug fails closed for non-MG.
    [InlineData("nieznany/slug", Bonefyre, false)]
    [InlineData("nieznany/slug", Anon, false)]
    public void CanAccessSlug_matches_golden_matrix(string slug, string profile, bool expected)
    {
        var manifest = BuildManifest();
        var (userName, context) = BuildProfile(profile, manifest);

        var actual = WikiAccessEvaluator.CanAccessSlug(userName, context, slug, manifest);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Mg_sees_everything()
    {
        var manifest = BuildManifest();
        var (userName, context) = BuildProfile(Mg, manifest);

        foreach (var slug in new[]
        {
            "świat-i-zasady", "index", "w-służbie-bonefyre",
            "w-służbie-bonefyre/postacie/lawenda", "tajne/gm", "nieznany/slug",
        })
        {
            Assert.True(WikiAccessEvaluator.CanAccessSlug(userName, context, slug, manifest), slug);
        }
    }

    [Theory]
    [InlineData("świat-i-zasady", true)]
    [InlineData("świat-i-zasady/organizacje/dom-bonefyre", true)]
    [InlineData("index", false)]
    [InlineData("w-służbie-bonefyre/postacie/lawenda", false)]
    public void IsAnonymousPublicPath_only_for_public_prefixes(string slug, bool expected) =>
        Assert.Equal(expected, WikiAccessEvaluator.IsAnonymousPublicPath(BuildManifest(), slug));

    [Theory]
    [InlineData("/foo/bar.html", "foo/bar")]
    [InlineData("foo/bar/index.html", "foo/bar")]
    [InlineData("foo/bar/index", "foo/bar")]
    [InlineData("foo/bar/", "foo/bar")]
    [InlineData("foo/", "foo")]
    [InlineData("index", "index")]
    [InlineData("foo\\bar", "foo/bar")]
    public void NormalizeSlug_strips_html_index_and_trailing_slash(string input, string expected) =>
        Assert.Equal(expected, WikiAccessEvaluator.NormalizeSlug(input));

    [Fact]
    public void Canonicalize_maps_aliases_to_canonical_names()
    {
        var manifest = BuildManifest();
        var result = WikiAccessEvaluator.Canonicalize(["Bron", "cedryk"], manifest);

        Assert.Contains("Sir Bron", result);
        Assert.Contains("Sir Cedrick", result);
    }

    private static WikiAccessManifest BuildManifest()
    {
        var bonefyre = new List<string> { "Lawenda", "Sariel", "Dorian", "Umbra", "Sir Bron", "Werner", "Baron Mevir" };
        var smok = new List<string> { "Sir Cedrick", "Granit", "Udar", "Sharu", "Tomin" };

        var manifest = new WikiAccessManifest
        {
            AnonymousPrefixes = ["świat-i-zasady"],
            LoggedInPublicPrefixes = ["index", "mapy"],
            CampaignIds = ["w-służbie-bonefyre"],
            DukeAccessibleCampaignIds = ["w-służbie-bonefyre"],
        };

        foreach (var name in bonefyre) manifest.AllPartyCharacters.Add(name);
        foreach (var name in smok) manifest.AllPartyCharacters.Add(name);
        manifest.Parties["bonefyre"] = bonefyre;
        manifest.Parties["pijany-smok"] = smok;

        foreach (var (alias, canonical) in new[]
        {
            ("Bron", "Sir Bron"), ("Cedrick", "Sir Cedrick"), ("Cedryk", "Sir Cedrick"),
        })
        {
            manifest.CharacterCanonical[WikiAccessEvaluator.NormalizeCharacter(alias)] = canonical;
        }

        manifest.Slugs["w-służbie-bonefyre"] = new WikiAccessEntry { Mode = WikiAccessMode.Authenticated };
        manifest.Slugs["w-służbie-bonefyre/postacie"] =
            new WikiAccessEntry { Mode = WikiAccessMode.Characters, Parties = ["bonefyre", "pijany-smok"] };
        manifest.Slugs["index"] = new WikiAccessEntry { Mode = WikiAccessMode.Authenticated };
        manifest.Slugs["w-służbie-bonefyre/postacie/lawenda"] =
            new WikiAccessEntry { Mode = WikiAccessMode.Characters, Characters = ["Lawenda"] };
        manifest.Slugs["w-służbie-bonefyre/wspolne/sekret"] =
            new WikiAccessEntry { Mode = WikiAccessMode.Party, Parties = ["bonefyre"] };
        manifest.Slugs["w-służbie-bonefyre/postacie/udar"] =
            new WikiAccessEntry { Mode = WikiAccessMode.Characters, Parties = ["pijany-smok"] };
        manifest.Slugs["tajne/gm"] = new WikiAccessEntry { Mode = WikiAccessMode.Deny };

        return manifest;
    }

    private static (string? UserName, WikiAccessContext Context) BuildProfile(string profile, WikiAccessManifest manifest)
    {
        switch (profile)
        {
            case Mg:
                return ("mg-user", new WikiAccessContext { TreatAsAdmin = true });

            case Anon:
                return (null, ContextFor([]));

            case LoggedNoParty:
                return ("u-noparty", ContextFor(["Wędrowiec"]));

            case Bonefyre:
                return ("u-bonefyre", ContextFor(["Lawenda"]));

            case Smok:
                return ("u-smok", ContextFor(["Granit"]));

            case Duke:
                var ctx = ContextFor([]);
                return ("u-duke", new WikiAccessContext
                {
                    UserCharacters = ctx.UserCharacters,
                    IsCampaignParticipant = false,
                    IsDukeLoreOnly = true,
                });

            default:
                throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown test profile");
        }

        WikiAccessContext ContextFor(string[] chars)
        {
            var set = WikiAccessEvaluator.Canonicalize(chars, manifest);
            return new WikiAccessContext
            {
                UserCharacters = set,
                IsCampaignParticipant = WikiAccessEvaluator.IsCampaignParticipant(set, manifest),
            };
        }
    }
}
