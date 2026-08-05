namespace DA_Common.Barony;

public sealed record UnitBattleExperienceBreakdown(
    int FromDamageDealt,
    int FromEngagedRounds,
    int FromKills,
    int FromDamageTakenLoss,
    int FromFleeLoss)
{
    public int Net => FromDamageDealt + FromEngagedRounds + FromKills - FromDamageTakenLoss - FromFleeLoss;
}

public static class UnitExperienceRules
{
    public const int DamageDealtPerXp = 3;
    public const int DamageTakenPerXpLoss = 8;
    public const int XpPerEngagedRound = 1;
    public const int XpPerKill = 3;
    public const int XpLossOnFlee = 1;

    public static UnitBattleExperienceBreakdown ComputeBattleXp(
        int damageDealt,
        int engagedRounds,
        int kills,
        int damageTaken,
        bool fled)
    {
        var dealt = Math.Max(0, damageDealt);
        var rounds = Math.Max(0, engagedRounds);
        var enemyKills = Math.Max(0, kills);
        var taken = Math.Max(0, damageTaken);

        return new UnitBattleExperienceBreakdown(
            FromDamageDealt: dealt / DamageDealtPerXp,
            FromEngagedRounds: rounds * XpPerEngagedRound,
            FromKills: enemyKills * XpPerKill,
            FromDamageTakenLoss: taken / DamageTakenPerXpLoss,
            FromFleeLoss: fled ? XpLossOnFlee : 0);
    }
}
