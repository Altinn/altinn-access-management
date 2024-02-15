using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn.EnterpriseUser"/> 
/// </summary>
public class AltinnEnterpriseUserResolver : AttributeResolver
{
    private readonly IProfileClient _profile;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnEnterpriseUserResolver(IProfileClient profile) : base(Urn.Altinn.EnterpriseUser.String())
    {
        AddLeaf([Urn.Altinn.EnterpriseUser.Username], [Urn.Altinn.EnterpriseUser.UserId], ResolveUsername());
        _profile = profile;
    }

    /// <summary>
    /// Resolves an enterprise user if given <see cref="Urn.Altinn.EnterpriseUser.Username"/>
    /// </summary>
    public LeafResolver ResolveUsername() => async (attributes, cancellationToken) =>
    {
        if (await _profile.GetUser(new UserProfileLookup { Username = attributes.GetRequiredString(Urn.Altinn.EnterpriseUser.Username) }) is var profile && profile != null)
        {
            return
            [
                new(Urn.Altinn.EnterpriseUser.UserId, profile.UserId),
            ];
        }

        return [];
    };
}