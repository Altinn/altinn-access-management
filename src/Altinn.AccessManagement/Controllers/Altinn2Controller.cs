using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Used by Altinn2 for managing delegations
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1")]
public class Altinn2Controller : ControllerBase
{
    private readonly IAltinn2RightsService _delegations;
    private readonly ILogger<RightsController> _logger;
    private readonly IMapper _mapper;

    /// <summary>
    /// ctor
    /// </summary>
    public Altinn2Controller(IAltinn2RightsService delegations, ILogger<RightsController> logger, IMapper mapper)
    {
        _delegations = delegations;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
    /// </summary>
    /// <param name="input">Used to specify the reportee party the authenticated user is acting on behalf of. Can either be the PartyId, or the placeholder values: 'person' or 'organization' in combination with providing the social security number or the organization number using the header values.</param>
    /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ)]
    [HttpGet("{party}/altinn2/rights/offered")]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<RightDelegationExternal>))]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<IActionResult> GetOfferedRights([FromRoute, FromHeader] AuthorizedPartyInput input, CancellationToken cancellationToken)
    {
        var reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(input.Party, HttpContext);
        var delegations = await _delegations.GetOfferedRights(reportee, cancellationToken);
        var response = _mapper.Map<IEnumerable<RightDelegationExternal>>(delegations);
        return Ok(response);
    }

    /// <summary>
    /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
    /// </summary>
    /// <param name="input">Used to specify the reportee party the authenticated user is acting on behalf of. Can either be the PartyId, or the placeholder values: 'person' or 'organization' in combination with providing the social security number or the organization number using the header values.</param>
    /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ)]
    [HttpGet("{party}/altinn2/rights/received")]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<RightDelegationExternal>))]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<IActionResult> GetReceivedRights([FromRoute, FromHeader] AuthorizedPartyInput input, CancellationToken cancellationToken)
    {
        var reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(input.Party, HttpContext);
        var delegations = await _delegations.GetReceivedRights(reportee, cancellationToken);
        var response = _mapper.Map<IEnumerable<RightDelegationExternal>>(delegations);
        return Ok(response);
    }
}