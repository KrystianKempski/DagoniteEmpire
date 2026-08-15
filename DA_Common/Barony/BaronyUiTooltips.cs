namespace DA_Common.Barony
{
    /// <summary>Hover copy for barony chrome (meta bar, resource HUD).</summary>
    public static class BaronyUiTooltips
    {
        public const string MetaBaronyName =
            "Formalna nazwa twojej baronii.";

        public const string MetaBaronyNameMg =
            "Kliknij, aby przełączyć zarządzaną baronię.";

        public const string MetaYear =
            "Rok kalendarzowy kampanii.";

        public const string MetaMonth =
            "Bieżący miesiąc w kalendarzu baronii.";

        public const string MetaSeason =
            "Jedna tura to jedna pora roku (Wiosna → Lato → Jesień → Zima). "
            + "Zimą farmy nie produkują żywności — baronia żyje z zapasów spichlerza.";

        public const string MetaTurn =
            "Liczba pór roku, które upłynęły od powstania baronii.";

        public const string MetaSize =
            "Pola terenu przypisane tej baronii na mapie.";

        public const string MetaUnrest =
            "Poziom niepokoju społeczności (0–5). Zasila sekcję Społeczność i obniża Lojalność, Stabilność, Prawo, Ekonomię oraz Produkcję. MG może kliknąć, aby edytować.";

        public static string MetaConjuncture(int dice, int modifier)
        {
            var effective = dice + modifier;
            var modText = modifier == 0
                ? "brak korekty MG"
                : $"korekta MG {(modifier > 0 ? "+" : "")}{modifier}";
            return
                $"Koniunktura gospodarcza w tej turze: {effective} (2d6 = {dice}, {modText}).\n"
                + "Rzucane na początku tury. Zasila Ekonomię Społeczności: "
                + $"(1) zysk netto w Złocie (Ekonomia + Koniunktura) × {EconomyConjunctureFormulas.NetProfitGoldFactor:0}; "
                + "(2) (Koniunktura − 7) we wzorze % dla Złota, Produkcji, Lojalności, Stabilności, Magii, Kultury, Nauki i Obrony.";
        }

        public static string MetaPpbTurnTotal(Ppb key, decimal value)
        {
            var name = PpbCatalog.NameEnglish(key);
            var baseTip =
                $"Suma całkowita Panelu Domeny dla {name} w tej turze: {PpbFormat.Additive(value)}.\n" +
                "Suma modyfikatorów addytywnych ze wszystkich sekcji (przed skalowaniem procentowym).";

            if (key == Ppb.Economy)
            {
                return baseTip + "\n"
                    + $"Złoto netto Społeczności korzysta z Ekonomii Końcowej (po pozostałych wierszach Społeczności): "
                    + $"(Ekonomia + Koniunktura) × {EconomyConjunctureFormulas.NetProfitGoldFactor:0}.";
            }

            return baseTip;
        }

        public static string ResourceHud(Ppb key, decimal stock, decimal delta)
        {
            var name = PpbCatalog.NameEnglish(key);
            var deltaText = PpbFormat.Round(delta) == 0m ? "+0" : PpbFormat.Additive(delta);
            var blurb = key switch
            {
                Ppb.Food => "Zapas żywności przenoszony między turami.",
                Ppb.Production => "Produkcja przemysłowa i rzemieślnicza gromadzona jako zapas.",
                Ppb.Science => "Postęp naukowy gromadzony między turami.",
                Ppb.Magic => "Zasoby magiczne gromadzone między turami.",
                Ppb.Culture => "Dorobek kulturalny gromadzony między turami.",
                Ppb.Intelligence => "Zasoby wywiadowcze gromadzone między turami.",
                Ppb.Defense => "Gotowość obronna gromadzona między turami.",
                Ppb.Treasury => "Złoto skarbca przenoszone między turami.",
                _ => "Skumulowany zapas zasobu.",
            };

            return
                $"{blurb}\n" +
                $"Bieżący zapas: {PpbFormat.Number(stock)}.\n" +
                $"Oczekiwana zmiana w tej turze: {deltaText} (suma całkowita Panelu Domeny).";
        }
    }
}
