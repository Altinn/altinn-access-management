using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.AccessManagement.Core.Telemetry;

/// <summary>
/// Collection of common ActivityEvents
/// </summary>
[ExcludeFromCodeCoverage]
public static class TelemetryEvents
{
    /// <summary>
    /// Common ActivityEvent for HttpResponses with unexpected result
    /// </summary>
    /// <param name="httpResponse">The response to extract data from</param>
    /// <returns></returns>
    public static ActivityEvent UnexpectedHttpStatusCode(HttpResponseMessage httpResponse)
    {
        return new ActivityEvent("Unexpected HttpStatusCode", tags: new ActivityTagsCollection(new Dictionary<string, object>() 
        { 
            { "StatusCode", httpResponse.StatusCode },
            { "Content", httpResponse.Content.ReadAsStringAsync() },
        }));

        // CreateValidationProblemDetails

        // ProblemDetailsFactory.CreateProblemDetails
    }

    /// <summary>
    /// Collection of ActivityEvents specific for Sbl Bridge
    /// </summary>
    public static class SblBridge
    {
        /// <summary>
        /// ActivityEvent when Sbl Bridge is unreachable
        /// </summary>
        /// <param name="description">Description of reason or other relevant information</param>
        /// <returns></returns>
        public static ActivityEvent Unreachable(string description)
        {
            var tags = new ActivityTagsCollection(new Dictionary<string, object> { { "Description", description } });
            return new ActivityEvent("Unable to reach Altinn 2", tags: tags);
        }
    }

    /// <summary>
    /// Collection of ActivityEvents specific for the Api
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// Specific ActivityEvent for an invalid praty request
        /// </summary>
        /// <param name="partyId">Party identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns></returns>
        public static ActivityEvent InvalidParty(int partyId, int userId)
        {
            var tags = new ActivityTagsCollection(new Dictionary<string, object> 
            { 
                { "PartyId", partyId },
                { "UserId", userId }
            });
            return new ActivityEvent("The party id is either invalid or is not an authorized party for the authenticated user", tags: tags);
        }
    }
}