using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;

namespace Altinn.AuthorizationAdmin.Services
{
    public interface IDelegationRequests
    {
        Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode,int? serviceEditionCode,RestAuthorizationRequestDirection direction,List<RestAuthorizationRequestStatus>? status,string? continuation);
    }
}
