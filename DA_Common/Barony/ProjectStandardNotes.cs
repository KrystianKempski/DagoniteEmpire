using System;
using System.Collections.Generic;
using System.Linq;

namespace DA_Common.Barony
{
    /// <summary>Parses / updates the standard-project subtype line in project Notes.</summary>
    public static class ProjectStandardNotes
    {
        private const string Prefix = "StandardSubtype=";

        public static string? GetSubtype(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return null;

            foreach (var line in notes.Split('\n'))
            {
                if (line.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                    return line[Prefix.Length..].Trim();
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
    }
}
