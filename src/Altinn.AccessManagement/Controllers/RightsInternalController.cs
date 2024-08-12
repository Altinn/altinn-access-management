using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using Altinn.AccessManagement.Core;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations regarding rights retrieval
    /// </summary>
    [ApiController]
    [Route("accessmanagement/api/v1/")]
    public class RightsInternalController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPolicyInformationPoint _pip;
        private readonly ISingleRightsService _rights;
        private readonly IAltinn2RightsService _rightsForAltinn2;

        /// <summary>
        /// Initializes a new instance of the <see cref="RightsInternalController"/> class.
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <param name="mapper">handler for mapping between internal and external models</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="singleRightsService">Service implementation for providing rights operations for BFF and external integrations</param>
        /// <param name="rightsForAltinn2">Service implementation for providing rights operations for Altinn 2 integrations</param>
        public RightsInternalController(ILogger<RightsInternalController> logger, IMapper mapper, IPolicyInformationPoint policyInformationPoint, ISingleRightsService singleRightsService, IAltinn2RightsService rightsForAltinn2)
        {
            _logger = logger;
            _mapper = mapper;
            _pip = policyInformationPoint;
            _rights = singleRightsService;
            _rightsForAltinn2 = rightsForAltinn2;
        }

        /// <summary>
        /// Endpoint for performing a query of rights between two parties for a specific resource
        /// </summary>
        /// <param name="rightsQuery">Query model for rights lookup</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user have access to</param>
        /// <response code="200" cref="List{RightExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("internal/query/rights/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<List<RightExternal>>> RightsQuery([FromBody] RightsQueryExternal rightsQuery, CancellationToken cancellationToken, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                RightsQuery rightsQueryInternal = _mapper.Map<RightsQuery>(rightsQuery);
                List<Right> rightsInternal = await _pip.GetRights(rightsQueryInternal, returnAllPolicyRights, cancellationToken: cancellationToken);
                return _mapper.Map<List<RightExternal>>(rightsInternal);
            }
            catch (ValidationException valEx)
            {
                ModelState.AddModelError("Validation Error", valEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during RightsQuery");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Endpoint for performing a query of rights a user can delegate to others a specified reportee and resource.
        /// IMPORTANT: The delegable rights lookup does itself not check that the user has access to the necessary RolesAdministration/MainAdmin or MaskinportenSchema delegation system resources needed to be allowed to perform delegation.
        /// </summary>
        /// <param name="rightsQuery">Query model for rights lookup</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user is allowed to delegate</param>
        /// <response code="200" cref="List{DelegationRightExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("internal/query/delegablerights")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<List<RightExternal>>> DelegableRightsQuery([FromBody] RightsQueryExternal rightsQuery, CancellationToken cancellationToken, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                RightsQuery rightsQueryInternal = _mapper.Map<RightsQuery>(rightsQuery);
                List<Right> rightsInternal = await _pip.GetRights(rightsQueryInternal, returnAllPolicyRights, getDelegableRights: true, cancellationToken);
                return _mapper.Map<List<RightExternal>>(rightsInternal);
            }
            catch (ValidationException valEx)
            {
                ModelState.AddModelError("Validation Error", valEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during DelegableRightsQuery");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Endpoint for performing a query of what rights a user can delegate to others on behalf of a specified reportee and resource.
        /// </summary>
        /// <param name="party">The reportee party</param>
        /// <param name="rightsDelegationCheckRequest">Request model for user rights delegation check</param>
        /// <response code="200" cref="List{RightDelegationStatusExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE)]
        [Route("internal/{party}/rights/delegation/delegationcheck")]
        [Route("{party}/rights/delegation/delegationcheck")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<ActionResult<List<RightDelegationCheckResultExternal>>> DelegationCheck([FromRoute] string party, [FromBody] RightsDelegationCheckRequestExternal rightsDelegationCheckRequest)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);

                RightsDelegationCheckRequest rightDelegationStatusRequestInternal = _mapper.Map<RightsDelegationCheckRequest>(rightsDelegationCheckRequest);
                rightDelegationStatusRequestInternal.From = reportee.SingleToList();

                DelegationCheckResponse delegationCheckResultInternal = await _rights.RightsDelegationCheck(authenticatedUserId, authenticationLevel, rightDelegationStatusRequestInternal);
                if (!delegationCheckResultInternal.IsValid)
                {
                    foreach (var error in delegationCheckResultInternal.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return _mapper.Map<List<RightDelegationCheckResultExternal>>(delegationCheckResultInternal.RightDelegationCheckResults);
            }
            catch (ValidationException valEx)
            {
                ModelState.AddModelError("Validation Error", valEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during DelegationCheck");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Endpoint for performing a delegation of rights on behalf of a specified reportee and resource, to a recipient.
        /// </summary>
        /// <param name="party">The reportee party</param>
        /// <param name="rightsDelegationRequest">Request model for rights delegation</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <response code="200" cref="List{RightDelegationStatusExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE)]
        [Route("internal/{party}/rights/delegation/offered")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<ActionResult<RightsDelegationResponseExternal>> Delegation([FromRoute] string party, [FromBody] RightsDelegationRequestExternal rightsDelegationRequest, CancellationToken cancellationToken)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);

                DelegationLookup rightsDelegationRequestInternal = _mapper.Map<DelegationLookup>(rightsDelegationRequest);
                rightsDelegationRequestInternal.From = reportee.SingleToList();

                DelegationActionResult delegationResultInternal = await _rights.DelegateRights(authenticatedUserId, authenticationLevel, rightsDelegationRequestInternal, cancellationToken);
                if (!delegationResultInternal.IsValid)
                {
                    foreach (var error in delegationResultInternal.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return _mapper.Map<RightsDelegationResponseExternal>(delegationResultInternal);
            }
            catch (ValidationException valEx)
            {
                ModelState.AddModelError("Validation Error", valEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (TooManyFailedLookupsException tooManyEx)
            {
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, (int)HttpStatusCode.TooManyRequests, detail: tooManyEx.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during Rights Delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
        /// </summary>
        /// <param name="party">reportee acting on behalf of</param>
        /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ)]
        [HttpGet("internal/{party}/rights/delegation/offered")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<RightDelegationExternal>))]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<IActionResult> GetOfferedRights([FromRoute] int party, CancellationToken cancellationToken)
        {
            var delegations = await _rightsForAltinn2.GetOfferedRights(party, cancellationToken);
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
        [HttpGet("internal/{party}/rights/delegation/received")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<RightDelegationExternal>))]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<IActionResult> GetReceivedRights([FromRoute] int party, CancellationToken cancellationToken)
        {
            var delegations = await _rightsForAltinn2.GetReceivedRights(party, cancellationToken);
            var response = _mapper.Map<IEnumerable<RightDelegationExternal>>(delegations);
            return Ok(response);
        }

        /// <summary>
        /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
        /// </summary>
        /// <param name="input">Used to specify the reportee party the authenticated user is acting on behalf of. Can either be the PartyId, or the placeholder values: 'person' or 'organization' in combination with providing the social security number or the organization number using the header values.</param>
        /// <param name="body">The specific delegation to be revoked</param>
        /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE)]
        [ActionName(nameof(RevokeReceivedDelegation))]
        [HttpPost("internal/{party}/rights/delegation/received/revoke")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(void))]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<IActionResult> RevokeReceivedDelegation([FromRoute, FromHeader] AuthorizedPartyInput input, [FromBody] RevokeReceivedDelegationExternal body, CancellationToken cancellationToken)
        {
            try
            {
                int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(input.Party, HttpContext);
                var delegation = _mapper.Map<DelegationLookup>(body);

                delegation.To = reportee.SingleToList();

                var result = await _rights.RevokeRightsDelegation(authenticatedUserId, delegation, cancellationToken);
                if (result != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value[0]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return NoContent();
            }
            catch (FormatException ex)
            {
                ModelState.AddModelError("Validation Error", ex.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("Validation Error", ex.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(StatusCodes.Status500InternalServerError, ex, "Internal exception occurred during Rights Delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Gets a list of all recipients having received right delegations from the reportee party including the resource/app/service info, but not specific rights
        /// </summary>
        /// <param name="input">Used to specify the reportee party the authenticated user is acting on behalf of. Can either be the PartyId, or the placeholder values: 'person' or 'organization' in combination with providing the social security number or the organization number using the header values.</param>
        /// <param name="body">payload</param>
        /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE)]
        [ActionName(nameof(RevokeOfferedDelegation))]
        [HttpPost("internal/{party}/rights/delegation/offered/revoke")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(void))]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<IActionResult> RevokeOfferedDelegation([FromRoute, FromHeader] AuthorizedPartyInput input, [FromBody] RevokeOfferedDelegationExternal body, CancellationToken cancellationToken)
        {
            try
            {
                int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(input.Party, HttpContext);
                var delegation = _mapper.Map<DelegationLookup>(body);

                delegation.From = reportee.SingleToList();
                var result = await _rights.RevokeRightsDelegation(authenticatedUserId, delegation, cancellationToken);
                if (result != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value[0]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return NoContent();
            }
            catch (FormatException ex)
            {
                ModelState.AddModelError("Validation Error", ex.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("Validation Error", ex.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(StatusCodes.Status500InternalServerError, ex, "Internal exception occurred during Rights Delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
            }
        }

        /// <summary>
        /// Clears access chaching for a given recipient having received right delegations from the reportee party, in order for the rights to take effect as imidiately as possible in the distributed authorization system between Altinn 2 and Altinn 3.
        /// </summary>
        /// <param name="party">Used to specify the reportee party id the authenticated user is acting on behalf of.</param>
        /// <param name="to">Attribute specification of the uuid of the recipient to clear access caching for</param>
        /// <param name="cancellationToken">Cancellation token used for cancelling the inbound HTTP</param>
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE)]
        [ActionName(nameof(ClearAccessCache))]
        [HttpPut("internal/{party}/accesscache/clear")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(void))]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [FeatureGate(FeatureFlags.RightsDelegationApi)]
        public async Task<IActionResult> ClearAccessCache([FromRoute] int party, [FromBody] BaseAttributeExternal to, CancellationToken cancellationToken)
        {
            BaseAttribute toAttribute = _mapper.Map<BaseAttribute>(to);
            HttpResponseMessage response = await _rightsForAltinn2.ClearReporteeRights(party, toAttribute, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, (int)response.StatusCode, detail: $"Could not complete the request. Reason: {response.ReasonPhrase}"));
            }

            return Ok();
        }
    }
}