namespace Altinn.AuthorizationAdmin.Integration.Configuration
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
    }
}
