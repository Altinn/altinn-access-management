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
        /// Gets or sets the host name.
        /// </summary>
        public string AccessManagementApplicationHostName { get; set; }
    }
}
