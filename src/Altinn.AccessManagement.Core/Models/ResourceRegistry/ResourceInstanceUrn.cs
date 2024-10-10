#nullable enable

using Altinn.Urn;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A unique reference to a resource instance in the form of an URN.
/// </summary>
[KeyValueUrn]
public abstract partial record ResourceInstanceUrn
{
    /// <summary>
    /// Try to get the urn as a resource instance id.
    /// </summary>
    /// <param name="resourceId">The resulting resource instance id.</param>
    /// <returns><see langword="true"/> if this resource instance reference is a resource instance id, otherwise <see langword="false"/>.</returns>
    [UrnKey("altinn:resource:instance-id")]
    public partial bool IsResourceInstanceId(out ResourceInstanceIdentifier resourceId);
}
