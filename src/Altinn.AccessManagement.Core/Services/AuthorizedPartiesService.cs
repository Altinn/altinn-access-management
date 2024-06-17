using System.Diagnostics;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class AuthorizedPartiesService : IAuthorizedPartiesService
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IDelegationMetadataRepository _delegations;
    private readonly IAltinnRolesClient _altinnRolesClient;
    private readonly IProfileClient _profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="delegations">Database repository for delegations</param>
    /// <param name="altinn2">SBL bridge client for role and reportee information from Altinn 2</param>
    /// <param name="profile">Service implementation for user profile retrieval</param>
    public AuthorizedPartiesService(IContextRetrievalService contextRetrievalService, IDelegationMetadataRepository delegations, IAltinnRolesClient altinn2, IProfileClient profile)
    {
        _contextRetrievalService = contextRetrievalService;
        _delegations = delegations;
        _altinnRolesClient = altinn2;
        _profile = profile;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedParties(BaseAttribute subjectAttribute, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken) => subjectAttribute.Type switch
    {
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId => await GetAuthorizedPartiesForPerson(subjectAttribute.Value.ToString(), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationId => await GetAuthorizedPartiesForOrganization(subjectAttribute.Value, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName => await GetAuthorizedPartiesForEnterpriseUser(subjectAttribute.Value, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid => await GetAuthorizedPartiesForPersonUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid => await GetAuthorizedPartiesForOrganizationUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid => await GetAuthorizedPartiesForEnterpriseUserUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute => await GetAuthorizedPartiesForParty(int.Parse(subjectAttribute.Value), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute => await GetAuthorizedPartiesForUser(int.Parse(subjectAttribute.Value), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken),
        _ => throw new ArgumentException(message: $"Unknown attribute type: {subjectAttribute.Type}", paramName: nameof(subjectAttribute))
    };

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForParty(int subjectPartyId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        Party subject = await _contextRetrievalService.GetPartyAsync(subjectPartyId, cancellationToken);
        if (subject?.PartyTypeName == PartyType.Person)
        {
            UserProfile user = await _profile.GetUser(new() { Ssn = subject.SSN }, cancellationToken);
            if (user != null)
            {
                return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
            }
        }

        if (subject?.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForUser(int subjectUserId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        List<int> keyRoleUnits = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId, cancellationToken);
        return await BuildAuthorizedParties(subjectUserId, keyRoleUnits, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForPerson(string subjectNationalId, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        UserProfile user = await _profile.GetUser(new() { Ssn = subjectNationalId }, cancellationToken);
        if (user != null)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForPersonUuid(string subjectPersonUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectPersonUuid, out Guid personUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectPersonUuid}", paramName: nameof(subjectPersonUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = personUuid }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Person)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganization(string subjectOrganizationNumber, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        Party subject = await _contextRetrievalService.GetPartyForOrganization(subjectOrganizationNumber, cancellationToken);
        if (subject != null)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganizationUuid(string subjectOrganizationUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectOrganizationUuid, out Guid orgUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectOrganizationUuid}", paramName: nameof(subjectOrganizationUuid));
        }

        Party subject = await _contextRetrievalService.GetPartyByUuid(orgUuid, cancellationToken: cancellationToken);
        if (subject != null && subject.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUser(string subjectEnterpriseUsername, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        UserProfile user = await _profile.GetUser(new() { Username = subjectEnterpriseUsername }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUserUuid(string subjectEnterpriseUserUuid, bool includeAltinn2AuthorizedParties, bool includeAuthorizedResourcesThroughRoles, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectEnterpriseUserUuid, out Guid enterpriseUserUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectEnterpriseUserUuid}", paramName: nameof(subjectEnterpriseUserUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = enterpriseUserUuid }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, includeAuthorizedResourcesThroughRoles, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    private async Task<List<AuthorizedParty>> BuildAuthorizedParties(int subjectUserId, List<int> subjectPartyIds, bool includeAltinn2AuthorizedParties, bool includeResourcesThroughRoles, CancellationToken cancellationToken)
    {
        List<AuthorizedParty> result = new();
        List<AuthorizedParty> a3AuthParties = new();
        SortedDictionary<int, AuthorizedParty> authorizedPartyDict = [];

        if ((includeAltinn2AuthorizedParties || includeResourcesThroughRoles) && subjectUserId != 0)
        {
            List<AuthorizedParty> a2AuthParties = await _altinnRolesClient.GetAuthorizedPartiesWithRoles(subjectUserId, cancellationToken);
            foreach (AuthorizedParty a2AuthParty in a2AuthParties)
            {
                if (includeResourcesThroughRoles)
                {
                    await EnrichPartyWithAuthorizedResourcesThroughRoles(a2AuthParty, cancellationToken);
                }

                authorizedPartyDict.Add(a2AuthParty.PartyId, a2AuthParty);
                if (a2AuthParty.Subunits != null)
                {
                    foreach (AuthorizedParty a2PartySubunit in a2AuthParty.Subunits)
                    {
                        authorizedPartyDict.Add(a2PartySubunit.PartyId, a2PartySubunit);
                    }
                }
            }

            result = a2AuthParties;
        }

        List<DelegationChange> delegations = await _delegations.GetAllDelegationChangesForAuthorizedParties(subjectUserId != 0 ? subjectUserId.SingleToList() : null, subjectPartyIds, cancellationToken: cancellationToken);

        List<int> fromPartyIds = delegations.Select(dc => dc.OfferedByPartyId).Distinct().ToList();
        List<MainUnit> mainUnits = await _contextRetrievalService.GetMainUnits(fromPartyIds, cancellationToken);

        fromPartyIds.AddRange(mainUnits.Where(m => m.PartyId > 0).Select(m => m.PartyId.Value));
        SortedDictionary<int, Party> delegationParties = await _contextRetrievalService.GetPartiesAsSortedDictionaryAsync(fromPartyIds, true, cancellationToken);

        foreach (var delegation in delegations)
        {
            if (!authorizedPartyDict.TryGetValue(delegation.OfferedByPartyId, out AuthorizedParty authorizedParty))
            {
                // Check if offering party has a main unit / is itself a subunit. 
                MainUnit mainUnit = await _contextRetrievalService.GetMainUnit(delegation.OfferedByPartyId, cancellationToken); // Since all mainunits were retrieved earlier results are in cache.
                if (mainUnit?.PartyId > 0)
                {
                    if (authorizedPartyDict.TryGetValue(mainUnit.PartyId.Value, out AuthorizedParty mainUnitAuthParty))
                    {
                        authorizedParty = mainUnitAuthParty.Subunits.Find(p => p.PartyId == delegation.OfferedByPartyId);

                        if (authorizedParty == null)
                        {
                            if (!delegationParties.TryGetValue(delegation.OfferedByPartyId, out Party party))
                            {
                                throw new UnreachableException($"Get AuthorizedParties failed to find subunit party for an existing active delegation from OfferedByPartyId: {delegation.OfferedByPartyId}");
                            }

                            authorizedParty = new AuthorizedParty(party);
                            mainUnitAuthParty.Subunits.Add(authorizedParty);
                        }

                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                    }
                    else
                    {
                        if (!delegationParties.TryGetValue(mainUnit.PartyId.Value, out Party mainUnitParty))
                        {
                            throw new UnreachableException($"Get AuthorizedParties failed to find mainunit party: {mainUnit.PartyId.Value} for an existing active delegation from subunit OfferedByPartyId: {delegation.OfferedByPartyId}");
                        }

                        mainUnitParty.OnlyHierarchyElementWithNoAccess = true;
                        mainUnitAuthParty = new AuthorizedParty(mainUnitParty, false);

                        // Find the authorized party as a subunit on the main unit
                        Party subunit = mainUnitParty.ChildParties.Find(p => p.PartyId == delegation.OfferedByPartyId);
                        if (subunit == null)
                        {
                            throw new UnreachableException($"Get AuthorizedParties failed to find subunit party: {delegation.OfferedByPartyId}, as child on the mainunit: {mainUnitParty.PartyId}");
                        }

                        authorizedParty = new(subunit);
                        mainUnitAuthParty.Subunits = new() { authorizedParty };
                        authorizedPartyDict.Add(mainUnitParty.PartyId, mainUnitAuthParty);
                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                        a3AuthParties.Add(mainUnitAuthParty);
                    }
                }
                else
                {
                    // Authorized party is not a subunit. Find party to add.
                    if (!delegationParties.TryGetValue(delegation.OfferedByPartyId, out Party party))
                    {
                        throw new UnreachableException($"Get AuthorizedParties failed to find party for an existing active delegation from OfferedByPartyId: {delegation.OfferedByPartyId}");
                    }

                    authorizedParty = new AuthorizedParty(party);
                    authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                    a3AuthParties.Add(authorizedParty);
                }
            }

            if (authorizedParty.OnlyHierarchyElementWithNoAccess)
            {
                // Delegation is from a MainUnit which has been added previously as hierarchy element. All children need to be added before resource enrichment
                if (!delegationParties.TryGetValue(authorizedParty.PartyId, out Party mainUnitParty))
                {
                    throw new UnreachableException($"Get AuthorizedParties failed to find mainunit party: {authorizedParty.PartyId} already added previously. Should not be possible.");
                }

                foreach (Party subunit in mainUnitParty.ChildParties)
                {
                    // Only add subunits which so far has not been already processed with some authorized access
                    if (!authorizedPartyDict.TryGetValue(subunit.PartyId, out AuthorizedParty authorizedSubUnit))
                    {
                        authorizedParty.Subunits.Add(new(subunit));
                    }
                }
            }

            authorizedParty.EnrichWithResourceAccess(delegation.ResourceId);
        }

        result.AddRange(a3AuthParties);
        return result;
    }

    private async Task EnrichPartyWithAuthorizedResourcesThroughRoles(AuthorizedParty party, CancellationToken cancellationToken)
    {
        if (party.AuthorizedRoles?.Count > 0)
        {
            IDictionary<string, IEnumerable<BaseAttribute>> subjectResources = await _contextRetrievalService.GetSubjectResources(party.AuthorizedRoles.Select(r => $"{AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute}:{r.ToLower()}"), cancellationToken);
            party.AuthorizedResources.AddRange(subjectResources.Keys.SelectMany(subject => subjectResources[subject].Where(resource => resource != null && resource.Value != null).Select(resource => resource.Value)));
        }

        if (party.Subunits?.Count > 0)
        {
            foreach (AuthorizedParty subunit in party.Subunits)
            {
                await EnrichPartyWithAuthorizedResourcesThroughRoles(subunit, cancellationToken);
            }
        }
    }
}
