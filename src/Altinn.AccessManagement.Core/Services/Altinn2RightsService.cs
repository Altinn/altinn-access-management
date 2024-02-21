using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class Altinn2RightsService : IAltinn2RightsService
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IPolicyInformationPoint _pip;

    /// <summary>
    /// Initializes a new instance of the <see cref="Altinn2RightsService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="pip">Service for getting policy information</param>
    public Altinn2RightsService(IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip)
    {
        _contextRetrievalService = contextRetrievalService;
        _pip = pip;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RightDelegation>> GetOfferedRights(int partyId, CancellationToken cancellationToken = default)
    {
        var delegations = await _pip.GetOfferedDelegationsFromRepository(partyId, cancellationToken);
        return await MapDelegationResponse(delegations);
    }

    /// <inheritdoc/>
    public async Task<List<RightDelegation>> GetReceivedRights(int partyId, CancellationToken cancellationToken = default)
    {
        var delegations = await _pip.GetReceivedDelegationFromRepository(partyId, cancellationToken);
        return await MapDelegationResponse(delegations);
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