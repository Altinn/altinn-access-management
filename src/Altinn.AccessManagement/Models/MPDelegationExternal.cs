using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a delegation. A delegation is an action that says which resource is delegated by supplier to consumer org
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MPDelegationExternal
    {
        /// <summary>
        /// Gets or sets the organization number of the consumer that gives the delegation
        /// </summary>
        [JsonPropertyName("consumer_org")]
        public string ConsumerOrg { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the supplier that received the delegation
        /// </summary>
        [JsonPropertyName("supplier_org")]
        public string SupplierOrg { get; set; }

        /// <summary>
        /// Gets or sets the id of the DelegationScheme that is delegated
        /// </summary>
        /// <value>The id of the delegation scheme.</value>
        [JsonPropertyName("delegation_scheme_Id")]
        public Guid? DelegationSchemeId { get; set; }

        /// <summary>
        /// Gets or sets a list of scopes in the DelegationScheme
        /// </summary>
        [JsonPropertyName("scopes")]
        public HashSet<string> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the time for when the delegation was preformed
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the id of the resource
        /// </summary>
        [JsonPropertyName("resourceid")]
        public string ResourceId { get; set; }
    }
}
