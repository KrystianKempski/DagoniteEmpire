using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// MG-curated adventure pin on the Audience Hall “Adventures” ring.
    /// </summary>
    public class BaronyHallAdventure
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        /// <summary>Display name on the hall satellite.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>wwwroot-relative icon path (e.g. icons/bookmarklet.svg).</summary>
        public string IconPath { get; set; } = "icons/bookmarklet.svg";

        /// <summary>
        /// Destination entered by MG: chapter id, relative path (/chapter/12), or absolute URL.
        /// </summary>
        public string LinkUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
