namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// UserProfile Lookup Settings
    /// </summary>
    public class UserProfileLookupSettings
    {
        /// <summary>
        /// Gets or sets the cache timeout for number of failed lookup attempts (in seconds) 
        /// </summary>
        public int FailedAttemptsCacheLifetimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of failed attempts before lockout 
        /// </summary>
        public int MaximumFailedAttempts { get; set; }
    }
}
