using Altinn.Brigde.Enums;
using Altinn.Brigde.Models;

namespace Altinn.Brigde.Services
{
    public interface IDelegationRequestsWrapper
    {
        Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status, string? continuation);
    }
}
