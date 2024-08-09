using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for operations regarding MaskinportenSchema delegations
    /// </summary>
    public interface IMaskinportenSchemaService
    {
        /// <summary>
        /// Performs a delegation check for the authenticated user on behalf of the from party, to find if and what rights the user can delegate to the to party, for the given maskinportenschema.
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="authenticatedUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="request">The model describing the right delegation check to perform</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The result of the delegation status check</returns>
        public Task<DelegationCheckResponse> DelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, RightsDelegationCheckRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all offered maskinporten schema delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetOfferedMaskinportenSchemaDelegations(AttributeMatch party, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all received maskinporten schema delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetReceivedMaskinportenSchemaDelegations(AttributeMatch party, CancellationToken cancellationToken = default);

        /// <summary> 
        /// Gets all the delegations for an admin or owner
        /// </summary>
        /// <param name="supplierOrg">the organisation number of the supplier org</param>
        /// <param name="consumerOrg">the organisation number of the consumer of the resource</param>
        /// <param name="scope">the scope of the resource</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetMaskinportenDelegations(string supplierOrg, string consumerOrg, string scope, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs the delegation on behalf of the from party
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="authenticatedUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="delegation">The delegation</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The result of the delegation</returns>
        public Task<DelegationActionResult> DelegateMaskinportenSchema(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Operation to revoke a maskinporten schema delegation
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user deleting the delegation</param>
        /// <param name="delegation">The delegation lookup model</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The result of the deletion</returns>
        public Task<DelegationActionResult> RevokeMaskinportenSchemaDelegation(int authenticatedUserId, DelegationLookup delegation, CancellationToken cancellationToken = default);
    }
}
