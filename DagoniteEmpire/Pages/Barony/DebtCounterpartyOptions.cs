using DA_Common.Barony;
using DA_Models.BaronyModels;
using Microsoft.Extensions.Localization;

namespace DagoniteEmpire.Pages.Barony;

/// <summary>Lords + barony relations for debt counterparty pickers.</summary>
public static class DebtCounterpartyOptions
{
    public sealed record Option(string Key, string Label, string CounterpartyName);

    public static List<Option> Build(IEnumerable<BaronyRelationDTO> relations, IStringLocalizer? localizer = null)
    {
        var options = new List<Option>();
        var coveredLordKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rel in relations.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
        {
            var lordKey = KnownLordsCatalog.MatchLordKeyFromRelationName(rel.Name);
            if (lordKey is not null)
                coveredLordKeys.Add(lordKey);

            var label = FormatRelationLabel(rel);
            options.Add(new Option($"relation:{rel.Id}", label, label));
        }

        foreach (var lord in KnownLordsCatalog.EasternMarch.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase))
        {
            var lordKey = KnownLordsCatalog.LordKey(lord);
            if (coveredLordKeys.Contains(lordKey))
                continue;

            var label = lord.DisplayFullName(localizer);
            options.Add(new Option($"lord:{lordKey}", label, label));
        }

        return options
            .OrderBy(o => o.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static Option? MatchByCounterpartyName(IEnumerable<Option> options, string? counterpartyName)
    {
        if (string.IsNullOrWhiteSpace(counterpartyName))
            return null;

        return options.FirstOrDefault(o =>
                   string.Equals(o.CounterpartyName, counterpartyName, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(o.Label, counterpartyName, StringComparison.OrdinalIgnoreCase))
               ?? options.FirstOrDefault(o =>
                   o.Label.Contains(counterpartyName, StringComparison.OrdinalIgnoreCase)
                   || counterpartyName.Contains(o.CounterpartyName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Option> Search(IEnumerable<Option> options, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return options;

        return options.Where(o =>
            o.Label.Contains(text, StringComparison.OrdinalIgnoreCase)
            || o.CounterpartyName.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatRelationLabel(BaronyRelationDTO rel) =>
        string.IsNullOrWhiteSpace(rel.Title) ? rel.Name : $"{rel.Name} — {rel.Title}";
}
