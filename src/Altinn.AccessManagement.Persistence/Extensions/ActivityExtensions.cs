using System.Collections;
using System.Diagnostics;
using Npgsql;
using OpenTelemetry.Trace;

namespace Altinn.AccessManagement.Persistence.Extensions;

/// <summary>
/// Extension for Activity used in Altinn.AccessManagement.Persistence
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Sets Activity tags based on command
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="command">NpgsqlCommand</param>
    public static void AddSqlTags(this Activity activity, NpgsqlCommand command)
    {
        activity?.SetTag("db.system", "Postgres");
        activity?.SetTag("db.statement", command.CommandText);

        foreach (var param in command.Parameters.ToList())
        {
            try
            {
                if (param.Value == null) { continue; }
                if (param.Value is IList)
                {
                    activity?.SetTag(param.ParameterName, string.Join(',', (IList)param.Value));
                }
                else
                {
                    activity?.SetTag(param.ParameterName, param.Value);
                }
            }
            catch (Exception ex) 
            { 
                activity.RecordException(ex);
            }
        }
    }

    /// <summary>
    /// Sets status and records exception
    /// </summary>
    /// <param name="activity">Current activity</param>
    /// <param name="ex">Exception to record</param>
    /// <param name="statusDescription">Optional description/message for error</param>
    public static void ErrorWithException(this Activity activity, Exception ex, string? statusDescription = null)
    {
        activity?.RecordException(ex);
        activity?.SetStatus(ActivityStatusCode.Error, statusDescription);
    }
}
