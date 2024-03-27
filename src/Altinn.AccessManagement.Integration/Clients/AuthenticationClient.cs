using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Telemetry;
using Altinn.AccessManagement.Integration.Configuration;
using AltinnCore.Authentication.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for authentication actions in Altinn Platform.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly PlatformSettings _platformSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationClient"/> class
        /// </summary>
        /// <param name="platformSettings">The current platform settings.</param>
        /// <param name="httpContextAccessor">The http context accessor </param>
        /// <param name="httpClient">A HttpClient provided by the HttpClientFactory.</param>
        public AuthenticationClient(IOptions<PlatformSettings> platformSettings, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _platformSettings = platformSettings.Value;
            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthenticationEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
        }

        /// <inheritdoc />
        public async Task<string> RefreshToken()
        {
            using var activity = TelemetryConfig._activitySource.StartActivity();
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
                    activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response)); 
                    
                    // Review: Original: _logger.LogError("Refreshing JwtToken failed with status code"); Fix: New ActivityType for JwtToken failed
                }
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }

            return null;
        }
    }
}
