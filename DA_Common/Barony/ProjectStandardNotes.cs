using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DA_Common.Barony
{
    /// <summary>Parses / updates the standard-project subtype line in project Notes.</summary>
    public static class ProjectStandardNotes
    {
        private const string Prefix = "StandardSubtype=";
        private const string SourceIdPrefix = "BuyProductionSourceId=";
        public const string ResultsAppliedMarker = "ResultsApplied=1";

        public static string? GetSubtype(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return null;

            foreach (var line in notes.Split('\n'))
            {
                if (!line.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Legacy rows may carry other markers appended to this line after a ';'.
                var value = line[Prefix.Length..];
                var cut = value.IndexOf(';');
                return (cut >= 0 ? value[..cut] : value).Trim();
            }

            return null;
        }

        public static string SetSubtype(string? notes, string subtype)
        {
            var rest = (notes ?? string.Empty)
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l)
                    && !l.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var lines = new List<string> { Prefix + subtype.Trim() };
            lines.AddRange(rest);
            return string.Join("\n", lines);
        }

        public static bool HasResultsApplied(string? notes) =>
            !string.IsNullOrWhiteSpace(notes)
            && notes.Contains(ResultsAppliedMarker, StringComparison.OrdinalIgnoreCase);

        public static int? GetBuyProductionSourceId(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return null;

            foreach (var line in notes.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith(SourceIdPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (int.TryParse(
                        trimmed[SourceIdPrefix.Length..].Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var id)
                    && id > 0)
                    return id;
            }

            return null;
        }

        public static string SetBuyProductionSourceId(string? notes, int? sourceId)
        {
            var rest = (notes ?? string.Empty)
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l)
                    && !l.StartsWith(SourceIdPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (sourceId is int id && id > 0)
                rest.Insert(0, SourceIdPrefix + id.ToString(CultureInfo.InvariantCulture));

            return string.Join("\n", rest);
        }
    }
}
