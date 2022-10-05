using System.Text;
using System.Text.Json;
using System.Web;
using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Services;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin.Services
{
    /// <summary>
    /// Proxy implementation for parties
    /// </summary>
    public class PartiesProxy : IPartiesWrapper
    {
        private readonly PlatformSettings _platformSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartiesProxy"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="platformSettings">the general settings</param>
        /// <param name="logger">the logger</param>
        public PartiesProxy(HttpClient httpClient, IOptions<PlatformSettings> platformSettings, ILogger<PartiesProxy> logger)
        {
            _platformSettings = platformSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<List<Party>> GetPartiesAsync(List<int> parties)
        {
            UriBuilder uriBuilder = new UriBuilder($"{_platformSettings.BridgeApiEndpoint}register/api/parties");

            StringContent requestBody = new StringContent(JsonSerializer.Serialize(parties), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(uriBuilder.Uri, requestBody);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await JsonSerializer.DeserializeAsync<List<Party>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                _logger.LogError("Getting delegationg requsts from bridge failed with {StatusCode}", response.StatusCode);
            }

            return null;
        }
    }
}
