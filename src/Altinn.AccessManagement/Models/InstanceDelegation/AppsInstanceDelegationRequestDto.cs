using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Enums;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationRequestDto
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
    /// Gets or sets a value indicating whether the instance delegation is for a parallel task
    /// </summary>
    [Required]
    public InstanceDelegationModeExternal InstanceDelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<RightDto> Rights { get; set; }
}