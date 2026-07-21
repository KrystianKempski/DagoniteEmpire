using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Diplomatic / feudal relation entry on the Relations tab (one person / contact).</summary>
    public class BaronyRelation
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Section category (<see cref="DA_Common.Barony.RelationCategory"/>).</summary>
        public string Category { get; set; } = DA_Common.Barony.RelationCategory.Acquaintances;

        /// <summary>House / organization name spanning multiple people in the UI.</summary>
        public string GroupName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TroopCount { get; set; }
        public string RelationDescription { get; set; } = string.Empty;

        /// <summary>Baron-editable personal notes.</summary>
        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        public List<BaronyRelationModifier> Modifiers { get; set; } = new();
    }

    /// <summary>MG-managed attitude delta for a relation entry.</summary>
    public class BaronyRelationModifier
    {
        [Key]
        public int Id { get; set; }

        public int RelationId { get; set; }

        [ForeignKey(nameof(RelationId))]
        public BaronyRelation? Relation { get; set; }

        public string Description { get; set; } = string.Empty;
        public int Value { get; set; }
        public int SortOrder { get; set; }
    }
}
