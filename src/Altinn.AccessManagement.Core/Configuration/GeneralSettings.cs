namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the SBL base adress
        /// </summary>
        public string SBLBaseAdress { get; set; }

        /// <summary>
        /// Gets or sets the cache timeout
        /// </summary>
        public int PolicyCacheTimeout { get; set;  }
        
        /// <summary>
        /// Name of the cookie for runtime
        /// </summary>
        public string RuntimeCookieName { get; set; }

        /// <summary>
        /// Open Id Connect Well known endpoint. Related to JSON WEB token validation.
        /// </summary>
        public string OpenIdWellKnownEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string AccessManagementApplicationHostName { get; set; }
    }
}
