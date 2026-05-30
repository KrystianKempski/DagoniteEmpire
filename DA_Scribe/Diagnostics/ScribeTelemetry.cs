using System.Diagnostics;

namespace DA_Scribe.Diagnostics
{
    /// <summary>
    /// Single ActivitySource for the entire Scribe stack. Consumed by OpenTelemetry
    /// (registered in Program.cs) when an OTLP endpoint is configured. When OTel is
    /// not wired up, all activities here are cheap no-ops.
    /// </summary>
    public static class ScribeTelemetry
    {
        public const string SourceName = "DagoniteEmpire.Scribe";

        public static readonly ActivitySource ActivitySource = new(SourceName);
    }
}
