using System.Text.Json;
using Altinn.AuthorizationAdmin.Core.Constants;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Core.Services.Interfaces;
using Altinn.AuthorizationAdmin.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AuthorizationAdmin.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [ApiController]
    public class DelegationsController : ControllerBase
    {
        private readonly IPolicyAdministrationPoint _pap;
        private readonly IPolicyInformationPoint _pip;
        private readonly ILogger _logger;
        private readonly IDelegationsService _delegation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="policyAdministrationPoint">The policy administration point</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="logger">the logger.</param>
        /// <param name="delegationsService">Handler for the delegation service</param>
        public DelegationsController(IPolicyAdministrationPoint policyAdministrationPoint, IPolicyInformationPoint policyInformationPoint, ILogger<DelegationsController> logger, IDelegationsService delegationsService)
        {
            _pap = policyAdministrationPoint;
            _pip = policyInformationPoint;
            _logger = logger;
            _delegation = delegationsService;
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
        [Route("authorization/api/v1/[controller]/AddRules")]
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
        ////[Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("authorization/api/v1/[controller]/GetRules")]
        public async Task<ActionResult<List<Rule>>> GetRules([FromBody] RuleQuery ruleQuery, [FromQuery] bool onlyDirectDelegations = false)
        {
            List<int> coveredByPartyIds = new List<int>();
            List<int> coveredByUserIds = new List<int>();
            List<int> offeredByPartyIds = new List<int>();
            List<string> appIds = new List<string>();

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
                string org = resource.FirstOrDefault(match => match.Id == XacmlRequestAttribute.OrgAttribute)?.Value;
                string app = resource.FirstOrDefault(match => match.Id == XacmlRequestAttribute.AppAttribute)?.Value;
                if (!string.IsNullOrEmpty(org) && !string.IsNullOrEmpty(app))
                {
                    appIds.Add($"{org}/{app}");
                }
            }

            if (DelegationHelper.TryGetCoveredByPartyIdFromMatch(ruleQuery.CoveredBy, out int partyId))
            {
                coveredByPartyIds.Add(partyId);
            }
            else if (DelegationHelper.TryGetCoveredByUserIdFromMatch(ruleQuery.CoveredBy, out int userId))
            {
                coveredByUserIds.Add(userId);
            }

            if (ruleQuery.OfferedByPartyId != 0)
            {
                offeredByPartyIds.Add(ruleQuery.OfferedByPartyId);
            }

            if (offeredByPartyIds.Count == 0)
            {
                _logger.LogInformation($"Unable to get the rules: Missing offeredbyPartyId value.");
                return StatusCode(400, $"Unable to get the rules: Missing offeredbyPartyId value.");
            }

            if (offeredByPartyIds.Count == 0 && coveredByPartyIds.Count == 0 && coveredByUserIds.Count == 0)
            {
                _logger.LogInformation($"Unable to get the rules: Missing offeredby and coveredby values.");
                return StatusCode(400, $"Unable to get the rules: Missing offeredby and coveredby values.");
            }

            List<Rule> rulesList = await _pip.GetRulesAsync(appIds, offeredByPartyIds, coveredByPartyIds, coveredByUserIds);
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
        [Route("authorization/api/v1/[controller]/DeleteRules")]
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
        [Route("authorization/api/v1/[controller]/DeletePolicy")]
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
        /// Endpoint for retrieving delegated resources between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("authorization/api/v1/[controller]/GetAllOfferedDelegations")]
        public async Task<ActionResult<List<OfferedDelegations>>> GetAllOfferedDelegations([FromQuery] int offeredbyPartyId, string resourceType)
        {
            if (offeredbyPartyId == 0)
            {
                return BadRequest("Missing query parameter offeredbypartyid");
            }

            if (!Enum.TryParse(resourceType, out ResourceType resource))
            {
                return BadRequest("Missing query parameter resourcetype or invalid value for resourcetype");
            }

            try
            {
                List<OfferedDelegations> delegations = await _delegation.GetAllOfferedDelegations(offeredbyPartyId, resource);
                if (delegations == null || delegations.Count == 0)
                {
                    return Ok("No delegations found");
                }

                return delegations;
            }
            catch (Exception ex) 
            {
                string errorMessage = ex.Message;
                _logger.LogError("GetAllOfferedDelegations failed to fetch delegations, See the error message for more details {errorMessage}", errorMessage);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Test method. Should be deleted?
        /// </summary>
        /// <returns>test string</returns>
        [HttpGet]
        [Route("authorization/api/v1/[controller]")]
        public string Get()
        {
            return "Hello world!";
        }
    }
}
