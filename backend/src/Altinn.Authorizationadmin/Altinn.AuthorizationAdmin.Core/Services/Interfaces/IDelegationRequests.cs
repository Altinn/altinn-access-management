using Altinn.AuthorizationAdmin.Core.Models;

namespace Altinn.AuthorizationAdmin.Services
{
    public interface IDelegationRequests
    {
        Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction);
    }
}
