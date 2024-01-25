using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// summary
/// </summary>
public class AltinnOrganizationResolver : AttributeResolver, IAttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnOrganizationResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Organization.ToString())
    {
        AddLeaf([Urn.Altinn.Organization.PartyId], [Urn.Altinn.Organization.Name, Urn.Altinn.Organization.IdentifierNo], ResolvePartyId());
        AddLeaf([Urn.Altinn.Organization.IdentifierNo], [Urn.Altinn.Organization.Name, Urn.Altinn.Organization.PartyId], ResolveOrganizationNumber());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolve Input Party
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolvePartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(GetAttributeInt(attributes, Urn.Altinn.Organization.PartyId)) is var party && party != null)
        {
            return
            [
                new(Urn.Altinn.Organization.Name, party.Name),
                new(Urn.Altinn.Organization.IdentifierNo, party.Organization.OrgNumber),
            ];
        }

        return [];
    };

    /// <summary>
    /// Resolve Organization number
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveOrganizationNumber() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyForOrganization(GetAttributeString(attributes, Urn.Altinn.Organization.IdentifierNo)) is var party && party != null)
        {
            return
            [
                new(Urn.Altinn.Organization.PartyId, party.PartyId),
                new(Urn.Altinn.Organization.Name, party.Name)
            ];
        }

        return [];
    };
}