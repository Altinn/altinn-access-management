using Altinn.AuthorizationAdmin.Core.Models;

namespace Altinn.AuthorizationAdmin.Core.Services.Interfaces
{
    /// <summary>
    /// Service for delegations
    /// </summary>
    public interface IDelegationsService
    {
        /// <summary>
        /// Gets all delegated resources for a reportee
        /// </summary>
        /// <param name="offeredbyPartyId">reportee id that delegated resources</param>
        /// <returns>list o delgations</returns>
        public Task<List<DelegatedResources>> GetApiDelegationsByOfferedbyAsync(int offeredbyPartyId);
    }
}
