using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationResponseDto
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
    /// Gets or sets a value identifying the resource of the instance
    /// </summary>
    [Required]
    public string ResourceId { get; set; }

    /// <summary>
    /// Gets or sets a value identifying the instance id
    /// </summary>
    [Required]
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<RightDelegationResultDto> Rights { get; set; }
}