using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.AccessManagement.Persistence.Configuration
{
    /// <summary>
    /// Config to be used for Telemetry in Altinn.AccessManagement.Persistence
    /// </summary>
    public static class TelemetryConfig
    {
        /// <summary>
        /// Used as source for the current project
        /// </summary>
        public static readonly ActivitySource _activitySource = new("Altinn.AccessManagement.Persistence");

        /// <summary>
        /// Used as source for the current project
        /// </summary>
        public static readonly Meter _meter = new("Altinn.AccessManagement.Persistence");
    }
}
