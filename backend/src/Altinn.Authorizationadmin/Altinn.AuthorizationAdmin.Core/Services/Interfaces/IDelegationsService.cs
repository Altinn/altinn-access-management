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
        public Task<List<ResourceDelegation>> GetDelegatedResourcesAsync(int offeredbyPartyId);

        /// <summary>
        /// Gets all received delegations for a reportee
        /// </summary>
        /// <param name="coveredByPartyId">reportee id that received delegations</param>
        /// <returns>list of received delgations</returns>
        public Task<List<Delegation>> GetReceivedDelegationsAsync(int coveredByPartyId);
    }
}
