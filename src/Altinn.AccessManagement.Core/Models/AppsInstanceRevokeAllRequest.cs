using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Request model for performing revoke of access to a resource from Apps
/// </summary>
public class AppsInstanceRevokeAllRequest
{
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
    /// The App performing the get request
    /// </summary>
    public ResourceIdUrn.ResourceId PerformedBy { get; set; }
}