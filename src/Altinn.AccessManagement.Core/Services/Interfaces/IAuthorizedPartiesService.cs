using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Service for operations regarding retrieval of authorized parties (aka reporteelist)
/// </summary>
public interface IAuthorizedPartiesService
{
    /// <summary>
    /// Gets the full unfiltered list of authorized parties the given user can represent in Altinn
    /// </summary>
    /// <param name="authenticatedUserId">The user id of the authenticated user to retrieve the authorized party list for</param>
    /// <param name="includeAltinn2AuthorizedParties">Whether Authorized Parties from Altinn 2 should be included in the result set</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The unfiltered party list</returns>
    public Task<List<AuthorizedParty>> GetAuthorizedParties(int authenticatedUserId, bool includeAltinn2AuthorizedParties, CancellationToken cancellationToken);
}
