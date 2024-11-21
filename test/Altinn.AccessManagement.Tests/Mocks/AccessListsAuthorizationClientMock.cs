using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.AccessList;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAccessListsAuthorizationClient"></see> interface
    /// </summary>
    public class AccessListsAuthorizationClientMock : IAccessListsAuthorizationClient
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        /// <inheritdoc/>
        public async Task<AccessListAuthorizationResponse> AuthorizePartyForAccessList(AccessListAuthorizationRequest request, CancellationToken cancellationToken = default)
        {
            AccessListAuthorizationResponse response = null;

            string responsePath = GetResponsePath(request);
            if (File.Exists(responsePath))
            {
                string content = File.ReadAllText(responsePath);
                response = JsonSerializer.Deserialize<AccessListAuthorizationResponse>(content, _jsonSerializerOptions);
            }

            return await Task.FromResult(response ?? new AccessListAuthorizationResponse { Result = AccessListAuthorizationResult.NotAuthorized });
        }

        private static string GetResponsePath(AccessListAuthorizationRequest request)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AccessListsAuthorizationClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Json", "AccessListResponses", $"{request.Resource.Value.ValueSpan}", $"{request.Subject.Value.ValueSpan}", $"{request.Action.Value.ValueSpan}.json");
        }
    }
}