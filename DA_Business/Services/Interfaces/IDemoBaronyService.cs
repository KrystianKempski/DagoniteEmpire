using System.Threading.Tasks;

namespace DA_Business.Services.Interfaces
{
    /// <summary>Result of provisioning a "Try baron" demo session.</summary>
    public record DemoSessionInfo(System.Guid Token, int CharacterId, int BaronyId);

    /// <summary>
    /// Provisions and tears down isolated, throwaway "Try baron" demo sessions.
    /// Each session clones the seeded Darkhold barony and its baron; everything the
    /// visitor creates lives only until the session is ended or swept (TTL).
    /// </summary>
    public interface IDemoBaronyService
    {
        /// <summary>Clones the demo baron + a fresh Darkhold barony and records a demo session.</summary>
        Task<DemoSessionInfo> CreateSessionAsync();

        /// <summary>Refreshes the session's last-seen timestamp (heartbeat) so it is not swept.</summary>
        Task TouchAsync(System.Guid token);

        /// <summary>Marks the session as leaving (near-expired) so it is swept promptly if the visitor does not return.</summary>
        Task MarkLeavingAsync(System.Guid token);

        /// <summary>Purges the session's barony, baron character and tracking row.</summary>
        Task EndSessionAsync(System.Guid token);

        /// <summary>Removes all demo sessions whose last-seen timestamp is older than <paramref name="ttl"/>. Returns count removed.</summary>
        Task<int> SweepExpiredAsync(System.TimeSpan ttl);

        /// <summary>True when the character belongs to an active demo session.</summary>
        Task<bool> IsDemoCharacterAsync(int characterId);

        /// <summary>True while the demo session identified by the token still exists.</summary>
        Task<bool> IsSessionActiveAsync(System.Guid token);
    }
}
