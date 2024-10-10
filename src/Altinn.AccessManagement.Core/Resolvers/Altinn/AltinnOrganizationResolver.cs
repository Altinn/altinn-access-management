using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn.Altinn.Organization"/> 
/// </summary>
public class AltinnOrganizationResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// ctor
    /// </summary>
    public AltinnOrganizationResolver(IContextRetrievalService contextRetrievalService) : base(BaseUrn.Altinn.Organization.String())
    {
        AddLeaf([BaseUrn.Altinn.Organization.PartyId], [BaseUrn.Altinn.Organization.Name, BaseUrn.Altinn.Organization.IdentifierNo], ResolvePartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [BaseUrn.Altinn.Organization.Name, BaseUrn.Altinn.Organization.IdentifierNo], ResolvePartyId());
        AddLeaf([BaseUrn.Altinn.Organization.IdentifierNo], [BaseUrn.Altinn.Organization.Name, BaseUrn.Altinn.Organization.PartyId], ResolveOrganizationNumber());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolves an organization if given <see cref="BaseUrn.Altinn.Organization.PartyId"/>
    /// </summary>
    public LeafResolver ResolvePartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(BaseUrn.Altinn.Organization.PartyId)) is var party && party != null)
        {
            if (party.Organization != null)
            {
                return
                [
                    new(BaseUrn.Altinn.Organization.Name, party.Name),
                    new(BaseUrn.Altinn.Organization.IdentifierNo, party.Organization.OrgNumber),
                ];
            }
        }

        return [];
    };

    /// <summary>
    /// Resolves an organization if given <see cref="BaseUrn.Altinn.Organization.IdentifierNo"/>
    /// </summary>
    public LeafResolver ResolveOrganizationNumber() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyForOrganization(attributes.GetRequiredString(BaseUrn.Altinn.Organization.IdentifierNo)) is var party && party != null)
        {
            return
            [
                new(BaseUrn.Altinn.Organization.PartyId, party.PartyId),
                new(BaseUrn.Altinn.Organization.Name, party.Name)
            ];
        }

        return [];
    };
}