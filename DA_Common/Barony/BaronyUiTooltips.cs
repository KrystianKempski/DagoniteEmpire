using DA_Common.Localization;

namespace DA_Common.Barony
{
    /// <summary>Hover copy for barony chrome (meta bar, resource HUD).</summary>
    public static class BaronyUiTooltips
    {
        public static string MetaBaronyName =>
            Loc.T("Formal name of your barony.");

        public static string MetaBaronyNameMg =>
            Loc.T("Click to switch which barony you are managing.");

        public static string MetaYear =>
            Loc.T("Campaign calendar year.");

        public static string MetaMonth =>
            Loc.T("Current month in the barony calendar.");

        public static string MetaSeason =>
            Loc.T("One turn equals one season (Spring → Summer → Autumn → Winter). Farms produce no food in Winter — the barony lives off granary stocks.");

        public static string MetaTurn =>
            Loc.T("Number of seasons elapsed since the barony began.");

        public static string MetaSize =>
            Loc.T("Terrain tiles assigned to this barony on the map.");

        public static string MetaUnrest =>
            Loc.T("Community unrest level (0–5). Feeds the Community section and penalizes Loyalty, Stability, Law, Economy, and Production. MG can click to edit.");

        public static string MetaConjuncture(int dice, int modifier)
        {
            var effective = dice + modifier;
            var modText = modifier == 0
                ? Loc.T("no MG modifier")
                : Loc.T("MG modifier {0}", (modifier > 0 ? "+" : "") + modifier);
            return Loc.T("Economic conjuncture this turn: {0} (2d6 = {1}, {2}).", effective, dice, modText)
                + "\n"
                + Loc.T("Rolled at turn start. Feeds Community Economy: (1) net Gold profit (Economy + Conjuncture) × {0}; (2) (Conjuncture − 7) in the % formula on Gold, Production, Loyalty, Stability, Magic, Culture, Science, and Defense.", EconomyConjunctureFormulas.NetProfitGoldFactor.ToString("0"));
        }

        public static string MetaPpbTurnTotal(Ppb key, decimal value)
        {
            var name = PpbCatalog.Name(key);
            var baseTip = Loc.T("Domain Panel grand total for {0} this turn: {1}.", name, PpbFormat.Additive(value))
                + "\n"
                + Loc.T("Sum of additive modifiers from all sections (before percent scaling).");

            if (key == Ppb.Economy)
            {
                return baseTip + "\n" + Loc.T(
                    "Community net Gold uses Final Economy (after other Community rows): (Economy + Conjuncture) × {0}.",
                    EconomyConjunctureFormulas.NetProfitGoldFactor.ToString("0"));
            }

            return baseTip;
        }

        public static string ResourceHud(Ppb key, decimal stock, decimal delta)
        {
            var deltaText = PpbFormat.Round(delta) == 0m ? "+0" : PpbFormat.Additive(delta);
            var blurb = key switch
            {
                Ppb.Food => Loc.T("Stored food supply carried between turns."),
                Ppb.Production => Loc.T("Industrial and craft output accumulated as stock."),
                Ppb.Science => Loc.T("Scientific progress accumulated between turns."),
                Ppb.Magic => Loc.T("Magical resources accumulated between turns."),
                Ppb.Culture => Loc.T("Cultural output accumulated between turns."),
                Ppb.Intelligence => Loc.T("Intelligence assets accumulated between turns."),
                Ppb.Defense => Loc.T("Defensive readiness accumulated between turns."),
                Ppb.Treasury => Loc.T("Treasury gold carried between turns."),
                _ => Loc.T("Cumulative resource stock."),
            };

            return blurb
                + "\n" + Loc.T("Current stock: {0}.", PpbFormat.Number(stock))
                + "\n" + Loc.T("Expected change this turn: {0} (Domain Panel grand total).", deltaText);
        }
    }
}
