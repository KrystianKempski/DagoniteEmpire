using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace DagoniteEmpire.Localization;

/// <summary>
/// Maps a visitor country to a supported UI culture. Only Poland selects Polish;
/// anything else falls through so Accept-Language / default English can apply.
/// </summary>
public static class GeoCountry
{
    public const string PolishCountry = "PL";
    public const string PolishCulture = "pl";

    public static readonly string[] HeaderNames =
    [
        "CF-IPCountry",
        "CloudFront-Viewer-Country",
        "Fastly-Country-Code",
        "X-Country-Code",
        "X-AppEngine-Country",
    ];

    public static string? CultureForCountry(string? countryCode) =>
        string.Equals(NormalizeCountryCode(countryCode), PolishCountry, StringComparison.Ordinal)
            ? PolishCulture
            : null;

    public static string? FromHeaders(IHeaderDictionary headers)
    {
        foreach (var name in HeaderNames)
        {
            if (!headers.TryGetValue(name, out var raw))
                continue;
            var code = NormalizeCountryCode(raw.ToString());
            if (code is not null)
                return code;
        }

        return null;
    }

    public static string? NormalizeCountryCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var code = value.Trim().ToUpperInvariant();
        if (code.Length != 2)
            return null;

        // Cloudflare / proxies: unknown, Tor, anonymous, satellite.
        if (code is "XX" or "T1" or "A1" or "A2" or "O1")
            return null;

        return code;
    }

    public static IPAddress? GetClientIp(HttpContext context)
    {
        if (TryParseIp(context.Request.Headers["CF-Connecting-IP"], out var ip))
            return ip;

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var first = forwarded.ToString().Split(',', 2)[0];
            if (TryParseIp(first, out ip))
                return ip;
        }

        return context.Connection.RemoteIpAddress;
    }

    public static bool IsPublicIp(IPAddress? ip)
    {
        if (ip is null)
            return false;
        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();
        if (IPAddress.IsLoopback(ip))
            return false;

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = ip.GetAddressBytes();
            if (b[0] == 0 || b[0] == 10 || b[0] == 127)
                return false;
            if (b[0] == 169 && b[1] == 254)
                return false;
            if (b[0] == 172 && b[1] is >= 16 and <= 31)
                return false;
            if (b[0] == 192 && b[1] == 168)
                return false;
            if (b[0] == 100 && b[1] is >= 64 and <= 127)
                return false;
            return true;
        }

        if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast)
            return false;
        var bytes = ip.GetAddressBytes();
        if (bytes.Length > 0 && (bytes[0] & 0xFE) == 0xFC)
            return false;
        return !ip.Equals(IPAddress.IPv6Any);
    }

    private static bool TryParseIp(string? value, out IPAddress ip)
    {
        ip = IPAddress.None;
        return !string.IsNullOrWhiteSpace(value) && IPAddress.TryParse(value.Trim(), out ip!);
    }
}
