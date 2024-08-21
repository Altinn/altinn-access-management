using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.Logging;

namespace Altinn.Platform.Authorization.Services.Implementation;

/// <summary>
/// Contains all actions related to app instance delegation from Apps
/// </summary>
public class AppsInstanceDelegationService : IAppsInstanceDelegationService
{
    private readonly ILogger<AppsInstanceDelegationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationService"/> class.
    /// </summary>
    public AppsInstanceDelegationService(ILogger<AppsInstanceDelegationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Delegate(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Get(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Revoke(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        throw new NotImplementedException();
    }
}
