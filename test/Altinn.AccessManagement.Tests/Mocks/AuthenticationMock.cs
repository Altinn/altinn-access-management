using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Platform.Register.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAuthenticationClient"></see> interface
    /// </summary>
    public class AuthenticationMock : IAuthenticationClient
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        /// <inheritdoc/>
        public async Task<string> RefreshToken(CancellationToken cancellationToken = default) => await Task.FromResult(PrincipalUtil.GetAccessToken("sbl-authorization"));

        /// <inheritdoc/>
        public async Task<SystemUser> GetSystemUser(int partyId, string systemUserId, CancellationToken cancellationToken = default)
        {
            SystemUser systemUser = null;

            string systemUserPath = GetSystemUserPath(systemUserId);
            if (File.Exists(systemUserPath))
            {
                string content = File.ReadAllText(systemUserPath);
                systemUser = (SystemUser)JsonSerializer.Deserialize(content, typeof(SystemUser), _jsonSerializerOptions);
            }

            return await Task.FromResult(systemUser);
        }

        /// <inheritdoc/>
        public async Task<List<DefaultRight>> GetDefaultRightsForRegisteredSystem(string productId, CancellationToken cancellationToken = default)
        {
            List<DefaultRight> defaultRights = new();

            string systemUserPath = GetDefaultRightsForRegisteredSystemPath(productId);
            if (File.Exists(systemUserPath))
            {
                string content = File.ReadAllText(systemUserPath);
                defaultRights = (List<DefaultRight>)JsonSerializer.Deserialize(content, typeof(List<DefaultRight>), _jsonSerializerOptions);
            }

            return await Task.FromResult(defaultRights);
        }

        private static string GetSystemUserPath(string systemUserId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AuthenticationMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "SystemUser", $"{systemUserId}", "systemuser.json");
        }

        private static string GetDefaultRightsForRegisteredSystemPath(string productId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AuthenticationMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "System", $"{productId}", "defaultrights.json");
        }
    }
}
