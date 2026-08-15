namespace DA_Common.Barony
{
    /// <summary>Prestige / Honor / Fear visual identity (icons + CSS accent classes).</summary>
    public enum PhpMetric
    {
        Prestige,
        Honor,
        Fear,
    }

    public static class BaronPhpVisuals
    {
        public const string PrestigeIconUrl = "/icons/jewel-crown.svg";
        public const string HonorIconUrl = "/icons/justice.svg";
        public const string FearIconUrl = "/icons/fist.svg";

        public static string Name(PhpMetric metric) => metric switch
        {
            PhpMetric.Prestige => "Prestiż",
            PhpMetric.Honor => "Honor",
            PhpMetric.Fear => "Postrach",
            _ => metric.ToString(),
        };

        public static string IconUrl(PhpMetric metric) => metric switch
        {
            PhpMetric.Prestige => PrestigeIconUrl,
            PhpMetric.Honor => HonorIconUrl,
            PhpMetric.Fear => FearIconUrl,
            _ => PrestigeIconUrl,
        };

        /// <summary>CSS modifier class, e.g. <c>php-metric--prestige</c>.</summary>
        public static string CssClass(PhpMetric metric) => metric switch
        {
            PhpMetric.Prestige => "php-metric--prestige",
            PhpMetric.Honor => "php-metric--honor",
            PhpMetric.Fear => "php-metric--fear",
            _ => "php-metric--prestige",
        };
    }
}
