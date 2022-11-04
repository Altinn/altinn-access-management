using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Register.Models;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Platform.Authorization.Services.Implementation
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
        public async Task<List<Role>> GetDecisionPointRolesForUser(int coveredByUserId, int offeredByPartyId)
        {
            string cacheKey = $"Roles_u:{coveredByUserId}_p:{offeredByPartyId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Role> roles))
            {
                roles = await _altinnRolesClient.GetDecisionPointRolesForUser(coveredByUserId, offeredByPartyId) ?? new List<Role>();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.AltinnRoleCacheTimeout, 0));

                _memoryCache.Set(cacheKey, roles, cacheEntryOptions);
            }

            return roles;
        }

        /// <inheritdoc/>
        public async Task<Party> GetPartyAsync(int partyId)
        {
            string cacheKey = $"p:{partyId}";

            if (!_memoryCache.TryGetValue(cacheKey, out Party party))
            {
                List<Party> result = await _partiesClient.GetPartiesAsync(partyId.SingleToList());

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.KeyRolePartyIdsCacheTimeout, 0));

                _memoryCache.Set(cacheKey, result.FirstOrDefault(), cacheEntryOptions);
            }

            return party;
        }

        /// <inheritdoc/>
        public async Task<List<Party>> GetPartiesAsync(List<int> partyIds)
        {
            List<Party> parties = new List<Party>();
            List<int> partyIdsNotInCache = new List<int>();

            foreach (int partyId in partyIds)
            {
                if (_memoryCache.TryGetValue($"p:{partyId}", out Party party))
                {
                    parties.Add(party);
                }
                else
                {
                    partyIdsNotInCache.Add(partyId);
                }
            }

            List<Party> remainingParties = await _partiesClient.GetPartiesAsync(partyIdsNotInCache);

            foreach (Party party in remainingParties)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PartyCacheTimeout, 0));
                _memoryCache.Set($"p:{party.PartyId}", party, cacheEntryOptions);
            }

            return parties;
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetKeyRoleParties(int userId)
        {
            string cacheKey = $"KeyRolePartyIds_u:{userId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<int> keyrolePartyIds))
            {
                keyrolePartyIds = await _partiesClient.GetKeyRoleParties(userId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.KeyRolePartyIdsCacheTimeout, 0));

                _memoryCache.Set(cacheKey, keyrolePartyIds, cacheEntryOptions);
            }

            return keyrolePartyIds;
        }

        /// <inheritdoc/>
        public async Task<List<MainUnit>> GetMainUnits(int subUnitPartyId)
        {
            string cacheKey = $"subunit:{subUnitPartyId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<MainUnit> mainUnits))
            {
                // Key not in cache, so get data.
                mainUnits = await _partiesClient.GetMainUnits(new MainUnitQuery { PartyIds = new List<int> { subUnitPartyId } });

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.MainUnitCacheTimeout, 0));

                _memoryCache.Set(cacheKey, mainUnits, cacheEntryOptions);
            }

            return mainUnits;
        }

        /// <inheritdoc/>
        public async Task<ServiceResource> GetResource(string resourceRegistryId)
        {
            string cacheKey = $"rrId:{resourceRegistryId}";

            if (!_memoryCache.TryGetValue(cacheKey, out ServiceResource resource))
            {
                resource = await _resourceRegistryClient.GetResource(resourceRegistryId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.ResourceRegistryResourceCacheTimeout, 0));

                _memoryCache.Set(cacheKey, resource, cacheEntryOptions);
            }

            return resource;
        }
    }
}
