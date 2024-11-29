﻿#nullable enable

using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
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
    /// Finds all rights the authenticated app can delegate for a given app instance
    /// </summary>
    /// <param name="resourceId">The resource id</param>
    /// <param name="instanceId">The instance id</param>
    /// <param name="token">platform token needed to define fetch wich app is calling this method</param>
    /// <returns>Result</returns>
    [HttpGet]
    [Route("v1/app/delegationcheck/resource/{resourceId}/instance/{instanceId}")]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ResourceRightDelegationCheckResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DelegationCheck([FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token)
    {
        ResourceIdUrn.ResourceId? performer = GetOrgAppFromToken(token);

        if (performer == null)
        {
            return Forbid();
        }

        AppsInstanceDelegationRequest request = new() { ResourceId = resourceId, InstanceId = instanceId, PerformedBy = performer };

        Result<ResourceDelegationCheckResponse> serviceResult = await _appInstanceDelegationService.DelegationCheck(request);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem.ToActionResult();
        }

        PaginatedLinks? next = new PaginatedLinks(null);
        IEnumerable<ResourceRightDelegationCheckResultDto> data = _mapper.Map<IEnumerable<ResourceRightDelegationCheckResultDto>>(serviceResult.Value.ResourceRightDelegationCheckResults);

        Paginated<ResourceRightDelegationCheckResultDto> result = new Paginated<ResourceRightDelegationCheckResultDto>(next, data);

        return Ok(result);
    }

    /// <summary>
    /// Delegates access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequestDto">The request model</param>
    /// <param name="resourceId">The resource id</param>
    /// <param name="instanceId">The instance id</param>
    /// <param name="token">platform token needed to define fetch wich app is calling this method</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Result</returns>
    [HttpPost]
    [Route("v1/app/delegations/resource/{resourceId}/instance/{instanceId}")]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status206PartialContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delegation([FromBody] AppsInstanceDelegationRequestDto appInstanceDelegationRequestDto, [FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token, CancellationToken cancellationToken = default)
    {
        ResourceIdUrn.ResourceId? performer = GetOrgAppFromToken(token);

        if (performer == null)
        {
            return Forbid();
        }

        AppsInstanceDelegationRequest request = _mapper.Map<AppsInstanceDelegationRequest>(appInstanceDelegationRequestDto);

        request.ResourceId = resourceId;
        request.InstanceId = instanceId;
        request.PerformedBy = performer;
        request.InstanceDelegationSource = Core.Enums.InstanceDelegationSource.App;
        request.InstanceDelegationMode = Core.Enums.InstanceDelegationMode.Normal;

        Result<AppsInstanceDelegationResponse> serviceResult = await _appInstanceDelegationService.Delegate(request, cancellationToken);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem.ToActionResult();
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
    /// <param name="token">the platformToken to use for Authorization</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Result</returns>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Route("v1/app/delegations/resource/{resourceId}/instance/{instanceId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Get([FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token, CancellationToken cancellationToken = default)
    {
        ResourceIdUrn.ResourceId? performer = GetOrgAppFromToken(token);

        if (performer == null)
        {
            return Forbid();
        }

        AppsInstanceGetRequest request = new()
        {
            InstanceDelegationSource = Core.Enums.InstanceDelegationSource.App,            
            PerformingResourceId = performer,
            ResourceId = resourceId,
            InstanceId = instanceId,
        };

        Result<List<AppsInstanceDelegationResponse>> serviceResult = await _appInstanceDelegationService.Get(request, cancellationToken);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem.ToActionResult();
        }

        List<AppsInstanceDelegationResponseDto> list = _mapper.Map<List<AppsInstanceDelegationResponseDto>>(serviceResult.Value);
        PaginatedLinks links = new PaginatedLinks(null);
        Paginated<AppsInstanceDelegationResponseDto> result = new Paginated<AppsInstanceDelegationResponseDto>(links, list);

        return Ok(result);
    }

    /// <summary>
    /// Revokes access to an app instance
    /// </summary>
    /// <param name="appInstanceDelegationRequestDto">The request model</param>
    /// <param name="resourceId">The resource identifier</param>
    /// <param name="instanceId">The instance identifier</param>
    /// <param name="token">the platformToken to use for Authorization</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Result</returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Route("v1/app/delegationrevoke/resource/{resourceId}/instance/{instanceId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult?> Revoke([FromBody] AppsInstanceDelegationRequestDto appInstanceDelegationRequestDto, [FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token, CancellationToken cancellationToken = default)
    {
        AppsInstanceDelegationRequest request = _mapper.Map<AppsInstanceDelegationRequest>(appInstanceDelegationRequestDto);

        ResourceIdUrn.ResourceId? performer = GetOrgAppFromToken(token);

        if (performer == null)
        {
            return Forbid();
        }

        request.ResourceId = resourceId;
        request.InstanceId = instanceId;
        request.InstanceDelegationSource = Core.Enums.InstanceDelegationSource.App;
        request.PerformedBy = performer;

        Result<AppsInstanceRevokeResponse> serviceResult = await _appInstanceDelegationService.Revoke(request, cancellationToken);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem?.ToActionResult();
        }

        // Check result
        int totalDelegations = request.Rights.Count();
        int validDelegations = serviceResult.Value.Rights.Count(r => r.Status == Core.Enums.RevokeStatus.Revoked);

        if (validDelegations == totalDelegations)
        {
            return Ok(_mapper.Map<AppsInstanceRevokeResponseDto>(serviceResult.Value));
        }

        return StatusCode(StatusCodes.Status206PartialContent, _mapper.Map<AppsInstanceRevokeResponseDto>(serviceResult.Value));
    }

    /// <summary>
    /// Revokes all access to an app instance
    /// </summary>
    /// <param name="resourceId">The resource identifier</param>
    /// <param name="instanceId">The instance identifier</param>
    /// <param name="token">the platformToken to use for Authorization</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Result</returns>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [Route("v1/app/delegationrevoke/resource/{resourceId}/instance/{instanceId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AppsInstanceDelegationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult?> RevokeAll([FromRoute] string resourceId, [FromRoute] string instanceId, [FromHeader(Name = "PlatformAccessToken")] string token, CancellationToken cancellationToken = default)
    {
        ResourceIdUrn.ResourceId? performer = GetOrgAppFromToken(token);

        if (performer == null)
        {
            return Forbid();
        }

        AppsInstanceGetRequest request = new AppsInstanceGetRequest 
        { 
            ResourceId = resourceId,
            InstanceId = instanceId,
            PerformingResourceId = performer,
            InstanceDelegationSource = Core.Enums.InstanceDelegationSource.App
        };

        Result<List<AppsInstanceRevokeResponse>> serviceResult = await _appInstanceDelegationService.RevokeAll(request, cancellationToken);

        if (serviceResult.IsProblem)
        {
            return serviceResult.Problem?.ToActionResult();
        }

        List<AppsInstanceRevokeResponseDto> items = _mapper.Map<List<AppsInstanceRevokeResponseDto>>(serviceResult.Value);
        PaginatedLinks links = new PaginatedLinks(null);

        Paginated<AppsInstanceRevokeResponseDto> result = new(links, items);

        return Ok(result);        
    }

    /// <summary>
    /// delegating app from the platform token
    /// </summary>
    private static ResourceIdUrn.ResourceId? GetOrgAppFromToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var appidentifier = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute);
            if (appidentifier != null)
            {
                return ResourceIdUrn.ResourceId.Create(ResourceIdentifier.CreateUnchecked($"app_{jwtSecurityToken.Issuer}_{appidentifier.Value}"));
            }
        }

        return null;
    }
}