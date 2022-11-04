namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// Cache configuration settings
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// Gets or sets the policy cache timeout
        /// </summary>
        public int PolicyCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the Altinn role cache timeout
        /// </summary>
        public int AltinnRoleCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of party information
        /// </summary>
        public int PartyCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of mainunits
        /// </summary>
        public int MainUnitCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of keyrole partyIds
        /// </summary>
        public int KeyRolePartyIdsCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of a resource from the resource registry
        /// </summary>
        public int ResourceRegistryResourceCacheTimeout { get; set; }
    }
}
