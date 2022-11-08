﻿using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Interfaces;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Profile.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for retrieving profiles from Altinn Platform.
    /// </summary>
    public class ProfileClient : IProfileClient
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GeneralSettings _settings;
        private readonly HttpClient _client;
        private readonly IAppResources _appResources;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IKeyVaultService _keyVaultService;
        private readonly KeyVaultSettings _keyVaultSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileClient"/> class
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <param name="platformSettings">the platform settings</param>
        /// <param name="httpContextAccessor">The http context accessor </param>
        /// <param name="settings">The application settings.</param>
        /// <param name="httpClient">A HttpClient provided by the HttpClientFactory.</param>
        /// <param name="appResources">An instance of the AppResources service.</param>
        /// <param name="accessTokenGenerator">An instance of the AccessTokenGenerator service.</param>
        public ProfileClient(
            IOptions<PlatformSettings> platformSettings,
            IOptions<KeyVaultSettings> keyVaultSettings,
            ILogger<ProfileClient> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<GeneralSettings> settings,
            HttpClient httpClient,
            IAppResources appResources,
            IAccessTokenGenerator accessTokenGenerator,
            IKeyVaultService keyVaultService)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _settings = settings.CurrentValue;
            httpClient.BaseAddress = new Uri(platformSettings.Value.ProfileApiEndpoint);
            httpClient.DefaultRequestHeaders.Add(General.SubscriptionKeyHeaderName, platformSettings.Value.SubscriptionKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _appResources = appResources;
            _accessTokenGenerator = accessTokenGenerator;
            _keyVaultService = keyVaultService;
            _keyVaultSettings = keyVaultSettings.Value;
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetUserProfile(int userId)
        {
            UserProfile userProfile = null;

            string endpointUrl = $"profile/{userId}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _settings.RuntimeCookieName);

            string certBase64 =
            await _keyVaultService.GetCertificateAsync(
                _keyVaultSettings.KeyVaultURI,
                _keyVaultSettings.PlatformCertSecretId);
            string accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management", new X509Certificate2(
            Convert.FromBase64String(certBase64),
            (string)null,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable));

            HttpResponseMessage response = await _client.GetAsync(token, endpointUrl, accessToken);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                userProfile = await response.Content.ReadAsAsync<UserProfile>();
            }
            else
            {
                _logger.LogError($"Getting user profile with userId {userId} failed with statuscode {response.StatusCode}");
            }

            return userProfile;
        }
    }
}
