using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationResponse
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
    public IEnumerable<InstanceRightDelegationResult> Rights { get; set; }
}