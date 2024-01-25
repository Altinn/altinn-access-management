using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Resolvers;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// test
/// </summary>
public class AltinnResourceResolver : AttributeResolver, IAttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnResourceResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Person.ToString())
    {
        AddLeaf([Urn.Altinn.Organization.IdentifierNo, Urn.Altinn.Resource.AppId], [], ResolveOrganizationAndAppId());
        AddLeaf([Urn.Altinn.Resource.ResourceRegistryId], [], ResolveResourceRegistry());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolve social security number and lastname
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveOrganizationAndAppId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyForPerson(GetAttributeString(attributes, Urn.Altinn.Person.IdentifierNo)) is var party && party != null)
        {
            return
            [
                new(Urn.Altinn.Person.PartyId, party.PartyId),
                new(Urn.Altinn.Person.Shortname, party.Person.Name),
                new(Urn.Altinn.Person.Firstname, party.Person.FirstName),
                new(Urn.Altinn.Person.Middlename, party.Person.MiddleName),
                new(Urn.Altinn.Person.Lastname, party.Person.LastName),
            ];
        }

        return [];
    };

    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveResourceRegistry() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetResourceList() is var resources && resources != null)
        {
            var resource = resources.First(resource => resource.Identifier == attributes.GetString(Urn.Altinn.Resource.ResourceRegistryId));
            return [
                new(Urn.Altinn.Resource.ResourceRegistryId, resource.Identifier),
            ];
        }

        return [];
    };
}