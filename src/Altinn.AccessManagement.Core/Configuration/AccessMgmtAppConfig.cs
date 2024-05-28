using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Altinn.AccessManagement.Core.Configuration;

/// <summary>
/// Telemetry configuration
/// </summary>
public class ConfigTelemetry
{
    /// <summary>
    /// Instructs Telemetry to write to console
    /// </summary>
    public bool WriteToConsole { get; set; }

    /// <summary>
    /// Telemetry will use AlwaysOnSampler
    /// </summary>
    public bool UseAlwaysOnSampler { get; set; }

    /// <summary>
    /// Connectionstring for exporting Telemetry to Azure Monitor / AppInsights
    /// </summary>
    public string AppInsightsConnectionString { get; set; }

    /// <summary>
    /// Endpoint to send telemetry data
    /// </summary>
    public string ScrapingEndpoint { get; set; }
}