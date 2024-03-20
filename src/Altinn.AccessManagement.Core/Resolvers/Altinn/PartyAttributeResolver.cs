using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Resolvers;
using Altinn.AccessManagement.Core.Resolvers.Extensions;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Resolvers;

/// <summary>
/// Resolves Party attribute as a PartyId
/// </summary>
public class PartyAttributeResolver : AttributeResolver
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IProfileClient _profile;

    /// <summary>
    /// Resolves Party attribute as a PartyId
    /// </summary>
    /// <param name="contextRetrievalService">service init</param>
    /// <param name="profile">profile client</param>
    public PartyAttributeResolver(IContextRetrievalService contextRetrievalService, IProfileClient profile) : base(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)
    {
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], [AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], ResolvePartyIdFromParty());
        AddLeaf([AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute], [AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute], ResolvePartyIdFromUser());

        _contextRetrievalService = contextRetrievalService;
        _profile = profile;
    }

    /// <summary>
    /// Resolves a PartyId if given <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute"/> exists
    /// </summary>
    public LeafResolver ResolvePartyIdFromUser() => async (attributes, cancellationToken) =>
    {
        UserProfile user = await _profile.GetUser(
            new() { UserId = attributes.GetRequiredInt(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute) },
            cancellationToken);

        if (user != null)
        {
            return [new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, user.Party.PartyId)];
        }

        return [];
    };

    /// <summary>
    /// Resolves a PartyId if given <see cref="AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute"/> exists
    /// </summary>
    public LeafResolver ResolvePartyIdFromParty() => async (attributes, cancellationToken) =>
    {
        if (await _contextRetrievalService.GetPartyAsync(attributes.GetRequiredInt(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute), cancellationToken) is var party && party != null)
        {
            return [new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, party.PartyId)];
        }

        return [];
    };
}