using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Controller responsible for all instance delegation operations from Apps
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/apps/instancedelegation")]
public class AppsInstanceDelegationController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IAppsInstanceDelegationService _appInstanceDelegationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationController"/> class.
    /// </summary>
    /// <param name="logger">logger service</param>
    /// <param name="mapper">mapper service</param>
    /// <param name="appInstanceDelegationService">app instance delegation handler</param>
    public AppsInstanceDelegationController(
        ILogger<ResourceOwnerAuthorizedPartiesController> logger,
        IMapper mapper,
        IAppsInstanceDelegationService appInstanceDelegationService)
    {
        _logger = logger;
        _mapper = mapper;
        _appInstanceDelegationService = appInstanceDelegationService;
    }

    /// <summary>
    /// Delegates access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">The request model</param>
    /// <returns>Result</returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delegation(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<bool> result = await _appInstanceDelegationService.Delegate(appInstanceDelegationRequest);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="appInstanceDelegationRequest">The request model</param>
    /// <returns>Result</returns>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<bool> result = await _appInstanceDelegationService.Delegate(appInstanceDelegationRequest);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">The request model</param>
    /// <returns>Result</returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Route("revoke")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Revoke(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<bool> result = await _appInstanceDelegationService.Revoke(appInstanceDelegationRequest);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }
}
