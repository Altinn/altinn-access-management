using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Controller responsible for all operations for retrieving AuthorizedParties list for a user / organization / system
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/")]
public class AuthorizedPartiesController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IAuthorizedPartiesService _authorizedPartiesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesController"/> class.
    /// </summary>
    /// <param name="logger">logger service</param>
    /// <param name="mapper">mapper service</param>
    /// <param name="authorizedPartiesService">service implementation for authorized parties</param>
    public AuthorizedPartiesController(
        ILogger<AuthorizedPartiesController> logger,
        IMapper mapper,
        IAuthorizedPartiesService authorizedPartiesService)
    {
        _logger = logger;
        _mapper = mapper;
        _authorizedPartiesService = authorizedPartiesService;
    }

    /// <summary>
    /// Endpoint for retrieving all authorized parties (with option to include Authorized Parties, aka Reportees, from Altinn 2) for the authenticated user
    /// </summary>
    /// <param name="includeAltinn2">Optional (Default: False): Whether Authorized Parties from Altinn 2 should be included in the result set, and if access to Altinn 3 resources through having Altinn 2 roles should be included.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <response code="200" cref="List{AuthorizedParty}">Ok</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [Authorize]
    [Route("authorizedparties")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<AuthorizedParty>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<ActionResult<List<AuthorizedParty>>> GetAuthorizedParties(bool includeAltinn2 = false, CancellationToken cancellationToken = default)
    {
        try
        {
            int userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == 0)
            {
                return Unauthorized();
            }

            List<AuthorizedParty> authorizedParties = await _authorizedPartiesService.GetAuthorizedParties(userId, includeAltinn2, cancellationToken);

            return _mapper.Map<List<AuthorizedParty>>(authorizedParties);
        }
        catch (Exception ex)
        {
            _logger.LogError(500, ex, "Unexpected internal exception occurred during GetAuthorizedParties");
            return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
        }
    }
}
