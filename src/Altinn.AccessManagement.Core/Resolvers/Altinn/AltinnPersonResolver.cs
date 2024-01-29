using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// test
/// </summary>
public class AltinnPersonResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    public AltinnPersonResolver(IContextRetrievalService contextRetrievalService) : base(Urn.Altinn.Person.ToString())
    {
        AddLeaf([Urn.Altinn.Person.IdentifierNo], [Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveIdentifierNo());
        AddLeaf([Urn.Altinn.Person.PartyId], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolvePartyId());
        _contextRetrievalService = contextRetrievalService;
    }

    /// <summary>
    /// Resolve social security number and lastname
    /// </summary>
    /// <returns></returns>
    public LeafResolver ResolveIdentifierNo() => async (attributes, cancellationToken) =>
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
    public LeafResolver ResolvePartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(GetAttributeInt(attributes, Urn.Altinn.Person.PartyId)) is var party && party != null)
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