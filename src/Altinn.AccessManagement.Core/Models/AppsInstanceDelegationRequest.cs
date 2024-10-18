using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationRequest
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
    /// Gets or sets a value indicating whether the instance delegation is for a parallel task
    /// </summary>
    [Required]
    public InstanceDelegationMode InstanceDelegationMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instance delegation is from a user or app
    /// </summary>
    [Required]
    public InstanceDelegationSource InstanceDelegationSource { get; set; }

    /// <summary>
    /// The instanceid to the spesific resource
    /// </summary>
    public string InstanceId { get; set; }

    /// <summary>
    /// The ResourceId for the specific resource
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// The app performing the delegation
    /// </summary>
    public ResourceIdUrn PerformedBy { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<RightInternal> Rights { get; set; }
}