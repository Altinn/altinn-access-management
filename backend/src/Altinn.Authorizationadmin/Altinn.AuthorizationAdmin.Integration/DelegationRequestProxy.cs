using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Altinn.AuthorizationAdmin.Services
{
    public class DelegationRequestProxy : IDelegationRequestsWrapper
    {
        private readonly PlatformSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsWrapper"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="generalSettings">the general settings</param>
        /// <param name="logger">the logger</param>
        public DelegationRequestProxy(HttpClient httpClient, IOptions<PlatformSettings> generalSettings, ILogger<DelegationRequestProxy> logger)
        {
            _generalSettings = generalSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        public async Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction)
        {
            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}api/DelegationRequests");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await JsonSerializer.DeserializeAsync<DelegationRequests>(await response.Content.ReadAsStreamAsync(), new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                _logger.LogError("Getting delegationg requsts from bridge failed with {StatusCode}", response.StatusCode);
            }

            return null;
        }
    }
}
