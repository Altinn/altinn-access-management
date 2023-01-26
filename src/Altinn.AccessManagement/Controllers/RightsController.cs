using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IPolicyInformationPoint _pip;

        /// <summary>
        /// Initializes a new instance of the <see cref="RightsController"/> class.
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <param name="mapper">handler for mapping between internal and external models</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        public RightsController(ILogger<RightsController> logger, IMapper mapper, IPolicyInformationPoint policyInformationPoint)
        {
            _logger = logger;
            _mapper = mapper;
            _pip = policyInformationPoint;
        }

        /// <summary>
        /// Endpoint for performing a query of rights between two parties for a specific resource
        /// </summary>
        /// <param name="rightsQuery">Query model for rights lookup</param>
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user have access to</param>
        /// <response code="200" cref="List{RightExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/internal/rights")]
        public async Task<ActionResult<List<RightExternal>>> RightsQuery([FromBody] RightsQueryExternal rightsQuery, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                RightsQuery rightsQueryInternal = _mapper.Map<RightsQuery>(rightsQuery);
                List<Right> rightsInternal = await _pip.GetRights(rightsQueryInternal, returnAllPolicyRights);
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
        /// <param name="returnAllPolicyRights">Whether the response should return all possible rights for the resource, not just the rights the user is allowed to delegate</param>
        /// <response code="200" cref="List{DelegationRightExternal}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/internal/delegablerights")]
        public async Task<ActionResult<List<RightExternal>>> DelegableRightsQuery([FromBody] RightsQueryExternal rightsQuery, [FromQuery] bool returnAllPolicyRights = false)
        {
            try
            {
                RightsQuery rightsQueryInternal = _mapper.Map<RightsQuery>(rightsQuery);
                List<Right> rightsInternal = await _pip.GetRights(rightsQueryInternal, returnAllPolicyRights, getDelegableRights: true);
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
    }
}
