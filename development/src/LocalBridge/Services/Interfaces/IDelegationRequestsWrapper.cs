using Altinn.Brigde.Models;

namespace Altinn.Brigde.Services
{
    public interface IDelegationRequestsWrapper
    {
        Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction);
    }
}
