using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a source from where a right exist for a user or party
    /// </summary>
    public class RightSource
    {
        /// <summary>
        /// Gets or sets the set of type of source this right originated from (Role, AccessGroup, AppDelegation, ResourceRegistryDelegation etc.)
        /// </summary>
        public RightSourceType RightSourceType { get; set; }

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
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party has the right
        /// </summary>
        public bool? HasPermit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party is permitted to delegate the right to others
        /// </summary>
        public bool? CanDelegate { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the minimum required authentication level
        /// </summary>
        public int MinimumAuthenticationLevel { get; set; }

        /// <summary>
        /// Gets or sets the list of subject matches the user has.
        /// </summary>
        public List<AttributeMatch> UserSubjects { get; set; }

        /// <summary>
        /// Gets or sets the list of subject matches which provides access to this right in the resource policy
        /// </summary>
        public List<List<PolicyAttributeMatch>> PolicySubjects { get; set; }
    }
}
