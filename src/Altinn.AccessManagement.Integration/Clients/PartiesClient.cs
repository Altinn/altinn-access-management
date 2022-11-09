using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// Proxy implementation for parties
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartiesClient : IPartiesClient
    {
        private readonly SblBridgeSettings _sblBridgeSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartiesClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        /// <param name="logger">the logger</param>
        public PartiesClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<PartiesClient> logger)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _logger = logger;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<Party> GetPartyAsync(int partyId)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}register/api/parties/{partyId}");
                
                HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Party partyInfo = JsonSerializer.Deserialize<Party>(responseContent);
                    return partyInfo;
                }
                else
                {
                    _logger.LogError("Getting party information from bridge failed with {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartyAsync // Exception");
                throw;
            }

            return null;
        }

        /// <inheritdoc/>
        public int GetPartyId(int id)
        {
            int partyId = 0;
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}register/api/parties/lookup");
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(id), Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync(uriBuilder.Uri, requestBody).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    partyId = JsonSerializer.Deserialize<int>(responseContent);
                    return partyId;
                }
                else
                {
                    _logger.LogError("Getting party information from bridge failed with {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartyAsync // Exception");
                throw;
            }

            return partyId;
        }

        /// <inheritdoc/>
        public async Task<List<Party>> GetPartiesAsync(List<int> parties)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}register/api/parties");

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

        /// <inheritdoc/>
        public async Task<List<int>> GetKeyRoleParties(int userId)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/partieswithkeyroleaccess?userid={userId}");
                HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<List<int>>(responseBody);
                }

                _logger.LogError("AccessManagement // PartiesClient // GetKeyRoleParties // Failed // Unexpected HttpStatusCode: {StatusCode}\n {responseBody}", response.StatusCode, responseBody);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // GetKeyRoleParties // Failed // Unexpected Exception");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/partyparents");
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(subunitPartyIds), Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uriBuilder.Uri,
                    Content = requestBody
                };

                HttpResponseMessage response = await _client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<List<MainUnit>>(responseBody);
                }

                _logger.LogError("AccessManagement // PartiesClient // GetMainUnits // Failed // Unexpected HttpStatusCode: {StatusCode}\n {responseBody}", response.StatusCode, responseBody);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // partyparents // Failed // Unexpected Exception");
                throw;
            }
        }
    }
}
