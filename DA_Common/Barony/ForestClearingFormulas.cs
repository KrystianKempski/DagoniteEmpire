using System;

namespace DA_Common.Barony
{
    public enum ForestClearingVariant
    {
        Forest,
        DenseForest,
    }

    /// <summary>Costs and one-time rewards for map forest-clearing projects.</summary>
    public static class ForestClearingFormulas
    {
        public const int ForestProductionCost = 100;
        public const int ForestGoldCost = 30;
        public const int ForestProductionReward = 200;
        public const int ForestGoldReward = 50;

        public const int DenseForestProductionCost = 300;
        public const int DenseForestGoldCost = 100;
        public const int DenseForestProductionReward = 600;
        public const int DenseForestGoldReward = 200;

        public static int ClampTurnsRemaining(int turns) =>
            ProjectConstructionRules.ClampTurnsRemaining(turns);

        public static bool IsOutputKind(string? kind) =>
            string.Equals(kind, ProjectOutputKind.ForestClearing, StringComparison.OrdinalIgnoreCase);

        public static bool CanClearTile(int featuresMask) => DetectVariant(featuresMask) is not null;

        public static ForestClearingVariant? DetectVariant(int featuresMask)
        {
            if (TerrainFeature.Has(featuresMask, TerrainFeature.DenseForest))
                return ForestClearingVariant.DenseForest;
            if (TerrainFeature.Has(featuresMask, TerrainFeature.Forest))
                return ForestClearingVariant.Forest;
            return null;
        }

        public static PpbVector Cost(ForestClearingVariant variant)
        {
            var cost = new PpbVector();
            if (variant == ForestClearingVariant.DenseForest)
            {
                cost[Ppb.Production] = DenseForestProductionCost;
                cost[Ppb.Treasury] = DenseForestGoldCost;
            }
            else
            {
                cost[Ppb.Production] = ForestProductionCost;
                cost[Ppb.Treasury] = ForestGoldCost;
            }

            return cost;
        }

        public static PpbVector Reward(ForestClearingVariant variant)
        {
            var reward = new PpbVector();
            if (variant == ForestClearingVariant.DenseForest)
            {
                reward[Ppb.Production] = DenseForestProductionReward;
                reward[Ppb.Treasury] = DenseForestGoldReward;
            }
            else
            {
                reward[Ppb.Production] = ForestProductionReward;
                reward[Ppb.Treasury] = ForestGoldReward;
            }

            return reward;
        }

        public static int ClearedFeatureFlag(ForestClearingVariant variant) =>
            variant == ForestClearingVariant.DenseForest
                ? TerrainFeature.DenseForest
                : TerrainFeature.Forest;

        public static string VariantDisplayKey(ForestClearingVariant variant) =>
            variant == ForestClearingVariant.DenseForest
                ? TerrainFeature.DenseForestName
                : TerrainFeature.ForestName;
    }
}
