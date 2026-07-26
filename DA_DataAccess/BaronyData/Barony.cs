using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// Baronia zarządzana przez postać typu Duke/Baron. Relacja 1:1 z Character (przez CharacterId).
    /// Encje potomne wskazują na baronię przez int BaronyId (bez nawigacji, wzorem Mob/CampaignId).
    /// Wektory PPB (baza) trzymane jako JSON (wzorem BattleMap.CellsJson).
    /// </summary>
    public class Barony
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Postać będąca baronem (1:1).</summary>
        public int CharacterId { get; set; }

        public string Name { get; set; } = "Nowa Baronia";

        /// <summary>Rozmiar baronii (liczba pól).</summary>
        public int Size { get; set; }

        // --- Stan sezonu / tury (BaronySeasonState, osadzony) ---
        public int Year { get; set; } = 625;
        public int Month { get; set; } = 1;
        public int TurnNumber { get; set; } = 1;
        public string Season { get; set; } = "Winter";

        // --- Akumulatory (przenoszą się między turami) ---
        /// <summary>Skarbiec baronii.</summary>
        public decimal TreasuryGold { get; set; }

        /// <summary>Kiesa barona (osobna od skarbca; baron może przelewać bez ograniczeń).</summary>
        public decimal BaronPurseGold { get; set; }

        /// <summary>Żywność w spichlerzach.</summary>
        public decimal FoodInGranaries { get; set; }

        /// <summary>Cumulative resource stocks (JSON PpbVector; Food/Gold mirrored with scalar fields).</summary>
        public string ResourceStocksJson { get; set; } = "{}";

        /// <summary>Income applied at end of the previous turn (JSON PpbVector).</summary>
        public string PreviousTurnIncomeJson { get; set; } = "{}";

        /// <summary>
        /// Stocks snapshot at Resolve Turn, before income and project grants (JSON PpbVector).
        /// Resource Balance “Stock from previous turn”.
        /// </summary>
        public string PreviousTurnStockJson { get; set; } = "{}";

        public int Unrest { get; set; }

        /// <summary>Raw 2d6 rolled at turn start for economic conjuncture (typically 2–12).</summary>
        public int ConjunctureDice { get; set; } = 7;

        /// <summary>MG-only modifier added to <see cref="ConjunctureDice"/> (war, harvest, etc.).</summary>
        public int ConjunctureModifier { get; set; }

        /// <summary>
        /// Share of gross gold income paid to the senior (Budget Fief expense). Default 15.
        /// </summary>
        public decimal LiegeTributePercent { get; set; } = 15m;

        /// <summary>
        /// Share of village gold on vassal fiefs kept by the baron. Default 15.
        /// </summary>
        public decimal VassalTributePercent { get; set; } = 15m;

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        /// <summary>Bazowe wartości PPB (przed modyfikatorami) — JSON PpbVector.</summary>
        public string BaseParametersJson { get; set; } = "{}";

        public string? Notes { get; set; }

        /// <summary>Player marked the current turn as finished; MG may resolve.</summary>
        public bool PlayerTurnReady { get; set; }
    }
}
