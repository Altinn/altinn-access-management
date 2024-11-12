using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Tests.Seeds;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <summary>
/// Mock class for <see cref="IPartiesClient"></see> interface
/// </summary>
public class PartiesClientMock : IPartiesClient
{
    private Dictionary<int, Party> AdditionalParties { get; set; } = new Dictionary<int, Party>()
    {
        { PersonSeeds.Paula.PartyId, PersonSeeds.Paula.Party },
        { PersonSeeds.Kasper.PartyId, PersonSeeds.Kasper.Party },
        { PersonSeeds.Olav.PartyId, PersonSeeds.Olav.Party },
    };

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        var result = new List<Party>();
        foreach (var partyId in partyIds)
        {
            if (AdditionalParties.TryGetValue(partyId, out var party))
            {
                result.Add(party);
            }
        }

        List<Party> partyList = GetTestDataParties();
        List<Party> filteredList = (from int partyId in partyIds.Distinct()
                                    let party = partyList.Find(p => p.PartyId == partyId)
                                    where party != null
                                    select party).ToList();
        result.AddRange(filteredList);
        return Task.FromResult(filteredList);
    }

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesAsync(List<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = GetTestDataParties();
        return Task.FromResult((from Guid partyUuid in partyUuids.Distinct()
                                let party = partyList.Find(p => p.PartyUuid == partyUuid)
                                where party != null
                                select party).ToList());
    }

    /// <inheritdoc/>
    public Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetTestDataParties().Find(p => p.PartyId == partyId));
    }

    /// <inheritdoc/>
    public Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = GetTestDataParties();
        Party party = null;

        if (!string.IsNullOrWhiteSpace(partyLookup.OrgNo))
        {
            party = partyList.Find(p => p.Organization?.OrgNumber == partyLookup.OrgNo);
        }
        else if (!string.IsNullOrWhiteSpace(partyLookup.Ssn))
        {
            party = partyList.Find(p => p.Person?.SSN == partyLookup.Ssn);
        }

        return Task.FromResult(party);
    }

    /// <inheritdoc/>
    public Task<List<int>> GetKeyRoleParties(int userId, CancellationToken cancellationToken = default)
    {
        List<int> keyRoleUnitPartyIds = new();

        string keyRoleUnitsPath = GetKeyRoleUnitsPaths(userId);
        if (File.Exists(keyRoleUnitsPath))
        {
            string content = File.ReadAllText(keyRoleUnitsPath);
            keyRoleUnitPartyIds = (List<int>)JsonSerializer.Deserialize(content, typeof(List<int>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return Task.FromResult(keyRoleUnitPartyIds);
    }

    /// <inheritdoc/>
    public Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds, CancellationToken cancellationToken = default)
    {
        List<MainUnit> mainUnits = new();

        foreach (int subunitPartyId in subunitPartyIds.PartyIds)
        {
            string mainUnitsPath = GetMainUnitsPath(subunitPartyId);
            if (File.Exists(mainUnitsPath))
            {
                string content = File.ReadAllText(mainUnitsPath);
                List<MainUnit> readMainUnits = (List<MainUnit>)JsonSerializer.Deserialize(content, typeof(List<MainUnit>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                mainUnits.AddRange(readMainUnits);
            }
        }

        return Task.FromResult(mainUnits);
    }

    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetTestDataParties());
    }

    private static List<Party> GetTestDataParties()
    {
        List<Party> partyList = new List<Party>();

        string partiesPath = GetPartiesPath();
        if (File.Exists(partiesPath))
        {
            string content = File.ReadAllText(partiesPath);
            partyList = JsonSerializer.Deserialize<List<Party>>(content);
        }

        return partyList;
    }

    private static string GetPartiesPath()
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "Parties", "parties.json");
    }

    private static string GetMainUnitsPath(int subunitPartyId)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "MainUnits", $"{subunitPartyId}", "mainunits.json");
    }

    private static string GetKeyRoleUnitsPaths(int userId)
    {
        string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "KeyRoleUnits", $"{userId}", "keyroleunits.json");
    }
}
