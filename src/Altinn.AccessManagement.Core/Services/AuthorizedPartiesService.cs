using System.Diagnostics;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
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
    public async Task<List<AuthorizedParty>> GetAuthorizedParties(BaseAttribute subjectAttribute, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken) => subjectAttribute.Type switch
    {
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId => await GetAuthorizedPartiesForPerson(subjectAttribute.Value.ToString(), includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationId => await GetAuthorizedPartiesForOrganization(subjectAttribute.Value, includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName => await GetAuthorizedPartiesForEnterpriseUser(subjectAttribute.Value, includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid => await GetAuthorizedPartiesForPersonUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid => await GetAuthorizedPartiesForOrganizationUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid => await GetAuthorizedPartiesForEnterpriseUserUuid(subjectAttribute.Value, includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute => await GetAuthorizedPartiesForParty(int.Parse(subjectAttribute.Value), includeAltinn2AuthorizedParties, cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute => await GetAuthorizedPartiesForUser(int.Parse(subjectAttribute.Value), includeAltinn2AuthorizedParties, cancellationToken),
        _ => throw new ArgumentException(message: $"Unknown attribute type: {subjectAttribute.Type}", paramName: nameof(subjectAttribute.Type))
    };

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForParty(int subjectPartyId, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        Party subject = await _contextRetrievalService.GetPartyAsync(subjectPartyId);
        if (subject?.PartyTypeName == PartyType.Person)
        {
            UserProfile user = await _profile.GetUser(new() { Ssn = subject.SSN });
            if (user != null)
            {
                return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, cancellationToken);
            }
        }

        if (subject?.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForUser(int subjectUserId, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        List<int> keyRoleUnits = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId, cancellationToken);
        return await BuildAuthorizedParties(subjectUserId, keyRoleUnits, includeAltinn2AuthorizedParties, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForPerson(string subjectNationalId, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        UserProfile user = await _profile.GetUser(new() { Ssn = subjectNationalId });
        if (user != null)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForPersonUuid(string subjectPersonUuid, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectPersonUuid, out Guid personUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectPersonUuid}", paramName: nameof(subjectPersonUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = personUuid });
        if (user != null && user.Party.PartyTypeName == PartyType.Person)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganization(string subjectOrganizationNumber, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        Party subject = await _contextRetrievalService.GetPartyForOrganization(subjectOrganizationNumber);
        if (subject != null)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForOrganizationUuid(string subjectOrganizationUuid, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectOrganizationUuid, out Guid orgUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectOrganizationUuid}", paramName: nameof(subjectOrganizationUuid));
        }

        Party subject = await _contextRetrievalService.GetPartyByUuid(orgUuid);
        if (subject != null && subject.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUser(string subjectEnterpriseUsername, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        UserProfile user = await _profile.GetUser(new() { Username = subjectEnterpriseUsername });
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesForEnterpriseUserUuid(string subjectEnterpriseUserUuid, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectEnterpriseUserUuid, out Guid enterpriseUserUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectEnterpriseUserUuid}", paramName: nameof(subjectEnterpriseUserUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = enterpriseUserUuid });
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesForUser(user.UserId, includeAltinn2AuthorizedParties, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    private async Task<List<AuthorizedParty>> BuildAuthorizedParties(int subjectUserId, List<int> subjectPartyIds, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken)
    {
        List<AuthorizedParty> result = new();
        List<AuthorizedParty> a3AuthParties = new();
        SortedDictionary<int, AuthorizedParty> authorizedPartyDict = [];

        if (includeAltinn2AuthorizedParties && subjectUserId != 0)
        {
            List<AuthorizedParty> a2AuthParties = await _altinnRolesClient.GetAuthorizedPartiesWithRoles(subjectUserId, cancellationToken);
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

        List<DelegationChange> delegations = await _delegations.GetAllDelegationChangesForAuthorizedParties(subjectUserId != 0 ? subjectUserId.SingleToList() : null, subjectPartyIds, cancellationToken: cancellationToken);

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
