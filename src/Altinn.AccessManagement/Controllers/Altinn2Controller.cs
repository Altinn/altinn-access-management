using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
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
    private readonly IMapper _mapper;

    /// <summary>
    /// ctor
    /// </summary>
    public Altinn2Controller(IAltinn2RightsService delegations, IMapper mapper)
    {
        _delegations = delegations;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
    /// </summary>
    /// <param name="party">reportee acting on behalf of</param>
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
    public async Task<IActionResult> GetOfferedRights([FromRoute] int party, CancellationToken cancellationToken)
    {
        var delegations = await _delegations.GetOfferedRights(party, cancellationToken);
        var response = _mapper.Map<IEnumerable<RightDelegationExternal>>(delegations);
        return Ok(response);
    }

    /// <summary>
    /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
    /// </summary>
    /// <param name="party">reportee acting on behalf of</param>
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
    public async Task<IActionResult> GetReceivedRights([FromRoute] int party, CancellationToken cancellationToken)
    {
        var delegations = await _delegations.GetReceivedRights(party, cancellationToken);
        var response = _mapper.Map<IEnumerable<RightDelegationExternal>>(delegations);
        return Ok(response);
    }
}