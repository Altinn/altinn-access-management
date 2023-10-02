namespace Altinn.AccessManagement.Core.Enums
{
    /// <summary>
    /// Enum representation of the different types of resource attribute match types supported
    /// </summary>
    public enum ResourceAttributeMatchType
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Resource registered in the Altinn Resource Registry
        /// </summary>
        ResourceRegistry = 1, 
        
        /// <summary>
        /// Legacy App resource identified by org owner and app name
        /// </summary>
        AltinnAppId = 2,

        /// <summary>
        /// Legacy Altinn 2 service resource identified by service codes
        /// </summary>
        Altinn2Service = 3
    }
}
