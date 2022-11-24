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
        /// Returns a party
        /// </summary>
        /// <returns>Party</returns>
        Task<Party> GetPartyAsync(int partyId);

        /// <summary>
        /// Returns a list of parties
        /// </summary>
        /// <returns>List of parties</returns>
        Task<List<Party>> GetPartiesAsync(List<int> partyIds);

        /// <summary>
        /// Returns looks up a PartyId for an organization number or ssn
        /// </summary>
        /// <returns>PartyId if exists</returns>
        Task<int> GetPartyId(string ssnOrOrgno);

        /// <summary>
        /// Method that fetches a list of PartyIds the given user id has key role access to (where the user inherit delegations to their organization)
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>list of PartyIds where the logged in user have key role access</returns>
        Task<List<int>> GetKeyRolePartyIds(int userId);

        /// <summary>
        /// Method that fetches a main unit for the input sub unit partyId. If the input partyId is not a sub unit the response model will have null values for main unit properties.
        /// </summary>
        /// <param name="subunitPartyId">The PartyId to check and retrieve any main unit for</param>
        /// <returns>main units</returns>
        Task<List<MainUnit>> GetMainUnits(int subunitPartyId);

        /// <summary>
        /// Integration point for retrieving a single resoure by it's resource id
        /// </summary>
        /// <param name="resourceRegistryId">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceRegistryId);

        /// <summary>
        /// Integration point for retrieving a list of resources by it's resource id
        /// </summary>
        /// <param name="resourceIds">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource list if exists</returns>
        Task<List<ServiceResource>> GetResources(List<string> resourceIds);
    }
}
