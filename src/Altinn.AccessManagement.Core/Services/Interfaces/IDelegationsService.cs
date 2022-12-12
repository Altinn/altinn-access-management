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

        /// <summary> 
        /// Gets all the delegations for an admin or owner
        /// </summary>
        /// <param name="supplierOrg">the organisation number of the supplier org</param>
        /// <param name="consumerOrg">the organisation number of the consumer of the resource</param>
        /// <param name="scope">the scope of the resource</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetMaskinportenSchemaDelegations(string supplierOrg, string consumerOrg, string scope);

        /// <summary>
        /// Gets all the delegation changes covering a user, both directly delegated or inheirited (through keyroles or from mainunit),
        /// for a given reportee and resource.
        /// </summary>
        /// <param name="subjectUserId">The user id to find delegations for</param>
        /// <param name="reporteePartyId">The party id of the reportee to find delegations from</param>
        /// <param name="resourceId">The resource to find delegations of. Either a resource registry id or an altinn app</param>
        /// <param name="resourceMatchType">Indicator whether the resourceId is a resource from the Resource Registry or an Altinn App</param>
        /// <returns>List of delgation changes</returns>
        public Task<List<DelegationChange>> FindAllDelegations(int subjectUserId, int reporteePartyId, string resourceId, ResourceAttributeMatchType resourceMatchType);
    }
}
