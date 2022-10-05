using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Core.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says if a resource is delegated to you or you have delegated a resource to another person/org
    /// </summary>
    public class Delegation
    {
        /// <summary>
        /// Gets or sets the identifier of the resource in a delegation
        /// </summary>
        [JsonPropertyName("resourceid")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource in a delegation
        /// </summary>
        [JsonPropertyName("resourcename")]
        public Dictionary<string, string> ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the delegation receiver
        /// </summary>
        [JsonPropertyName("delegatedToName")]
        public string DelegatedToName { get; set; }

        /// <summary>
        /// Gets or sets the userid id for the delegation
        /// </summary>
        [JsonPropertyName("delegatedbyid")]
        public int DelegatedById { get; set; }

        /// <summary>
        /// Gets or sets the reportee that received the delegation
        /// </summary>
        [JsonPropertyName("delegatedtoid")]
        public int DelegatedToId { get; set; }
    }
}
