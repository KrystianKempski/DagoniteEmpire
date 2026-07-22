using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Baron's single seat of power (one per barony).</summary>
    public class BaronySeat
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        public string Name { get; set; } = "Lord's Seat";

        public int GridWidth { get; set; } = 12;

        public int GridHeight { get; set; } = 8;

        public List<SeatRoom> Rooms { get; set; } = new();
    }

    /// <summary>Physical chamber within the lord's seat.</summary>
    public class SeatRoom
    {
        [Key]
        public int Id { get; set; }

        public int SeatId { get; set; }

        [ForeignKey(nameof(SeatId))]
        public BaronySeat? Seat { get; set; }

        public string Name { get; set; } = string.Empty;

        public int GridX { get; set; }

        public int GridY { get; set; }

        public int GridW { get; set; } = 1;

        public int GridH { get; set; } = 1;

        /// <see cref="DA_Common.Barony.SeatRoomMaterial"/>
        public string Material { get; set; } = DA_Common.Barony.SeatRoomMaterial.Stone;

        public decimal PrestigeMultiplier { get; set; } = 1m;

        /// <see cref="DA_Common.Barony.SeatRoomStatus"/>
        public string Status { get; set; } = DA_Common.Barony.SeatRoomStatus.Active;

        public string AdditiveJson { get; set; } = string.Empty;

        public string PercentJson { get; set; } = string.Empty;

        public int? PurposeTemplateId { get; set; }

        /// <summary>Optional advisor or baron assigned to this chamber.</summary>
        public int? OccupantAdvisorId { get; set; }

        /// <summary>Optional free-text occupant when not chosen from advisors.</summary>
        public string OccupantCustom { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public List<SeatRoomTrait> Traits { get; set; } = new();
    }

    /// <summary>Free-text advantage or disadvantage on a room.</summary>
    public class SeatRoomTrait
    {
        [Key]
        public int Id { get; set; }

        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public SeatRoom? Room { get; set; }

        /// <see cref="DA_Common.Barony.SeatRoomTraitKind"/>
        public string Kind { get; set; } = DA_Common.Barony.SeatRoomTraitKind.Advantage;

        public string Text { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Reusable room purpose template (global or barony-specific).</summary>
    public class SeatPurposeTemplate
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <see cref="DA_Common.Barony.SeatRoomSizeCategory"/>
        public string MinSizeCategory { get; set; } = DA_Common.Barony.SeatRoomSizeCategory.Small;

        public string WhoOccupies { get; set; } = string.Empty;

        public int SleepCapacity { get; set; }

        /// <summary>Additive prestige from purpose (multiplied by room prestige later).</summary>
        public decimal AdditivePrestige { get; set; }

        public string AdditiveJson { get; set; } = string.Empty;

        public string PercentJson { get; set; } = string.Empty;

        /// <summary>When true, available to every barony; otherwise scoped to <see cref="BaronyId"/>.</summary>
        public bool IsUniversal { get; set; } = true;

        public int? BaronyId { get; set; }

        public int SortOrder { get; set; }
    }
}
