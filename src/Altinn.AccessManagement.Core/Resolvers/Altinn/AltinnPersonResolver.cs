using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves attributes for <see cref="BaseUrn.Altinn.Person"/> 
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
    public AltinnPersonResolver(IContextRetrievalService contextRetrievalService, IProfileClient profile) : base(BaseUrn.Altinn.Person.String())
    {
        AddLeaf([BaseUrn.Altinn.Person.IdentifierNo], [BaseUrn.Altinn.Person.UserId, BaseUrn.Altinn.Person.PartyId, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolveProfileUsingIdentifierNo());
        AddLeaf([BaseUrn.Altinn.Person.UserId], [BaseUrn.Altinn.Person.IdentifierNo, BaseUrn.Altinn.Person.PartyId, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolveProfileUsingUserId());
        AddLeaf([BaseUrn.Altinn.Person.PartyId], [BaseUrn.Altinn.Person.IdentifierNo, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolvePartyUsingPartyId());
        AddLeaf([BaseUrn.Altinn.Person.PartyId], [BaseUrn.Altinn.Person.UserId, BaseUrn.Altinn.Person.PartyId, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolveProfileUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [BaseUrn.Altinn.Person.IdentifierNo, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolvePartyUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [BaseUrn.Altinn.Person.UserId, BaseUrn.Altinn.Person.PartyId, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolveProfileUsingPartyId());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], [BaseUrn.Altinn.Person.IdentifierNo, BaseUrn.Altinn.Person.PartyId, BaseUrn.Altinn.Person.Shortname, BaseUrn.Altinn.Person.Firstname, BaseUrn.Altinn.Person.Middlename, BaseUrn.Altinn.Person.Lastname], ResolveProfileUsingUserId());

        _contextRetrievalService = contextRetrievalService;
        _profile = profile;
    }

    /// <summary>
    /// Resolves a person if given <see cref="BaseUrn.Altinn.Person.IdentifierNo"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingIdentifierNo() => async (attributes, cancellationToken) =>
    {
        var user = await _profile.GetUser(new()
        {
            Ssn = attributes.GetString(BaseUrn.Altinn.Person.IdentifierNo)
        });

        if (user?.Party?.Person != null)
        {
            return
            [
                new(BaseUrn.Altinn.Person.UserId, user.UserId),
                new(BaseUrn.Altinn.Person.PartyId, user.PartyId),
                new(BaseUrn.Altinn.Person.Shortname, user.Party.Person.Name),
                new(BaseUrn.Altinn.Person.Firstname, user.Party.Person.FirstName),
                new(BaseUrn.Altinn.Person.Middlename, user.Party.Person.MiddleName),
                new(BaseUrn.Altinn.Person.Lastname, user.Party.Person.LastName),
            ];
        }

        return [];
    };

    /// <summary>
    /// Resolves a profile if given <see cref="BaseUrn.Altinn.Person.PartyId"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingPartyId() => async (attributes, cancellationToken) =>
    {
        var party = await ResolvePartyUsingPartyId()(attributes, cancellationToken);
        return await ResolveProfileUsingIdentifierNo()(party, cancellationToken);
    };

    /// <summary>
    /// Resolves a profile if given <see cref="BaseUrn.Altinn.Person.UserId"/>
    /// </summary>
    public LeafResolver ResolveProfileUsingUserId() => async (attributes, cancellationToken) =>
    {
        var user = await _profile.GetUser(new()
        {
            UserId = attributes.GetRequiredInt(BaseUrn.Altinn.Person.UserId, AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute)
        });

        if (user?.Party?.Person != null)
        {
            return
            [
                new(BaseUrn.Altinn.Person.IdentifierNo, user.Party.Person.SSN),
                new(BaseUrn.Altinn.Person.PartyId, user.PartyId),
                new(BaseUrn.Altinn.Person.Shortname, user.Party.Person.Name),
                new(BaseUrn.Altinn.Person.Firstname, user.Party.Person.FirstName),
                new(BaseUrn.Altinn.Person.Middlename, user.Party.Person.MiddleName),
                new(BaseUrn.Altinn.Person.Lastname, user.Party.Person.LastName),
            ];
        }

        return [];
    };

    /// <summary>
    /// Resolves a person if given <see cref="BaseUrn.Altinn.Person.PartyId"/> or <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute"/>
    /// </summary>
    public LeafResolver ResolvePartyUsingPartyId() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(BaseUrn.Altinn.Person.PartyId, AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)) is var party && party != null)
        {
            if (party.Person != null)
            {
                return
                [
                    new(BaseUrn.Altinn.Person.IdentifierNo, party.Person.SSN),
                    new(BaseUrn.Altinn.Person.Shortname, party.Person.Name),
                    new(BaseUrn.Altinn.Person.Firstname, party.Person.FirstName),
                    new(BaseUrn.Altinn.Person.Middlename, party.Person.MiddleName),
                    new(BaseUrn.Altinn.Person.Lastname, party.Person.LastName),
                ];
            }
        }

        return [];
    };
}