using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Organization claim matching structure from maskinporten
    /// </summary>
    public class OrgClaim
    {
        /// <summary>
        /// The authority that defines organization numbers. 
        /// </summary>
        [JsonPropertyName("authority")]
        public string Authority => "iso6523-actorid-upis";

        [JsonPropertyName("ID")]
        public string ID { get; set; }
    }
}
