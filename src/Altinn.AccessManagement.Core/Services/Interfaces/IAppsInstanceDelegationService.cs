using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public interface IAppsInstanceDelegationService
{
    /// <summary>
    /// Gets all rights available for delegation by an app for a given app instance
    /// </summary>
    /// <param name="request">App instance delegation check request model</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was successful</returns>
    public Task<Result<ResourceDelegationCheckResponse>> DelegationCheck(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delegate access to an app instance
    /// </summary>
    /// <param name="request">App instance delegation request model</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was successful</returns>
    public Task<Result<AppsInstanceDelegationResponse>> Delegate(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="request">the request data collected in a dto</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was revoked</returns>
    public Task<Result<AppsInstanceRevokeResponse>> Revoke(AppsInstanceDelegationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="request">the request data collected in a dto</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean whether the app instance delegation was found</returns>
    public Task<Result<List<AppsInstanceDelegationResponse>>> Get(AppsInstanceGetRequest request, CancellationToken cancellationToken = default);
}
