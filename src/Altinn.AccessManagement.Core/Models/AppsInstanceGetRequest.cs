using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Request model for getting delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceGetRequest
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
    

}