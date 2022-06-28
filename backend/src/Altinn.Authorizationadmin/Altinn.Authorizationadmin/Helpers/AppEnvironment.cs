namespace Altinn.AuthorizationAdmin.Helpers
{
    public static class AppEnvironment
    {
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