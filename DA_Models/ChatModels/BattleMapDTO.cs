using System.Collections.Generic;

namespace DA_Models.ChatModels
{
    public class BattleMapDTO
    {
        public BattleMapDTO() { }

        public BattleMapDTO(int campaignId, int chapterId)
        {
            CampaignId = campaignId;
            ChapterId = chapterId;
        }

        public int Id { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int Width { get; set; } = 10;
        public int Height { get; set; } = 10;

        /// <summary>Cells that carry a background color and/or centered text.</summary>
        public List<BattleMapCellDTO> Cells { get; set; } = new();

        /// <summary>Movable markers placed on the grid.</summary>
        public List<BattleMapTokenDTO> Tokens { get; set; } = new();
    }

    public class BattleMapCellDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string? Color { get; set; }
        public string? Text { get; set; }
        public string? TextColor { get; set; }

        /// <summary>Terrain type: null/empty = normal, "difficult" = x2 move cost, "impassable" = blocks movement.</summary>
        public string? Terrain { get; set; }
    }

    public class BattleMapTokenDTO
    {
        public string Id { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Color { get; set; } = "#1976d2";
        public string? IconUrl { get; set; }
        public bool IsMob { get; set; }

        /// <summary>Movement budget in cells per turn (enriched from participants).</summary>
        public int Range { get; set; }

        /// <summary>Whether this token is an ally (player character or Ally mob).</summary>
        public bool IsAlly { get; set; }
    }

    public class BattleMapParticipantDTO
    {
        public string Label { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public bool IsMob { get; set; }

        /// <summary>Movement budget in cells per turn.</summary>
        public int Range { get; set; }

        /// <summary>Whether this participant is an ally (player character or Ally mob).</summary>
        public bool IsAlly { get; set; }
    }
}
