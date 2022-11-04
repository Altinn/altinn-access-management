namespace Altinn.AccessManagement.Core.Helpers.Extensions
{
    /// <summary>
    /// Generic Extensions
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Creates a new List of objects type, containing just the single object
        /// </summary>
        /// <param name="object">The object to create a list of</param>
        /// <returns>A list containing the object</returns>
        public static List<T> SingleToList<T>(this T @object)
        {
            return new List<T> { @object };
        }
    }
}
