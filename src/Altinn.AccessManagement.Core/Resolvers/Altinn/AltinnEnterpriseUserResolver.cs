using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// summary
/// </summary>
public class AltinnEnterpriseUserResolver : AttributeResolver
{
    private readonly IProfileClient _profile;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="profile">a</param>
    public AltinnEnterpriseUserResolver(IProfileClient profile) : base(Urn.Altinn.EnterpriseUser.ToString())
    {
        AddLeaf([Urn.Altinn.EnterpriseUser.Username], [Urn.Altinn.EnterpriseUser.PartyId], ResolveUsername());
        _profile = profile;
    }

    /// <summary>
    /// Resolve Input Party
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveUsername() => async (attributes, cancellationToken) =>
    {
        if (await _profile.GetUser(new UserProfileLookup { Username = attributes.GetString(Urn.Altinn.EnterpriseUser.Username) }) is var party && party != null)
        {
            return
            [
                new(Urn.Altinn.EnterpriseUser.PartyId, party.PartyId),
            ];
        }

        return [];
    };
}