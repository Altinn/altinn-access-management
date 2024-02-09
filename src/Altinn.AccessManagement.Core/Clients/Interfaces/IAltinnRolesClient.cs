using Altinn.AccessManagement.Core.Models;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces;

/// <summary>
/// Interface for client for getting Altinn roles from AltinnII SBL Bridge
/// </summary>
public interface IAltinnRolesClient
{
    /// <summary>
    /// Get the decision point roles for the loggedin user for a selected party
    /// </summary>
    /// <param name="coveredByUserId">the logged in user id</param>
    /// <param name="offeredByPartyId">the partyid of the person/org the logged in user is representing</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the roles the user has for a given reportee, as basis for evaluating rights for delegation.
    /// For any user having HADM this means, getting additional roles as DAGL etc.
    /// </summary>
    /// <param name="coveredByUserId">the user id</param>
    /// <param name="offeredByPartyId">the partyid of the person/org the user is representing</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>list of actors that the logged in user can represent</returns>
    Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the list of authorized parties from Altinn 2 that a given user have one or more accesses for, including 
    /// </summary>
    /// <param name="userId">The user to get the list of AuthorizedParties for</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns></returns>
    Task<List<AuthorizedParty>> GetAuthorizedPartiesWithRoles(int userId, CancellationToken cancellationToken = default);
}
