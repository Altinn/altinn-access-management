using Altinn.AuthorizationAdmin.Core.Models;

namespace Altinn.AuthorizationAdmin.Services
{
    public interface IDelegationRequestsWrapper
    {
        Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction);
    }
}
