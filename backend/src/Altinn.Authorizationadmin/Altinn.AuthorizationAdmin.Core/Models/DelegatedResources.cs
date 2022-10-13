using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Core.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says if a resource is delegated to you or you have delegated a resource to another person/org
    /// </summary>
    public class DelegatedResources
    {
        /// <summary>
        /// Gets or sets the identifier of the resource in a delegation
        /// </summary>
        [JsonPropertyName("resourceid")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource in a delegation
        /// </summary>
        [JsonPropertyName("resourcetitle")]
        public string ResourceTitle { get; set; }

        /// <summary>
        /// Delegations for the resource
        /// </summary>
        public List<Delegation> Delegations { get; set; }
    }
}
