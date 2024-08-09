using System.Runtime.CompilerServices;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Enums;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class Altinn2RightsService : IAltinn2RightsService
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IPolicyInformationPoint _pip;
    private readonly IAltinn2RightsClient _altinn2RightsClient;
    private readonly IProfileClient _profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="Altinn2RightsService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="pip">Service for getting policy information</param>
    /// <param name="altinn2RightsClient">SBL Bridge client implementation for rights operations on Altinn 2 services</param>
    /// <param name="profileClient">Profile lookup client</param>
    public Altinn2RightsService(IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IAltinn2RightsClient altinn2RightsClient, IProfileClient profileClient)
    {
        _contextRetrievalService = contextRetrievalService;
        _pip = pip;
        _altinn2RightsClient = altinn2RightsClient;
        _profile = profileClient;
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

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, BaseAttribute toAttribute, CancellationToken cancellationToken = default) => toAttribute.Type switch
    {
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid => await ClearReporteeRightsForUser(fromPartyId, toAttribute.Value, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid => await ClearReporteeRightsForParty(fromPartyId, toAttribute.Value, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid => await ClearReporteeRightsForUser(fromPartyId, toAttribute.Value, cancellationToken),
        _ => throw new ArgumentException(message: $"Unknown attribute type: {toAttribute.Type}", paramName: nameof(toAttribute))
    };

    private async Task<HttpResponseMessage> ClearReporteeRightsForUser(int fromPartyId, string toUserUuid, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(toUserUuid, out Guid userUuid))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { ReasonPhrase = $"Not a well-formed uuid: {toUserUuid}" };
        }

        if (await _profile.GetUser(new Models.Profile.UserProfileLookup { UserUuid = userUuid }, cancellationToken) is var profile && profile != null)
        {
            return await _altinn2RightsClient.ClearReporteeRights(fromPartyId, profile.PartyId, profile.UserId, cancellationToken);
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
    }

    private async Task<HttpResponseMessage> ClearReporteeRightsForParty(int fromPartyId, string toPartyUuid, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(toPartyUuid, out Guid partyUuid))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { ReasonPhrase = $"Not a well-formed uuid: {toPartyUuid}" };
        }

        if (await _contextRetrievalService.GetPartyByUuid(partyUuid, false, cancellationToken) is var party && party != null && party.PartyTypeName == PartyType.Organisation)
        {
            return await _altinn2RightsClient.ClearReporteeRights(fromPartyId, party.PartyId, cancellationToken: cancellationToken);
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
    }

    private async Task<List<RightDelegation>> MapDelegationResponse(IEnumerable<DelegationChange> delegations)
    {
        var result = new List<RightDelegation>();

        foreach (var delegation in delegations)
        {
            var entry = new RightDelegation();

            if (delegation.CoveredByUserId == null && delegation.CoveredByPartyId == null)
            {
                // This is a temporary fix just to remove delegations given to system users so they do not give problems for the use from Altinn II to be changed later
                continue;
            }

            if (delegation.CoveredByUserId != null)
            {
                entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, delegation.CoveredByUserId.ToString()));
            }

            if (delegation.CoveredByPartyId != null)
            {
                entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.CoveredByPartyId.ToString()));
            }

            entry.From.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.OfferedByPartyId.ToString()));

            if (delegation.ResourceType.Contains("AltinnApp", StringComparison.InvariantCultureIgnoreCase))
            {
                var app = delegation.ResourceId.Split("/");
                entry.Resource.AddRange([
                    new(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, app[0]),
                    new(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, app[1])
                ]);
            }
            else
            {
                var resources = await _contextRetrievalService.GetResourceList();
                entry.Resource.AddRange(resources.Find(r => r.Identifier == delegation.ResourceId).AuthorizationReference ?? []);
            }

            result.Add(entry);
        }

        return result;
    }
}