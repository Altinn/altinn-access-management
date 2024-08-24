using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.ProblemDetails;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Controller responsible for all instance delegation operations from Apps
/// </summary>
[ApiController]
[Route("accessmanagement/api")]
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
    [Route("v1/apps/instancedelegation")]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delegation(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<AppsInstanceDelegationResponse> result = new();

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
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Route("v1/apps/instancedelegation/query")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<AppsInstanceDelegationRequest> result = new();

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
    [Route("v1/apps/instancedelegation/revoke")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Revoke(AppsInstanceDelegationRequest appInstanceDelegationRequest)
    {
        Result<AppsInstanceDelegationResponse> result = new();

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delegates access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequest">The request model</param>
    /// <returns>Result</returns>
    [HttpPost]
    [Route("v2/apps/instancedelegation/{resourceId}/{resourceInstanceId}")]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseV2), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DelegationV2(AppsInstanceDelegationRequestV2 appInstanceDelegationRequest)
    {
        Result<AppsInstanceDelegationResponseV2> result = new();

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all instance delegations for a given instance
    /// </summary>
    /// <param name="resourceId">The resource identifier</param>
    /// <param name="resourceInstanceId">The resource instance identifier</param>
    /// <param name="fromPartyUuid">The party uuid of the party the delegation is made from</param>
    /// <param name="toPartyUuid">he party uuid of the party the delegation is made to</param>
    /// <param name="isParalellTaskDelegation">Whether the delegation to lookup is a paralell task delegation</param>
    /// <returns>Result</returns>
    [HttpGet]
    [Route("v2/apps/instancedelegation/{resourceId}/{resourceInstanceId}")]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AppsInstanceDelegationResponseV2>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetV2([FromRoute] string resourceId, [FromRoute] string resourceInstanceId, [FromQuery] string fromPartyUuid, [FromQuery] string toPartyUuid, [FromQuery] bool isParalellTaskDelegation = false)
    {
        Result<AppsInstanceDelegationResponseV2> result = new();

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delegates access to an app instance
    /// </summary>
    /// <param name="resourceId">The resource identifier</param>
    /// <param name="resourceInstanceId">The resource instance identifier</param>
    /// <param name="fromPartyUuid">The party uuid of the party the delegation is made from</param>
    /// <param name="toPartyUuid">he party uuid of the party the delegation is made to</param>
    /// <param name="isParalellTaskDelegation">Whether the delegation to lookup is a paralell task delegation</param>
    /// <returns>Result</returns>
    [HttpDelete]
    [Route("v2/apps/instancedelegation/{resourceId}/{resourceInstanceId}")]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseV2), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RevokeV2([FromRoute] string resourceId, [FromRoute] string resourceInstanceId, [FromQuery] string fromPartyUuid, [FromQuery] string toPartyUuid, [FromQuery] bool isParalellTaskDelegation = false)
    {
        Result<AppsInstanceDelegationResponseV2> result = new();

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }
}
