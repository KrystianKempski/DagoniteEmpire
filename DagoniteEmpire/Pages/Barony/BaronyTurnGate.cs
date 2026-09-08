namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Tabs the Game Master writes up between Resolve Turn and Finish Resolving
    /// (audiences, hall events, project outcomes, resource grants, letters).
    /// The baron is kept out of them so he cannot act on a half-written turn.
    /// </summary>
    public static class BaronyTurnGate
    {
        /// <summary>Tab keys from <c>BaronyCardTabs</c> that are closed while the MG resolves.</summary>
        public static readonly string[] LockedTabKeys =
        {
            "audience-hall",
            "audiences",
            "projects",
            "resources",
            "letters",
        };

        public static bool IsLockedTab(string tabKey) =>
            Array.IndexOf(LockedTabKeys, tabKey) >= 0;

        /// <summary>Budget is a redirect onto the Resources page, so it is gated with it.</summary>
        public static bool IsLockedPath(string path) => path switch
        {
            "/barony/audience-hall" or "/barony/audiences" or "/barony/projects"
                or "/barony/resources" or "/barony/budget" or "/barony/letters" => true,
            _ => false,
        };
    }
}
