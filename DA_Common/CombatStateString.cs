using System;
using System.Collections.Generic;
using System.Linq;

namespace DA_Common
{
    /// <summary>A single combat state and how many turns it still lasts.</summary>
    public readonly record struct CombatStateEntry(string Name, int Duration);

    /// <summary>
    /// Single source of truth for the "Name:duration, " combat-state string format used on
    /// mobs (<c>MobDTO.States</c>) and character temporary-trait snapshots (<c>AllParamsModel.States</c>).
    /// Reading tolerates both "," and ", " separators plus stray whitespace; writing always emits
    /// the canonical ", "-separated form with a trailing separator so downstream parsers stay happy.
    /// </summary>
    public static class CombatStateString
    {
        public const string Separator = ", ";

        /// <summary>Parses a state string into ordered entries, skipping malformed tokens.</summary>
        public static List<CombatStateEntry> Parse(string? states)
        {
            var result = new List<CombatStateEntry>();
            if (string.IsNullOrWhiteSpace(states))
                return result;

            foreach (var token in states.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = token.Split(':', 2);
                if (parts.Length < 2)
                    continue;

                var name = parts[0].Trim();
                if (name.Length == 0 || !int.TryParse(parts[1].Trim(), out var duration))
                    continue;

                result.Add(new CombatStateEntry(name, duration));
            }

            return result;
        }

        /// <summary>Serializes entries back into the canonical "Name:duration, " string (empty when none).</summary>
        public static string Format(IEnumerable<CombatStateEntry>? entries)
        {
            if (entries is null)
                return string.Empty;

            var parts = entries
                .Where(e => !string.IsNullOrEmpty(e.Name))
                .Select(e => $"{e.Name}:{e.Duration}")
                .ToList();

            return parts.Count == 0 ? string.Empty : string.Join(Separator, parts) + Separator;
        }

        public static bool TryGetDuration(string? states, string name, out int duration)
        {
            foreach (var entry in Parse(states))
            {
                if (string.Equals(entry.Name, name, StringComparison.Ordinal))
                {
                    duration = entry.Duration;
                    return true;
                }
            }

            duration = 0;
            return false;
        }

        public static bool HasState(string? states, string name) =>
            TryGetDuration(states, name, out _);

        /// <summary>
        /// Upserts every state from <paramref name="incoming"/> into <paramref name="existing"/>
        /// (last duration wins, insertion order preserved). Adding "No turn" clears a pending "Half turn".
        /// </summary>
        public static string Merge(string? existing, string? incoming)
        {
            var ordered = new List<CombatStateEntry>();
            var index = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var entry in Parse(existing))
                Upsert(ordered, index, entry);
            foreach (var entry in Parse(incoming))
                Upsert(ordered, index, entry);

            return Format(ordered);
        }

        /// <summary>Upserts a single state, applying the same merge rules (dedupe, "No turn" clears "Half turn").</summary>
        public static string Add(string? states, string name, int duration) =>
            Merge(states, Format(new[] { new CombatStateEntry(name, duration) }));

        /// <summary>Decrements every state's remaining turns by one and drops those that reach zero.</summary>
        public static string DecrementTurn(string? states)
        {
            var survivors = Parse(states)
                .Select(e => e with { Duration = e.Duration - 1 })
                .Where(e => e.Duration > 0);

            return Format(survivors);
        }

        private static void Upsert(List<CombatStateEntry> ordered, Dictionary<string, int> index, CombatStateEntry entry)
        {
            if (string.Equals(entry.Name, States.Names.NoTurn, StringComparison.Ordinal)
                && index.TryGetValue(States.Names.HalfTurn, out var halfIndex))
            {
                ordered.RemoveAt(halfIndex);
                RebuildIndex(ordered, index);
            }

            if (index.TryGetValue(entry.Name, out var existingIndex))
            {
                ordered[existingIndex] = entry;
            }
            else
            {
                index[entry.Name] = ordered.Count;
                ordered.Add(entry);
            }
        }

        private static void RebuildIndex(List<CombatStateEntry> ordered, Dictionary<string, int> index)
        {
            index.Clear();
            for (var i = 0; i < ordered.Count; i++)
                index[ordered[i].Name] = i;
        }
    }
}
