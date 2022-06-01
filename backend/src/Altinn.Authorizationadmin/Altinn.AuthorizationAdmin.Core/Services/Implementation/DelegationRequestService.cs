using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Services;
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


        public async Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction)
        {
            return await _delegationRequestsWrapper.GetDelegationRequestsAsync(requestedFromParty, requestedToParty, direction);
        }
    }
}
