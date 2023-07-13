using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
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
        /// <returns>List of parties</returns>
        Task<List<Party>> GetPartiesAsync(List<int> partyIds);

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
        /// <returns>list of PartyIds where the logged in user have key role access</returns>
        Task<List<int>> GetKeyRolePartyIds(int userId);

        /// <summary>
        /// Gets a main unit for the input sub unit partyId. If the input partyId is not a sub unit the response model will have null values for main unit properties.
        /// </summary>
        /// <param name="subunitPartyId">The PartyId to check and retrieve any main unit for</param>
        /// <returns>main units</returns>
        Task<List<MainUnit>> GetMainUnits(int subunitPartyId);

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
        Task<ServiceResource> GetResourceFromResourceList(string resourceId, string org, string app, string serviceCode, string serviceEditionCode);

        /// <summary>
        /// Gets a Party based on partyId if the party is in the users reporteelist
        /// </summary>
        /// <param name="userId">The id of the authenticated user</param>
        /// <param name="partyId">The party Id of the party to retrieve</param>
        /// <returns>Party that corresponds to partyId parameter if it's in the users reporteelist</returns>
        public Task<Party> GetPartyForUser(int userId, int partyId);
    }
}
