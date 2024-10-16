using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// Request model for a list of all delegable rights for a specific resource.
/// </summary>
public class ResourceDelegationCheckRequestDto
{
    /// <summary>
    /// Gets or sets the urn for identifying the resource of the rights to be checked
    /// </summary>
    [Required]
    public UrnJsonTypeValue<ResourceIdUrn> ResourceId { get; set; }
}
