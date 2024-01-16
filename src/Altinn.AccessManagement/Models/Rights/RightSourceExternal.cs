using System.Text.Json.Serialization;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a source from where a right exist for a user or party
    /// </summary>
    public class RightSourceExternal
    {
        /// <summary>
        /// Gets or sets the set of type of source this right originated from (Role, AccessGroup, AppDelegation, ResourceRegistryDelegation etc.)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RightSourceTypeExternal RightSourceType { get; set; }

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
        /// Gets or sets a value indicating whether the user or party has the right
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? HasPermit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party is permitted to delegate the right to others
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? CanDelegate { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the minimum required authentication level
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinimumAuthenticationLevel { get; set; }

        /// <summary>
        /// Gets or sets the list of subject matches the user has.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<AttributeMatchExternal> UserSubjects { get; set; }

        /// <summary>
        /// Gets or sets the list of subject matches which provides access to this right in the resource policy
        /// </summary>
        public List<List<PolicyAttributeMatchExternal>> PolicySubjects { get; set; }
    }
}
