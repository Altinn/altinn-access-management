using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Context Retrieval Service
    /// </summary>
    public class ContextRetrievalService : IContextRetrievalService
    {
        private readonly ILogger _logger;
        private readonly CacheConfig _cacheConfig;
        private readonly IMemoryCache _memoryCache;
        private readonly IResourceRegistryClient _resourceRegistryClient;
        private readonly IAltinnRolesClient _altinnRolesClient;
        private readonly IPartiesClient _partiesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextRetrievalService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="cacheConfig">Cache config</param>
        /// <param name="memoryCache">The cache handler </param>
        /// <param name="resourceRegistryClient">The client for integration with the ResourceRegistry</param>
        /// <param name="altinnRolesClient">The client for integration with the SBL Bridge for role information</param>
        /// <param name="partiesClient">The client for integration </param>
        public ContextRetrievalService(ILogger<IContextRetrievalService> logger, IOptions<CacheConfig> cacheConfig, IMemoryCache memoryCache, IResourceRegistryClient resourceRegistryClient, IAltinnRolesClient altinnRolesClient, IPartiesClient partiesClient)
        {
            _logger = logger;
            _cacheConfig = cacheConfig.Value;
            _memoryCache = memoryCache;
            _resourceRegistryClient = resourceRegistryClient;
            _altinnRolesClient = altinnRolesClient;
            _partiesClient = partiesClient;
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceRegistryId)
        {
            string cacheKey = $"rrId:{resourceRegistryId}";

            if (!_memoryCache.TryGetValue(cacheKey, out ServiceResource resource))
            {
                resource = GetResources().Result.Find(r => r.Identifier == resourceRegistryId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resource, cacheEntryOptions);
            }

            return resource;
        }

        /// <inheritdoc/>
        public async Task<List<ServiceResource>> GetResources()
        {
            string cacheKey = $"resources:all";

            if (!_memoryCache.TryGetValue(cacheKey, out List<ServiceResource> resources))
            {
                resources = await _resourceRegistryClient.GetResources();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resources, cacheEntryOptions);
            }

            return resources;
        }
    }
}
