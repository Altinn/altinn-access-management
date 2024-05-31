using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;
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
    private readonly IContextRetrievalService _contextRetrievalService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesController"/> class.
    /// </summary>
    /// <param name="logger">logger service</param>
    /// <param name="mapper">mapper service</param>
    /// <param name="authorizedPartiesService">service implementation for authorized parties</param>
    /// <param name="contextRetrievalService">service implementation for getting information regaring users, party etc.</param>
    public AuthorizedPartiesController(
        ILogger<AuthorizedPartiesController> logger,
        IMapper mapper,
        IAuthorizedPartiesService authorizedPartiesService,
        IContextRetrievalService contextRetrievalService)
    {
        _logger = logger;
        _mapper = mapper;
        _authorizedPartiesService = authorizedPartiesService;
        _contextRetrievalService = contextRetrievalService;
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
    [ProducesResponseType(typeof(List<AuthorizedPartyExternal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<ActionResult<List<AuthorizedPartyExternal>>> GetAuthorizedParties(bool includeAltinn2 = false, CancellationToken cancellationToken = default)
    {
        try
        {
            int userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == 0)
            {
                return Unauthorized();
            }

            List<AuthorizedParty> authorizedParties = await _authorizedPartiesService.GetAuthorizedPartiesForUser(userId, includeAltinn2, includeAuthorizedResourcesThroughRoles: false, cancellationToken);

            return _mapper.Map<List<AuthorizedPartyExternal>>(authorizedParties);
        }
        catch (Exception ex)
        {
            _logger.LogError(500, ex, "Unexpected internal exception occurred during GetAuthorizedParties");
            return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
        }
    }

    /// <summary>
    /// Endpoint for retrieving a given authorized party if it exists (with option to include Authorized Parties, aka Reportees from Altinn 2, when getting the underlying list of authorized parties) in the authenticated user's list of authorized parties
    /// </summary>
    /// <param name="partyId">The partyId to get if exists in the authenticated user's list of authorized parties</param>
    /// <param name="includeAltinn2">Optional (Default: False): Whether Authorized Parties from Altinn 2 should be included in the underlying result set, and if access to Altinn 3 resources through having Altinn 2 roles should be included.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <response code="200" cref="List{AuthorizedParty}">Ok</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [Authorize]
    [Route("authorizedparty/{partyId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<AuthorizedPartyExternal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<ActionResult<AuthorizedPartyExternal>> GetAuthorizedParty([FromRoute] int partyId, bool includeAltinn2 = false, CancellationToken cancellationToken = default)
    {
        try
        {
            int userId = AuthenticationHelper.GetUserId(HttpContext);
            if (userId == 0)
            {
                return Unauthorized();
            }

            List<AuthorizedParty> authorizedParties = await _authorizedPartiesService.GetAuthorizedPartiesForUser(userId, includeAltinn2, includeAuthorizedResourcesThroughRoles: false, cancellationToken);
            AuthorizedParty authorizedParty = authorizedParties.Find(ap => ap.PartyId == partyId && !ap.OnlyHierarchyElementWithNoAccess)
                ?? authorizedParties.SelectMany(ap => ap.Subunits).FirstOrDefault(subunit => subunit.PartyId == partyId);

            if (authorizedParty == null)
            {
                ModelState.AddModelError("InvalidParty", "The party id is either invalid or is not an authorized party for the authenticated user");
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }

            return _mapper.Map<AuthorizedPartyExternal>(authorizedParty);
        }
        catch (Exception ex)
        {
            _logger.LogError(500, ex, "Unexpected internal exception occurred during GetAuthorizedParties");
            return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
        }
    }

    /// <summary>
    /// Endpoint for retrieving all authorized parties (with option to include Authorized Parties, aka Reportees, from Altinn 2) for the authenticated user
    /// </summary>
    /// <param name="party">The party to retrieve the list of authorized parties for</param>
    /// <param name="includeAltinn2">Optional (Default: False): Whether Authorized Parties from Altinn 2 should be included in the result set, and if access to Altinn 3 resources through having Altinn 2 roles should be included.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <response code="200" cref="List{AuthorizedParty}">Ok</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ)]
    [Route("{party}/authorizedparties")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<AuthorizedPartyExternal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<ActionResult<List<AuthorizedPartyExternal>>> GetAuthorizedPartiesAsAccessManager([FromRoute] int party, bool includeAltinn2 = false, CancellationToken cancellationToken = default)
    {
        try
        {
            int authenticatedUserPartyId = AuthenticationHelper.GetPartyId(HttpContext);

            Party subject = await _contextRetrievalService.GetPartyAsync(party, cancellationToken);
            if (subject.PartyTypeName == PartyType.Person && subject.PartyId != authenticatedUserPartyId)
            {
                return Forbid();
            }

            List<AuthorizedParty> authorizedParties = await _authorizedPartiesService.GetAuthorizedPartiesForParty(subject.PartyId, includeAltinn2, includeAuthorizedResourcesThroughRoles: false, cancellationToken);

            return _mapper.Map<List<AuthorizedPartyExternal>>(authorizedParties);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("Argument exception", ex.Message);
            return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
        }
        catch (Exception ex)
        {
            _logger.LogError(500, ex, "Unexpected internal exception occurred during GetAuthorizedParties");
            return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
        }
    }
}
