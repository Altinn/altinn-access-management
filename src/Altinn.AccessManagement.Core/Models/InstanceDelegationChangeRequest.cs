using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Enums;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models;

public class InstanceDelegationChangeRequest
{
    /// <summary>
    /// Gets or sets the resource.
    /// </summary>
    [JsonPropertyName("resource")]
    public ResourceUrn Resource { get; set; }

    /// <summary>
    /// Gets or sets the instance.
    /// </summary>
    [JsonPropertyName("instance")]
    public ResourceInstanceUrn Instance { get; set; }

    /// <summary>
    /// Gets or sets the InstanceDelegationType.
    /// </summary>
    public InstanceDelegationType InstanceDelegationType { get; set; }

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
}