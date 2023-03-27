using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says if a resource is delegated to you or you have delegated a resource to another person/org
    /// </summary>
    public class Delegation
    {
        /// <summary>
        /// Gets or sets the name of the delegation receiver
        /// </summary>
        public string CoveredByName { get; set; }

        /// <summary>
        /// Gets or sets the name of the delegator
        /// </summary>
        public string OfferedByName { get; set; }

        /// <summary>
        /// Gets or sets the userid id for the delegation
        /// </summary>
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the reportee that received the delegation
        /// </summary>
        public int? CoveredByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
        /// </summary>
        [JsonPropertyName("performedbyuserid")]
        public int? PerformedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
        /// </summary>
        [JsonPropertyName("performedbypartyid")]
        public int? PerformedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the userid that performed the delegation
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the organization number that offered the delegation
        /// </summary>
        public int OfferedByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the organization number that received the delegation
        /// </summary>
        public int CoveredByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the resource id that is delegated
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// The title of resource
        /// </summary>
        public Dictionary<string, string> ResourceTitle { get; set; }

        /// <summary>
        /// Gets or sets the type of resource that is delegated
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// ResourceReference
        /// </summary>
        public List<ResourceReference> ResourceReferences { get; set; }

        /// <summary>
        /// HasCompetentAuthority
        /// </summary>
        public CompetentAuthority HasCompetentAuthority { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public Dictionary<string, string> Description { get; set; }

        /// <summary>
        /// Description explaining the rights a recipient will receive if given access to the resource
        /// </summary>
        public Dictionary<string, string> RightDescription { get; set; }
    }
}
