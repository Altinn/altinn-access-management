using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services.Implementation;

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
    public Task<Result<bool>> Delegate()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Get()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Revoke()
    {
        throw new NotImplementedException();
    }
}
