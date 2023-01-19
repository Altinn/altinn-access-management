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
        /// Gets a Party based on userId
        /// </summary>
        /// <param name="userId">The id of the authenticated user</param>
        /// <param name="partyId">The party Id of the party to retrieve</param>
        /// <returns>Party that corresponds to partyId parameter if it's in the users reporteelist</returns>
        public Task<Party> GetPartiesForUser(int userId, int partyId);
    }
}
