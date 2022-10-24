using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AuthorizationAdmin.Core;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Core.Services.Interfaces;
using Altinn.AuthorizationAdmin.Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AuthorizationAdmin.Integration.Clients
{
    /// <summary>
    /// Client implementation for integration with the Resource Registry
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceRegistryClient : IResourceRegistryClient
    {
        private readonly HttpClient _httpClient = new();
        private readonly ILogger<IDelegationsService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="settings">The resource registry config settings</param>
        public ResourceRegistryClient(IOptions<ResourceRegistrySettings> settings, ILogger<IDelegationsService> logger)
        {
            ResourceRegistrySettings resourceRegistrySettings = settings.Value;
            _httpClient.BaseAddress = new Uri(resourceRegistrySettings.BaseApiUrl);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId)
        {
            ServiceResource? result = null;
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

        /// <summary>
        /// Get resource information for the the given list of resourceids
        /// </summary>
        /// <param name="resourceIds"> the list of resource ids</param>
        /// <returns></returns>
        public async Task<List<ServiceResource>> GetResources(List<string> resourceIds)
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            foreach (string id in resourceIds)
            {
                ServiceResource resource = null;

                try
                {
                    resource = await GetResource(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AccessManagement // ResourceRegistryClient // GetResources // Exception");
                    throw;
                }

                if (resource == null)
                {
                    ServiceResource unavailableResource = new ServiceResource
                    {
                        Identifier = id,
                        Title = new Dictionary<string, string>
                        {
                            { "en", "Not Available" },
                            { "nb-no", "ikke tilgjengelig" },
                            { "nn-no", "ikkje tilgjengelig" }
                        }
                    };
                    resources.Add(unavailableResource);
                }
                else
                {
                    resources.Add(resource);
                }
            }

            return resources;
        }
    }
}
