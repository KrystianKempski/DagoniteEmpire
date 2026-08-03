using DA_Common.Barony;

namespace DA_Business.Tests;

public class TradeGoodAvailabilityTests
{
    [Fact]
    public void ProducedKeys_MatchMapImprovementCatalogDescription()
    {
        // Map pins: Name = map kind, Description = catalog template (matches ProductionBuilding).
        var produced = TradeGoodAvailability.ProducedKeysFromFacilityNames(
            TradeGoodAvailability.FacilityNamesFromMapImprovement("Sawmill", "Sawmill - Ironwood")
                .Concat(TradeGoodAvailability.FacilityNamesFromMapImprovement("Mine", "Mine - Salt")));

        Assert.Contains("ironwood", produced);
        Assert.Contains("salt", produced);
        Assert.DoesNotContain("shipbuilding-wood", produced);
        Assert.DoesNotContain("iron", produced);
    }

    [Fact]
    public void ProducedKeys_MatchFacilityNames_ExactCaseInsensitive()
    {
        var produced = TradeGoodAvailability.ProducedKeysFromFacilityNames(new[]
        {
            "Mine - Salt",
            "brewery",
            "Horse Stud (regular)",
            "Horse Stud (military)",
            "Import",
        });

        Assert.Contains("salt", produced);
        Assert.Contains("beer", produced);
        Assert.Contains("horses", produced);
        Assert.Contains("war-horses", produced);
        Assert.DoesNotContain("noble-horses", produced);
        Assert.DoesNotContain("olive-oil", produced);
    }

    [Fact]
    public void Resolve_TreatyReceived_IsAvailableButNotGrantable()
    {
        var treaty = new BaronyTradeTreaty
        {
            Id = "t1",
            CounterpartyLordKey = "x",
            Paragraphs =
            {
                new TradeTreatyParagraph
                {
                    LordKey = "x",
                    IsDestination = true,
                    CounterpartyGrantsGoodKeys = { "olive-oil", "silk" },
                },
            },
        };

        var snap = TradeGoodAvailability.Resolve(
            facilityNames: new[] { "Brewery" },
            treaties: new[] { treaty },
            mgOverrideKeys: new[] { "furs" });

        Assert.Contains("beer", snap.ProducedKeys);
        Assert.Contains("olive-oil", snap.TreatyReceivedKeys);
        Assert.Contains("silk", snap.TreatyReceivedKeys);
        Assert.Contains("furs", snap.OverrideKeys);

        Assert.True(snap.IsAvailable("beer"));
        Assert.True(snap.IsAvailable("olive-oil"));
        Assert.True(snap.IsAvailable("furs"));

        Assert.True(snap.IsGrantable("beer"));
        Assert.True(snap.IsGrantable("furs"));
        Assert.False(snap.IsGrantable("olive-oil"));
        Assert.False(snap.IsGrantable("silk"));
    }

    [Fact]
    public void DomainPanelBonusParts_IncludesAvailableGoodsLuxuryAndRoute()
    {
        var treaty = new BaronyTradeTreaty
        {
            Id = "t1",
            CounterpartyLordKey = "x",
            Paragraphs =
            {
                new TradeTreatyParagraph
                {
                    LordKey = "x",
                    IsDestination = true,
                    CounterpartyGrantsGoodKeys = { "spices" },
                },
            },
        };

        // Use a known lord key with Wealth so route economy is non-zero if possible;
        // otherwise parts still include the spice good.
        var availability = TradeGoodAvailability.Resolve(
            new[] { "Apiary" },
            new[] { treaty },
            null);

        var parts = TradeGoodAvailability.DomainPanelBonusParts(
            availability,
            new[] { treaty },
            LuxuryGoodsAccessCatalog.Insufficient);

        Assert.Contains(parts, p => p.Label == "Honey & wax");
        Assert.Contains(parts, p => p.Label == "Spices");
        Assert.Contains(parts, p => p.Label.StartsWith("Luxury access", StringComparison.Ordinal));
    }
}
