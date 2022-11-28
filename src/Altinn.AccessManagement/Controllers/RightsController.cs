using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
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
        /// <response code="200" cref="List{XacmlRule}">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        ////[Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/{party}/rights/received")]
        public async Task<List<Right>> RightsQuery([FromBody] RightsQuery rightsQuery) // TODO replace body with path and query
        {
            List<Right> result = new();
            try
            {
                result = await _pip.GetRights(rightsQuery);
            }
            catch (ValidationException valEx)
            {
                BadRequest(valEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(500, ex, "Internal exception occured during GetRights");
                StatusCode(500, "Internal Server Error");
            }

            return result;
        }
    }
}
