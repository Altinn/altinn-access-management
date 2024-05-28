using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Altinn.AccessManagement.Core.Configuration;

/// <summary>
/// Configuration for Access Management API
/// </summary>
public class AccessMgmtAppConfig
{
    /// <summary>
    /// Telemetry configuration
    /// </summary>
    public ConfigTelemetry Telemetry { get; set; }

    /// <summary>
    /// SblBridge configuration
    /// </summary>
    public ConfigSblBridge SblBridge { get; set; }
    
    /// <summary>
    /// Platform configuration
    /// </summary>
    public PlatformSettingsNEW Platform { get; set; }

    public ConfigDatabase Database { get; set; }
}

/// <summary>
/// General configuration settings
/// </summary>
public class PlatformSettingsNEW
{
    /// <summary>
    /// Open Id Connect Well known endpoint
    /// </summary>
    public string? OpenIdWellKnownEndpoint { get; set; }

    /// <summary>
    /// Name of the cookie for where JWT is stored
    /// </summary>
    public string? JwtCookieName { get; set; }

    /// <summary>
    /// Gets or sets the profile api endpoint.
    /// </summary>
    public string? ApiProfileEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the register api endpoint.
    /// </summary>
    public string? ApiRegisterEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the resource registry api endpoint.
    /// </summary>
    public string? ApiResourceRegistryEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the subscriptionkey.
    /// </summary>
    public string? SubscriptionKey { get; set; }

    /// <summary>
    /// Gets or sets the SubscriptionKeyHeaderName
    /// </summary>
    public string? SubscriptionKeyHeaderName { get; set; }

    /// <summary>
    /// Endpoint for authentication
    /// </summary>
    public string? ApiAuthenticationEndpoint { get; set; }

    /// <summary>
    /// Altinn Authorization base url
    /// </summary>
    public string? ApiAuthorizationEndpoint { get; set; }
}

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

/// <summary>
/// SblBridge Configuration
/// </summary>
public class ConfigSblBridge
{
    /// <summary>
    /// Base Url for SblBridge API
    /// </summary>
    public string BaseApiUrl { get; set; }
}

/// <summary>
/// Enum for the environments used for Azure AccessMgmtAppConfig labels
/// </summary>
public enum ConfigEnvironment
{
    Local,
    AT21,
    AT22,
    AT23,
    AT24
}


/// <summary>
/// Settings for Postgres database
/// </summary>
public class ConfigDatabase
{
    /// <summary>
    /// Connection string for the postgres db
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Password for app user for the postgres db
    /// </summary>
    public string AuthorizationDbPwd { get; set; }
}