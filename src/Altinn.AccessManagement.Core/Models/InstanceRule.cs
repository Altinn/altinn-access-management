using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// DTO for delegation rules
/// </summary>
public class InstanceRule
{
    /// <summary>
    /// Gets or sets the unique identifier for a specific rule within a policy (Output only).
    /// </summary>
    public string RuleId { get; set; }

    /// <summary>
    /// The resource delegating for
    /// </summary>
    public List<UrnJsonTypeValue> Resource { get; set; }

    /// <summary>
    /// The action to delegate
    /// </summary>
    public ActionUrn Action { get; set; }

    /// <summary>
    /// Flag identifying if this instance rule was created successfully
    /// </summary>
    public bool CreatedSuccessfully { get; set; }
}