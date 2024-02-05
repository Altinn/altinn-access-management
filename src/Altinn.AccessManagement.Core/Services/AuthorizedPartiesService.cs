using System.Diagnostics;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class AuthorizedPartiesService : IAuthorizedPartiesService
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IPolicyInformationPoint _pip;
    private readonly IDelegationMetadataRepository _delegations;
    private readonly IAltinnRolesClient _altinnRolesClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="delegations">Database repository for delegations</param>
    /// <param name="altinn2">SBL bridge client for role and reportee information from Altinn 2</param>
    public AuthorizedPartiesService(IContextRetrievalService contextRetrievalService, IDelegationMetadataRepository delegations, IAltinnRolesClient altinn2)
    {
        _contextRetrievalService = contextRetrievalService;
        _delegations = delegations;
        _altinnRolesClient = altinn2;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedParties(int authenticatedUserId, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        List<AuthorizedParty> result = new();
        List<AuthorizedParty> a3AuthParties = new();
        SortedDictionary<int, AuthorizedParty> authorizedPartyDict = [];

        if (includeAltinn2AuthorizedParties)
        {
            List<AuthorizedParty> a2AuthParties = await _altinnRolesClient.GetAuthorizedPartiesWithRoles(authenticatedUserId, cancellationToken);
            foreach (AuthorizedParty a2Party in a2AuthParties)
            {
                authorizedPartyDict.Add(a2Party.PartyId, a2Party);
                if (a2Party.ChildParties != null)
                {
                    foreach (AuthorizedParty a2PartySubunit in a2Party.ChildParties)
                    {
                        authorizedPartyDict.Add(a2PartySubunit.PartyId, a2PartySubunit);
                    }
                }
            }

            result = a2AuthParties;
        }

        //// To-be-implemented: Find all authorized resources through roles (needs RR Role - Resource API)

        // Find all needed datasets: A3 delegations (direct and inherited), KeyRole-relations, MainUnits, Subunits and Parties
        List<int> keyRoleUnits = await _contextRetrievalService.GetKeyRolePartyIds(authenticatedUserId, cancellationToken);
        List<DelegationChange> delegations = await _delegations.GetAllDelegationChangesForAuthorizedParties(authenticatedUserId.SingleToList(), keyRoleUnits, cancellationToken: cancellationToken);

        List<int> fromPartyIds = delegations.Select(dc => dc.OfferedByPartyId).Distinct().ToList();
        List<MainUnit> mainUnits = await _contextRetrievalService.GetMainUnits(fromPartyIds, cancellationToken);

        fromPartyIds.AddRange(mainUnits.Where(m => m.PartyId.HasValue).Select(m => m.PartyId.Value));
        List<Party> delegationParties = await _contextRetrievalService.GetPartiesAsync(fromPartyIds, true, cancellationToken);
        
        foreach (var delegation in delegations)
        {
            if (!authorizedPartyDict.TryGetValue(delegation.OfferedByPartyId, out AuthorizedParty authorizedParty))
            {
                // Check if offering party has a main unit / is itself a subunit
                MainUnit mainUnit = mainUnits.Find(mu => mu.SubunitPartyId == delegation.OfferedByPartyId);
                if (mainUnit?.PartyId > 0)
                {
                    if (!authorizedPartyDict.TryGetValue(mainUnit.PartyId.Value, out AuthorizedParty mainUnitAuthParty))
                    {
                        Party mainUnitParty = delegationParties.Find(p => p.PartyId == mainUnit.PartyId.Value);
                        mainUnitParty.OnlyHierarchyElementWithNoAccess = true;

                        // Find the authorized party as a subunit on the main unit
                        foreach (Party subunit in mainUnitParty.ChildParties)
                        {
                            if (subunit.PartyId == delegation.OfferedByPartyId)
                            {
                                authorizedParty = new AuthorizedParty(subunit);
                            }
                            else
                            {
                                subunit.OnlyHierarchyElementWithNoAccess = true;
                            }
                        }

                        mainUnitAuthParty = new AuthorizedParty(mainUnitParty);
                        authorizedPartyDict.Add(mainUnitParty.PartyId, mainUnitAuthParty);
                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                        a3AuthParties.Add(mainUnitAuthParty);
                    }
                    else
                    {
                        authorizedParty = mainUnitAuthParty.ChildParties.Find(p => p.PartyId == delegation.OfferedByPartyId);
                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                    }
                }
                else
                {
                    // Authorized party is not a subunit. Find party to add.
                    Party party = delegationParties.Find(p => p.PartyId == delegation.OfferedByPartyId);
                    if (party != null)
                    {
                        authorizedParty = new AuthorizedParty(party);
                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                        a3AuthParties.Add(authorizedParty);
                    }
                    else
                    {
                        throw new UnreachableException($"Get AuthorizedParties failed to find Party for an existing active delegation from OfferedByPartyId: {delegation.OfferedByPartyId}");
                    }
                }
            }

            authorizedParty.EnrichWithResourceAccess(delegation.ResourceId);
        }

        result.AddRange(a3AuthParties);
        return result;
    }
}
