using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// InstanceDelegationChange
    /// </summary>
    public class InstanceDelegationChange
    {
        /// <summary>
        /// Gets or sets the instance delegation change id
        /// </summary>
        [JsonPropertyName("instancedelegationchangeid")]
        public int InstanceDelegationChangeId { get; set; }

        /// <summary>
        /// Gets or sets the DelegationChangeType.
        /// </summary>
        public DelegationChangeType DelegationChangeType { get; set; }

        /// <summary>
        /// Gets or sets the InstanceDelegationType.
        /// </summary>
        public InstanceDelegationMode InstanceDelegationMode { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        [JsonPropertyName("resource")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        [JsonPropertyName("instance")]
        public string InstanceId { get; set; }

        /// <summary>
        /// The uuid of the party the right is on behalf of
        /// </summary>
        [JsonPropertyName("from")]
        public Guid FromUuid { get; set; }

        /// <summary>
        /// The type of party the right is on behalf of (Person, Organization, SystemUser)
        /// </summary>
        [JsonPropertyName("fromtype")]
        public UuidType FromUuidType { get; set; }

        /// <summary>
        /// The uuid of the party holding the right
        /// </summary>
        [JsonPropertyName("to")]
        public Guid ToUuid { get; set; }

        /// <summary>
        /// The type of party holding the right
        /// </summary>
        [JsonPropertyName("totype")]
        public UuidType ToUuidType { get; set; }

        /// <summary>
        /// The uuid of the party that performed the delegation
        /// </summary>
        [JsonPropertyName("performedby")]
        public string PerformedBy { get; set; }

        /// <summary>
        /// The type of the party that performed the delegation
        /// </summary>
        [JsonPropertyName("performedbytype")]
        public UuidType PerformedByType { get; set; }

        /// <summary>
        /// Gets or sets blobstoragepolicypath.
        /// </summary>
        [JsonPropertyName("blobstoragepolicypath")]
        public string BlobStoragePolicyPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blobstorage versionid
        /// </summary>
        [JsonPropertyName("blobstorageversionid")]
        public string BlobStorageVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the created date and timestamp for the delegation change
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }
    }
}
