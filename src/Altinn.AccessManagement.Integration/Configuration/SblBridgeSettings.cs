namespace Altinn.AccessManagement.Integration.Configuration
{
    /// <summary>
    /// Configuration settings for integration with the SBL Bridge
    /// </summary>
    public class SblBridgeSettings
    {
        /// <summary>
        /// Gets or sets the base api url for the SBL Bridge
        /// </summary>
        public string BaseApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout
        /// </summary>
        public int RoleCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of mainunits
        /// </summary>
        public int MainUnitCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout for lookup of keyrole partyIds
        /// </summary>
        public int KeyrolePartyIdsCacheTimeout { get; set; }
    }
}
