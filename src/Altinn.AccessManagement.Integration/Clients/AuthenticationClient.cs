using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Register.Models;
using AltinnCore.Authentication.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql.Internal;
using DefaultRight = Altinn.AccessManagement.Core.Models.Authentication.DefaultRight;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for authentication actions in Altinn Platform.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly PlatformSettings _platformSettings;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationClient"/> class
        /// </summary>
        /// <param name="platformSettings">The current platform settings.</param>
        /// <param name="logger">the logger</param>
        /// <param name="httpContextAccessor">The http context accessor </param>
        /// <param name="httpClient">A HttpClient provided by the HttpClientFactory.</param>
        public AuthenticationClient(
            IOptions<PlatformSettings> platformSettings,
            ILogger<AuthenticationClient> logger,
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _platformSettings = platformSettings.Value;
            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthenticationEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
        }

        /// <inheritdoc />
        public async Task<string> RefreshToken(CancellationToken cancellationToken = default)
        {
            try
            {
                string endpointUrl = $"refresh";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.GetAsync(endpointUrl, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string refreshedToken = await response.Content.ReadAsStringAsync(cancellationToken);
                    refreshedToken = refreshedToken.Replace('"', ' ').Trim();
                    return refreshedToken;
                }
                else
                {
                    _logger.LogError("Refreshing JwtToken failed with status code");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AuthenticationClient // Refresh // Exception");
                throw;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<SystemUser> GetSystemUser(int partyId, string systemUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                string endpointUrl = $"systemuser/{partyId}/{systemUserId}";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _client.GetAsync(endpointUrl, cancellationToken);

                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SystemUser systemUser = JsonSerializer.Deserialize<SystemUser>(responseContent, _serializerOptions);
                    
                    // The endpoint is not using the partyId input so added a check here to ensure the partyId is correct.
                    if (int.TryParse(systemUser?.PartyId, out int ownerParsed) && ownerParsed != partyId)
                    {
                        return null;
                    }
                    else
                    {
                        return systemUser;
                    }
                }
                else
                {
                    HttpStatusCode statusCode = response.StatusCode;
                    _logger.LogError("Fetching system user failed with status code: {statusCode}, details: {responseContent}", statusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AuthenticationClient // GetSystemUser // Exception");
                throw;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<List<DefaultRight>> GetDefaultRightsForRegisteredSystem(string systemId, CancellationToken cancellationToken = default)
        {
            List<DefaultRight> result = new List<DefaultRight>();

            try
            {
                string endpointUrl = $"systemregister/{systemId}/rights";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.GetAsync(endpointUrl, cancellationToken);

                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonSerializer.Deserialize<List<DefaultRight>>(responseContent, _serializerOptions);
                    return result;
                }
                else
                {
                    HttpStatusCode statusCode = response.StatusCode;
                    _logger.LogError("Fetching system user default rights failed with status code: {statusCode}, details: {responseContent}", statusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AuthenticationClient // GetSystemUser // Exception");
                throw;
            }

            return result;
        }
    }
}
