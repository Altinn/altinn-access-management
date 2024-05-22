using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Contexts;

/// <inheritdoc/>
public class PartiesClientMock(MockContext context) : IPartiesClient
{
    private MockContext Context { get; } = context;

    /// <inheritdoc/>
    public Task<List<int>> GetKeyRoleParties(int userId, CancellationToken cancellationToken = default)
    {
        Context.KeyRoles.TryGetValue(userId, out var result);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds, CancellationToken cancellationToken = default) =>
        Task.FromResult(subunitPartyIds.PartyIds
            .Where(Context.MainUnits.ContainsKey)
            .Select(party => Context.MainUnits[party])
            .ToList());

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Parties.Where(party => partyIds.Contains(party.PartyId)).ToList());

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesAsync(List<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Parties.Where(party => partyUuids.Contains(party.PartyUuid ?? Guid.Empty)).ToList());

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.UserProfiles.Where(user => user.UserId == userId).Select(user => user.Party).ToList());

    /// <inheritdoc/>
    public Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Parties.FirstOrDefault(party => party.PartyId == partyId));

    /// <inheritdoc/>
    public Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.Parties.FirstOrDefault(party => party.SSN == partyLookup.Ssn || party.OrgNumber == partyLookup.OrgNo));
}