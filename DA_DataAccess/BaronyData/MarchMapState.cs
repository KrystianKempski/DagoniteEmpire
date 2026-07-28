using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Global Eastern March map overlay (lords, cities, trade routes). Single row <see cref="GlobalId"/>.</summary>
    public class MarchMapState
    {
        public const int GlobalId = 1;

        [Key]
        public int Id { get; set; }

        public string PayloadJson { get; set; } = "{}";
    }
}
