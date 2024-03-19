using System.Diagnostics;

namespace Altinn.AccessManagement.Core.Telemetry;

/// <summary>
/// Collection of known ActivityEvents
/// </summary>
public static class TelemetryEvents
{
    public static ActivityEvent UnexpectedHttpStatusCode(HttpResponseMessage httpResponse)
    {
        return new ActivityEvent("Unexpected HttpStatusCode", tags: new ActivityTagsCollection(new Dictionary<string, object>() 
        { 
            { "StatusCode", httpResponse.StatusCode }
        }));
    }


    public readonly static ActivityEvent SqlOutOfMemry = new ActivityEvent("The sql server is out of memeory");

    public static class SBLBridge
    {
        public static ActivityEvent Unreachable(string description)
        {
            var tags = new ActivityTagsCollection(new Dictionary<string, object> { { "Description", description } });
            return new ActivityEvent("Unable to reach Altinn 2", tags: tags);
        }
    }
}