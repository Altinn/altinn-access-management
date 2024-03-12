using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn.Organization"/> 
/// </summary>
public class AltinnOrganizationResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnOrganizationResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Organization.String())
    {
        AddLeaf([Urn.Altinn.Organization.PartyId], [Urn.Altinn.Organization.Name, Urn.Altinn.Organization.IdentifierNo], ResolvePartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [Urn.Altinn.Organization.Name, Urn.Altinn.Organization.IdentifierNo], ResolvePartyId());
        AddLeaf([Urn.Altinn.Organization.IdentifierNo], [Urn.Altinn.Organization.Name, Urn.Altinn.Organization.PartyId], ResolveOrganizationNumber());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolves an organization if given <see cref="Urn.Altinn.Organization.PartyId"/>
    /// </summary>
    public LeafResolver ResolvePartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(Urn.Altinn.Organization.PartyId)) is var party && party != null)
        {
            if (party.Organization != null)
            {
                return
                [
                    new(Urn.Altinn.Organization.Name, party.Name),
                    new(Urn.Altinn.Organization.IdentifierNo, party.Organization.OrgNumber),
                ];
            }
        }

        return [];
    };

    /// <summary>
    /// Resolves an organization if given <see cref="Urn.Altinn.Organization.IdentifierNo"/>
    /// </summary>
    public LeafResolver ResolveOrganizationNumber() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyForOrganization(attributes.GetRequiredString(Urn.Altinn.Organization.IdentifierNo)) is var party && party != null)
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