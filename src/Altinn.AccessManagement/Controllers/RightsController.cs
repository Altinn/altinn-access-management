using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations regarding rights retrieval
    /// </summary>
    [ApiController]
    public class RightsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPolicyInformationPoint _pip;

        /// <summary>
        /// Initializes a new instance of the <see cref="RightsController"/> class.
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        public RightsController(ILogger<RightsController> logger, IPolicyInformationPoint policyInformationPoint)
        {
            _pip = policyInformationPoint;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint for performing a query of rights between two parties for a specific resource
        /// </summary>
        /// <param name="rightsQuery">Query model for rights lookup</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user have access to</param>
        /// <response code="200" cref="List{Right}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/internal/rights")]
        public async Task<ActionResult<List<Right>>> RightsQuery([FromBody] RightsQuery rightsQuery, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                return await _pip.GetRights(rightsQuery, returnAllPolicyRights);
            }
            catch (ValidationException valEx)
            {
                return BadRequest(valEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during RightsQuery");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Endpoint for performing a query of rights a user can delegate to others a specified reportee and resource.
        /// IMPORTANT: The delegable rights lookup does itself not check that the user has access to the necessary RolesAdministration/MainAdmin or MaskinportenSchema delegation system resources needed to be allowed to perform delegation.
        /// </summary>
        /// <param name="rightsQuery">Query model for rights lookup</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user is allowed to delegate</param>
        /// <response code="200" cref="List{Right}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/internal/delegablerights")]
        public async Task<ActionResult<List<Right>>> DelegableRightsQuery([FromBody] RightsQuery rightsQuery, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                return await _pip.GetRights(rightsQuery, returnAllPolicyRights, getDelegableRights: true);
            }
            catch (ValidationException valEx)
            {
                return BadRequest(valEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occurred during DelegableRightsQuery");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
