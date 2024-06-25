using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Npgsql;
using OpenTelemetry.Trace;

namespace Altinn.AccessManagement.Persistence.Extensions;
#nullable enable
/// <summary>
/// Extension for Activity used in Altinn.AccessManagement
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
    public static void StopWithError(this Activity? activity, Exception ex, string? statusDescription = null)
    {
        if (activity?.Recorded ?? false)
        {
            activity.RecordException(ex);
            activity.SetStatus(ActivityStatusCode.Error, statusDescription);
        }
    }

    /// <summary>
    /// Sets status and records event
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="evnt">ActivityEvent to record</param>
    /// <param name="tags">Information to record</param>
    /// <param name="statusDescription">Optional status description (Default: event.Name)</param>
    public static void StopWithError(this Activity? activity, ActivityEvent evnt, Dictionary<string, string>? tags = null, string? statusDescription = null)
    {
        if (activity?.Recorded ?? false)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.AddTag(tag.Key, tag.Value);
                }
            }

            activity.AddEvent(evnt);
            activity.SetStatus(ActivityStatusCode.Error, statusDescription ?? evnt.Name);
        }
    }

    /// <summary>
    /// Sets status and records exception
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="statusDescription">Optional description/message for error</param>
    /// <param name="resultSize">Optional metric of resultsize</param>
    public static void StopOk(this Activity? activity, string? statusDescription = null, int? resultSize = null)
    {
        if (activity?.Recorded ?? false)
        {
            if (resultSize != null)
            {
                activity.SetTag("ResultSize", resultSize.Value);
            }

            activity.SetStatus(ActivityStatusCode.Ok, statusDescription);
        }
    }
}