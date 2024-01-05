using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for operations regarding single rights delegations
    /// </summary>
    public interface ISingleRightsService
    {
        /// <summary>
        /// Performs a delegation check for the authenticated user on behalf of the from party, to find if and what rights the user can delegate to the to party, for the given resource.
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="authenticatedUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="request">The model describing the right delegation check to perform</param>
        /// <returns>The result of the delegation status check</returns>
        public Task<DelegationCheckResponse> RightsDelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, RightsDelegationCheckRequest request);

        /// <summary>
        /// Performs the delegation on behalf of the from party
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user performing the delegation</param>
        /// <param name="authenticatedUserAuthlevel">The authentication level of the authenticated user performing the delegation</param>
        /// <param name="delegation">The delegation</param>
        /// <returns>The result of the delegation</returns>
        public Task<DelegationActionResult> DelegateRights(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation);

        /// <summary>
        /// Gets all offered single rights delegations for a reportee
        /// </summary>
        /// <param name="reportee">reportee</param>
        /// <param name="token">cancellation token</param>
        /// <returns>list of delgations</returns>
        Task<IEnumerable<RightDelegation>> GetOfferedRightsDelegations(AttributeMatch reportee, CancellationToken token = default);

        /// <summary>
        /// Gets all received single rights delegations for a reportee
        /// </summary>
        /// <param name="party">reportee that delegated resources</param>
        /// <returns>list of delgations</returns>
        public Task<List<Delegation>> GetReceivedRightsDelegations(AttributeMatch party);

        /// <summary>
        /// Operation to revoke a single rights delegation
        /// </summary>
        /// <param name="authenticatedUserId">The user id of the authenticated user deleting the delegation</param>
        /// <param name="delegation">The delegation lookup model</param>
        /// <returns>The result of the deletion</returns>
        public Task<DelegationActionResult> RevokeRightsDelegation(int authenticatedUserId, DelegationLookup delegation);
    }
}
