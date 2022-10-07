using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AuthorizationAdmin.Core;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin.Integration.Clients
{
    /// <summary>
    /// Client implementation for integration with the Resource Registry
    /// </summary>
    public class ResourceRegistryClient : IResourceRegistryClient
    {
        private readonly HttpClient _httpClient = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="settings">The resource registry config settings</param>
        public ResourceRegistryClient(IOptions<ResourceRegistrySettings> settings)
        {
            ResourceRegistrySettings resourceRegistrySettings = settings.Value;
            _httpClient.BaseAddress = new Uri(resourceRegistrySettings.BaseApiUrl);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId)
        {
            ServiceResource result = null;
            string endpointUrl = $"resourceregistry/api/v1/resource/{resourceId}";

            HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                string content = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<ServiceResource>(content, options);
            }

            return await Task.FromResult(result);
        }
    }
}
