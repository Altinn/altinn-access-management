using Altinn.AccessManagement.Core.Enums;
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
        /// <param name="party">reportee that delegated resources</param>
        /// <param name="resourceType">the type of resource that was delegated</param>
        /// <returns>list o delgations</returns>
        public Task<List<Delegation>> GetAllOutboundDelegationsAsync(string party, ResourceType resourceType);

        /// <summary>
        /// Gets all the rceived delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <param name="resourceType">the type of resource that was delegated</param>
        /// <returns>list o delgations</returns>
        public Task<List<Delegation>> GetAllInboundDelegationsAsync(string party, ResourceType resourceType);

        /// <summary> 
        /// Gets all the delegations for an admin or owner
        /// </summary>
        /// <param name="supplierOrg">the organisation number of the supplier org</param>
        /// <param name="consumerOrg">the organisation number of the consumer of the resource</param>
        /// <param name="scope">the scope of the resource</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetMaskinportenSchemaDelegations(string supplierOrg, string consumerOrg, string scope);

        /// <summary>
        /// Performs the delegation on behalf of the from party
        /// </summary>
        /// <param name="delegatingUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="delegatingUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="from">The offering party</param>
        /// <param name="delegation">The delegation</param>
        /// <returns>The result of the delegation</returns>
        public Task<DelegationOutput> MaskinportenDelegation(int delegatingUserId, int delegatingUserAuthlevel, string from, DelegationInput delegation);
    }
}
