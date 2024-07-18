using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Defines the interface for the context retrieval service defining operations for getting external context information for decision point requests
/// </summary>
public interface IContextRetrievalService
{
    /// <summary>
    /// Get the decision point roles for the loggedin user for a selected party
    /// </summary>
    /// <param name="coveredByUserId">the logged in user id</param>
    /// <param name="offeredByPartyId">the partyid of the person/org the logged in user is representing</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the roles the user has for a given reportee, as basis for evaluating rights for delegation.
    /// For any user having HADM this means, getting additional roles as DAGL etc.
    /// </summary>
    /// <param name="coveredByUserId">the user id</param>
    /// <param name="offeredByPartyId">the partyid of the person/org the user is representing</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single party by its party id
    /// </summary>
    /// <param name="partyId">The party id</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Party</returns>
    Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of parties by their party ids
    /// </summary>
    /// <param name="partyIds">List of partyIds to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the lookup party IDs are for a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a dictionary of parties by their party ids
    /// </summary>
    /// <param name="partyIds">List of partyIds to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the lookup party IDs are for a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<SortedDictionary<int, Party>> GetPartiesAsSortedDictionaryAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single party by its party uuid
    /// </summary>
    /// <param name="partyUuid">The party uuid</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the parties are a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Party</returns>
    Task<Party> GetPartyByUuid(Guid partyUuid, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a dictionary of parties by their party uuids
    /// </summary>
    /// <param name="partyUuids">Collection of party uuids to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the parties are a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Dictionary of parties</returns>
    Task<Dictionary<string, Party>> GetPartiesByUuids(IEnumerable<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a system user by the uuid and owning party
    /// </summary>
    /// <param name="partyId">partyId for the system user owning party</param>
    /// <param name="systemUserUuid">the identifier og the system user</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<SystemUser> GetSystemUserById(int partyId, string systemUserUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the registered default rights for a given system type
    /// </summary>
    /// <param name="productId">the system to fetch the default rights for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<List<DefaultRight>> GetDefaultRightsForRegisteredSystem(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the party of an organization
    /// </summary>
    /// <param name="organizationNumber">The organization number to lookup party</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Party</returns>
    Task<Party> GetPartyForOrganization(string organizationNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the party of a person
    /// </summary>
    /// <param name="ssn">The social security number to lookup party</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Party</returns>
    Task<Party> GetPartyForPerson(string ssn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of PartyIds the given user id has key role access to (where the user inherit delegations to their organization)
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>list of PartyIds where the logged in user have key role access</returns>
    Task<List<int>> GetKeyRolePartyIds(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a main unit for the input sub unit partyId. If the input partyId is not a sub unit the response model will have null values for main unit properties.
    /// </summary>
    /// <param name="subunitPartyIds">The list of PartyId to check and retrieve any main units for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>main units</returns>
    Task<List<MainUnit>> GetMainUnits(List<int> subunitPartyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a main unit for the input sub unit partyId. If the input partyId is not a sub unit the response model will have null values for main unit properties.
    /// </summary>
    /// <param name="subunitPartyId">The PartyId to check and retrieve any main unit for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>main unit</returns>
    Task<MainUnit> GetMainUnit(int subunitPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single resoure by it's resource id if registered in the Resource Registry
    /// </summary>
    /// <param name="resourceRegistryId">The identifier of the resource in the Resource Registry</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The resource if exists</returns>
    Task<ServiceResource> GetResource(string resourceRegistryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all resources from the Resource Registry
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The resource list</returns>
    Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all available resources including Altinn Apps, Altinn 2 services and resources from the Resource Registry
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The resource list</returns>
    Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single resource from the list of all available resources including Altinn Apps, Altinn 2 services and resources from the Resource Registry, if it exists.
    /// </summary>
    /// <param name="resourceId">The resource id</param>
    /// <param name="org">Org code of the resource/app owner</param>
    /// <param name="app">The app name</param>
    /// <param name="serviceCode">Tha Altinn 2 Service Code</param>
    /// <param name="serviceEditionCode">The Altinn 2 Service Edition Code</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The resource if exists</returns>
    Task<ServiceResource> GetResourceFromResourceList(string resourceId = null, string org = null, string app = null, string serviceCode = null, string serviceEditionCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Party based on partyId if the party is in the users reporteelist
    /// </summary>
    /// <param name="userId">The id of the authenticated user</param>
    /// <param name="partyId">The party Id of the party to retrieve</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Party that corresponds to partyId parameter if it's in the users reporteelist</returns>
    Task<Party> GetPartyForUser(int userId, int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resources having any of the request subjects in one or more resource policy rules
    /// </summary>
    /// <param name="subjects">Urn string representation of the subjects to lookup resources for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Dictionary of all resources per subject, having policy rules with the subject</returns>
    Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default);
}
