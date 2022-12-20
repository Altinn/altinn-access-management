using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models.SblBridge
{
    /// <summary>
    /// Model returned by SBL Bridge partyparents endpoint, describing a main unit (hovedenhet).
    /// </summary>
    public class MainUnit
    {
        /// <summary>
        /// Gets or sets the PartyId of the main unit
        /// </summary>
        [JsonPropertyName("ParentPartyId")]
        public int? PartyId { get; set; }

        /// <summary>
        /// Gets or sets the PartyId of the subunit
        /// </summary>
        [JsonPropertyName("PartyId")]
        public int SubunitPartyId { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the main unit
        /// </summary>
        [JsonPropertyName("ParentOrganizationNumber")]
        public string OrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the main unit
        /// </summary>
        [JsonPropertyName("ParentOrganizationName")]
        public string OrganizationName { get; set; }
    }
}
