#nullable enable

using Altinn.Urn;

namespace Altinn.AccessManagement.Core.Models.Rights;

/// <summary>
/// A unique reference to an action in the form of an URN.
/// </summary>
[KeyValueUrn]
public abstract partial record ActionUrn
{
    /// <summary>
    /// Try to get the urn as an action.
    /// </summary>
    /// <param name="action">The resulting action.</param>
    /// <returns><see langword="true"/> if this is an action, otherwise <see langword="false"/>.</returns>
    [UrnKey("oasis:names:tc:xacml:1.0:action:action-id")]
    public partial bool IsActionId(out ActionIdentifier action);
}
