namespace DA_Common.Barony
{
    /// <summary>
    /// Deterministic icon assignment for advisors on the Audience Hall page.
    /// Core offices get a fixed icon; custom advisors get a stable pick from a small pool
    /// keyed by their id, so the same person always shows the same icon. No portraits yet
    /// (could later be replaced by a per-advisor PortraitUrl field).
    /// </summary>
    public static class AdvisorIconCatalog
    {
        public const string Baron = "icons/Throne-of-medieval-lord-with-vertical-banners.svg";
        public const string Chancellor = "icons/jeweled-chalice.svg";
        public const string GuardCaptain = "icons/elf-helmet.svg";
        public const string Steward = "icons/abacus.svg";

        private static readonly string[] CustomPool =
        {
            "icons/quill-ink.svg",
            "icons/people.svg",
            "icons/hunting-horn-black.svg",
            "icons/lyre.svg",
            "icons/herbs-bundle.svg",
            "icons/compass.svg",
        };

        /// <summary>Icon path (relative to wwwroot) for an office holder.</summary>
        public static string IconFor(string officeType, int stableId) => officeType switch
        {
            OfficeType.Baron => Baron,
            OfficeType.Chancellor => Chancellor,
            OfficeType.GuardCaptain => GuardCaptain,
            OfficeType.Steward => Steward,
            _ => CustomPool[Math.Abs(stableId) % CustomPool.Length],
        };
    }
}
