using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// DTO for holding resource and action for instance delegations
/// </summary>
public class RightInternal
{
    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public List<UrnJsonTypeValue> Resource { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public ActionUrn Action { get; set; }
}