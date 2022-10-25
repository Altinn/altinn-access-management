namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// AppEnvironment helper
    /// </summary>
    public static class AppEnvironment
    {
        /// <summary>
        /// Gets an environment variable by its key if it exists otherwise fallback value is returned
        /// </summary>
        /// <param name="key">The key of the environment variable to retrieve</param>
        /// <param name="fallback">Fallback value if the environment variable cannot be found</param>
        /// <returns>Environment variable or fallback</returns>
        public static string GetVariable(string key, string fallback = "")
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (value is string && value.Length > 0)
            {
                return value;
            }

            return fallback;
        }
    }
}
