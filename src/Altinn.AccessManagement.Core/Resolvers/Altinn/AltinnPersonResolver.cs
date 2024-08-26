using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
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
    private readonly IProfileClient _profile;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    /// <param name="profile">profile client</param>
    public AltinnPersonResolver(IContextRetrievalService contextRetrievalService, IProfileClient profile) : base(Urn.Altinn.Person.String())
    {
        AddLeaf([Urn.Altinn.Person.IdentifierNo], [Urn.Altinn.Person.UserId, Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveProfileUsingIdentifierNo());
        AddLeaf([Urn.Altinn.Person.UserId], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveProfileUsingUserId());
        AddLeaf([Urn.Altinn.Person.PartyId], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolvePartyUsingPartyId());
        AddLeaf([Urn.Altinn.Person.PartyId], [Urn.Altinn.Person.UserId, Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveProfileUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolvePartyUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [Urn.Altinn.Person.UserId, Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveProfileUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], [Urn.Altinn.Person.IdentifierNo, Urn.Altinn.Person.PartyId, Urn.Altinn.Person.Shortname, Urn.Altinn.Person.Firstname, Urn.Altinn.Person.Middlename, Urn.Altinn.Person.Lastname], ResolveProfileUsingUserId());

        _contextRetrievalService = contextRetrievalService;
        _profile = profile;
    }

    /// <summary>
    /// Resolves a person if given <see cref="Urn.Altinn.Person.IdentifierNo"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingIdentifierNo() => async (attributes, cancellationToken) =>
    {
        var user = await _profile.GetUser(new()
        {
            Ssn = attributes.GetString(Urn.Altinn.Person.IdentifierNo)
        });

        if (user?.Party?.Person != null)
        {
            return
            [
                new(Urn.Altinn.Person.UserId, user.UserId),
                new(Urn.Altinn.Person.PartyId, user.PartyId),
                new(Urn.Altinn.Person.Shortname, user.Party.Person.Name),
                new(Urn.Altinn.Person.Firstname, user.Party.Person.FirstName),
                new(Urn.Altinn.Person.Middlename, user.Party.Person.MiddleName),
                new(Urn.Altinn.Person.Lastname, user.Party.Person.LastName),
            ];
        }

        return [];
    };

    /// <summary>
    /// Resolves a profile if given <see cref="Urn.Altinn.Person.PartyId"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingPartyId() => async (attributes, cancellationToken) =>
    {
        var party = await ResolvePartyUsingPartyId()(attributes, cancellationToken);
        return await ResolveProfileUsingIdentifierNo()(party, cancellationToken);
    };

    /// <summary>
    /// Resolves a profile if given <see cref="Urn.Altinn.Person.UserId"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingUserId() => async (attributes, cancellationToken) =>
    {
        var user = await _profile.GetUser(new()
        {
            UserId = attributes.GetRequiredInt(Urn.Altinn.Person.UserId, AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute)
        });

        if (user?.Party?.Person != null)
        {
            return
            [
                new(Urn.Altinn.Person.IdentifierNo, user.Party.Person.SSN),
                new(Urn.Altinn.Person.PartyId, user.PartyId),
                new(Urn.Altinn.Person.Shortname, user.Party.Person.Name),
                new(Urn.Altinn.Person.Firstname, user.Party.Person.FirstName),
                new(Urn.Altinn.Person.Middlename, user.Party.Person.MiddleName),
                new(Urn.Altinn.Person.Lastname, user.Party.Person.LastName),
            ];
        }

        return [];
    };

    /// <summary>
    /// Resolves a person if given <see cref="Urn.Altinn.Person.PartyId"/> or <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute"/>
    /// </summary>
    public LeafResolver ResolvePartyUsingPartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(Urn.Altinn.Person.PartyId, AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)) is var party && party != null)
        {
            if (party.Person != null)
            {
                return
                [
                    new(Urn.Altinn.Person.IdentifierNo, party.Person.SSN),
                    new(Urn.Altinn.Person.Shortname, party.Person.Name),
                    new(Urn.Altinn.Person.Firstname, party.Person.FirstName),
                    new(Urn.Altinn.Person.Middlename, party.Person.MiddleName),
                    new(Urn.Altinn.Person.Lastname, party.Person.LastName),
                ];
            }
        }

        return [];
    };
}