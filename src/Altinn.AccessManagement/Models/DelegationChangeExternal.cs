using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This is an external model that describes a delegation change as stored in the Authorization postgre DelegationChanges table.
    /// </summary>
    public class DelegationChangeExternal
    {
        /// <summary>
        /// Gets or sets the delegation change id
        /// </summary>
        public int DelegationChangeId { get; set; }

        /// <summary>
        /// Gets or sets the resource registry delegation change id
        /// </summary>
        public int ResourceRegistryDelegationChangeId { get; set; }

        /// <summary>
        /// Gets or sets the delegation change type
        /// </summary>
        public DelegationChangeTypeExternal DelegationChangeType { get; set; }

        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string ResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resourcetype.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the offeredbypartyid, refering to the party id of the user or organization offering the delegation.
        /// </summary>
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the coveredbypartyid, refering to the party id of the organization having received the delegation. Otherwise Null if the recipient is a user.
        /// </summary>
        public int? CoveredByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the coveredbyuserid, refering to the user id of the user having received the delegation. Otherwise Null if the recipient is an organization.
        /// </summary>
        public int? CoveredByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
        /// </summary>
        public int? PerformedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the party id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
        /// </summary>
        public int? PerformedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets blobstoragepolicypath.
        /// </summary>
        public string BlobStoragePolicyPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blobstorage versionid
        /// </summary>
        public string BlobStorageVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the created date and timestamp for the delegation change
        /// </summary>
        public DateTime? Created { get; set; }
    }
}
