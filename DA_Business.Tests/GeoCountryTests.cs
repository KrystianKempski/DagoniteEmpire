using System.Net;
using DagoniteEmpire.Localization;
using Microsoft.AspNetCore.Http;

namespace DA_Business.Tests;

public class GeoCountryTests
{
    [Theory]
    [InlineData("PL", "pl")]
    [InlineData("pl", "pl")]
    [InlineData(" US ", null)]
    [InlineData("XX", null)]
    [InlineData("T1", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void CultureForCountry_SelectsPolishOnly(string? country, string? expected) =>
        Assert.Equal(expected, GeoCountry.CultureForCountry(country));

    [Fact]
    public void FromHeaders_ReadsCloudflareCountry()
    {
        var headers = new HeaderDictionary { ["CF-IPCountry"] = "PL" };
        Assert.Equal("PL", GeoCountry.FromHeaders(headers));
        Assert.Equal("pl", GeoCountry.CultureForCountry(GeoCountry.FromHeaders(headers)));
    }

    [Fact]
    public void FromHeaders_IgnoresUnknownCloudflareCodes()
    {
        var headers = new HeaderDictionary { ["CF-IPCountry"] = "XX" };
        Assert.Null(GeoCountry.FromHeaders(headers));
    }

    [Theory]
    [InlineData("127.0.0.1", false)]
    [InlineData("10.0.0.8", false)]
    [InlineData("192.168.1.10", false)]
    [InlineData("172.16.0.1", false)]
    [InlineData("8.8.8.8", true)]
    [InlineData("193.0.0.1", true)]
    public void IsPublicIp_ClassifiesAddresses(string ip, bool expected) =>
        Assert.Equal(expected, GeoCountry.IsPublicIp(IPAddress.Parse(ip)));
}
