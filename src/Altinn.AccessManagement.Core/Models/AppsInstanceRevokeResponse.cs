using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.Register;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Response model for performing revoke of access to a respource from Apps
/// </summary>
public class AppsInstanceRevokeResponse
{
    /// <summary>
    /// Gets or sets the urn identifying the party to delegate from
    /// </summary>
    [Required]
    public PartyUrn From { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public PartyUrn To { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the resource of the instance
    /// </summary>
    [Required]
    public string ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the instance id
    /// </summary>
    [Required]
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instance delegation is for a parallel task
    /// </summary>
    [Required]
    public InstanceDelegationMode InstanceDelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<InstanceRightRevokeResult> Rights { get; set; }
}