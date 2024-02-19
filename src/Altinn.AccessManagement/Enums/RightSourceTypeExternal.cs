namespace Altinn.AccessManagement.Enums
{
    /// <summary>
    /// Enum for different the source types exist for a right in Altinn Authorization
    /// </summary>
    public enum RightSourceTypeExternal
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// XACML policy for an Altinn app
        /// </summary>
        AppPolicy = 1,

        /// <summary>
        /// XACML policy for a resource from the resource registry
        /// </summary>
        ResourceRegistryPolicy = 2,

        /// <summary>
        /// Altinn delegation policy
        /// </summary>
        DelegationPolicy = 3
    }
}
