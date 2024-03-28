using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Altinn.AccessManagement;

/// <summary>
/// Contains a set of extensions methods for the web application builder
/// </summary>
public static partial class Startup
{
    private static readonly ILogger Logger = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("*", LogLevel.Debug)
            .AddConsole();
    }).CreateLogger(nameof(Startup));

    /// <summary>
    /// Configures the logger and exportation of logs
    /// </summary>
    /// <param name="builder">web application builder</param>
    /// <param name="applicationInsightsConnectionString">application insights connection string</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddLogger(this WebApplicationBuilder builder, string applicationInsightsConnectionString)
    {
        Log.AddLogger(Logger);
        builder.Logging.AddOpenTelemetry(builder =>
        {
            if (string.IsNullOrEmpty(applicationInsightsConnectionString))
            {
                Log.WarningMissingAIConnectionString(Logger);
            }
            else
            {
                builder.AddAzureMonitorLogExporter(azure => azure.ConnectionString = applicationInsightsConnectionString);
            }
        })
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning);

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry and exportation of data
    /// </summary>
    /// <param name="builder">web application builder</param>
    /// <param name="applicationInsightsConnectionString">application insights connection string</param>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder, string applicationInsightsConnectionString)
    {
        Log.AddOpenTelemetry(Logger);

        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
        var telemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService("access-management");
            })
            .WithMetrics(metrics =>
            {
                metrics.AddMeter()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource()
                    .AddSource("Azure.*")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            if (!builder.Environment.IsProduction())
                            {
                                activity.AddTag("Request Body", request?.Content?.ReadAsStringAsync().Result);
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            if (!builder.Environment.IsProduction())
                            {
                                activity.AddTag("Response Body", response?.Content?.ReadAsStringAsync()?.Result);
                            }
                        };
                    });
            });

        if (string.IsNullOrEmpty(applicationInsightsConnectionString))
        {
            telemetryBuilder.UseAzureMonitor(azure => azure.ConnectionString = applicationInsightsConnectionString);
        }
        else
        {
            Log.WarningMissingAIConnectionString(Logger);
        }

        return builder;
    }

    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "Missing application insights connection string")]
        internal static partial void WarningMissingAIConnectionString(ILogger logger);

        [LoggerMessage(1, LogLevel.Debug, "Add logger to service collection")]
        internal static partial void AddLogger(ILogger logger);

        [LoggerMessage(2, LogLevel.Debug, "Add OpenTelemetry to service collection")]
        internal static partial void AddOpenTelemetry(ILogger logger);
    }
}