using Altinn.AccessManagement.Core.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for client for getting Altinn rights from AltinnII SBL Bridge
    /// </summary>
    public interface IAltinn2RightsClient
    {
        /// <summary>
        /// Get Altinn rights from AltinnII SBL bridge
        /// </summary>
        /// <param name="userId">the logged in user id</param>
        /// /// <param name="request">the request</param>
        /// <returns>Delegation Check Response</returns>
        Task<DelegationCheckResponse> PostDelegationCheck(int userId, RightsDelegationCheckRequest request);
    }
}
