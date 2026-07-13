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

        public int Unrest { get; set; }

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        /// <summary>Bazowe wartości PPB (przed modyfikatorami) — JSON PpbVector.</summary>
        public string BaseParametersJson { get; set; } = "{}";

        public string? Notes { get; set; }
    }
}
