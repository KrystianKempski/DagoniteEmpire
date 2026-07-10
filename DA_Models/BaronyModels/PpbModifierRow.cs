using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    /// <summary>
    /// Wiersz tabeli PPB na Panelu Domeny: nazwa modyfikatora + jego wpływ na PPB
    /// (addytywny i/lub procentowy) oraz opcjonalny tekst formuły do podglądu (hover).
    /// </summary>
    public sealed class PpbModifierRow
    {
        public string Label { get; set; } = string.Empty;

        public PpbVector Additive { get; set; } = new();

        /// <summary>Wartości procentowe w punktach procentowych (10 = +10%).</summary>
        public PpbVector Percent { get; set; } = new();

        /// <summary>Czytelny tekst formuły / źródła wartości (tooltip).</summary>
        public string? Formula { get; set; }

        public string? Note { get; set; }
    }
}
