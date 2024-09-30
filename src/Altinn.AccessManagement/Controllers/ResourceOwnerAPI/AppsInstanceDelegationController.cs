﻿using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
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
    private readonly IMapper _mapper;
    private readonly IAppsInstanceDelegationService _appInstanceDelegationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppsInstanceDelegationController"/> class.
    /// </summary>
    /// <param name="mapper">mapper service</param>
    /// <param name="appInstanceDelegationService">app instance delegation handler</param>
    public AppsInstanceDelegationController(
        IMapper mapper,
        IAppsInstanceDelegationService appInstanceDelegationService)
    {
        _mapper = mapper;
        _appInstanceDelegationService = appInstanceDelegationService;
    }

    /// <summary>
    /// Delegates access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequestDto">The request model</param>
    /// <param name="resourceId">The resource id</param>
    /// <param name="instanceId">The instance id</param>
    /// <param name="token">platform token needed to define fetch wich app is calling this method</param>
    /// <returns>Result</returns>
    [HttpPost]
    [Route("v1/apps/instancedelegation/{resourceId}/{instanceId}")]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status206PartialContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delegation([FromBody] AppsInstanceDelegationRequestDto appInstanceDelegationRequestDto, [FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token)
    {
        AppsInstanceDelegationRequest request = _mapper.Map<AppsInstanceDelegationRequest>(appInstanceDelegationRequestDto);
        
        request.ResourceId = resourceId;
        request.InstanceId = instanceId;

        List<AttributeMatch> performedBy = GetOrgAppFromToken(token);
        request.PerformedBy = performedBy;

        Result<AppsInstanceDelegationResponse> serviceResult = await _appInstanceDelegationService.Delegate(request);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem?.ToActionResult();
        }

        // Check result
        int totalDelegations = request.Rights.Count();
        int validDelegations = serviceResult.Value.Rights.Count(r => r.Status == Core.Enums.DelegationStatus.Delegated);

        if (validDelegations == totalDelegations)
        {
            return Ok(_mapper.Map<AppsInstanceDelegationResponseDto>(serviceResult.Value));
        }

        return StatusCode(StatusCodes.Status206PartialContent, _mapper.Map<AppsInstanceDelegationResponseDto>(serviceResult.Value));        
    }    

    /// <summary>
    /// Gets app instance delegation
    /// </summary>
    /// <param name="resourceId">The resoure to fetch instance delegations for</param>
    /// <param name="instanceId">The instance to fetch instance delegations for</param>
    /// <returns>Result</returns>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Route("v1/apps/instancedelegation/{resourceId}/{instanceId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get([FromRoute] string resourceId, [FromRoute] string instanceId)
    {
        Result<AppsInstanceDelegationRequestDto> result = default;

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequestDto">The request model</param>
    /// <param name="resourceId">The resource identifier</param>
    /// <param name="instanceId">The instance identifier</param>
    /// <returns>Result</returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_APPS_INSTANCEDELEGATION)]
    [Route("v1/apps/instancedelegation/revoke/{resourceId}/{instanceId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Revoke([FromBody] AppsInstanceDelegationRequestDto appInstanceDelegationRequestDto, [FromRoute] string resourceId, [FromRoute] string instanceId)
    {
        Result<AppsInstanceDelegationResponseDto> result = default;

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// delegating app from the platform token
    /// </summary>
    private static List<AttributeMatch> GetOrgAppFromToken(string token)
    {
        List<AttributeMatch> performedBy = new List<AttributeMatch>();

        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var appidentifier = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute);
            performedBy.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, Value = jwtSecurityToken.Issuer });
            if (appidentifier != null)
            {
                performedBy.Add(new AttributeMatch { Id = appidentifier.Type, Value = appidentifier.Value });
            }
        }

        return performedBy;
    }
}
