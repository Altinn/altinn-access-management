using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// The servcie implementation for operations related to working with delegation requests
    /// </summary>
    public class DelegationRequestService : IDelegationRequests
    {
        private readonly IDelegationRequestsWrapper _delegationRequestsWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequestService"/> class
        /// </summary>
        /// <param name="delegationRequestsWrapper">Delegation request client wrapper</param>
        public DelegationRequestService(IDelegationRequestsWrapper delegationRequestsWrapper)
        {
            _delegationRequestsWrapper = delegationRequestsWrapper;
        }

        /// <inheritdoc/>
        public async Task<DelegationRequests> GetDelegationRequestsAsync(string who, string serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus> status = null, string continuation = "")
        {
            return await _delegationRequestsWrapper.GetDelegationRequestsAsync(who, serviceCode, serviceEditionCode, direction, status, continuation);
        }
    }
}
