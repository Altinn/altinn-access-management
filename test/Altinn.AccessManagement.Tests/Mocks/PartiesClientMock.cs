using System;
using System.Collections.Generic;
using System.IO;
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

                foreach (int partyId in parties)
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
        public int GetPartyId(int ssnOrOrgno)
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

            return partyId;
        }

        /// <inheritdoc/>
        public Task<List<int>> GetKeyRoleParties(int userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds)
        {
            throw new NotImplementedException();
        }

        private static string GetPartiesPaths()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Parties");
        }

        private static string GetFilterFileName(int offeredByPartyId)
        {
            return "parties";
        }
    }
}
