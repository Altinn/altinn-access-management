namespace Altinn.AccessManagement.Core.Enums;

/// <summary>
/// Enum for different types of rights queries in Altinn Authorization
/// </summary>
public enum RightsQueryType
{
    /// <summary>
    /// Default
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// Rights query where the recipient is a user
    /// </summary>
    User = 1,

    /// <summary>
    /// Rights query where the recipient is an Altinn app
    /// </summary>
    AltinnApp = 2
}
