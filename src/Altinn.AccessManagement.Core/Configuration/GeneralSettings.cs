namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Option to disable csrf check
        /// </summary>
        public bool DisableCsrfCheck { get; set; }

        /// <summary>
        /// Gets or sets the cdn url for frontend
        /// </summary>
        public string FrontendBaseUrl { get; set; }
    }
}
