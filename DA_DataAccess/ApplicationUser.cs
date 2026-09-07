using Microsoft.AspNetCore.Identity;

namespace DA_DataAccess
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public int SelectedCharacterId { get; set; }

        public bool ShowBadge { get; set; } = false;
        public int? BadgeContent { get; set; } = null;

        /// <summary>JSON array of barony tab keys (e.g. ["domain","resources",…]) for custom tab bar order.</summary>
        public string? BaronyTabOrderJson { get; set; }
    }
}
