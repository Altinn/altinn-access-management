using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Core.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says if a resource is delegated to you or you have delegated a resource to another person/org
    /// </summary>
    public class Delegation
    {
        /// <summary>
        /// Gets or sets the name of the delegation receiver
        /// </summary>
        [JsonPropertyName("coveredbyname")]
        public string CoveredByName { get; set; }

        /// <summary>
        /// Gets or sets the name of the delegator
        /// </summary>
        [JsonPropertyName("offeredbyname")]
        public string OfferedByName { get; set; }

        /// <summary>
        /// Gets or sets the userid id for the delegation
        /// </summary>
        [JsonPropertyName("offeredbypartyid")]
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the reportee that received the delegation
        /// </summary>
        [JsonPropertyName("coveredbypartyid")]
        public int? CoveredByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
        /// </summary>
        [JsonPropertyName("performedbyuserid")]
        public int PerformedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the userid that performed the delegation
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
    }
}
