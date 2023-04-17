using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// An enriched delegation model describing a delegation of a resource between two parties. Combines information from <see cref="DelegationChange"/>, <see cref="ServiceResource"/>
    /// as well as Party information for both the offering and receiving parties.
    /// </summary>
    public class Delegation
    {
        /// <summary>
        /// Gets or sets the party id of the party which have offered the delegation
        /// </summary>
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the party which have offered the delegation
        /// </summary>
        public string OfferedByName { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the party which have offered the delegation
        /// </summary>
        public string OfferedByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the party id of the party which have received the delegation
        /// </summary>
        public int? CoveredByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the party which have received the delegation
        /// </summary>
        public string CoveredByName { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the party which have received the delegation
        /// </summary>
        public string CoveredByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation
        /// </summary>
        [JsonPropertyName("performedbyuserid")]
        public int? PerformedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the party id of the party that performed the delegation
        /// </summary>
        [JsonPropertyName("performedbypartyid")]
        public int? PerformedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the date and timestamp the delegation was performed
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the resource id of the resource registered in the resource registry which have been delegated
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the type of resource which have been delegated
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Dictionary of the title of resource in all registered languages
        /// </summary>
        public Dictionary<string, string> ResourceTitle { get; set; }

        /// <summary>
        /// List of reference values associated with the resource. This can be service codes from Altinn II, Delegation Scheme Id from Altinn II, Scopes from Maskinporten or just an external URL.
        /// </summary>
        public List<ResourceReference> ResourceReferences { get; set; }

        /// <summary>
        /// HasCompetentAuthority
        /// </summary>
        public CompetentAuthority HasCompetentAuthority { get; set; }

        /// <summary>
        /// Dictionary of the description of the resource in all registered languages
        /// </summary>
        public Dictionary<string, string> Description { get; set; }

        /// <summary>
        /// Dictionary of the delegation description of the resource in all registered languages. THe delegation description gives additional information about what the consequence and rights the recipient of a delegation will receive.
        /// </summary>
        public Dictionary<string, string> RightDescription { get; set; }
    }
}
