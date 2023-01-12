using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for register
    /// </summary>
    public interface IRegister
    {
        /// <summary>
        /// Gets an organization for an organization number
        /// </summary>
        /// <param name="organisationNumber">the organisation number</param>
        /// <returns>organisation information</returns>
        public Task<Party> GetOrganisation(string organisationNumber);

        /// <summary>
        /// Gets a party based on partyId
        /// </summary>
        /// <param name="partyId">The id of party to look for</param>
        /// <param name="userId">The id of authenticated user</param>
        /// <returns>Party based on partyId</returns>
        public Task<Party> GetPartyForPartyId(int partyId, int userId);

        /// <summary>
        /// Gets a list of parties based on userId
        /// </summary>
        /// <param name="userId">The id of the authenticated user</param>
        /// <returns>List of parties for user</returns>
        public Task<List<Party>> GetPartiesForUser(int userId);
    }
}
