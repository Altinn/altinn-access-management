using System.Net.Http;
using System.Net.Http.Headers;
using Altinn.AccessManagement.Core.Clients;
using Altinn.AccessManagement.Integration.Configuration;
using AltinnCore.Authentication.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for authentication actions in Altinn Platform.
    /// </summary>
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly PlatformSettings _platformSettings;

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
        public async Task<string> RefreshToken()
        {
            try
            {
                string endpointUrl = $"refresh";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.GetAsync(endpointUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string refreshedToken = await response.Content.ReadAsStringAsync();
                    refreshedToken = refreshedToken.Replace('"', ' ').Trim();
                    return refreshedToken;
                }
                else
                {
                    _logger.LogError($"Refreshing JwtToken failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AuthenticationClient // Refresh // Exception");
                throw;
            }

            return null;
        }
    }
}
