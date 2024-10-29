using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Utilities;

/// <summary>
/// Hash utility class
/// </summary>
public static class HashUtil
{
    /// <summary>
    /// Get a hash code for a list of objects that is order independent
    /// </summary>
    /// <param name="source">The list of objects</param>
    /// <returns>Hash code for the list</returns>
    public static int GetOrderIndependentHashCode<T>(IEnumerable<T> source)
    {
        int hash = 0;
        foreach (T element in source)
        {
            hash = unchecked(hash + EqualityComparer<T>.Default.GetHashCode(element));
        }

        return hash;
    }
}
