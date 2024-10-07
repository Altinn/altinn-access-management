using System.Text.RegularExpressions;

namespace Altinn.AccessManagement.Core.Helpers
{
    /// <summary>
    /// ServiceResource helper methods
    /// </summary>
    public static partial class ServiceResourceHelper
    {
        /// <summary>
        /// Resource identifier regex.
        /// </summary>
        [GeneratedRegex("^[a-z0-9_-]{4,}$")]
        internal static partial Regex ResourceIdentifierRegex();
    }
}
