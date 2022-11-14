using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for delegations
    /// </summary>
    public interface IDelegationsService
    {
        /// <summary>
        /// Gets all delegated resources for a reportee
        /// </summary>
        /// <param name="who">reportee that delegated resources</param>
        /// <param name="resourceType">the type of resource that was delegated</param>
        /// <returns>list o delgations</returns>
        public Task<List<Delegation>> GetAllOutboundDelegationsAsync(string who, ResourceType resourceType);

        /// <summary>
        /// Gets all the rceived delegations for a reportee
        /// </summary>
        /// <param name="who">reportee that delegated resources</param>
        /// <param name="resourceType">the type of resource that was delegated</param>
        /// <returns>list o delgations</returns>
        public Task<List<Delegation>> GetAllInboundDelegationsAsync(string who, ResourceType resourceType);
    }
}
