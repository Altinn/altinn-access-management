#nullable enable
using System.ComponentModel.DataAnnotations;
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

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations regarding Maskinporten Schema
    /// </summary>
    [ApiController]
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
        [Route("accessmanagement/api/v1/admin/delegations/maskinportenschema")]// Old path to be removed later (after maskinporten no longer use A2 proxy or A2 updated with new endpoint)
        [Route("accessmanagement/api/v1/maskinporten/delegations/")]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATIONS_PROXY)]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MPDelegationExternal>>> GetMaskinportenDelegations([FromQuery] string? supplierOrg, [FromQuery] string? consumerOrg, [FromQuery] string scope)
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
                List<Delegation> delegations = await _delegation.GetMaskinportenDelegations(supplierOrg, consumerOrg, scope);
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
        /// Endpoint for delegating maskinporten scheme resources between two parties
        /// </summary>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE)]
        [Route("accessmanagement/api/v1/{party}/maskinportenschema/offered")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DelegationOutputExternal>> MaskinportenScopeDelegation([FromRoute] string party, [FromBody] DelegationInput delegation)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            var delegationInputExternal = new DelegationInputExternal
            {
                To = new List<AttributeMatchExternal>
                {
                    // Create an AttributeMatchExternal object for the 'To' property
                    new AttributeMatchExternal
                    {
                        // Set properties of AttributeMatchExternal as needed
                        // For example:
                        Id = delegation.To.FirstOrDefault().Id,
                        Value = delegation.To.FirstOrDefault().Value
                    }
                },
                Rights = new List<BaseRightExternal>()
            };
            
            var baseRightExternal = delegation.Rights.Select(right => new BaseRightExternal
            {
                Resource = right.Resource.Select(resource => new AttributeMatchExternal
                {
                    Id = resource.Id,
                    Value = resource.Value,
                }).ToList(),
                Action = new AttributeMatchExternal
                {
                    Id = right.Action,
                    Value = right.Action
                }
            }).ToList();

            delegationInputExternal.Rights = baseRightExternal;

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegationInputExternal);
                internalDelegation.From = reportee.SingleToList();
                DelegationActionResult response = await _delegation.DelegateMaskinportenSchema(authenticatedUserId, authenticationLevel, internalDelegation);

                if (!response.IsValid)
                {
                    foreach (string errorKey in response.Errors.Keys)
                    {
                        ModelState.AddModelError(errorKey, response.Errors[errorKey]);
                    }

                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                DelegationOutputExternal delegationOutput = _mapper.Map<DelegationOutputExternal>(response);
                DelegationHelper.TryGetResourceFromAttributeMatch(response.Rights.First().Resource, out var _, out string resourceId, out var _, out var _);
                DelegationHelper.TryGetPartyIdFromAttributeMatch(internalDelegation.To, out int toPartyId);
                return Created(new Uri($"https://{Request.Host}/accessmanagement/api/v1/{party}/delegations/maskinportenschema/offered?to={toPartyId}&resourceId={resourceId}"), delegationOutput);
            }
            catch
                (Exception ex)
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
        [Route("accessmanagement/api/v1/{party}/maskinportenschema/offered")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MaskinportenSchemaDelegationExternal>>> GetOfferedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetOfferedMaskinportenSchemaDelegations(partyMatch);
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
        [Route("accessmanagement/api/v1/{party}/maskinportenschema/offered/revoke")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RevokeOfferedMaskinportenScopeDelegation([FromRoute] string party, [FromBody] RevokeOfferedDelegationExternal delegation)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegation);
                internalDelegation.From = reportee.SingleToList();
                DelegationActionResult response = await _delegation.RevokeMaskinportenSchemaDelegation(authenticatedUserId, internalDelegation);

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
        [Route("accessmanagement/api/v1/{party}/maskinportenschema/received")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MaskinportenSchemaDelegationExternal>>> GetReceivedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetReceivedMaskinportenSchemaDelegations(partyMatch);
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
        [Route("accessmanagement/api/v1/{party}/maskinportenschema/received/revoke")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RevokeReceivedMaskinportenScopeDelegation([FromRoute] string party, [FromBody] RevokeReceivedDelegationExternal delegation)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationLookup internalDelegation = _mapper.Map<DelegationLookup>(delegation);
                internalDelegation.To = reportee.SingleToList();
                DelegationActionResult response = await _delegation.RevokeMaskinportenSchemaDelegation(authenticatedUserId, internalDelegation);

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
