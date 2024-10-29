using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Request model for a list of all delegable rights for a specific resource.
/// </summary>
public class ResourceDelegationCheckRequest
{
    /// <summary>
    /// Gets or sets the urn for identifying the resource of the rights to be checked
    /// </summary>
    [Required]
    public ResourceIdUrn ResourceId { get; set; }
}
