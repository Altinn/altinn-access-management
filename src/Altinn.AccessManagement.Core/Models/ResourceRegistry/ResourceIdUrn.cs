#nullable enable

using Altinn.Urn;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A unique reference to a resource in the form of an URN.
/// </summary>
[KeyValueUrn]
public abstract partial record ResourceIdUrn
{
    /// <summary>
    /// Try to get the urn as a resource id.
    /// </summary>
    /// <param name="resourceId">The resulting resource id.</param>
    /// <returns><see langword="true"/> if this resource reference is a resource id, otherwise <see langword="false"/>.</returns>
    [UrnKey("altinn:resource")]
    public partial bool IsResourceId(out ResourceIdentifier resourceId);
}
