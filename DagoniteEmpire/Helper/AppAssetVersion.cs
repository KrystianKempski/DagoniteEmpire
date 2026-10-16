using System.Reflection;

namespace DagoniteEmpire.Helper;

/// <summary>
/// Cache-bust token for local static assets (CSS/JS/icons). Changes on every publish/rebuild
/// so browsers fetch fresh files after a deploy without manual <c>?v=</c> bumps.
/// </summary>
public static class AppAssetVersion
{
    public static string Current { get; } = Resolve();

    private static string Resolve()
    {
        var asm = typeof(AppAssetVersion).Assembly;

        // Prefer git commit from InformationalVersion (SDK default: 1.0.0+abcdef…) when available.
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            var plus = info.IndexOf('+');
            if (plus >= 0 && plus < info.Length - 1)
            {
                var rev = info[(plus + 1)..];
                var cut = rev.IndexOfAny(['.', ' ', '\t']);
                if (cut > 0)
                    rev = rev[..cut];
                rev = Sanitize(rev);
                if (rev.Length >= 7)
                    return rev.Length > 12 ? rev[..12] : rev;
            }
        }

        // Fallback: DLL write time — always changes on docker publish even without .git in the image.
        try
        {
            var path = asm.Location;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return File.GetLastWriteTimeUtc(path).ToString("yyyyMMddHHmmss");
        }
        catch
        {
            // ignore
        }

        return "dev";
    }

    private static string Sanitize(string value)
    {
        Span<char> buf = stackalloc char[value.Length];
        var n = 0;
        foreach (var c in value)
        {
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
                buf[n++] = c;
        }
        return n == 0 ? string.Empty : new string(buf[..n]);
    }
}
