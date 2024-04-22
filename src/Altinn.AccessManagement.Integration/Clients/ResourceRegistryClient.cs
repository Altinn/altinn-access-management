using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Telemetry;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// Client implementation for integration with the Resource Registry
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceRegistryClient : IResourceRegistryClient
    {
        private readonly HttpClient _httpClient = new();
        private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="settings">The resource registry config settings</param>
        public ResourceRegistryClient(IOptions<PlatformSettings> settings)
        {
            PlatformSettings platformSettings = settings.Value;
            _httpClient.BaseAddress = new Uri(platformSettings.ApiResourceRegistryEndpoint);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                string endpointUrl = $"resource/{resourceId}";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<ServiceResource>(content, options);
                }

                activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response));
                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                List<ServiceResource> resources = new();
                string endpointUrl = $"resource/search";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                }

                activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response));
                return resources;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity();
            try
            {
                List<ServiceResource> resources = new();
                string endpointUrl = $"resource/resourcelist";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                }

                activity?.StopWithError(TelemetryEvents.UnexpectedHttpStatusCode(response));
                return resources;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
                throw;
            }
        }
    }
}
