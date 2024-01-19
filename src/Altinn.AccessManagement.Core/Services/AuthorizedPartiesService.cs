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
    private readonly IUserProfileLookupService _profileLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="pip">Service implementation for policy information point</param>
    /// <param name="delegations">Database repository for delegations</param>
    /// <param name="roles">SBL bridge client for role information form Altinn 2</param>
    /// <param name="profileLookup">Service implementation for lookup of userprofile with lastname verification</param>
    public AuthorizedPartiesService(IContextRetrievalService contextRetrievalService, IPolicyInformationPoint pip, IDelegationMetadataRepository delegations, IAltinnRolesClient roles, IUserProfileLookupService profileLookup)
    {
        _contextRetrievalService = contextRetrievalService;
        _pip = pip;
        _delegations = delegations;
        _altinnRolesClient = roles;
        _profileLookup = profileLookup;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedParties(int authenticatedUserId, CancellationToken cancellationToken)
    {
        List<AuthorizedParty> result = null;
        SortedDictionary<int, AuthorizedParty> authorizedPartyDict = [];
        bool includeAltinn2Reportees = true;

        List<AuthorizedParty> a2Reportees = await _altinnRolesClient.GetAuthorizedPartiesWithRoles(authenticatedUserId, cancellationToken);
        if (includeAltinn2Reportees)
        {
            result = a2Reportees;
        }

        //// ToDo: Find all Resource rights through roles (needs RR Role - Resource API)
        
        // Find all A3 delegations (direct and inherited)
        List<int> keyRoleUnits = await _contextRetrievalService.GetKeyRolePartyIds(authenticatedUserId, cancellationToken);
        List<DelegationChange> delegations = await _delegations.GetAllDelegationChangesTo(authenticatedUserId.SingleToList(), keyRoleUnits, cancellationToken: cancellationToken);

        List<int> fromPartyIds = delegations.Select(dc => dc.OfferedByPartyId).Distinct().ToList();
        List<MainUnit> mainUnits = await _contextRetrievalService.GetMainUnits(fromPartyIds, cancellationToken);

        fromPartyIds.AddRange(mainUnits.Where(m => m.PartyId.HasValue).Select(m => m.PartyId.Value));
        Task<List<Party>> delegationParties = _contextRetrievalService.GetPartiesAsync(fromPartyIds, cancellationToken); // ToDo: Update to include subunits
        
        foreach (var delegation in delegations)
        {
            // Todo build or enrich AuthorizedParties
        }

        return result;
    }
}
