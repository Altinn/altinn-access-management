﻿using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="settings">The resource registry config settings</param>
        /// <param name="logger">Logger instance for this ResourceRegistryClient</param>
        public ResourceRegistryClient(IOptions<ResourceRegistrySettings> settings, ILogger<IResourceRegistryClient> logger)
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

        /// <summary>
        /// Get resource information for the the given list of resourceids
        /// </summary>
        /// <param name="scopes"> the scope of resources</param>
        /// <returns></returns>
        public async Task<List<ServiceResource>> SearchResources(string scopes)
        {
            List<ServiceResource> resources = new List<ServiceResource>();
            List<ServiceResource> filteredResources = new List<ServiceResource>();
            ResourceSearch resourceSearch = new ResourceSearch();
            resourceSearch.ResourceType = ResourceType.MaskinportenSchema;

            try
            {
                string endpointUrl = $"resourceregistry/api/v1/resource/search?search.resourcetype={ResourceType.MaskinportenSchema}";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    string content = await response.Content.ReadAsStringAsync();
                    resources = JsonSerializer.Deserialize<List<ServiceResource>>(content, options);
                    ResourceReference resourceReference = new ResourceReference();
                    resourceReference.ReferenceSource = ReferenceSource.Altinn3;
                    resourceReference.Reference = scopes;
                    resourceReference.ReferenceType = ReferenceType.MaskinportenScope;

                    foreach (ServiceResource resource in resources)
                    {
                        foreach (ResourceReference reference in resource.ResourceReferences)
                        {
                            if (reference != null && reference.ReferenceSource == ReferenceSource.Altinn3 && reference.Reference.Equals(scopes) && reference.ReferenceType == ReferenceType.MaskinportenScope)
                            {
                                filteredResources.Add(resource);
                            }

                            //if (resource.ResourceReferences.Contains(resourceReference))
                            //{
                            //    filteredResources.Add(resource);
                            //}
                        }
                    }

                    //filteredResources = resources.FindAll(r => r.ResourceReferences.any(resourceReference));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AccessManagement // ResourceRegistryClient // SearchResources // Exception");
                throw;
            }

            return filteredResources;
        }
    }
}
