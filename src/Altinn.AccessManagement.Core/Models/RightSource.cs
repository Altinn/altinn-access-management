using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a source from where a right exist for a user or party
    /// </summary>
    public class RightSource
    {
        /// <summary>
        /// Gets or sets the unique identifier for the specific policy the right originates (Output only).
        /// </summary>
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the version of the policy which the right originates (Output only).
        /// </summary>
        public string PolicyVersion { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the specific rule within the policy the right originates (Output only).
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the party offering the rights to the receiving (CoveredBy) entity.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the party receiving (covered by) the rights from the delegating (OfferedByPartyId) entity
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<AttributeMatch> CoveredBy { get; set; }

        /// <summary>
        /// Gets or sets the list of subject matches which uniquely identifies the subject this rule applies to.
        /// </summary>
        public List<AttributeMatch> Subject { get; set; }

        /// <summary>
        /// Gets or sets the set of type of source this right originated from (Role, AccessGroup, AppDelegation, ResourceRegistryDelegation etc.)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RightSourceType RightSourceType { get; set; }
    }
}
