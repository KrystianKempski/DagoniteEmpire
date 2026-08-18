using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;

namespace DagoniteEmpire.Localization;

/// <summary>
/// First-visit culture from visitor country (CDN country header, else IP lookup).
/// Cookie and query-string providers still run first, so an explicit PL/EN choice wins.
/// </summary>
public sealed class GeoCountryRequestCultureProvider : RequestCultureProvider
{
    public const string HttpClientName = "geo-country";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);
    private static readonly Uri LookupBase = new("https://get.geojs.io/v1/ip/country/");

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoCountryRequestCultureProvider> _logger;

    public GeoCountryRequestCultureProvider(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<GeoCountryRequestCultureProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var country = GeoCountry.FromHeaders(httpContext.Request.Headers)
                      ?? await LookupCountryFromIpAsync(httpContext, httpContext.RequestAborted);
        var culture = GeoCountry.CultureForCountry(country);
        if (culture is null)
            return null;

        return new ProviderCultureResult(culture, culture);
    }

    private async Task<string?> LookupCountryFromIpAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var ip = GeoCountry.GetClientIp(httpContext);
        if (!GeoCountry.IsPublicIp(ip))
            return null;

        var cacheKey = "geo-country:" + ip;
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(new Uri(LookupBase, ip!.ToString()), cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            var country = GeoCountry.NormalizeCountryCode(body);
            _cache.Set(cacheKey, country, CacheDuration);
            return country;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogDebug(ex, "Geo country lookup failed for {Ip}", ip);
            return null;
        }
    }
}
