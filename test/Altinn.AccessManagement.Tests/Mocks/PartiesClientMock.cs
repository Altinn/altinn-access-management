using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <summary>
/// Mock class for <see cref="IPartiesClient"></see> interface
/// </summary>
public class PartiesClientMock : IPartiesClient
{
    /// <inheritdoc/>
    public Task<List<Party>> GetPartiesAsync(List<int> parties, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = new List<Party>();
        List<Party> filteredList = new List<Party>();

        string path = GetPartiesPaths();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                if (file.Contains("parties"))
                {
                    string content = File.ReadAllText(Path.Combine(path, file));                        
                    partyList = JsonSerializer.Deserialize<List<Party>>(content);
                }
            }

            foreach (int partyId in parties.Distinct())
            {
                Party party = partyList.Find(p => p.PartyId == partyId);
                if (party != null)
                {
                    filteredList.Add(party);
                }
            }
        }

        return Task.FromResult(filteredList);
    }

    /// <inheritdoc/>
    public Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = new List<Party>();
        Party party = null;

        string path = GetPartiesPaths();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                if (file.Contains("parties"))
                {
                    string content = File.ReadAllText(Path.Combine(path, file));
                    partyList = JsonSerializer.Deserialize<List<Party>>(content);
                }
            }

            party = partyList.Find(p => p.PartyId == partyId);
        }

        return Task.FromResult(party);
    }

    /// <inheritdoc/>
    public Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup, CancellationToken cancellationToken = default)
    {
        List<Party> partyList = new List<Party>();
        Party party = null;

        string path = GetPartiesPaths();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                if (file.Contains("parties"))
                {
                    string content = File.ReadAllText(Path.Combine(path, file));
                    partyList = JsonSerializer.Deserialize<List<Party>>(content);
                }
            }

            if (!string.IsNullOrWhiteSpace(partyLookup.OrgNo))
            {
                party = partyList.Find(p => p.Organization?.OrgNumber == partyLookup.OrgNo);
            }
            else if (!string.IsNullOrWhiteSpace(partyLookup.Ssn))
            {
                party = partyList.Find(p => p.Person?.SSN == partyLookup.Ssn);
            }                
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
        List<Party> partyList = new List<Party>();

        string path = GetPartiesPaths();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                if (file.Contains("parties"))
                {
                    string content = File.ReadAllText(Path.Combine(path, file));
                    partyList = JsonSerializer.Deserialize<List<Party>>(content);
                }
            }
        }

        return Task.FromResult(partyList);
    }

    private static string GetPartiesPaths()
    {
        string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "Parties");
    }

    private static string GetMainUnitsPath(int subunitPartyId)
    {
        string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "MainUnits", $"{subunitPartyId}", "mainunits.json");
    }

    private static string GetKeyRoleUnitsPaths(int userId)
    {
        string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
        return Path.Combine(unitTestFolder, "Data", "KeyRoleUnits", $"{userId}", "keyroleunits.json");
    }

    private static string GetFilterFileName(int offeredByPartyId)
    {
        return "parties";
    }
}
