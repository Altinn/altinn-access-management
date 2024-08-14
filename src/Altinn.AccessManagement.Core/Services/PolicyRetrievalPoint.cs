using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ABAC.Xacml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// The Policy Retrieval point responsible to find the correct policy
    /// based on the context Request
    /// </summary>
    public class PolicyRetrievalPoint : IPolicyRetrievalPoint
    {
        private readonly IPolicyFactory _policyFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRetrievalPoint"/> class.
        /// </summary>
        /// <param name="policyFactory">The policy factory</param>
        /// <param name="memoryCache">The cache handler</param>
        /// <param name="cacheConfig">The cache config settings</param>
        public PolicyRetrievalPoint(IPolicyFactory policyFactory, IMemoryCache memoryCache, IOptions<CacheConfig> cacheConfig)
        {
            _policyFactory = policyFactory;
            _memoryCache = memoryCache;
            _cacheConfig = cacheConfig.Value;
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(XacmlContextRequest request, CancellationToken cancellationToken = default)
        {
            string policyPath = PolicyHelper.GetPolicyPath(request);
            return await GetPolicyInternalAsync(policyPath, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(string org, string app, CancellationToken cancellationToken = default)
        {
            string policyPath = PolicyHelper.GetAltinnAppsPolicyPath(org, app);
            return await GetPolicyInternalAsync(policyPath, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(string resourceRegistryId, CancellationToken cancellationToken = default)
        {
            string policyPath = PolicyHelper.GetResourceRegistryPolicyPath(resourceRegistryId);
            return await GetPolicyInternalAsync(policyPath, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyVersionAsync(string policyPath, string version, CancellationToken cancellationToken = default)
        {
            return await GetPolicyInternalAsync(policyPath, version, cancellationToken);
        }

        private async Task<XacmlPolicy> GetPolicyInternalAsync(string policyPath, string version = "", CancellationToken cancellationToken = default)
        {
            if (!_memoryCache.TryGetValue(policyPath + version, out XacmlPolicy policy))
            {
                Stream policyBlob = string.IsNullOrEmpty(version) ?
                    await _policyFactory.Create(policyPath).GetPolicyAsync(cancellationToken) :
                    await _policyFactory.Create(policyPath).GetPolicyVersionAsync(version, cancellationToken);
                using (policyBlob)
                {
                    policy = (policyBlob.Length > 0) ? PolicyHelper.ParsePolicy(policyBlob) : null;
                }

                PutXacmlPolicyInCache(policyPath, policy);
            }

            return policy;
        }

        private void PutXacmlPolicyInCache(string policyPath, XacmlPolicy policy)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _cacheConfig.PolicyCacheTimeout, 0));

            _memoryCache.Set(policyPath, policy, cacheEntryOptions);
        }
    }
}