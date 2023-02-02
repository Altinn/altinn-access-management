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
        /// Gets or sets the host name.
        /// </summary>
        public string AccessManagementApplicationHostName { get; set; }

        /// <summary>
        /// Option to disable csrf check
        /// </summary>
        public bool DisableCsrfCheck { get; set; }

        /// <summary>
        /// Gets or sets the RuntimeCookieName
        /// </summary>
        public string RuntimeCookieName { get; set; }

        /// <summary>
        /// Gets or sets the cdn url for frontend
        /// </summary>
        public string FrontendBaseUrl { get; set; }
    }
}
