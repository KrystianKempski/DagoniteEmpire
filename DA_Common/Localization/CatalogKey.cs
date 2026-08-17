using System.Collections.Generic;
using System.Text;

namespace DA_Common.Localization;

/// <summary>
/// Fold + resolve helpers so catalog Canonical() methods accept English keys and Polish aliases.
/// </summary>
public static class CatalogKey
{
    public static string Fold(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        var prevSpace = false;
        foreach (var c in value.Trim().ToLowerInvariant())
        {
            var mapped = c switch
            {
                'ą' => 'a',
                'ć' => 'c',
                'ę' => 'e',
                'ł' => 'l',
                'ń' => 'n',
                'ó' => 'o',
                'ś' => 's',
                'ź' or 'ż' => 'z',
                _ => c
            };
            if (char.IsWhiteSpace(mapped))
            {
                if (prevSpace)
                    continue;
                sb.Append(' ');
                prevSpace = true;
            }
            else
            {
                prevSpace = false;
                sb.Append(mapped);
            }
        }
        return sb.ToString();
    }

    public static Dictionary<string, string> BuildMap(
        IEnumerable<string> canonicalKeys,
        IReadOnlyDictionary<string, string>? aliases = null)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in canonicalKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;
            map[Fold(key)] = key;
        }
        if (aliases is not null)
        {
            foreach (var kv in aliases)
            {
                if (string.IsNullOrWhiteSpace(kv.Key) || string.IsNullOrWhiteSpace(kv.Value))
                    continue;
                map[Fold(kv.Key)] = kv.Value;
            }
        }
        return map;
    }

    public static string Resolve(string? name, IReadOnlyDictionary<string, string> map)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var trimmed = name.Trim();
        return map.TryGetValue(Fold(trimmed), out var key) ? key : trimmed;
    }
}
