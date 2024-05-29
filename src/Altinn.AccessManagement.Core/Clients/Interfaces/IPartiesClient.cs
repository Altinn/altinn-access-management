using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces;

/// <summary>
/// Interface for a client wrapper for integration with SBL bridge delegation request API
/// </summary>
public interface IPartiesClient
{
    /// <summary>
    /// Returns partyInfo
    /// </summary>
    /// <param name="partyId">The party ID to lookup</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>party information</returns>
    Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of parties
    /// </summary>
    /// <param name="partyIds">List of party IDs to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the lookup party IDs are for a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of parties
    /// </summary>
    /// <param name="partyUuids">List of party uuids to lookup</param>
    /// <param name="includeSubunits">(Optional) Whether subunits should be included as ChildParties, if any of the parties are a main unit</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<List<Party>> GetPartiesAsync(List<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of parties for user
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of parties</returns>
    Task<List<Party>> GetPartiesForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Method that fetches a list of PartyIds the given user id has key role access to (where the user inherit delegations to their organization)
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>list of PartyIds where the logged in user have key role access</returns>
    Task<List<int>> GetKeyRoleParties(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Method that fetches a list of main units for the input list of sub unit partyIds. If any of the input partyIds are not a sub unit the response model will have null values for main unit properties.
    /// </summary>
    /// <param name="subunitPartyIds">The list of PartyIds to check and retrieve any main units for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>list of main units</returns>
    Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up a party based on SSN or OrgNumber.
    /// </summary>
    /// <param name="partyLookup">
    /// SSN or OrgNumber as a PartyLookup object.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>
    /// The party that represents the given SSN or OrgNumber.
    /// </returns>
    Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup, CancellationToken cancellationToken = default);
}
