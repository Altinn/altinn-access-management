using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Authorization.Platform.Authorization.Models;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAltinnRolesClient"></see> interface
    /// </summary>
    public class AltinnRolesClientMock : IAltinnRolesClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnRolesClientMock"/> class
        /// </summary>
        public AltinnRolesClientMock()
        {
        }

        /// <inheritdoc/>
        public Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId)
        {
            List<Role> roles = new List<Role>();
            string rolesPath = GetRolesPath(coveredByUserId, offeredByPartyId);
            if (File.Exists(rolesPath))
            {
                string content = File.ReadAllText(rolesPath);
                roles = (List<Role>)JsonSerializer.Deserialize(content, typeof(List<Role>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return Task.FromResult(roles);
        }

        /// <inheritdoc/>
        public Task<List<Role>> GetRolesForDelegation(int coveredByUserId, int offeredByPartyId)
        {
            List<Role> roles = new List<Role>();
            string rolesPath = GetRolesForDelegationPath(coveredByUserId, offeredByPartyId);
            if (File.Exists(rolesPath))
            {
                string content = File.ReadAllText(rolesPath);
                roles = (List<Role>)JsonSerializer.Deserialize(content, typeof(List<Role>), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return Task.FromResult(roles);
        }

        private static string GetRolesPath(int coveredByUserId, int offeredByPartyId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnRolesClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Roles", $"user_{coveredByUserId}", $"party_{offeredByPartyId}", "roles.json");
        }

        private static string GetRolesForDelegationPath(int coveredByUserId, int offeredByPartyId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnRolesClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "RolesForDelegation", $"user_{coveredByUserId}", $"party_{offeredByPartyId}", "roles.json");
        }
    }
}
