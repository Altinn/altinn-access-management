using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models;

public class InstanceRight
{
    public InstanceRight()
    {
        InstanceRules = new List<InstanceRule>();
    }

    /// <summary>
    /// Uuid of the receiver of the right
    /// </summary>
    public Guid ToUuid { get; set; }

    /// <summary>
    /// The type of receiver of the right
    /// </summary>
    public UuidType ToType { get; set; }

    /// <summary>
    /// The uuid of the party the right is for
    /// </summary>
    public Guid FromUuid { get; set; }

    /// <summary>
    /// The type of the party the right is for
    /// </summary>
    public UuidType FromType { get; set; }

    /// <summary>
    /// The identficator of the party performing the delegation
    /// </summary>
    public string PerformedBy { get; set; }

    /// <summary>
    /// The type of the party performing the delegation
    /// </summary>
    public UuidType PerformedByType { get; set; }

    /// <summary>
    /// The resourceId of the delegating rule
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// The urn for this instance
    /// </summary>
    public string Instance { get; set; }

    /// <summary>
    /// The mode of delegation for now Parallel Signing and normal
    /// </summary>
    public InstanceDelegationMode InstanceDelegationMode { get; set; }

    /// <summary>
    /// List of rule specific data not shared between the difrent rules
    /// </summary>
    public List<InstanceRule> InstanceRules { get; set; }
}