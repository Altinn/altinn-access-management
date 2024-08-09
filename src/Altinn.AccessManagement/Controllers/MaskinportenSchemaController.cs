#nullable enable
using System.ComponentModel.DataAnnotations;
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
    /// Controller responsible for all operations regarding Maskinporten Schema
    /// </summary>
    [ApiController]
    [Route("accessmanagement/api/v1/")]
    public class MaskinportenSchemaController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMaskinportenSchemaService _delegation;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskinportenSchemaController"/> class.
        /// </summary>
        /// <param name="logger">logger instance</param>
        /// <param name="delegationsService">Handler for the delegation service</param>
        /// <param name="mapper">mapper handler</param>
        public MaskinportenSchemaController(
            ILogger<MaskinportenSchemaController> logger,
            IMaskinportenSchemaService delegationsService,
            IMapper mapper)
        {
            _logger = logger;
            _delegation = delegationsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint for API owners and integration with Maskinporten for retrieval of information from Altinn 3 Access Management regarding active delegations of maskinporten schemas between organizations, giving access to one or more scopes in maskinporten.
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("admin/delegations/maskinportenschema")]// Old path to be removed later (after maskinporten no longer use A2 proxy or A2 updated with new endpoint)
        [Route("maskinporten/delegations/")]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATIONS_PROXY)]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MPDelegationExternal>>> GetMaskinportenDelegations([FromQuery] string? supplierOrg, [FromQuery] string? consumerOrg, [FromQuery] string scope, CancellationToken cancellationToken)
        {
            if (!MaskinportenSchemaAuthorizer.IsAuthorizedDelegationLookupAccess(scope, HttpContext.User))
            {
                ProblemDetails result = new() { Title = $"Not authorized for lookup of delegations for the scope: {scope}", Status = StatusCodes.Status403Forbidden, Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3" };
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            if (!string.IsNullOrEmpty(supplierOrg) && !IdentifierUtil.IsValidOrganizationNumber(supplierOrg))
            {
                ModelState.AddModelError(nameof(supplierOrg), "Supplierorg is not an valid organization number");
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }

            if (!string.IsNullOrEmpty(consumerOrg) && !IdentifierUtil.IsValidOrganizationNumber(consumerOrg))
            {
                ModelState.AddModelError(nameof(consumerOrg), "Consumerorg is not an valid organization number");
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }

            try
            {
                List<Delegation> delegations = await _delegation.GetMaskinportenDelegations(supplierOrg, consumerOrg, scope, cancellationToken);
                List<MPDelegationExternal> delegationsExternal = _mapper.Map<List<MPDelegationExternal>>(delegations);

                return delegationsExternal;
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName ?? "Validation Error", ex.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMaskinportenDelegation failed to fetch delegations");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }

        /// <summary>
        /// Endpoint for performing a query of what rights a user can delegate to others on behalf of a specified reportee and maskinporten schema.
        /// </summary>
        /// <param name="party">The reportee party</param>
        /// <param name="rightsDelegationCheckRequest">Request model for user rights delegation check</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <response code="200" cref="List{RightDelegationStatusExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE)]
        [Route("{party}/maskinportenschema/delegationcheck")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<RightDelegationCheckResultExternal>>> DelegationCheck([FromRoute] string party, [FromBody] RightsDelegationCheckRequestExternal rightsDelegationCheckRequest, CancellationToken cancellationToken)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);

                RightsDelegationCheckRequest rightDelegationStatusRequestInternal = _mapper.Map<RightsDelegationCheckRequest>(rightsDelegationCheckRequest);
                rightDelegationStatusRequestInternal.From = reportee.SingleToList();

                DelegationCheckResponse delegationCheckResultInternal = await _delegation.DelegationCheck(authenticatedUserId, authenticationLevel, rightDelegationStatusRequestInternal, cancellationToken);
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
        /// Endpoint for delegating maskinporten scheme resources between two parties
        /// </summary>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE)]
        [Route("{party}/maskinportenschema/offered")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RightsDelegationResponseExternal>> MaskinportenScopeDelegation([FromRoute] string party, [FromBody] RightsDelegationRequestExternal delegation, CancellationToken cancellationToken)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegation);
                internalDelegation.From = reportee.SingleToList();
                DelegationActionResult response = await _delegation.DelegateMaskinportenSchema(authenticatedUserId, authenticationLevel, internalDelegation, cancellationToken);

                if (!response.IsValid)
                {
                    foreach (string errorKey in response.Errors.Keys)
                    {
                        ModelState.AddModelError(errorKey, response.Errors[errorKey]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                RightsDelegationResponseExternal delegationResponse = _mapper.Map<RightsDelegationResponseExternal>(response);

                return StatusCode(201, delegationResponse);
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is ArgumentException)
                {
                    ModelState.AddModelError("Validation Error", ex.Message);
                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                _logger.LogError(ex, "Internal exception occurred during maskinportenschema delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }

        /// <summary>
        /// Endpoint for retrieving delegated Maskinporten resources offered by the reportee party to others
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_READ)]
        [Route("{party}/maskinportenschema/offered")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MaskinportenSchemaDelegationExternal>>> GetOfferedMaskinportenSchemaDelegations([FromRoute] string party, CancellationToken cancellationToken)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetOfferedMaskinportenSchemaDelegations(partyMatch, cancellationToken);
                return _mapper.Map<List<MaskinportenSchemaDelegationExternal>>(delegations);
            }
            catch (ArgumentException argEx)
            {
                ModelState.AddModelError("Validation Error", argEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                _logger.LogError(ex, "Failed to fetch offered delegations, See the error message for more details {errorMessage}", errorMessage);
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }

        /// <summary>
        /// Endpoint for revoking a maskinporten scope delegation on behalf of the party having offered the delegation
        /// </summary>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE)]
        [Route("{party}/maskinportenschema/offered/revoke")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RevokeOfferedMaskinportenScopeDelegation([FromRoute] string party, [FromBody] RevokeOfferedDelegationExternal delegation, CancellationToken cancellationToken)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegation);
                internalDelegation.From = reportee.SingleToList();
                DelegationActionResult response = await _delegation.RevokeMaskinportenSchemaDelegation(authenticatedUserId, internalDelegation, cancellationToken);

                if (!response.IsValid)
                {
                    foreach (string errorKey in response.Errors.Keys)
                    {
                        ModelState.AddModelError(errorKey, response.Errors[errorKey]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is ArgumentException)
                {
                    ModelState.AddModelError("Validation Error", ex.Message);
                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                _logger.LogError(ex, "Internal exception occurred during deletion of maskinportenschema delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }

        /// <summary>
        /// Endpoint for retrieving received Maskinporten resource delegation to the reportee party from others
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_READ)]
        [Route("{party}/maskinportenschema/received")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MaskinportenSchemaDelegationExternal>>> GetReceivedMaskinportenSchemaDelegations([FromRoute] string party, CancellationToken cancellationToken)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetReceivedMaskinportenSchemaDelegations(partyMatch, cancellationToken);
                return _mapper.Map<List<MaskinportenSchemaDelegationExternal>>(delegations);
            }
            catch (ArgumentException argEx)
            {
                ModelState.AddModelError("Validation Error", argEx.Message);
                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                _logger.LogError(ex, "Failed to fetch received delegations, See the error message for more details {errorMessage}", errorMessage);
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }

        /// <summary>
        /// Endpoint for revoking a maskinporten scope delegation on behalf of the party having received the delegation
        /// </summary>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE)]
        [Route("{party}/maskinportenschema/received/revoke")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RevokeReceivedMaskinportenScopeDelegation([FromRoute] string party, [FromBody] RevokeReceivedDelegationExternal delegation, CancellationToken cancellationToken)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegation);
                internalDelegation.To = reportee.SingleToList();
                DelegationActionResult response = await _delegation.RevokeMaskinportenSchemaDelegation(authenticatedUserId, internalDelegation, cancellationToken);

                if (!response.IsValid)
                {
                    foreach (string errorKey in response.Errors.Keys)
                    {
                        ModelState.AddModelError(errorKey, response.Errors[errorKey]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex is ValidationException || ex is ArgumentException)
                {
                    ModelState.AddModelError("Validation Error", ex.Message);
                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                _logger.LogError(ex, "Internal exception occurred during deletion of maskinportenschema delegation");
                return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext));
            }
        }
    }
}
