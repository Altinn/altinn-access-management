using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="Urn.Altinn.Person"/> 
/// </summary>
public class AltinnPersonResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnPersonResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Person.String())
    {
        AddLeaf([Urn.Altinn.Person.IdentifierNo], [Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveIdentifierNo());
        AddLeaf([Urn.Altinn.Person.PartyId], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolvePartyId());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolves a person if given <see cref="Urn.Altinn.Person.IdentifierNo"/>
    /// </summary>
    public LeafResolver ResolveIdentifierNo() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyForPerson(attributes.GetRquiredString(Urn.Altinn.Person.IdentifierNo)) is var party && party != null)
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
    /// Resolves a person if given <see cref="Urn.Altinn.Person.PartyId"/>
    /// </summary>
    public LeafResolver ResolvePartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(Urn.Altinn.Person.PartyId)) is var party && party != null)
        {
            return [
                new(Urn.Altinn.Person.IdentifierNo, party.Person.SSN),
                new(Urn.Altinn.Person.Shortname, party.Person.Name),
                new(Urn.Altinn.Person.Firstname, party.Person.FirstName),
                new(Urn.Altinn.Person.Middlename, party.Person.MiddleName),
                new(Urn.Altinn.Person.Lastname, party.Person.LastName),
            ];
        }

        return [];
    };
}