#nullable enable

using Altinn.Urn;
using System.Buffers;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A unique reference to a resource in the form of an URN.
/// </summary>
[KeyValueUrn]
public abstract partial record ResourceUrn
{
    /// <summary>
    /// Try to get the urn as a party uuid.
    /// </summary>
    /// <param name="resourceId">The resulting party uuid.</param>
    /// <returns><see langword="true"/> if this resource reference is a party uuid, otherwise <see langword="false"/>.</returns>
    [UrnKey("altinn:resource")]
    public partial bool IsResource(out ResourceId resourceId);    
}
