using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.AccessManagement.Persistence.Configuration
{
    /// <summary>
    /// Config to be used for Telemetry in Altinn.AccessManagement.Persistence
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TelemetryConfig
    {
        /// <summary>
        /// Used as source for the current project
        /// </summary>
        public static readonly ActivitySource ActivitySource = new("Altinn.AccessManagement.Persistence");
    }
}
