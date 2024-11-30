using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// System User claim matching structure from maskinporten
    /// </summary>
    public class SystemUserClaim
    {
        /// <summary>
        /// The type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type => "urn:altinn:systemuser";

        /// <summary>
        /// The organization that created the system user
        /// </summary>
        [JsonPropertyName("systemuser_org")]
        public OrgClaim Systemuser_org { get; set; }

        /// <summary>
        /// The system user id
        /// </summary>
        [JsonPropertyName("systemuser_id")]
        public List<string> Systemuser_id { get; set; }

        /// <summary>
        /// The system id
        /// </summary>
        [JsonPropertyName("system_id")]
        public string System_id { get; set; }
    }
}
