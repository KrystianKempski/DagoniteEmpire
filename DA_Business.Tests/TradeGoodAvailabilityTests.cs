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
    public void DomainPanelBonusParts_SumsIntoSingleTradeGoodsAndTreatiesRow()
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

        var availability = TradeGoodAvailability.Resolve(
            new[] { "Apiary" },
            new[] { treaty },
            null);

        var parts = TradeGoodAvailability.DomainPanelBonusParts(
            availability,
            new[] { treaty },
            LuxuryGoodsAccessCatalog.Insufficient);

        var row = Assert.Single(parts);
        Assert.Equal(TradeGoodAvailability.DomainPanelRowLabel, row.Label);
        Assert.Equal(-1m, row.Additive[Ppb.Loyalty]); // honey +1, insufficient luxury −2
        Assert.Equal(1m, row.Additive[Ppb.Science]);
        Assert.Equal(2m, row.Additive[Ppb.Economy]); // spices
        Assert.Equal(-2m, row.Additive[Ppb.Stability]); // insufficient luxury
        Assert.Contains("dostępne towary: 2", row.Note);
        Assert.Contains("luksus", row.Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DomainPanelBonusParts_IncludesCustomsAndSweetenersAsTreasury()
    {
        var treaty = new BaronyTradeTreaty
        {
            Id = "t-gold",
            CounterpartyLordKey = "corlin",
            Paragraphs =
            {
                new TradeTreatyParagraph
                {
                    LordKey = "olgred",
                    IsDestination = false,
                    CustomsGoldPerTurn = 4,
                },
                new TradeTreatyParagraph
                {
                    LordKey = "corlin",
                    IsDestination = true,
                    SweetenerGoldPerTurn = 5,
                    CounterpartyGrantsGoodKeys = { "horses" },
                },
            },
        };

        var availability = TradeGoodAvailability.Resolve(
            Array.Empty<string>(),
            new[] { treaty },
            null);

        var parts = TradeGoodAvailability.DomainPanelBonusParts(
            availability,
            new[] { treaty },
            LuxuryGoodsAccessCatalog.Basic);

        var row = Assert.Single(parts);
        Assert.Equal(TradeGoodAvailability.DomainPanelRowLabel, row.Label);
        Assert.Equal(-9m, row.Additive[Ppb.Treasury]);
        Assert.Equal(1m, row.Additive[Ppb.Defense]);
        Assert.Equal(1m, row.Additive[Ppb.Production]);

        TradeTreatyCalculator.SumTreatyBonuses(new[] { treaty }, out var add, out _);
        Assert.Equal(-9m, add[Ppb.Treasury]);
        Assert.Equal(1m, add[Ppb.Defense]);
        Assert.Equal(1m, add[Ppb.Production]);
    }

    [Fact]
    public void Resolve_PendingTreaty_DoesNotUnlockGoodsOrBonuses()
    {
        var treaty = new BaronyTradeTreaty
        {
            Id = "t-pending",
            CounterpartyLordKey = "x",
            IsApproved = false,
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

        var snap = TradeGoodAvailability.Resolve(Array.Empty<string>(), new[] { treaty }, null);

        Assert.DoesNotContain("spices", snap.TreatyReceivedKeys);
        Assert.False(snap.IsAvailable("spices"));

        TradeTreatyCalculator.SumTreatyBonuses(new[] { treaty }, out var add, out var pct);
        Assert.True(add.IsEmpty);
        Assert.True(pct.IsEmpty);
    }

    [Fact]
    public void Resolve_LegacyTreatyWithoutApprovalFlag_StillApplies()
    {
        var treaty = new BaronyTradeTreaty
        {
            Id = "t-legacy",
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

        var snap = TradeGoodAvailability.Resolve(Array.Empty<string>(), new[] { treaty }, null);

        Assert.Contains("spices", snap.TreatyReceivedKeys);
    }
}
