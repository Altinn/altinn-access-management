﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Telemetry;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Platform.Profile.Models;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for retrieving profiles from Altinn Platform.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProfileClient : IProfileClient
    {
        private readonly PlatformSettings _settings;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileClient"/> class
        /// </summary>
        /// <param name="platformSettings">the platform settings</param>
        /// <param name="httpClient">A HttpClient provided by the HttpClientFactory.</param>
        public ProfileClient(IOptions<PlatformSettings> platformSettings, HttpClient httpClient)
        {
            _settings = platformSettings.Value;
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc/>
        public async Task<UserProfile> GetUser(UserProfileLookup userProfileLookup, CancellationToken cancellationToken = default)
        {
            // Review: Added try/catch
            using var activity = TelemetryConfig._activitySource.StartActivity();
            try
            {
                UriBuilder endpoint = new UriBuilder($"{_settings.ApiProfileEndpoint}internal/user/");

                StringContent requestBody = new StringContent(JsonSerializer.Serialize(userProfileLookup), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(endpoint.Uri, requestBody, cancellationToken);
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response));
                    return null;
                }
            
                UserProfile userProfile = JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptions);

                return userProfile;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }
    }
}
