using Altinn.AccessManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The result of the delegation</returns>
        public Task<DelegationActionResult> DelegateRights(int authenticatedUserId, int authenticatedUserAuthlevel, DelegationLookup delegation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all offered single rights delegations for a reportee
        /// </summary>
        /// <param name="reportee">reportee</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>list of delgations</returns>
        Task<IEnumerable<RightDelegation>> GetOfferedRights(AttributeMatch reportee, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all received single rights delegations for a reportee
        /// </summary>
        /// <param name="reportee">reportee</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>list of delgations</returns>
        public Task<List<RightDelegation>> GetReceivedRights(AttributeMatch reportee, CancellationToken cancellationToken = default);

        /// <summary>
        /// Operation to revoke a single rights delegation
        /// </summary>
        /// <param name="authenticatedUserId">authenticed user</param>
        /// <param name="delegation">delegation</param>
        /// <param name="cancellationToken">http context token</param>
        /// <returns>The result of the deletion</returns>
        Task<ValidationProblemDetails> RevokeRightsDelegation(int authenticatedUserId, DelegationLookup delegation, CancellationToken cancellationToken);
    }
}
