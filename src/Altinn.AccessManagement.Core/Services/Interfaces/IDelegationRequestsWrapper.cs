using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for a client wrapper for integration with SBL bridge delegation request API
    /// </summary>
    public interface IDelegationRequestsWrapper
    {
        /// <summary>
        /// Returns a list of DelegationRequests
        /// </summary>
        /// <param name="who">The reportee to get delegation requests for</param>
        /// <param name="serviceCode">Optional filter parameter for serviceCode</param>
        /// <param name="serviceEditionCode">Optional filter parameter for serviceEditionCode</param>
        /// <param name="direction">Optional filter parameter for directions (incoming, outgoing). If no direction is specified, both incoming and outgoing requests will be returned</param>
        /// <param name="status">Optional filter parameter for status. (created, unopened, approved, rejected, deleted)</param>
        /// <param name="continuation">Optional filter parameter for continuationToken</param>
        /// <returns>List of delegation requests</returns>
        Task<DelegationRequests> GetDelegationRequestsAsync(string who, string serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus> status, string continuation);
    }
}
