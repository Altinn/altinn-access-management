using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Filters;
using Altinn.AccessManagement.Models;
using Altinn.AccessManagement.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [ApiController]
    [AutoValidateAntiforgeryTokenIfAuthCookie]
    public class DelegationsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IDelegationsService _delegation;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="policyAdministrationPoint">The policy administration point</param>
        /// <param name="delegationsService">Handler for the delegation service</param>
        /// <param name="mapper">mapper handler</param>
        public DelegationsController(
            ILogger<DelegationsController> logger, 
            IPolicyInformationPoint policyInformationPoint, 
            IPolicyAdministrationPoint policyAdministrationPoint, 
            IDelegationsService delegationsService,
            IMapper mapper)
        {
            _logger = logger;
            _pap = policyAdministrationPoint;
            _pip = policyInformationPoint;
            _delegation = delegationsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint for adding one or more rules for the given app/offeredby/coveredby. This updates or creates a new delegated policy of type "DirectlyDelegated". DelegatedByUserId is included to store history information in 3.0.
        /// </summary>
        /// <param name="rules">All rules to be delegated</param>
        /// <response code="201" cref="List{PolicyRule}">Created</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/addrules")]
        public async Task<ActionResult> Post([FromBody] List<Rule> rules)
        {
            if (rules == null || rules.Count < 1)
            {
                return BadRequest("Missing rules in body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            List<Rule> delegationResults = await _pap.TryWriteDelegationPolicyRules(rules);

            if (delegationResults.All(r => r.CreatedSuccessfully))
            {
                return Created("Created", delegationResults);
            }

            if (delegationResults.Any(r => r.CreatedSuccessfully))
            {
                return StatusCode(206, delegationResults);
            }

            string rulesJson = JsonSerializer.Serialize(rules);
            _logger.LogInformation("Delegation could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{rulesJson}", rulesJson);
            return BadRequest("Delegation could not be completed");
        }

        /// <summary>
        /// Endpoint for retrieving delegated rules between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/getrules")]
        public async Task<ActionResult<List<Rule>>> GetRules([FromBody] RuleQuery ruleQuery, [FromQuery] bool onlyDirectDelegations = false)
        {
            List<int> coveredByPartyIds = new List<int>();
            List<int> coveredByUserIds = new List<int>();
            List<int> offeredByPartyIds = new List<int>();
            List<string> resourceIds = new List<string>();

            if (ruleQuery.KeyRolePartyIds.Any(id => id != 0))
            {
                coveredByPartyIds.AddRange(ruleQuery.KeyRolePartyIds);
            }

            if (ruleQuery.ParentPartyId != 0)
            {
                offeredByPartyIds.Add(ruleQuery.ParentPartyId);
            }

            foreach (List<AttributeMatch> resource in ruleQuery.Resources)
            {
                if (DelegationHelper.TryGetResourceFromAttributeMatch(resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out _, out _))
                {
                    resourceIds.Add(resourceId);
                }
            }

            if (DelegationHelper.TryGetPartyIdFromAttributeMatch(ruleQuery.CoveredBy, out int partyId))
            {
                coveredByPartyIds.Add(partyId);
            }
            else if (DelegationHelper.TryGetUserIdFromAttributeMatch(ruleQuery.CoveredBy, out int userId))
            {
                coveredByUserIds.Add(userId);
            }

            if (ruleQuery.OfferedByPartyId != 0)
            {
                offeredByPartyIds.Add(ruleQuery.OfferedByPartyId);
            }

            if (offeredByPartyIds.Count == 0)
            {
                return StatusCode(400, $"Unable to get the rules: Missing offeredbyPartyId value.");
            }

            if (offeredByPartyIds.Count == 0 && coveredByPartyIds.Count == 0 && coveredByUserIds.Count == 0)
            {
                return StatusCode(400, $"Unable to get the rules: Missing offeredby and coveredby values.");
            }

            List<Rule> rulesList = await _pip.GetRulesAsync(resourceIds, offeredByPartyIds, coveredByPartyIds, coveredByUserIds);
            DelegationHelper.SetRuleType(rulesList, ruleQuery.OfferedByPartyId, ruleQuery.KeyRolePartyIds, ruleQuery.CoveredBy, ruleQuery.ParentPartyId);
            return Ok(rulesList);
        }

        /// <summary>
        /// Endpoint for deleting delegated rules between parties
        /// </summary>
        /// <response code="200" cref="List{PolicyRule}">Deleted</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/deleterules")]
        public async Task<ActionResult> DeleteRule([FromBody] RequestToDeleteRuleList rulesToDelete)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _pap.TryDeleteDelegationPolicyRules(rulesToDelete);
            int ruleCountToDelete = DelegationHelper.GetRulesCountToDeleteFromRequestToDelete(rulesToDelete);
            int deletionResultsCount = deletionResults.Count;

            if (deletionResultsCount == ruleCountToDelete)
            {
                return StatusCode(200, deletionResults);
            }

            string rulesToDeleteSerialized = JsonSerializer.Serialize(rulesToDelete);
            if (deletionResultsCount > 0)
            {
                string deletionResultsSerialized = JsonSerializer.Serialize(deletionResults);
                _logger.LogInformation("Partial deletion completed deleted {deletionResultsCount} of {ruleCountToDelete}.\n{rulesToDeleteSerialized}\n{deletionResultsSerialized}", deletionResultsCount, ruleCountToDelete, rulesToDeleteSerialized, deletionResultsSerialized);
                return StatusCode(206, deletionResults);
            }

            _logger.LogInformation("Deletion could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{rulesToDeleteSerialized}", rulesToDeleteSerialized);
            return StatusCode(400, $"Unable to complete deletion");
        }

        /// <summary>
        /// Endpoint for deleting an entire delegated policy between parties
        /// </summary>
        /// <response code="200" cref="List{PolicyRule}">Deleted</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/deletepolicy")]
        public async Task<ActionResult> DeletePolicy([FromBody] RequestToDeletePolicyList policiesToDelete)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _pap.TryDeleteDelegationPolicies(policiesToDelete);
            int countPolicies = DelegationHelper.GetPolicyCount(deletionResults);
            int policiesToDeleteCount = policiesToDelete.Count;

            if (countPolicies == policiesToDeleteCount)
            {
                return StatusCode(200, deletionResults);
            }

            string policiesToDeleteSerialized = JsonSerializer.Serialize(policiesToDelete);
            if (countPolicies > 0)
            {
                string deletionResultsSerialized = JsonSerializer.Serialize(deletionResults);
                _logger.LogInformation("Partial deletion completed deleted {countPolicies} of {policiesToDeleteCount}.\n{deletionResultsSerialized}", countPolicies, policiesToDeleteCount, deletionResultsSerialized);
                return StatusCode(206, deletionResults);
            }

            _logger.LogInformation("Deletion could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{policiesToDeleteSerialized}", policiesToDeleteSerialized);
            return StatusCode(400, $"Unable to complete deletion");
        }

        /// <summary>
        /// Endpoint for retrieving delegated Maskinporten resources offered by the reportee party to others
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/{party}/delegations/maskinportenschema/offered")]
        public async Task<ActionResult<List<DelegationExternal>>> GetOfferedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetOfferedMaskinportenSchemaDelegations(partyMatch);
                return _mapper.Map<List<DelegationExternal>>(delegations);
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
        /// Endpoint for retrieving received Maskinporten resource delegation to the reportee party from others
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize]
        [Route("accessmanagement/api/v1/{party}/delegations/maskinportenschema/received")]
        public async Task<ActionResult<List<DelegationExternal>>> GetReceivedMaskinportenSchemaDelegations([FromRoute] string party)
        {
            try
            {
                AttributeMatch partyMatch = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                List<Delegation> delegations = await _delegation.GetReceivedMaskinportenSchemaDelegations(partyMatch);
                return _mapper.Map<List<DelegationExternal>>(delegations);
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
        /// Endpoint for retrieving delegated resources between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("accessmanagement/api/v1/admin/delegations/maskinportenschema")]
        [Authorize]
        public async Task<ActionResult<List<MPDelegationExternal>>> GetMaskinportenSchemaDelegations([FromQuery] string? supplierOrg, string? consumerOrg, string scope)
        {
            if (!string.IsNullOrEmpty(supplierOrg) && !IdentifierUtil.IsValidOrganizationNumber(supplierOrg))
            {
                return BadRequest("Supplierorg is not an valid organization number");
            }

            if (!string.IsNullOrEmpty(consumerOrg) && !IdentifierUtil.IsValidOrganizationNumber(consumerOrg))
            {
                return BadRequest("Consumerorg is not an valid organization number");
            }

            if (string.IsNullOrEmpty(scope))
            {
                return BadRequest("Either the parameter scope has no value or the provided value is invalid");
            }

            try
            {
                List<Delegation> delegations = await _delegation.GetMaskinportenSchemaDelegations(supplierOrg, consumerOrg, scope);
                List<MPDelegationExternal> delegationsExternal = _mapper.Map<List<MPDelegationExternal>>(delegations);

                return delegationsExternal;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllDelegationsForAdmin failed to fetch delegations");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Endpoint for delegating maskinporten scheme resources between two parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize]
        [Route("accessmanagement/api/v1/{party}/delegations/maskinportenschema/")]
        public async Task<ActionResult<DelegationOutputExternal>> MaskinportenScopeDelegation([FromRoute] string party, [FromBody] DelegationInputExternal delegation)
        {
            int authenticatedUserId = AuthenticationHelper.GetUserId(HttpContext);
            int authenticationLevel = AuthenticationHelper.GetUserAuthenticationLevel(HttpContext);

            try
            {
                AttributeMatch reportee = IdentifierUtil.GetIdentifierAsAttributeMatch(party, HttpContext);
                DelegationInput internalDelegation = _mapper.Map<DelegationInput>(delegation);
                internalDelegation.From = reportee.SingleToList();
                DelegationOutput response = await _delegation.MaskinportenDelegation(authenticatedUserId, authenticationLevel, internalDelegation);

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
    }
}
