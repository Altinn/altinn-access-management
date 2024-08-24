using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationResponse
{
    /// <summary>
    /// Gets or sets the urn identifying the party to delegate from
    /// </summary>
    [Required]
    public UrnJsonTypeValue<PartyUrn> From { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public UrnJsonTypeValue<PartyUrn> To { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the resource of the instance
    /// </summary>
    [Required]
    public UrnJsonTypeValue<ResourceIdUrn> Resource { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the instance id
    /// </summary>
    [Required]
    public UrnJsonTypeValue<ResourceInstanceUrn> Instance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instance delegation is for a parallel task
    /// </summary>
    [Required]
    public bool IsParallelTask { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<RightV2DelegationResult> Rights { get; set; }
}