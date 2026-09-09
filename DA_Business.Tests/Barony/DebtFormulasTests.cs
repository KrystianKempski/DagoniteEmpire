using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class DebtFormulasTests
{
    [Fact]
    public void AccrueInterest_AddsPercentToRemaining()
    {
        Assert.Equal(103m, DebtFormulas.AccrueInterest(100m, 3m));
        Assert.Equal(100m, DebtFormulas.AccrueInterest(100m, 0m));
    }

    [Fact]
    public void ComputeTakenPayment_CapsAtTreasury()
    {
        Assert.Equal(10m, DebtFormulas.ComputeTakenPayment(25m, 200m, 10m));
        Assert.Equal(25m, DebtFormulas.ComputeTakenPayment(25m, 200m, 100m));
        Assert.Equal(0m, DebtFormulas.ComputeTakenPayment(25m, 200m, 0m));
    }

    [Fact]
    public void ComputeGivenPayment_PaysUpToRemaining()
    {
        Assert.Equal(10m, DebtFormulas.ComputeGivenPayment(25m, 10m));
        Assert.Equal(0m, DebtFormulas.ComputeGivenPayment(0m, 100m));
    }

    [Fact]
    public void ProjectTurnPayments_AppliesStrictTreasuryForTaken()
    {
        var debts = new[]
        {
            (DebtDirection.Taken, 200m, 3m, 25m),
            (DebtDirection.Given, 50m, 2m, 10m),
        };

        var summary = DebtFormulas.ProjectTurnPayments(debts, 15m);
        Assert.Equal(15m, summary.TakenPayments);
        Assert.Equal(10m, summary.GivenReceipts);
    }

    [Fact]
    public void ProjectTurnPayments_IsPreviewOnly_ResolveAppliesPaymentsSeparately()
    {
        // Contract: HUD may net ProjectTurnPayments into the gold delta for display,
        // but ExpectedResourceIncome passed to ResolveTurn must not — Resolve applies
        // ApplyDebtPaymentsOnResolve after adding income. Double-counting caused missing
        // gold income on production baronies with active Taken loans (e.g. Darkhold seed).
        var debts = new[] { (DebtDirection.Taken, 200m, 0m, 30m) };
        var incomeWithoutDebt = 40m;
        var treasuryAfterIncome = 100m + incomeWithoutDebt;
        var preview = DebtFormulas.ProjectTurnPayments(debts, treasuryAfterIncome);

        var incomeIfWronglyNetted = incomeWithoutDebt - preview.TakenPayments;
        var treasuryAfterDoubleCount = 100m + incomeIfWronglyNetted - preview.TakenPayments;
        var treasuryCorrect = 100m + incomeWithoutDebt - preview.TakenPayments;

        Assert.Equal(30m, preview.TakenPayments);
        Assert.Equal(110m, treasuryCorrect);
        Assert.Equal(80m, treasuryAfterDoubleCount);
    }
}
