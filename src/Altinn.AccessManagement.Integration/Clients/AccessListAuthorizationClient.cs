using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models.AccessList;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for access list authorization actions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AccessListAuthorizationClient : IAccessListsAuthorizationClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlatformSettings _platformSettings;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessListAuthorizationClient"/> class.
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="logger">the logger</param>
        /// <param name="httpContextAccessor">handler for http context</param>
        /// <param name="platformSettings">the platform setttings</param>
        /// <param name="accessTokenGenerator">An instance of the AccessTokenGenerator service.</param>
        public AccessListAuthorizationClient(
            IOptions<PlatformSettings> platformSettings,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticationClient> logger,
            HttpClient httpClient,
            IAccessTokenGenerator accessTokenGenerator)
        {
            _logger = logger;
            _platformSettings = platformSettings.Value;
            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthorizationEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _accessTokenGenerator = accessTokenGenerator;
        }

        /// <inheritdoc/>
        public async Task<AccessListAuthorizationResponse> AuthorizePartyForAccessList(AccessListAuthorizationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                string endpointUrl = "accesslist/accessmanagement/authorization";
                string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
                var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
                StringContent requestBody = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadFromJsonAsync<AccessListAuthorizationResponse>(cancellationToken);
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("AccessManagement // AccessListAuthorizationClient // AuthorizePartyForAccessList  // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
                    return new() { Result = AccessListAuthorizationResult.NotAuthorized };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // AccessListAuthorizationClient // AuthorizePartyForAccessList // Exception");
                throw;
            }
        }
    }
}
