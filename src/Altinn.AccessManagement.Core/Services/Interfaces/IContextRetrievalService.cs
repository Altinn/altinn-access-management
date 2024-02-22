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
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId);

    /// <summary>
    /// Get the roles the user has for a given reportee, as basis for evaluating rights for delegation.
    /// For any user having HADM this means, getting additional roles as DAGL etc.
    /// </summary>
    /// <param name="coveredByUserId">the user id</param>
    /// <param name="offeredByPartyId">the partyid of the person/org the user is representing</param>
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId);

    /// <summary>
    /// Gets a single party by its party id
    /// </summary>
    /// <returns>Party</returns>
    Task<Party> GetPartyAsync(int partyId);

    /// <summary>
    /// Gets a list of parties by their party ids
    /// </summary>
    /// <param name="partyIds">List of partyIds to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the lookup party IDs are for a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single party by its party uuid
    /// </summary>
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
    /// Gets the party of an organization
    /// </summary>
    /// <param name="organizationNumber">The organization number to lookup party</param>
    /// <returns>Party</returns>
    Task<Party> GetPartyForOrganization(string organizationNumber);

    /// <summary>
    /// Gets the party of a person
    /// </summary>
    /// <param name="ssn">The social security number to lookup party</param>
    /// <returns>Party</returns>
    Task<Party> GetPartyForPerson(string ssn);

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
    /// <returns>main units</returns>
    Task<List<MainUnit>> GetMainUnits(int subunitPartyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a single resoure by it's resource id if registered in the Resource Registry
    /// </summary>
    /// <param name="resourceRegistryId">The identifier of the resource in the Resource Registry</param>
    /// <returns>The resource if exists</returns>
    Task<ServiceResource> GetResource(string resourceRegistryId);

    /// <summary>
    /// Gets a list of all resources from the Resource Registry
    /// </summary>
    /// <returns>The resource list</returns>
    Task<List<ServiceResource>> GetResources();

    /// <summary>
    /// Gets a list of all available resources including Altinn Apps, Altinn 2 services and resources from the Resource Registry
    /// </summary>
    /// <returns>The resource list</returns>
    Task<List<ServiceResource>> GetResourceList();

    /// <summary>
    /// Gets a single resource from the list of all available resources including Altinn Apps, Altinn 2 services and resources from the Resource Registry, if it exists.
    /// </summary>
    /// <returns>The resource if exists</returns>
    Task<ServiceResource> GetResourceFromResourceList(string resourceId = null, string org = null, string app = null, string serviceCode = null, string serviceEditionCode = null);

    /// <summary>
    /// Gets a Party based on partyId if the party is in the users reporteelist
    /// </summary>
    /// <param name="userId">The id of the authenticated user</param>
    /// <param name="partyId">The party Id of the party to retrieve</param>
    /// <returns>Party that corresponds to partyId parameter if it's in the users reporteelist</returns>
    Task<Party> GetPartyForUser(int userId, int partyId);
}
