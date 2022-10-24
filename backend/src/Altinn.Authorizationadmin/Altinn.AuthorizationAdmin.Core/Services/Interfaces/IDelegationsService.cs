using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;

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
        /// <param name="resourceType">the type of resource that was delegated</param>
        /// <returns>list o delgations</returns>
        public Task<List<OfferedDelegations>> GetAllOfferedDelegations(int offeredbyPartyId, ResourceType resourceType);

        /// <summary>
        /// Gets all the rceived delegations for a reportee
        /// </summary>
        /// <param name="coveredByPartyId">reportee id that delegated resources</param>
        /// <param name="resourceType">the type of resource</param>
        /// <returns>list o delgations</returns>
        public Task<List<ReceivedDelegation>> GetReceivedDelegationsAsync(int coveredByPartyId, ResourceType resourceType);

    }
}
