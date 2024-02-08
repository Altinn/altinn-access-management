using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

public interface IAltinn2DelegationsService
{
    /// <summary>
    /// Gets all offered single rights delegations for a reportee
    /// </summary>
    /// <param name="reportee">reportee</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>list of delgations</returns>
    Task<IEnumerable<RightDelegation>> GetOfferedRightsDelegations(AttributeMatch reportee, CancellationToken token = default);

    /// <summary>
    /// Gets all received single rights delegations for a reportee
    /// </summary>
    /// <param name="reportee">reportee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>list of delgations</returns>
    public Task<List<RightDelegation>> GetReceivedRightsDelegations(AttributeMatch reportee, CancellationToken cancellationToken = default);
}