﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AuthorizationAdmin.Core.Clients;
using Altinn.AuthorizationAdmin.Integration.Clients;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AuthorizationAdmin.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IPartiesClient"></see> interface
    /// </summary>
    public class PartiesClientMock : IPartiesClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartiesClientMock"/> class
        /// </summary>
        /// <param name="logger">the logger</param>
        public PartiesClientMock(ILogger<PartiesClientMock> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Party information for a list of party numbers
        /// </summary>
        /// <param name="parties"> list of party numbers</param>
        /// <returns>party information list</returns>
        public Task<List<Party>> GetPartiesAsync(List<int> parties)
        {
            List<Party> partyList = new List<Party>();
            List<Party> filteredList = new List<Party>();

            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClientMock // GetPartiesAsync // Exception");
                throw;
            }

            return Task.FromResult(filteredList);
        }

        private static string GetPartiesPaths()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, @"Data\Parties\");
        }

        private static string GetFilterFileName(int offeredByPartyId)
        {
            return "parties";
        }
    }
}
