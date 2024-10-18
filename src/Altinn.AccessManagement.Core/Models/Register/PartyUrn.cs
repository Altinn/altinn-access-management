#nullable enable

using Altinn.Urn;

namespace Altinn.AccessManagement.Core.Models.Register;

/// <summary>
/// A unique reference to a party in the form of an URN.
/// </summary>
[KeyValueUrn]
public abstract partial record PartyUrn
{
    /// <summary>
    /// Try to get the urn as a party uuid.
    /// </summary>
    /// <param name="partyUuid">The resulting party uuid.</param>
    /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
    [UrnKey("altinn:party:uuid")]
    public partial bool IsPartyUuid(out Guid partyUuid);

    /// <summary>
    /// Try to get the urn as an organization number.
    /// </summary>
    /// <param name="organizationNumber">The resulting organization number.</param>
    /// <returns><see langword="true"/> if this party reference is an organization number, otherwise <see langword="false"/>.</returns>
    [UrnKey("altinn:organization:identifier-no")]
    public partial bool IsOrganizationIdentifier(out OrganizationNumber organizationNumber);        
}
