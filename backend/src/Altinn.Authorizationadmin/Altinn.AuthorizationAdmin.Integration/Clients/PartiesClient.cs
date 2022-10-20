using System.Text;
using System.Text.Json;
using System.Web;
using Altinn.AuthorizationAdmin.Core.Clients;
using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin.Integration.Clients
{
    /// <summary>
    /// Proxy implementation for parties
    /// </summary>
    public class PartiesClient : IPartiesClient
    {
        private readonly PlatformSettings _platformSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartiesClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="platformSettings">the general settings</param>
        /// <param name="logger">the logger</param>
        public PartiesClient(HttpClient httpClient, IOptions<PlatformSettings> platformSettings, ILogger<PartiesClient> logger)
        {
            _platformSettings = platformSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<List<Party>> GetPartiesAsync(List<int> parties)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_platformSettings.BridgeApiEndpoint}register/api/parties");

                StringContent requestBody = new StringContent(JsonSerializer.Serialize(parties), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(uriBuilder.Uri, requestBody);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<Party> partiesInfo = JsonSerializer.Deserialize<List<Party>>(responseContent);
                    return partiesInfo;
                }
                else
                {
                    _logger.LogError("Getting parties information from bridge failed with {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartiesAsync // Exception");
                throw;
            }

            return null;
        }
    }
}
