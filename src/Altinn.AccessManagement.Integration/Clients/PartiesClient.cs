using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Register.Models;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlatformSettings _platformSettings;
        private readonly IAccessTokenGenerator _accessTokenGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartiesClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="sblBridgeSettings">the sbl bridge settings</param>
        /// <param name="logger">the logger</param>
        /// <param name="httpContextAccessor">handler for http context</param>
        /// <param name="platformSettings">the platform setttings</param>
        /// <param name="accessTokenGenerator">An instance of the AccessTokenGenerator service.</param>
        public PartiesClient(
            HttpClient httpClient, 
            IOptions<SblBridgeSettings> sblBridgeSettings, 
            ILogger<PartiesClient> logger, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<PlatformSettings> platformSettings,
            IAccessTokenGenerator accessTokenGenerator)
        {
            _sblBridgeSettings = sblBridgeSettings.Value;
            _logger = logger;
            httpClient.BaseAddress = new Uri(platformSettings.Value.RegisterApiEndpoint);
            httpClient.DefaultRequestHeaders.Add(platformSettings.Value.SubscriptionKeyHeaderName, platformSettings.Value.SubscriptionKey);
            _client = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _platformSettings = platformSettings.Value;
            _accessTokenGenerator = accessTokenGenerator;
        }

        /// <inheritdoc/>
        public async Task<Party> GetPartyAsync(int partyId)
        {
            try
            {
                string endpointUrl = $"parties/{partyId}";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");

                HttpResponseMessage response = await _client.GetAsync(token, endpointUrl, accessToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    options.Converters.Add(new JsonStringEnumConverter());
                    Party partyInfo = JsonSerializer.Deserialize<Party>(responseContent, options);
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
        public async Task<int> GetPartyId(string id)
        {
            int partyId = 0;
            try
            {
                string endpointUrl = $"register/api/parties/lookup";
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(id), Encoding.UTF8, "application/json");
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
                HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken);

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
            List<Party> filteredList = new List<Party>();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());

            try
            {
                string endpointUrl = $"parties/partylist/";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(parties), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<Party> partiesInfo = JsonSerializer.Deserialize<List<Party>>(responseContent, options);
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

            return filteredList;
        }

        /// <inheritdoc/>
        public async Task<List<Party>> GetPartiesForUserAsync(int userId)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            try
            {
                string endpointUrl = $"{_platformSettings.ApiAuthorizationEndpoint}parties?userId={userId}";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");

                HttpResponseMessage response = await _client.GetAsync(token, endpointUrl, accessToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    List<Party> partiesInfo = JsonSerializer.Deserialize<List<Party>>(responseContent, options);
                    return partiesInfo;
                }
                else
                {
                    _logger.LogError("Getting parties information from authorization failed with {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartiesForUserAsync // Exception");
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

        /// <inheritdoc/>
        public async Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup)
        {
            Party party = null;
            try
            {
                string endpointUrl = $"parties/lookup";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(partyLookup), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    party = JsonSerializer.Deserialize<Party>(responseContent);
                    return party;
                }
                else
                {
                    _logger.LogError("Getting party information from register failed with {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // PartiesClient // LookupPartyBySSNOrOrgNo // Exception");
                throw;
            }

            return party;
        }
    }
}
