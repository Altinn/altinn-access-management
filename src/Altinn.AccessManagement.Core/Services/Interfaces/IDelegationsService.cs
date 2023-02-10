using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for delegations
    /// </summary>
    public interface IDelegationsService
    {
        /// <summary>
        /// Gets all offered maskinporten schema delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetOfferedMaskinportenSchemaDelegations(AttributeMatch party);

        /// <summary>
        /// Gets all received maskinporten schema delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetReceivedMaskinportenSchemaDelegations(AttributeMatch party);

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
        /// <param name="authenticatedUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="authenticatedUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="delegation">The delegation</param>
        /// <returns>The result of the delegation</returns>
        public Task<DelegationOutput> MaskinportenDelegation(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationInput delegation);
    }
}
