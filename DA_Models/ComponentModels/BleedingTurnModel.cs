using DA_Common;

namespace DA_Models.ComponentModels;

public sealed record BleedingPainTestResult(
    int Dc,
    bool Passed,
    string ResultText,
    int UnconsciousDuration);

public static class BleedingTurnModel
{
    public const int DcBase = 104;

    public static int GetBleedingPainTestDc(int turnsRemaining) =>
        Math.Max(1, DcBase - turnsRemaining);

    public static BleedingPainTestResult RunPainTest(int turnsRemaining, int painResistance)
    {
        var dc = GetBleedingPainTestDc(turnsRemaining);
        var roll = SD.MakeRollTestForFight(dc, painResistance);
        var unconsciousDuration = Math.Max(1, dc / 10);
        return new BleedingPainTestResult(dc, roll.Item1, roll.Item2, unconsciousDuration);
    }

    public static bool TryParseMobStateDuration(string? states, string stateName, out int duration)
    {
        duration = 0;
        if (string.IsNullOrWhiteSpace(states))
            return false;

        foreach (var state in states.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = state.Split(':', 2);
            if (parts.Length < 2 || !string.Equals(parts[0].Trim(), stateName, StringComparison.Ordinal))
                continue;

            if (int.TryParse(parts[1].Trim(), out duration))
                return true;
        }

        return false;
    }

    public static bool MobHasState(string? states, string stateName) =>
        TryParseMobStateDuration(states, stateName, out _);
}
