using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Enums;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class Altinn2RightsService : IAltinn2RightsService
{
    private readonly IDelegationMetadataRepository _delegationRepository;
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IProfileClient _profile;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="delegationRepository">database implementation for fetching and inserting delegations</param>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="profile">Client implementation for getting user profile</param>
    public Altinn2RightsService(IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IProfileClient profile)
    {
        _delegationRepository = delegationRepository;
        _contextRetrievalService = contextRetrievalService;
        _profile = profile;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RightDelegation>> GetOfferedRights(int partyId, CancellationToken cancellationToken = default)
    {
        var delegations = await GetOfferedDelegationsFromRepository(partyId, cancellationToken);
        return await MapDelegationResponse(delegations);
    }

    /// <inheritdoc/>
    public async Task<List<RightDelegation>> GetReceivedRights(int partyId, CancellationToken cancellationToken = default)
    {
        var delegations = await GetReceivedDelegationFromRepository(partyId, cancellationToken);
        return await MapDelegationResponse(delegations);
    }

    /// <summary>
    /// returns the reportee's received delegations from db 
    /// </summary>
    /// <param name="partyId">reportee</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    private async Task<IEnumerable<DelegationChange>> GetReceivedDelegationFromRepository(int partyId, CancellationToken cancellationToken)
    {
        var party = await _contextRetrievalService.GetPartyAsync(partyId);

        if (party?.PartyTypeName == PartyType.Person)
        {
            var user = await _profile.GetUser(new()
            {
                Ssn = party.SSN,
            });

            var keyRoles = await _contextRetrievalService.GetKeyRolePartyIds(user.UserId, cancellationToken);
            return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(user.UserId.SingleToList(), keyRoles, cancellationToken);
        }

        if (party?.PartyTypeName == PartyType.Organisation)
        {
            return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(null, party.PartyId.SingleToList(), cancellationToken);
        }

        throw new ArgumentException($"failed to handle party with id '{partyId}'");
    }

    /// <summary>
    /// returns the reportee's offereds delegations from db
    /// </summary>
    /// <param name="partyId">reportee</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    private async Task<IEnumerable<DelegationChange>> GetOfferedDelegationsFromRepository(int partyId, CancellationToken cancellationToken)
    {
        var party = await _contextRetrievalService.GetPartyAsync(partyId);

        if (party.PartyTypeName == PartyType.Person)
        {
            var user = await _profile.GetUser(new()
            {
                Ssn = party.SSN
            });

            var keyRoles = await _contextRetrievalService.GetKeyRolePartyIds(user.UserId, cancellationToken);
            var offeredBy = user.PartyId.SingleToList();
            if (keyRoles.Count > 0)
            {
                offeredBy.AddRange(keyRoles);
            }

            return await _delegationRepository.GetOfferedDelegations(offeredBy, cancellationToken);
        }

        if (party.PartyTypeName == PartyType.Organisation)
        {
            var mainUnits = await _contextRetrievalService.GetMainUnits(party.PartyId.SingleToList(), cancellationToken);
            var parties = party.PartyId.SingleToList();
            if (mainUnits?.FirstOrDefault() is var mainUnit && mainUnit?.PartyId != null)
            {
                parties.Add((int)mainUnit.PartyId);
            }

            return await _delegationRepository.GetOfferedDelegations(parties, cancellationToken);
        }

        throw new ArgumentException($"failed to handle party with id '{partyId}'");
    }

    private async Task<List<RightDelegation>> MapDelegationResponse(IEnumerable<DelegationChange> delegations)
    {
        var result = new List<RightDelegation>();
        var resources = await _contextRetrievalService.GetResourceList();

        foreach (var delegation in delegations)
        {
            var entry = new RightDelegation();
            if (delegation.CoveredByUserId != null)
            {
                entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, delegation.CoveredByUserId.ToString()));
            }

            if (delegation.CoveredByPartyId != null)
            {
                entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.CoveredByPartyId.ToString()));
            }

            entry.From.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.OfferedByPartyId.ToString()));

            var resourcePath = delegation.ResourceId.Split("/");
            if (delegation.ResourceType.Contains("AltinnApp", StringComparison.InvariantCultureIgnoreCase) && resourcePath.Length > 1)
            {
                entry.Resource.AddRange(resources
                    .Where(a => a.AuthorizationReference.Exists(p => p.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute && p.Value == resourcePath[0]))
                    .First(a => a.AuthorizationReference.Exists(p => p.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute && p.Value == resourcePath[1]))
                    .AuthorizationReference);
            }
            else
            {
                entry.Resource.AddRange(resources.First(r => r.Identifier == delegation.ResourceId).AuthorizationReference);
            }

            result.Add(entry);
        }

        return result;
    }
}