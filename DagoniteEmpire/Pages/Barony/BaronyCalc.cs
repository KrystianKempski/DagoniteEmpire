using DA_Common.Barony;
using DA_Models.BaronyModels;

namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Pomocnicze przeliczenia dla Panelu Domeny: budowa wierszy tabel PPB
    /// oraz sumy sekcji i podsumowanie całościowe.
    /// Uwaga: dokładne formuły dojdą później — tu stosujemy wstępny wzór
    /// (baza + Σ addytywne) * (1 + Σ procent/100).
    /// </summary>
    public static class BaronyCalc
    {
        public static PpbModifierRow Row(string label, PpbVector additive, PpbVector percent, string? formula = null, string? note = null)
            => new() { Label = label, Additive = additive ?? new PpbVector(), Percent = percent ?? new PpbVector(), Formula = formula, Note = note };

        public static List<PpbModifierRow> AdvisorRows(IEnumerable<AdvisorDTO> advisors)
            => advisors.Select(a => Row(
                    string.IsNullOrWhiteSpace(a.PersonName) ? a.Title : $"{a.Title} — {a.PersonName}",
                    a.Additive, a.Percent, a.FormulaText, a.Description))
                .ToList();

        public static List<PpbModifierRow> BuildingRows(IEnumerable<BaronyBuildingDTO> buildings)
            => buildings.Select(b => Row(b.Name, b.Additive, b.Percent, null, b.Description)).ToList();

        public static List<PpbModifierRow> SocialRows(IEnumerable<SocialGroupRelationDTO> relations)
            => relations.Select(r => Row(
                    $"{r.Group} ({RelationLevel.Name(r.RelationLevel)})",
                    r.Additive, r.Percent, r.FormulaText))
                .ToList();

        public static List<PpbModifierRow> ImprovementRows(IEnumerable<TerrainImprovementDTO> improvements)
            => improvements.Select(i => Row(i.Name, i.Additive, i.Percent, i.FormulaText, i.Description)).ToList();

        public static List<PpbModifierRow> DecreeRows(IEnumerable<DecreeDTO> decrees)
            => decrees.Select(d => Row(d.Name, d.Additive, d.Percent, d.FormulaText, d.Description)).ToList();

        public static List<PpbModifierRow> EventRows(IEnumerable<BaronyEventDTO> events)
            => events.Where(e => e.IsActive).Select(e => Row(e.Name, e.Additive, e.Percent, null, e.Description)).ToList();

        public static List<PpbModifierRow> CommunityRows(IEnumerable<CommunityModifierDTO> mods)
            => mods.Select(m => Row(m.Source, m.Additive, m.Percent, m.FormulaText)).ToList();

        /// <summary>Suma addytywna wierszy (do sumowania sekcji).</summary>
        public static PpbVector SumAdditive(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Additive));

        /// <summary>Suma procentowa wierszy.</summary>
        public static PpbVector SumPercent(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Percent));

        /// <summary>
        /// Uproszczony wektor "podglądu sekcji" do chipów w zwiniętym nagłówku:
        /// suma addytywna + suma procentowa (czysto informacyjnie).
        /// </summary>
        public static PpbVector SectionGlance(IEnumerable<PpbModifierRow> rows)
        {
            var list = rows.ToList();
            var glance = SumAdditive(list);
            glance.AddInPlace(SumPercent(list));
            return glance;
        }

        /// <summary>Podsumowanie całościowe wszystkich sekcji względem bazy PPB.</summary>
        public static PpbVector GrandTotal(BaronyOverviewDTO ov)
        {
            var allRows = new List<PpbModifierRow>();
            allRows.AddRange(AdvisorRows(ov.Advisors));
            allRows.AddRange(BuildingRows(ov.Buildings));
            allRows.AddRange(SocialRows(ov.SocialRelations));
            allRows.AddRange(ImprovementRows(ov.Improvements));
            allRows.AddRange(DecreeRows(ov.Decrees));
            allRows.AddRange(EventRows(ov.Events));
            allRows.AddRange(CommunityRows(ov.CommunityModifiers));

            var additive = SumAdditive(allRows);
            var percent = SumPercent(allRows);
            return PpbMath.Summarize(ov.Barony.BaseParameters, additive, percent);
        }
    }
}
