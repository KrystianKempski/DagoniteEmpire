using System.Globalization;

namespace DA_Common.Barony
{
    /// <summary>Bieżąca kultura UI dla katalogów dwujęzycznych (PL/EN). EN = wartość domyślna/fallback.</summary>
    public static class BaronyCulture
    {
        public static bool IsPolish =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pl", StringComparison.OrdinalIgnoreCase);
    }
}
