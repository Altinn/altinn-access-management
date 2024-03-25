using Altinn.AccessManagement.Core.Models;

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
        /// <param name="authenticatedUserId">the authenticated user id</param>
        /// <param name="reporteePartyId">the party id of the reportee/from party</param>
        /// <param name="serviceCode">the service code</param>
        /// <param name="serviceEditionCode">the service edition code</param>
        /// <returns>Delegation Check Response</returns>
        Task<DelegationCheckResponse> PostDelegationCheck(int authenticatedUserId, int reporteePartyId, string serviceCode, string serviceEditionCode);

        /// <summary>
        /// Post delegation of Altinn 2 service rights to SBL bridge
        /// </summary>
        /// <param name="authenticatedUserId">the authenticated user id</param>
        /// <param name="reporteePartyId">the party id of the reportee/from party</param>
        /// <param name="delegationRequest">the delegation request model</param>
        /// <returns>Delegation Response</returns>
        Task<DelegationActionResult> PostDelegation(int authenticatedUserId, int reporteePartyId, SblRightDelegationRequest delegationRequest);

        /// <summary>
        /// Operation to clear a recipients cached rights from a given reportee/from party, and the recipients authorized parties/reportees
        /// </summary>
        /// <param name="fromPartyId">The party id of the from party</param>
        /// <param name="toPartyId">The party id of the to party</param>
        /// <param name="toUserId">The user id of the to party (if the recipient is a user)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HttpResponse</returns>
        Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, int toPartyId, int toUserId = 0, CancellationToken cancellationToken = default);
    }
}
