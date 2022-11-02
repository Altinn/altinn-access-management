using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for client for getting Altinn roles from AltinnII SBL Bridge
    /// </summary>
    public interface IAltinnRolesClient
    {
        /// <summary>
        /// Get the decision point roles for the loggedin user for a selected party
        /// </summary>
        /// <param name="coveredByUserId">the logged in user id</param>
        /// <param name="offeredByPartyId">the partyid of the person/org the logged in user is representing</param>
        /// <returns>list of actors that the logged in user can represent</returns>
        Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId);
    }
}
