using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn.Altinn.EnterpriseUser"/> 
/// </summary>
public class AltinnEnterpriseUserResolver : AttributeResolver
{
    private readonly IProfileClient _profile;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnEnterpriseUserResolver(IProfileClient profile) : base(BaseUrn.Altinn.EnterpriseUser.String())
    {
        AddLeaf([BaseUrn.Altinn.EnterpriseUser.Username], [BaseUrn.Altinn.EnterpriseUser.UserId], ResolveUsername());
        _profile = profile;
    }

    /// <summary>
    /// Resolves an enterprise user if given <see cref="BaseUrn.Altinn.EnterpriseUser.Username"/>
    /// </summary>
    public LeafResolver ResolveUsername() => async (attributes, cancellationToken) =>
    {
        if (await _profile.GetUser(new UserProfileLookup { Username = attributes.GetRequiredString(BaseUrn.Altinn.EnterpriseUser.Username) }) is var profile && profile != null)
        {
            return
            [
                new(BaseUrn.Altinn.EnterpriseUser.UserId, profile.UserId),
            ];
        }

        return [];
    };
}