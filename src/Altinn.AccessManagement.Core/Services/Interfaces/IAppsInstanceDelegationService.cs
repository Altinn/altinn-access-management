using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public interface IAppsInstanceDelegationService
{
    /// <summary>
    /// Delegate access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">App instance delegation request model</param>
    /// <returns>Boolean whether the app instance delegation was successful</returns>
    public Task<Result<bool>> Delegate();

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">App instance delegation request model</param>
    /// <returns>Boolean whether the app instance delegation was revoked</returns>
    public Task<Result<bool>> Revoke();

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="appInstanceDelegationRequest">App instance delegation request model</param>
    /// <returns>Boolean whether the app instance delegation was found</returns>
    public Task<Result<bool>> Get();
}
