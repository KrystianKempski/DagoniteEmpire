using Microsoft.Extensions.Localization;

namespace DA_Common.Localization;

/// <summary>
/// Static bridge to the application's shared <see cref="IStringLocalizer"/> for backend
/// class libraries (static helpers, catalogs, and non-DI models such as the combat engine)
/// that cannot use constructor injection.
/// <para>
/// Configured once at web startup via <see cref="Configure"/>. The resource KEY is the English
/// source text; when unconfigured (e.g. unit tests, design-time) it safely returns the English
/// key (or the key formatted with the given arguments) so English stays the built-in fallback.
/// Culture is resolved per call by the underlying localizer via <c>CultureInfo.CurrentUICulture</c>.
/// </para>
/// </summary>
public static class Loc
{
    private static IStringLocalizer? _localizer;

    /// <summary>Wires the shared localizer. Call once during application startup.</summary>
    public static void Configure(IStringLocalizer localizer) => _localizer = localizer;

    /// <summary>Localized text for the given English key (falls back to the key itself).</summary>
    public static string T(string key)
        => _localizer is null ? key : _localizer[key].Value;

    /// <summary>Localized, formatted text (falls back to formatting the English key).</summary>
    public static string T(string key, params object[] args)
        => _localizer is null ? SafeFormat(key, args) : _localizer[key, args].Value;

    private static string SafeFormat(string format, object[] args)
    {
        try { return string.Format(format, args); }
        catch (FormatException) { return format; }
    }
}
