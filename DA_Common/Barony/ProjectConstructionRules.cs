using System;

namespace DA_Common.Barony
{
    /// <summary>Turn duration rules for map/city construction projects.</summary>
    public static class ProjectConstructionRules
    {
        public const int MinTurnsRemaining = 1;
        public const int DefaultTurnsRemaining = 1;

        public static bool RequiresMinTurn(string? outputKind) =>
            ForestClearingFormulas.IsOutputKind(outputKind)
            || string.Equals(outputKind, ProjectOutputKind.Building, StringComparison.OrdinalIgnoreCase)
            || string.Equals(outputKind, ProjectOutputKind.Improvement, StringComparison.OrdinalIgnoreCase);

        public static int ClampTurnsRemaining(int turns) =>
            Math.Max(MinTurnsRemaining, turns);

        public static int DefaultTurnsFor(string? outputKind) =>
            RequiresMinTurn(outputKind) ? DefaultTurnsRemaining : 0;
    }
}
