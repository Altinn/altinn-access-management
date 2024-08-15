using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Services.Interfaces;

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
    public Task<Result<bool>> Delegate(AppsInstanceDelegationRequest appInstanceDelegationRequest);

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">App instance delegation request model</param>
    /// <returns>Boolean whether the app instance delegation was revoked</returns>
    public Task<Result<bool>> Revoke(AppsInstanceDelegationRequest appInstanceDelegationRequest);

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="appInstanceDelegationRequest">App instance delegation request model</param>
    /// <returns>Boolean whether the app instance delegation was found</returns>
    public Task<Result<bool>> Get(AppsInstanceDelegationRequest appInstanceDelegationRequest);
}
