using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Seeds;

/// <summary>
/// For seeds
/// </summary>
public interface IParty
{
    /// <summary>
    /// Get Party profile from seed
    /// </summary>
    Party Party { get; }
}

/// <summary>
/// For seeds
/// </summary>
public interface IUserProfile
{
    /// <summary>
    /// Get User profile from seed
    /// </summary>
    UserProfile UserProfile { get; }
}

/// <summary>
/// For seeds
/// </summary>
public interface IAccessManagementResource
{
    /// <summary>
    /// Db resource
    /// </summary>
    AccessManagementResource DbResource { get; }

    /// <summary>
    /// Get resource from seed
    /// </summary>
    ServiceResource Resource { get; }
}