using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationResponseV2
{
    /// <summary>
    /// Gets or sets the urn identifying the party to delegate from
    /// </summary>
    [Required]
    public Guid FromPartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public Guid ToPartyUuid { get; set; }

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
    public bool IsParallelTask { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<SimpleRightV2> Rights { get; set; }
}