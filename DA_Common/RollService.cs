using System.Text;

namespace DA_Common;

public readonly record struct DiceRoll(int Sum, string Text);

public readonly record struct RollCheckResult(bool Success, string Text);

public readonly record struct OppositeRollResult(bool FirstSideWins, bool IsTie, string Text);

public static class RollService
{
    public const int CriticalSuccessMin = 17;
    public const int CriticalSuccessMax = 18;

    /// <summary>Override for unit tests: returns three d6 values.</summary>
    internal static Func<(int D1, int D2, int D3)>? TestDiceOverride { get; set; }

    public static bool IsCriticalSuccess(int diceSum) =>
        diceSum is >= CriticalSuccessMin and <= CriticalSuccessMax;

    public static DiceRoll RollDice()
    {
        int d1, d2, d3;
        if (TestDiceOverride is not null)
        {
            (d1, d2, d3) = TestDiceOverride();
        }
        else
        {
            d1 = RollD6();
            d2 = RollD6();
            d3 = RollD6();
        }

        var sum = d1 + d2 + d3;
        var text = $"(3d6: {d1}+{d2}+{d3}={sum})";
        return new DiceRoll(sum, text);
    }

    public static RollCheckResult MakeRollTest(int dc, int skill)
    {
        var roll = RollDice();
        var total = skill + roll.Sum;
        var success = total >= dc;
        var outcome = success ? "Success!" : "Fail!";
        var text = $"{skill} + {roll.Text} is {RichText.BoldText(total.ToString())} vs DC: {RichText.BoldText(dc.ToString())}. {RichText.BoldText(outcome)}";
        return new RollCheckResult(success, text);
    }

    public static OppositeRollResult MakeOppositeRollTest(string name1, int skill1, string name2, int skill2)
    {
        var roll1 = RollDice();
        var roll2 = RollDice();
        var total1 = skill1 + roll1.Sum;
        var total2 = skill2 + roll2.Sum;
        var side1 = $"{name1}: {skill1} + {roll1.Text} = {RichText.BoldText(total1.ToString())}";
        var side2 = $"{name2}: {skill2} + {roll2.Text} = {RichText.BoldText(total2.ToString())}";
        return BuildOppositeResult(name1, name2, total1, total2, side1, side2);
    }

    public static OppositeRollResult MakeOppositeRollTest(
        string name1,
        List<Pair<string, int>> bonuses1,
        string name2,
        List<Pair<string, int>> bonuses2)
    {
        var roll1 = RollDice();
        var roll2 = RollDice();
        var total1 = roll1.Sum + bonuses1.Sum(b => b.Second);
        var total2 = roll2.Sum + bonuses2.Sum(b => b.Second);
        var side1 = FormatSide(name1, bonuses1, roll1.Text, total1);
        var side2 = FormatSide(name2, bonuses2, roll2.Text, total2);
        return BuildOppositeResult(name1, name2, total1, total2, side1, side2);
    }

    private static OppositeRollResult BuildOppositeResult(
        string name1, string name2, int total1, int total2, string side1, string side2)
    {
        var isTie = total1 == total2;
        var firstWins = total1 >= total2;
        string outcome;
        if (isTie)
            outcome = $"Tie! {RichText.BoldText(name1)} wins on equal totals.";
        else if (firstWins)
            outcome = $"{RichText.BoldText(name1)} wins!";
        else
            outcome = $"{RichText.BoldText(name2)} wins!";

        var text = $"{side1} vs {side2}. {outcome}";
        return new OppositeRollResult(firstWins, isTie, text);
    }

    /// <summary>2d6 for economic conjuncture (sum 2–12).</summary>
    public static DiceRoll Roll2d6()
    {
        var d1 = RollD6();
        var d2 = RollD6();
        var sum = d1 + d2;
        return new DiceRoll(sum, $"(2d6: {d1}+{d2}={sum})");
    }

    /// <summary>Single d6: 1–6. Next(max) is exclusive, so upper bound must be 7.</summary>
    internal static int RollD6() => Random.Shared.Next(1, 7);

    private static string FormatSide(string name, List<Pair<string, int>> bonuses, string diceText, int total)
    {
        var sb = new StringBuilder();
        sb.Append($"{name}: ");
        for (var i = 0; i < bonuses.Count; i++)
        {
            var bonus = bonuses[i];
            if (i > 0)
                sb.Append(' ');
            sb.Append(bonus.Second >= 0 ? '+' : string.Empty);
            sb.Append($"{bonus.Second} ({bonus.First})");
        }
        sb.Append($" {diceText} = {RichText.BoldText(total.ToString())}");
        return sb.ToString();
    }
}
