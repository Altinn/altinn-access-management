using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves From attribute as a PartyId
/// </summary>
public class UserAttributeResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IProfileClient _profile;

    /// <summary>
    /// Resolves To attribute as a either a PartyId (if found as an organization) or a UserId (if found as a user)
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    /// <param name="profile">profile client</param>
    public UserAttributeResolver(IContextRetrievalService contextRetrievalService, IProfileClient profile) : base(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)
    {
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], ResolveFromParty());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], [AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], ResolveFromUser());

        _contextRetrievalService = contextRetrievalService;
        _profile = profile;
    }

    /// <summary>
    /// Resolves a UserId if given <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute"/> exists
    /// </summary>
    public LeafResolver ResolveFromUser() => async (attributes, cancellationToken) =>
    {
        UserProfile user = await _profile.GetUser(
            new() { UserId = attributes.GetRequiredInt(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute) },
            cancellationToken);

        if (user != null)
        {
            return [new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, user.UserId)];
        }

        return [];
    };

    /// <summary>
    /// Resolves a party if given a <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute"/>
    /// </summary>
    public LeafResolver ResolveFromParty() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute), cancellationToken) is var party && party?.Person != null)
        {
            return await ResolveUserIdUsingIdentifierNo()([new(BaseUrn.Altinn.Person.IdentifierNo, party.Person.SSN)], cancellationToken);
        }

        return [];
    };

    /// <summary>
    /// Resolves a person to a userId
    /// </summary>
    public LeafResolver ResolveUserIdUsingIdentifierNo() => async (attributes, cancellationToken) =>
    {
        var user = await _profile.GetUser(
            new() { Ssn = attributes.GetRequiredString(BaseUrn.Altinn.Person.IdentifierNo) },
            cancellationToken);

        if (user != null)
        {
            return
            [
                new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, user.UserId)
            ];
        }

        return [];
    };
}