using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [ApiController]
    public class DelegationsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPolicyInformationPoint _pip;
        private readonly IPolicyAdministrationPoint _pap;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="policyAdministrationPoint">The policy administration point</param>
        public DelegationsController(
            ILogger<DelegationsController> logger,
            IPolicyInformationPoint policyInformationPoint,
            IPolicyAdministrationPoint policyAdministrationPoint)
        {
            _logger = logger;
            _pap = policyAdministrationPoint;
            _pip = policyInformationPoint;
        }

        /// <summary>
        /// Endpoint for adding one or more rules for the given app/offeredby/coveredby. This updates or creates a new delegated policy of type "DirectlyDelegated". DelegatedByUserId is included to store history information in 3.0.
        /// </summary>
        /// <param name="rules">All rules to be delegated</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <response code="201" cref="List{PolicyRule}">Created</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/addrules")]
        public async Task<ActionResult> Post([FromBody] List<Rule> rules, CancellationToken cancellationToken)
        {
            if (rules == null || rules.Count < 1)
            {
                return BadRequest("Missing rules in body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            List<Rule> delegationResults = await _pap.TryWriteDelegationPolicyRules(rules, cancellationToken);

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
        public async Task<ActionResult<List<Rule>>> GetRules([FromBody] RuleQuery ruleQuery, CancellationToken cancellationToken, [FromQuery] bool onlyDirectDelegations = false)
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
                if (DelegationHelper.TryGetResourceFromAttributeMatch(resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out _, out _, out _, out _))
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

            if (coveredByPartyIds.Count == 0 && coveredByUserIds.Count == 0)
            {
                return StatusCode(400, $"Unable to get the rules: Missing offeredby and coveredby values.");
            }

            List<Rule> rulesList = await _pip.GetRulesAsync(resourceIds, offeredByPartyIds, coveredByPartyIds, coveredByUserIds, cancellationToken);
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
        public async Task<ActionResult> DeleteRule([FromBody] RequestToDeleteRuleList rulesToDelete, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _pap.TryDeleteDelegationPolicyRules(rulesToDelete, cancellationToken);
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
        public async Task<ActionResult> DeletePolicy([FromBody] RequestToDeletePolicyList policiesToDelete, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _pap.TryDeleteDelegationPolicies(policiesToDelete, cancellationToken);
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
    }
}
