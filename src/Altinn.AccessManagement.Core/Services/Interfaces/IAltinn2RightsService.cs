using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Used by Altinn for managing delegations
/// </summary>
public interface IAltinn2RightsService
{
    /// <summary>
    /// Gets all offered single rights delegations for a reportee
    /// </summary>
    /// <param name="partyId">reportee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>list of delgations</returns>
    Task<IEnumerable<RightDelegation>> GetOfferedRights(int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all received single rights delegations for a reportee
    /// </summary>
    /// <param name="partyId">reportee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>list of delgations</returns>
    Task<List<RightDelegation>> GetReceivedRights(int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Operation to clear a recipients cached rights from a given reportee/from party, and the recipients authorized parties/reportees
    /// </summary>
    /// <param name="fromPartyId">The party id of the from party</param>
    /// <param name="toAttribute">Attribute model identifying the recipient/to party</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HttpResponse</returns>
    Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, BaseAttribute toAttribute, CancellationToken cancellationToken = default);
}