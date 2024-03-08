using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Npgsql;
using OpenTelemetry.Trace;

namespace Altinn.AccessManagement.Persistence.Extensions;
#nullable enable
/// <summary>
/// Extension for Activity used in Altinn.AccessManagement.Persistence
/// </summary>
[ExcludeFromCodeCoverage]
public static class ActivityExtensions
{
    /// <summary>
    /// Sets status and records exception
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="ex">Exception to record</param>
    /// <param name="statusDescription">Optional description/message for error</param>
    public static void ErrorWithException(this Activity? activity, Exception ex, string? statusDescription = null)
    {
        activity?.RecordException(ex);
        activity?.SetStatus(ActivityStatusCode.Error, statusDescription);
    }

    /// <summary>
    /// Sets status and records exception
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="statusDescription">Optional description/message for error</param>
    /// <param name="resultSize">Optional metric of resultsize</param>
    public static void FinishedOk(this Activity? activity, string? statusDescription = null, int? resultSize = null)
    {
        if (resultSize != null) 
        {
            activity?.SetTag("ResultSize", resultSize.Value);
        }

        activity?.SetStatus(ActivityStatusCode.Ok, statusDescription);
    }
}
