using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// This model describes a single right
/// </summary>
public class InstanceRightDelegationResult
{
    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public List<UrnJsonTypeValue> Resource { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public UrnJsonTypeValue<ActionUrn> Action { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the right was successfully delegated or not
    /// </summary>
    public DelegationStatus Status { get; set; }
}