using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// DTO for sending a instance delegation request internaly 
/// </summary>
public class InstanceDelegationChangeRequest
{
    /// <summary>
    /// Gets or sets the resource.
    /// </summary>
    [JsonPropertyName("resource")]
    public string Resource { get; set; }

    /// <summary>
    /// Gets or sets the instance.
    /// </summary>
    [JsonPropertyName("instance")]
    public string Instance { get; set; }

    /// <summary>
    /// Gets or sets the InstanceDelegationType.
    /// </summary>
    public InstanceDelegationMode InstanceDelegationMode { get; set; }

    /// <summary>
    /// The uuid of the party the right is on behalf of
    /// </summary>
    [JsonPropertyName("from")]
    public Guid FromUuid { get; set; }

    /// <summary>
    /// The type of party the right is on behalf of (Person, Organization, SystemUser)
    /// </summary>
    [JsonPropertyName("fromtype")]
    public UuidType FromType { get; set; }

    /// <summary>
    /// The uuid of the party holding the right
    /// </summary>
    [JsonPropertyName("to")]
    public Guid ToUuid { get; set; }

    /// <summary>
    /// The type of party holding the right
    /// </summary>
    [JsonPropertyName("totype")]
    public UuidType ToType { get; set; }
}