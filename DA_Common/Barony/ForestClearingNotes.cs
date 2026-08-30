using System;
using System.Linq;

namespace DA_Common.Barony
{
    /// <summary>Parses / updates the forest-clearing variant line in project Notes.</summary>
    public static class ForestClearingNotes
    {
        private const string Prefix = "ForestClearingVariant=";

        public static ForestClearingVariant? GetVariant(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return null;

            foreach (var line in notes.Split('\n'))
            {
                if (!line.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var raw = line[Prefix.Length..].Trim();
                if (string.Equals(raw, nameof(ForestClearingVariant.DenseForest), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, TerrainFeature.DenseForestName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, "Gęsty las", StringComparison.OrdinalIgnoreCase))
                    return ForestClearingVariant.DenseForest;

                if (string.Equals(raw, nameof(ForestClearingVariant.Forest), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, TerrainFeature.ForestName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, "Las", StringComparison.OrdinalIgnoreCase))
                    return ForestClearingVariant.Forest;
            }

            return null;
        }

        public static string SetVariant(string? notes, ForestClearingVariant variant)
        {
            var rest = (notes ?? string.Empty)
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0
                            && !l.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase));

            var line = $"{Prefix}{variant}";
            return string.Join("\n", rest.Prepend(line));
        }
    }
}
