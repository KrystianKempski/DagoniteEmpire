namespace DA_Common.Barony
{
    /// <summary>Hover copy for barony chrome (meta bar, resource HUD).</summary>
    public static class BaronyUiTooltips
    {
        public const string MetaBaronyName =
            "Formal name of your barony.";

        public const string MetaYear =
            "Campaign calendar year.";

        public const string MetaMonth =
            "Current month in the barony calendar.";

        public const string MetaSeason =
            "One turn equals one season. Current season of the year.";

        public const string MetaTurn =
            "Number of seasons elapsed since the barony began.";

        public const string MetaSize =
            "Terrain tiles assigned to this barony on the map.";

        public const string MetaUnrest =
            "Community unrest level. Feeds the Community section and penalizes Loyalty, Stability, Law, Economy, and Production.";

        public static string MetaConjuncture(int dice, int modifier)
        {
            var effective = dice + modifier;
            var modText = modifier == 0
                ? "no MG modifier"
                : $"MG modifier {(modifier > 0 ? "+" : "")}{modifier}";
            return
                $"Economic conjuncture this turn: {effective} (2d6 = {dice}, {modText}).\n"
                + "Rolled at turn start. Feeds Community Economy: "
                + $"(1) net Gold profit (Economy + Conjuncture) × {EconomyConjunctureFormulas.NetProfitGoldFactor:0}; "
                + "(2) (Conjuncture − 7) in the % formula on Gold, Production, Loyalty, Stability, Magic, Culture, Science, and Defense.";
        }

        public static string MetaPpbTurnTotal(Ppb key, decimal value)
        {
            var name = PpbCatalog.NameEnglish(key);
            var baseTip =
                $"Domain Panel grand total for {name} this turn: {PpbFormat.Additive(value)}.\n" +
                "Sum of additive modifiers from all sections (before percent scaling).";

            if (key == Ppb.Economy)
            {
                return baseTip + "\n"
                    + $"Community net Gold uses pre-Community Economy additive: "
                    + $"(Economy + Conjuncture) × {EconomyConjunctureFormulas.NetProfitGoldFactor:0}.";
            }

            return baseTip;
        }

        public static string ResourceHud(Ppb key, decimal stock, decimal delta)
        {
            var name = PpbCatalog.NameEnglish(key);
            var deltaText = PpbFormat.Round(delta) == 0m ? "+0" : PpbFormat.Additive(delta);
            var blurb = key switch
            {
                Ppb.Food => "Stored food supply carried between turns.",
                Ppb.Production => "Industrial and craft output accumulated as stock.",
                Ppb.Science => "Scientific progress accumulated between turns.",
                Ppb.Magic => "Magical resources accumulated between turns.",
                Ppb.Culture => "Cultural output accumulated between turns.",
                Ppb.Intelligence => "Intelligence assets accumulated between turns.",
                Ppb.Defense => "Defensive readiness accumulated between turns.",
                Ppb.Treasury => "Treasury gold carried between turns.",
                _ => "Cumulative resource stock.",
            };

            return
                $"{blurb}\n" +
                $"Current stock: {PpbFormat.Number(stock)}.\n" +
                $"Expected change this turn: {deltaText} (Domain Panel grand total).";
        }
    }
}
