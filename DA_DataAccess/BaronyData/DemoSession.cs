using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// Tracks a throwaway "Try baron" demo session: a per-visitor cloned Darkhold barony and
    /// its baron character. All rows referenced here are disposable and purged on exit / TTL.
    /// </summary>
    public class DemoSession
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Opaque token handed to the browser (ProtectedSessionStorage) to identify this demo session.</summary>
        public Guid Token { get; set; }

        /// <summary>Cloned baron character owning the demo barony.</summary>
        public int CharacterId { get; set; }

        /// <summary>Cloned Darkhold barony for this session.</summary>
        public int BaronyId { get; set; }

        public DateTime CreatedUtc { get; set; }

        /// <summary>Updated by client heartbeat; drives TTL sweeping of abandoned sessions.</summary>
        public DateTime LastSeenUtc { get; set; }
    }
}
