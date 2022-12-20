using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IPartiesClient"></see> interface
    /// </summary>
    public class PartiesClientMock : IPartiesClient
    {
        /// <summary>
        /// Party information for a list of party numbers
        /// </summary>
        /// <param name="parties"> list of party numbers</param>
        /// <returns>party information list</returns>
        public Task<List<Party>> GetPartiesAsync(List<int> parties)
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
                    filteredList.Add(partyList.Find(p => p.PartyId == partyId));
                }
            }

            return Task.FromResult(filteredList);
        }

        /// <inheritdoc/>
        public Task<Party> GetPartyAsync(int partyId)
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
        public Task<int> GetPartyId(string ssnOrOrgno)
        {
            List<Party> partyList = new List<Party>();
            Party party = null;
            int partyId = 0;

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

                party = partyList.Find(p => p.SSN.Equals(ssnOrOrgno.ToString()) || p.OrgNumber.Equals(ssnOrOrgno.ToString()));

                partyId = party != null ? party.PartyId : 0; 
            }

            return Task.FromResult(partyId);
        }

        /// <inheritdoc/>
        public Task<Party> LookupPartyBySSNOrOrgNo(string orgnummer)
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

                party = partyList.Find(p => p.Organization?.OrgNumber == orgnummer);
            }

            return Task.FromResult(party);
        }

        /// <inheritdoc/>
        public Task<List<int>> GetKeyRoleParties(int userId)
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
        public Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds)
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
}
