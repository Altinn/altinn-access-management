using Altinn.AccessManagement.Core.Models;
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
    /// <param name="appsInstanceDelegationRequest">App instance delegation request model</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was successful</returns>
    public Task<Result<AppsInstanceDelegationResponse>> Delegate(AppsInstanceDelegationRequest appsInstanceDelegationRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was revoked</returns>
    public Task<Result<AppsInstanceDelegationResponse>> Revoke(AppsInstanceDelegationRequest appsInstanceDelegationRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="resourceId">The resource to fetch instance delegations for</param>
    /// <param name="instanceId">The specific instance the delegation is for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was found</returns>
    public Task<Result<List<AppsInstanceDelegationResponse>>> Get(string resourceId, string instanceId, CancellationToken cancellationToken = default);
}
