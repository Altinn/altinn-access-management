using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for a client wrapper for integration with SBL bridge delegation request API
    /// </summary>
    public interface IPartiesClient
    {
        /// <summary>
        /// Returns partyInfo
        /// </summary>
        /// <returns>party information</returns>
        Task<Party> GetPartyAsync(int partyId);

        /// <summary>
        /// Returns a list of parties
        /// </summary>
        /// <returns>List of parties</returns>
        Task<List<Party>> GetPartiesAsync(List<int> parties);

        /// <summary>
        /// Returns partyid for 
        /// </summary>
        /// <returns>List of parties</returns>
        Task<int> GetPartyId(string id);

        /// <summary>
        /// Method that fetches a list of PartyIds the given user id has key role access to (where the user inherit delegations to their organization)
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>list of PartyIds where the logged in user have key role access</returns>
        Task<List<int>> GetKeyRoleParties(int userId);

        /// <summary>
        /// Method that fetches a list of main units for the input list of sub unit partyIds. If any of the input partyIds are not a sub unit the response model will have null values for main unit properties.
        /// </summary>
        /// <param name="subunitPartyIds">The list of PartyIds to check and retrieve any main units for</param>
        /// <returns>list of main units</returns>
        Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds);
    }
}
