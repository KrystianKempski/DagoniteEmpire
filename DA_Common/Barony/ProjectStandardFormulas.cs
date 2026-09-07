using System;

namespace DA_Common.Barony
{
    /// <summary>Standard projects: convert one resource into another on completion.</summary>
    public static class ProjectStandardFormulas
    {
        public const int BuyProductionGoldPerUnit = 3;

        public static bool IsStandardKind(string? kind) =>
            string.Equals(kind, ProjectOutputKind.Standard, StringComparison.OrdinalIgnoreCase);

        public static bool IsBuyProduction(string? outputKind, string? notes) =>
            IsStandardKind(outputKind)
            && string.Equals(
                ProjectStandardNotes.GetSubtype(notes),
                ProjectStandardSubtype.BuyProduction,
                StringComparison.OrdinalIgnoreCase);

        /// <summary>Max Production purchasable: floor(Loyalty / 2).</summary>
        public static int MaxProductionFromLoyalty(decimal loyalty) =>
            Math.Max(0, (int)Math.Floor(loyalty / 2m));

        public static int ProductionFromGold(int gold) =>
            Math.Max(0, gold / BuyProductionGoldPerUnit);

        public static int GoldForProduction(int production) =>
            Math.Max(0, production) * BuyProductionGoldPerUnit;

        public static int ClampGoldSpend(int gold, decimal loyalty)
        {
            var maxGold = GoldForProduction(MaxProductionFromLoyalty(loyalty));
            if (maxGold <= 0)
                return 0;
            var stepped = Math.Max(0, gold / BuyProductionGoldPerUnit) * BuyProductionGoldPerUnit;
            return Math.Min(stepped, maxGold);
        }

        public static int ClampProductionAmount(int production, decimal loyalty)
        {
            var max = MaxProductionFromLoyalty(loyalty);
            return Math.Clamp(production, 0, max);
        }
    }
}
