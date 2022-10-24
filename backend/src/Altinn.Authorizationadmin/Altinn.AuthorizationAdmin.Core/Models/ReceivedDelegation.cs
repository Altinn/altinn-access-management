using System.Text.Json.Serialization;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;

namespace Altinn.AuthorizationAdmin.Core.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says if a resource is delegated to you or you have delegated a resource to another person/org
    /// </summary>
    public class ReceivedDelegation
    {
        /// <summary>
        /// Gets or sets the name of the organisation that delegated the resource
        /// </summary>
        [JsonPropertyName("offeredbyname")]
        public string OfferedByName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the resource in a delegation
        /// </summary>
        [JsonPropertyName("offeredbypartyid")]
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the organisation number that delegated the resource
        /// </summary>
        [JsonPropertyName("offeredbyorgnumber")]
        public int OfferedByOrgNumber { get; set; }

        /// <summary>
        /// Resources that were received
        /// </summary>
        public List<ServiceResource> Resources { get; set; }   
    }
}
