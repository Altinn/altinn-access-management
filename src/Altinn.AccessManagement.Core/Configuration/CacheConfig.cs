namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// Cache configuration settings
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// Gets or sets the policy cache timeout (in minutes) 
        /// </summary>
        public int PolicyCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the Altinn role cache timeout (in minutes) 
        /// </summary>
        public int AltinnRoleCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of party information
        /// </summary>
        public int PartyCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of mainunits
        /// </summary>
        public int MainUnitCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of keyrole partyIds
        /// </summary>
        public int KeyRolePartyIdsCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of a resource from the resource registry
        /// </summary>
        public int ResourceRegistryResourceCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of a subject resources from the resourceregistry
        /// </summary>
        public int ResourceRegistrySubjectResourcesCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout (in minutes) for lookup of a rights
        /// </summary>
        public int RightsCacheTimeout { get; set; }        
    }
}
