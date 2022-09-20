using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AuthorizationAdmin.Core.Services.Implementation
{
    public class DelegationRequestService : IDelegationRequests
    {
        private readonly IDelegationRequestsWrapper _delegationRequestsWrapper;

        public DelegationRequestService(IDelegationRequestsWrapper delegationRequestsWrapper)
        {
            _delegationRequestsWrapper = delegationRequestsWrapper;
        }

        public async Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode , int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status = null, string? continuation = "")
        {
            return await _delegationRequestsWrapper.GetDelegationRequestsAsync(who, serviceCode, serviceEditionCode, direction, status, continuation);
        }
    }
}
