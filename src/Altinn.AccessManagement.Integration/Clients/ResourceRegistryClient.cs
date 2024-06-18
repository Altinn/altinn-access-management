using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Integration.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<IResourceRegistryClient> _logger;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="settings">The resource registry config settings</param>
        /// <param name="logger">Logger instance for this ResourceRegistryClient</param>
        public ResourceRegistryClient(IOptions<PlatformSettings> settings, ILogger<IResourceRegistryClient> logger)
        {
            PlatformSettings platformSettings = settings.Value;
            _httpClient.BaseAddress = new Uri(platformSettings.ApiResourceRegistryEndpoint);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceId, CancellationToken cancellationToken = default)
        {
            string endpointUrl = $"resource/{resourceId}";

            HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ServiceResource>(content, options);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResources(CancellationToken cancellationToken = default)
        {
            List<ServiceResource> resources = new();

            try
            {
                string endpointUrl = $"resource/search";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // ResourceRegistryClient // SearchResources // Exception");
                throw;
            }

            return resources;
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResourceList(CancellationToken cancellationToken = default)
        {
            List<ServiceResource> resources = new();

            try
            {
                string endpointUrl = $"resource/resourcelist";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // ResourceRegistryClient // GetResourceList // Exception");
                throw;
            }

            return resources;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, IEnumerable<BaseAttribute>>> GetSubjectResources(IEnumerable<string> subjects, CancellationToken cancellationToken = default)
        {
            string endpointUrl = $"resource/bysubjects";
            Dictionary<string, IEnumerable<BaseAttribute>> subjectResources = new Dictionary<string, IEnumerable<BaseAttribute>>();
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(subjects), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpointUrl, requestBody, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                PaginatedResult<SubjectResources> result = JsonSerializer.Deserialize<PaginatedResult<SubjectResources>>(content, options);

                if (result != null && result.Items != null)
                {
                    foreach (SubjectResources resultItem in result.Items)
                    {
                        subjectResources.Add(resultItem.Subject.Urn, resultItem.Resources);
                    }
                }
            }

            return subjectResources;
        }
    }
}
