namespace Altinn.AccessManagement.Integration.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class PlatformSettings
    {
        /// <summary>
        /// Gets or sets the bridge api endpoint
        /// </summary>
        public string? BridgeApiEndpoint { get; set; }

        /// <summary>
        /// Open Id Connect Well known endpoint
        /// </summary>
        public string? OpenIdWellKnownEndpoint { get; set; }

        /// <summary>
        /// Name of the cookie for where JWT is stored
        /// </summary>
        public string? JwtCookieName { get; set; }

        /// <summary>
        /// Gets or sets the profile api endpoint.
        /// </summary>
        public string? ProfileApiEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the subscriptionkey.
        /// </summary>
        public string? SubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets the SubscriptionKeyHeaderName
        /// </summary>
        public string? SubscriptionKeyHeaderName { get; set; }
    }
}
