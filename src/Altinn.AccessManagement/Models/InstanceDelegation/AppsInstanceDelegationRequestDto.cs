using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
    public required UrnJsonTypeValue<PartyUrn> From { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public required UrnJsonTypeValue<PartyUrn> To { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instance delegation is for a parallel task
    /// </summary>
    [Required]
    public required InstanceDelegationModeExternal InstanceDelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public required IEnumerable<RightDto> Rights { get; set; }
}